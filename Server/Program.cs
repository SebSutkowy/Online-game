using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server;

class Program
{
    static void Main(string[] args)
    {
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
            Console.WriteLine($"Connection at {peer}");
            NetDataWriter writer = new NetDataWriter();
            writer.Put("Hello Client");
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        };

        listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod, channel) =>
        {
            Console.WriteLine($"Received Data from {fromPeer.Address}: {dataReader.GetString(100 /* max length of string */)}");
            dataReader.Recycle();
        };

        while (!Console.KeyAvailable)
        {
            server.PollEvents();
            Thread.Sleep(15);
        }
        server.Stop();
    }
}