using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class DeepCloneExtensions
{
public static T DeepClone<T>(this T obj)
{
    if (obj == null) return default;
    
    using var ms = new MemoryStream();
    var formatter = new BinaryFormatter();
    formatter.Serialize(ms, obj);
    ms.Position = 0;
    return (T)formatter.Deserialize(ms);
}
}
