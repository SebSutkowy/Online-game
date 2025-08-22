

using System.Collections.Generic;

namespace Client
{
    public enum Scenes
    {
        None,
        SelectNetworkMode
    }

    public static class SceneManager
    {
        public static Scenes PreviousScene { get; private set; }
        public static Scenes CurrentScene { get; private set; }
        private static Dictionary<Scenes, Scene> Scenes = new Dictionary<Scenes, Scene>();

        public static void AddScene(Scenes sceneName, Scene scene)
        {
            Scenes.Add(sceneName, scene);
        }

        public static void SwitchScene(Scenes scene)
        {
            PreviousScene = CurrentScene;
            CurrentScene = scene;
        }



    }

    public class Scene
    {
        
    }
}
