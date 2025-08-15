using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace Client
{
    internal class Player
    {
        private const int BUFFER_SIZE = 1024;

        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public StatePayload[] StateBuffer = new StatePayload[BUFFER_SIZE];
        public Vector2 Size { get; set; } = new Vector2(100, 100);
        public float Speed { get; set; } = 5f;

        public Player()
        { }

        public Player(Vector2 position, Vector2 size, int bufferSize)
        {
            Position = position;
            Size = size;
            StateBuffer = new StatePayload[bufferSize];
        }

        public void UpdatePlayer(StatePayload state)
        {
            int bufferIndex = state.Tick % BUFFER_SIZE;
            StateBuffer[bufferIndex] = state;
            Position = state.Position;
        }

        public StatePayload ProcessMovement(InputPayload input)
        {
            Vector2 newPosition = Position + input.Input;
            return new StatePayload
            {
                Tick = input.Tick,
                Position = newPosition
            };
        }

    }
}
