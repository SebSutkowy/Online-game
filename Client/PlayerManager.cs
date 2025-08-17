using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Client
{
    static class PlayerManager
    {
        private static Dictionary<int, Player> Players = new Dictionary<int, Player>();
        private static Texture2D BlankTexture { get; set; }

        public static void setBlankTexture(Texture2D blankTexture)
        {
            BlankTexture = blankTexture;
        }

        public static void Remove(int id)
        {
            Players.Remove(id);
        }

        public static void UpdatePlayer(int Id, StatePayload statePayload)
        {
            if (!Players.ContainsKey(Id))
                Players.Add(Id, new Player(BlankTexture));
            Players[Id].UpdatePlayer(statePayload);
        }

        public static void AddInput(int Id, InputPayload Input)
        {
            int bufferIndex = Input.Tick % Server.BUFFER_SIZE;
            Players[Id].InputBuffer[bufferIndex] = Input;
        }
        
        public static void DrawPlayers(SpriteBatch spriteBatch)
        {
            foreach(Player player in Players.Values)
            {
                player.Draw(spriteBatch);
            }
        }

    }
}
