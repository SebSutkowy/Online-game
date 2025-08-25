
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Client
{
    public static class Camera
    {
        public static Point Offset { get; private set; } = Point.Zero;

        public static float Distance { get; private set; } = 1.0f;
        private static float translationFactor;

        public static Action ToDraw = null;

        private static SpriteBatch spriteBatch;

        public static void Zoom(float amount)
        {
            if (Distance + amount == 0.0f)
                return;
            Distance += amount;
        }


        public static void Move(Point direction)
        {
            Point newDirection = new Point();
            newDirection.X = (int) (direction.X * Distance); 
            newDirection.Y = (int) (direction.Y * Distance);
            Offset -= newDirection;
        }

        public static void Display(SpriteBatch _spriteBatch)
        {
            spriteBatch = _spriteBatch;
            _spriteBatch.Begin();

            ToDraw?.Invoke();

            _spriteBatch.End();
            ToDraw = null;
        }

        private static float GetTranslationFactor() => 1 / Distance;

        public static void Draw(Texture2D texture, Rectangle bounds, Color color)
        {
            translationFactor = GetTranslationFactor();
            Rectangle newBounds = new Rectangle();
            newBounds.X = (int)(translationFactor * (Offset.X + bounds.X));
            newBounds.Y = (int)(translationFactor * (Offset.Y + bounds.Y));
            newBounds.Width = (int)(translationFactor * bounds.Width);
            newBounds.Height = (int)(translationFactor * bounds.Height);
            spriteBatch.Draw(texture, newBounds, color);
        }

        public static void DrawString(SpriteFont font, string text, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, text, position, color);
        }
    }
}
