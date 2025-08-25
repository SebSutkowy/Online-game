using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Client;

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

    public static void Decode(PlayerManager playerManager, string message)
    {
        string[] splitMessage = message.Split(' ');
        ushort typeValue = (ushort)int.Parse(splitMessage[0]);
        bool isDefined = Enum.IsDefined(typeof(MessageType), typeValue);
        MessageType type;
        if (!isDefined)
            return;
        type = (MessageType)typeValue;


        int tick, id;
        float X, Y;
        switch (type)
        {
            case MessageType.Sync:
                Client.SetTick(int.Parse(splitMessage[1]));
                Client.Write($"Received tick update to {int.Parse(splitMessage[1])}");
                break;
            case MessageType.ClientJoin:
                id = int.Parse(splitMessage[1]);
                tick = int.Parse(splitMessage[2]);
                Client.OnClientJoin(id, tick);
                Client.Write($"Player {id} Joined");
                break;
            case MessageType.ClientDisconnect:
                id = int.Parse(splitMessage[1]);
                Client.OnClientDisconnect(id);
                Client.Write($"Player {id} Disconnected");
                break;
            case MessageType.SpawnPlayer:
            case MessageType.PlayerState:
                id = int.Parse(splitMessage[1]);
                tick = int.Parse(splitMessage[2]);
                X = float.Parse(splitMessage[3]);
                Y = float.Parse(splitMessage[4]);
                StatePayload state = new StatePayload
                {
                    Tick = tick,
                    Position = new Vector2(X, Y)
                };
                playerManager.SetPlayerState(id, state);
                Client.Write($"Received player state: {{Tick: {tick} X: {X} Y: {Y}}}");
                break;
            case MessageType.PlayerSpawnRequest:
                id = int.Parse(splitMessage[1]);
                playerManager.CreatePlayer(id);
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
                playerManager.AddInput(id, input);
                break;
        }
    }

    public static string CreateSyncMessage(int tick) => $"{(ushort)MessageType.Sync} {tick}";

    public static string CreateClientJoinMessage(int id, int tick) => $"{(ushort)MessageType.ClientJoin} {id} {tick}";

    public static string CreateClientDisconnectMessage(int id) => $"{(ushort)MessageType.ClientDisconnect} {id}";

    public static string CreatePlayerSpawnMessage(int id, int tick, Vector2 position) => $"{(ushort)MessageType.SpawnPlayer} {id} {tick} {position.X} {position.Y}";

    public static string CreatePlayerSpawnRequestMessage(int id) => $"{(ushort)MessageType.PlayerSpawnRequest} {id}";

    public static string CreatePlayerStateMessage(int id, StatePayload state) => $"{(ushort)MessageType.PlayerState} {id} {state.Tick} {state.Position.X} {state.Position.Y}";

    public static string CreatePlayerInputMessage(int id, InputPayload input) => $"{(ushort)MessageType.PlayerInput} {id} {input.Tick} {input.Input.X} {input.Input.Y}";


}
