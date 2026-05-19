namespace Atlas2D.Networking
{
    public interface INetworkFactory
    {
        public static abstract NetworkGameClass FactoryMethod(string json);
    }
}
