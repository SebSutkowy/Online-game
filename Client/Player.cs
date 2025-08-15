using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace Client
{
    public enum Direction
    {
        None = 0,
        Left = -1,
        Up = -2,
        Right = 1,
        Down = 2 // given Values of 1 for left and right and values of 2 for up and down. Sign tells the direction.
            // Need to divide by two for vertical directions
    }

    class Player : Sprite
    {
        public bool Online { get; set; }
        public int Speed { get; set; }
        public int MovementFrame = 0;
        public Vector2 PreviousPosition { get; set; }
        public Direction MovementDirection { get; set; }

        public int Health { get; set; }
        public int MaxHealth { get; set; }

        public Player() : base() { }

        public Player(Texture2D _texture, int maxHealth, Vector2 _position, int _speed) : base(_texture, _position, Vector2.Zero) 
        {
            MaxHealth = maxHealth;
            Health = maxHealth;
            Speed = _speed;
        }

        public Player(Vector2 _position, int maxHealth, int width, int height, int _speed) : base(_position, width, height, Vector2.Zero) 
        {
            MaxHealth = maxHealth;
            Health = maxHealth;
            Speed = _speed;
        }

        public Player(Vector2 _position, int maxHealth, int width, int height, string info, int _speed) : base(_position, width, height, Vector2.Zero)
        {
            ReadString(info);
            MaxHealth = maxHealth;
            Health = maxHealth;
            Speed = _speed;
        }

        public void Update()
        {
            Move();

            Hitbox = new Rectangle((int)Position.X, (int)Position.Y, Hitbox.Width, Hitbox.Height);
        }

        public void Move()
        {
            if(Online)
            {
                MoveOnline();
                return;
            }
            Position += InputManager.MovementDirection * Speed;
            

        }

        public void MoveOnline()
        {
            //Position = 
        }

        public void TakeDamage(int damage)
        {
            Health = Math.Max(Health - damage, 0);
        }

        public void Heal(int healAmount)
        {
            Health = Math.Min(Health + healAmount, MaxHealth);
        }

        public string GetString()
        {
            //return $"{Position.X} {Position.Y} {VelX} {VelY}";
            return $"{Position.X} {Position.Y} 0 0";
        }

        public void ReadString(string info)
        {
            // ignore first and second number (opcode and player id)
            float[] values = Array.ConvertAll(info.Split(' '), float.Parse);
            Position = new Vector2(values[2], values[3]);
            //MovementDirection = (Direction)values[4];
            //Speed = (int)values[5];
        }
    }
}
