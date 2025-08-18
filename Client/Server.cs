
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Reflection;

namespace Client
{
    static class Server
    {
        #region Server constants
        public const int BUFFER_SIZE = 1024;
        public const float TICK_RATE = 5.0f;
        public const float TIME_BETWEEN_TICKS = 1f / TICK_RATE;
        private static float Timer;
        private static int CurrentTick = 0;
        #endregion

        #region Server connection constants
        private const string KEY = "gameKey";
        private const string IP = "localhost";
        private const int PORT = 9050;
        #endregion

        private static EventBasedNetListener listener;
        private static NetManager client;
        private static NetPeer server;

        private static int ClientId;
        private static Stack<string?> Messages = new Stack<string?>();

        private static float deltaTime;

        public static void Initialize()
        {
            Messages.Push(null);

            listener = new EventBasedNetListener();
            client = new NetManager(listener);
            client.Start();
            server = ConnectToServer(client, IP, PORT, KEY);

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                int maxMessageLength = 100; // In characters
                string message = dataReader.GetString(maxMessageLength); // gets the message from the server
                if (Messages.Peek() != message)
                    Messages.Push(message);
                DecodeMessage(message);
                dataReader.Recycle();
            };
        }

        public static void Update(GameTime gameTime)
        {
            client.PollEvents();

            bool Connection = Server.CheckServerConnection();
            if (Connection)
            {
                TickTimer(gameTime);
                PlayerManager.Update();
            }
        }

        #region Tick Handling
        private static void TickTimer(GameTime gameTime)
        {
            deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Timer += deltaTime;
            while (Timer >= TIME_BETWEEN_TICKS)
            {
                Timer -= TIME_BETWEEN_TICKS;
                HandleTick();
                CurrentTick++;
            }
        }

        private static void HandleTick()
        {
            int bufferIndex = CurrentTick % BUFFER_SIZE;
            InputPayload input = new InputPayload
            {
                Tick = CurrentTick,
                Input = InputManager.GetInput()
            };
            if (input.Input != Vector2.Zero)
                SendMessage(input.ToString(ClientId));
        }

        public static int GetTick() => CurrentTick;

        public static float GetDeltaTime() => deltaTime;
        #endregion

        #region Messages
        public static void SendMessage(string message)
        {
            // message to server
            NetDataWriter writer = new NetDataWriter();
            writer.Put(message);
            server.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public static string GetMostRecentMessage() => Messages.Peek() != null ? Messages.Peek() : "";

        public static void DecodeMessage(string message)
        {
            int playerId,
                tick;
            float X,
                  Y;
            string[] code = message.Split(' ');
            if (code.Length == 0)
                return;
            string opcode = code[0];
            playerId = int.Parse(code[1]);
            switch (opcode)
            {
                case "0": // On Join: 0 {id} {tick}
                    ClientId = playerId;
                    CurrentTick = int.Parse(code[2]);
                    PlayerManager.UpdatePlayer(playerId);
                    break;
                case "1": // On Leave: 1 {id}
                    PlayerManager.Remove(playerId);
                    break;
                case "2": // Player state payload: 2 {id} {tick} {posX} {posY}
                    tick = int.Parse(code[2]);
                    X = float.Parse(code[3]);
                    Y = float.Parse(code[4]);
                    StatePayload state = new StatePayload
                    {
                        Tick = tick,
                        Position = new Vector2(X, Y)
                    };
                    PlayerManager.SetTargetState(playerId, state);
                    break;
                case "3": // Player Input payload: 3 {id} {tick} {dirX} {dirY}
                    tick = int.Parse(code[2]);
                    X = float.Parse(code[3]);
                    Y = float.Parse(code[4]);
                    InputPayload input = new InputPayload
                    {
                        Tick = tick,
                        Input = new Vector2(X, Y)
                    };
                    PlayerManager.AddInput(playerId, input);
                    break;
            }
        }
        #endregion

        #region Server Connection
        public static NetPeer ConnectToServer(NetManager client, string ip, int port, string key) => client.Connect(ip, port, key);

        public static bool CheckServerConnection()
        {
            string message;
            switch (server.ConnectionState)
            {
                case ConnectionState.Outgoing:
                    message = "Connecting to server...";
                    break;
                case ConnectionState.Disconnected:
                    message = "Failed to connect to server";
                    if (InputManager.ReceivedPressedInput(Input.RefreshServer))
                        server = ConnectToServer(client, IP, PORT, KEY);
                    break;
                default:
                    return true;
            }
            if (Messages?.Peek() != message)
                Messages?.Push(message);
            return false;
        }
        #endregion

        public static int GetClientId() => ClientId;

    }
}
