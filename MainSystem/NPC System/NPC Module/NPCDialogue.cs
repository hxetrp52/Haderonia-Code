using UnityEngine;
using UIStringKey;

public class NPCDialogue : NPCModuleBase
{
    public ChatData chatData;

    public override void Initialize(NPCBase npc)
    {
        base.Initialize(npc);
        npc.Oninteract += ShowDialogue;
    }
    public void ShowDialogue()
    {
        gameManager.GetManager<UIManager>().OpenPanel(UIKey.DialogueUI, chatData);
    }
}
