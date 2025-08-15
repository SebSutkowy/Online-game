using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server;

class Program
{
    private const float SERVER_TICK_RATE = 30.0f;
    private const float minTimeBetweenTicks = 1f / SERVER_TICK_RATE;

    public static void StartServer()
    {
        string key = "gameKey";
        int port = 9050;
        int maxConnections = 10;
        int maxStringLength = 100;

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
        };

        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            string message = dataReader.GetString(maxStringLength);
            Console.WriteLine($"[SERVER] Received Data from {fromPeer.Address}: {message}");
            dataReader.Recycle();
        };

        while(!Console.KeyAvailable)
        {
            server.PollEvents();
            Thread.Sleep((int) minTimeBetweenTicks*1000);
        }
        server.Stop();
    }

    public void DecodeMessage(string message)
    {

    }

    static void Main(string[] args)
    {
        Thread serverThread = new Thread(StartServer);
        serverThread.Start();

        
    }
}