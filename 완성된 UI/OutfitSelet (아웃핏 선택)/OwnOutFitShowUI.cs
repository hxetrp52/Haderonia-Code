using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;
using System;

public class OwnOutFitShowUI : UIPopup , IUiSetup<PlayerMain>
{

    [Header("버튼 프리팹")]
    [SerializeField] private GameObject outfitButtonPrefab;
    [SerializeField] private Transform content;

    private List<OutFitButton> buttonPool = new List<OutFitButton>();
    

    private UserDataManager userDataManager;

    private PlayerMain player;
    private OutFitDataManager outFitDataManager;

    private PlayerOutfitSystem playerOutfit;
    private CharacterOutfitData lastOutfit = null;

    [SerializeField] private OutfitDescriptionUI descriptionUI;



    private void Initialize()
    {
        if (isInitialize) return;
        userDataManager = gameManager.GetManager<UserDataManager>();
        outFitDataManager = gameManager.GetManager<OutFitDataManager>();
        descriptionUI.playerOutfitSystem = playerOutfit;
        descriptionUI.gameObject.SetActive(false);

        CreateAllButtons();

        isInitialize = true;
    }

    public override void OnShow(Action<UIBase> finished = null)
    {
        base.OnShow(finished);
        Initialize();
        EventSystem.current.SetSelectedGameObject(buttonPool[0].gameObject);
        RefreshButtons();
    }
    private void CreateAllButtons()
    {
        foreach (var outfit in outFitDataManager.GetAllOutfits())
        {
            GameObject obj = Instantiate(outfitButtonPrefab, content);
            OutFitButton btn = obj.GetComponent<OutFitButton>();
            btn.Initialize(outfit, this);

            obj.SetActive(false);  // 보유 여부에 따라 활성화됨
            buttonPool.Add(btn);
        }

        RefreshButtons();
    }

    public void RefreshButtons()
    {
        foreach (var btn in buttonPool)
        {
            bool owned = userDataManager.HaveOutfit(btn.OutfitData);
            btn.gameObject.SetActive(owned);
        }
    }

    public void SelectOutfit(CharacterOutfitData data)
    {
        descriptionUI.gameObject.SetActive(true);
        descriptionUI.Initialize();
        descriptionUI.UpdateUI(data);
    }


    public void Setup(PlayerMain playerMain)
    {
        player = playerMain;
        playerOutfit = player.GetPlayerComponent<PlayerOutfitSystem>();
    }

}
