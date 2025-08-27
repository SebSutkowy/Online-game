using Microsoft.Xna.Framework;

namespace Client
{
    public class StatePayload
    {
        public int Tick;
        public Vector2 Position;

        public Payload ToPayload() => 
            new Payload
            {
                Tick = Tick,
                X = Position.X,
                Y = Position.Y
            };

    }

    public class Payload
    {
        public int Tick { get; set; }
        public float X { get; set; }
        public float Y { get; set; }

    }

    public class InputPayload
    {
        public int Tick;
        public Vector2 Input;

        public string ToString(int id) => $"3 {id} {Tick} {Input.X} {Input.Y}";
    }
}
