using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Client
{
    public static class Console
    {
        private static List<string> Messages = new List<string>();
        private static SpriteFont Font;
        private static int SCREEN_WIDTH;
        private static int SCREEN_HEIGHT;

        public static void Initialize(SpriteFont font, Point dimensions)
        {
            Font = font;
            SCREEN_HEIGHT = dimensions.Y;
            SCREEN_WIDTH = dimensions.X;
        }

        public static void WriteLine(string message)
        {
            Messages.Add(message);
        }

        public static void DisplayConsole(SpriteBatch spriteBatch)
        {
            int height = (int)Font.MeasureString(Messages[0]).Y;
            int yPos;
            for (int i = Messages.Count - 1; i >= 0; i--)
            {
                string message = Messages[i];
                yPos = SCREEN_HEIGHT - (Messages.Count - i) * height; 
                Vector2 Position = new Vector2(10, yPos);
                spriteBatch.DrawString(Font, message, Position, Color.White);
            }
        }
    }
}
