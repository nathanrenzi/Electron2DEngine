namespace Electron2D.Core.GameObjects
{
    public static class GameObjectManager
    {
        public static event Action<int> onLayerRendered;
        public static List<GameObject> gameObjectsInScene = new List<GameObject>();

        private static SortedList<int, List<GameObject>> renderLayerOrderedList = new SortedList<int, List<GameObject>>();
        private static bool hasStarted = false;
        public static void RegisterGameObject(GameObject _obj)
        {
            if (gameObjectsInScene.Contains(_obj)) return;

            OrderGameObject(_obj);

            gameObjectsInScene.Add(_obj);
            if (hasStarted)
            {
                DoStartGameObject(gameObjectsInScene.Count - 1);
            }
        }

        /// <summary>
        /// Registers or reorders the gameobject in the render layer sorted list.
        /// </summary>
        /// <param name="_obj">The object to order in the sorted list.</param>
        /// <param name="_reorder">True if the object already exists in the render layer list.</param>
        /// <param name="_oldRenderLayer">Used for reordering. The old render layer of the object being reordered.</param>
        /// <param name="_newRenderLayer">Used for reordering. The new render layer of the object being reordered.</param>
        public static void OrderGameObject(GameObject _obj, bool _reorder = false, int _oldRenderLayer = -1, int _newRenderLayer = -1)
        {
            // Removing the old render layer if the gameobject is reordering itself instead of initializing
            if (_reorder)
            {
                // If the render layer is registered in the sorted list, remove the gameobject from the value list
                List<GameObject> list;
                if(renderLayerOrderedList.TryGetValue(_oldRenderLayer, out list))
                {
                    bool removed = list.Remove(_obj);
                    if (!removed) Console.WriteLine($"Since item does not exist in layer {_oldRenderLayer}, cannot remove it.");
                }
            }

            int renderOrder = _reorder ? _newRenderLayer : _obj.renderLayer;
            // If true, the render layer was not in the sorted list yet so it is added
            if (!renderLayerOrderedList.TryAdd(renderOrder, new List<GameObject> { _obj }))
            {
                // If false, the render layer already exists so the object must be added to an existing value list
                renderLayerOrderedList[renderOrder].Add(_obj);

                // The sorted list class is already sorted in ascending layer, so no extra sorting is necessary
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

            // Removing the object from the render order dictionary
            List<GameObject> list;
            if (renderLayerOrderedList.TryGetValue(_obj.renderLayer, out list))
            {
                list.Remove(_obj);
            }
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
            foreach (KeyValuePair<int, List<GameObject>> pair in renderLayerOrderedList)
            {
                onLayerRendered?.Invoke(pair.Key);

                for (int i = 0; i < pair.Value.Count; i++)
                {
                    pair.Value[i].Render();
                }
            }
        }

        public static void StartGameObjects()
        {
            for (int i = 0; i < gameObjectsInScene.Count; i++)
            {
                DoStartGameObject(i);
                hasStarted = true;
            }
        }

        private static void DoStartGameObject(int _i)
        {
            if (gameObjectsInScene[_i].useAutoInitialization) gameObjectsInScene[_i].InitializeMeshRenderer();
            gameObjectsInScene[_i].Start();
        }

    }
}
