using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// SoundManager와 연계 
/// 볼륨없으면 아이콘 이미지 교체 
/// </summary>
public class SoundGroupUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image handle;
    [SerializeField] private Slider slider;
    [SerializeField] private Sprite iconSoundOn;
    [SerializeField] private Sprite iconSoundOff;
    [SerializeField] private Sprite handleSoundOn;
    [SerializeField] private Sprite handleSoundOff;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateIconImage();
    }

    private void UpdateIconImage()
    {
        if (slider.value != 0) { 
            icon.sprite = iconSoundOn;
            handle.sprite = handleSoundOn;
            return; 
        }
        icon.sprite = iconSoundOff;
        handle.sprite = handleSoundOff;
    }
}
