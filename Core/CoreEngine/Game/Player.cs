using System.Collections;
using CoreEngine.Cards;

namespace CoreEngine.Game
{
    public delegate int PlayerChoiceDelegate(IEnumerable idk);
    
    public class Player
    { 
        public List<TicketCard> TicketsHand { get; private set; }
        public List<TrainCard> TrainsHand { get; private set; }
        public int StationsCount { get; private set; }
        public PlayerChoiceDelegate InputFunc { get; init; }


        public Player(PlayerChoiceDelegate inputFunc)
        {
            InputFunc = inputFunc;

            TicketsHand = new List<TicketCard>();
            TrainsHand = new List<TrainCard>();
        }


        public bool SpendNCardsOfColor(int n, TrainColor color)
        {
            int spend = 0;
            List<TrainCard> toBeDiscarded = new();
            foreach (var card in TrainsHand)
            {
                if (card.Color == color)
                {
                    spend++;
                    toBeDiscarded.Add(card);
                }

                if (spend >= n) break;

            }

            if (spend < n)
            {
                throw new Exception("Could not find required cards on player hand."); //Probably shouldn't be an exception, maybe True/False return?
                return false;
            }

            foreach(var card in toBeDiscarded)
            {
                TrainsHand.Remove(card);
                GameManager.Instance.UsedCardsDeck.AddOnBottom(card);
            }

            return true;
        }

        internal bool HasNCardOfSameColor(int n)
        {
            int locomotiveCounts = 0;
            int maxAvailable = 0;

            var availableColors = Utils.GetCardsColorMap(TrainsHand);

            if (availableColors.ContainsKey(TrainColor.Locomotive))
            {
                locomotiveCounts = availableColors[TrainColor.Locomotive];
            }

            foreach (var pair in availableColors)
            {
                if (pair.Key == TrainColor.Locomotive) continue;

                maxAvailable = Math.Max(maxAvailable, pair.Value);
            }

            return maxAvailable + locomotiveCounts >= n;
        }
    }
}
