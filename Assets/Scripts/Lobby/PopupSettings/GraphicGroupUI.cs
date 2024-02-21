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
        int qualityLevel = GraphicQualityManager.instance.GetQualityLevel();
        switch (qualityLevel)
        {
            case 0: 
                tglLow.isOn = true;
                break;
            case 1:
                tglMedium.isOn = true;
                break;
            case 2:
                tglHigh.isOn = true;
                break;
        }
    }

    // 그래픽 퀄리티 변경
    private void OnToggleValueChanged()
    {
        SoundManager.instance.PlayButtonClickSound();
        if (tglLow.isOn) { GraphicQualityManager.instance.SetQualityLevel(0); }
        else if (tglMedium.isOn) { GraphicQualityManager.instance.SetQualityLevel(1); }
        else if (tglHigh.isOn) { GraphicQualityManager.instance.SetQualityLevel(2); }
    }
}
