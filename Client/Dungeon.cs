using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Client
{
    static class Dungeon
    {
        private static Dictionary<TilemapName, Tilemap> Tilemaps { get; set; } = new Dictionary<TilemapName, Tilemap>();
        public static TilemapName ActiveTilemapName { get; private set; }

        public static Tilemap ActiveTilemap => Tilemaps[ActiveTilemapName];

        private static bool StartedBossFightSequence { get; set; } = false;
        private static Boss1 Boss { get; set; }

        public static void ImportTilemaps(List<TilemapName> tilemaps)
        {
            foreach (TilemapName tilemap in tilemaps)
            {
                string fileName = $@"Presets\Maps\{tilemap.ToString()}.json";
                if (File.Exists(fileName))
                {
                    Debug.WriteLine($"{fileName} exists");
                    Tilemaps[tilemap] = new Tilemap(fileName);
                }
                else
                {
                    Debug.WriteLine($"{fileName} does not exist");
                }
            }
            ActiveTilemapName = TilemapName.PlayerSpawn;
        }

        public static void AddChanges(TilemapName tilemap, TilemapChange Change, Mode networkMode)
        {
            Tilemaps[tilemap].AddChanges(Change, networkMode);
        }

        public static void AddEffectBox(TilemapName tilemap, EffectBox box)
        {
            Tilemaps[tilemap].AddEffectBox(box);
        }

        public static void AddTile(TilemapName tilemap, Point pos, TileType type)
        {
            Tilemaps[tilemap].AddTile(pos, type);
        }

        public static void UpdateTilemap()
        {
            foreach (Tilemap tilemap in Tilemaps.Values)
            {
                tilemap.Update();
            }
        }

        public static void ChangeTilemap(TilemapName tilemap)
        {
            ActiveTilemapName = tilemap;
            if (NetworkManager.GetMode() == Mode.Server)
            {
                string message = Message.CreateChangeTilemapMessage(tilemap);
                Server.SendGlobalMessage(message);
                foreach (TilemapChange change in Tilemaps[tilemap].Changes)
                {
                    message = Message.CreateInteractionConfirmationMessage(change.Tick, tilemap, change.Position, 1);
                    Server.SendGlobalMessage(message);
                }
            }
        }


        public static void Update()
        {
            UI.AddMessage($"{ActiveTilemapName}");
            // DO SERVER SIDED IF PLAYER ENTERS BOSS ROOM START BOSS FIGHT
            if(NetworkManager.GetMode() == Mode.Server)
            {
                if (InputManager.ReceivedPressedInput(Input.StartBossFight) && StartedBossFightSequence)
                {
                    Boss.StartBossFight();
                    Debug.WriteLine("Boss entered phase 1");
                }
                if (InputManager.ReceivedPressedInput(Input.StartBossFight) && !StartedBossFightSequence)
                {
                    StartBossFight();
                    Debug.WriteLine("Boss prepared for phase 1");
                }

            }
            if (StartedBossFightSequence && NetworkManager.GetMode() != Mode.Client)
                BossFight();

        }

        public static void StartBossFight()
        {
            StartedBossFightSequence = true;
            ChangeTilemap(TilemapName.BossRoom);
            Texture2D BossTexture = Camera.EntityAssets[EntityType.Boss];
            Point bossTilemapPos = new Point(0, -4);
            float X = bossTilemapPos.X * Tilemap.TILE_SIZE - BossTexture.Width / 2 + Tilemap.TILE_SIZE/2;
            float Y = bossTilemapPos.Y * Tilemap.TILE_SIZE - BossTexture.Height / 2 + Tilemap.TILE_SIZE / 2;
            Boss = new Boss1(BossTexture, X, Y, BossTexture.Width, BossTexture.Height, 0f, 500);
            string message = Message.CreateUpdateBossMessage(Boss.Position, Boss.Health, Boss.MaxHealth);
            Server.SendGlobalMessage(message);
        }

        public static void BossFight()
        {
            Boss.Update();
        }

        public static void UpdateBoss()
        {

        }

        public static void Draw()
        {
            ActiveTilemap.Draw();
            if (Boss != null)
            {
                Boss.Draw();
            }
        }
    }
}
