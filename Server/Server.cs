using LiteNetLib;
using LiteNetLib.Utils;
using System.Diagnostics;
using System.Numerics;

namespace Server
{
    static class Server
    {
        public const float TICK_RATE = 30.0f;
        public const float minTimeBetweenTicks = 1f / TICK_RATE;
        public static int currentTick = 0;

        private const int maxConnections = 10;
        private const int maxStringLength = 100;
        private const string key = "gameKey";

        public static Dictionary<int, NetPeer> ConnectedClients = new Dictionary<int, NetPeer>();

        public static void StartServer(int port)
        {
            EventBasedNetListener listener = new EventBasedNetListener();
            NetManager server = new NetManager(listener);

            Console.WriteLine("===Server===");

            server.Start(port);
            Console.WriteLine($"[SERVER] Started on port {port}");

            listener.ConnectionRequestEvent += request =>
            {
                if (server.ConnectedPeersCount < maxConnections)
                    request.AcceptIfKey(key);
                else
                    request.Reject();
            };

            listener.PeerConnectedEvent += peer =>
            {
                Console.WriteLine($"[SERVER] Connection at {peer}");
                NetDataWriter writer = new NetDataWriter();
                int playerId = GetNextAvailableId();
                ConnectedClients.Add(playerId, peer);
                PlayerManager.CreatePlayer(playerId);
                writer.Put($"0 {playerId} {currentTick}");
                peer.Send(writer, DeliveryMethod.ReliableOrdered);
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
            {
                string message = dataReader.GetString(maxStringLength);
                DecodeMessage(message);
                dataReader.Recycle();
            };

            listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
            {
                Console.WriteLine($"[SERVER] {peer.Address} Disconnected from Server: {disconnectInfo.ToString()}");
                int id = GetClientId(peer);
                SendGlobalMessage($"1 {id}");
                ConnectedClients.Remove(id);
            };

            while (!Console.KeyAvailable)
            {
                server.PollEvents();
                HandleTick();
                currentTick++;
                Thread.Sleep((int)(minTimeBetweenTicks * 1000));
            }
            server.Stop();
        }

        #region ID
        public static int GetNextAvailableId()
        {
            int n = 0;
            while (n < maxConnections && ConnectedClients.ContainsKey(n))
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
        }

        public static void SendMessage(int clientId, string message)
        {
            SendMessage(ConnectedClients[clientId], message);
        }

        #endregion

        #region Handling Ticks

        private static void HandleTick()
        {
            PlayerManager.ProcessPlayerMovement();
        }

        #endregion

        #region Debugging

        //public static void 

        #endregion

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
                case "3":
                    tick = int.Parse(code[2]);
                    X = float.Parse(code[3]);
                    Y = float.Parse(code[4]);
                    InputPayload input = new InputPayload
                    {
                        Tick = tick,
                        Input = new Vector2(X, Y)
                    };
                    PlayerManager.AddInput(playerId, input);
                    Console.WriteLine($"[SERVER] Player {playerId} Sent an input of ({X}, {Y}) at tick {tick}");
                    break;
            }
        }

    }
}
