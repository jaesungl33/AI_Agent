using UnityEngine;
using UnityEditor;
public class AutoAnchorWindow : EditorWindow
{
    [MenuItem("Tools/Auto Anchor")]
    public static void ShowWindow()
    {
        GetWindow<AutoAnchorWindow>("Auto Anchor");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Set Anchor To Corners (Selection)"))
        {
            foreach (GameObject obj in Selection.gameObjects)
            {
                RectTransform t = obj.GetComponent<RectTransform>();
                if (t != null)
                {
                    SetAnchorToCorners(t);
                }
            }
        }
    }

    private void SetAnchorToCorners(RectTransform t)
    {
        if (t == null || t.parent == null) return;

        RectTransform pt = t.parent as RectTransform;

        Vector2 newAnchorsMin = new Vector2(
            t.anchorMin.x + t.offsetMin.x / pt.rect.width,
            t.anchorMin.y + t.offsetMin.y / pt.rect.height);
        Vector2 newAnchorsMax = new Vector2(
            t.anchorMax.x + t.offsetMax.x / pt.rect.width,
            t.anchorMax.y + t.offsetMax.y / pt.rect.height);

        t.anchorMin = newAnchorsMin;
        t.anchorMax = newAnchorsMax;

        t.offsetMin = Vector2.zero;
        t.offsetMax = Vector2.zero;

        Debug.Log("Anchors set to corners for " + t.name);
    }
   
}