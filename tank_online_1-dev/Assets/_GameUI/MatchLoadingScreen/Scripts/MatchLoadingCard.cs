using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchLoadingCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image tankImage;
    [SerializeField] private TextMeshProUGUI tankNameText;
    //[SerializeField] private Image tankClassImage;
    

    public void SetInfo(string playerName, Sprite tankSprite, string tankName, Sprite tankClassSprite)
    {
        playerNameText.text = playerName;
        tankImage.sprite = tankSprite;
        tankNameText.text = tankName;
        //tankClassImage.sprite = tankClassSprite;
    }
}
