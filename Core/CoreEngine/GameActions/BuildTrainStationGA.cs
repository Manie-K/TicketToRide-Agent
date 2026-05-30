using CoreEngine.Cards;
using CoreEngine.Game;
using CoreEngine.Helpers;

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
    ///   • CanExecute checks both station availability AND that a valid payment exists.
    ///   • Execute builds the color-choice list correctly (was empty in original).
    ///   • StationsRemaining is decremented on the player after building.
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
                .Select(c => new PlayerChoice(c, c.Name))
                .ToList();

            if (availableCities.Count == 0)
                throw new InvalidOperationException("No cities available for a station.");

            int cityIdx = gm.GetChoiceFromCurrentPlayer(availableCities);
            City selectedCity = (City)availableCities[cityIdx].value!;

            // ── Step 2: choose a payment combo ────────────────────────────────────
            // Station cost accepts any one color (or locomotives as wild).
            var spends = PaymentPlanner.EnumerateSpends(
                currentPlayer.TrainsHand,
                routeLength: requiredCards,
                routeColor: TrainColor.Wild,
                minLocomotives: 0);

            SpendOption spend;
            if (spends.Count == 1)
            {
                spend = spends[0];
            }
            else
            {
                var spendChoices = spends
                    .Select(s => new PlayerChoice(s, s.Description))
                    .ToList();
                int spendIdx = gm.GetChoiceFromCurrentPlayer(spendChoices);
                spend = spends[spendIdx];
            }

            // ── Commit ────────────────────────────────────────────────────────────
            currentPlayer.CommitSpend(spend);
            selectedCity.BuildStation(currentPlayer);
            currentPlayer.StationsRemaining--;

            Console.WriteLine($"  {currentPlayer} builds station in {selectedCity.Name} " +
                              $"(cost: {spend.Description}). Stations remaining: {currentPlayer.StationsRemaining}");
        }
    }
}
