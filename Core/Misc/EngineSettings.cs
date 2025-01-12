namespace Electron2D.Core
{
    public static class EngineSettings
    {
        public static float PhysicsTimestep { get; } = 0.016f;
        public static float PhysicsGravity { get; } = -15f;
        public static int PhysicsVelocityIterations { get; } = 6;
        public static int PhysicsPositionIterations { get; } = 2;
        public static bool GraphicsErrorCheckingEnabled { get; } = false;
        public static bool ShowElectron2DSplashscreen { get; } = true;
    }
}