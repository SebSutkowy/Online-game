using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server;

class Program
{
    private const float SERVER_TICK_RATE = 30.0f;
    private const float minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

    private static Dictionary<int, NetPeer> ConnectedClients = new Dictionary<int, NetPeer>();
    private static string key = "gameKey";
    private static int port = 9050;
    private static int maxConnections = 10;
    private static int maxStringLength = 100;

    private static PlayerManager _playerManager = new PlayerManager();

    public static void StartServer()
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
            writer.Put("Hello Client");
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
            ConnectedClients.Add(GetNextAvailableId(), peer);
        };

        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            string message = dataReader.GetString(maxStringLength);
            Console.WriteLine($"[SERVER] Received Data from {fromPeer.Address}: {message}");
            DecodeMessage(message);
            dataReader.Recycle();
        };

        while(!Console.KeyAvailable)
        {
            server.PollEvents();
            Thread.Sleep((int) minTimeBetweenTicks*1000);
        }
        server.Stop();
    }

    public static int GetNextAvailableId()
    {
        int n = 0;
        while (n < maxConnections && ConnectedClients.ContainsKey(n))
            n++;
        if (!ConnectedClients.ContainsKey(n))
            return n;

        return -1;
    }

    public static void SendGlobalMessage(string message)
    {
        foreach(var (id, client) in ConnectedClients)
        {
            SendMessage(client, message);
        }
    }

    private static void SendMessage(NetPeer client, string message)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(message);
        client.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    private static StatePayload CreateStatePayload(int tick, float posX, float posY) => new StatePayload
    {
        Tick = tick,
        Position = new Vector2(posX, posY)
    };

    private static InputPayload CreateInputPayload(int tick, float dirX, float dirY) => new InputPayload
    {
        Tick = tick,
        Input = new Vector2(dirX, dirY)
    };

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
            case "2": // Player state payload: 2 {id} {tick} {posX} {posY}
                tick = int.Parse(code[2]);
                X = float.Parse(code[3]);
                Y = float.Parse(code[4]);
                StatePayload state = CreateStatePayload(tick, X, Y);
                _playerManager.UpdatePlayer(playerId, state);
                break;
            case "3":
                tick = int.Parse(code[2]);
                X = float.Parse(code[3]);
                Y = float.Parse(code[4]);
                InputPayload input = CreateInputPayload(tick, X, Y);
                _playerManager.AddInput(playerId, input);
                break;
        }
    }

    static void Main(string[] args)
    {
        Thread serverThread = new Thread(StartServer);
        serverThread.Start();

        
    }
}