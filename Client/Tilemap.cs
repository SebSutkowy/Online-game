using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Client
{
    public enum TileType : int
    {
        Floor = 0,
        Wall = 1,
        Trap = 2,
        Chest = 3
    }

    public class Tile
    {
        public Point Position { get; set; }
        public TileType Type { get; set; }
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
            TileType.Chest
        };
        private static Dictionary<Point, Tile> tilemap = new Dictionary<Point, Tile>();
        private static Dictionary<Point, Tile> InteractiveTilemap = new Dictionary<Point, Tile>();

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

        public static void Deserialize()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.Converters.Add(new TilemapConverter());
            options.Converters.Add(new PointConverter());
            options.Converters.Add(new TileConverter());
            TilemapData tilemapData = new TilemapData();

            tilemapData = JsonSerializer.Deserialize<TilemapData>(File.ReadAllText("map.json"), options);
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
