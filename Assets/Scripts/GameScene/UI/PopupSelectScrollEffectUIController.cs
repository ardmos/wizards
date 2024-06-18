using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 스크롤 획득시 획득한 스크롤을 어느 스펠에 적용할지 
/// 선택하는 팝업을 관리하는 스크립트 입니다.
/// 
/// 1. 어떤 스킬 관련해서 이 팝업이 열리게된건지 스크롤 아이템 정보 저장
/// </summary>
public class PopupSelectScrollEffectUIController : MonoBehaviour
{
    public SelectScrollEffectPopupItemTemplateUI scrollEffectSlot0;
    public SelectScrollEffectPopupItemTemplateUI scrollEffectSlot1;
    public SelectScrollEffectPopupItemTemplateUI scrollEffectSlot2;

    private void Start()
    {
        Hide();
    }

    public void InitPopup(List<ISkillUpgradeOption> skillUpgradeOptions)
    {
        scrollEffectSlot0.InitUI(skillUpgradeOptions[0]);
        scrollEffectSlot1.InitUI(skillUpgradeOptions[1]);
        scrollEffectSlot2.InitUI(skillUpgradeOptions[2]);
    }

    public void Show()
    {
        gameObject.SetActive(true);

        // 효과음 재생        
        SoundManager.Instance.PlayItemSFXServerRPC(ItemName.ScrollOpen, PlayerClient.Instance.transform.position);

        // 팝업이 열릴 때 플레이어에게 랜덤의 능력 세 가지를 선택지로 제시. 플레이어는 이 중 선택.
        ScrollManagerServer.Instance.GetUniqueRandomAblilitiesServerRPC();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}