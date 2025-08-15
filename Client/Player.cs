using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
namespace Client
{
    internal class Player
    {
        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public float Speed { get; set; } = 5f;

        public Player(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
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
