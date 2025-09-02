using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Client
{
    public class ConsoleMessage
    {
        public string Message;
        public ConsoleMessage? NextMessage;
        public int MessageIndex;
    }

    public static class Console
    {
        private static ConsoleMessage FirstMessage;
        private static ConsoleMessage LastMessage;
    
        private static SpriteFont Font;
        private static int messageCount = 0;
        private const int MAX_MESSAGE_COUNT = 50;

        public static void Initialize(SpriteFont font)
        {
            Font = font;
        }

        public static void WriteLine(string message)
        {
            
            messageCount++;
            if (messageCount == 1)
            {
                FirstMessage = new ConsoleMessage()
                {
                    Message = message,
                    NextMessage = null,
                    MessageIndex = messageCount
                };
                LastMessage = FirstMessage;
            }
            else
            {
                LastMessage.NextMessage = new ConsoleMessage()
                {
                    Message = message,
                    NextMessage = null,
                    MessageIndex = messageCount
                };
                LastMessage = LastMessage.NextMessage;
            }
            if (LastMessage.MessageIndex - FirstMessage.MessageIndex > MAX_MESSAGE_COUNT)
                FirstMessage = FirstMessage.NextMessage;
        }

        public static void DisplayConsole()
        {
            int height;
            int yPos = 10;
            ConsoleMessage currentMessage = FirstMessage;
            while (currentMessage != null)
            {
                Vector2 position = new Vector2(10, yPos);
                Camera.DrawString(Font, currentMessage.Message, position, Color.White);
                height = (int)Font.MeasureString(currentMessage.Message).Y;
                yPos += height;
                currentMessage = currentMessage.NextMessage;
            }
        }
    }
}
