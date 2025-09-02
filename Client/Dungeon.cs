using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client
{
    static class Dungeon
    {
        private static Dictionary<TilemapName, Tilemap> Tilemaps { get; set; }
        public static TilemapName ActiveTilemap;

        private static bool StartedBossFightSequence { get; set; } = false;

        public static void ImportTilemaps(List<TilemapName> tilemaps)
        {
            foreach (TilemapName tilemap in tilemaps)
            {
                string fileName = $@"Presets\Maps\{tilemap.ToString()}.json";
                if(File.Exists(fileName))
                    Tilemaps[tilemap] = new Tilemap(fileName);
            }
            ActiveTilemap = TilemapName.PlayerSpawn;
        }

        public static void AddChanges(TilemapName tilemap, TilemapChange Change, Mode networkMode)
        {
            Tilemaps[tilemap].AddChanges(Change, networkMode);
        }

        public static void AddTile(TilemapName tilemap, Point pos, TileType type)
        {
            Tilemaps[tilemap].AddTile(pos, type);
        }

        public static void Update()
        {
            // DO SERVER SIDED IF PLAYER ENTERS BOSS ROOM START BOSS FIGHT
        }


    }
}
