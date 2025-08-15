
using System.Numerics;

namespace Server
{
    class Player
    {
        private const int BUFFER_SIZE = 1024;
        public StatePayload[] States = new StatePayload[BUFFER_SIZE];
        public float Speed = 5f;
        public Queue<InputPayload> InputQueue = new Queue<InputPayload>();

        public void UpdatePlayer(StatePayload state)
        {
            int bufferIndex = state.Tick % BUFFER_SIZE;
            States[bufferIndex] = state;
        }

        public void ProcessMovement(InputPayload input)
        {
            
        }
    }
}
