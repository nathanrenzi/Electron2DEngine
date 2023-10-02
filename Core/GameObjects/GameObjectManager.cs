namespace Electron2D.Core.GameObjects
{
    public static class GameObjectManager
    {
        public static List<GameObject> gameObjectsInScene = new List<GameObject>();
        private static bool hasStarted = false;
        public static void RegisterGameObject(GameObject _obj)
        {
            if (gameObjectsInScene.Contains(_obj)) return;

            gameObjectsInScene.Add(_obj);
            if (hasStarted)
            {
                DoStartGameObject(gameObjectsInScene.Count - 1);
            }
        }

        /// <summary>
        /// Removes a gameobject from the gameobject manager, removing all gameobject functionality from it.
        /// </summary>
        /// <param name="_obj">The gameobject to unregister.</param>
        public static void UnregisterGameObject(GameObject _obj)
        {
            if (!gameObjectsInScene.Contains(_obj)) return;

            gameObjectsInScene.Remove(_obj);
        }

        public static void UpdateGameObjects()
        {
            for (int i = 0; i < gameObjectsInScene.Count; i++)
            {
                gameObjectsInScene[i].Update();
            }
        }

        // GameObject rendering is handled in RenderLayerManager - Remove gameobjects from rendering process altogether and swap with renderers

        public static void StartGameObjects()
        {
            for (int i = 0; i < gameObjectsInScene.Count; i++)
            {
                DoStartGameObject(i);
            }
            hasStarted = true;
        }

        private static void DoStartGameObject(int _i)
        {
            gameObjectsInScene[_i].Start();
        }

    }
}
