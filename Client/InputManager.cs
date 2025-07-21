using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Client
{
    static class InputManager
    {
        public static Direction MovementDirection = Direction.None;
        
        public static void GetInput()
        {
            MovementDirection = Direction.None;
            if(Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
                MovementDirection = Direction.Left;
            if (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
                MovementDirection = Direction.Right;
            if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
                MovementDirection = Direction.Up;
            if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
                MovementDirection = Direction.Down;
            //Debug.WriteLine($"Direction: {Direction}");
        }
    }
}
