using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 1. 버튼 클릭시 -> 선택된 기능을 지정된 스펠 슬롯에 적용
/// </summary>
public class SelectScrollEffectPopupItemTemplateUI : MonoBehaviour
{
    //public sbyte slotNumber;
    public CustomClickSoundButton btnApply;

    public TextMeshProUGUI txtScrollName;
    public TextMeshProUGUI txtSpellDescription;
    public Image imgSpellIcon;
    //public Image imgScrollEffectIcon;

    /// <summary>
    ///  scroll정보에 맞춰 UI의 정보를 초기화 해주는 메소드 입니다.
    /// </summary>
    public void InitUI(ISkillUpgradeOption skillUpgradeOption, PopupSelectScrollEffectUIController popupSelectScrollEffectUIController)
    {
        //Debug.Log($"InitUI. scrollName:{scrollName}, spellIndexToApply:{spellIndexToApply}");
        skillUpgradeOption.ToDTO();
        // upgradeOption 이름
        txtScrollName.text = $"{skillUpgradeOption.GetName()}!!";
        // upgradeOption 설명  
        txtSpellDescription.text = skillUpgradeOption.GetDescription();
        // upgradeOption 아이콘 이미지
        imgSpellIcon.sprite = skillUpgradeOption.GetIcon();

        // 버튼 기능 설정   
        btnApply.onClick.RemoveAllListeners();
        btnApply.AddClickListener(() => {
            // 전달받은 스크롤 이름과 스펠인덱스를 사용해서 효과 적용을 진행한다.
            //ScrollManagerServer.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);
            ScrollManagerServer.Instance.UpdateScrollEffectServerRPC(skillUpgradeOption);

            // SFX 재생
            SoundManager.Instance.PlayItemSFXServerRPC(ItemName.ScrollUse, transform.position);

            // VFX 재생
            PlayerClient.Instance.GetComponent<PlayerServer>().StartApplyScrollVFXServerRPC();

            popupSelectScrollEffectUIController.Hide();
        });
    }
}
