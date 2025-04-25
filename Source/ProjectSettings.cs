namespace Electron2D
{
    public static class ProjectSettings
    {
        // Physics -----------
        public static float PhysicsTimestep { get; } = 0.016f;
        public static float PhysicsGravity { get; } = -15f;
        public static int PhysicsVelocityIterations { get; } = 6;
        public static int PhysicsPositionIterations { get; } = 2;
        // -------------------

        // Networking --------
        public static ushort SteamPort { get; } = 42420;
        // -------------------

        // Misc --------------
        public static bool GraphicsErrorCheckingEnabled { get; } = false;
        public static bool ShowElectron2DSplashscreen { get; } = false;
        public static bool UseHDRFrameBuffers { get; } = false;
        public static string WindowTitle { get; } = "Electron2D Development Build";
        public static string EngineResourcePath { get; } = "Resources/Electron2D/";
        public static string ResourcePath { get; } = "Resources/";
        // -------------------
    }
}