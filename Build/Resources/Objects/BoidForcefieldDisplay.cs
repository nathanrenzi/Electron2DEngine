using Electron2D.Core;
using Electron2D.Core.GameObjects;
using System.Numerics;

namespace Electron2D.Build.Resources.Objects
{
    public class BoidForcefieldDisplay : GameObject
    {
        public override void Start()
        {
            // Applying texture
            renderer.SetVertexValueAll(Core.Rendering.SpriteVertexAttribute.TextureIndex, 2);
            renderer.SetVertexValueAll(Core.Rendering.SpriteVertexAttribute.ColorA, 0.4f);
            transform.scale = Vector2.One * 5f;
        }
    }
}
