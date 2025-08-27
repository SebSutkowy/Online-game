using LiteNetLib;
using Microsoft.Xna.Framework;
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
        private bool ShowInteractText = false;
        private int InteractTextOffset = 5;

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

        public void UpdateLocalPlayer()
        {
            int id = Client.GetClientId();
            if (!Players.ContainsKey(id))
                return;

            CameraFollow(id);

            CheckForInteraction(id);
        }

        private void CameraFollow(int id)
        {
            Player player = Players[id];
            Vector2 playerPos = player.Position - Camera.Offset;
            Vector2 targetPos = new Vector2((Camera.WIDTH - player.Size.X) / 2, (Camera.HEIGHT - player.Size.Y) / 2);

            Vector2 TranslationAmount = (playerPos - targetPos) / 30.0f;
            Camera.Move(TranslationAmount);
        }
        private void CheckForInteraction(int id)
        {

            // INTERACTION WITH CHESTS
            Player player = Players[id];
            Point mousePos = Tilemap.GetTilemapPos(Camera.AccountForOffset(InputManager.GetMousePos()));
            Debug.WriteLine($"mouse pos: {mousePos}");
            Point playerPos = Tilemap.GetTilemapPos(player.Center);
            Debug.WriteLine($"player pos: {playerPos}");
            Debug.WriteLine("---");
            Tile tile = Tilemap.GetInteractiveTile(mousePos);

            ShowInteractText = false;
            if (tile == null)
                return;
            if (tile.Type == TileType.Chest && Tilemap.IsANeighbour(playerPos, mousePos))
                ShowInteractText = true;
            if (ShowInteractText && InputManager.ReceivedPressedInput(Input.Interact))
            {
                string message = Message.CreateInteractionMessage(id, Client.GetTick(), mousePos);
                Client.SendMessage(message);
            }
        }
       
        private void CheckTrapInteractions(int playerId)
        {
            Vector2 playerPos = Players[playerId].Center;
            Point tilemapPos = Tilemap.GetTilemapPos(playerPos);
            Tile playerTile = Tilemap.GetInteractiveTile(tilemapPos);

            if (playerTile == null)
                return;
            Debug.WriteLine(playerTile.Type);

            if (playerTile.Type == TileType.Trap)
            {
                Rectangle damageBox = Tilemap.TileToHitbox(tilemapPos);
                int TrapDamage = -5;
                int TrapPeriod = 30;
                int TrapTime = 5 * 60;
                EffectBox box = new EffectBox(damageBox, TrapDamage, TrapPeriod, TrapTime);
                if (!Tilemap.EffectBoxAlreadyThere(box.Position))
                    Tilemap.AddEffectBox(box);  
            }

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
    
            UpdateLocalPlayer();

            UpdatePlayers();
        }

        public Player GetPlayer(int playerId) => Players.ContainsKey(playerId) ? Players[playerId] : null; 

        public StatePayload[] GetPlayerStates(int playerId) => Players[playerId].StateBuffer; 

        public void UpdatePlayers()
        {
            foreach (var (playerId, player) in Players)
            {
                player.Update();
                CheckTrapInteractions(playerId);
                Tilemap.CheckBoxCollisions(playerId);
            }
        }

        public void ChangePlayerHealth(int id, int health)
        {
            Player player = Players[id];
            player.ChangeHealth(health);

            if (Server.IsRunning)
            {
                string message = Message.CreatePlayerHealthChangeMessage(id, player.Health);
                Server.SendGlobalMessage(message);
            }

        }

        public void SetPlayerHealth(int id, int health)
        {
            Player player = Players[id];
            player.SetHealth(health);
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
                player.Draw();
            }
            if (ShowInteractText)
            {
                Point mousePos = InputManager.GetMousePos();
                mousePos.X += InteractTextOffset;
                mousePos.Y += InteractTextOffset;
                Camera.DrawString("Interact", mousePos, Color.White);
            }
        }

    }
}
