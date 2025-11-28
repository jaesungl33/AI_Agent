using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GarageStatItem : MonoBehaviour
{
    public StatType statType;
    public Image image;
    public TextMeshProUGUI textMeshProUGUI;

    public void Init(float ratio, float currentValue)
    {
        image.fillAmount = ratio;
        textMeshProUGUI.text = currentValue.ToString();
    }

    public void InitDmg(float ratio, int[] damages)
    {
        image.fillAmount = ratio;
        textMeshProUGUI.text = $"{damages[0]} - {damages[1]}";
    }
}