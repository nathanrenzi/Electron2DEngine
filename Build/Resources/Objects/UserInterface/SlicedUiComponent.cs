using Electron2D.Core.Rendering;
using Electron2D.Core.UI;
using Electron2D.Core.UserInterface;
using System.Drawing;
using System.Numerics;

namespace Electron2D.Build.Resources.Objects
{
    public class SlicedUiComponent : UiComponent
    {
        public float[] vertices { get; private set; } = new float[36 * 9];

        public static readonly float[] defaultUV = // 36 verts * 2 floats
        {

        };

        public static readonly uint[] indices = // 18 tris * 3 uints
        {

        };

        // MAKE THESE EDITABLE AFTER INITIALIZATION??
        public int Left { get; private set; }
        public int Right { get; private set; }
        public int Top { get; private set; }
        public int Bottom { get; private set; }
        public int PixelsPerUnit { get; private set; } = 100;


        public SlicedUiComponent(Color _startColor, int _sizeX, int _sizeY,
            int _left, int _right, int _top, int _bottom, int _pixelsPerUnit = 100)
            : base(_sizeX: _sizeX, _sizeY: _sizeY)
        {
            Left= _left;
            Right= _right;
            Top= _top;
            Bottom= _bottom;
            PixelsPerUnit= _pixelsPerUnit;

            BuildVertexMesh();
            rendererReference.SetVertexArrays(vertices, indices, defaultUV);

            constraints.SetPosition(new PixelConstraint(20, UiConstraintSide.Left));
            constraints.SetPosition(new PixelConstraint(20, UiConstraintSide.Bottom));
            Color = _startColor;
        }

        /// <summary>
        /// Builds 9-sliced vertex arrays based on size and edge padding
        /// </summary>
        private void BuildVertexMesh()
        {
            float sx = sizeX * 2;
            float sy = sizeY * 2;

            float L = -sx + (Left * 2);
            float R = sx - (Right * 2);
            float T = sy - (Top * 2);
            float B = -sy + (Bottom * 2);

            float LU = Math.Clamp((Left * 2) / PixelsPerUnit, 0, 0.5f);
            float RU = 1 - Math.Clamp((Right * 2) / PixelsPerUnit, 0, 0.5f);
            float TV = 1 - Math.Clamp((Top * 2) / PixelsPerUnit, 0, 0.5f);
            float BV = Math.Clamp((Bottom * 2) / PixelsPerUnit, 0, 0.5f);

            AddVertex(0, -sx, sy,   0, 1, 1, 1, 1, 1, 0); 
            AddVertex(1, L, sy,     LU, 1, 1, 1, 1, 1, 0);   
            AddVertex(2, L, T,      LU, TV, 1, 1, 1, 1, 0);    
            AddVertex(3, -sx, T,    0, TV, 1, 1, 1, 1, 0);  
            AddVertex(4, R, sy,     RU, 1, 1, 1, 1, 1, 0);   
            AddVertex(5, sx, sy,    1, 1, 1, 1, 1, 1, 0);  
            AddVertex(6, sx, T,     1, TV, 1, 1, 1, 1, 0);   
            AddVertex(7, R, T,      RU, TV, 1, 1, 1, 1, 0);    
            AddVertex(8, R, B,      RU, BV, 1, 1, 1, 1, 0);    
            AddVertex(9, sx, B,     1, BV, 1, 1, 1, 1, 0);   
            AddVertex(10, sx, -sy,  1, 0, 1, 1, 1, 1, 0);  
            AddVertex(11, R, -sy,   RU, 0, 1, 1, 1, 1, 0);   
            AddVertex(12, -sx, B,   0, BV, 1, 1, 1, 1, 0);   
            AddVertex(13, L, B,     LU, BV, 1, 1, 1, 1, 0);     
            AddVertex(14, L, -sy,   LU, 0, 1, 1, 1, 1, 0);   
            AddVertex(15, -sx, -sy, 0, 0, 1, 1, 1, 1, 0); 
            AddVertex(16, L, sy,    LU, 1, 1, 1, 1, 1, 0);
            AddVertex(17, R, sy,    RU, 1, 1, 1, 1, 1, 0);
            AddVertex(18, R, T,     RU, TV, 1, 1, 1, 1, 0);
            AddVertex(19, L, T,     LU, TV, 1, 1, 1, 1, 0);
            AddVertex(20, R, T,     RU, TV, 1, 1, 1, 1, 0);
            AddVertex(21, sx, T,    1, TV, 1, 1, 1, 1, 0);
            AddVertex(22, sx, B,    1, BV, 1, 1, 1, 1, 0);
            AddVertex(23, R, B,     RU, BV, 1, 1, 1, 1, 0);
            AddVertex(24, L, B,     LU, BV, 1, 1, 1, 1, 0);
            AddVertex(25, R, B,     RU, BV, 1, 1, 1, 1, 0);
            AddVertex(26, R, -sy,   RU, 0, 1, 1, 1, 1, 0);
            AddVertex(27, L, -sy,   LU, 0, 1, 1, 1, 1, 0);
            AddVertex(28, -sx, T,   0, TV, 1, 1, 1, 1, 0);
            AddVertex(29, L, T,     LU, TV, 1, 1, 1, 1, 0);
            AddVertex(30, L, B,     LU, BV, 1, 1, 1, 1, 0);
            AddVertex(31, -sx, B,   0, BV, 1, 1, 1, 1, 0);
            AddVertex(32, L, T,     LU, TV, 1, 1, 1, 1, 0);
            AddVertex(33, R, T,     RU, TV, 1, 1, 1, 1, 0);
            AddVertex(34, R, B,     RU, BV, 1, 1, 1, 1, 0);
            AddVertex(35, L, B,     LU, BV, 1, 1, 1, 1, 0);
        }

        private void AddVertex(int _index, float _x, float _y, float _u,
            float _v, float _r, float _g, float _b, float _a, float _tex)
        {
            int stride = 9;
            vertices[(_index * stride) + 0] = _x;
            vertices[(_index * stride) + 1] = _y;
            vertices[(_index * stride) + 2] = _u;
            vertices[(_index * stride) + 3] = _v;
            vertices[(_index * stride) + 4] = _r;
            vertices[(_index * stride) + 5] = _g;
            vertices[(_index * stride) + 6] = _b;
            vertices[(_index * stride) + 7] = _a;
            vertices[(_index * stride) + 8] = _tex;
        }

        protected override void OnUiEvent(UiEvent _event)
        {
            if(_event == UiEvent.LeftClickDown)
            {
                Color = Color.DarkGray;
            }
            else if(_event == UiEvent.LeftClickUp)
            {
                Color = thisFrameData.isHovered ? Color.LightGray : Color.White;
            }

            if(_event == UiEvent.HoverStart)
            {
                if (!thisFrameData.isLeftClicked)
                {
                    Color = Color.LightGray;
                }
            }
            else if(_event == UiEvent.HoverEnd)
            {
                if (!thisFrameData.isLeftClicked)
                {
                    Color = Color.White;
                }
            }
        }
    }
}