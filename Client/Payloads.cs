using Microsoft.Xna.Framework;

namespace Client
{
    class StatePayload
    {
        public int Tick;
        public Vector2 Position;

        public string ToString(int id) => $"2 {id} {Tick} {Position.X} {Position.Y}";

    }

    class InputPayload
    {
        public int Tick;
        public Vector2 Input;

        public string ToString(int id) => $"3 {id} {Tick} {Input.X} {Input.Y}";
    }
}
