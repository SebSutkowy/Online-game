using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Xna.Framework.Content;


namespace Client
{
    public enum TilemapName
    {
        SampleMap1,
        PlayerSpawn,
        Room1,
        Room2,
        BossRoom
    }

    public enum TileType : uint
    {
        Floor = 0,
        Wall = 1,
        Trap = 2,
        Chest = 3,
        ActiveTrap = 4,
        Door = 5,
        PlayerSpawn = 6
    }

    [Flags]
    public enum TileTags
    {
        None = 0b_0000,
        IsCollideable = 0b_0001,
        IsDoor = 0b_0010,
        IsPlayerSpawn = 0b_0100,
        IsClickable = 0b_1000
    }

    [Flags]
    public enum TilemapTags
    {
        None = 0b_0000,
        BossRoom = 0b_0001,
        RegularRoom = 0b_0010,
        PuzzleRoom = 0b_0100,
        PlayerSpawn = 0b_1000
    }

    public class Tile
    {
        public Point Position { get; set; }
        public TileType Type { get; set; }
        public TileTags Tags { get; set; }
    }

    public class TilemapChange
    {
        public int PlayerId { get; set; }
        public int Tick { get; set; }
        public Point Position { get; set; }
    }

    public class Tilemap
    {
        public const int TILE_SIZE = 100;
        private Dictionary<TileType, Texture2D> Assets = new Dictionary<TileType, Texture2D>();
        private HashSet<TileType> InteractiveTiles = new HashSet<TileType>()
        {
            TileType.Trap,
            TileType.Chest,
            TileType.ActiveTrap
        };
        public static TilemapName tilemapName;
        private Dictionary<Point, Tile> tilemap { get; set; }
        private Dictionary<Point, Tile> InteractiveTilemap { get; set; }
        private TilemapTags Tags { get; set; }

        private List<TilemapChange> ChangesYetToHappen { get; set; }
        public List<TilemapChange> Changes { get; private set; }

        private List<EffectBox> EffectBoxes { get; set; }
        private List<Point> EffectBoxLocations { get; set; }

        public Tilemap(TilemapData data)
        {
            tilemap = data.Tilemap;
            InteractiveTilemap = data.InteractiveTilemap;
            Tags = data.Tags;
        }

        public Tilemap(string filePath)
        {
            Deserialize(filePath);
        }

        public Tilemap()
        {
            tilemap = new Dictionary<Point, Tile>();
            InteractiveTilemap = new Dictionary<Point, Tile>();
            Tags = TilemapTags.None;

            ChangesYetToHappen = new List<TilemapChange>();
            Changes = new List<TilemapChange>();

            EffectBoxes = new List<EffectBox>();
            EffectBoxLocations = new List<Point>();
        }

        public bool HasTag(TilemapTags tag) => (Tags & tag) != 0;

        public void AddChanges(TilemapChange change, Mode networkMode)
        {
            switch (networkMode)
            {
                case Mode.Client:
                    ChangesYetToHappen.Add(change);
                    break;
                case Mode.Server:
                    if (InteractiveTilemap.ContainsKey(change.Position))
                        InteractiveTilemap.Remove(change.Position);
                    Changes.Add(change);
                    string message = Message.CreateInteractionConfirmationMessage(change.PlayerId, change.Tick, tilemapName, change.Position, "1");
                    Server.SendGlobalMessage(message);
                    break;
            }
        }

        public void Update()
        {
            // CHANGES
            List<TilemapChange> changesToRemove = new List<TilemapChange>();
            foreach (TilemapChange change in ChangesYetToHappen)
            {
                if (change.Tick <= Client.GetTick())
                {
                    if (InteractiveTilemap.ContainsKey(change.Position))
                        InteractiveTilemap.Remove(change.Position);
                    Changes.Add(change);
                    changesToRemove.Add(change);
                }
            }
            foreach (TilemapChange change in changesToRemove)
            {
                if (ChangesYetToHappen.Contains(change))
                    ChangesYetToHappen.Remove(change);
            }

            // CREATING DAMAGE BOXES / HEAL BOXES
            for (int i = EffectBoxes.Count - 1; i >= 0; i--)
            {
                EffectBox box = EffectBoxes[i];
                box.Update();
                if (box.Lifespan <= 0)
                {
                    EffectBoxes.RemoveAt(i);
                    EffectBoxLocations.Remove(box.Position);
                    InteractiveTilemap[GetTilemapPos(box.Position)].Type = TileType.Trap;
                    Server.PlayerManager.RemoveEffectBox(box.Position);
                    string message = Message.CreateTrapToggleMessage(tilemapName, GetTilemapPos(box.Position), TrapActivationStatus.Inactive);
                    Server.SendGlobalMessage(message);
                }
            }
        }

        public void CheckBoxCollisions(int playerId)
        {
            foreach (EffectBox box in EffectBoxes)
            {
                box.UpdateCollision(playerId);
            }
        }

        public bool EffectBoxAlreadyThere(Point point) => EffectBoxLocations.Contains(point);

        public void AddEffectBox(EffectBox box)
        {
            EffectBoxes.Add(box);
            EffectBoxLocations.Add(box.Position);
            InteractiveTilemap[GetTilemapPos(box.Position)].Type = TileType.ActiveTrap;
            string message = Message.CreateTrapToggleMessage(tilemapName, GetTilemapPos(box.Position), TrapActivationStatus.Active);
            Server.SendGlobalMessage(message);
        }


        public static Rectangle TileToHitbox(Point pos) => new Rectangle(pos.X * TILE_SIZE, pos.Y * TILE_SIZE, TILE_SIZE, TILE_SIZE);

        public static Point GetTilemapPos(Vector2 pos)
        {
            Point tilemapPos = new Point();
            int tileSize = (int)(TILE_SIZE / Camera.Distance);
            float dx = (pos.X / tileSize), dy = (pos.Y / tileSize);
            if (dx < 0 && dx % 1 != 0)
                tilemapPos.X = (int)dx - 1;
            else
                tilemapPos.X = (int)dx;
            if (dy < 0 && dy % 1 != 0)
                tilemapPos.Y = (int)dy - 1;
            else
                tilemapPos.Y = (int)dy;


            return tilemapPos;
        }

        public static Point GetTilemapPos(Point posPoint)
        {
            Vector2 pos = new Vector2(posPoint.X, posPoint.Y);
            return GetTilemapPos(pos);
        }

        public static bool IsANeighbour(Point pos1, Point pos2, int range = 1) => Math.Abs(pos1.X - pos2.X) <= range && Math.Abs(pos1.Y - pos2.Y) <= range;

        public Tile GetInteractiveTile(Point pos) => InteractiveTilemap.ContainsKey(pos) ? InteractiveTilemap[pos] : null;
        public Tile GetTile(Point pos) => tilemap.ContainsKey(pos) ? tilemap[pos] : null;

        public void ImportTexture(TileType type, Texture2D texture)
        {
            if (!Assets.ContainsKey(type))
                Assets.Add(type, texture);
            else
                Assets[type] = texture;
        }

        public void ImportTextures(ContentManager Content)
        {
            foreach (TileType tileType in Enum.GetValues(typeof(TileType)))
            {
                string name = $"{tileType.ToString()}Tile";
                if (File.Exists($@"Content\{name}.xnb"))
                    ImportTexture(tileType, Content.Load<Texture2D>(name));
            }
        }

        public void AddTile(Point position, TileType type)
        {
            Tile newTile = new Tile()
            {
                Position = position,
                Type = type
            };
            if (!InteractiveTiles.Contains(type))
            {
                if (tilemap.ContainsKey(newTile.Position))
                    tilemap[position] = newTile;
                else
                    tilemap.Add(position, newTile);
            }
            else
            {
                if (InteractiveTilemap.ContainsKey(newTile.Position))
                    InteractiveTilemap[position] = newTile;
                else
                    InteractiveTilemap.Add(position, newTile);
            }
        }

        public void RemoveTile(Point position)
        {
            if (tilemap.ContainsKey(position))
                tilemap.Remove(position);
            if(InteractiveTilemap.ContainsKey(position))
                InteractiveTilemap.Remove(position);
        }

        public void ImportFrom(string filePath)
        {
            if (!File.Exists(filePath))
                return;
            Deserialize(filePath);
        }

        public void Serialize()
        {
            TilemapData tilemapData = new TilemapData()
            {
                Tilemap = tilemap,
                InteractiveTilemap = InteractiveTilemap
            };
            JsonSerializerOptions options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            options.Converters.Add(new TilemapConverter());
            options.Converters.Add(new PointConverter());
            options.Converters.Add(new TileConverter());

            string json = JsonSerializer.Serialize(tilemapData, options);
            File.WriteAllText("map.json", json);
        }

        public void Deserialize(string filePath)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new TilemapConverter());
            options.Converters.Add(new PointConverter());
            options.Converters.Add(new TileConverter());
            TilemapData tilemapData = new TilemapData();

            tilemapData = JsonSerializer.Deserialize<TilemapData>(File.ReadAllText(filePath), options);
            tilemap = tilemapData.Tilemap;
            InteractiveTilemap = tilemapData.InteractiveTilemap;
            Tags = tilemapData.Tags;
        }

        public void Draw()
        {


            foreach(var (pos, tile) in tilemap)
            {
                DrawTile(tile);
            }
            foreach (var (pos, tile) in InteractiveTilemap)
            {
                DrawTile(tile);
            }
        }

        public void DrawTile(Tile tile, int alpha=255)
        {
            if (!Assets.ContainsKey(tile.Type))
                return;
            Point position = new Point(tile.Position.X * TILE_SIZE, tile.Position.Y * TILE_SIZE);
            Rectangle rect = new Rectangle(position, new Point(TILE_SIZE, TILE_SIZE));
            Camera.Draw(Assets[tile.Type], rect, new Color(255, 255, 255, alpha));
        }
    }
}
