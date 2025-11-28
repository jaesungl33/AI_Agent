using Fusion.TankOnlineModule;
using FusionHelpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FinalScreenPlayerCard : MonoBehaviour
{
    [SerializeField] private GameObject goMine;
    [SerializeField] private Image tankIcon;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI killCountText;
    [SerializeField] private TextMeshProUGUI deathCountText, goldText;
    [SerializeField] private GameObject mvpIndicator;

    //private Player player;
    private MatchPlayerData playerData;
    private bool isMVP;

    public void SetPlayerInfo(MatchPlayerData data, Sprite tankIconSprite, bool isMvp)
    {
        //this.player = player as Player;
        playerData = data;
        isMVP = isMvp;

        // Set tank icon
        if (tankIcon != null)
        {
            tankIcon.sprite = tankIconSprite;
            tankIcon.gameObject.SetActive(true);
        }
        // if (tankIcon != null && CoreGamePlay.Instance != null)
        // {
        //     var renderIconData = CoreGamePlay.Instance.GetTankIconRenderTexture(player.NetPlayerIndex);
        //     tankIcon.texture = renderIconData.renderTexture;
        //     tankIcon.uvRect = renderIconData.uvRect;
        //     tankIcon.gameObject.SetActive(true);
        // }
        
        // Set player name
        if (playerNameText != null)
        {
            playerNameText.text = data.PlayerName.ToString();
        }
        
        // Set kill count
        if (killCountText != null)
        {
            killCountText.text = playerData.Kill.ToString();
        }

        // Set death count
        if (deathCountText != null)
        {
            deathCountText.text = playerData.Death.ToString();
        }
        
        if (goldText != null)
        {
            goldText.text = playerData.Gold.ToString();
        }
        
        // Show/hide MVP indicator
        if (mvpIndicator != null)
        {
            mvpIndicator.SetActive(isMvp);
        }
        
        if (goMine != null)
        {
            goMine.SetActive(false);
        }
        // if (goAttack != null)
        // {
        //     goAttack.SetActive(false);
        // }
        // if (goDefend != null)
        // {
        //     goDefend.SetActive(false);
        // }
        if (playerData.IsLocalPlayer)
        {
            if (goMine != null)
            {
                goMine.SetActive(true);
            }
        }
        else
        {
            // if (this.player.PlayerTeamIndex == 1)
            // {
            //     if (goAttack)
            //     {
            //         goAttack.SetActive(true);
            //     }
            // }
            // else
            // {
            //     if (goDefend)
            //     {
            //         goDefend.SetActive(true);
            //     }
            // }
        }
    }

    private void OnDisable()
    {
        playerData = null;
        isMVP = false;
    }
}
