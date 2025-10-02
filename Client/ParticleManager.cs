using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Client
{
    static class ParticleManager
    {
        private static List<Particle> Particles = new List<Particle>();
        private static Texture2D Texture;

        public static void LoadContent(GraphicsDevice graphics)
        {
            Texture = new Texture2D(graphics, 1, 1);
            Texture.SetData(new[] { Color.White });
        }

        public static void CreateRandomParticles(Vector2 Position, Point minSize, Point maxSize, Vector2 minVel, Vector2 maxVel, Color minCol, Color maxCol, int lifespan)
        {
            Random rand = new Random();
            int colR, colG, colB;
            if (maxCol.R == maxCol.R) colR = maxCol.R;
            else colR = rand.Next() % (maxCol.R - minCol.R) + minCol.R;
            if (maxCol.G == minCol.G) colG = minCol.G;
            else colG = rand.Next() % (maxCol.G - minCol.G) + minCol.G;
            if (maxCol.B == minCol.B) colB = maxCol.B;
            else colB = rand.Next() % (maxCol.B - minCol.B) + minCol.B;
            Color col = new Color(colR, colG, colB);
            float velX, velY;
            if (maxVel.X == minVel.X) velX = maxVel.X;
            else velX = (float)rand.NextDouble() * (maxVel.X - minVel.X) + minVel.X;
            if(maxVel .Y == minVel.Y) velY = maxVel.Y;
            else velY = (float)rand.NextDouble() * (maxVel.Y - minVel.Y) + minVel.Y;
            Vector2 vel = new Vector2(velX, velY);
            int width, height;
            if(maxSize.X == minSize.X) width = maxSize.X;
            else width = rand.Next() % (maxSize.X - minSize.X) + minSize.X;
            if (maxSize .Y == minSize.Y) height = maxSize.Y;
            else height = rand.Next() % (maxSize.Y - minSize.Y) + minSize.Y;
            Point size = new Point(width, height);
            Particle p = new Particle(Position, vel, size, col, lifespan);
            Particles.Add(p);
        }


        public static void Update()
        {
            Stack<Particle> toRemove =new Stack<Particle>();
            foreach (Particle p in Particles)
            {
                p.Update();
                if (p.Lifespan <= 0)
                {
                    toRemove.Push(p);
                }
            }

            while (toRemove.Count > 0)
            {
                Particles.Remove(toRemove.Pop());
            }
        }

        public static void Draw()
        {
            foreach (Particle p in Particles)
            {
                p.Draw(Texture);
            }
        }


    }
}
