namespace Electron2D.Core.GameObjects
{
    public static class GameObjectManager
    {
        public static List<GameObject> gameObjectsInScene = new List<GameObject>();
        private static bool hasStarted = false;
        public static void RegisterGameObject(GameObject _obj)
        {
            if (gameObjectsInScene.Contains(_obj)) return;

            // Resorting the gameobjects in the scene after adding a new one
            SortByRenderOrder();

            gameObjectsInScene.Add(_obj);
            if (hasStarted)
            {
                DoStartGameObjects(gameObjectsInScene.Count - 1);
            }
        }

        /// <summary>
        /// Sorts all gameobjects in the scene by their render order so that the higher the render order, the closer to the top of the scene they will be.
        /// </summary>
        public static void SortByRenderOrder()
        {
            // A possible new system would be to have lists of GameObjects in a dictionary where the render order is the key
            // This would eliminate the need to reorder a list and gameobjects could easily
            // switch render order at will without a performance impact
            gameObjectsInScene = gameObjectsInScene.OrderBy(s => s.renderOrder).ToList();
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

        public static void RenderGameObjects()
        {
            for (int i = 0; i < gameObjectsInScene.Count; i++)
            {
                gameObjectsInScene[i].Render();
            }
        }

        public static void StartGameObjects()
        {
            for (int i = 0; i < gameObjectsInScene.Count; i++)
            {
                DoStartGameObjects(i);
                hasStarted = true;
            }
        }

        private static void DoStartGameObjects(int _i)
        {
            if (gameObjectsInScene[_i].useAutoInitialization) gameObjectsInScene[_i].InitializeMeshRenderer();
            gameObjectsInScene[_i].Start();
        }

    }
}
