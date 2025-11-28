using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class PopupBase : MonoBehaviour, IScreen
{
    [SerializeField] protected GameObject root;
    [SerializeField] protected PopupIDs id;
    [SerializeField] protected CanvasLayer layer = CanvasLayer.Popup;

    protected Canvas rootCanvas;
    protected int sortingOrder;
    
    public bool IsVisible => root.activeSelf;
    public CanvasLayer Layer => layer;
    public PopupIDs ID => id;

    protected virtual void Awake()
    {
        rootCanvas = GetComponent<Canvas>();
    }

    public int SortingOrder => sortingOrder;

    public virtual void Show(int additionalSortingOrder = 0, ScreenParam param = null)
    {
        root.SetActive(true);
        sortingOrder = (int)layer + additionalSortingOrder;
        rootCanvas.sortingOrder = sortingOrder;
    }

    public virtual void Hide()
    {
        root.SetActive(false);
    }
    public void ClosePopup()
    {
        EventManager.TriggerEvent(new PopPopupEvent()
        {
            popupID = id
        });
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
            transform.GetComponent<Canvas>().sortingOrder = (int)layer;
        }
        else if (transform.GetComponent<Canvas>() == null)
        {
            Debug.LogError("PopupBase requires a Canvas component on the root GameObject.");
        }
#endif
    }

    public virtual void Initialize()
    {
        Hide();
    }
}