using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class UIScreenBase : MonoBehaviour, IScreen
{
    public UIIDs ID;
    [SerializeField] protected GameObject root;
    public bool IsVisible => root.activeSelf;
    public int SortingOrder => (int)CanvasLayer.Screen;

    protected virtual void Awake()
    {
        
    }

    public virtual void Initialize()
    {
        Hide();
    }

    public virtual void RegisterEvents()
    {
        // Override in derived classes to register events
    }

    public virtual void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        root.SetActive(true);
    }

    public virtual void Hide()
    {
        root.SetActive(false);
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (root == null)
        {
            root = transform.GetChild(0).gameObject;
        }

        if (transform.GetComponent<Canvas>() != null)
        {
            transform.GetComponent<Canvas>().sortingOrder = (int)CanvasLayer.Screen;
        }
        else if (transform.GetComponent<Canvas>() == null)
        {
            Debug.LogError("UIScreenBase requires a Canvas component on the root GameObject.");
        }
#endif
    }
}