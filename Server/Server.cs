using LiteNetLib;
using LiteNetLib.Utils;
using System.Diagnostics;
using System.Numerics;

namespace Server
{
    static class Server
    {
        public const int BUFFER_SIZE = 1024;
        public const float TICK_RATE = 60.0f;
        public const float TIME_BETWEEN_TICKS = 1f / TICK_RATE;
        private static int CurrentTick = 2;
        private static float TimeTaken = 0;

        private const int MAX_CONNECTIONS = 10;
        private const int MAX_STRING_LENGTH = 100;
        private const string KEY = "gameKey";

        public static Dictionary<int, NetPeer> ConnectedClients = new Dictionary<int, NetPeer>();
        public static Stopwatch Stopwatch = new Stopwatch();

        public static void StartServer(int port)
        {
            Stopwatch = Stopwatch.StartNew();

            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager server = new NetManager(listener);

            Console.WriteLine("===Server===");

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
                string message = Message.CreateSyncMessage(CurrentTick);
                SendMessage(playerId, message);
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                string message = dataReader.GetString(MAX_STRING_LENGTH);
                Message.Decode(message);
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

            while (!Console.KeyAvailable)
            {
                server.PollEvents();
                TimeTaken += Stopwatch.Elapsed.Milliseconds;
                Debug.WriteLine(TimeTaken);
                while (TimeTaken > TIME_BETWEEN_TICKS*1000)
                {
                    TimeTaken -= TIME_BETWEEN_TICKS*1000;
                    HandleTick();
                    CurrentTick++;
                    Write("Tick Updated");
                }
                Stopwatch.Restart();
                float sleepingTime = (1000*TIME_BETWEEN_TICKS - TimeTaken);
                Debug.WriteLine($"Sleeping Time: {sleepingTime}ms");
                sleepingTime = Math.Clamp(sleepingTime, 0, TIME_BETWEEN_TICKS * 1000);
                Thread.Sleep((int)sleepingTime);
            }
            server.Stop();
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
            if(CurrentTick % TICK_RATE/2 == 0)
            {
                string message = Message.CreateSyncMessage(CurrentTick);
                SendGlobalMessage(message);
            }
            PlayerManager.ProcessPlayerMovement();
        }

        public static void SetTick(int tick)
        {
            CurrentTick = tick;
        }

        public static int GetTick() => CurrentTick;

        #endregion

        #region Debugging

        //public static void 

        #endregion

        public static void RemoveId(int id)
        {
            ConnectedClients.Remove(id);
            PlayerManager.Remove(id);
        }

    }
}
