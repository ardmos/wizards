using UnityEngine;
using UnityEngine.UI;

public class MusicGroupUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Image handle;
    [SerializeField] private Slider slider;
    [SerializeField] private Sprite iconSoundOn;
    [SerializeField] private Sprite iconSoundOff;
    [SerializeField] private Sprite handleSoundOn;
    [SerializeField] private Sprite handleSoundOff;
    [SerializeField] private Color colorOn;
    [SerializeField] private Color colorOff;

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
        if (slider.value != 0)
        {
            icon.sprite = iconSoundOn;
            icon.color = colorOn;
            handle.sprite = handleSoundOn;
            return;
        }
        icon.sprite = iconSoundOff;
        icon.color = colorOff;
        handle.sprite = handleSoundOff;
    }
}
