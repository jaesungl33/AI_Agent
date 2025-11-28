using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ScreenNavigator : MonoBehaviour
{
    [SerializeField] private UIIDs initialScreen = UIIDs.None;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClicked);
    }

    public void OnClicked()
    {
        if(initialScreen == UIIDs.None)
        {
            string message = LocalizationHelper.GetString(nameof(LocKeys.UI_Popup), LocKeys.UI_Popup.UI_Popup_ComingSoon);
            EventManager.TriggerEvent(new ShowPopupEvent(PopupIDs.Inform, new InformPopupParam() { message = message }));
            return;
        }
        EventManager.TriggerEvent(new UIEvent(initialScreen));
    }
}
