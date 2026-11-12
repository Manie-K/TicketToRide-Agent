using CoreEngine.Cards;
using CoreEngine.Game;
using CoreEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreEngine.GameActions
{
    /// <summary>
    /// Claim Route action.
    ///
    /// Covers all route variants:
    /// - Plain colored and gray (Wild) routes
    /// - Ferry routes (mandatory Locomotive minimum)
    /// - Tunnel routes (reveal-3 extra-card mechanic, optional back-out)
    /// - Double-route enforcement (player cannot claim both siblings;
    ///     in 2-3 player games the second route closes when the first is claimed)
    ///     
    /// Architecture update: 
    /// - Converted to event-driven (State Machine) flow using SetPendingChoices.
    /// - Cascades up to 3 times for Tunnels: Route Selection -> Base Payment -> Extra Payment.
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

            if (claimable.Count == 0)
            {
                gm.EndTurn();
                return;
            }

            // ── Step 1: choose a route ────────────────────────────────────────────
            var routeChoices = new List<PlayerChoice>();
            var routeCallbacks = new List<Action>();

            foreach (var route in claimable)
            {
                routeChoices.Add(new PlayerChoice(route, RouteLabel(route)));

                routeCallbacks.Add(() =>
                {
                    HandleBasePaymentSelection(gm, currentPlayer, route);
                });
            }

            // Suspend game and wait for route selection
            gm.SetPendingChoices(routeChoices, idx => routeCallbacks[idx]());
        }

        private void HandleBasePaymentSelection(GameManager gm, Player player, Route chosenRoute)
        {
            // ── Step 2: choose a payment combo ────────────────────────────────────
            var spends = PaymentPlanner.EnumerateSpends(
                player.TrainsHand,
                chosenRoute.Length,
                chosenRoute.Color,
                chosenRoute.LocomotivesNeeded);

            if (spends.Count == 0)
                throw new InvalidOperationException("Route was claimable but no valid payment options found.");

            if (spends.Count == 1)
            {
                // Auto-resolve base payment if there's only one option
                ProcessPaymentAndCheckTunnel(gm, player, chosenRoute, spends[0]);
            }
            else
            {
                var spendChoices = new List<PlayerChoice>();
                var spendCallbacks = new List<Action>();

                foreach (var spend in spends)
                {
                    spendChoices.Add(new PlayerChoice(spend, spend.Description));

                    spendCallbacks.Add(() =>
                    {
                        ProcessPaymentAndCheckTunnel(gm, player, chosenRoute, spend);
                    });
                }

                // Suspend game and wait for base payment selection
                gm.SetPendingChoices(spendChoices, idx => spendCallbacks[idx]());
            }
        }

        private void ProcessPaymentAndCheckTunnel(GameManager gm, Player player, Route chosenRoute, SpendOption baseSpend)
        {
            // ── Step 3: tunnel resolution (if applicable) ─────────────────────────
            if (chosenRoute.IsTunnel)
            {
                HandleTunnelReveal(gm, player, chosenRoute, baseSpend);
            }
            else
            {
                // Normal claim (Not a tunnel)
                player.CommitSpend(baseSpend);
                chosenRoute.Claim(player);
                Console.WriteLine($"  {player} claims {chosenRoute.Origin.Name}–{chosenRoute.Destination.Name} " +
                                  $"({chosenRoute.Length} cars) for {Route.RoutePoints(chosenRoute.Length)} pts.");

                gm.EndTurn();
            }
        }

        // ── Tunnel mechanic (Async) ───────────────────────────────────────────────
        private void HandleTunnelReveal(GameManager gm, Player player, Route route, SpendOption baseSpend)
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
                gm.EndTurn();
                return;
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
                // Cannot afford — auto back-out (turn forfeited, no cards lost)
                Console.WriteLine($"  {player} cannot afford {extra} extra card(s). Back out.");
                gm.EndTurn();
                return;
            }

            // Step 4: Offer extra-spend options + back-out
            var extraChoices = new List<PlayerChoice>();
            var extraCallbacks = new List<Action>();

            // Option 0: Back out
            extraChoices.Add(new PlayerChoice(null, $"Back out (forfeit turn) — don't pay {extra} extra card(s)"));
            extraCallbacks.Add(() =>
            {
                Console.WriteLine($"  {player} backed out of the tunnel. Turn forfeited.");
                gm.EndTurn();
            });

            // Options 1+: Pay extra
            foreach (var extraSpend in extraSpends)
            {
                extraChoices.Add(new PlayerChoice(extraSpend, $"Pay extra: {extraSpend.Description}"));

                extraCallbacks.Add(() =>
                {
                    // Commit both payments and claim
                    player.CommitSpend(baseSpend);
                    player.CommitSpend(extraSpend);
                    route.Claim(player);
                    Console.WriteLine($"  {player} claims tunnel {route.Origin.Name}–{route.Destination.Name} " +
                                      $"(+{extra} extra) for {Route.RoutePoints(route.Length)} pts.");
                    gm.EndTurn();
                });
            }

            // Suspend game for the final tunnel payment decision
            gm.SetPendingChoices(extraChoices, idx => extraCallbacks[idx]());
        }

        /// <summary>Removes the cards described by <paramref name="spend"/> from a copy of the hand (non-destructive simulation).</summary>
        private static void RemoveSpendFromHand(List<TrainCard> hand, SpendOption spend)
        {
            int colorRemaining = spend.ColorCards;
            int locoRemaining = spend.Locomotives;

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
            string color = r.Color == TrainColor.Wild ? "Gray(any)" : r.Color.ToString();
            string extras = "";
            if (r.IsTunnel) extras += " [TUNNEL]";
            if (r.LocomotivesNeeded > 0) extras += $" [FERRY: {r.LocomotivesNeeded} loco]";
            if (r.Sibling != null) extras += " [double]";
            return $"{r.Origin.Name} → {r.Destination.Name}  ({r.Length} cars, {color}{extras})  +{Route.RoutePoints(r.Length)} pts";
        }
    }
}