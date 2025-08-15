using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server;

class Program
{
    public static void StartServer()
    {
        int port = 9050;
        int maxConnections = 10;
        int maxStringLength = 100;

        EventBasedNetListener listener = new EventBasedNetListener();
        NetManager server = new NetManager(listener);

        Console.WriteLine("===Server===");

        server.Start(port);
        Console.WriteLine($"Started on port {port}");

        listener.ConnectionRequestEvent += request =>
        {
            if (server.ConnectedPeersCount < maxConnections)
                request.AcceptIfKey("gameKey");
            else
                request.Reject();
        };

        listener.PeerConnectedEvent += peer =>
        {
            Console.WriteLine($"Connection at {peer}");
            NetDataWriter writer = new NetDataWriter();
            writer.Put("Hello Client");
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        };

        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            Console.WriteLine($"Received Data from {fromPeer.Address}: {dataReader.GetString(maxStringLength)}");
            dataReader.Recycle();
        };

        while(!Console.KeyAvailable)
        {
            server.PollEvents();

        }
        server.Stop();
    }

    static void Main(string[] args)
    {
        Thread serverThread = new Thread(StartServer);
        serverThread.Start();

        
    }
}