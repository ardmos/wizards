using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HPBarUI : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Image imgFill;

    [SerializeField] private Color[] hpColors;

    public void SetHP(float value)
    {
        slider.value = value;
        UpdateHPBarColor();
    }

    private void UpdateHPBarColor()
    {
        float currentHPPercentage = slider.value / slider.maxValue;
        // ��ü hp 60%�̻� �ʷ�
        if ( currentHPPercentage >= 0.6f)
        {
            imgFill.color = hpColors[0];
        }
        // ��ü hp 30%�̻� ���
        else if (currentHPPercentage >= 0.3f) {
            imgFill.color = hpColors[1];
        }
        // ��ü hp 0%�̻� ����
        else
        {
            imgFill.color = hpColors[2];
        }
    }
}
