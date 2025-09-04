using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public enum CursorType
    {
        Normal,
        Interact,
        Attack
    }

    static class UI
    {
        private static Dictionary<CursorType, Texture2D> CursorTextures = new Dictionary<CursorType, Texture2D>();
        private static CursorType CursorType = CursorType.Normal;

        private static SpriteFont Font;

        public static bool Interacting { get; private set; } = false;
        public static bool Attacking { get; private set; } = false;

        private static List<string> MessagesToWrite = new List<string>();
        


        public static void ImportTextures(ContentManager Content)
        {
            // CURSOR
            foreach (CursorType cursorType in Enum.GetValues(typeof(CursorType)))
            {
                string name = $"{cursorType.ToString()}Cursor";
                if(File.Exists($@"Content\{name}.xnb"))
                    CursorTextures.Add(cursorType, Content.Load<Texture2D>(name));
            }

            //TEXT
            string fontName = "UI_Font";
            if (File.Exists($@"Content\{fontName}.xnb"))
                Font = Content.Load<SpriteFont>(fontName);
        }

        public static void Update()
        {
            //CURSOR AND TEXT
            Point mousePos = InputManager.GetMousePos();
            MessagesToWrite.Add($"{mousePos}");
            CheckIfInteracting(mousePos);

            Attacking = false;
            mousePos = Camera.AccountForOffset(mousePos);
            foreach (Enemy enemy in Dungeon.Enemies.Values)
            {
                if (enemy.Hitbox.Contains(mousePos))
                    Attacking = true;
            }

            UpdateCursor();
        }

        public static void CheckIfInteracting(Point mousePos)
        {
            Point newMousePos = new Point(mousePos.X, mousePos.Y);
            mousePos = Camera.AccountForOffset(mousePos);
            mousePos = Tilemap.GetTilemapPos(mousePos);
            MessagesToWrite.Add($"{mousePos}");
            MessagesToWrite.Add($"{Camera.Offset}");
            Tile tile = Dungeon.ActiveTilemap.GetInteractiveTile(mousePos);
            if (tile == null)
            {
                Interacting = false;
                UpdateCursor();
                return;
            }

            if ((tile.Tags & TileTags.IsClickable) != 0)
                Interacting = true;
            else
                Interacting = false;
        }


        public static void UpdateCursor()
        {
            CursorType = CursorType.Normal;
            if (Interacting)
                CursorType = CursorType.Interact;
            if (Attacking)
                CursorType = CursorType.Attack;
        }

        public static Vector2 WriteMessage(string message, Vector2 position)
        {
            Vector2 messageLength = Font.MeasureString(message);
            Camera.DrawString(Font, message, position, Color.White);
            return position + messageLength;
        }

        public static Vector2 WriteMultipleMesssages(List<string> messages, Vector2 startingPosition, int padding = 5)
        {
            Vector2 lastPos = startingPosition;
            Vector2 paddingVector = new Vector2(padding);
            foreach (string message in messages)
            {
                lastPos = new Vector2(startingPosition.X, lastPos.Y);
                lastPos = WriteMessage(message, lastPos + paddingVector);
            }
            return lastPos;
        }

        public static void AddMessage(string message)
        {
            MessagesToWrite.Add(message);
        }

        public static void Draw()
        {
            //TEXT
            WriteMultipleMesssages(MessagesToWrite, new Vector2(Camera.WIDTH - 110, 0));

            //CURSOR
            if (!CursorTextures.ContainsKey(CursorType))
                return;
            Texture2D mouseTexture = CursorTextures[CursorType];
            Point mousePos = InputManager.GetMousePos();
            Point mouseSize = new Point(mouseTexture.Width, mouseTexture.Height);
            Rectangle mouseRect = new Rectangle(mousePos, mouseSize);
            Camera.DrawUI(CursorTextures[CursorType], mouseRect, Color.White);
            MessagesToWrite.Clear();
        }
    }
}
