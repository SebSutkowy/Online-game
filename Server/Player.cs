
using System.Numerics;

namespace Server
{
    class Player
    {
        private const int BUFFER_SIZE = 1024;
        public Vector2 Position = Vector2.Zero;
        public StatePayload[] StateBuffer = new StatePayload[BUFFER_SIZE];
        public float Speed = 5f;
        public Queue<InputPayload> InputQueue = new Queue<InputPayload>();

        public void UpdatePlayer(StatePayload state)
        {
            int bufferIndex = state.Tick % BUFFER_SIZE;
            StateBuffer[bufferIndex] = state;
        }

        public StatePayload ProcessMovement(InputPayload input)
        {
            Position += input.Input * Speed * Server.minTimeBetweenTicks;

            StatePayload state = new StatePayload
            {
                Tick = input.Tick,
                Position = Vector2.Zero
            };
            return state;
        }
    }
}
