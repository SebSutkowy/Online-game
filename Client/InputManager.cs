using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Client
{
    enum Input
    {
        Up,
        Down,
        Left,
        Right,
        RefreshServer
    }

    static class InputManager
    {
        public static KeyboardState currentKeyboardState = new KeyboardState();
        public static KeyboardState prevKeyboardState;

        public static Vector2 InputDirection = Vector2.Zero;
        public static Dictionary<Input, Keys> InputKeys = new Dictionary<Input, Keys>
        {
            { Input.Up, Keys.W },
            { Input.Down, Keys.S },
            { Input.Left, Keys.A },
            { Input.Right, Keys.D },
            { Input.RefreshServer, Keys.R }
        };
        public static Dictionary<Input, bool> InputValues = new Dictionary<Input, bool>();

        public static bool OnPress(Keys key) => (currentKeyboardState.IsKeyDown(key) && !prevKeyboardState.IsKeyDown(key));
        public static bool OnHold(Keys key) => (currentKeyboardState.IsKeyDown(key));
        public static bool IsInputPresent(Input input) => InputValues[input];


        public static void Update()
        {
            prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            foreach(var (input, key) in InputKeys)
            {
                if(!InputValues.ContainsKey(input))
                    InputValues.Add(input, OnPress(key));
                else
                    InputValues[input] = OnPress(key);
            }

            if (InputValues[Input.Left])
                InputDirection.X--;
            if (InputValues[Input.Right])
                InputDirection.X++;
            if (InputValues[Input.Up])
                InputDirection.Y--;
            if (InputValues[Input.Down])
                InputDirection.Y++;

            if (InputDirection != Vector2.Zero)
                InputDirection.Normalize();
        }

    }
}
