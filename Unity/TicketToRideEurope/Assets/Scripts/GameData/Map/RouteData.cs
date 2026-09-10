using UnityEngine;
using CoreEngine;

namespace CoreEngine.UnityData
{
    [CreateAssetMenu(
        fileName = "RouteData",
        menuName = "Ticket to Ride/Map/Route Data"
    )]
    public class RouteData : ScriptableObject
    {
        public CityData Origin;
        public CityData Destination;

        public int Length;
        public TrainColor Color;

        public bool IsTunnel;
        public int LocomotivesNeeded;
    }
}