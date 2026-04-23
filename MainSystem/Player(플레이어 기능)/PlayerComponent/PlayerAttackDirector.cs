using UnityEngine;
using EnumTypes;

public class PlayerAttackDirector
{
    public AttackDirectionType currentType;
    public AttackDirection current;

    /// <summary>
    /// 플레이어 이동 방향을 받아 공격 방향 Vector3 반환
    /// </summary>
    public Vector3 GetAttackDirection(Vector3 moveDir)
    {
        // 입력 없으면 기본 전방
        if (moveDir.sqrMagnitude < 0.0001f)
            return Vector3.forward;

        switch (currentType)
        {
            case AttackDirectionType.FourDirection:
                return GetFourDirection(moveDir);

            case AttackDirectionType.TwoDirection:
                return GetTwoDirection(moveDir);

            case AttackDirectionType.AutoTarget:
                return GetAutoTargetDirection(); // 일단 분리

            default:
                return Vector3.back;
        }
    }

    #region Four Direction (앞/뒤/좌/우)
    private Vector3 GetFourDirection(Vector3 moveDir)
    {
        if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.z))
        {
            // 좌 / 우
            if (moveDir.x > 0)
            {
                current = AttackDirection.Right;
                return Vector3.right;
            }
            else
            {
                current = AttackDirection.Left;
                return Vector3.left;
            }
        }
        else
        {
            // 앞 / 뒤
            if (moveDir.z > 0)
            {
                current = AttackDirection.Front;
                return Vector3.forward;
            }
            else
            {
                current = AttackDirection.Back;
                return Vector3.back;
            }
        }
    }
    #endregion

    #region Two Direction (앞 / 뒤)
    private Vector3 GetTwoDirection(Vector3 moveDir)
    {
        if (moveDir.z >= 0)
        {
            current = AttackDirection.Front;
            return Vector3.forward;
        }
        else
        {
            current = AttackDirection.Back;
            return Vector3.back;
        }
    }
    #endregion

    #region Auto Target
    private Vector3 GetAutoTargetDirection()
    {
        // TODO: 타겟 기준 방향 계산
        return Vector3.forward;
    }
    #endregion
}
