using IngameDebugConsole;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class CheatForConsole : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    [ConsoleMethod("toggle", "Toggle feature", "feature")]
    public static void ToggleFeature(string featureName)
    {
        Debug.Log($"Toggling feature: {featureName}");
        // Implement feature toggling logic here
        EventManager.TriggerEvent(new ToggleEvent() { featureName = featureName });

    }
    private static Dictionary<string, GameObject> cachedObjects = new Dictionary<string, GameObject>();

    [ConsoleMethod("toggle-obj", "Toggle feature", "feature")]
    public static void ToggleObject(string objName)
    {
        Debug.Log($"ToggleObject: {objName}");

        GameObject obj;
        if (!cachedObjects.TryGetValue(objName, out obj) || obj == null)
        {
            obj = GameObject.Find(objName);

            if (obj != null)
                cachedObjects[objName] = obj;
        }

        if (obj != null)
        {
            obj.SetActive(!obj.activeSelf);
            Debug.Log($"Object {objName} is now {(obj.activeSelf ? "active" : "inactive")}");
        }
        else
        {
            Debug.Log($"Object {objName} not found");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}

public class ToggleEvent
{
    public string featureName;
}
