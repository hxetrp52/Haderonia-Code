using Haderonia.CameraSystem;
using UnityEngine;

public class NPCCameraInteractionModule : NPCModuleBase
{
    [SerializeField] private CameraPresetData interactPreset;

    [SerializeField] private Transform npcFocusPoint;

    private PlayerCameraPresetController cameraController;


    public override void Initialize(NPCBase npcBase)
    {
        base.Initialize(npcBase);

        cameraController = player.GetPlayerComponent<PlayerCameraPresetController>();

        npcBase.Onfocus += OnInteractionEnter;
        npcBase.Onunfocus += OnInteractionExit;
    }

    public void OnInteractionEnter()
    {
        cameraController.ApplyInteractionPreset(interactPreset, player.gameObject.transform, npcFocusPoint);
    }

    public void OnInteractionExit()
    {
        cameraController.RestoreDefault(interactPreset.blendDuration);
    }
}
