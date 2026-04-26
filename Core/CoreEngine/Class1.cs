namespace CoreEngine
{

    public abstract class GameAction
    {
        public string Name;
        public string Description;

        public abstract bool CanExecute(); //CurrentPlayer ?

        public abstract void Execute();
    }


    public class BuildTrainStationGA : GameAction
    {
        public static readonly BuildTrainStationGA Instance = new BuildTrainStationGA();

        public override void Execute()
        {

            List<PlayerChoice> availableCities = new();

            GameManager.Instance.Cities.ForEach(city =>
            {
                if (!city.HasStation)
                {
                    availableCities.Add(new PlayerChoice { availableCities.Count, city.Name });
                }
            });

            int selectedCityIndex = GameManager.Instance.GetChoiceFromCurrentPlayer(availableCities);
            City selectedCity = GameManager.Instance.Cities[selectedCityIndex];

            //numberOfRequiredCardOfTheSameColor 
            int requiredCardsCount = Player.CurrentPlayer.StationsCount + 1;

            List<TrainCard> playerCards = Player.CurrentPlayer.TrainsHand;
            List<TrainColor> availableColors = Utils.FindAvailableColorsByCount(playerCards, requiredCardsCount);

            if (availableColors.Count == 0)
            {
                throw new Exception("We fucked up - player should have available cards otherwise he could not have picked this action.");
            }

            List<PlayerChoice> availableColorsToDiscard = new List<PlayerChoice>();
            int selectedColorIndex = GameManager.Instance.GetChoiceFromCurrentPlayer(availableColorsToDiscard);
            TrainColor colorToDiscard = availableColors[selectedColorIndex];

            Player.CurrentPlayer.SpendNCardsOfColor(requiredCardsCount, colorToDiscard);

            selectedCity.BuildStation(Player.CurrentPlayer);

        }
    }

    //public class DrawCardsGA : GameAction
    //{

    //}

    public class ClaimRouteGA : GameAction
    {
        public static readonly ClaimRouteGA Instance = new ClaimRouteGA();

        public override bool CanExecute()
        {
            throw new NotImplementedException();
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    //public class DrawTicketsGA : GameAction
    //{

    //}
}
