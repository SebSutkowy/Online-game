
using System.Numerics;

namespace Server
{
    class Player
    {
        public Vector2 Position = Vector2.Zero;
        public StatePayload[] StateBuffer = new StatePayload[Server.BUFFER_SIZE];
        public float Speed = 300f;
        public Queue<InputPayload> InputQueue = new Queue<InputPayload>();

        public void UpdatePlayer(StatePayload state)
        {
            int bufferIndex = state.Tick % Server.BUFFER_SIZE;
            StateBuffer[bufferIndex] = state;
        }

        public StatePayload ProcessMovement(InputPayload input)
        {
            Position += input.Input * Speed * Server.minTimeBetweenTicks;

            StatePayload state = new StatePayload
            {
                Tick = input.Tick,
                Position = Position
            };
            return state;
        }
    }
}
