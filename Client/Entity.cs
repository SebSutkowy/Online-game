using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client
{
    enum EntityType
    {
        Player,
        Mob,
        Boss
    }

    class Entity
    {
        public EntityType Type { get; set; }

        public Vector2 Position { get; set; }
        public float X => Position.X;
        public float Y => Position.Y;

        public float Speed { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }
        public Point Size => new Point(Width, Height);

        public Vector2 Center => new Vector2(X + Width / 2, Y + Height / 2);
        public Rectangle Hitbox => new Rectangle((int)X, (int)Y, Width, Height);

        public Texture2D Texture { get; set; }

        public Entity(Texture2D texture, Vector2 position, Point size, float speed)
        {
            Texture = texture;
            Position = position;
            Width = size.X;
            Height = size.Y;
            Speed = speed;
        }

        public Entity(Texture2D texture, float x, float y, int width, int height, float speed)
        {
            Texture = texture;
            Position = new Vector2(X, Y);
            Width = width;
            Height = height;
            Speed = speed;
        }
    }
}
