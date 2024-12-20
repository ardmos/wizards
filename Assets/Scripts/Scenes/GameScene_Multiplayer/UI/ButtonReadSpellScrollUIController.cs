using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스크롤 아이템 보유시 활성화되는 버튼 UI 컨트롤러 입니다.
/// Spell Scroll Queue 카운트가 0 초과인 경우 플레이어의 화면에 활성화됩니다.
/// 클릭하면 Spell 효과 선택 팝업이 펼쳐집니다. 
/// </summary>
public class ButtonReadSpellScrollUIController : MonoBehaviour
{
    public TextMeshProUGUI txtSpellScrollCount;
    private int _scrollCount;

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
        _scrollCount = scrollCount;
        txtSpellScrollCount.text = _scrollCount.ToString();     
    }
    public void MinusScrollCount()
    {
        --_scrollCount;
        txtSpellScrollCount.text = _scrollCount.ToString();
    }
    public int GetScrollCount()
    {
        return _scrollCount;
    }
    public void DeactivateUI()
    {
        gameObject.SetActive(false);
    }
}
