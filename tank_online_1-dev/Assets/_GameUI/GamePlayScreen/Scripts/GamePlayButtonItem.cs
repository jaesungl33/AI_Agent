using Fusion.TankOnlineModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Unity.VisualScripting;

public class GamePlayButtonItem : MonoBehaviour
{
    public AbilityUseType skillType = AbilityUseType.Instant;
    [SerializeField] private int slotIdx;

    [SerializeField] private GameObject goItem;
    [SerializeField] private Transform pnlStack;
    [SerializeField] private Image icon;
    [SerializeField] private Image imgCountDown;
    [SerializeField] private Image imgCountStack;
    [SerializeField] private TMP_Text txtStack;
    [SerializeField] private Button btnClick;

    [SerializeField] private Sprite[] slotIcon;
    private Player player;
    private bool initialized;
    private AbilityBase ability;

    private float range;
    public void Init(Player player)
    {
        this.player = player;
        // if (slotIdx < 0)
        // {
        //     initialized = false;
        //     Hide();
        //     return;
        // }
        Debug.Log("MobileInput|Init|slotIdx: " + slotIdx);
        const int DEFAULT_ATK_SLOT = 0;
        if (slotIdx <= DEFAULT_ATK_SLOT)
        {
            initialized = false;
            return;
        }
        const int REMOVE_NORMALIZE_INDEX = 1;//NOTE: to convert to zero-based index OF ABILITY ARRAY
        ability = player.abilityManager.GetAbilityBySlot(slotIdx - REMOVE_NORMALIZE_INDEX);
        if (!ability)
        {
            initialized = false;
            Hide();
            return;
        }
        var customProperties = ability.GetCustomPropertiesRuntime();
        //set default if not found customProperties.FirstOrDefault
        var rangeProp = customProperties.FirstOrDefault(p => p.propertyName == "Range");
        range = rangeProp != null ? rangeProp.propertyValue : 0f;

        var typeProp = customProperties.FirstOrDefault(p => p.propertyName == "AbilityUseType");
        skillType = typeProp != null ? (AbilityUseType)typeProp.propertyValue : AbilityUseType.Instant;

        initialized = true;

        LoadIcon(ability.abilityID);

        imgCountDown.fillAmount = 0f;
        imgCountStack.fillAmount = 0f;
        txtStack.text = string.Empty;
    }

    public void Update()
    {
        //update countdown
        if (!initialized || ability == null) return;
        var customProperties = ability.GetCustomPropertiesRuntime();

        var cooldownProp = customProperties.FirstOrDefault(p => p.propertyName == "Cooldown");
        var cooldownRemainProp = customProperties.FirstOrDefault(p => p.propertyName == "CooldownRemaining");

        float cooldown = cooldownProp != null ? cooldownProp.propertyValue : 0f;
        float cooldownRemain = cooldownRemainProp != null ? cooldownRemainProp.propertyValue : 0f;

        var coolDownStackProp = customProperties.FirstOrDefault(p => p.propertyName == "CoolDownStack");
        var stackTimerProp = customProperties.FirstOrDefault(p => p.propertyName == "StackTimer");
        var stackProp = customProperties.FirstOrDefault(p => p.propertyName == "Stack");


        float coolDownStack = coolDownStackProp != null ? coolDownStackProp.propertyValue : 0f;
        float stackTimer = stackTimerProp != null ? stackTimerProp.propertyValue : 0f;

        int stack = stackProp != null ? (int)stackProp.propertyValue : 0;
        txtStack.text = stack.ToString();
        pnlStack.gameObject.SetActive(coolDownStack > 0);

        imgCountDown.fillAmount = cooldown > 0 ? Mathf.Clamp01(cooldownRemain / cooldown) : 0f;


        imgCountStack.fillAmount = coolDownStack > 0 ? Mathf.Clamp01(stackTimer / coolDownStack) : 0f;
        //Debug.LogError($"GamePlayButtonItem|Update|abilityId: {ability.abilityID}, coolDownStack: {coolDownStack}, stackTimer: {stackTimer}, stack: {stack}, imgCountStack.fillAmount: {imgCountStack.fillAmount}");
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
    public void LoadIcon(string abilityID)
    {
        if (!initialized) return;
        if (abilityID == null || abilityID == "")
        {
            goItem.SetActive(false);
            return;
        }
        string iconName = string.Format("{0}.icon", abilityID);
        int idx = System.Array.FindIndex(slotIcon, s => s.name == iconName);
        if (idx < 0)
        {
            Debug.LogWarning($"GamePlayButtonItem|LoadIcon|Not found icon for abilityId: {abilityID}");
            goItem.SetActive(false);
            return;
        }
        icon.sprite = slotIcon[idx];
        goItem.SetActive(true);
    }
}