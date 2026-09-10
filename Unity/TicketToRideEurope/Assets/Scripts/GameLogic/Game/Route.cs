using CoreEngine.Game;
using System;

namespace CoreEngine
{
    public class Route
    {
        public City Origin { get; }
        public City Destination { get; }
        public int Length { get; }
        /// <summary>
        /// Required card color. <see cref="TrainColor.Wild"/> means a gray route
        /// (player may use any one color, or locomotives).
        /// </summary>
        public TrainColor Color { get; }
        public Player? ClaimedBy { get; private set; }
        /// <summary>Minimum number of Locomotive cards required (ferry routes).</summary>
        public int LocomotivesNeeded { get; }
        public bool IsTunnel { get; }
        /// <summary>
        /// The parallel route in a double-route pair. Null for single routes.
        /// Both siblings reference each other.
        /// </summary>
        public Route? Sibling { get; internal set; }

        public Route(City origin, City destination, int length, TrainColor color,
                     bool isTunnel, int locomotivesNeeded = 0)
        {
            Origin = origin;
            Destination = destination;
            Length = length;
            Color = color;
            IsTunnel = isTunnel;
            LocomotivesNeeded = locomotivesNeeded;
        }

        public bool IsClaimed => ClaimedBy != null;

        /// <summary>
        /// Claims this route for <paramref name="player"/>:
        /// sets ownership, decrements trains, records the route and awards points immediately.
        /// Does NOT validate affordability — caller must do that.
        /// </summary>
        internal void Claim(Player player)
        {
            if (IsClaimed) throw new InvalidOperationException("Route is already claimed.");
            ClaimedBy = player;
            player.TrainsRemaining -= Length;
            player.ClaimedRoutes.Add(this);
            player.Score += RoutePoints(Length);
        }

        /// <summary>Points scored for claiming a route of the given length (Route Scoring Table).</summary>
        public static int RoutePoints(int length) => length switch
        {
            1 => 1,
            2 => 2,
            3 => 4,
            4 => 7,
            6 => 15,
            8 => 21,
            _ => throw new ArgumentOutOfRangeException(nameof(length), $"Unsupported route length: {length}")
        };

        public override string ToString() =>
            $"{Origin.Name} — {Destination.Name} ({Length}, {Color}" +
            (IsTunnel ? ", tunnel" : "") +
            (LocomotivesNeeded > 0 ? $", ferry:{LocomotivesNeeded}loco" : "") +
            (Sibling != null ? ", double" : "") +
            (IsClaimed ? $", claimed by {ClaimedBy}" : "") +
            ")";
    }
}
