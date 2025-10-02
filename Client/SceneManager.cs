

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Diagnostics;

namespace Client
{
    public enum SceneName
    {
        SelectNetworkMode,
        Game
    }

    public class UIFeature
    {
        public Point Position { get; set; }
        public Point Size { get; set; }
        public Color Color { get; set; }
        public int Width => Size.X;
        public int Height => Size.Y;

        public int Left => Position.X;
        public int Top => Position.Y;
        public int Right => Left + Width;
        public int Bottom => Top + Height;


        public Rectangle Rect => new Rectangle(Position, Size);
        public bool IsColliding(Point point) => Rect.Contains(point);

        //public bool IsColliding(Point point)
        //{
        //    Debug.WriteLine(point);
        //    Debug.WriteLine(Rect);
        //    Debug.WriteLine(Rect.Contains(point));
        //    return Rect.Contains(point);
        //}


        public UIFeature(Point pos, Point size, Color color)
        {
            Position = pos;
            Size = size;
            Color = color;
        }

        public void Draw()
        {
            Camera.Draw(Rect, Color);
        }
    }

    public class Scene
    {
        public SceneName SceneName { get; set; }
        public List<UIFeature> Features { get; set; }

        public Scene(List<UIFeature> features)
        {
            Features = features;
        }

        public Scene()
        {
            Features = new List<UIFeature>();
        }

        public void AddUIFeature()
        { }

        public delegate void UpdateHandler();
        public event UpdateHandler OnUpdate;
        public void Update()
        {
            OnUpdate?.Invoke();
        }

        public delegate void DrawHandler();
        public event DrawHandler OnDraw;
        public void Draw()
        {
            foreach (UIFeature feature in Features)
            {
                feature.Draw();
            }
            OnDraw?.Invoke();
        }
    }

    public static class SceneManager
    {
        public static Dictionary<SceneName, Scene> Scenes = new Dictionary<SceneName, Scene>();
        public static SceneName currentScene;
        public static Stack<SceneName> SceneStack = new Stack<SceneName>();
        private static GameTime _gameTime;

        /* -- Giving Scenes Functions -- */
        public static void Init()
        {
            currentScene = SceneName.SelectNetworkMode;
            List<UIFeature> features;

            /*
             * Dungeon.Update();
             * NetworkManager.Update(gameTime);
             * Dungeon.Draw();
             * NetworkManager.Draw();
             * UI.Draw();
             */

            /* --- selection scene --- */
            Point posClient, posServer, size;
            posClient = new Point(200, 450);
            posServer = new Point(700, 450);
            size = new Point(100, 50);
            UIFeature clientButton = new UIFeature(posClient, size, Color.Blue);
            UIFeature serverButton = new UIFeature(posServer, size, Color.Blue);
            int indexC = 0, indexS = 1;

            features = new List<UIFeature>()
            {
                clientButton,
                serverButton
            };

            Scenes.Add(SceneName.SelectNetworkMode, new Scene(features));
            Scenes[SceneName.SelectNetworkMode].OnUpdate += () =>
            {
                UI.UpdateCursor();
                if (Scenes[SceneName.SelectNetworkMode].Features[indexC].IsColliding(InputManager.GetMousePos())) // Client
                {
                    if (InputManager.ReceivedPressedInput(Input.Interact))
                    {
                        NetworkManager.ChangeNetworkMode(Mode.Client);
                        SwitchScene(SceneName.Game);
                    }
                    else
                        Scenes[SceneName.SelectNetworkMode].Features[indexC].Color = Color.Red;
                }
                else
                    Scenes[SceneName.SelectNetworkMode].Features[indexC].Color = Color.Blue;

                if (Scenes[SceneName.SelectNetworkMode].Features[indexS].IsColliding(InputManager.GetMousePos())) // Server
                {
                    if (InputManager.ReceivedPressedInput(Input.Interact))
                    {
                        NetworkManager.ChangeNetworkMode(Mode.Server);
                        SwitchScene(SceneName.Game);
                    }
                    else
                        Scenes[SceneName.SelectNetworkMode].Features[indexS].Color = Color.Red;
                }
                else
                    Scenes[SceneName.SelectNetworkMode].Features[indexS].Color = Color.Blue;
            };

            Scenes[SceneName.SelectNetworkMode].OnDraw += () =>
            {
                Camera.DrawString("Client mode", posClient, Color.White);
                Camera.DrawString("Server mode", posServer, Color.White);

                UI.Draw();
            };


            /* --- Game scene --- */
            Scenes.Add(SceneName.Game, new Scene());
            Scenes[SceneName.Game].OnUpdate += () =>
            {
                Dungeon.Update();
                NetworkManager.Update(_gameTime);
                UI.Update();

                if (InputManager.ReceivedPressedInput(Input.GoBack))
                    GoBackScene();
            };

            Scenes[SceneName.Game].OnDraw += () =>
            {
                Dungeon.Draw();
                NetworkManager.Draw();
                ParticleManager.Draw();
                UI.Draw();
            };
        }

        public static Scene activeScene => Scenes[currentScene];

        public static void SwitchScene(SceneName scene)
        {
            SceneStack.Push(currentScene);
            currentScene = scene;
        }

        public static void GoBackScene()
        {
            if (SceneStack.Count <= 0) return;
            currentScene = SceneStack.Pop();
        }

        public static void Update(GameTime gameTime)
        {
            _gameTime = gameTime;
            activeScene.Update();
        }

        public static void Draw()
        {
            activeScene.Draw();
        }

    }
}
