using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace Client
{
    static class PlayerManager
    {
        private static Dictionary<int, Player> Players = new Dictionary<int, Player>();
        private static Texture2D BlankTexture { get; set; }

        private static float LerpConstant = 0.5f;

        public static void setBlankTexture(Texture2D blankTexture)
        {
            BlankTexture = blankTexture;
        }

        public static void Remove(int id)
        {
            Players.Remove(id);
        }

        public static float GetLerpConstant() => LerpConstant;

        public static void UpdatePlayer(int id)
        {
            CheckIfPlayerExists(id);

            Players[id].UpdatePlayer();
        }

        public static void Update()
        {
            if (InputManager.ReceivedPressedInput(Input.IncreaseLerpConstant))
                LerpConstant = Math.Min(LerpConstant + 0.01f, 1f);
            if (InputManager.ReceivedPressedInput(Input.DecreaseLerpConstant))
                LerpConstant = Math.Max(LerpConstant - 0.01f, 0f);
            Debug.WriteLine(LerpConstant);

            UpdatePlayers();
        }

        public static void UpdatePlayers()
        {
            foreach (Player player in Players.Values)
                player.UpdatePlayer();
        }

        public static void CheckIfPlayerExists(int id)
        {
            if (!Players.ContainsKey(id))
                Players.Add(id, new Player(BlankTexture));
        }

        public static void SetPlayerState(int id, StatePayload state)
        {
            CheckIfPlayerExists(id);

            int bufferIndex = state.Tick % Server.BUFFER_SIZE;
            Player player = Players[id];
            player.StateBuffer[bufferIndex] = state;
            player.Position = state.Position;
        }

        public static void SetTargetState(int id, StatePayload targetState)
        {
            CheckIfPlayerExists(id);

            Player player = Players[id];
            if (player.TargetState != null)
                player.Position = player.TargetState.Position;
            Players[id].TargetState = targetState;
        }

        public static void AddInput(int id, InputPayload Input)
        {
            CheckIfPlayerExists(id);

            int bufferIndex = Input.Tick % Server.BUFFER_SIZE;
            Players[id].InputBuffer[bufferIndex] = Input;
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
