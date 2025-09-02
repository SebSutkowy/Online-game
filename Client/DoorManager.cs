using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Client
{
    // TO DO LATER
    public class Door
    {
        public TilemapName Map { get; set; }
        public Point Position { get; set; }
        public Point SpawnOffset { get; set; }
        public Door LinkedDoor { get; set; }
    }

    internal class DoorManager
    {
        public Dictionary<TilemapName, List<Door>> Doors { get; set; }
        
        public void LinkDoors(int seed)
        {
            Random random = new Random(seed);

            List<Door> currentRoomDoors = Doors[TilemapName.PlayerSpawn];
            Queue<TilemapName> roomQueue = new Queue<TilemapName>();
            List<TilemapName> tilemaps = new List<TilemapName>();
            foreach (TilemapName key in Doors.Keys)
            {
                if (key != TilemapName.PlayerSpawn && key != TilemapName.BossRoom)
                    tilemaps.Add(key);
            }
            int index;
            while(tilemaps.Count > 0)
            {
                index = random.Next(tilemaps.Count);

            }

        }
    }
}
