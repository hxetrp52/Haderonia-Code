using UnityEngine;
using EnumTypes;

public static class DamageCalculator // 데미지 계산기 
{
    // 크리티컬 확률 계산 0%~ 100%로 계산
    public static bool RollCritical(float criticalChance)
    {
        // 범위 보정
        criticalChance = Mathf.Clamp(criticalChance, 0f, 100f);

        float rand = Random.Range(0f, 100f);
        return rand < criticalChance;
    }


    //데미지 계산
    public static int CalculateDamage(
        int baseDamage, // 기본 데미지
        DamageType damageType, // 데미지 타입
        int defaultDef, // 기본 방어력
        int physicalDef, // 물리 방어력
        int magicDef, // 마법 방어력 
        bool isCritical = false) // 크리티컬 여부
    {
        int defence = defaultDef;

        switch (damageType)
        {
            case DamageType.Physical:
                defence += physicalDef;
                break;

            case DamageType.Magic:
                defence += magicDef;
                break;

            case DamageType.Normal:
                break;

            case DamageType.Fixed:
                defence = 0; 
                break;
        }

        // 계산
        int finalDamage = baseDamage - defence;

        // 최소 데미지는 1
        if (finalDamage < 1)
            finalDamage = 1;

        // 크리티컬 시 2배
        if (isCritical)
            finalDamage *= 2;

        return finalDamage;
    }
}
