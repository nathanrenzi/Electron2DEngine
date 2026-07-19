namespace Atlas2D.Networking
{
    public interface INetworkFactory
    {
        public static abstract NetworkNode FactoryMethod(string json);
    }
}
