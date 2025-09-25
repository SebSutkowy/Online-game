
using Microsoft.Xna.Framework;
using System.Security.Cryptography.X509Certificates;

namespace Client
{
    static class NetworkManager
    {
        private static Mode Mode = Mode.None;

        public const int BUFFER_SIZE = 1024;
        public const float TICK_RATE = 60.0f;
        public const float TIME_BETWEEN_TICKS = 1f / TICK_RATE;

        private static int serverPort = 9050;

        public static bool DisplayConsole { get; private set; } = false;

        public static Mode GetMode() => Mode;

        public static PlayerManager PlayerManager => Mode switch
        {
            Mode.Client => Client.PlayerManager,
            Mode.Server => Server.PlayerManager,
            _ => null
        };

        public static void ToggleConsole()
        {
            DisplayConsole = !DisplayConsole;
        }

        public static void ChangeNetworkMode(Mode mode)
        {
            if (Mode != Mode.None)
                return;
            switch (mode)
            {
                case Mode.Server:
                    DisplayConsole = true;
                    break;
                case Mode.Client:
                    DisplayConsole = false;
                    break;
            }

            Mode = mode;
        }

        public static void GetChangeNetworkMode()
        {
            if (InputManager.ReceivedPressedInput(Input.SwitchToServer))
                ChangeNetworkMode(Mode.Server);
            if (InputManager.ReceivedPressedInput(Input.SwitchToClient))
                ChangeNetworkMode(Mode.Client);
        }

        public static void Update(GameTime gameTime)
        {
            GetChangeNetworkMode();

            switch (Mode)
            {
                case Mode.Server:
                    if (!Server.IsRunning)
                        Server.StartServer(serverPort);
                    Server.Update(gameTime);
                    break;
                case Mode.Client:
                    if (!Client.IsRunning)
                        Client.Initialize();
                    Client.Update(gameTime);
                    break;
            }
            if (InputManager.ReceivedPressedInput(Input.DisplayConsole))
                ToggleConsole();

        }

        public static void Draw()
        {
            switch (Mode)
            {
                case Mode.Server:
                    Server.PlayerManager.DrawPlayers();
                    break;
                case Mode.Client:
                    Client.PlayerManager.DrawPlayers();
                    break;
            }
            if(DisplayConsole)
                Console.DisplayConsole();

        }

    }
}
