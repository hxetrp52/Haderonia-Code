using UnityEngine;

public interface IPlayerComponent
{
    public void Init(PlayerMain player);

    public void LateInit();
}