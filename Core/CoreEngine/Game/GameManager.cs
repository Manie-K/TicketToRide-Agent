using CoreEngine.Cards;
using CoreEngine.GameActions;
using CoreEngine.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        public CardDeck<TrainCard> TrainsDeck { get; private set; } = null!;
        public CardDeck<TrainCard> UsedCardsDeck { get; private set; } = null!;

        // ── Face-up train card market (5 slots; null = empty slot) ───────────────
        public List<TrainCard?> FaceUpCards { get; private set; } = new(5) { null, null, null, null, null };

        // ── Board ────────────────────────────────────────────────────────────────
        public List<City> Cities { get; private set; } = new();
        public List<Route> Routes { get; private set; } = new();

        // ── Players ──────────────────────────────────────────────────────────────
        public List<Player> Players { get; private set; }
        public Player CurrentPlayer { get; private set; }

        // ── Mode / recorder ───────────────────────────────────────────────────────
        public GameMode GameMode { get; init; }
        private readonly Recorder? recorder;

        // ── State Machine properties for RL / External environment ────────────────
        public bool IsGameOver { get; private set; }
        public List<PlayerChoice> CurrentChoices { get; private set; } = new();
        private Action<int>? pendingChoiceResolver;

        private bool finalRound = false;
        private int finalRoundTurnsLeft = 0;

        // ── Actions ───────────────────────────────────────────────────────────────
        private readonly GameAction[] allGameActions =
        {
            BuildTrainStationGA.Instance,
            ClaimRouteGA.Instance,
            DrawCardsGA.Instance,
            DrawTicketsGA.Instance,
        };

        // ── Constructor ──────────────────────────────────────────────────────────
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
            var colours = Enum.GetValues<PlayerColor>();
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
            TicketsDeck = new CardDeck<TicketCard>(MapDefinition.BuildRegularTickets(), Rng);
            TrainsDeck = new CardDeck<TrainCard>(DecksInitialization.GetInitialTrainCards(), Rng);
            UsedCardsDeck = new CardDeck<TrainCard>(new List<TrainCard>(), Rng);
        }

        // ── Gym Environment Entry Point (Replaces Start() and Setup()) ───────────
        public void BeginGame()
        {
            IsGameOver = false;
            finalRound = false;
            finalRoundTurnsLeft = 0;

            if (recorder?.Mode == RecorderMode.Record)
            {
                // TODO: record initial game state for full replay support
            }

            // 1. Deal 4 train cards to each player
            foreach (var player in Players)
                for (int i = 0; i < 4; i++)
                    DrawFromMainDeckToHand(player);

            // 2. Open the face-up market
            RefillFaceUpCards();

            // 3. Deal 1 long-route ticket to each player (kept automatically)
            foreach (var player in Players)
            {
                var longTicket = LongTicketsDeck.DrawCard();
                if (longTicket != null)
                    player.TicketsHand.Add(longTicket);
            }

            // 4. Start the async/callback-based setup for regular tickets
            RunSetupTicketSelection(0);
        }

        // ── Gym Environment Step ──────────────────────────────────────────────────
        public void Step(int choiceIndex)
        {
            if (IsGameOver) return;

            if (choiceIndex < 0 || choiceIndex >= CurrentChoices.Count)
                throw new ArgumentOutOfRangeException(nameof(choiceIndex), "Invalid choice index.");

            if (recorder?.Mode == RecorderMode.Record)
                recorder.RecordPlayerChoice(choiceIndex);

            // Fetch the current callback, clear the state, and execute it
            var resolver = pendingChoiceResolver;
            pendingChoiceResolver = null;
            CurrentChoices.Clear();

            resolver?.Invoke(choiceIndex);
        }

        // ── Event-Driven Game Loop ────────────────────────────────────────────────
        private void RunSetupTicketSelection(int playerIndex)
        {
            // If all players have selected their initial tickets, start the real game
            if (playerIndex >= Players.Count)
            {
                CurrentPlayer = Players[0];
                StartTurn();
                return;
            }

            CurrentPlayer = Players[playerIndex];
            DealTicketsToPlayer(CurrentPlayer, count: 3, minKeep: 1, returnToBottom: false, onDone: () =>
            {
                RunSetupTicketSelection(playerIndex + 1); // Move to the next player
            });
        }

        private void StartTurn()
        {
            // Check for final round conclusion
            if (finalRound && finalRoundTurnsLeft <= 0)
            {
                IsGameOver = true;
                CalculateFinalScores();
                AnnounceWinner();
                return;
            }

            List<GameAction> available = GetAvailableActions();
            var choices = available.Select(a => new PlayerChoice(a, a.Name)).ToList();

            SetPendingChoices(choices, idx =>
            {
                available[idx].Execute(CurrentPlayer);

                // If the executed action was synchronous and didn't suspend the game 
                // for sub-choices (like PaymentOptions), end the turn immediately.
                if (pendingChoiceResolver == null)
                {
                    EndTurn();
                }
            });
        }

        public void EndTurn()
        {
            // Check end-game trigger (trains <= 2 at the END of a turn)
            if (!finalRound && CurrentPlayer.TrainsRemaining <= 2)
            {
                finalRound = true;
                finalRoundTurnsLeft = Players.Count;
                Console.WriteLine($"\n*** {CurrentPlayer} has {CurrentPlayer.TrainsRemaining} trains left — final round begins! ***\n");
            }

            if (finalRound)
                finalRoundTurnsLeft--;

            // Advance to next player
            int nextIdx = (Players.IndexOf(CurrentPlayer) + 1) % Players.Count;
            CurrentPlayer = Players[nextIdx];

            StartTurn();
        }

        // ── Choice Suspension Mechanism ───────────────────────────────────────────
        public void SetPendingChoices(List<PlayerChoice> choices, Action<int> onResolved)
        {
            CurrentChoices = choices;
            pendingChoiceResolver = onResolved;
        }

        private List<GameAction> GetAvailableActions()
        {
            var available = allGameActions
                .Where(a => a.CanExecute(CurrentPlayer))
                .ToList();

            if (available.Count == 0)
                throw new InvalidOperationException($"No actions available for {CurrentPlayer} — corrupted game state.");

            return available;
        }

        // ── Flattened Ticket drawing helper (RL-friendly combinatorics) ───────────
        public void DealTicketsToPlayer(Player player, int count = 3, int minKeep = 1, bool returnToBottom = true, Action? onDone = null)
        {
            var drawn = new List<TicketCard>();
            for (int i = 0; i < count; i++)
            {
                var t = TicketsDeck.DrawCard();
                if (t != null) drawn.Add(t);
            }

            // If deck is empty, immediately proceed
            if (drawn.Count == 0)
            {
                if (onDone != null) onDone();
                else EndTurn();
                return;
            }

            var choices = new List<PlayerChoice>();
            var choiceCallbacks = new List<Action>();
            int totalCombos = 1 << drawn.Count; // 2^count combinations

            // Generate all valid keep/discard combinations
            for (int mask = 1; mask < totalCombos; mask++)
            {
                var kept = new List<TicketCard>();
                var discarded = new List<TicketCard>();

                for (int i = 0; i < drawn.Count; i++)
                {
                    if ((mask & (1 << i)) != 0) kept.Add(drawn[i]);
                    else discarded.Add(drawn[i]);
                }

                if (kept.Count < minKeep) continue;

                string desc = "Keep tickets: " + string.Join(" & ", kept.Select(t => $"{t.Origin.Name}-{t.Destination.Name}"));
                choices.Add(new PlayerChoice(kept, desc));

                choiceCallbacks.Add(() =>
                {
                    foreach (var t in kept)
                        player.TicketsHand.Add(t);

                    if (returnToBottom)
                        foreach (var t in discarded)
                            TicketsDeck.AddOnBottom(t);

                    if (onDone != null) onDone();
                    else EndTurn();
                });
            }

            SetPendingChoices(choices, idx => choiceCallbacks[idx]());
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

        public void EnforceLocomotiveRule()
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