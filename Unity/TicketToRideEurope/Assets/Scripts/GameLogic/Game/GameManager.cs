using CoreEngine.Cards;
using CoreEngine.GameActions;
using CoreEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace CoreEngine.Game
{
    public enum GameMode { Live, Record, Replay }

    public class GameManager
    {
        // ── Singleton ─────────────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; } = null!;

        // ── Shared RNG (inject a seeded Random for deterministic replay) ──────────
        public Random Rng { get; }

        // ── Decks ────────────────────────────────────────────────────────────────
        public CardDeck<TicketCard> LongTicketsDeck { get; private set; } = null!;
        public CardDeck<TicketCard> TicketsDeck { get; private set; } = null!;
        public CardDeck<TrainCard>  TrainsDeck    { get; private set; } = null!;
        public CardDeck<TrainCard>  UsedCardsDeck { get; private set; } = null!;

        // ── Face-up train card market (5 slots; null = empty slot) ───────────────
        public List<TrainCard?> FaceUpCards { get; private set; } = new(5) { null, null, null, null, null };

        // ── Board ────────────────────────────────────────────────────────────────
        public List<City>  Cities { get; private set; } = new();
        public List<Route> Routes { get; private set; } = new();

        // ── Players ──────────────────────────────────────────────────────────────
        public List<Player> Players { get; private set; }
        public Player CurrentPlayer { get; private set; }

        // ── Mode / recorder ───────────────────────────────────────────────────────
        public GameMode GameMode { get; init; }
        private readonly Recorder? recorder;

        // ── Actions ───────────────────────────────────────────────────────────────
        private readonly GameAction[] allGameActions =
        {
            BuildTrainStationGA.Instance,
            ClaimRouteGA.Instance,
            DrawCardsGA.Instance,
            DrawTicketsGA.Instance,
        };

        // ── Constructor ──────────────────────────────────────────────────────────
        /// <param name="agents">Players in turn order.</param>
        /// <param name="gameMode">Live / Record / Replay.</param>
        /// <param name="rng">
        ///   Optional seeded <see cref="Random"/>. Pass a fixed seed for deterministic replays.
        /// </param>
        public GameManager(List<Player> agents, GameMode gameMode = GameMode.Live, Random? rng = null)
        {
            if (Instance != null)
                throw new InvalidOperationException("GameManager instance already exists.");
            Instance = this;

            GameMode = gameMode;
            Rng = rng ?? new Random();

            if (GameMode != GameMode.Live)
                recorder = new Recorder(Path.GetTempFileName(), GameMode);

            // Assign player colours in order
            var colours = (PlayerColor[])Enum.GetValues(typeof(PlayerColor));
            Players = agents;
            for (int i = 0; i < agents.Count; i++)
                agents[i] = new Player(agents[i].InputFunc, colours[i % colours.Length]);
            Players = agents;
            CurrentPlayer = Players[0];

            // Build map (must come before deck init so City objects are shared)
            MapDefinition.Build();
            Cities = MapDefinition.Cities.Values.ToList();
            Routes = MapDefinition.Routes.ToList();

            // Build decks — share the same Rng for reproducibility
            LongTicketsDeck = new CardDeck<TicketCard>(MapDefinition.BuildLongRouteTickets(), Rng);
            TicketsDeck     = new CardDeck<TicketCard>(MapDefinition.BuildRegularTickets(), Rng);
            TrainsDeck      = new CardDeck<TrainCard>(DecksInitialization.GetInitialTrainCards(), Rng);
            UsedCardsDeck   = new CardDeck<TrainCard>(new List<TrainCard>(), Rng);
        }

        // ── Game entry point ──────────────────────────────────────────────────────
        public void Start()
        {
            if (recorder?.Mode == RecorderMode.Record)
            {
                // TODO: record initial game state for full replay support
            }

            Setup();
            RunGameLoop();
            CalculateFinalScores();
            AnnounceWinner();
        }

        // ── Setup phase ───────────────────────────────────────────────────────────
        private void Setup()
        {
            // 1. Deal 4 train cards to each player
            foreach (var player in Players)
                for (int i = 0; i < 4; i++)
                    DrawFromMainDeckToHand(player);

            // 2. Open the face-up market
            RefillFaceUpCards();

            // 3. Deal 1 long-route ticket to each player (kept automatically; excess removed)
            foreach (var player in Players)
            {
                var longTicket = LongTicketsDeck.DrawCard();
                if (longTicket != null)
                    player.TicketsHand.Add(longTicket);
            }
            // Remaining long-route tickets are removed from play (don't put back)

            // 4. Deal 3 regular tickets to each player; player keeps >= 1
            //    (they already have 1 long route → total kept >= 2 as per rules)
            foreach (var player in Players)
            {
                var setupPlayer = CurrentPlayer; // save context
                CurrentPlayer = player;
                DealTicketsToPlayer(player, count: 3, minKeep: 1, returnToBottom: false);
                CurrentPlayer = setupPlayer;
            }
        }

        // ── Game loop ─────────────────────────────────────────────────────────────
        private void RunGameLoop()
        {
            bool finalRound = false;
            int finalRoundTurnsLeft = 0;

            while (true)
            {
                // Final-round countdown
                if (finalRound)
                {
                    if (finalRoundTurnsLeft <= 0) break;
                    finalRoundTurnsLeft--;
                }

                // Execute current player's turn
                ExecuteTurn();

                // Check end-game trigger (trains <= 2 at the END of a turn)
                if (!finalRound && CurrentPlayer.TrainsRemaining <= 2)
                {
                    finalRound = true;
                    // Every player (including the trigger player) gets one final turn.
                    // The trigger player just went, so remaining turns = Players.Count.
                    finalRoundTurnsLeft = Players.Count;
                    Console.WriteLine($"\n*** {CurrentPlayer} has {CurrentPlayer.TrainsRemaining} trains left — final round begins! ***\n");
                }

                // Advance to next player
                int nextIdx = (Players.IndexOf(CurrentPlayer) + 1) % Players.Count;
                CurrentPlayer = Players[nextIdx];
            }
        }

        private void ExecuteTurn()
        {
            List<GameAction> available = GetAvailableActions();

            var choices = available
                .Select(a => new PlayerChoice(a, a.Name))
                .ToList();

            int idx = GetChoiceFromCurrentPlayer(choices);
            available[idx].Execute(CurrentPlayer);
        }

        private List<GameAction> GetAvailableActions()
        {
            var available = allGameActions
                .Where(a => a.CanExecute(CurrentPlayer))
                .ToList();

            if (available.Count == 0)
                throw new InvalidOperationException(
                    $"No actions available for {CurrentPlayer} — corrupted game state.");

            return available;
        }

        // ── Player interaction ────────────────────────────────────────────────────
        public int GetChoiceFromCurrentPlayer(List<PlayerChoice> choices)
        {
            if (recorder?.Mode == RecorderMode.Replay)
                return recorder.GetNextPlayerChoice();

            int idx = CurrentPlayer.InputFunc?.Invoke(choices) ?? -1;

            if (recorder?.Mode == RecorderMode.Record)
                recorder.RecordPlayerChoice(idx);

            return idx;
        }

        // ── Ticket drawing helper (shared by DrawTicketsGA and Setup) ─────────────
        /// <summary>
        /// Draws up to <paramref name="count"/> tickets from <see cref="TicketsDeck"/>,
        /// presents keep/discard choices to the current player (enforcing
        /// <paramref name="minKeep"/>), then either adds kept tickets to the player's hand
        /// or returns discarded ones to the deck bottom (if <paramref name="returnToBottom"/>)
        /// or removes them from play (setup).
        /// </summary>
        public void DealTicketsToPlayer(Player player, int count = 3, int minKeep = 1, bool returnToBottom = true)
        {
            var drawn = new List<TicketCard>();
            for (int i = 0; i < count; i++)
            {
                var t = TicketsDeck.DrawCard();
                if (t != null) drawn.Add(t);
            }

            if (drawn.Count == 0) return;

            var kept = new List<TicketCard>();
            var discarded = new List<TicketCard>();

            for (int i = 0; i < drawn.Count; i++)
            {
                var ticket = drawn[i];
                int mustKeepRemaining = minKeep - kept.Count;
                int remainingTickets  = drawn.Count - i;

                // Hide discard if player MUST keep this ticket to meet the minimum
                bool mustKeepThis = mustKeepRemaining >= remainingTickets;

                List<PlayerChoice> opts;
                if (mustKeepThis)
                {
                    // Only one option: keep
                    opts = new List<PlayerChoice>
                    {
                        new(ticket, $"KEEP  {ticket.Origin.Name} → {ticket.Destination.Name} ({ticket.Points} pts)")
                    };
                }
                else
                {
                    opts = new List<PlayerChoice>
                    {
                        new(ticket, $"Keep    {ticket.Origin.Name} → {ticket.Destination.Name} ({ticket.Points} pts)"),
                        new(ticket, $"Discard {ticket.Origin.Name} → {ticket.Destination.Name} ({ticket.Points} pts)"),
                    };
                }

                int choice = GetChoiceFromCurrentPlayer(opts);
                bool keeping = mustKeepThis || choice == 0;

                if (keeping)
                    kept.Add(ticket);
                else
                    discarded.Add(ticket);
            }

            foreach (var t in kept)
                player.TicketsHand.Add(t);

            if (returnToBottom)
                foreach (var t in discarded)
                    TicketsDeck.AddOnBottom(t);
            // else: discarded tickets are simply removed from play (setup)
        }

        // ── Card draw helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Draws one card from the main deck (reshuffles discard pile if the deck is empty).
        /// Returns null only if no cards exist anywhere.
        /// </summary>
        public TrainCard? DrawFromMainDeck()
        {
            if (TrainsDeck.DeckSize == 0 && UsedCardsDeck.DeckSize > 0)
                TrainsDeck.ReshuffleFrom(UsedCardsDeck);

            return TrainsDeck.DrawCard();
        }

        private void DrawFromMainDeckToHand(Player player)
        {
            var card = DrawFromMainDeck();
            if (card != null) player.TrainsHand.Add(card);
        }

        /// <summary>
        /// Ensures all 5 face-up slots are filled, then enforces the
        /// "3-of-5 are locomotives → discard all 5 and redraw" rule until stable.
        /// </summary>
        public void RefillFaceUpCards()
        {
            for (int i = 0; i < 5; i++)
                if (FaceUpCards[i] == null)
                    FaceUpCards[i] = DrawFromMainDeck();

            EnforceLocomotiveRule();
        }

        private void EnforceLocomotiveRule()
        {
            while (FaceUpCards.Count(c => c?.IsLocomotive == true) >= 3)
            {
                foreach (var card in FaceUpCards)
                    if (card != null) UsedCardsDeck.AddOnBottom(card);

                for (int i = 0; i < 5; i++)
                    FaceUpCards[i] = DrawFromMainDeck();
            }
        }

        // ── Route query helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Returns routes the current player can legally claim on their next turn.
        /// Filters: unclaimed, player has trains, no sibling already claimed by player,
        /// 2-3 player double-route exclusion, and affordability.
        /// </summary>
        public List<Route> GetClaimableRoutes(Player player)
        {
            return Routes.Where(r =>
            {
                if (r.IsClaimed) return false;
                if (player.TrainsRemaining < r.Length) return false;

                // Player can't claim both routes in a double-route
                if (r.Sibling != null && r.Sibling.ClaimedBy == player) return false;

                // 2-3 player game: second route closes when first is taken
                if (r.Sibling != null && Players.Count <= 3 && r.Sibling.IsClaimed) return false;

                // Affordable?
                return PaymentPlanner.CanAfford(player.TrainsHand, r.Length, r.Color, r.LocomotivesNeeded);
            }).ToList();
        }

        // ── End-game scoring ──────────────────────────────────────────────────────
        private void CalculateFinalScores()
        {
            Console.WriteLine("\n═══ FINAL SCORING ═══\n");

            // Station bonus: +4 per unused station
            foreach (var player in Players)
            {
                int stationBonus = player.StationsRemaining * 4;
                player.Score += stationBonus;
                if (stationBonus > 0)
                    Console.WriteLine($"  {player}: +{stationBonus} pts (station bonus, {player.StationsRemaining} unused)");
            }

            // Destination tickets
            foreach (var player in Players)
            {
                int ticketNet = Scorer.ScoreTickets(player, Players, Routes);
                player.Score += ticketNet;
                Console.WriteLine($"  {player}: ticket net = {(ticketNet >= 0 ? "+" : "")}{ticketNet}");
            }

            // Longest continuous path bonus (10 pts)
            var longestPaths = Scorer.FindLongestPaths(Players);
            int maxPath = longestPaths.Values.Max();
            if (maxPath > 0)
            {
                var winners = longestPaths.Where(kv => kv.Value == maxPath).Select(kv => kv.Key).ToList();
                Console.WriteLine($"\n  Longest path: {maxPath} cars");
                foreach (var w in winners)
                {
                    w.Score += 10;
                    Console.WriteLine($"  ★ {w} gets European Express bonus (+10 pts) — path length {maxPath}");
                }
            }
        }

        private void AnnounceWinner()
        {
            Console.WriteLine("\n═══ FINAL SCORES ═══");
            foreach (var p in Players.OrderByDescending(p => p.Score))
                Console.WriteLine($"  {p}: {p.Score} pts  " +
                    $"(tickets completed: {Scorer.CompletedTicketCount(p)} / {p.TicketsHand.Count})");

            var winner = Scorer.DetermineWinner(Players, Routes);
            Console.WriteLine($"\n🏆 Winner: {winner}");
        }
    }
}
