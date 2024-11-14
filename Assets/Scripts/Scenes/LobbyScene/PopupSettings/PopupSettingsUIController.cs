using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupSettingsUIController : MonoBehaviour
{
    public CustomClickSoundButton btnClose;
    public CustomClickSoundButton btnLanguage;
    public CustomClickSoundButton btnSupport;

    public MusicGroupUI musicGroup;
    public SoundFXGroupUI soundFXGroup;
    public GraphicGroupUI graphicGroup;

    public PopupSupportUIController popupSupport;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_SERVER
        return;
#endif

        InitPopupSettings();

        btnClose.AddClickListener(Hide);
        btnLanguage.AddClickListener(() => { }); // 아직 영어만 지원
        btnSupport.AddClickListener(popupSupport.Show);
        Hide();
    } 

    private void InitPopupSettings()
    {
        musicGroup.Setup();
        soundFXGroup.Setup();
        graphicGroup.Setup();
    }

    public void Show()
    {
        gameObject.SetActive(true);    
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

}
