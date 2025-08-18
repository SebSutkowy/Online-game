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
        RefreshServer,
        IncreaseLerpConstant,
        DecreaseLerpConstant
    }

    class InputPair
    {
        public bool HeldInput;
        public bool PressedInput;
    }

    static class InputManager
    {
        private static KeyboardState currentKeyboardState = new KeyboardState();
        private static KeyboardState prevKeyboardState;

        private static Vector2 InputDirection = Vector2.Zero;
        private static Dictionary<Input, Keys> InputKeys = new Dictionary<Input, Keys>
        {
            { Input.Up, Keys.W },
            { Input.Down, Keys.S },
            { Input.Left, Keys.A },
            { Input.Right, Keys.D },
            { Input.RefreshServer, Keys.R },
            { Input.IncreaseLerpConstant, Keys.Up },
            { Input.DecreaseLerpConstant, Keys.Down }
        };
        private static Dictionary<Input, InputPair> InputValues = new Dictionary<Input, InputPair>();

        public static bool OnPress(Keys key) => (currentKeyboardState.IsKeyDown(key) && !prevKeyboardState.IsKeyDown(key));
        public static bool OnHold(Keys key) => (currentKeyboardState.IsKeyDown(key));
        public static bool ReceivedPressedInput(Input input) => InputValues[input].PressedInput;
        public static bool ReceivedHeldInput(Input input) => InputValues[input].HeldInput;


        public static void Update()
        {
            prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            foreach(var (input, key) in InputKeys)
            {
                InputPair newPair = new InputPair
                {
                    HeldInput = OnHold(key),
                    PressedInput = OnPress(key)
                };
                if (!InputValues.ContainsKey(input))
                    InputValues.Add(input, newPair);
                else
                    InputValues[input] = newPair;
            }

            InputDirection = Vector2.Zero;

            if (ReceivedHeldInput(Input.Left))
                InputDirection.X--;
            if (ReceivedHeldInput(Input.Right))
                InputDirection.X++;
            if (ReceivedHeldInput(Input.Up))
                InputDirection.Y--;
            if (ReceivedHeldInput(Input.Down))
                InputDirection.Y++;

            if (InputDirection != Vector2.Zero)
                InputDirection.Normalize();
        }

        public static Vector2 GetInput() => InputDirection;

    }
}
