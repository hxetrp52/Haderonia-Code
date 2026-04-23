using UnityEngine;

public class NPCSubtitleSystem : NPCModuleBase
{
    [SerializeField] private string[] subtiles;
    [SerializeField] private int nextSubtitleTime;

    private bool isSubtitleShowing = false;

    public override void Initialize(NPCBase npcBase)
    {
        base.Initialize(npcBase);
        npcBase.Oninteract += ShowSubtitle;
    }

    public void ShowSubtitle()
    {
        if (isSubtitleShowing) return;
        isSubtitleShowing = true;
        gameManager.GetManager<SubtitleManager>().SetAllFinishedCallback(() => {isSubtitleShowing = false;});
        for (int i = 0; i < subtiles.Length; i++)
        {
            gameManager.GetManager<SubtitleManager>().ShowSubtitle(subtiles[i], nextSubtitleTime);
        }
    }
}
