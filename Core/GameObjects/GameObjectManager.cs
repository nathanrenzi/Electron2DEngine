using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electron2D.Core.GameObjects
{
    public static class GameObjectManager
    {
        public static List<GameObject> gameObjects = new List<GameObject>();
        private static bool hasStarted = false;
        public static void RegisterGameObject(GameObject _obj)
        {
            if (gameObjects.Contains(_obj)) return;

            gameObjects.Add(_obj);
            if (hasStarted)
            {
                DoStartGameObjects(gameObjects.Count - 1);
            }
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
                DoStartGameObjects(i);
                hasStarted = true;
            }
        }

        private static void DoStartGameObjects(int _i)
        {
            if (gameObjects[_i].useRendering) gameObjects[_i].InitializeMeshRenderer();
            gameObjects[_i].Start();
        }

    }
}
