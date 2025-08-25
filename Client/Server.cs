using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Client
{
    static class Server
    {
        public const int BUFFER_SIZE = 1024;
        public const float TICK_RATE = 60.0f;
        public const float TIME_BETWEEN_TICKS = 1f / TICK_RATE;
        private static int CurrentTick = 2;
        public static bool IsRunning { get; private set; } = false;
        private static bool IsStopped = false;
        private static float Timer;
        private static float deltaTime;

        private const int MAX_CONNECTIONS = 10;
        private const int MAX_STRING_LENGTH = 100;
        private const string KEY = "gameKey";

        private static EventBasedNetListener listener;
        private static NetManager server;

        public static Dictionary<int, NetPeer> ConnectedClients = new Dictionary<int, NetPeer>();
        public static PlayerManager PlayerManager;

        public static void StartServer(int port)
        {
            IsRunning = true;

            listener = new EventBasedNetListener();
            server = new NetManager(listener);

            server.Start(port);
            Write($"Started on port {port}");

            listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < MAX_CONNECTIONS)
                    request.AcceptIfKey(KEY);
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer =>
            {
                Write($"Connection at {peer}");

                int playerId = GetNextAvailableId();
                ConnectedClients.Add(playerId, peer);
                string message = Message.CreateClientJoinMessage(playerId, CurrentTick);
                SendMessage(playerId, message);
                PlayerManager.SendPlayerStates(playerId);
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                string message = dataReader.GetString(MAX_STRING_LENGTH);
                Message.Decode(PlayerManager, message);
                dataReader.Recycle();
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                Write($"{peer.Address} Disconnected from Server: {disconnectInfo.ToString()}");
                int id = GetClientId(peer);
                string message = Message.CreateClientDisconnectMessage(id);
                SendGlobalMessage(message);
                RemoveId(id);
            };
        }

        public static void Update(GameTime gameTime)
        {
            if (!IsRunning)
            {
                if(!IsStopped)
                {
                    server.Stop();
                    IsStopped = true;
                }
                return;
            }
            server.PollEvents();

            deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            Timer += deltaTime;
            while (Timer > TIME_BETWEEN_TICKS)
            {
                Timer -= TIME_BETWEEN_TICKS;
                HandleTick();
                CurrentTick++;
            }
        }

        public static void Write(string message)
        {
            Console.WriteLine($"[SERVER - {CurrentTick}t] {message}");
        }

        #region Client
        public static int GetNextAvailableId()
        {
            int n = 0;
            while (n < MAX_CONNECTIONS && ConnectedClients.ContainsKey(n))
                n++;
            if (!ConnectedClients.ContainsKey(n))
                return n;

            return -1;
        }

        public static int GetClientId(NetPeer client)
        {
            foreach (var (clientId, peer) in ConnectedClients)
            {
                if (peer == client)
                    return clientId;
            }
            return -1;
        }

        public static void ClientJoin()
        {

        }
        #endregion

        #region Sending Messages
        public static void SendGlobalMessage(string message)
        {
            foreach (var (id, client) in ConnectedClients)
            {
                SendMessage(client, message);
            }
        }

        public static void SendMessage(NetPeer client, string message)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put(message);
            client.Send(writer, DeliveryMethod.ReliableOrdered);
            Write($"Sent message \"{message}\" to {client.Address}");
        }

        public static void SendMessage(int clientId, string message)
        {
            SendMessage(ConnectedClients[clientId], message);
        }

        #endregion

        #region Handling Ticks

        private static void HandleTick()
        {
            if(CurrentTick % (TICK_RATE) == 0)
            {
                string message = Message.CreateSyncMessage(CurrentTick);
                SendGlobalMessage(message);
            }
            PlayerManager.ProcessPlayerMovement(Mode.Server);
        }

        public static void SetTick(int tick)
        {
            CurrentTick = tick;
        }

        public static int GetTick() => CurrentTick;

        #endregion

        #region Debugging

        public static void DrawPlayers()
        {
            PlayerManager.DrawPlayers();
        }

        public static void DisplayConsole()
        {
            Console.DisplayConsole();
        }

        #endregion

        public static void SetPlayerTexture(Texture2D Texture)
        {
            PlayerManager = new PlayerManager(Texture);
        }
        public static void RemoveId(int id)
        {
            ConnectedClients.Remove(id);
            PlayerManager.Remove(id);
        }

    }
}
