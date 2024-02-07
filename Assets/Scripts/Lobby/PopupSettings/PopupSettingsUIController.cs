using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupSettingsUIController : MonoBehaviour
{
    public CustomButtonUI btnClose;
    public CustomButtonUI btnLanguage;
    public CustomButtonUI btnSupport;

    public MusicGroupUI musicGroup;
    public SoundFXGroupUI soundFXGroup;

    public PopupSupportUIController popupSupport;

    // Start is called before the first frame update
    void Start()
    {
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
