using System;
using Microsoft.Xna.Framework;

namespace Client;

enum MessageType : ushort
{
    Sync = 0,
    ClientJoin = 1,
    ClientDisconnect = 2,
    SpawnPlayer = 3,
    PlayerSpawnRequest = 4,
    PlayerState = 5,
    PlayerInput = 6,
    Interaction = 7,
    InteractionConfirmation = 8,
    PlayerHealthChange = 9,
    TrapActivation = 10,
    EnteredBossRoom = 11,
    ChangeTilemap = 12,
    UpdateEnemy = 13,
    PlayerAttacking = 14
}

enum TrapActivationStatus : ushort
{
    Active = 0,
    Inactive = 1
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


        int tick, id, health;
        float X, Y;
        Point tilemapPos = new Point();
        string seed;
        TilemapChange Change;
        TilemapName tilemap;
        EntityType entityType;
        
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
            
            case MessageType.Interaction:
                id = int.Parse(splitMessage[1]);
                tick = int.Parse(splitMessage[2]);
                tilemap = (TilemapName)int.Parse(splitMessage[3]);
                tilemapPos.X = int.Parse(splitMessage[4]);
                tilemapPos.Y = int.Parse(splitMessage[5]);
                Change = new TilemapChange()
                {
                    Tick = tick,
                    Position = tilemapPos
                };
                Dungeon.AddChanges(tilemap, Change, Mode.Server);
                Server.Write($"Received Interaction message position: {tilemapPos} in {tilemap}");
                break;
            
            case MessageType.InteractionConfirmation:
                tick = int.Parse(splitMessage[1]);
                tilemap = (TilemapName)int.Parse(splitMessage[2]);
                tilemapPos.X = int.Parse(splitMessage[3]);
                tilemapPos.Y = int.Parse(splitMessage[4]);
                seed = splitMessage[5];
                Change = new TilemapChange()
                {
                    Tick = tick,
                    Position = tilemapPos
                };
                Dungeon.AddChanges(tilemap, Change, Mode.Client);
                Client.Write($"Received Interaction confirmation position: {tilemapPos}");
                break;
            
            case MessageType.PlayerHealthChange:
                id = int.Parse(splitMessage[1]);
                int newHealth = int.Parse(splitMessage[2]);
                Client.PlayerManager.SetPlayerHealth(id, newHealth);
                Client.Write($"Received new health message: {newHealth}");
                break;
            
            case MessageType.TrapActivation:
                tilemap = (TilemapName)int.Parse(splitMessage[1]);
                tilemapPos.X = int.Parse(splitMessage[2]);
                tilemapPos.Y = int.Parse(splitMessage[3]);
                TrapActivationStatus status = (TrapActivationStatus) ushort.Parse(splitMessage[4]);
                if (status == TrapActivationStatus.Inactive)
                    Dungeon.AddTile(tilemap, tilemapPos, TileType.Trap);
                else
                    Dungeon.AddTile(tilemap, tilemapPos, TileType.ActiveTrap);
                Client.Write($"Received new Trap toggle message: {tilemapPos}");
                break;
            
            case MessageType.EnteredBossRoom:
                break;
            
            case MessageType.ChangeTilemap:
                tilemap = (TilemapName)int.Parse(splitMessage[1]);
                Client.PlayerManager.ResetPositions();
                Client.Write($"Received tilemap change: {Dungeon.ActiveTilemapName} -> {tilemap}");
                Dungeon.ChangeTilemap(tilemap);
                break;
            
            case MessageType.UpdateEnemy:
                id = int.Parse(splitMessage[1]);
                entityType = (EntityType)int.Parse(splitMessage[2]);
                X = float.Parse(splitMessage[3]);
                Y = float.Parse(splitMessage[4]);
                health = int.Parse(splitMessage[5]);
                int maxHealth = int.Parse(splitMessage[6]);
                Client.Write($"Received {entityType.ToString()} Update message");

                if (Dungeon.Enemies.ContainsKey(id))
                {
                    Dungeon.Enemies[id].Position = new Vector2(X, Y);
                    Dungeon.Enemies[id].SetHealth(health);
                    break;
                }
                Dungeon.Enemies[id] = entityType switch
                {
                    EntityType.Boss => new Boss1(Camera.EntityAssets[entityType], X, Y, Camera.EntityAssets[entityType].Width, Camera.EntityAssets[entityType].Height, 0, maxHealth),
                    EntityType.Totem => new Totem(Camera.EntityAssets[entityType], X, Y, Camera.EntityAssets[entityType].Width, Camera.EntityAssets[entityType].Height, maxHealth)
                };
                break;
            
            case MessageType.PlayerAttacking:
                int damage = int.Parse(splitMessage[1]);
                int enemyId = int.Parse(splitMessage[2]);
                if (Dungeon.Enemies.ContainsKey(enemyId))
                    Dungeon.Enemies[enemyId].TakeDamage(damage);
                Server.Write($"Received Player Attack: -{damage}HP -> {Dungeon.Enemies[enemyId].Type}");
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

    public static string CreateInteractionMessage(int id, int tick, TilemapName tilemap, Point tilemapPos) => $"{(ushort)MessageType.Interaction} {id} {tick} {(int)tilemap} {tilemapPos.X} {tilemapPos.Y}";
    public static string CreateInteractionConfirmationMessage(int tick, TilemapName tilemap, Point tilemapPos, int seed) => $"{(ushort)MessageType.InteractionConfirmation} {tick} {(int)tilemap} {tilemapPos.X} {tilemapPos.Y} {seed}";

    public static string CreatePlayerHealthChangeMessage(int id, int newHealth) => $"{(ushort)MessageType.PlayerHealthChange} {id} {newHealth}";

    public static string CreateTrapToggleMessage(TilemapName tilemap, Point tilemapPos, TrapActivationStatus ActivatedStatus) => $"{(ushort)MessageType.TrapActivation} {(int)tilemap} {tilemapPos.X} {tilemapPos.Y} {(ushort)ActivatedStatus}";

    public static string CreateEnteredBossRoomMessage() => $"{(ushort)MessageType.EnteredBossRoom}";

    public static string CreateChangeTilemapMessage(TilemapName tilemap) => $"{(ushort)MessageType.ChangeTilemap} {(int)tilemap}";

    public static string CreateUpdateEnemyMessage(Enemy enemy) => $"{(ushort)MessageType.UpdateEnemy} {enemy.Id} {(int)enemy.Type} {enemy.X} {enemy.Y} {enemy.Health} {enemy.MaxHealth}";

    public static string CreatePlayerAttackMessage(int damage, int enemyId) => $"{(ushort)MessageType.PlayerAttacking} {damage} {enemyId}";
}
