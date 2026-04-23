using UnityEngine;

public class PlayerAnimator : MonoBehaviour, IPlayerComponent
{
    [SerializeField] private Animator playerAnimator;
    private readonly int MoveX = Animator.StringToHash("Xdir");
    private readonly int MoveZ = Animator.StringToHash("Zdir");
    private readonly int IsWalk = Animator.StringToHash("isWalk");
    private readonly int IsBack = Animator.StringToHash("isBack");
    private readonly int AttackXdir = Animator.StringToHash("AttackXdir");
    private readonly int AttackZdir = Animator.StringToHash("AttackZdir");
    private readonly int AttackTrigger = Animator.StringToHash("Attack");
    private readonly int RollingTrigger = Animator.StringToHash("Rolling");

    public void Init(PlayerMain _player)
    {

    }

    public void LateInit()
    {

    }



    private bool isBack = true;

    public void ControlMoveAnimation(Vector3 moveDir)
    {
        float x = moveDir.x;
        float z = moveDir.z;
        if (z > 0)
        {
            isBack = true;

        }
        else if (z < 0)
        {
            isBack = false;
        }

        playerAnimator.SetFloat(MoveX, x);
        playerAnimator.SetFloat(MoveZ, z);


        if (Mathf.Approximately(x, 0f) && Mathf.Approximately(z, 0f))
        {
            playerAnimator.SetBool(IsWalk, false);
        }
        else
        {
            playerAnimator.SetBool(IsWalk, true);
        }
        playerAnimator.SetBool(IsBack, isBack);
    }

    public void PlayAttackAnimation(Vector3 attackDir = default)
    {
        playerAnimator.SetFloat(AttackXdir, attackDir.x);
        playerAnimator.SetFloat(AttackZdir, attackDir.z);
        playerAnimator.SetTrigger(AttackTrigger);
    }

    public void PlayRollingAnimation()
    {
        playerAnimator.SetTrigger(RollingTrigger);
    }

    public void StopAnimation()
    {
       playerAnimator.SetBool(IsWalk, false);
    }

}
