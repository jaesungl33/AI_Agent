using Fusion.TankOnlineModule;
using TMPro;
using UnityEngine;

public class TankNameComponent : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameTag;

    public void UpdateNameTag(Player player)
    {
        if (nameTag == null)
        {
            Debug.LogError("Name tag is not assigned.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player is null.");
            return;
        }

        // Update the name tag text with the player's name
        nameTag.text = player.PlayerName.ToString();
    }
}