using Electron2D.Rendering;
using System.Drawing;

namespace Electron2D.UI
{
    public enum UIPanelType
    {
        Panel,
        Sliced
    }

    public sealed class UIPanelDef
    {
        public UIPanelType Type { get; }

        public Material? Material { get; }
        public ITexture? Texture { get; }
        public Color? Color { get; }

        public Border BorderUV { get; }
        public int BorderPixelSize { get; }

        private UIPanelDef(UIPanelType type, Color? color, Material? material, ITexture? texture, Border borderUV, int borderPixelSize)
        {
            Type = type;
            Color = color;
            Material = material;
            Texture = texture;
            BorderUV = borderUV;
            BorderPixelSize = borderPixelSize;
        }

        public static UIPanelDef PanelFromColor(Color color)
            => new UIPanelDef(UIPanelType.Panel, color, null, null, default, 0);
        public static UIPanelDef PanelFromMaterial(Material material)
            => new UIPanelDef(UIPanelType.Panel, null, material, null, default, 0);
        public static UIPanelDef PanelFromTexture(ITexture texture)
            => new UIPanelDef(UIPanelType.Panel, null, null, texture, default, 0);
        public static UIPanelDef SlicedFromMaterial(Material material, Border borderUV, int borderPixelSize)
            => new UIPanelDef(UIPanelType.Sliced, null, material, null, borderUV, borderPixelSize);
        public static UIPanelDef SlicedFromTexture(ITexture texture, Border borderUV, int borderPixelSize)
            => new UIPanelDef(UIPanelType.Sliced, null, null, texture, borderUV, borderPixelSize);

        public UIElement Create(UIElementArgs? arguments = null)
        {
            return Type switch
            {
                UIPanelType.Sliced => Material != null ? new UISlicedPanel(Material, BorderUV, BorderPixelSize, arguments) : Texture != null ?
                    new UISlicedPanel(Texture, BorderUV, BorderPixelSize, arguments)
                    : throw new InvalidOperationException("A Material or Texture is needed to create a UISlicedPanel."),

                _ => Material != null ? new UIPanel(Material, arguments)
                    : Texture != null ? new UIPanel(Texture, arguments)
                    : Color.HasValue ? new UIPanel(Color.Value, arguments)
                    : throw new InvalidOperationException("A Material, Texture, or Color is needed to create a UIPanel.")
            };
        }
    }
}
