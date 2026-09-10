namespace CoreEngine.Cards
{
    public class TicketCard : Card 
    {
        public City Origin { get; init; }
        public City Destination { get; init; }
        public int Points { get; init; }

        public TicketCard(City origin, City destination, int points)
        {
            Points = points;
            Origin = origin;
            Destination = destination;
        }
    }
}
