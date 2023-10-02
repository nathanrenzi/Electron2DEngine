using static Electron2D.OpenGL.GL;

namespace Electron2D.Core.Rendering
{
    /// <summary>
    /// Handles rendering every renderable object in the scene. All Render() functions are called from here.
    /// </summary>
    public static class RenderLayerManager
    {
        public static event Action<int> onLayerRendered;

        private static SortedList<int, List<IRenderable>> renderLayerOrderedList = new SortedList<int, List<IRenderable>>();
        private static bool wasLinearLastRenderCall = false;

        /// <summary>
        /// Registers or reorders the IRenderable in the render layer sorted list.
        /// </summary>
        /// <param name="_renderable">The IRenderable to order in the sorted list.</param>
        /// <param name="_reorder">True if the IRenderable already exists in the render layer list.</param>
        /// <param name="_oldRenderLayer">Used for reordering. The old render layer of the IRenderable being reordered.</param>
        /// <param name="_newRenderLayer">Used for reordering. The new render layer of the IRenderable being reordered.</param>
        public static void OrderRenderable(IRenderable _renderable, bool _reorder = false, int _oldRenderLayer = -1, int _newRenderLayer = -1)
        {
            // Removing the old render layer if the IRenderable is reordering itself instead of initializing
            if (_reorder)
            {
                // If the render layer is registered in the sorted list, remove the gameobject from the value list
                List<IRenderable> list;
                if (renderLayerOrderedList.TryGetValue(_oldRenderLayer, out list))
                {
                    bool removed = list.Remove(_renderable);
                    if (!removed) Console.WriteLine($"Since item does not exist in layer {_oldRenderLayer}, cannot remove it.");
                }
            }

            int renderOrder = _reorder ? _newRenderLayer : _renderable.GetRenderLayer();
            // If true, the render layer was not in the sorted list yet so it is added
            if (!renderLayerOrderedList.TryAdd(renderOrder, new List<IRenderable> { _renderable }))
            {
                // If false, the render layer already exists so the object must be added to an existing value list
                renderLayerOrderedList[renderOrder].Add(_renderable);

                // The sorted list class is already sorted in ascending layer, so no extra sorting is necessary
            }
        }

        /// <summary>
        /// Removes the IRenderable from the rendering system (Can still render these manually after removal).
        /// </summary>
        /// <param name="_renderable">The IRenderable to remove</param>
        public static void RemoveRenderable(IRenderable _renderable)
        {
            // Removing the object from the render order dictionary
            List<IRenderable> list;
            if (renderLayerOrderedList.TryGetValue(_renderable.GetRenderLayer(), out list))
            {
                list.Remove(_renderable);
            }
        }

        /// <summary>
        /// Called by the game loop to render all current render layers. Should not be called manually.
        /// </summary>
        public static void RenderAllLayers()
        {
            foreach (KeyValuePair<int, List<IRenderable>> pair in renderLayerOrderedList)
            {
                onLayerRendered?.Invoke(pair.Key);

                for (int i = 0; i < pair.Value.Count; i++)
                {
                    pair.Value[i].Render();
                }
            }
        }

        public static void SetTextureFiltering(bool _linear)
        {
            // Currently not working - No changes observed after this was added
            if(_linear && !wasLinearLastRenderCall)
            {
                // Linear filtering should be used
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR_MIPMAP_NEAREST); // https://community.khronos.org/t/changing-texture-filtering-on-the-fly/28226/4
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR_MIPMAP_NEAREST);
                wasLinearLastRenderCall = true;
            }
            else if(!_linear && wasLinearLastRenderCall)
            {
                // Nearest filtering should be used
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST_MIPMAP_NEAREST);
                glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST_MIPMAP_NEAREST);
                wasLinearLastRenderCall = false;
            }
        }
    }
}
