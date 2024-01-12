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
    public Image[] attackSkillIcons;
    // Defence or Dodge skill array[1] �ϳ��� �߰��� ����.
    //public Image imgBtnSkill1, imgBtnSkill2, imgBtnSkill3;//, imgBtnSkillDefence;
    public Image imgCooltimeSkill1, imgCooltimeSkill2, imgCooltimeSkill3;//, imgCooltimeSkillDefence;
    public GameManager gameManager;

    public RectTransform[] spellButtons;

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
            attackSkillIcons[i].sprite = GameAssets.instantiate.GetSpellIconImage(spellNames[i]);
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

        float spell1CoolTimeRatio = Player.LocalInstance.gameObject.GetComponent<SpellController>().GetCurrentSpellCoolTimeRatio(0);
        if (spell1CoolTimeRatio > 0)
        {
            imgCooltimeSkill1.fillAmount = 1 - spell1CoolTimeRatio;
        }
        float spell2CoolTimeRatio = Player.LocalInstance.gameObject.GetComponent<SpellController>().GetCurrentSpellCoolTimeRatio(1);
        if (spell2CoolTimeRatio > 0)
        {
            imgCooltimeSkill2.fillAmount = 1 - spell2CoolTimeRatio;
        }
        float spell3CoolTimeRatio = Player.LocalInstance.gameObject.GetComponent<SpellController>().GetCurrentSpellCoolTimeRatio(2);
        if (spell3CoolTimeRatio > 0)
        {
            imgCooltimeSkill3.fillAmount = 1 - spell3CoolTimeRatio;
        }
    }
}
