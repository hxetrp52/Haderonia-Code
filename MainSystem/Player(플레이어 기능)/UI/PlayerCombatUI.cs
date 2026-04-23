using EnumTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCombatUI : MonoBehaviour
{
    [SerializeField] private GameObject combatUICanvas;

    [SerializeField] private Image skillImage;

    [SerializeField] private GameObject skillFullEffect;

    [SerializeField] private Slider hpSlider;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Slider itemCooldownSlider;

    [SerializeField] private GameObject combatFrameTransform;

    [Header("스탯 인터페이스 UI")]
    public StatInterfaceUI statInterfaceUI;

    private PlayerStat playerStat;
    private PlayerCombat playerCombat;
    private PlayerItemBase currentItem;

    public void Initialize(PlayerStat stat, PlayerCombat combat)
    {
        playerStat = stat;
        playerCombat = combat;

        UpdateHpSlider();
        UpdateSkillCooldown();
    }

    public void SetCanvas()
    {
        combatUICanvas.SetActive(true);
    }

    public void SetCombatFrame(GameObject combatFrame)
    {
        RectTransform parentRect = combatFrameTransform.GetComponent<RectTransform>();
        RectTransform frameRect = combatFrame.GetComponent<RectTransform>();

        if (parentRect != null && frameRect != null)
        {
            frameRect.SetParent(parentRect, false);
            frameRect.anchoredPosition = Vector2.zero;
            frameRect.localScale = Vector3.one;
            frameRect.localRotation = Quaternion.identity;
            return;
        }

        combatFrame.transform.SetParent(combatFrameTransform.transform, false);
        combatFrame.transform.localPosition = Vector3.zero;
        combatFrame.transform.localScale = Vector3.one;
        combatFrame.transform.localRotation = Quaternion.identity;
    }

    public void ChangeSkillIcon(Sprite newIcon)
    {
        skillImage.sprite = newIcon;
    }

    public void BindItem(PlayerItemBase item)
    {
        currentItem = item;
        RefreshItemIcon();
        RefreshItemCooldownUI();
    }

    public void RefreshItemCooldownUI()
    {
        itemCooldownSlider.minValue = 0f;
        itemCooldownSlider.maxValue = 1f;
        itemCooldownSlider.value = currentItem != null ? currentItem.GetCooldownRemainingNormalized() : 0f;
    }

    public void UpdateHpSlider()
    {
        Stat maxHpStat = playerStat.GetStat(StatType.MaxHP);
        int maxHp = Mathf.RoundToInt(maxHpStat?.Value ?? 0f);
        int currentHp = playerStat.GetCurrentHp();

        hpSlider.maxValue = maxHp;
        hpSlider.value = currentHp;

        if (hpText != null)
            hpText.text = $"{currentHp}/{maxHp}";
    }

    public void UpdateSkillCooldown()
    {
        float fillAmount = playerCombat.GetSkillCooldownFillAmount();

        skillImage.fillAmount = fillAmount;

        skillFullEffect.SetActive(playerCombat.CanUseSkill());
    }

    private void RefreshItemIcon()
    {
        Sprite itemSprite = currentItem != null ? currentItem.GetItemSprite() : null;
        itemImage.sprite = itemSprite;
        itemImage.enabled = itemSprite != null;
    }
}
