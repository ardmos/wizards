using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ChickenAIHPManagerClient : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI txtHPValue;
    [SerializeField] private Slider slider;

    public void SetHP(sbyte currentHP, sbyte maxHP)
    {
        slider.maxValue = maxHP;
        slider.value = currentHP;
        txtHPValue.text = $"{currentHP} / {maxHP}";
    }

    [ClientRpc]
    public void SetHPClientRPC(sbyte hp, sbyte maxHP)
    {
        SetHP(hp, maxHP);
    }
}
