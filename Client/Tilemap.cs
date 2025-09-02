using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace Client
{
    public enum TileType : int
    {
        Floor = 0,
        Wall = 1,
        Trap = 2,
        Chest = 3,
        ActiveTrap = 4
    }

    public class Tile
    {
        public Point Position { get; set; }
        public TileType Type { get; set; }
    }

    public class TilemapChange
    {
        public int PlayerId { get; set; }
        public int Tick { get; set; }
        public Point Position { get; set; }
    }

    public static class Tilemap
    {
        public const int TILE_SIZE = 100;
        private static Dictionary<TileType, Texture2D> Assets = new Dictionary<TileType, Texture2D>();

        private static HashSet<TileType> CollidableTiles = new HashSet<TileType>()
        {
            TileType.Wall
        };
        private static HashSet<TileType> InteractiveTiles = new HashSet<TileType>()
        {
            TileType.Trap,
            TileType.Chest,
            TileType.ActiveTrap
        };
        private static Dictionary<Point, Tile> tilemap = new Dictionary<Point, Tile>();
        private static Dictionary<Point, Tile> InteractiveTilemap = new Dictionary<Point, Tile>();

        private static List<TilemapChange> ChangesYetToHappen = new List<TilemapChange>();
        public static List<TilemapChange> Changes { get; private set; } = new List<TilemapChange>();

        private static List<EffectBox> EffectBoxes = new List<EffectBox>();
        private static List<Point> EffectBoxLocations = new List<Point>();

        public static void AddChanges(TilemapChange change, Mode networkMode)
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
                    string message = Message.CreateInteractionConfirmationMessage(change.PlayerId, change.Tick, change.Position, "1");
                    Server.SendGlobalMessage(message);
                    break;
            }
        }

        public static void Update()
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
                    string message = Message.CreateTrapToggleMessage(GetTilemapPos(box.Position), TrapActivationStatus.Inactive);
                    Server.SendGlobalMessage(message);
                }
            }
        }

        public static void CheckBoxCollisions(int playerId)
        {
            foreach (EffectBox box in EffectBoxes)
            {
                box.UpdateCollision(playerId);
            }
        }

        public static bool EffectBoxAlreadyThere(Point point) => EffectBoxLocations.Contains(point);

        public static void AddEffectBox(EffectBox box)
        {
            EffectBoxes.Add(box);
            EffectBoxLocations.Add(box.Position);
            InteractiveTilemap[GetTilemapPos(box.Position)].Type = TileType.ActiveTrap;
            string message = Message.CreateTrapToggleMessage(GetTilemapPos(box.Position), TrapActivationStatus.Active);
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

        public static Point GetTilemapPos(Point pos)
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

        public static bool IsANeighbour(Point pos1, Point pos2) => Math.Abs(pos1.X - pos2.X) <= 1 && Math.Abs(pos1.Y - pos2.Y) <= 1;

        public static Tile GetInteractiveTile(Point pos) => InteractiveTilemap.ContainsKey(pos) ? InteractiveTilemap[pos] : null;
        public static Tile GetTile(Point pos) => tilemap.ContainsKey(pos) ? tilemap[pos] : null;

        public static void ImportTexture(TileType type, Texture2D texture)
        {
            if (!Assets.ContainsKey(type))
                Assets.Add(type, texture);
            else
                Assets[type] = texture;
        }

        public static void AddTile(Point position, TileType type)
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

        public static void RemoveTile(Point position)
        {
            if (tilemap.ContainsKey(position))
                tilemap.Remove(position);
            if(InteractiveTilemap.ContainsKey(position))
                InteractiveTilemap.Remove(position);
        }

        public static void ImportFrom(string filePath)
        {
            Deserialize(filePath);
        }

        public static void Serialize()
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

        public static void Deserialize(string filePath)
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new TilemapConverter());
            options.Converters.Add(new PointConverter());
            options.Converters.Add(new TileConverter());
            TilemapData tilemapData = new TilemapData();

            tilemapData = JsonSerializer.Deserialize<TilemapData>(File.ReadAllText(filePath), options);
            tilemap = tilemapData.Tilemap;
            InteractiveTilemap = tilemapData.InteractiveTilemap;
        }

        public static void Draw()
        {


            foreach(var (pos, tile) in tilemap)
            {
                Point position = new Point(pos.X * TILE_SIZE, pos.Y * TILE_SIZE);
                Rectangle rect = new Rectangle(position, new Point(TILE_SIZE, TILE_SIZE));
                Camera.Draw(Assets[tile.Type], rect, Color.White);
            }
            foreach (var (pos, tile) in InteractiveTilemap)
            {
                Point position = new Point(pos.X * TILE_SIZE, pos.Y * TILE_SIZE);
                Rectangle rect = new Rectangle(position, new Point(TILE_SIZE, TILE_SIZE));
                Camera.Draw(Assets[tile.Type], rect, Color.White);
            }
        }

        public static void DrawTile(Tile tile)
        {
            Point position = new Point(tile.Position.X * TILE_SIZE, tile.Position.Y * TILE_SIZE);
            Rectangle rect = new Rectangle(position, new Point(TILE_SIZE, TILE_SIZE));
            Camera.Draw(Assets[tile.Type], rect, new Color(255, 255, 255, 50));
        }
    }
}
