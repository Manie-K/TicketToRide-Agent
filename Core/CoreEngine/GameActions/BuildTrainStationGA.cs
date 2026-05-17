using CoreEngine.Cards;
using CoreEngine.Game;

namespace CoreEngine.GameActions
{
    public class BuildTrainStationGA : GameAction
    {
        public static readonly BuildTrainStationGA Instance = new BuildTrainStationGA();

        public override bool CanExecute(Player currentPlayer) { return currentPlayer.HasNCardOfSameColor(currentPlayer.StationsCount + 1); } 

        public override void Execute(Player currentPlayer)
        {

            List<PlayerChoice> availableCities = new();

            GameManager.Instance.Cities.ForEach(city =>
            {
                if (!city.HasStation)
                {
                    availableCities.Add(new PlayerChoice(availableCities.Count, city.Name ));
                }
            });

            int selectedCityIndex = GameManager.Instance.GetChoiceFromCurrentPlayer(availableCities);
            City selectedCity = GameManager.Instance.Cities[selectedCityIndex];

            //numberOfRequiredCardOfTheSameColor 
            int requiredCardsCount = currentPlayer.StationsCount + 1;

            List<TrainCard> playerCards = currentPlayer.TrainsHand;
            List<TrainColor> availableColors = Utils.FindAvailableColorsByCount(playerCards, requiredCardsCount);

            if (availableColors.Count == 0)
            {
                throw new Exception("We fucked up - player should have available cards otherwise he could not have picked this action.");
            }

            List<PlayerChoice> availableColorsToDiscard = new List<PlayerChoice>();
            int selectedColorIndex = GameManager.Instance.GetChoiceFromCurrentPlayer(availableColorsToDiscard);
            TrainColor colorToDiscard = availableColors[selectedColorIndex];

            currentPlayer.SpendNCardsOfColor(requiredCardsCount, colorToDiscard);

            selectedCity.BuildStation(currentPlayer);

        }
    }
}
