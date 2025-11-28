using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System;

public class TankUI : MonoBehaviour
{
    [Header("HP References")]
    [SerializeField] private Image realHPImg;      // HP hiện tại (màu đậm)
    [SerializeField] private Image virtualHPImg;   // HP ảo (màu nhạt, delayed)
    
    [Header("UI References")]
    [SerializeField] private Image[] stars;
    [SerializeField] private Image avatarImg, panelBgImg, starsBgImg;
    [SerializeField] private TMP_Text nameText, levelText, damageText, revivalText;
    
    [Header("Settings")]
    [SerializeField] private float realHPAnimDuration = 0.15f;
    [SerializeField] private float virtualHPDelay = 0.3f;
    [SerializeField] private float virtualHPAnimDuration = 0.25f;
    private Ease realHPEase = Ease.OutQuad;
    private Ease virtualHPEase = Ease.OutCubic;
    
    [Header("Colors")]
    [SerializeField] private Color[] teamColors;
    [SerializeField] private Color[] panelBgColors;
    [SerializeField] private Color[] starsBgColors;
    
    [Header("Components")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private DamageVisual damageVisual;
    [SerializeField] private GameObject[] objectsToHideWhenDead;
    [SerializeField] private HPLineComp hpLineComp;

    private int playerLevel = 0;
    private Sequence currentHPSequence; // Cache sequence để kill khi cần
    private int lastLife = -1;
    private int lastMaxLife = -1;

    private void Awake()
    {
        ValidateComponents();
        SetDefaultInfo();
    }

    private void OnDestroy()
    {
        // Cleanup DOTween sequences
        KillHPSequence();
    }

    private void ValidateComponents()
    {
        if (realHPImg == null || virtualHPImg == null)
        {
            Debug.LogError("[TankUI] Real HP or Virtual HP Image is not assigned!");
        }

        if (canvas == null)
        {
            Debug.LogError("[TankUI] Canvas is not assigned!");
        }
    }

    #region HP System

    /// <summary>
    /// Update HP with smooth animation
    /// Real HP updates immediately, Virtual HP follows with delay
    /// </summary>
    public void ChangeHP(int life, int maxLife)
    {
        // Validate inputs
        if (maxLife <= 0)
        {
            Debug.LogWarning("[TankUI] MaxLife must be greater than 0");
            maxLife = 1;
        }

        life = Mathf.Clamp(life, 0, maxLife);

        // Skip if no change
        if (life == lastLife && maxLife == lastMaxLife)
            return;

        float targetFillAmount = (float)life / maxLife;
        float currentRealHP = realHPImg.fillAmount;

        // Determine if taking damage or healing
        bool isTakingDamage = targetFillAmount < currentRealHP;

        // Kill previous sequence to prevent conflicts
        KillHPSequence();

        // Create new sequence
        currentHPSequence = DOTween.Sequence();

        if (isTakingDamage)
        {
            // ===== TAKING DAMAGE =====
            // Real HP drops immediately
            // Virtual HP follows after delay
            currentHPSequence
                .Append(realHPImg.DOFillAmount(targetFillAmount, realHPAnimDuration).SetEase(realHPEase))
                .AppendInterval(virtualHPDelay)
                .Append(virtualHPImg.DOFillAmount(targetFillAmount, virtualHPAnimDuration).SetEase(virtualHPEase))
                .OnComplete(() => OnHPAnimationComplete(life, maxLife));
        }
        else
        {
            // ===== HEALING =====
            // Virtual HP rises immediately
            // Real HP follows after delay
            currentHPSequence
                .Append(virtualHPImg.DOFillAmount(targetFillAmount, realHPAnimDuration).SetEase(realHPEase))
                .AppendInterval(virtualHPDelay)
                .Append(realHPImg.DOFillAmount(targetFillAmount, virtualHPAnimDuration).SetEase(virtualHPEase))
                .OnComplete(() => OnHPAnimationComplete(life, maxLife));
        }

        currentHPSequence.Play();

        // Update cache
        lastLife = life;
        lastMaxLife = maxLife;

        Debug.Log($"[TankUI] HP changed: {life}/{maxLife} ({targetFillAmount:P0}) - {(isTakingDamage ? "Damage" : "Heal")}");
    }

    /// <summary>
    /// Set HP immediately without animation (for initialization)
    /// </summary>
    public void SetHPImmediate(int life, int maxLife)
    {
        if (maxLife <= 0)
        {
            Debug.LogWarning("[TankUI] MaxLife must be greater than 0");
            maxLife = 1;
        }

        life = Mathf.Clamp(life, 0, maxLife);
        float fillAmount = (float)life / maxLife;

        KillHPSequence();

        realHPImg.fillAmount = fillAmount;
        virtualHPImg.fillAmount = fillAmount;

        lastLife = life;
        lastMaxLife = maxLife;

        Debug.Log($"[TankUI] HP set immediately: {life}/{maxLife} ({fillAmount:P0})");
    }

    /// <summary>
    /// Kill current HP animation sequence
    /// </summary>
    private void KillHPSequence()
    {
        if (currentHPSequence != null && currentHPSequence.IsActive())
        {
            currentHPSequence.Kill();
            currentHPSequence = null;
        }
    }

    /// <summary>
    /// Called when HP animation completes
    /// </summary>
    private void OnHPAnimationComplete(int life, int maxLife)
    {
        currentHPSequence = null;

        // Handle death
        if (life <= 0)
        {
            OnPlayerDeath();
        }

        Debug.Log($"[TankUI] HP animation completed: {life}/{maxLife}");
    }

    /// <summary>
    /// Handle player death
    /// </summary>
    private void OnPlayerDeath()
    {
        Debug.Log("[TankUI] Player died");
        EnableUI(false);
    }

    #endregion

    #region UI Management

    public void EnableUI(bool enable)
    {
        foreach (var obj in objectsToHideWhenDead)
        {
            if (obj != null)
                obj.SetActive(enable);
        }

        if (enable)
        {
            ResetUI();
        }
    }

    private void ResetUI()
    {
        UpdateKillStreak(0);
        UpdateRevivalText(0);
    }

    public void UpdateRevivalText(int count)
    {
        if (revivalText != null)
        {
            revivalText.gameObject.SetActive(count > 0);
            revivalText.text = count > 0 ? $"Respawn in: {count}" : "";
        }
    }

    public void UpdateLevel()
    {
        playerLevel++;
        if (levelText != null)
            levelText.text = playerLevel.ToString();
    }

    public void UpdateKillStreak(int kill)
    {
        if (stars == null) return;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
                stars[i].gameObject.SetActive(i < kill);
        }
    }

    public void ShowTextDamage(int damage)
    {
        if (damage > 0 && damageVisual != null)
        {
            damageVisual.ShowText(damage.ToString(), Color.red);
        }
    }

    internal void SetDefaultInfo()
    {
        if (canvas != null)
            canvas.worldCamera = Camera.main;

        if (levelText != null)
            levelText.text = "0";

        playerLevel = 0;

        if (nameText != null)
            nameText.text = "";

        SetHPImmediate(100, 100); // Initialize with 100% HP
    }

    public void SetPlayerName(string playerName)
    {
        if (nameText != null)
            nameText.text = playerName;
    }

    public void SetTeam(int teamIndex)
    {
        if (teamIndex < 0 || teamIndex >= teamColors.Length)
        {
            Debug.LogError($"[TankUI] Invalid team index: {teamIndex}");
            return;
        }

        if (realHPImg != null)
            realHPImg.color = teamColors[teamIndex];

        if (panelBgImg != null)
            panelBgImg.color = panelBgColors[teamIndex];

        if (starsBgImg != null)
            starsBgImg.color = starsBgColors[teamIndex];
    }

    public void SetLineDecorHP(int maxHp)
    {
        if (hpLineComp != null)
        {
            hpLineComp.SetByHP(maxHp);
        }
    }

    #endregion

    #region Debug

    [ContextMenu("Test Take Damage")]
    private void TestTakeDamage()
    {
        int currentHP = Mathf.RoundToInt(realHPImg.fillAmount * 100);
        ChangeHP(Mathf.Max(0, currentHP - 20), 100);
    }

    [ContextMenu("Test Heal")]
    private void TestHeal()
    {
        int currentHP = Mathf.RoundToInt(realHPImg.fillAmount * 100);
        ChangeHP(Mathf.Min(100, currentHP + 20), 100);
    }

    [ContextMenu("Test Death")]
    private void TestDeath()
    {
        ChangeHP(0, 100);
    }

    [ContextMenu("Test Full Heal")]
    private void TestFullHeal()
    {
        ChangeHP(100, 100);
    }

    #endregion
}
