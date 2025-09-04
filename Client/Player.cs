using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Net.Sockets;
using System.Diagnostics;
using System.Xml.Schema;
using System;
using System.Collections.Generic;

namespace Client
{
    class Player : Entity
    {
        public int Health { get; private set; } = 100;
        public int MaxHealth { get; private set; } = 100;
        public int Damage { get; set; } = 50;
        public Dictionary<Point, int> EffectBoxTimers;

        public Player(Texture2D texture, Vector2 position, Point size, float speed) : base(texture, position, size, speed)
        { 
            EffectBoxTimers = new Dictionary<Point, int>();
        }

        //public void ChangeStateBuffer(int tick, StatePayload state)
        //{
        //    Debug.WriteLine("----------------------------------------");
        //    Debug.WriteLine($"Previous State ({tick % Client.BUFFER_SIZE}): {StateBuffer[tick % Client.BUFFER_SIZE]}");
        //    StateBuffer[tick % Client.BUFFER_SIZE] = state;
        //    Debug.WriteLine($"New State ({tick % Client.BUFFER_SIZE}): {state}");
        //    Debug.WriteLine("----------------------------------------");
        //}

        public void Update()
        {
            //int tick = Client.GetTick();

            //int pastTick = tick - 5;
            //int bufferIndex = ((pastTick % Client.BUFFER_SIZE) + Client.BUFFER_SIZE) % Client.BUFFER_SIZE;
            //StatePayload state = StateBuffer[bufferIndex];

            //if (state != null && state.Tick == tick - 5)
            //    Position = state.Position;
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
        public StatePayload ProcessMovement(InputPayload input)
        {
            switch (NetworkManager.GetMode())
            {
                case Mode.Server:
                    Position = Position + Speed * input.Input * Server.TIME_BETWEEN_TICKS;
                    //Debug.WriteLine(Position);
                    return new StatePayload()
                    {
                        Tick = input.Tick,
                        Position = Position
                    };
                case Mode.Client:
                    return new StatePayload()
                    {
                        Tick = input.Tick,
                        Position = Position + Speed * input.Input * Client.TIME_BETWEEN_TICKS
                    };
                default:
                    return null;
            }
        }

        public void Draw()
        {
            Camera.Draw(Texture, Hitbox, Color.White);
        }
    }
}
