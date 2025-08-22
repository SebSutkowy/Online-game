using Server;
using System.Numerics;

namespace Server;

enum MessageType : ushort
{
    Sync = 0,
    ClientJoin = 1,
    ClientDisconnect = 2,
    SpawnPlayer = 3,
    PlayerSpawnRequest = 4,
    PlayerState = 5,
    PlayerInput = 6
}

static class Message
{

    public static void Decode(string message)
    {
        string[] splitMessage = message.Split(' ');
        ushort typeValue = ushort.Parse(splitMessage[0]);
        bool isDefined = Enum.IsDefined(typeof(MessageType), typeValue);
        MessageType type;
        if (!isDefined)
            return;
        type = (MessageType)typeValue;


        int tick, id;
        float X, Y;
        switch (type)
        {
            case MessageType.PlayerSpawnRequest:
                id = int.Parse(splitMessage[1]);
                PlayerManager.CreatePlayer(id);
                Server.Write($"Received player spawn request for client {id}");
                break;
            case MessageType.PlayerInput:
                id = int.Parse(splitMessage[1]);
                tick = int.Parse(splitMessage[2]);
                X = float.Parse(splitMessage[3]);
                Y = float.Parse(splitMessage[4]);
                InputPayload input = new InputPayload
                {
                    Tick = tick,
                    Input = new Vector2(X, Y)
                };
                Server.Write($"Received an input of {tick} {X} {Y} from client {id}");
                PlayerManager.AddInput(id, input);
                break;
        }
    }

    public static string CreateSyncMessage(int tick) => $"{(ushort)MessageType.Sync} {tick}";

    public static string CreateClientJoinMessage(int id, int tick) => $"{(ushort)MessageType.ClientJoin} {id} {tick}";

    public static string CreateClientDisconnectMessage(int id) => $"{(ushort)MessageType.ClientDisconnect} {id}";

    public static string CreatePlayerSpawnMessage(int id, int tick, Vector2 position) => $"{(ushort)MessageType.SpawnPlayer} {id} {tick} {position.X} {position.Y}";

    public static string CreatePlayerStateMessage(int id, StatePayload state) => $"{(ushort)MessageType.PlayerState} {id} {state.Tick} {state.Position.X} {state.Position.Y}";

    public static string CreatePlayerInputMessage(int id, InputPayload input) => $"{(ushort)MessageType.PlayerInput} {id} {input.Tick} {input.Input.X} {input.Input.Y}";


}
