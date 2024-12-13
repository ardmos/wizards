using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 기존 InputSystem의 OnScreenSick에 다이나믹 터치 기능을 추가한 커스텀 스크립트 입니다.
/// </summary>

public class CustomOnScreenStickWithDynamicTouch : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private const string kDynamicOriginClickable = "DynamicOriginClickable";

    /// <summary>
    /// Callback to handle OnPointerDown UI events.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (m_UseIsolatedInputActions)
            return;

        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        BeginInteraction(eventData.position, eventData.pressEventCamera);
    }

    /// <summary>
    /// Callback to handle OnDrag UI events.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (m_UseIsolatedInputActions)
            return;

        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        MoveStick(eventData.position, eventData.pressEventCamera);
    }

    /// <summary>
    /// Callback to handle OnPointerUp UI events.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (m_UseIsolatedInputActions)
            return;

        EndInteraction();
    }

    private void Start()
    {
        if (m_UseIsolatedInputActions)
        {
            // avoid allocations every time the pointer down event fires by allocating these here
            // and re-using them
            m_RaycastResults = new List<RaycastResult>();
            m_PointerEventData = new PointerEventData(EventSystem.current);

            // if the pointer actions have no bindings (the default), add some
            if (m_PointerDownAction == null || m_PointerDownAction.bindings.Count == 0)
            {
                if (m_PointerDownAction == null)
                    m_PointerDownAction = new InputAction();

                m_PointerDownAction.AddBinding("<Mouse>/leftButton");
                m_PointerDownAction.AddBinding("<Pen>/tip");
                m_PointerDownAction.AddBinding("<Touchscreen>/touch*/press");
                m_PointerDownAction.AddBinding("<XRController>/trigger");
            }

            if (m_PointerMoveAction == null || m_PointerMoveAction.bindings.Count == 0)
            {
                if (m_PointerMoveAction == null)
                    m_PointerMoveAction = new InputAction();

                m_PointerMoveAction.AddBinding("<Mouse>/position");
                m_PointerMoveAction.AddBinding("<Pen>/position");
                m_PointerMoveAction.AddBinding("<Touchscreen>/touch*/position");
            }

            m_PointerDownAction.started += OnPointerDown;
            m_PointerDownAction.canceled += OnPointerUp;
            m_PointerDownAction.Enable();
            m_PointerMoveAction.Enable();
        }

        m_StartPos = realJoystickHandle.anchoredPosition;

        if (m_Behaviour != Behaviour.ExactPositionWithDynamicOrigin) return;
        m_PointerDownPos = m_StartPos;

        var dynamicOrigin = new GameObject(kDynamicOriginClickable, typeof(Image));
        dynamicOrigin.transform.SetParent(transform);
        var image = dynamicOrigin.GetComponent<Image>();
        image.color = new Color(1, 1, 1, 0);
        var rectTransform = (RectTransform)dynamicOrigin.transform;
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.sizeDelta = new Vector2(0, 0);
        rectTransform.localScale = new Vector3(1, 1, 0);
        rectTransform.anchoredPosition3D = Vector3.zero;
    }

    /// <summary>
    /// 유저가 화면(정해진 인식 범위. 여기서는 canvasRect 변수 입니다.)을 터치했을 때 동작하는 메소드 입니다. 
    /// </summary>
    private void BeginInteraction(Vector2 pointerPosition, Camera uiCamera)
    {
        var canvasRect = transform.parent?.GetComponentInParent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("OnScreenStick needs to be attached as a child to a UI Canvas to function properly.");
            return;
        }

        switch (m_Behaviour)
        {
            case Behaviour.RelativePositionWithStaticOrigin:
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, uiCamera, out m_PointerDownPos);
                break;
            case Behaviour.ExactPositionWithStaticOrigin:
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, uiCamera, out m_PointerDownPos);
                MoveStick(pointerPosition, uiCamera);
                break;
            case Behaviour.ExactPositionWithDynamicOrigin:
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, uiCamera, out var pointerDown);            
               
                // 더미 조이스틱을 비활성화 시킵니다,
                dummyJoystickObject.gameObject.SetActive(false);

                // 실제 조이스틱을 활성화 시킵니다.
                realJoystickHandle.gameObject.SetActive(true);
                realJoystickBackGoundObject.gameObject.SetActive(true);

                // 유저가 터치한 곳으로 조이스틱의 위치를 설정
                m_PointerDownPos = pointerDown;
                realJoystickHandle.anchoredPosition = pointerDown;
                realJoystickBackGoundObject.anchoredPosition = pointerDown;
                break;
        }
    }

    /// <summary>
    /// 조이스틱 핸들을 움직이고, 유저가 조작한 값을 InputSystem에 넘겨주는 메소드. 
    /// </summary>
    private void MoveStick(Vector2 pointerPosition, Camera uiCamera)
    {
        var canvasRect = transform.parent?.GetComponentInParent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("OnScreenStick needs to be attached as a child to a UI Canvas to function properly.");
            return;
        }
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, uiCamera, out var position);
        var delta = position - m_PointerDownPos;

        switch (m_Behaviour)
        {
            case Behaviour.RelativePositionWithStaticOrigin:
                delta = Vector2.ClampMagnitude(delta, movementRange);
                realJoystickHandle.anchoredPosition = (Vector2)m_StartPos + delta;
                break;

            case Behaviour.ExactPositionWithStaticOrigin:
                delta = position - (Vector2)m_StartPos;
                delta = Vector2.ClampMagnitude(delta, movementRange);
                realJoystickHandle.anchoredPosition = (Vector2)m_StartPos + delta;
                break;

            case Behaviour.ExactPositionWithDynamicOrigin:
                delta = Vector2.ClampMagnitude(delta, movementRange);
                realJoystickHandle.anchoredPosition = m_PointerDownPos + delta;
                break;
        }

        var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);
        SendValueToControl(newPos);
    }

    /// <summary>
    /// 유저의 조작이 끝났을 때 동작하는 메소드
    /// </summary>
    private void EndInteraction()
    {  
        // 더미 조이스틱을 활성화 시킵니다
        dummyJoystickObject.gameObject.SetActive(true);

        // 실제 조이스틱은 비활성화 시킵니다
        realJoystickHandle.gameObject.SetActive(false);
        realJoystickBackGoundObject.gameObject.SetActive(false);

        // 조이스틱 위치 초기화
        m_PointerDownPos = m_StartPos;
        realJoystickHandle.anchoredPosition = m_StartPos;
        realJoystickBackGoundObject.anchoredPosition = m_StartPos;
        SendValueToControl(Vector2.zero);
    }

    private void OnPointerDown(InputAction.CallbackContext ctx)
    {
        Debug.Assert(EventSystem.current != null);

        var screenPosition = Vector2.zero;
        if (ctx.control?.device is Pointer pointer)
            screenPosition = pointer.position.ReadValue();

        m_PointerEventData.position = screenPosition;
        EventSystem.current.RaycastAll(m_PointerEventData, m_RaycastResults);
        if (m_RaycastResults.Count == 0)
            return;

        var stickSelected = false;
        foreach (var result in m_RaycastResults)
        {
            if (result.gameObject != gameObject) continue;

            stickSelected = true;
            break;
        }

        if (!stickSelected)
            return;

        BeginInteraction(screenPosition, GetCameraFromCanvas());
        m_PointerMoveAction.performed += OnPointerMove;
    }

    private void OnPointerMove(InputAction.CallbackContext ctx)
    {
        // only pointer devices are allowed
        Debug.Assert(ctx.control?.device is Pointer);

        var screenPosition = ((Pointer)ctx.control.device).position.ReadValue();

        MoveStick(screenPosition, GetCameraFromCanvas());
    }

    private void OnPointerUp(InputAction.CallbackContext ctx)
    {
        EndInteraction();
        m_PointerMoveAction.performed -= OnPointerMove;
    }

    private Camera GetCameraFromCanvas()
    {
        var canvas = GetComponentInParent<Canvas>();
        var renderMode = canvas?.renderMode;
        if (renderMode == RenderMode.ScreenSpaceOverlay
            || (renderMode == RenderMode.ScreenSpaceCamera && canvas?.worldCamera == null))
            return null;

        return canvas?.worldCamera ?? Camera.main;
    }

    /// <summary>
    /// 실제 조이스틱 오브젝트
    /// </summary>
    public RectTransform realJoystickBackGoundObject;
    public RectTransform realJoystickHandle;
    /// <summary>
    /// 유저로부터의 입력이 없을 때(다이나믹 터치 비활성화시) 보여줄 더미 조이스틱 오브젝트
    /// </summary>
    public RectTransform dummyJoystickObject;

    /// <summary>
    /// The distance from the onscreen control's center of origin, around which the control can move.
    /// </summary>
    public float movementRange
    {
        get => m_MovementRange;
        set => m_MovementRange = value;
    }

    /// <summary>
    /// Prevents stick interactions from getting cancelled due to device switching.
    /// </summary>
    /// <remarks>
    /// This property is useful for scenarios where the active device switches automatically
    /// based on the most recently actuated device. A common situation where this happens is
    /// when using a <see cref="PlayerInput"/> component with Auto-switch set to true. Imagine
    /// a mobile game where an on-screen stick simulates the left stick of a gamepad device.
    /// When the on-screen stick is moved, the Input System will see an input event from a gamepad
    /// and switch the active device to it. This causes any active actions to be cancelled, including
    /// the pointer action driving the on screen stick, which results in the stick jumping back to
    /// the center as though it had been released.
    ///
    /// In isolated mode, the actions driving the stick are not cancelled because they are
    /// unique Input Action instances that don't share state with any others.
    /// </remarks>
    public bool useIsolatedInputActions
    {
        get => m_UseIsolatedInputActions;
        set => m_UseIsolatedInputActions = value;
    }

    [FormerlySerializedAs("movementRange")]
    [SerializeField]
    [Min(0)]
    private float m_MovementRange = 50;

    [SerializeField]
    [Tooltip("Defines the circular region where the onscreen control may have it's origin placed.")]
    [Min(0)]
    private float m_DynamicOriginRange = 100;

    [InputControl(layout = "Vector2")]
    [SerializeField]
    private string m_ControlPath;

    [SerializeField]
    [Tooltip("Choose how the onscreen stick will move relative to it's origin and the press position.\n\n" +
        "RelativePositionWithStaticOrigin: The control's center of origin is fixed. " +
        "The control will begin un-actuated at it's centered position and then move relative to the pointer or finger motion.\n\n" +
        "ExactPositionWithStaticOrigin: The control's center of origin is fixed. The stick will immediately jump to the " +
        "exact position of the click or touch and begin tracking motion from there.\n\n" +
        "ExactPositionWithDynamicOrigin: The control's center of origin is determined by the initial press position. " +
        "The stick will begin un-actuated at this center position and then track the current pointer or finger position.")]
    private Behaviour m_Behaviour;

    [SerializeField]
    [Tooltip("Set this to true to prevent cancellation of pointer events due to device switching. Cancellation " +
        "will appear as the stick jumping back and forth between the pointer position and the stick center.")]
    private bool m_UseIsolatedInputActions;

    [SerializeField]
    [Tooltip("The action that will be used to detect pointer down events on the stick control. Note that if no bindings " +
        "are set, default ones will be provided.")]
    private InputAction m_PointerDownAction;

    [SerializeField]
    [Tooltip("The action that will be used to detect pointer movement on the stick control. Note that if no bindings " +
        "are set, default ones will be provided.")]
    private InputAction m_PointerMoveAction;

    private Vector3 m_StartPos;
    private Vector2 m_PointerDownPos;

    [NonSerialized]
    private List<RaycastResult> m_RaycastResults;
    [NonSerialized]
    private PointerEventData m_PointerEventData;

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    /// <summary>Defines how the onscreen stick will move relative to it's origin and the press position.</summary>
    public Behaviour behaviour
    {
        get => m_Behaviour;
        set => m_Behaviour = value;
    }

    /// <summary>Defines how the onscreen stick will move relative to it's center of origin and the press position.</summary>
    public enum Behaviour
    {
        /// <summary>The control's center of origin is fixed in the scene.
        /// The control will begin un-actuated at it's centered position and then move relative to the press motion.</summary>
        RelativePositionWithStaticOrigin,

        /// <summary>The control's center of origin is fixed in the scene.
        /// The control may begin from an actuated position to ensure it is always tracking the current press position.</summary>
        ExactPositionWithStaticOrigin,

        /// <summary>The control's center of origin is determined by the initial press position.
        /// The control will begin unactuated at this center position and then track the current press position.</summary>
        ExactPositionWithDynamicOrigin
    }

    
#if UNITY_EDITOR
    // 이 구간이 있어야 합니다.  사용 안하더라도 있어야 이 커스텀 OnScreenStick이 정상 작동함.
#endif
}

