using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HPBarUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtHPValue;
    [SerializeField] private Slider slider;
    [SerializeField] private Image imgFill;

    [SerializeField] private Color[] hpColors;

    public void SetHP(sbyte currentHP, sbyte maxHP)
    {
        slider.value = currentHP;
        slider.maxValue = maxHP;
        UpdateHPBarColorAndTextValue(currentHP, maxHP);
    }

    private void UpdateHPBarColorAndTextValue(sbyte currentHP, sbyte maxHP)
    {
        txtHPValue.text = $"{currentHP} / {maxHP}";
        float currentHPPercentage = slider.value / slider.maxValue;
        // 전체 hp 60%이상 초록
        if ( currentHPPercentage >= 0.6f)
        {
            imgFill.color = hpColors[0];
            txtHPValue.color = hpColors[0];
        }
        // 전체 hp 30%이상 노랑
        else if (currentHPPercentage >= 0.3f) {
            imgFill.color = hpColors[1];
            txtHPValue.color = hpColors[1];
        }
        // 전체 hp 0%이상 빨강
        else
        {
            imgFill.color = hpColors[2];
            txtHPValue.color = hpColors[2];
        }
    }
}
