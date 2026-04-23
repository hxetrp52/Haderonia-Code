using DamageNumbersPro;
using EnumTypes;
using UnityEngine;

public class DamagePopupManager : ManagerBase
{
    [Header("데미지 팝업")]
    [SerializeField] private DamageNumber nomalDamageNumber;
    [SerializeField] private DamageNumber physicalDamageNumber;
    [SerializeField] private DamageNumber magicDamageNumber;
    [SerializeField] private DamageNumber fixedDamageNumber;
    [SerializeField] private DamageNumber HealDamageNumber;

    public override void Init()
    {

    }
    public override void ManagerUpdate()
    {
        
    }

    public void SpawnPopup(float number, GameObject target, DamageType damageType = DamageType.Normal, bool isCritical = false)
    {
        switch (damageType)
        {
            case DamageType.Physical:
                CreatePopup(number, physicalDamageNumber, isCritical, target);
                break;

            case DamageType.Magic:
                CreatePopup(number, magicDamageNumber, isCritical, target);
                break;

            case DamageType.Normal:
                CreatePopup(number, nomalDamageNumber, isCritical, target);
                break;

            case DamageType.Fixed:
                CreatePopup(number, fixedDamageNumber, isCritical, target);
                break;
            case DamageType.Heal:
                CreatePopup(number, HealDamageNumber, isCritical, target);
                break;
        }
    }

    private void CreatePopup(float number, DamageNumber damageNumber, bool isCritical, GameObject target)
    {
        string normalText = $"<sprite=0>{number}";
        string criticalText = $"<sprite=0>{number}!!!";
        Vector3 meshSpawnPoint = target.transform.position + new Vector3(0, 0, -0.1f);
        if (!isCritical) // 크리티컬이 아닐때
        {
            damageNumber.Spawn(meshSpawnPoint, normalText);
        }
        else // 크리티컬 일 때
        {
            DamageNumber newpopup;
            newpopup = damageNumber.Spawn(meshSpawnPoint, criticalText);
            newpopup.SetScale(1.2f);
        }
    }

    private void CreatePopup(float number, DamageNumber damageNumber, bool isCritical, RectTransform spawnPoint, Vector2 anchoredPosition)
    {
        string normalText = $"<sprite=0>{number}";
        string criticalText = $"<sprite=0>{number}!!!";
        if (!isCritical) // 크리티컬이 아닐때
        {
            damageNumber.SpawnGUI(spawnPoint, anchoredPosition, normalText);
        }
        else // 크리티컬 일 때
        {
            DamageNumber newpopup;
            newpopup = damageNumber.SpawnGUI(spawnPoint, anchoredPosition, criticalText);
            newpopup.SetScale(1.2f);
        }
    }
}
