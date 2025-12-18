public interface ISerializedFieldCheckable
{
    public enum Flag
    {
        OnPreprocessBuild,
        OnLoadOrRecompiled,
        OnAssetImported, // IAssetImportedCallback
    }
    bool onSerializedChanged(Flag flag);
}