namespace EnumTypes // enum 타입 모아두는 곳
{
    public enum InputDeviceType // 현재 사용중인 디바이스
    {
        KeyboardMouse,
        Xbox,
        PlayStation,
        Nintendo
    }

    public enum QuestType
    {
        Normal,
        Boss
    }
    public enum Boss
    {
        None,
        Dummy,
        TrashMosnster,
    }

    public enum BossState // 보스 상태 분류
    {
        idle,       // 대기
        battle,     // 공격
        downTime,   // 다운타임(ex. 컷씬, 무적 상태 상태 등)
        die         // 죽었을 때
    }
    public enum DamageType // 데미지 타입 분류
    {
        Physical,   // 물리
        Magic,      // 마법
        Normal,     // 일반
        Fixed,      // 고정
        Heal        // 회복
    }
    public enum FireDirection // 4방향 공격 시 발사 분류
    {
        Front,
        Back,
        Left,
        Right
    }

    public enum BuffStackType
    {
        Stack,          // 스택만 증가
        Refresh,        // 시간만 갱신
        StackRefresh    // 스택 증가 + 시간 갱신
    }

    public enum BuffCategory
    {
        Buff,       // 이로운 효과
        Debuff      // 해로운 효과
    }

    public enum StatType // 플레이어 스탯 타입 
    {
        MaxHP,
        CurrentHP,
        MoveSpeed,
        AttackPower,
        AttackSpeed,
        SkillCooldown
    }

    public enum AttackDirection // 공격 방향
    {
        Left,
        Right,
        Front,
        Back
    }

    public enum AttackDirectionType // 공격 방향 타입
    {
       FourDirection,
       TwoDirection,
       AutoTarget
    }
}
