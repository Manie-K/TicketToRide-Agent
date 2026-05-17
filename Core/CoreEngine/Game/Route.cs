using CoreEngine.Game;

namespace CoreEngine
{
    public class Route
    {
        public City Origin { get; }
        public City Destination { get; }
        public int Length { get; }
        public TrainColor Color { get; } // here it can be even 'Wild'
        public Player? ClaimedBy { get; }
        public int LocomotivesNeeded { get; }
        public bool IsTunnel { get; }

        public Route(City origin, City destination, int length, TrainColor color, bool isTunnel, int locomotivesNeeded = 0)
        {
            this.Origin = origin;
            this.Destination = destination;
            this.Length = length;
            this.Color = color;
            this.IsTunnel = isTunnel;
            this.LocomotivesNeeded = locomotivesNeeded;
        }

        public bool IsClaimed => ClaimedBy != null;
    }
}
