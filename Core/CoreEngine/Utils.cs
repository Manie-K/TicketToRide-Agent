using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine
{
    public class Utils
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

    public enum PlayerColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Black
    }

    public enum TrainColor
    {
        Pink,
        White,
        Blue,
        Yellow,
        Orange,
        Black,
        Red,
        Green,
        Locomotive, // ANY color
        Wild // Used to represent routes that require any color to be completed, never drawn from deck!
    }

    public static class DecksInitialization
    {
        public static List<TicketCard> GetInitialTicketCards()
        {
            var tickets = new List<TicketCard>();

            // --- LONG ROUTES (6 Cards - Blue Backed) ---
            tickets.Add(new TicketCard(new City("Edinburgh"), new City("Athina"), 21));
            tickets.Add(new TicketCard(new City("København"), new City("Erzurum"), 21));
            tickets.Add(new TicketCard(new City("Cádiz"), new City("Stockholm"), 21));
            tickets.Add(new TicketCard(new City("Brest"), new City("Petrograd"), 20));
            tickets.Add(new TicketCard(new City("Lisboa"), new City("Danzig"), 20));
            tickets.Add(new TicketCard(new City("Palermo"), new City("Moskva"), 20));

            // --- REGULAR ROUTES (40 Cards) ---
            tickets.Add(new TicketCard(new City("Madrid"), new City("Moskva"), 25));
            tickets.Add(new TicketCard(new City("Amsterdam"), new City("Pamplona"), 22));
            tickets.Add(new TicketCard(new City("Brest"), new City("Venezia"), 21));
            tickets.Add(new TicketCard(new City("Berlin"), new City("Roma"), 20));
            tickets.Add(new TicketCard(new City("London"), new City("Berlin"), 20));
            tickets.Add(new TicketCard(new City("Athina"), new City("Wilno"), 17));
            tickets.Add(new TicketCard(new City("Madrid"), new City("Dieppe"), 17));
            tickets.Add(new TicketCard(new City("London"), new City("Wien"), 13));
            tickets.Add(new TicketCard(new City("Paris"), new City("Wien"), 13));
            tickets.Add(new TicketCard(new City("Stockholm"), new City("Wien"), 13));
            tickets.Add(new TicketCard(new City("Athina"), new City("Angora"), 12));
            tickets.Add(new TicketCard(new City("Barcelona"), new City("München"), 12));
            tickets.Add(new TicketCard(new City("Berlin"), new City("Moskva"), 12));
            tickets.Add(new TicketCard(new City("Paris"), new City("Roma"), 12));
            tickets.Add(new TicketCard(new City("Budapest"), new City("Sofia"), 11));
            tickets.Add(new TicketCard(new City("London"), new City("Paris"), 11));
            tickets.Add(new TicketCard(new City("München"), new City("Venezia"), 11));
            tickets.Add(new TicketCard(new City("Paris"), new City("Berlin"), 11));
            tickets.Add(new TicketCard(new City("Roma"), new City("Smyrna"), 11));
            tickets.Add(new TicketCard(new City("Warszawa"), new City("Smolensk"), 11));
            tickets.Add(new TicketCard(new City("Essen"), new City("Kyiv"), 10));
            tickets.Add(new TicketCard(new City("Madrid"), new City("Zürich"), 10));
            tickets.Add(new TicketCard(new City("Roma"), new City("Athina"), 10));
            tickets.Add(new TicketCard(new City("Venezia"), new City("Constantinople"), 10));
            tickets.Add(new TicketCard(new City("Kyiv"), new City("Rostov"), 9));
            tickets.Add(new TicketCard(new City("Kyiv"), new City("Sochi"), 9));
            tickets.Add(new TicketCard(new City("Marseille"), new City("Essen"), 9));
            tickets.Add(new TicketCard(new City("Paris"), new City("Marseille"), 9));
            tickets.Add(new TicketCard(new City("Sarajevo"), new City("Sevastopol"), 9));
            tickets.Add(new TicketCard(new City("Stockholm"), new City("København"), 9));
            tickets.Add(new TicketCard(new City("Angora"), new City("Kharkov"), 8));
            tickets.Add(new TicketCard(new City("Berlin"), new City("Bucuresti"), 8));
            tickets.Add(new TicketCard(new City("London"), new City("Edinburgh"), 8));
            tickets.Add(new TicketCard(new City("München"), new City("Budapest"), 8));
            tickets.Add(new TicketCard(new City("Palermo"), new City("Constantinople"), 8));
            tickets.Add(new TicketCard(new City("Sofia"), new City("Smyrna"), 8));
            tickets.Add(new TicketCard(new City("Zürich"), new City("Brindisi"), 8));
            tickets.Add(new TicketCard(new City("Frankfurt"), new City("Praha"), 7));
            tickets.Add(new TicketCard(new City("Zürich"), new City("Frankfurt"), 7));
            tickets.Add(new TicketCard(new City("Madrid"), new City("Lisboa"), 6));
            tickets.Add(new TicketCard(new City("Rostov"), new City("Erzurum"), 5));

            return tickets;
        }

        public static List<TrainCard> GetInitialTrainCards()
        {
            var cardCounts = new Dictionary<TrainColor, int>
            {
                { TrainColor.Pink, 12 },
                { TrainColor.White, 12 },
                { TrainColor.Blue, 12 },
                { TrainColor.Yellow, 12 },
                { TrainColor.Orange, 12 },
                { TrainColor.Black, 12 },
                { TrainColor.Red, 12 },
                { TrainColor.Green, 12 },
                { TrainColor.Locomotive, 14 }
            };

            return cardCounts
                .SelectMany(kvp => Enumerable.Repeat(new TrainCard(kvp.Key), kvp.Value))
                .ToList();
        }
    }


    public abstract class Card
    {

    }

    public class TicketCard : Card 
    {
        public City Origin { get; init; }
        public City Destination { get; init; }
        public int Points { get; init; }

        public TicketCard(City origin, City destination, int points)
        {
            this.Points = points;
            this.Origin = origin;
            this.Destination = destination;
        }
    }

    public class TrainCard : Card
    {
        public TrainColor Color { get; init; }

        public TrainCard(TrainColor color)
        {
            this.Color = color;
        }

        public bool IsLocomotive => Color == TrainColor.Locomotive;
    }

    public class City
    {
        public string Name { get; init;  }
        public bool HasStation { get; private set; }

        public City(string name)
        {
            this.Name = name;
        }


    }

    public class Route
    {
        public City Origin { get; }
        public City Destination { get; }
        public int Length { get; }
        public TrainColor Color { get; } // here it can be even 'Wild'
        public Player? ClaimedBy { get; }
        public int LocomotivesNeeded { get; }
        public bool IsTunnel { get; }

        public Route(City origin, City destination, int length, TrainColor color, int locomotivesNeeded = 0, bool isTunnel)
        {
            this.Origin = origin;
            this.Destination = destination;
            this.Length = length;
            this.Color = color;
            this.LocomotivesNeeded = locomotivesNeeded;
            this.IsTunnel = isTunnel;
        }

        public bool IsClaimed => ClaimedBy != null;
    }

    public struct PlayerChoice
    {
        public Object value;
        public string description;
    }
}
