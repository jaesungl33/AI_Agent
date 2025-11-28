using System.Collections;
using System.Linq;
using DG.Tweening;
using Fusion.GameSystems;
using Fusion.TankOnlineModule;
using FusionHelpers;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text txtName;
    [SerializeField] private Image imgAvatar;
    [SerializeField] private Sprite defaultAvatar;
    [SerializeField] private Sprite[] avatarSprites;

    [SerializeField] private Image classIcon;
    [SerializeField] private Image classBackground;
    [SerializeField] private Image dimIcon;
    [SerializeField] private TMP_Text txtCountdown;

    [SerializeField] private TMP_Text txtKills;
    [SerializeField] private TMP_Text txtDeaths;
    [SerializeField] private TMP_Text txtHp;
    [SerializeField] private TankIconUI tankIcon;
    [SerializeField] private GameObject goMvp;
    [SerializeField] private Slider sliderHp;

    [SerializeField] private AssetContainer assetContainer;

    private MatchPlayerData _player;
    private bool _isMine = false;
    private float respawnTime = 0f;
    public void Init(MatchPlayerData player, bool isMine)
    {
        _player = player;
        _isMine = isMine;

        //UpdateAvatar(player.AvatarId);
        UpdateTankInfo(player.TankId);
        dimIcon?.gameObject.SetActive(false);
        respawnTime = player.RespawnInSeconds;
        
        // // Set kills
        // if (txtKills != null)
        //     txtKills.text = player.Kill.ToString();

        // // Set deaths
        // if (txtDeaths != null)
        //     txtDeaths.text = player.Death.ToString();

        // // Set HP text
        // if (txtHp != null)
        //     txtHp.text = player.HP.ToString();

        // // Set HP slider
        // if (sliderHp != null)
        // {
        //     sliderHp.value = player.HP;
        //     sliderHp.maxValue = player.MaxHitpoints;
        //     // If maxValue is not initialized, wait and re-init (uncomment if needed)
        //     // if (sliderHp.maxValue < 1f)
        //     // {
        //     //     StartCoroutine(DoWaitInit());
        //     // }
        // }

        // // Hide MVP by default
        // if (goMvp != null)
        //     goMvp.SetActive(false);
    }
    public void UpdateAvatar(string avatarId)
    {
        if (imgAvatar == null)
            return;

        int avatarIndex;
        bool hasValidIndex = int.TryParse(avatarId, out avatarIndex) && avatarSprites != null && avatarSprites.Length > 0 && avatarIndex >= 0 && avatarIndex < avatarSprites.Length;
        if (hasValidIndex)
        {
            imgAvatar.sprite = avatarSprites[avatarIndex];
        }
        else
        {
            imgAvatar.sprite = defaultAvatar; // TODO: Add avatar player
        }
    }
    
    public void UpdateTankInfo(string tankId)
    {
        if (string.IsNullOrEmpty(tankId))
            return;

        DatabaseManager.GetDB<GameAssetCollection>(result =>
        {
            if (result == null)
                return;

            var doc = result.GetGameAssetDocumentById(tankId);
            if (doc == null || doc.sprites == null)
                return;

            // Use LINQ to find the first class icon
            var classIconSprite = doc.sprites.FirstOrDefault(s => s.type == AssetType.ClassIcon)?.asset as Sprite;
            if (classIcon && classIconSprite)
            {
                classIcon.sprite = classIconSprite;
            }

            var tankIconSprite = doc.sprites.FirstOrDefault(s => s.type == AssetType.PreviewIcon)?.asset as Sprite;
            if (imgAvatar && tankIconSprite)
            {
                imgAvatar.sprite = tankIconSprite;
            }
        });

        var tankCollection = DatabaseManager.GetDB<TankCollection>();
        var tank = tankCollection.GetTankByID(tankId);
        assetContainer.TryGet<Sprite>(tank.tankType.ToString(), out Sprite tankClassData);
        if (classBackground && tankClassData)
        {
            classBackground.sprite = tankClassData;
        }
    }

    private void Update()
    {
        // if (respawnTime <= 0) return;
        // if (respawnTime > 0)
        //     respawnTime -= Time.deltaTime;
        // txtCountdown.text = respawnTime > 0 ? respawnTime.ToString("F1") : "";
        // if (respawnTime > 0 && dimIcon != null && !dimIcon.gameObject.activeSelf)
        //     dimIcon?.gameObject.SetActive(true);
        // if (respawnTime <= 0 && dimIcon != null && dimIcon.gameObject.activeSelf)
        //     dimIcon?.gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        EventManager.Register<MatchPlayerData>(OnPlayerStatsUpdate);
    }

    private void OnDisable()
    {
        if (!GameManager.IsApplicationQuitting)
            EventManager.Unregister<MatchPlayerData>(OnPlayerStatsUpdate);
    }

    // private IEnumerator DoWaitInit()
    // {
    //     yield return new WaitUntil(() => CoreGamePlay.Instance.GetPlayerByIndex<Player>(_player.NetPlayerIndex).NetMaxHP >= 1f);
    //     Init(_player, _isMine);
    // }

    private void OnPlayerStatsUpdate(MatchPlayerData stats)
    {
        if (_player == null || stats == null) return;
        if (_player.IndexInTeam < 0 && _player.IndexInTeam != stats.IndexInTeam)
            return;
        respawnTime = stats.RespawnInSeconds;
        //if (txtKills != null) txtKills.text = stats.Kill.ToString();
        //if (txtDeaths != null) txtDeaths.text = stats.Death.ToString();
        //if (txtHp != null) txtHp.text = stats.HP.ToString();
        //if (sliderHp != null) sliderHp.value = stats.HP;
        //if (goMvp != null) goMvp.SetActive(false);  ///TODO: Add MVP
    }
}