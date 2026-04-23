using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OutFitButton : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button button;

    public CharacterOutfitData OutfitData { get; private set; }

    private OwnOutFitShowUI ownOutFitShowUI;

    public void Initialize(CharacterOutfitData data, OwnOutFitShowUI ui)
    {
        OutfitData = data;
        ownOutFitShowUI = ui;

        titleText.text = data.outfitName;

        button.onClick.AddListener(() =>
        {
            ownOutFitShowUI.SelectOutfit(OutfitData);
        });
    }
}
