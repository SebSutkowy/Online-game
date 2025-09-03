
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Collections.Generic;

namespace Client
{
    public static class Camera
    {
        public const int WIDTH = 1000;
        public const int HEIGHT = 1000;

        public static Vector2 Offset { get; private set; } = Vector2.Zero;
        public static float Distance { get; private set; } = 1.0f;
        private static float translationFactor;

        public static Action ToDraw = null;

        private static SpriteBatch spriteBatch;
        private static SpriteFont spriteFont;

        public static Dictionary<TileType, Texture2D> TilemapAssets { get; private set; } = new Dictionary<TileType, Texture2D>();
        public static Dictionary<EntityType, Texture2D> EntityAssets { get; private set; } = new Dictionary<EntityType, Texture2D>();

        public static void ImportTextures(ContentManager Content)
        {
            ImportTilemapTextures(Content);
            ImportEntityTextures(Content);
        }

        public static void ImportTilemapTextures(ContentManager Content)
        {
            foreach (TileType tileType in Enum.GetValues(typeof(TileType)))
            {
                string name = $"{tileType.ToString()}Tile";
                if (File.Exists($@"Content\{name}.xnb")) 
                    TilemapAssets[tileType] = Content.Load<Texture2D>(name);
            }
        }

        public static void ImportEntityTextures(ContentManager Content)
        {
            foreach (EntityType entityType in Enum.GetValues(typeof(EntityType)))
            {
                string name = $"{entityType.ToString()}";
                if (File.Exists($@"Content\{name}.xnb")) 
                    EntityAssets[entityType] = Content.Load<Texture2D>(name);
            }
        }

        public static void Zoom(float amount)
        {
            if (Distance + amount == 0.0f)
                return;
            Distance += amount;
        }

        public static Point AccountForOffset(Point point)
        {
            Point newPoint = new Point();
            newPoint.X = (int) (point.X + Offset.X / Distance);
            newPoint.Y =(int) (point.Y + Offset.Y / Distance);
            return newPoint;
        }
        public static Vector2 AccountForOffset(Vector2 point)
        {
            Vector2 newPoint = new Vector2();
            newPoint.X = point.X + Offset.X / Distance;
            newPoint.Y = point.Y + Offset.Y / Distance;
            return newPoint;
        }

        public static void Move(Vector2 direction)
        {
            Vector2 newDirection = new Vector2();
            newDirection.X = (int) (direction.X * Distance); 
            newDirection.Y = (int) (direction.Y * Distance);
            Offset += newDirection;
        }

        public static void AddFont(SpriteFont font)
        {
            spriteFont = font;
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
            newBounds.X = (int)(translationFactor * (bounds.X - Offset.X));
            newBounds.Y = (int)(translationFactor * (bounds.Y - Offset.Y));
            newBounds.Width = (int)(translationFactor * bounds.Width);
            newBounds.Height = (int)(translationFactor * bounds.Height);
            spriteBatch.Draw(texture, newBounds, color);
        }

        public static void DrawString(SpriteFont font, string text, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, text, position, color);
        }

        public static void DrawString(string text, Vector2 position, Color color)
        {
            spriteBatch.DrawString(spriteFont, text, position, color);
        }

        public static void DrawUI(Texture2D texture, Rectangle rect, Color color)
        {
            spriteBatch.Draw(texture, rect, color);
        }

        public static void DrawString(string text, Point position, Color color)
        {
            Vector2 vectorPosition = new Vector2(position.X, position.Y);
            spriteBatch.DrawString(spriteFont, text, vectorPosition, color);
        }
    }
}
