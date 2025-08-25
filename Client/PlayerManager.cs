using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Client
{
    class PlayerManager
    {
        private Dictionary<int, Player> Players;
        private Dictionary<int, Queue<InputPayload>> InputQueue;
        private Texture2D BlankTexture;

        private float LerpConstant = 0.5f;

        public PlayerManager(Texture2D texture)
        {
            Players = new Dictionary<int, Player>();
            InputQueue = new Dictionary<int, Queue<InputPayload>>();
            BlankTexture = texture;
        }

        
        public void Remove(int id)
        {
            Players.Remove(id);
            InputQueue.Remove(id);
        }

        public bool Contains(int id) => Players.ContainsKey(id);

        public float GetLerpConstant() => LerpConstant;

        public void UpdatePlayer(int id)
        {
            CheckIfPlayerExists(id);

            Players[id].Update();
        }

        public void CreatePlayer(int playerId)
        {
            Players.Add(playerId, new Player(BlankTexture));
            InputQueue.Add(playerId, new Queue<InputPayload>());
            string message = Message.CreatePlayerSpawnMessage(playerId, Server.GetTick(), Players[playerId].Position);
            Server.SendGlobalMessage(message);

        }

        public void Update()
        {
            if (InputManager.ReceivedPressedInput(Input.SpawnPlayer) && !Players.ContainsKey(Client.GetClientId()))
            {
                string message = Message.CreatePlayerSpawnRequestMessage(Client.GetClientId());
                Client.SendMessage(message);
            }

            if (InputManager.ReceivedPressedInput(Input.IncreaseLerpConstant))
                LerpConstant = Math.Min(LerpConstant + 0.01f, 1f);
            if (InputManager.ReceivedPressedInput(Input.DecreaseLerpConstant))
                LerpConstant = Math.Max(LerpConstant - 0.01f, 0f);
            //Debug.WriteLine(LerpConstant);

            UpdatePlayers();
        }

        public StatePayload[] GetPlayerStates(int playerId) => Players[playerId].StateBuffer; 

        public void UpdatePlayers()
        {
            foreach (Player player in Players.Values)
                player.Update();
        }

        public void CheckIfPlayerExists(int id)
        {
            if (!Players.ContainsKey(id))
                Players.Add(id, new Player(BlankTexture));
            if(!InputQueue.ContainsKey(id))
                InputQueue.Add(id, new Queue<InputPayload>());
        }

        public void SetPlayerState(int id, StatePayload state)
        {
            CheckIfPlayerExists(id);

            int bufferIndex = state.Tick % Client.BUFFER_SIZE;
            Player player = Players[id];
            player.StateBuffer[bufferIndex] = state;
            if (player.LastPassedState == null || state.Tick >= player.LastPassedState.Tick)
                player.LastPassedState = state;
        }

        public void AddInput(int id, InputPayload Input)
        {
            CheckIfPlayerExists(id);

            int bufferIndex = Input.Tick % Client.BUFFER_SIZE;
            Players[id].InputBuffer[bufferIndex] = Input;
            InputQueue[id].Enqueue(Input);
        }

        public void ProcessPlayerMovement(Mode networkMode)
        {
            foreach (var (playerId, player) in Players)
            {
                int bufferIndex = -1;
                while (InputQueue[playerId].Count > 0)
                {
                    InputPayload input = InputQueue[playerId].Dequeue();

                    bufferIndex = input.Tick % Server.BUFFER_SIZE;

                    StatePayload state = player.ProcessMovement(input, networkMode); ;
                    if (networkMode == Mode.Server)
                    {
                        SetPlayerState(playerId, state);
                        string message = Message.CreatePlayerStateMessage(playerId, state);
                        Server.SendGlobalMessage(message);
                    }
                }
            }
        }

        public void SendPlayerStates(int recipientId)
        {
            foreach (var (playerId, player) in Players)
            {
                StatePayload state = new StatePayload
                {
                    Tick = Server.GetTick(),
                    Position = player.Position
                };
                string message = Message.CreatePlayerStateMessage(playerId, state);
                Server.SendMessage(recipientId, message);
            }
        }

        public void DrawPlayers()
        {
            foreach(Player player in Players.Values)
            {
                Camera.ToDraw += () => player.Draw();
            }
        }

    }
}
