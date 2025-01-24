namespace Electron2D
{
    public abstract class GameClass
    {
        public static List<GameClass> GameClasses = new List<GameClass>();

        public GameClass()
        {
            GameClasses.Add(this);
        }

        ~GameClass()
        {
            GameClasses.Remove(this);
        }

        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void Dispose();
    }
}
