using Electron2D.Rendering;
using System.Drawing;

namespace Electron2D.UI
{
    /// <summary>
    /// Defines the visual style of a <see cref="UIPanelDef"/>.
    /// </summary>
    public enum UIPanelType
    {
        Panel,
        Sliced
    }

    /// <summary>
    /// A definition describing how a UI panel should be created and rendered.
    /// Use the static factory methods to construct instances.
    /// </summary>
    public sealed class UIPanelDef
    {
        /// <summary>
        /// Gets the visual style of the panel.
        /// </summary>
        public UIPanelType Type { get; }

        /// <summary>
        /// Gets the material used to render this panel, or <see langword="null"/> if not applicable.
        /// </summary>
        public Material? Material { get; }

        /// <summary>
        /// Gets the texture used to render this panel, or <see langword="null"/> if not applicable.
        /// </summary>
        public ITexture? Texture { get; }

        /// <summary>
        /// Gets the solid color used to render this panel, or <see langword="null"/> if not applicable.
        /// </summary>
        public Color? Color { get; }

        /// <summary>
        /// Gets the UV coordinates defining the 9-slice border regions.
        /// Only meaningful when <see cref="Type"/> is <see cref="UIPanelType.Sliced"/>.
        /// </summary>
        public Border BorderUV { get; }

        /// <summary>
        /// Gets the border thickness in pixels for 9-sliced rendering.
        /// Only meaningful when <see cref="Type"/> is <see cref="UIPanelType.Sliced"/>.
        /// </summary>
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

        /// <summary>
        /// Creates a <see cref="UIPanelType.Panel"/> definition rendered with a solid color.
        /// </summary>
        public static UIPanelDef PanelFromColor(Color color)
            => new UIPanelDef(UIPanelType.Panel, color, null, null, default, 0);
        /// <summary>
        /// Creates a <see cref="UIPanelType.Panel"/> definition rendered with a material.
        /// </summary>
        public static UIPanelDef PanelFromMaterial(Material material)
            => new UIPanelDef(UIPanelType.Panel, null, material, null, default, 0);
        /// <summary>
        /// Creates a <see cref="UIPanelType.Panel"/> definition rendered with a texture.
        /// </summary>
        public static UIPanelDef PanelFromTexture(ITexture texture)
            => new UIPanelDef(UIPanelType.Panel, null, null, texture, default, 0);
        /// <summary>
        /// Creates a <see cref="UIPanelType.Sliced"/> definition rendered with a material.
        /// </summary>
        public static UIPanelDef SlicedFromMaterial(Material material, Border borderUV, int borderPixelSize)
            => new UIPanelDef(UIPanelType.Sliced, null, material, null, borderUV, borderPixelSize);
        /// <summary>
        /// Creates a <see cref="UIPanelType.Sliced"/> definition rendered with a texture.
        /// </summary>
        public static UIPanelDef SlicedFromTexture(ITexture texture, Border borderUV, int borderPixelSize)
            => new UIPanelDef(UIPanelType.Sliced, null, null, texture, borderUV, borderPixelSize);

        /// <summary>
        /// Instantiates a <see cref="UIElement"/> based on this definition.
        /// </summary>
        public UIElement Create(UIRenderArgs? arguments = null)
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
