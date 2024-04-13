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
public class GamePadUIController : MonoBehaviour
{
    public Image[] spellIcons;
    public Image[] spellCooltimeImages;
    
    // Update is called once per frame
    void Update()
    {
        CoolTimePresenter();
    }

    /// <summary>
    /// �����е� ��ų �������� ������Ʈ ���ִ� �޼ҵ�
    /// </summary>
    public void UpdateSpellUI(SkillName[] skills)
    {
        if (PlayerClient.Instance == null) return;

        for (ushort i = 0; i < skills.Length; i++)
        {
            Debug.Log($"GamePadUIController.UpdateSpellUI() skills: {skills[i]}");
            spellIcons[i].sprite = GameAssets.instantiate.GetSpellIconImage(skills[i]);
        }       
    }

    /// <summary>
    /// ��Ÿ���� �ð�ȭ���ִ� �޼ҵ�
    /// </summary>
    private void CoolTimePresenter()
    {
        if (PlayerClient.Instance == null)
        {
            return;
        }

        for (byte i = 0;i < spellCooltimeImages.Length; i++)
        {
            //Debug.Log($"{Player.LocalInstance.OwnerClientId}");
            float coolTimeRatio = PlayerClient.Instance.GetComponent<SpellController>().GetCurrentSpellCoolTimeRatio(i);
            if (coolTimeRatio > 0) 
            {
                spellCooltimeImages[i].fillAmount = 1 - coolTimeRatio;
            }
        }
    }
}
