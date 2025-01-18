namespace Electron2D
{
    public interface ISaveable
    {
        public SaveData GetSaveData();
        public void SetSaveData(SaveData data);
    }
}
