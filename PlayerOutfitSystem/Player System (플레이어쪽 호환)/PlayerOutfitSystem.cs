using UnityEngine;

public class PlayerOutfitSystem : MonoBehaviour, IPlayerComponent
{
    [SerializeField] private CharacterOutfitData currentOutfit;
    [SerializeField] private OutfitCombatBase currentCombat;

    private PlayerMain player;

    private PlayerStat playerStat;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerCombat playerCombat;

    private GameManager gameManager;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
    }

    public void Init(PlayerMain _player)
    {
        player = _player;
        gameManager = player.GetGameManager();
        playerStat = player.GetPlayerComponent<PlayerStat>();
        playerCombat = player.GetPlayerComponent<PlayerCombat>();
    }


    public void LateInit()
    {
        var savedId = SaveDataManager.Instance?.CurrentSave?.player?.currentOutfitId;
        if (!string.IsNullOrEmpty(savedId))
        {
            var savedOutfit = gameManager.GetManager<OutFitDataManager>().GetOutFit(savedId);
            if (savedOutfit != null)
            {
                ChangeOutfit(savedOutfit);
                return;
            }
        }
        ChangeOutfit(currentOutfit);
    }

    public void ChangeOutfit(CharacterOutfitData newOutfit)
    {
        currentOutfit = newOutfit;
        ApplyOutfitData();
        ApplyStatsAndVisual();

        var playerSave = SaveDataManager.Instance?.CurrentSave?.player;
        if (playerSave != null && newOutfit != null)
            playerSave.currentOutfitId = newOutfit.outfitName;
    }

    private void ApplyOutfitData()
    {
        // 기존 Combat 해제
        if (currentCombat != null)
        {
            currentCombat.Exit(); //Exit 함수 작성
            Destroy(currentCombat); //스크립트 삭제
        }

        var type = System.Type.GetType(currentOutfit.combatClassName); // 다음에 가져올 스크립트 정보 찾기

        if (type == null) // 오류 예외 처리
        {
            Debug.LogError($"Combat class not found: {currentOutfit.combatClassName}");
            return;
        }

        currentCombat = gameObject.AddComponent(type) as OutfitCombatBase;
        // Combat 초기화
        currentCombat.Enter(playerCombat, playerStat, currentOutfit); // 그 의상에 필요한 리소스 불러오기

        // PlayerCombat에 등록
        playerCombat.SetOutfitCombat(currentCombat);
        playerCombat.SetOutfitData(currentOutfit);
    }


    private void ApplyStatsAndVisual() // 아웃핏 SO의 데이터 바탕으로 애니메이션, 스탯 변경 함수
    {
        if (currentOutfit == null)
            return;

        playerStat.InitializeStatData(currentOutfit);

        spriteRenderer.sprite = currentOutfit.characterSprite;
        animator.runtimeAnimatorController = currentOutfit.characterAnim;

        gameManager.GetManager<SpriteDataManager>().SetPlayerSprite(spriteRenderer.sprite);
    }

}
