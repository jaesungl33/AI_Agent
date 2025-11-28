using UnityEngine;

public static class ScriptableObjectCloner
{
    /// <summary>
    /// Clone một ScriptableObject và trả về bản sao mới, độc lập với bản gốc.
    /// </summary>
    /// <typeparam name="T">Kiểu của ScriptableObject</typeparam>
    /// <param name="original">ScriptableObject gốc</param>
    /// <returns>Bản sao clone</returns>
    public static T Clone<T>(T original) where T : ScriptableObject
    {
        if (original == null)
        {
            Debug.LogWarning("Clone failed: original is null");
            return null;
        }

        T clone = ScriptableObject.CreateInstance<T>();
        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(original), clone);
        return clone;
    }
}
