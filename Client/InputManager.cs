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
        SpawnPlayer,
        IncreaseLerpConstant,
        DecreaseLerpConstant,
        GetStates,
        SwitchToServer,
        SwitchToClient,
        SwitchToNone,
        SwitchToHybrid,
        DisplayConsole,
        Interact,
        StartBossFight
    }

    public enum InputType
    {
        Key,
        Button
    }

    public enum MouseButton
    {
        Left,
        Middle,
        Right
    }

    class InputBinding
    {
        public InputType InputType;
        public Keys Key;
        public MouseButton Button;

        public InputBinding(Keys key)
        {
            InputType = InputType.Key;
            Key = key;
        }

        public InputBinding(MouseButton button)
        {
            InputType = InputType.Button;
            Button = button;
        }
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

        private static MouseState currentMouseState = new MouseState();
        private static MouseState prevMouseState;

        private static Vector2 InputDirection = Vector2.Zero;
        private static Dictionary<Input, InputBinding> InputBinds = new Dictionary<Input, InputBinding>
        {
            { Input.Up, new InputBinding(Keys.W) },
            { Input.Down, new InputBinding(Keys.S) },
            { Input.Left, new InputBinding(Keys.A) },
            { Input.Right, new InputBinding(Keys.D) },
            { Input.RefreshServer, new InputBinding(Keys.R) },
            { Input.SpawnPlayer, new InputBinding(Keys.Space) },
            { Input.IncreaseLerpConstant, new InputBinding(Keys.Up)  },
            { Input.DecreaseLerpConstant, new InputBinding(Keys.Down) },
            { Input.GetStates, new InputBinding(Keys.Q) },
            { Input.SwitchToServer, new InputBinding(Keys.J) },
            { Input.SwitchToClient, new InputBinding(Keys.K) },
            { Input.SwitchToNone, new InputBinding(Keys.L) },
            { Input.SwitchToHybrid, new InputBinding(Keys.H) },
            { Input.DisplayConsole, new InputBinding(Keys.OemTilde) },
            { Input.Interact, new InputBinding(MouseButton.Left) },
            { Input.StartBossFight, new InputBinding(Keys.Space) }
        };
        private static Dictionary<Input, InputPair> InputValues = new Dictionary<Input, InputPair>();

        public static Point GetMousePos() => currentMouseState.Position;

        public static bool OnPress(Keys key) => (currentKeyboardState.IsKeyDown(key) && !prevKeyboardState.IsKeyDown(key));
        public static bool OnPress(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => currentMouseState.LeftButton == ButtonState.Pressed && prevMouseState.LeftButton != ButtonState.Pressed,
                MouseButton.Middle => currentMouseState.MiddleButton == ButtonState.Pressed && prevMouseState.MiddleButton != ButtonState.Pressed,
                MouseButton.Right => currentMouseState.RightButton == ButtonState.Pressed && prevMouseState.RightButton != ButtonState.Pressed,
                _ => false
            };
        }
        public static bool OnHold(Keys key) => (currentKeyboardState.IsKeyDown(key));
        public static bool OnHold(MouseButton button)
        {
            return button switch
            {
                MouseButton.Left => currentMouseState.LeftButton == ButtonState.Pressed,
                MouseButton.Middle => currentMouseState.MiddleButton == ButtonState.Pressed,
                MouseButton.Right => currentMouseState.RightButton == ButtonState.Pressed,
                _ => false
            };
        }
        public static bool ReceivedPressedInput(Input input) => InputValues[input].PressedInput;
        public static bool ReceivedHeldInput(Input input) => InputValues[input].HeldInput;


        public static void Update()
        {
            prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
         
            prevMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            foreach(var (input, binding) in InputBinds)
            {
                InputPair newPair = new InputPair
                {
                    HeldInput = binding.InputType switch
                    {
                        InputType.Button => OnHold(binding.Button),
                        InputType.Key => OnHold(binding.Key),
                        _ => false
                    },
                    PressedInput = binding.InputType switch
                    {
                        InputType.Button => OnPress(binding.Button),
                        InputType.Key => OnPress(binding.Key),
                        _ => false
                    }
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