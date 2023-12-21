using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    // Update is called once per frame
    void Update()
    {
        CoolTimePresenter();
    }

    // spellNumber 0���� ���� 
    public void UpdateSpellUI(Sprite spellIcon, int spellNumber)
    {
        UpdateButtonImage(spellIcon, spellNumber);
    }
    // spellNumber 0���� ���� 
    private void UpdateButtonImage(Sprite spellIcon, int spellNumber)
    {
        //Debug.Log($"{nameof(UpdateButtonImage)} Player.LocalInstance:{Player.LocalInstance}, spellIcon:{spellIcon.name}, spellNumber:{spellNumber}");
        if (Player.LocalInstance == null)
        {
            return;
        }

        attackSkillIcons[spellNumber].sprite = spellIcon;
        //imgBtnSkillDefence.sprite = Player.LocalInstance.gameObject.GetComponent<SpellController>().GetCurrentSpellIcon(4);
    }

    // spellNumber 0���� ����
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
