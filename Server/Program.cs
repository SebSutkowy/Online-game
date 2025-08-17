using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server;

class Program
{  
    static void Main(string[] args)
    {
        int port = 9050;
        Server.StartServer(port);
    }
}