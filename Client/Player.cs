using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Net.Sockets;
using System.Diagnostics;
using System.Xml.Schema;

namespace Client
{
    internal class Player
    {
        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public StatePayload[] StateBuffer = new StatePayload[Client.BUFFER_SIZE];
        public StatePayload LastPassedState;
        public InputPayload[] InputBuffer = new InputPayload[Client.BUFFER_SIZE];
        public Vector2 Size { get; set; } = new Vector2(100, 100);
        public float Speed { get; set; } = 100f;
        public Rectangle Hitbox { get; private set; }

        public Player(Texture2D texture)
        { 
            Texture = texture;
            Position = Vector2.Zero;
            UpdateHitbox();
        }

        public Player(Texture2D texture, Vector2 position, Vector2 size, int bufferSize)
        {
            Texture = texture;
            Position = position;
            Size = size;
            StateBuffer = new StatePayload[bufferSize];
            Hitbox = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        public void ChangeStateBuffer(int tick, StatePayload state)
        {
            Debug.WriteLine("----------------------------------------");
            Debug.WriteLine($"Previous State ({tick % Client.BUFFER_SIZE}): {StateBuffer[tick % Client.BUFFER_SIZE]}");
            StateBuffer[tick % Client.BUFFER_SIZE] = state;
            Debug.WriteLine($"New State ({tick % Client.BUFFER_SIZE}): {state}");
            Debug.WriteLine("----------------------------------------");
        }

        public void Update()
        {
            int tick = Client.GetTick();

            int bufferIndex = (tick - 5) % Client.BUFFER_SIZE;
            StatePayload state = StateBuffer[bufferIndex];

            if (state != null && state.Tick == tick - 5)
                Position = state.Position;
            


            UpdateHitbox();
        }

        private void UpdateHitbox()
        {
            Hitbox = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        // USE PROCESS MOVEMENT LATER --> FOR CLIENT RECONCILIATION
        public StatePayload ProcessMovement(InputPayload input, Mode networkMode)
        {
            if (networkMode == Mode.Server)
            {
                Position = Position + input.Input * Speed * Server.TIME_BETWEEN_TICKS;
                return new StatePayload
                {
                    Tick = input.Tick,
                    Position = Position
                };
            }
            else
            {
                Vector2 newPosition = Position + input.Input;
                return new StatePayload
                {
                    Tick = input.Tick,
                    Position = newPosition
                };
            }
        }

        public void Draw()
        {
            Camera.Draw(Texture, Hitbox, Color.White);
        }
    }
}
