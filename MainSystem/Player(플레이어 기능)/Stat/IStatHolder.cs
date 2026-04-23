using EnumTypes;

public interface IStatHolder
{
    // 요청한 스탯을 반환합니다. 존재하지 않으면 null 반환.
    Stat GetStat(StatType statType);
}