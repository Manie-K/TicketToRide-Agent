using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CoreEngine.Game
{
    public class Connection
    {
        // These must be the same in Unity project! Possible to define them in file somewhere
        private const string host = "127.0.0.1";
        private const int port = 5000;

        public static void SendGameStateUpdate(GameState state)
        {
            byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(state);

            using (UdpClient client = new UdpClient())
            {
                client.Send(bytes, bytes.Length, host, port);
            }

        }
    }
}
