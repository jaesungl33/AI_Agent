using UnityEngine;
using UnityEngine.UI;

public class GarageAbilityItem: MonoBehaviour
{
    [SerializeField] private Image imageIcon;
    [SerializeField] private TMPro.TextMeshProUGUI textDescription;
    public void Init(string description, Sprite icon)
    {
        textDescription.text = description;
        imageIcon.sprite = icon;
    }
}