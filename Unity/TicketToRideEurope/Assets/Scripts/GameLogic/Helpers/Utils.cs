using CoreEngine.Cards;
using System.Collections.Generic;
using System.Linq;

namespace CoreEngine
{
    public static class Utils
    {
        public static Dictionary<TrainColor, int> GetCardsColorMap(List<TrainCard> cards)
        {
            var result = new Dictionary<TrainColor, int>();
            foreach (var card in cards)
            {
                TrainColor color = card.Color;

                if (result.ContainsKey(color))
                    result[color]++;
                else 
                    result[color] = 1;
            }

            return result;
        }

        public static List<TrainColor> FindAvailableColorsByCount(List<TrainCard> cards, int requiredCount) 
        {
            var result = new HashSet<TrainColor>();
            
            Dictionary<TrainColor, int> colorMap = GetCardsColorMap(cards);

            foreach (var kv in colorMap)
            {
                if (kv.Value >= requiredCount)
                    result.Add(kv.Key);
            }

            return result.ToList();
        }
    }
}
