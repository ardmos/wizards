using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.Serialization;

/// <summary>
/// GamePad의 Skill버튼용 스크립트 입니다.
/// 누르면 드래그되고, 드래그되는동안 플레이어에게 회전방향값을 넘겨서 플레이어를 회전시킬 수 있도록 합니다.
/// </summary>
public class CustomOnScreenButton : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [InputControl(layout = "Button")]
    [SerializeField]
    private string m_ControlPath;

    [FormerlySerializedAs("movementRange")]
    [SerializeField]
    [Min(0)]
    private float m_MovementRange = 50;
    private Vector3 m_StartPos;
    private Vector2 m_PointerDownPos;
    public float movementRange
    {
        get => m_MovementRange;
        set => m_MovementRange = value;
    }

    [SerializeField] private ushort spellIndex;

    protected override string controlPathInternal
    {
        get => m_ControlPath;
        set => m_ControlPath = value;
    }

    private void Start()
    {
        m_StartPos = transform.GetComponent<RectTransform>().anchoredPosition;
        m_PointerDownPos = m_StartPos;
    }

    /// <summary>
    /// 터치 시작
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        BeginInteraction(eventData.position, eventData.pressEventCamera);       
    }

    /// <summary>
    /// 드래그되는동안 
    /// 1. UI 위치 드래그중인 곳으로 변경
    /// 2. 플레이어에게 회전방향값을 넘겨서 플레이어를 회전시킨다
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        // 혹시나. 예외 체크. Spell State 체크. Casting이 아닌 경우는 무시
        if (PlayerClient.Instance.GetComponent<SpellManagerClient>().GetSpellStateFromSpellIndexOnClient(spellIndex) != SpellState.Casting)
        {
            return;
        }
        // 버튼 이동
        Vector2 dir = MoveButton(eventData.position, eventData.pressEventCamera);
        // 플레이어 회전
        PlayerClient.Instance.GetComponent<PlayerMovementClient>().RotateByDragSpellButton(new Vector3(dir.x, 0f, dir.y));
    }

    /// <summary>
    /// 터치 끝
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        EndInteraction();        
    }

    private void BeginInteraction(Vector2 pointerPosition, Camera uiCamera)
    {
        var canvasRect = transform.parent?.GetComponentInParent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("CustomOnScreenButton needs to be attached as a child to a UI Canvas to function properly.");
            return;
        }
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, uiCamera, out m_PointerDownPos);
        SendValueToControl(1.0f);
    }

    private Vector2 MoveButton(Vector2 pointerPosition, Camera uiCamera)
    {
        var canvasRect = transform.parent?.GetComponentInParent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("CustomOnScreenButton needs to be attached as a child to a UI Canvas to function properly.");
            return Vector2.zero;
        }
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, uiCamera, out var position);
        Vector2 dir = position - m_PointerDownPos;

        Vector2 delta = Vector2.ClampMagnitude(dir, movementRange);
        transform.GetComponent<RectTransform>().anchoredPosition = (Vector2)m_StartPos + delta;

        //var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);

        return dir;
    }

    private void EndInteraction()
    {
        m_PointerDownPos = m_StartPos;
        // 핸들 오브젝트도 위치를 초기화 시켜줍니다.
        transform.GetComponent<RectTransform>().anchoredPosition = m_StartPos;
        SendValueToControl(0.0f);
    }
}