using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 1. 스위치 이미지 변경
/// 2. 스위치 상태 업데이트
/// </summary>
public class SwitchUI : MonoBehaviour
{    
    [SerializeField] private Button btnTouchArea;
    [SerializeField] private Image switchFrame;
    [SerializeField] private Image switchHandle;

    // 변경할 이미지들
    [SerializeField] private Sprite switchFrameOn;
    [SerializeField] private Sprite switchFrameOff;
    [SerializeField] private Sprite switchHandleOn;
    [SerializeField] private Sprite switchHandleOff;

    // 아래 스위치 상태값은 다른곳에서 저장 관리가 필요함
    [SerializeField] private bool isSwitchOn;

    // 핸들 위치 
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
