using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
/// <summary>
/// 1. ���� ��ư�� ���� ��ų �̹��� ����ֱ�
/// 2. ��Ÿ�� �ð�ȿ���� �����ֱ�
/// </summary>
public class GamePadUI : MonoBehaviour
{
    public Image[] spellIcons;
    public Image[] spellCooltimeImages;
    
    public GameManager gameManager;

    // Update is called once per frame
    void Update()
    {
        CoolTimePresenter();
    }

    /// <summary>
    /// �����е� ��ų �������� ������Ʈ ���ִ� �޼ҵ�
    /// </summary>
    public void UpdateSpellUI(SpellName[] spellNames)
    {
        if (Player.LocalInstance == null) return;

        for (ushort i = 0; i < spellNames.Length; i++)
        {
            Debug.Log($"spellName: {spellNames[i]}");
            spellIcons[i].sprite = GameAssets.instantiate.GetSpellIconImage(spellNames[i]);
        }       
    }

    /// <summary>
    /// ��Ÿ���� �ð�ȭ���ִ� �޼ҵ�
    /// </summary>
    private void CoolTimePresenter()
    {
        if (Player.LocalInstance == null)
        {
            return;
        }

        for (byte i = 0;i < spellCooltimeImages.Length; i++)
        {
            float coolTimeRatio = Player.LocalInstance.gameObject.GetComponent<SpellController>().GetCurrentSpellCoolTimeRatio(i);
            if (coolTimeRatio > 0) 
            {
                spellCooltimeImages[i].fillAmount = 1 - coolTimeRatio;
            }
        }
    }
}
