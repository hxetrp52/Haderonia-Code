using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace Haderonia.CameraSystem
{
    public class PlayerCameraPresetController : MonoBehaviour, IPlayerComponent
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera cineCamera;
        [SerializeField] private CinemachineTargetGroup targetGroup;

        private CinemachineFollow _follow;
        private CinemachineRotationComposer _rotationComposer;

        private Coroutine _blendRoutine;

        // default cache
        private float _defaultFov;
        private Vector3 _defaultFollowOffset;
        private Vector3 _defaultTargetOffset;

        // look/follow restore cache
        private Transform _defaultLookAt;
        private Transform _defaultFollow;
        private bool _interactionCameraActive;

        // smooth lookat transition
        private Transform _lookAtBlendPivot;
        private Transform _currentInteractionNpc;

        public void Init(PlayerMain _player)
        {
            cineCamera = GetComponentInChildren<CinemachineCamera>(true);

            _follow = cineCamera.GetComponent<CinemachineFollow>();
            _rotationComposer = cineCamera.GetComponent<CinemachineRotationComposer>();

            if (_lookAtBlendPivot == null)
            {
                var go = new GameObject("Camera_LookAtBlendPivot");
                go.transform.SetParent(transform, false);
                _lookAtBlendPivot = go.transform;
            }

            CacheDefaults();
        }


        public void LateInit() { }
        

        private void CacheDefaults()
        {
            _defaultFov = cineCamera.Lens.FieldOfView;

            if (_follow != null)
                _defaultFollowOffset = _follow.FollowOffset;

            if (_rotationComposer != null)
                _defaultTargetOffset = _rotationComposer.TargetOffset;

            _defaultLookAt = cineCamera.LookAt;
            _defaultFollow = cineCamera.Follow;
        }

        public void ApplyPreset(CameraPresetData preset)
        {
            if (preset == null) return;

            if (_blendRoutine != null) StopCoroutine(_blendRoutine);
            _blendRoutine = StartCoroutine(BlendToPresetRoutine(preset));
        }

        public void ApplyInteractionPreset(CameraPresetData preset, Transform player, Transform npc)
        {
            if (preset == null || player == null || npc == null) return;

            if (!_interactionCameraActive)
            {
                _defaultLookAt = cineCamera.LookAt;
                _defaultFollow = cineCamera.Follow;
            }

            _currentInteractionNpc = npc;
            SetupTargetGroup(player, npc);
            _interactionCameraActive = true;

            ApplyPreset(preset);
        }

        public void RestoreDefault(float duration = 0.2f)
        {
            if (_blendRoutine != null) StopCoroutine(_blendRoutine);
            _blendRoutine = StartCoroutine(RestoreAllRoutine(duration));
        }

        private void SetupTargetGroup(Transform player, Transform npc)
        {
            if (targetGroup == null)
            {
                cineCamera.LookAt = npc;
                cineCamera.Follow = player;
                return;
            }

            targetGroup.Targets = new List<CinemachineTargetGroup.Target>
            {
                new() { Object = player, Weight = 1f, Radius = 0.3f },
                new() { Object = npc, Weight = 1f, Radius = 0.3f }
            };

            cineCamera.LookAt = targetGroup.transform;
            cineCamera.Follow = player;
        }

        private IEnumerator RestoreAllRoutine(float duration)
        {
            duration = Mathf.Max(0.01f, duration);

            float t = 0f;
            float half = duration * 0.5f;

            float startFov = cineCamera.Lens.FieldOfView;
            Vector3 startFollowOffset = _follow != null ? _follow.FollowOffset : Vector3.zero;
            Vector3 startTargetOffset = _rotationComposer != null ? _rotationComposer.TargetOffset : Vector3.zero;

            // LookAt 블렌딩 시작/종료 지점 계산
            Transform restoreLookAtTarget = _defaultLookAt != null ? _defaultLookAt : _defaultFollow;
            Vector3 fromLookPos = GetCurrentInteractionLookPosition();
            Vector3 toLookPos = restoreLookAtTarget != null ? restoreLookAtTarget.position : fromLookPos;

            bool lookBlendStarted = false;

            while (t < duration)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / duration);

                // 1) 기존 기본값 복귀 블렌드(FOV/Offset)
                var lens = cineCamera.Lens;
                lens.FieldOfView = Mathf.Lerp(startFov, _defaultFov, a);
                cineCamera.Lens = lens;

                if (_follow != null)
                    _follow.FollowOffset = Vector3.Lerp(startFollowOffset, _defaultFollowOffset, a);

                if (_rotationComposer != null)
                    _rotationComposer.TargetOffset = Vector3.Lerp(startTargetOffset, _defaultTargetOffset, a);

                // 2) duration 절반부터 LookAt을 천천히 backup target으로 전환
                if (t >= half && restoreLookAtTarget != null)
                {
                    if (!lookBlendStarted)
                    {
                        lookBlendStarted = true;
                        // 시작 시점 LookAt 고정
                        fromLookPos = GetCurrentInteractionLookPosition();
                        _lookAtBlendPivot.position = fromLookPos;
                        cineCamera.LookAt = _lookAtBlendPivot;
                    }

                    float lookT = Mathf.Clamp01((t - half) / half);
                    Vector3 p = Vector3.Lerp(fromLookPos, toLookPos, lookT);
                    _lookAtBlendPivot.position = p;
                }

                yield return null;
            }

            // finalize
            var finalLens = cineCamera.Lens;
            finalLens.FieldOfView = _defaultFov;
            cineCamera.Lens = finalLens;

            if (_follow != null)
                _follow.FollowOffset = _defaultFollowOffset;

            if (_rotationComposer != null)
                _rotationComposer.TargetOffset = _defaultTargetOffset;

            // 최종 원복
            cineCamera.LookAt = _defaultLookAt;
            cineCamera.Follow = _defaultFollow;

            _interactionCameraActive = false;
            _currentInteractionNpc = null;
            _blendRoutine = null;
        }

        private Vector3 GetCurrentInteractionLookPosition()
        {
            // TargetGroup 사용 중이면 그룹 중심
            if (targetGroup != null && cineCamera.LookAt == targetGroup.transform)
                return targetGroup.transform.position;

            // fallback: 현재 NPC
            if (_currentInteractionNpc != null)
                return _currentInteractionNpc.position;

            // 마지막 fallback: 현재 LookAt
            if (cineCamera.LookAt != null)
                return cineCamera.LookAt.position;

            return transform.position + transform.forward * 2f;
        }

        private IEnumerator BlendToPresetRoutine(CameraPresetData preset)
        {
            float duration = Mathf.Max(0.01f, preset.blendDuration);
            float t = 0f;

            float startFov = cineCamera.Lens.FieldOfView;
            Vector3 startFollowOffset = _follow != null ? _follow.FollowOffset : Vector3.zero;
            Vector3 startTargetOffset = _rotationComposer != null ? _rotationComposer.TargetOffset : Vector3.zero;

            float targetFov = preset.fieldOfView;
            Vector3 targetFollowOffset = preset.followOffset;

            Vector3 targetTargetOffset = startTargetOffset;
            if (preset.overrideTargetOffsetYDelta)
                targetTargetOffset.y = startTargetOffset.y - preset.targetOffsetYDelta;

            while (t < duration)
            {
                t += Time.deltaTime;
                float a = Mathf.Clamp01(t / duration);

                var lens = cineCamera.Lens;
                lens.FieldOfView = Mathf.Lerp(startFov, targetFov, a);
                cineCamera.Lens = lens;

                if (_follow != null && preset.overrideFollowOffset)
                    _follow.FollowOffset = Vector3.Lerp(startFollowOffset, targetFollowOffset, a);

                if (_rotationComposer != null && preset.overrideTargetOffsetYDelta)
                    _rotationComposer.TargetOffset = Vector3.Lerp(startTargetOffset, targetTargetOffset, a);

                yield return null;
            }

            var finalLens = cineCamera.Lens;
            finalLens.FieldOfView = targetFov;
            cineCamera.Lens = finalLens;

            if (_follow != null && preset.overrideFollowOffset)
                _follow.FollowOffset = targetFollowOffset;

            if (_rotationComposer != null && preset.overrideTargetOffsetYDelta)
                _rotationComposer.TargetOffset = targetTargetOffset;

            _blendRoutine = null;
        }
    }
}