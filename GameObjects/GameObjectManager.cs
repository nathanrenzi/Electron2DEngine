using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenGLTest.GameObjects
{
    public static class GameObjectManager
    {
        public static List<GameObject> gameObjects = new List<GameObject>();

        public static void RegisterGameObject(GameObject _obj)
        {
            if (gameObjects.Contains(_obj)) return;

            gameObjects.Add(_obj);
        }

        public static void UnregisterGameObject(GameObject _obj)
        {
            if (!gameObjects.Contains(_obj)) return;

            gameObjects.Remove(_obj);
        }

        public static void UpdateGameObjects()
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Update();
            }
        }

        public static void RenderGameObjects()
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Render();
            }
        }

        public static void StartGameObjects()
        {
            for (int i = 0; i < gameObjects.Count; i++)
            {
                gameObjects[i].Start();
            }
        }
    }
}
