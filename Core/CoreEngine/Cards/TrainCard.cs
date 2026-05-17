namespace CoreEngine.Cards
{
    public class TrainCard : Card
    {
        public TrainColor Color { get; init; }

        public TrainCard(TrainColor color)
        {
            Color = color;
        }

        public bool IsLocomotive => Color == TrainColor.Locomotive;
    }
}
