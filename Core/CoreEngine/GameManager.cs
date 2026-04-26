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

        public CardDeck<TicketCard> TicketsDeck { get; private set; }
        public CardDeck<TrainCard> TrainsDeck { get; private set; }
        public CardDeck<TrainCard> UsedCardsDeck { get; private set; }

        public List<City> Cities { get; private set; }


        private readonly List<GameAction> currentAvailableActions;
        private readonly GameAction[] allGameActions = new GameAction []
        {
            BuildTrainStationGA.Instance,
            ClaimRouteGA.Instance,
            //more actions here

        };


        public GameManager() 
        {
            if (Instance != null)
            {
                throw new Exception("GameManager instance already exists.");
            }

            Instance = this;

            currentAvailableActions = new();

            TicketsDeck = new CardDeck<TicketCard>(DecksInitialization.GetInitialTicketCards());
            TrainsDeck = new CardDeck<TrainCard>(DecksInitialization.GetInitialTrainCards());
            UsedCardsDeck = new CardDeck<TrainCard>(new List<TrainCard>());

            Cities = new List<City>();
        }


        public List<GameAction> GetAvailableActions()
        {
            List<GameAction> availableActions = new();

            foreach (var action in allGameActions)
            {
                if (action.CanExecute())
                {
                    availableActions.Add(action);
                }
            }

            if (availableActions.Count == 0)
            {
                throw new Exception("No actions available - corrupted game state");
            }

            currentAvailableActions.Clear();
            currentAvailableActions.AddRange(availableActions);

            return availableActions;

        }

        public void SelectAction(int index)
        {
            if (index < 0 || index >= currentAvailableActions.Count)
            {
                throw new ArgumentOutOfRangeException($"Index [{index}] was out of range ({0}; {currentAvailableActions.Count})");
            }   
            
            GameAction action = currentAvailableActions[index];
            action.Execute();

            currentAvailableActions.Clear();

            //Next Turn()
        }

        public int GetChoiceFromCurrentPlayer(List<PlayerChoice> choices)
        {
            int index = -1;

            //print to console and wait for input etc.



            return index;
        }
    }
}
