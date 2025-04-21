using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Client
{
    static class InputManager
    {
        public static Vector2 Direction = new Vector2(0, 0);
        
        public static void GetInput()
        {
            Direction = Vector2.Zero;
            if(Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
                Direction.X += -1;
            if (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
                Direction.X += 1;
            if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
                Direction.Y += -1;
            if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
                Direction.Y += 1;
            //Debug.WriteLine($"Direction: {Direction}");
        }
    }
}
