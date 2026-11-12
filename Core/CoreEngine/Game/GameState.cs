using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreEngine.Game
{
    /// !! Must be serializable to JSON
    /// Must use the exact same class in unity project
    public class GameState
    {
        public int TestProperty { get; set; } = 100;
    }
}
