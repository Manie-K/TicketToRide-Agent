using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreEngine.Cards;
using CoreEngine.GameActions;
using CoreEngine.Helpers;

namespace CoreEngine.Game
{
    public enum GameMode
    {
        Live,
        Record,
        Replay
    }

    public class GameManager
    {
        public static GameManager Instance { get; private set; }

        public Player CurrentPlayer { get; private set; }
        public CardDeck<TicketCard> TicketsDeck { get; private set; }
        public CardDeck<TrainCard> TrainsDeck { get; private set; }
        public CardDeck<TrainCard> UsedCardsDeck { get; private set; }

        public List<City> Cities { get; private set; }
        public List<Player> Players { get; private set; }

        public GameMode GameMode { get; init; }

        private readonly Recorder? recorder;

        private readonly GameAction[] allGameActions = 
        {
            BuildTrainStationGA.Instance,
            ClaimRouteGA.Instance,
            //more actions here

        };

        

        public GameManager(List<Player> agents, GameMode gameMode = GameMode.Live)
        {
            GameMode = gameMode;

            // Decide where to save etc.
            if (GameMode != GameMode.Live) recorder = new Recorder(Path.GetTempFileName(), GameMode);

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
            if (recorder?.Mode == RecorderMode.Record)
            {
                // Record initial game state, decks, player sequence etc.
            }

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
            if (recorder?.Mode == RecorderMode.Replay)
            {
                int index = recorder.GetNextPlayerChoice();
                return index;
            }

            var currentPlayer = CurrentPlayer;
            int index = currentPlayer.InputFunc?.Invoke(choices) ?? -1;

            if (recorder?.Mode == RecorderMode.Record)
            {
                recorder.RecordPlayerChoice(index); //Maybe currentPlayer isnt needed?
            }

            return index;
        }
    }
}
