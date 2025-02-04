using Electron2D.Rendering;

namespace Electron2D.UserInterface
{
    public class SliderDef
    {
        public Material BackgroundMaterial;
        public Material SliderMaterial;
        public Material HandleMaterial;

        public SlicedPanelDef BackgroundPanelDef;
        public SlicedPanelDef SliderPanelDef;
        public SlicedPanelDef HandlePanelDef;

        public int SizeX;
        public int BackgroundSizeY;
        public int SliderSizeY;
        public int HandleSizeXY;
        public int HandlePadding;

        public float InitialValue;
        public float MaxValue;
        public float MinValue;
        public bool ForceWholeNumbers;
        public bool AllowNonHandleValueUpdates;
        public bool Interactable;

        /// <summary>Creates a new definition for a slider.</summary>
        /// <param name="backgroundMaterial">The material of the background panel.</param>
        /// <param name="sliderMaterial">The material of the slider panel (when the handle is moved,
        /// this panel is displayed to indicate the current percentage/value).</param>
        /// <param name="handleMaterial">The material of the handle.</param>
        /// <param name="backgroundPanelDef">The <see cref="SlicedPanel"/> definition for the background panel. Leaving this as null will use a non-sliced panel.</param>
        /// <param name="sliderPanelDef">The <see cref="SlicedPanel"/> definition for the slider panel (when the handle is moved,
        /// this panel is displayed to indicate the current percentage/value). Leaving this as null will use a non-sliced panel.</param>
        /// <param name="handlePanelDef">The <see cref="SlicedPanel"/> definition for the handle panel. Leaving this as null will use a non-sliced panel.</param>
        /// <param name="initialValue">The initial value of the slider when it is created.</param>
        /// <param name="maxValue">The max value of the slider.</param>
        /// <param name="minValue">The min value of the slider.</param>
        /// <param name="handlePadding">The padding of the handle, in pixels, from either side of the slider. This will also affect the start and end points for reading the values.</param>
        /// <param name="forceWholeNumbers">Should the slider snap to whole numbers?</param>
        /// <param name="allowNonHandleValueUpdates">Should clicking the background should change the value of the slider?</param>
        /// <param name="interactable">Should the slider be interactable using the mouse?</param>
        public SliderDef(Material backgroundMaterial, Material sliderMaterial, Material handleMaterial,
            int sizeX, int backgroundSizeY, int sliderSizeY, int handleSizeXY,
            SlicedPanelDef backgroundPanelDef = null, SlicedPanelDef sliderPanelDef = null, SlicedPanelDef handlePanelDef = null,
            float initialValue = 0, float maxValue = 1, float minValue = 0, int handlePadding = 0, bool forceWholeNumbers = false,
            bool allowNonHandleValueUpdates = true, bool interactable = true)
        {
            BackgroundMaterial = backgroundMaterial;
            SliderMaterial = sliderMaterial;
            HandleMaterial = handleMaterial;
            SizeX = sizeX;
            BackgroundSizeY = backgroundSizeY;
            SliderSizeY = sliderSizeY;
            HandleSizeXY = handleSizeXY;
            BackgroundPanelDef = backgroundPanelDef;
            SliderPanelDef = sliderPanelDef;
            HandlePanelDef = handlePanelDef;
            InitialValue = initialValue;
            MaxValue = maxValue;
            MinValue = minValue;
            HandlePadding = handlePadding;
            ForceWholeNumbers = forceWholeNumbers;
            AllowNonHandleValueUpdates = allowNonHandleValueUpdates;
            Interactable = interactable;
        }
    }
}