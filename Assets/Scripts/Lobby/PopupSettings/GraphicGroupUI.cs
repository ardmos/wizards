using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicGroupUI : MonoBehaviour
{
    public Toggle tglLow;
    public Toggle tglMedium;
    public Toggle tglHigh;


    void Start()
    {
        tglLow.onValueChanged.AddListener(delegate { OnToggleValueChanged(); });
        tglMedium.onValueChanged.AddListener(delegate { OnToggleValueChanged(); });
        tglHigh.onValueChanged.AddListener(delegate { OnToggleValueChanged(); });     
    }

    public void Setup()
    {
        int qualityLevel = QualitySettings.GetQualityLevel();
        switch (qualityLevel)
        {
            case 0: 
                tglLow.isOn = true;
                //tglMedium.isOn = false;
                //tglHigh.isOn = false;
                break;
            case 1:
                //tglLow.isOn = true;
                tglMedium.isOn = true;
                //tglHigh.isOn = false;
                break;
            case 2:
                //tglLow.isOn = true;
                //tglMedium.isOn = false;
                tglHigh.isOn = true;
                break;
        }
    }

    // 그래픽 퀄리티 변경
    private void OnToggleValueChanged()
    {
        SoundManager.instance.PlayButtonClickSound();
        if (tglLow.isOn) { QualitySettings.SetQualityLevel(0, true); }
        if (tglMedium.isOn) { QualitySettings.SetQualityLevel(1, true); }
        if (tglHigh.isOn) { QualitySettings.SetQualityLevel(2, true); }
    }
}
