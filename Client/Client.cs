
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace Client
{
    static class Client
    {
        #region Server constants
        public const int BUFFER_SIZE = 1024;
        public const float TICK_RATE = 60.0f;
        public const float TIME_BETWEEN_TICKS = 1f / TICK_RATE;
        private static float Timer;
        private static int CurrentTick;
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
        public static bool IsRunning { get; private set; } = false;
        private static Stack<string> Messages = new Stack<string>();
        public static PlayerManager PlayerManager { get; private set; }

        private static float deltaTime;

        public static void Initialize()
        {
            IsRunning = true;

            listener = new EventBasedNetListener();
            client = new NetManager(listener);
            client.Start();
            server = ConnectToServer(client, IP, PORT, KEY);

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                Debug.WriteLine("Here");
                int maxMessageLength = 100; // In characters
                string message = dataReader.GetString(maxMessageLength); // gets the message from the server
                if (GetMostRecentMessage() != message)
                    Messages.Push(message);
                Message.Decode(PlayerManager, message);
                dataReader.Recycle();
            };
        }

        public static void Update(GameTime gameTime)
        {
            client.PollEvents();

            bool Connection = CheckServerConnection();
            if (Connection)
            {
                TickTimer(gameTime);
                //if (InputManager.ReceivedPressedInput(Input.GetStates))
                //    GetStates();
                PlayerManager.Update();
                Dungeon.ActiveTilemap.Update();
            }
        }

        #region Client
        public static int GetClientId() => ClientId;

        public static void OnClientJoin(int clientId, int tick)
        {
            ClientId = clientId;
            SetTick(tick);
        }

        public static void OnClientDisconnect(int id)
        {
            if(PlayerManager.Contains(id))
                PlayerManager.Remove(id);
        }

        #endregion

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
                SendMessage(Message.CreatePlayerInputMessage(ClientId, input));
        }

        public static int GetTick() => CurrentTick;

        public static void SetTick(int tick)
        {
            CurrentTick = tick;
        }

        public static float GetDeltaTime() => deltaTime;

        public static float TimeSinceLastTick() => Timer;
        #endregion

        #region Messages
        public static void SendMessage(string message)
        {
            // message to server
            NetDataWriter writer = new NetDataWriter();
            writer.Put(message);
            server.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public static string GetMostRecentMessage() => Messages.Count > 0 ? Messages.Peek() : "";
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
            if (GetMostRecentMessage() != message)
                Messages.Push(message);
            return false;
        }
        #endregion

        #region Debugging

        public static void Write(string message)
        {
            Console.WriteLine($"[CLIENT - {CurrentTick}t] {message}");
        }

        //public static void GetStates()
        //{
        //    int i = 0;
        //    Dictionary<int, Payload> states = new Dictionary<int, Payload>();
        //    foreach (StatePayload statePayload in PlayerManager.GetPlayerStates(GetClientId()))
        //    {
        //        if (statePayload != null)
        //            states.Add(i, statePayload.ToPayload());
        //        i++;
        //    }
        //    string jsonText = JsonSerializer.Serialize(states, new JsonSerializerOptions { WriteIndented = true });
        //    File.WriteAllText("playerStatesDebugging.json", jsonText);
        //}

        #endregion

        public static void SetPlayerTexture(Texture2D Texture)
        {
            PlayerManager = new PlayerManager(Texture);
        }


    }
}
