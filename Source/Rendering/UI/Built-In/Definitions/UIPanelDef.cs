using Electron2D.Rendering;
using System.Drawing;

namespace Electron2D.UI
{
    /// <summary>
    /// Defines the rendering type of a <see cref="UIPanelDef"/>.
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
        public SharedResource<Material>? Material { get; }

        /// <summary>
        /// Gets the texture used to render this panel, or <see langword="null"/> if not applicable.
        /// </summary>
        public SharedResource<Texture2D>? Texture { get; }

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

        private UIPanelDef(UIPanelType type, Color? color, SharedResource<Material>? material,
            SharedResource<Texture2D>? texture, Border borderUV, int borderPixelSize)
        {
            Type = type;
            Color = color;
            Material = material;
            Texture = texture;
            BorderUV = borderUV;
            BorderPixelSize = borderPixelSize;
        }

        /// <summary>
        /// Creates a blank <see cref="UIPanelType.Panel"/> definition rendered in white.
        /// </summary>
        /// <returns></returns>
        public static UIPanelDef Blank()
            => new UIPanelDef(UIPanelType.Panel, System.Drawing.Color.White, null, null, default, 0);

        /// <summary>
        /// Creates a <see cref="UIPanelType.Panel"/> definition rendered with a solid color.
        /// </summary>
        /// <param name="color">The color used to render the panel.</param>
        public static UIPanelDef PanelFromColor(Color color)
            => new UIPanelDef(UIPanelType.Panel, color, null, null, default, 0);

        /// <summary>
        /// Creates a <see cref="UIPanelType.Panel"/> definition rendered with a material.
        /// </summary>
        /// <param name="material">The material used to render the panel.</param>
        public static UIPanelDef PanelFromMaterial(SharedResource<Material> material)
            => new UIPanelDef(UIPanelType.Panel, null, material, null, default, 0);

        /// <summary>
        /// Creates a <see cref="UIPanelType.Panel"/> definition rendered with a texture.
        /// </summary>
        /// <param name="texture">The texture used to render the panel.</param>
        public static UIPanelDef PanelFromTexture(SharedResource<Texture2D> texture)
            => new UIPanelDef(UIPanelType.Panel, null, null, texture, default, 0);

        /// <summary>
        /// Creates a <see cref="UIPanelType.Sliced"/> definition rendered with a material.
        /// </summary>
        /// <param name="material">The material used to render the panel.</param>
        /// <param name="borderUV">The UV coordinates defining the 9-slice border regions.</param>
        /// <param name="borderPixelSize">The border thickness in pixels for 9-sliced rendering.</param>
        public static UIPanelDef SlicedFromMaterial(SharedResource<Material> material, Border borderUV, int borderPixelSize)
            => new UIPanelDef(UIPanelType.Sliced, null, material, null, borderUV, borderPixelSize);

        /// <summary>
        /// Creates a <see cref="UIPanelType.Sliced"/> definition rendered with a texture.
        /// </summary>
        /// <param name="texture">The texture used to render the panel.</param>
        /// <param name="borderUV">The UV coordinates defining the 9-slice border regions.</param>
        /// <param name="borderPixelSize">The border thickness in pixels for 9-sliced rendering.</param>
        public static UIPanelDef SlicedFromTexture(SharedResource<Texture2D> texture, Border borderUV, int borderPixelSize)
            => new UIPanelDef(UIPanelType.Sliced, null, null, texture, borderUV, borderPixelSize);

        /// <summary>
        /// Instantiates a <see cref="UIElement"/> based on this definition.
        /// </summary>
        /// <returns>A new <see cref="UIPanel"/> or <see cref="UISlicedPanel"/> configured from this definition.</returns>
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
