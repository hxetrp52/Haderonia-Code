using UnityEngine;
using UnityEngine.InputSystem;

public interface IOutFitComponent
{
    void Enter(PlayerCombat combat, PlayerStat stat, CharacterOutfitData data);

    void Attack(InputAction.CallbackContext context);

    void Skill(InputAction.CallbackContext context);

    void Exit();
}
