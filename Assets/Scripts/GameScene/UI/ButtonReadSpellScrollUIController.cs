using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ��ũ�� ������ ������ Ȱ��ȭ�Ǵ� ��ư UI ��Ʈ�ѷ� �Դϴ�.
/// Spell Scroll Queue ī��Ʈ�� 0 �ʰ��� ��� �÷��̾��� ȭ�鿡 Ȱ��ȭ�˴ϴ�.
/// Ŭ���ϸ� Spell ȿ�� ���� �˾��� �������ϴ�. 
/// </summary>
public class ButtonReadSpellScrollUIController : MonoBehaviour
{
    public TextMeshProUGUI txtSpellScrollCount;

    private void Start()
    {
        if (GetComponent<CustomClickSoundButton>() == null) return;
        if(txtSpellScrollCount == null) return ;

        GetComponent<CustomClickSoundButton>().AddClickListener(() =>
        {
            GameSceneUIManager.Instance.popupSelectScrollEffectUIController.Show();
        });

        txtSpellScrollCount.text = "";
        DeactivateUI();
    }

    public void ActivateUI()
    {
        gameObject.SetActive(true);
    }
    public void UpdateUI(int scrollCount)
    {
        gameObject.SetActive(true);
        txtSpellScrollCount.text = scrollCount.ToString();     
    }
    public void DeactivateUI()
    {
        gameObject.SetActive(false);
    }
}
