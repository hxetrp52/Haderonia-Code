using UnityEngine;

public abstract class NPCModuleBase : MonoBehaviour
{
    protected NPCBase npcBase;
    protected GameManager gameManager;
    protected PlayerMain player;

    public virtual void Initialize(NPCBase npcBase) 
    {
        this.npcBase = npcBase;
        this.gameManager = npcBase.gameManager;
        this.player = npcBase.player;
    }

    

}
