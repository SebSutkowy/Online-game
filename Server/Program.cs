using System.Numerics;
using System.Threading.Tasks.Dataflow;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server;

class Player
{
    public Player()
    { }

    public string GetString()
    {
        return $"{Position.X} {Position.Y} {Velocity.X} {Velocity.Y}";
    }

    public void ReadString(string info)
    {
        // ignore first and second number (opcode and player id)
        float[] values = Array.ConvertAll(info.Split(' '), float.Parse);
        Position = new Vector2(values[2], values[3]);
        Velocity = new Vector2(values[4], values[5]);
    }

    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
}

/* Opcodes                          | Sample
 * 0 - new player receive id        | 0 <id>
 * 1 - passing player info          | 1 <id> <posX> <posY> <velX> <velY>
 * 2 - player leaving               | 2 <id>
 */

class Program
{
    static Dictionary<int, NetPeer> clientNames = new Dictionary<int, NetPeer>(); // store the numbers (id) of the players
    static Dictionary<NetPeer, Player> playerList = new Dictionary<NetPeer, Player>(); // store the positions and velocities of the players
    static int GetNum(NetPeer peer)
    {
        foreach(int key in clientNames.Keys)
        {
            if (clientNames[key] == peer)
                return key;
        }
        return -1;
    }

    static void SendMessage(string message, NetPeer peer)
    {
        NetDataWriter writer = new NetDataWriter();
        writer.Put(message);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    static void SendGlobalMessage(string message, params NetPeer[] peers)
    {
        //sends message to everyone but the passed peers
        foreach(var (num, peer) in clientNames)
        {
            if (!Array.Exists(peers, element => element == peer))
                SendMessage(message, peer);
        }
    }

    static void Main(string[] args)
    {
        int n;

        EventBasedNetListener listener = new EventBasedNetListener();
        NetManager server = new NetManager(listener);

        Console.WriteLine("===Server===");

        server.Start(9050); // Port
        Console.WriteLine("Started on port 9050");

        listener.ConnectionRequestEvent += request =>
        {
            if (server.ConnectedPeersCount < 10)
                request.AcceptIfKey("gameKey");
            else
                request.Reject();
        };

        listener.PeerConnectedEvent += peer =>
        {
            n = 0;
            Console.WriteLine($"Connection at {peer}");
            while(clientNames.ContainsKey(n))
            {
                n++;
            }
            SendMessage($"0 {n}", peer);
            clientNames.Add(n, peer);
            playerList.Add(peer, new Player());

        };

        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            string message = dataReader.GetString(100 /* max length of the string */ );
            Console.WriteLine($"Received Data from Client {GetNum(fromPeer)}: {message}");
            List<string> nums = message.Split(' ').ToList<string>();
            switch(nums[0]) 
            {
                case "1":
                    playerList[fromPeer].ReadString(message);
                    SendGlobalMessage($"1 {nums[1]} {playerList[fromPeer].GetString()}", fromPeer);
                    break;
            }
            
            dataReader.Recycle();
        };

        listener.PeerDisconnectedEvent += (peer, disconnectInfo) =>
        {
            Console.WriteLine($"Client {GetNum(peer)} Disconnected: {disconnectInfo}");
            SendGlobalMessage($"2 {GetNum(peer)}", peer);
            clientNames.Remove(GetNum(peer));
            playerList.Remove(peer);
        };

        

        while (!Console.KeyAvailable)
        {
            server.PollEvents();
            
            Thread.Sleep(15);
        }
        server.Stop();
    }
}