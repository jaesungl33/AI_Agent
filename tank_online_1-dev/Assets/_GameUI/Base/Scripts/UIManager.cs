using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Purchasing;

/// <summary>
/// UIManager handles the display and management of UI elements in the game.
/// It provides methods to show and hide the main menu, game UI, and other UI elements as needed.
/// </summary>
public class UIManager : Singleton<UIManager>, IInitializableManager
{
    [SerializeField] private UIIDs currentUIID = UIIDs.None;
    [SerializeField] private List<UIScreenBase> screens;    
    private Dictionary<UIIDs, UIScreenBase> uiIDToScreenMap;

    public UnityAction<bool> OnInitialized { get; set; }

    private void Start()
    {
        OnInitialized?.Invoke(true);
    }
    
    public void Initialize()
    {
        // Find all screen prefabs in Resources/Prefabs folder
        UIScreenBase[] screenPrefabs = Resources.LoadAll<UIScreenBase>("Prefabs");

        uiIDToScreenMap = new Dictionary<UIIDs, UIScreenBase>();
        List<UIScreenBase> instantiatedScreens = new List<UIScreenBase>();

        // Create instances of each screen
        foreach (var prefab in screenPrefabs)
        {
            UIScreenBase screenInstance = Instantiate(prefab, transform);
            screenInstance.Initialize();
            instantiatedScreens.Add(screenInstance);
            uiIDToScreenMap.Add(screenInstance.ID, screenInstance);
        }
        // Update the screens list with instantiated objects
        screens = instantiatedScreens;
        EventManager.Register<UIEvent>(ShowUI);

        ShowStartupUI();
    }

    private void ShowStartupUI()
    {
        ShowUI(new UIEvent(UIIDs.Loading));
    }

    private void ShowUI(UIEvent uiEvent)
    {
        if (currentUIID == uiEvent.ID)
        {
            Debug.Log($"UI {uiEvent.ID} is already active.");
            return; // No need to show the same UI again
        }

        if (uiEvent.ID == UIIDs.None)
        {
            HideAll();
            currentUIID = uiEvent.ID;
            return;
        }

        HideUI(currentUIID);

        EventManager.TriggerEvent(new ShowPopupEvent(PopupIDs.None));    //clear all popups
        currentUIID = uiEvent.ID;
        uiIDToScreenMap.TryGetValue(uiEvent.ID, out UIScreenBase screen);
        if (screen != null)
        {
            screen.Show(param: uiEvent.param);
        }
        else
        {
            Debug.LogError($"UI Screen with ID {uiEvent.ID} not found.");
        }
    }

    private void HideUI(UIIDs ID)
    {
        uiIDToScreenMap.TryGetValue(ID, out UIScreenBase screen);
        if (screen != null)
        {
            screen.Hide();
            EventManager.TriggerEvent<HideUIEvent>(new HideUIEvent(ID));
        }
    }

    private void HideAll()
    {
        // Implement logic to hide all UI elements
        Debug.Log("Hiding All UI");
        foreach (var screen in screens)
        {
            screen.Hide();
        }
    }
}

public struct UIEvent
{
    public UIIDs ID { get; }
    public ScreenParam param { get; }

    public UIEvent(UIIDs id, ScreenParam param = null)
    {
        ID = id;
        this.param = param;
    }
}
public struct HideUIEvent
{
    public UIIDs ID { get; }
    public HideUIEvent(UIIDs id)
    {
        ID = id;
    }
}