using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace Client
{
    class Sprite
    {
        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Rectangle Hitbox { get; set; }
        public bool IsVisible = true;

        public Sprite() { }

        public Sprite(Texture2D _texture, Vector2 _position, Vector2 _velocity)
        {
            Texture = _texture;
            Position = _position;
            Velocity = _velocity;
            Hitbox = new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
        }

        public Sprite(Vector2 _position, int width, int height, Vector2 _velocity)
        {
            Texture = Globals.PixelTexture;
            Debug.WriteLine("Here");
            Position = _position;
            Velocity = _velocity;
            Hitbox = new Rectangle((int)Position.X, (int)Position.Y, width, height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(IsVisible)
                spriteBatch.Draw(Texture, Hitbox, Color.White);
        }
    }
}
