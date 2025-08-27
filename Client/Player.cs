using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Net.Sockets;
using System.Diagnostics;
using System.Xml.Schema;
using System;

namespace Client
{
    public class Player
    {
        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Center => new Vector2(Position.X + Size.X / 2, Position.Y + Size.Y / 2);
        public StatePayload[] StateBuffer = new StatePayload[Client.BUFFER_SIZE];
        public StatePayload LastPassedState;
        public InputPayload[] InputBuffer = new InputPayload[Client.BUFFER_SIZE];
        public Vector2 Size { get; set; } = new Vector2(100, 100);
        public float Speed { get; set; } = 200f;
        public int Health { get; private set; } = 100;
        public int MaxHealth { get; private set; } = 100;

        public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

        public Player(Texture2D texture)
        { 
            Texture = texture;
            Position = Vector2.Zero;
        }

        public Player(Texture2D texture, Vector2 position, Vector2 size, int bufferSize)
        {
            Texture = texture;
            Position = position;
            Size = size;
            StateBuffer = new StatePayload[bufferSize];
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

            int pastTick = tick - 5;
            int bufferIndex = ((pastTick % Client.BUFFER_SIZE) + Client.BUFFER_SIZE) % Client.BUFFER_SIZE;
            StatePayload state = StateBuffer[bufferIndex];

            if (state != null && state.Tick == tick - 5)
                Position = state.Position;
        }

        public void ChangeHealth(int amount)
        {
            Health = Math.Clamp(Health + amount, 0, MaxHealth);
        }

        public void SetHealth(int health)
        {
            Health = Math.Clamp(health, 0, MaxHealth);
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
            Point pos = new Point(5, 5);
            Camera.Draw(Texture, Hitbox, Color.White);
            Camera.DrawString($"Health: {Health}", pos, Color.White);
        }
    }
}
