using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// PopupManager handles the display and management of Popup elements in the game.
/// It provides methods to show and hide the main menu, game Popup, and other Popup elements as needed.
/// </summary>
public class PopupManager : Singleton<PopupManager>
{
    [SerializeField] private List<PopupBase> popups;
    
    private Dictionary<PopupIDs, PopupBase> popupMap;
    private Stack<PopupBase> showingPopups = new();

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    private void Initialize()
    {
        // Find all popup prefabs in Resources/Prefabs folder
        PopupBase[] popupPrefabs = Resources.LoadAll<PopupBase>("Prefabs");

        popupMap = new Dictionary<PopupIDs, PopupBase>();
        List<PopupBase> instantiatedPopups = new List<PopupBase>();

        // Create instances of each popup
        foreach (var prefab in popupPrefabs)
        {
            PopupBase popupInstance = Instantiate(prefab, transform);
            popupInstance.Initialize();
            instantiatedPopups.Add(popupInstance);
            popupMap.Add(popupInstance.ID, popupInstance);
        }
        // Update the popups list with instantiated objects
        popups = instantiatedPopups;
        EventManager.Register<ShowPopupEvent>(ShowPopup);
        EventManager.Register<PopPopupEvent>(Pop);
    }

    private void ShowPopup(ShowPopupEvent uiEvent)
    {
        if(uiEvent.ID == PopupIDs.None)
        {
            PopAll();
            return;
        }
        if (showingPopups.Any(p => p.ID == uiEvent.ID))
        {
            Debug.Log($"Popup {uiEvent.ID} is already active.");
            return; // No need to show the same Popup again
        }
        showingPopups.Push(ShowPopup(uiEvent.ID, uiEvent.Param));
    }

    private void Pop(PopPopupEvent popPopup)
    {
        if(showingPopups.Count == 0)
        {
            Debug.LogWarning("No popups to hide.");
            return;
        }
        var popup = showingPopups.Pop();
        popup.Hide();
    }

    private void PopAll()
    {
        // Implement logic to hide all Popup elements
        Debug.Log("Hiding All Popup");
        foreach (var popup in popups)
        {
            popup.Hide();
        }
    }

    private PopupBase ShowPopup(PopupIDs popupID, ScreenParam param) 
    {
        if (popupMap.TryGetValue(popupID, out PopupBase popup))
        {
            var lastPopupInLayer = showingPopups.LastOrDefault(p => p.Layer == popup.Layer);
            var additionalSortingOrder = lastPopupInLayer != null ? lastPopupInLayer.SortingOrder + 5 : 5;
            popup.Show(additionalSortingOrder, param);
            return popup;
        }
        else
        {
            Debug.LogError($"Popup with ID {popupID} not found.");
            return null;
        }
    }
}

public struct ShowPopupEvent
{
    public PopupIDs ID { get; }
    public ScreenParam Param { get; }

    public ShowPopupEvent(PopupIDs id, ScreenParam param = null)
    {
        ID = id;
        Param = param;
    }
}

public struct PopPopupEvent
{
    public PopupIDs popupID;
}