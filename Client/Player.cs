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
            switch (MovementDirection) 
            {
                case Direction.Left:
                case Direction.Right:
                    Position = new Vector2(PreviousPosition.X + (int)MovementDirection * (MovementFrame + 1) * Tilemap.TileSize / Speed, Position.Y);
                    MovementFrame = (MovementFrame + 1) % Speed;
                    MovementDirection = (MovementFrame != 0) ? MovementDirection : Direction.None;
                    break;
                case Direction.Up:
                case Direction.Down:
                    Position = new Vector2(Position.X, PreviousPosition.Y + (int)MovementDirection * (MovementFrame + 1) * Tilemap.TileSize / (2*Speed)); // Divide by 2 for vertical correction
                    MovementFrame = (MovementFrame + 1) % Speed;
                    MovementDirection = (MovementFrame != 0) ? MovementDirection : Direction.None;
                    break;
                default:
                    if(!Online)
                        MovementDirection = InputManager.MovementDirection;
                    PreviousPosition = Position;
                    break;
            }
            //Debug.WriteLine($"{Position.X} {Position.Y}");
            
            Hitbox = new Rectangle((int)Position.X, (int)Position.Y, Hitbox.Width, Hitbox.Height);
        }

        public string GetString()
        {
            return $"{Position.X} {Position.Y} {(int)MovementDirection} {Speed}";
            //return $"{Position.X} {Position.Y} 0 0";
        }

        public void ReadString(string info)
        {
            // ignore first and second number (opcode and player id)
            float[] values = Array.ConvertAll(info.Split(' '), float.Parse);
            Position = new Vector2(values[2], values[3]);
            MovementDirection = (Direction)values[4];
            Speed = (int)values[5];
        }
    }
}
