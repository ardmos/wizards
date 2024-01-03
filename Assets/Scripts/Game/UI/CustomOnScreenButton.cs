using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.Serialization;

/// <summary>
/// GamePad�� Skill��ư�� ��ũ��Ʈ �Դϴ�.
/// ������ �巡�׵ǰ�, �巡�׵Ǵµ��� �÷��̾�� ȸ�����Ⱚ�� �Ѱܼ� �÷��̾ ȸ�����Ѿ� �մϴ�.
/// 
/// ����
/// 1. ��Ÿ���� �巡�׹��� ���ϱ�   �Ϸ�
/// 2. �巡���� �÷��̾� ȸ����Ű��
/// </summary>
public class CustomOnScreenButton : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private void Start()
    {
        m_StartPos = transform.GetComponent<RectTransform>().anchoredPosition;
        m_PointerDownPos = m_StartPos;
    }

    /// <summary>
    /// ��ġ ����
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        BeginInteraction(eventData.position, eventData.pressEventCamera);       
    }
    /// <summary>
    /// �巡�׵Ǵµ��� 
    /// 1. UI ��ġ �巡������ ������ ����
    /// 2. �÷��̾�� ȸ�����Ⱚ�� �Ѱܼ� �÷��̾ ȸ����Ų��
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        // Spell State üũ. Casting�� �ƴ� ���� ����
        if (Player.LocalInstance.gameObject.GetComponent<SpellController>().GetSpellStateFromSpellIndex(spellIndex) != SpellState.Casting)
        {
            return;
        }

        MoveButton(eventData.position, eventData.pressEventCamera);
    }
    /// <summary>
    /// ��ġ ��
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

    private void MoveButton(Vector2 pointerPosition, Camera uiCamera)
    {
        var canvasRect = transform.parent?.GetComponentInParent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("CustomOnScreenButton needs to be attached as a child to a UI Canvas to function properly.");
            return;
        }
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, uiCamera, out var position);
        var delta = position - m_PointerDownPos;

        delta = Vector2.ClampMagnitude(delta, movementRange);
        transform.GetComponent<RectTransform>().anchoredPosition = (Vector2)m_StartPos + delta;

        var newPos = new Vector2(delta.x / movementRange, delta.y / movementRange);
        //SendValueToControl(newPos);
    }

    private void EndInteraction()
    {
        m_PointerDownPos = m_StartPos;
        // �ڵ� ������Ʈ�� ��ġ�� �ʱ�ȭ �����ݴϴ�.
        transform.GetComponent<RectTransform>().anchoredPosition = m_StartPos;
        SendValueToControl(0.0f);
    }

    ////TODO: pressure support
    /*
    /// <summary>
    /// If true, the button's value is driven from the pressure value of touch or pen input.
    /// </summary>
    /// <remarks>
    /// This essentially allows having trigger-like buttons as on-screen controls.
    /// </remarks>
    [SerializeField] private bool m_UsePressure;
    */
    [InputControl(layout = "Button")]
    [SerializeField]
    private string m_ControlPath;

    // �巡�׸� ���� ������
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
}