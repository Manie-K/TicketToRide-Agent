using CoreEngine.Game;
using System;

namespace CoreEngine
{
    public class City
    {
        public string Name { get; init;  }
        public Player? StationOwner { get; private set; }


        public City(string name)
        {
            this.Name = name;
        }
        
        public bool HasStation => StationOwner != null;

        public void BuildStation(Player player)
        {
            if (StationOwner != null) 
            { 
                throw new Exception("Illegal game state. Trying to build station in occupied city."); 
            }
            
            StationOwner = player;
        }
    }
}
