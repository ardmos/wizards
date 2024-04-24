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
    public TextMeshProUGUI txtSpellName;
    public Image imgSpellIcon;
    public Image imgScrollEffectIcon;

    /// <summary>
    ///  scroll정보에 맞춰 UI의 정보를 초기화 해주는 메소드 입니다.
    /// </summary>
    public void InitUI(ItemName scrollName, byte spellIndexToApply)
    {
        //Debug.Log($"InitUI. scrollName:{scrollName}, spellIndexToApply:{spellIndexToApply}");

        // scroll 이름
        txtScrollName.text = $"{Item.GetName(scrollName)}!!";
        // spell 이름
        SkillName spellName = PlayerClient.Instance.GetComponent<SkillSpellManagerClient>().GetSpellInfoList()[spellIndexToApply].spellName;
        txtSpellName.text = spellName.ToString();
        // spell 아이콘 이미지
        imgSpellIcon.sprite = GameAssets.instantiate.GetSpellIconImage(spellName);
        // scroll effect 아이콘 이미지
        imgScrollEffectIcon.sprite = GameAssets.instantiate.GetScrollEffectIconImage(scrollName);

        // 버튼 기능 설정   
        btnApply.onClick.RemoveAllListeners();
        btnApply.AddClickListener(() => {
            if (PlayerClient.Instance == null) return;
            if(GetComponentInParent<PopupSelectScrollEffectUIController>() == null) return;

            RequestApplyScrollEffectToServer(scrollName, spellIndexToApply);
            GetComponentInParent<PopupSelectScrollEffectUIController>().Hide();
        });
    }

    // 슬롯 선택시 동작. 클라이언트에서 돌아가는 메소드 입니다.
    public void RequestApplyScrollEffectToServer(ItemName scrollName, byte spellIndex)
    {
        //Debug.Log($"RequestApplyScrollEffectToServer. scrollNames:{scrollName}, spellIndexToApply:{spellIndex}");

        // 전달받은 스크롤 이름과 스펠인덱스를 사용해서 효과 적용을 진행한다.
        ScrollManagerServer.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);

        // SFX 재생
        SoundManager.Instance.PlayItemSFX(scrollName);
        // VFX 재생
        PlayerClient.Instance.GetComponent<PlayerServer>().StartApplyScrollVFXServerRPC();
    }
}
