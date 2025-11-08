using System.Numerics;

namespace Electron2D.UserInterface
{
    public enum UIScalingMode
    {
        RealResolution,
        VirtualResolution
    }

    public class UISettings
    {
        public UIScalingMode ScalingMode = UIScalingMode.VirtualResolution;
        public Vector2 VirtualResolution = new Vector2(1920, 1080);
        public bool MaintainAspect = true;
    }
}
