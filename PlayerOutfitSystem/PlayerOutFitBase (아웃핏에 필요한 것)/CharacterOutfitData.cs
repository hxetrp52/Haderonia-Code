using UnityEditor;
using UnityEngine;
using EnumTypes;

[CreateAssetMenu(menuName = "Outfit/Character Outfit")]
public class CharacterOutfitData : ScriptableObject // 플레이어 의상의 모든 데이터를 담당
    // 플레이어 의상 새로 제작할 때 따로 만들어둔 커스텀 에디터 이용
{
    [Header("기본 능력치")]
    public int maxHP; // 최대 HP
    public float moveSpeed; // 이동 속도
    public int attackPower; // 공격력
    public float attackSpeed; // 공격 속도
    public float skillCoolTime; //스킬 쿨타임

    [Header("의상 정보")]
    public string outfitName; // 아웃핏 이름
    public string codeName; // 아웃핏 영어 이름

    [Multiline(3)]
    public string outfitDescription;// 아웃핏 설명
    public OutFitType outfitType;

    [Header("외형")]
    public Sprite characterSprite; //대화 시 나올 스프라이트
    public RuntimeAnimatorController characterAnim; // 의상 애니메이션

    public enum OutFitType
    {
        Nomal,
        Magic,
        Physical,
        Fixed,
        Hybrid,
        Support,
        Special
    };

    [Header("공격 정보")]
    public string weaponName;
    public string skillName;


    [TextArea] public string weaponDescription;
    [TextArea] public string skillDescription;

    public Sprite weaponSprite;
    public Sprite skillSprite;

    public AttackDirectionType attackDirectionType; // 공격 방향 타입
    public HapticProfile attackHaptic; // 공격 시 진동 프로필
    public HapticProfile skillHaptic; // 스킬 시 진동 프로필

    [Header("SFX모음")]
    public SFXContainerData sfxData;

    [Header("전투 데이터")] // 커스텀 에디터에서 아웃핏 제작후에 나오는 컴뱃 스크립트에서 무기, 스킬 구현

    [HideInInspector]
    public OutfitCombatBase combatComponent;

    public string combatClassName;
}
