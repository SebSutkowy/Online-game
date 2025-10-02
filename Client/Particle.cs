using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client
{
    class Particle
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Point Size { get; set; }
        public Color Color { get; set; }
        public int Lifespan { get; set; }
        public Rectangle bounds => new Rectangle((int)Position.X, (int)Position.Y, Size.X, Size.Y);

        public Particle(Vector2 position,  Vector2 velocity, Point size, Color color, int lifespan)
        {
            Position = position;
            Velocity = velocity;
            Size = size;
            Color = color;
            Lifespan = lifespan;
        }

        public void Update()
        {
            Lifespan--;
            Position += Velocity;
        }

        public void Draw(Texture2D texture)
        {
            Camera.Draw(texture, bounds, Color);
        }
    }
}
