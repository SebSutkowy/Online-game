using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Client
{
    internal class Player
    {
        private const int BUFFER_SIZE = 1024;

        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public StatePayload[] StateBuffer = new StatePayload[Server.BUFFER_SIZE];
        public InputPayload[] InputBuffer = new InputPayload[Server.BUFFER_SIZE];
        public Vector2 Size { get; set; } = new Vector2(100, 100);
        public float Speed { get; set; } = 5f;
        public Rectangle Hitbox { get; private set; }

        public Player(Texture2D texture)
        { 
            Texture = texture;
        }

        public Player(Texture2D texture, Vector2 position, Vector2 size, int bufferSize)
        {
            Texture = texture;
            Position = position;
            Size = size;
            StateBuffer = new StatePayload[bufferSize];
            Hitbox = new Rectangle((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);
        }

        public void UpdatePlayer(StatePayload state)
        {
            int bufferIndex = state.Tick % BUFFER_SIZE;
            StateBuffer[bufferIndex] = state;
            Position = state.Position;
        }

        // USE PROCESS MOVEMENT LATER --> FOR CLIENT RECONCILIATION
        public StatePayload ProcessMovement(InputPayload input)
        {
            Vector2 newPosition = Position + input.Input;
            return new StatePayload
            {
                Tick = input.Tick,
                Position = newPosition
            };
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Hitbox, Color.White);
        }
    }
}
