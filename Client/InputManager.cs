using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;

namespace Client
{
    static class InputManager
    {
        public static Vector2 MovementDirection = Vector2.Zero;
        
        public static void GetInput()
        {
            MovementDirection = Vector2.Zero;
            if (Keyboard.GetState().IsKeyDown(Keys.A) || Keyboard.GetState().IsKeyDown(Keys.Left))
                MovementDirection.X--;
            if (Keyboard.GetState().IsKeyDown(Keys.D) || Keyboard.GetState().IsKeyDown(Keys.Right))
                MovementDirection.X++;
            if (Keyboard.GetState().IsKeyDown(Keys.W) || Keyboard.GetState().IsKeyDown(Keys.Up))
                MovementDirection.Y--; ;
            if (Keyboard.GetState().IsKeyDown(Keys.S) || Keyboard.GetState().IsKeyDown(Keys.Down))
                MovementDirection.Y++;
            if (MovementDirection != Vector2.Zero)
                MovementDirection.Normalize();
            Debug.WriteLine(MovementDirection);
        }

        public static void Normalize()
        {

        }
    }
}
