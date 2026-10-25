using CoreEngine.Cards;
using CoreEngine.Game;
using CoreEngine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreEngine.GameActions
{
    /// <summary>
    /// Build Train Station action.
    ///
    /// A player may build at most 3 stations during the game.
    /// Cost: 1 same-color card for the 1st station, 2 for the 2nd, 3 for the 3rd.
    /// Locomotives count as wild. A station may be built in any unoccupied city.
    ///
    /// Fixes vs. original stub:
    /// - CanExecute checks both station availability AND that a valid payment exists.
    /// - Execute uses event-driven architecture (SetPendingChoices) instead of blocking calls.
    /// - StationsRemaining is decremented on the player after building.
    /// </summary>
    public class BuildTrainStationGA : GameAction
    {
        public static readonly BuildTrainStationGA Instance = new BuildTrainStationGA();

        private BuildTrainStationGA()
        {
            Name = "Build Train Station";
            Description = "Build a station in an unoccupied city to gain access to one opponent route.";
        }

        public override bool CanExecute(Player currentPlayer)
        {
            if (currentPlayer.StationsRemaining <= 0) return false;

            int requiredCards = currentPlayer.StationsBuilt + 1;  // 1st→1, 2nd→2, 3rd→3
            return currentPlayer.HasNCardOfSameColor(requiredCards);
        }

        public override void Execute(Player currentPlayer)
        {
            var gm = GameManager.Instance;
            int requiredCards = currentPlayer.StationsBuilt + 1;

            // ── Step 1: choose an unoccupied city ─────────────────────────────────
            var availableCities = gm.Cities
                .Where(c => !c.HasStation)
                .ToList();

            if (availableCities.Count == 0)
            {
                // Edge case fallback
                gm.EndTurn();
                return;
            }

            var cityChoices = new List<PlayerChoice>();
            var cityCallbacks = new List<Action>();

            foreach (var city in availableCities)
            {
                cityChoices.Add(new PlayerChoice(city, $"Build station in {city.Name}"));

                cityCallbacks.Add(() =>
                {
                    // Callback executed when THIS city is selected by the RL Agent/Human
                    HandlePaymentSelection(gm, currentPlayer, city, requiredCards);
                });
            }

            // Suspend the game and wait for the city selection
            gm.SetPendingChoices(cityChoices, idx => cityCallbacks[idx]());
        }

        private void HandlePaymentSelection(GameManager gm, Player currentPlayer, City selectedCity, int requiredCards)
        {
            // ── Step 2: choose a payment combo ────────────────────────────────────
            // Station cost accepts any one color (or locomotives as wild).
            var spends = PaymentPlanner.EnumerateSpends(
                currentPlayer.TrainsHand,
                routeLength: requiredCards,
                routeColor: TrainColor.Wild,
                minLocomotives: 0);

            if (spends.Count == 0)
                throw new InvalidOperationException("Valid payment existed during CanExecute but is now missing.");

            if (spends.Count == 1)
            {
                // Only one way to pay - auto-resolve and end turn immediately
                ApplyPaymentAndBuild(gm, currentPlayer, selectedCity, spends[0]);
                gm.EndTurn();
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
                        // Callback executed when THIS payment option is selected
                        ApplyPaymentAndBuild(gm, currentPlayer, selectedCity, spend);

                        // Turn ends ONLY after payment is resolved
                        gm.EndTurn();
                    });
                }

                // Suspend the game AGAIN for the payment submenu
                gm.SetPendingChoices(spendChoices, idx => spendCallbacks[idx]());
            }
        }

        private void ApplyPaymentAndBuild(GameManager gm, Player currentPlayer, City selectedCity, SpendOption spend)
        {
            // ── Commit ────────────────────────────────────────────────────────────
            currentPlayer.CommitSpend(spend);
            selectedCity.BuildStation(currentPlayer);
            currentPlayer.StationsRemaining--;

            Console.WriteLine($"  {currentPlayer} builds station in {selectedCity.Name} " +
                              $"(cost: {spend.Description}). Stations remaining: {currentPlayer.StationsRemaining}");
        }
    }
}