using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Client
{
    class Player : Sprite
    {
        public int Speed { get; set; }

        public Player() : base() { }

        public Player(Texture2D _texture, Vector2 _position, int _speed) : base(_texture, _position, Vector2.Zero) 
        {
            Speed = _speed;
        }

        public Player(Vector2 _position, int width, int height, int _speed) : base(_position, width, height, Vector2.Zero) 
        {
            Speed = _speed;
        }

        public Player(Vector2 _position, int width, int height, string info, int _speed) : base(_position, width, height, Vector2.Zero)
        {
            ReadString(info);
            Speed = _speed;
        }

        public void Move()
        {
            if (InputManager.Direction == Vector2.Zero)
                Velocity = Vector2.Zero;
            else
                Velocity = Speed * Vector2.Normalize(InputManager.Direction);
            Position += Velocity;

            Hitbox = new Rectangle((int)Position.X, (int)Position.Y, Hitbox.Width, Hitbox.Height);
        }

        public void MoveOnline()
        {
            Position += Velocity;
            Hitbox = new Rectangle((int)Position.X, (int)Position.Y, Hitbox.Width, Hitbox.Height);
        }

        public string GetString()
        {
            return $"{Position.X} {Position.Y} {Velocity.X} {Velocity.Y}";
        }

        public void ReadString(string info)
        {
            // ignore first and second number (opcode and player id)
            float[] values = Array.ConvertAll(info.Split(' '), float.Parse);
            Position = new Vector2(values[2], values[3]);
            Velocity = new Vector2(values[4], values[5]);
        }
    }
}
