using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine
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
            this.InputFunc = inputFunc;
        }

        public Player()
        {
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

        internal bool HasNCardOfSameColor(int v)
        {
            return false;
        }
    }
}
