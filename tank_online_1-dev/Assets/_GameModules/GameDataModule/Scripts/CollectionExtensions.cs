using UnityEngine;

public static class CollectionExtensions
{
    public static TCollection GetCollection<TCollection>() where TCollection : ScriptableObject
    {
        var loadedCollection = Resources.Load<TCollection>(CollectionBase<TCollection>.CollectionPath + typeof(TCollection).Name);
        if (loadedCollection == null)
        {
            Debug.LogError($"Failed to load collection at path: {typeof(TCollection).Name}");
            return default(TCollection);
        }

        return loadedCollection;
    }

}
