using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ��ũ�� ȹ��� ȹ���� ��ũ���� ��� ���翡 �������� 
/// �����ϴ� �˾��� �����ϴ� ��ũ��Ʈ �Դϴ�.
/// 
/// 1. � ��ų �����ؼ� �� �˾��� �����ԵȰ��� ��ũ�� ������ ���� ����
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

        // ȿ���� ���        
        SoundManager.Instance.PlayItemSFXServerRPC(ItemName.ScrollOpen, PlayerClient.Instance.transform.position);

        // �˾��� ���� �� �÷��̾�� ������ �ɷ� �� ������ �������� ����. �÷��̾�� �� �� ����.
        ScrollManagerServer.Instance.GetUniqueRandomAblilitiesServerRPC();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}