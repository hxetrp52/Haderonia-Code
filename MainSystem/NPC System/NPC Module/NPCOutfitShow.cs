using UIStringKey;
using UnityEngine;

public class NPCOutfitShow : NPCModuleBase
{
    public override void Initialize(NPCBase npcBase)
    {
        base.Initialize(npcBase);
        npcBase.Oninteract += ShowOutfitSelectUI;
    }

    public void ShowOutfitSelectUI()
    {
        gameManager.GetManager<UIManager>().OpenPopup(UIKey.OwnOutfitShowUI, player);
    }
}
