﻿using Electron2D.Core;
using Electron2D.Core.GameObjects;
using Electron2D.Core.Rendering;
using System.Numerics;

namespace Electron2D.Build.Resources.Objects
{
    public class BoidForcefieldDisplay : GameObject
    {
        public BoidForcefieldDisplay() : base((int)RenderLayer.Transparent) { } 

        public override void Start()
        {
            // Applying texture
            renderer.SetSprite(0, 2, 0);
            renderer.SetVertexValueAll((int)SpriteVertexAttribute.ColorA, 0.4f);
            transform.scale = Vector2.One * 5f;
        }
    }
}