using System.Numerics;

namespace Server
{
    class StatePayload
    {
        public int Tick;
        public Vector2 Position;
    }

    class InputPayload
    {
        public int Tick;
        public Vector2 Input;
    }
}
