using CoreEngine.Cards;
using CoreEngine.Game;
using CoreEngine.Helpers;

namespace CoreEngine.GameActions
{
    /// <summary>
    /// Claim Route action.
    ///
    /// Covers all route variants:
    ///   • Plain colored and gray (Wild) routes
    ///   • Ferry routes (mandatory Locomotive minimum)
    ///   • Tunnel routes (reveal-3 extra-card mechanic, optional back-out)
    ///   • Double-route enforcement (player cannot claim both siblings;
    ///     in 2-3 player games the second route closes when the first is claimed)
    /// </summary>
    public class ClaimRouteGA : GameAction
    {
        public static readonly ClaimRouteGA Instance = new ClaimRouteGA();

        private ClaimRouteGA()
        {
            Name = "Claim Route";
            Description = "Play matching train cards to claim a route and score points.";
        }

        public override bool CanExecute(Player currentPlayer) =>
            GameManager.Instance.GetClaimableRoutes(currentPlayer).Count > 0;

        public override void Execute(Player currentPlayer)
        {
            var gm = GameManager.Instance;
            var claimable = gm.GetClaimableRoutes(currentPlayer);

            // ── Step 1: choose a route ────────────────────────────────────────────
            var routeChoices = claimable
                .Select(r => new PlayerChoice(r, RouteLabel(r)))
                .ToList();

            int routeIdx = gm.GetChoiceFromCurrentPlayer(routeChoices);
            Route chosen = claimable[routeIdx];

            // ── Step 2: choose a payment combo ────────────────────────────────────
            var spends = PaymentPlanner.EnumerateSpends(
                currentPlayer.TrainsHand,
                chosen.Length,
                chosen.Color,
                chosen.LocomotivesNeeded);

            SpendOption spend;
            if (spends.Count == 1)
            {
                spend = spends[0];  // only one way to pay — no choice needed
            }
            else
            {
                var spendChoices = spends
                    .Select(s => new PlayerChoice(s, s.Description))
                    .ToList();
                int spendIdx = gm.GetChoiceFromCurrentPlayer(spendChoices);
                spend = spends[spendIdx];
            }

            // ── Step 3: tunnel resolution (if applicable) ─────────────────────────
            if (chosen.IsTunnel)
            {
                bool claimed = TryClaimTunnel(currentPlayer, chosen, spend, gm);
                if (!claimed)
                {
                    Console.WriteLine($"  {currentPlayer} backed out of the tunnel. Turn forfeited.");
                    return;  // turn consumed, no cards spent
                }
            }
            else
            {
                // Normal claim
                currentPlayer.CommitSpend(spend);
                chosen.Claim(currentPlayer);
                Console.WriteLine($"  {currentPlayer} claims {chosen.Origin.Name}–{chosen.Destination.Name} " +
                                  $"({chosen.Length} cars) for {Route.RoutePoints(chosen.Length)} pts.");
            }
        }

        // ── Tunnel mechanic ───────────────────────────────────────────────────────
        /// <summary>
        /// Returns true if the tunnel was successfully claimed (cards committed + route claimed).
        /// Returns false if the player backed out or could not afford the extra cards.
        /// </summary>
        private static bool TryClaimTunnel(Player player, Route route, SpendOption baseSpend, GameManager gm)
        {
            // Reveal top 3 cards from the draw deck
            var revealed = new List<TrainCard>();
            for (int i = 0; i < 3; i++)
            {
                var card = gm.DrawFromMainDeck();
                if (card != null) revealed.Add(card);
            }

            Console.WriteLine($"  Tunnel reveal: [{string.Join(", ", revealed.Select(c => c.Color))}]");

            // Count extra cards needed: revealed cards matching the chosen color or Locomotive
            TrainColor matchColor = baseSpend.Color == TrainColor.Locomotive
                ? TrainColor.Locomotive  // all-loco payment: only locos trigger extras
                : baseSpend.Color;

            int extra = revealed.Count(c => c.IsLocomotive || c.Color == matchColor);
            Console.WriteLine($"  Extra cards needed: {extra}");

            // Discard the 3 revealed cards (always, regardless of outcome)
            foreach (var c in revealed)
                gm.UsedCardsDeck.AddOnBottom(c);

            if (extra == 0)
            {
                // No extra cost — commit base spend and claim
                player.CommitSpend(baseSpend);
                route.Claim(player);
                Console.WriteLine($"  {player} claims tunnel {route.Origin.Name}–{route.Destination.Name} " +
                                  $"for {Route.RoutePoints(route.Length)} pts.");
                return true;
            }

            // Compute remaining hand after base spend (for checking affordability)
            var remainingHand = new List<TrainCard>(player.TrainsHand);
            RemoveSpendFromHand(remainingHand, baseSpend);

            // Extra spend must be the same color (or loco for any slot)
            var extraSpends = PaymentPlanner.EnumerateSpends(
                remainingHand,
                extra,
                matchColor == TrainColor.Locomotive ? TrainColor.Locomotive : matchColor);

            if (extraSpends.Count == 0)
            {
                // Cannot afford — auto back-out (consistent for replay)
                Console.WriteLine($"  {player} cannot afford {extra} extra card(s). Back out.");
                return false;
            }

            // Offer extra-spend options + back-out
            const int BackOutIndex = 0;
            var extraChoices = new List<PlayerChoice>
            {
                new(null, $"Back out (forfeit turn) — cannot afford {extra} extra card(s)? No — back out anyway.")
            };
            foreach (var es in extraSpends)
                extraChoices.Add(new PlayerChoice(es, $"Pay extra: {es.Description}"));

            int choice = gm.GetChoiceFromCurrentPlayer(extraChoices);

            if (choice == BackOutIndex)
                return false;  // player chose to back out

            var extraSpend = extraSpends[choice - 1];  // -1 because index 0 = back-out

            // Commit both payments and claim
            player.CommitSpend(baseSpend);
            player.CommitSpend(extraSpend);
            route.Claim(player);
            Console.WriteLine($"  {player} claims tunnel {route.Origin.Name}–{route.Destination.Name} " +
                              $"(+{extra} extra) for {Route.RoutePoints(route.Length)} pts.");
            return true;
        }

        /// <summary>Removes the cards described by <paramref name="spend"/> from a copy of the hand (non-destructive simulation).</summary>
        private static void RemoveSpendFromHand(List<TrainCard> hand, SpendOption spend)
        {
            int colorRemaining = spend.ColorCards;
            int locoRemaining  = spend.Locomotives;

            for (int i = hand.Count - 1; i >= 0 && (colorRemaining > 0 || locoRemaining > 0); i--)
            {
                var card = hand[i];
                if (colorRemaining > 0 && card.Color == spend.Color && spend.Color != TrainColor.Locomotive)
                {
                    hand.RemoveAt(i);
                    colorRemaining--;
                }
                else if (locoRemaining > 0 && card.IsLocomotive)
                {
                    hand.RemoveAt(i);
                    locoRemaining--;
                }
            }
        }

        private static string RouteLabel(Route r)
        {
            string color  = r.Color == TrainColor.Wild ? "Gray(any)" : r.Color.ToString();
            string extras = "";
            if (r.IsTunnel)          extras += " [TUNNEL]";
            if (r.LocomotivesNeeded > 0) extras += $" [FERRY: {r.LocomotivesNeeded} loco]";
            if (r.Sibling != null)   extras += " [double]";
            return $"{r.Origin.Name} → {r.Destination.Name}  ({r.Length} cars, {color}{extras})  +{Route.RoutePoints(r.Length)} pts";
        }
    }
}
