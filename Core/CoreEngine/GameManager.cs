using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine
{
    public class GameManager
    {
        public static GameManager Instance { get; private set; }

        public Player CurrentPlayer { get; private set; }
        public CardDeck<TicketCard> TicketsDeck { get; private set; }
        public CardDeck<TrainCard> TrainsDeck { get; private set; }
        public CardDeck<TrainCard> UsedCardsDeck { get; private set; }

        public List<City> Cities { get; private set; }
        public List<Player> Players { get; private set; }

        private readonly GameAction[] allGameActions = 
        {
            BuildTrainStationGA.Instance,
            ClaimRouteGA.Instance,
            //more actions here

        };

        public GameManager(List<Player> agents)
        {
            Players = agents;
            CurrentPlayer = agents[0];

            if (Instance != null)
            {
                throw new Exception("GameManager instance already exists.");
            }

            Instance = this;

            TicketsDeck = new CardDeck<TicketCard>(DecksInitialization.GetInitialTicketCards());
            TrainsDeck = new CardDeck<TrainCard>(DecksInitialization.GetInitialTrainCards());
            UsedCardsDeck = new CardDeck<TrainCard>(new List<TrainCard>());

            Cities = new List<City>();
        }

        public void Start()
        {
            bool quit = false;

            while (!quit)
            {
                List<GameAction> availableActions = GetAvailableActions();

                List<PlayerChoice> choices = new();

                foreach (GameAction action in availableActions)
                {
                    choices.Add(new PlayerChoice(action, ""));
                }

                int index = GetChoiceFromCurrentPlayer(choices);

                availableActions[index].Execute(CurrentPlayer);

                int nextPlayerIndex = (Players.IndexOf(CurrentPlayer) + 1) % Players.Count;
                CurrentPlayer = Players[nextPlayerIndex];
            }
        }

        private List<GameAction> GetAvailableActions()
        {
            List<GameAction> availableActions = new();

            foreach (var action in allGameActions)
            {
                if (action.CanExecute(CurrentPlayer))
                {
                    availableActions.Add(action);
                }
            }

            if (availableActions.Count == 0)
            {
                throw new Exception("No actions available - corrupted game state");
            }

            return availableActions;

        }

        public int GetChoiceFromCurrentPlayer(List<PlayerChoice> choices)
        {
            var currentPlayer = this.CurrentPlayer;
            int index = currentPlayer.InputFunc?.Invoke(choices) ?? -1;
            return index;
        }
    }
}
