using TMPro;
using UnityEngine;

public class InformPopup : PopupBase
{
    public TextMeshProUGUI textMeshProUGUI;

    public override void Show(int additionalSortingOrder, ScreenParam param = null)
    {
        base.Show(additionalSortingOrder, param);
        if (param is InformPopupParam informParam)
        {
            textMeshProUGUI.text = informParam.message;
        }
    }
}

public class InformPopupParam : ScreenParam
{
    public string message;
}