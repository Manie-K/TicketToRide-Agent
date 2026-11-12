using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using GameState;
// Some usings are missing - simple alt+enter will fix

public class GameUpdateManager : MonoBehaviour
{
    // These must be the same in C# core project! Possible to define them in file somewhere
    private const string host = "127.0.0.1";
    private const int port = 5000;

    public override void Start()
    {
        _ = Task.Run(() => Listen());
    }

    public static async Task Listen()
    {
        using UdpClient listener = new UdpClient(port);

        while (true)
        {
            UdpReceiveResult result = await listener.ReceiveAsync();

            try
            {
                GameState? state = JsonSerializer.Deserialize<GameState>(result.Buffer);

                if (state != null)
                {
                    OnGameStateReceived(state);
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Invalid packet: {ex.Message}");
            }
        }
    }


    private static void OnGameStateReceived(GameState state)
    {
        // Franek - tu pole do popisu
    }
}