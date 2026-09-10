using UnityEngine;

namespace CoreEngine.UnityData
{
    [CreateAssetMenu(
        fileName = "CityData",
        menuName = "Ticket to Ride/Map/City Data"
    )]
    public class CityData : ScriptableObject
    {
        public string CityName;
        public Vector2 BoardPosition;
    }
}