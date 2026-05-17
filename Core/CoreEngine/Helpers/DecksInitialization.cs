using CoreEngine.Cards;

namespace CoreEngine.Helpers
{
    public static class DecksInitialization
    {
        public static List<TicketCard> GetInitialTicketCards()
        {
            var tickets = new List<TicketCard>();

            //// --- LONG ROUTES (6 Cards - Blue Backed) ---
            //tickets.Add(new TicketCard(new City("Edinburgh"), new City("Athina"), 21));
            //tickets.Add(new TicketCard(new City("København"), new City("Erzurum"), 21));
            //tickets.Add(new TicketCard(new City("Cádiz"), new City("Stockholm"), 21));
            //tickets.Add(new TicketCard(new City("Brest"), new City("Petrograd"), 20));
            //tickets.Add(new TicketCard(new City("Lisboa"), new City("Danzig"), 20));
            //tickets.Add(new TicketCard(new City("Palermo"), new City("Moskva"), 20));

            //// --- REGULAR ROUTES (40 Cards) ---
            //tickets.Add(new TicketCard(new City("Madrid"), new City("Moskva"), 25));
            //tickets.Add(new TicketCard(new City("Amsterdam"), new City("Pamplona"), 22));
            //tickets.Add(new TicketCard(new City("Brest"), new City("Venezia"), 21));
            //tickets.Add(new TicketCard(new City("Berlin"), new City("Roma"), 20));
            //tickets.Add(new TicketCard(new City("London"), new City("Berlin"), 20));
            //tickets.Add(new TicketCard(new City("Athina"), new City("Wilno"), 17));
            //tickets.Add(new TicketCard(new City("Madrid"), new City("Dieppe"), 17));
            //tickets.Add(new TicketCard(new City("London"), new City("Wien"), 13));
            //tickets.Add(new TicketCard(new City("Paris"), new City("Wien"), 13));
            //tickets.Add(new TicketCard(new City("Stockholm"), new City("Wien"), 13));
            //tickets.Add(new TicketCard(new City("Athina"), new City("Angora"), 12));
            //tickets.Add(new TicketCard(new City("Barcelona"), new City("München"), 12));
            //tickets.Add(new TicketCard(new City("Berlin"), new City("Moskva"), 12));
            //tickets.Add(new TicketCard(new City("Paris"), new City("Roma"), 12));
            //tickets.Add(new TicketCard(new City("Budapest"), new City("Sofia"), 11));
            //tickets.Add(new TicketCard(new City("London"), new City("Paris"), 11));
            //tickets.Add(new TicketCard(new City("München"), new City("Venezia"), 11));
            //tickets.Add(new TicketCard(new City("Paris"), new City("Berlin"), 11));
            //tickets.Add(new TicketCard(new City("Roma"), new City("Smyrna"), 11));
            //tickets.Add(new TicketCard(new City("Warszawa"), new City("Smolensk"), 11));
            //tickets.Add(new TicketCard(new City("Essen"), new City("Kyiv"), 10));
            //tickets.Add(new TicketCard(new City("Madrid"), new City("Zürich"), 10));
            //tickets.Add(new TicketCard(new City("Roma"), new City("Athina"), 10));
            //tickets.Add(new TicketCard(new City("Venezia"), new City("Constantinople"), 10));
            //tickets.Add(new TicketCard(new City("Kyiv"), new City("Rostov"), 9));
            //tickets.Add(new TicketCard(new City("Kyiv"), new City("Sochi"), 9));
            //tickets.Add(new TicketCard(new City("Marseille"), new City("Essen"), 9));
            //tickets.Add(new TicketCard(new City("Paris"), new City("Marseille"), 9));
            //tickets.Add(new TicketCard(new City("Sarajevo"), new City("Sevastopol"), 9));
            //tickets.Add(new TicketCard(new City("Stockholm"), new City("København"), 9));
            //tickets.Add(new TicketCard(new City("Angora"), new City("Kharkov"), 8));
            //tickets.Add(new TicketCard(new City("Berlin"), new City("Bucuresti"), 8));
            //tickets.Add(new TicketCard(new City("London"), new City("Edinburgh"), 8));
            //tickets.Add(new TicketCard(new City("München"), new City("Budapest"), 8));
            //tickets.Add(new TicketCard(new City("Palermo"), new City("Constantinople"), 8));
            //tickets.Add(new TicketCard(new City("Sofia"), new City("Smyrna"), 8));
            //tickets.Add(new TicketCard(new City("Zürich"), new City("Brindisi"), 8));
            //tickets.Add(new TicketCard(new City("Frankfurt"), new City("Praha"), 7));
            //tickets.Add(new TicketCard(new City("Zürich"), new City("Frankfurt"), 7));
            //tickets.Add(new TicketCard(new City("Madrid"), new City("Lisboa"), 6));
            //tickets.Add(new TicketCard(new City("Rostov"), new City("Erzurum"), 5));

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
}
