using EnumTypes;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class InputModeManager : ManagerBase
{
    public InputDeviceType CurrentDevice { get; private set; }
    public event Action<InputDeviceType> OnDeviceChanged;

    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private InputActionReference moveAction; // 왼쪽 스틱
    [SerializeField] private InputActionReference lookAction; // 오른쪽 스틱
    [SerializeField] private float axisThreshold = 0.2f;

    private IDisposable _buttonListener;

    public override void Init()
    {
        CurrentDevice = InputDeviceType.KeyboardMouse;
    }

    private void OnEnable()
    {
        // 유니티 뉴인풋에서 지원하는 아무 버튼이나 눌렸을 때 호출되는 이벤트에 감지 함수 등록
        _buttonListener = InputSystem.onAnyButtonPress.Call(OnAnyButtonPressed);

        moveAction.action.performed += OnAxisPerformed;
        moveAction.action.Enable();

        lookAction.action.performed += OnAxisPerformed;
        lookAction.action.Enable();
    }

    private void OnDisable()
    {
        _buttonListener?.Dispose();

        moveAction.action.performed -= OnAxisPerformed;

        lookAction.action.performed -= OnAxisPerformed;
    }

    public override void ManagerUpdate()
    {

    }

    private void OnAnyButtonPressed(InputControl control) // 버튼이 눌렸을 때 호출되는 함수
    {
        if (control == null) return;
        NotifyInput(control.device);
    }

    private void OnAxisPerformed(InputAction.CallbackContext context)
    {
        if (context.control?.device == null) return;

        if (context.valueType == typeof(Vector2))
        {
            Vector2 v = context.ReadValue<Vector2>();
            if (v.sqrMagnitude < axisThreshold * axisThreshold) // 스틱 데드존 설정(스틱 쏠림이 심한 패드에서 계속 입력되는거 방지하기 위해)
                return;
        }

        NotifyInput(context.control.device);
    }

    public void NotifyInput(InputDevice device)
    {
        var newDevice = Detect(device);
        if (newDevice == CurrentDevice) return;

        CurrentDevice = newDevice;
        OnDeviceChanged?.Invoke(CurrentDevice);
    }

    private InputDeviceType Detect(InputDevice device) // 디바이스 감지하는 함수
    {
        if (device is Keyboard || device is Mouse)
            return InputDeviceType.KeyboardMouse;

        if (device is Gamepad)
        {
            // 디바이스의 제조사 정보를 바탕으로 플레이스테이션, 닌텐도, 엑스박스 구분
            string desc = device.description.manufacturer?.ToLower(); // 제대로된 비교를 위해 소문자 변환

            if (!string.IsNullOrEmpty(desc)) // 
            {
                // 소니(듀얼 센스, 듀얼 쇼크 등)
                if (desc.Contains("sony")) return InputDeviceType.PlayStation;
                // 닌텐도(스위치 프로콘)
                if (desc.Contains("nintendo")) return InputDeviceType.Nintendo;
            }
            // 그 외엔 일단 엑박컨으로 디폴트 설정
            return InputDeviceType.Xbox;
        }

        return InputDeviceType.KeyboardMouse;
    }
}