using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutfitDescriptionUI : MonoBehaviour
{
    [Header("공통 UI")]
    [SerializeField] private TMP_Text DescriptionNameText;
    [SerializeField] private TMP_Text DescriptionText;
    [Header("아웃핏 정보 UI")]
    [SerializeField] private Image outfitSpriteUI;
    [SerializeField] private Button outfitButton;

    [SerializeField] private Image outfitAttackTypeImage;

    [Header("무기 정보 UI")]
    [SerializeField] private Image weaponIconUI;
    [SerializeField] private Button weaponButton;

    [Header("스킬 정보 UI")]
    [SerializeField] private Image skillIconUI;
    [SerializeField] private Button skillButton;

    [Header("선택 버튼")]
    [SerializeField] private Button selectButton;

    [HideInInspector] public PlayerOutfitSystem playerOutfitSystem;
    private CharacterOutfitData lastOutfit;

    private bool isInitialized = false;

    public void Initialize()
    {
        if (isInitialized) return;

        outfitButton.onClick.AddListener(ShowOutFitDescription);
        weaponButton.onClick.AddListener(ShowWeaponDescription);
        skillButton.onClick.AddListener(ShowSkillDescription);
        selectButton.onClick.AddListener(ChagePlayerOutfit);
        isInitialized = true;
    }


    public void UpdateUI(CharacterOutfitData data)
    {
        // Outfit 정보
        outfitSpriteUI.sprite = data.characterSprite;
        DescriptionText.text = data.outfitDescription;

        // 무기 정보
        weaponIconUI.sprite = data.weaponSprite;

        skillIconUI.sprite = data.skillSprite;

        lastOutfit = data;
    }

    private void ShowOutFitDescription()
    {
        DescriptionNameText.text = "- 의상 설명 -";
        DescriptionText.text = lastOutfit.outfitDescription;
    }
    private void ShowWeaponDescription()
    {
        DescriptionNameText.text = "- 무기 설명 -";
        DescriptionText.text = lastOutfit.weaponDescription;
    }
    private void ShowSkillDescription()
    {
        DescriptionNameText.text = "- 스킬 설명 -";
        DescriptionText.text = lastOutfit.skillDescription;
    }


    private void ChagePlayerOutfit()
    {
        playerOutfitSystem.ChangeOutfit(lastOutfit);
    }
}
