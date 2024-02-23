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
        // ��ü hp 60%�̻� �ʷ�
        if ( currentHPPercentage >= 0.6f)
        {
            imgFill.color = hpColors[0];
            txtHPValue.color = hpColors[0];
        }
        // ��ü hp 30%�̻� ���
        else if (currentHPPercentage >= 0.3f) {
            imgFill.color = hpColors[1];
            txtHPValue.color = hpColors[1];
        }
        // ��ü hp 0%�̻� ����
        else
        {
            imgFill.color = hpColors[2];
            txtHPValue.color = hpColors[2];
        }
    }
}
