using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 1. ����ġ �̹��� ����
/// 2. ����ġ ���� ������Ʈ
/// </summary>
public class SwitchUI : MonoBehaviour
{    
    [SerializeField] private Button btnTouchArea;
    [SerializeField] private Image switchFrame;
    [SerializeField] private Image switchHandle;

    // ������ �̹�����
    [SerializeField] private Sprite switchFrameOn;
    [SerializeField] private Sprite switchFrameOff;
    [SerializeField] private Sprite switchHandleOn;
    [SerializeField] private Sprite switchHandleOff;

    // �Ʒ� ����ġ ���°��� �ٸ������� ���� ������ �ʿ���
    [SerializeField] private bool isSwitchOn;

    // �ڵ� ��ġ 
    [SerializeField] private Vector2 posHandleOn;
    [SerializeField] private Vector2 posHandleOff;


    // Start is called before the first frame update
    void Start()
    {
        btnTouchArea.onClick.AddListener(SwitchClicked);
    }

    private void SwitchClicked()
    {
        if (isSwitchOn) SwitchOff();
        else SwitchOn();
    }

    private void SwitchOn()
    {
        isSwitchOn = true;
        switchFrame.sprite = switchFrameOn;
        switchHandle.sprite = switchHandleOn;
        switchHandle.transform.localPosition = posHandleOn;
    }

    private void SwitchOff()
    {
        isSwitchOn = false;
        switchFrame.sprite = switchFrameOff;
        switchHandle.sprite = switchHandleOff;
        switchHandle.transform.localPosition = posHandleOff;
    }
}
