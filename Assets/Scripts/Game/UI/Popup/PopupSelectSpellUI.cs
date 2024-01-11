using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
/// <summary>
/// 스크롤 획득시 획득한 스크롤을 어느 스펠에 적용할지 
/// 선택하는 팝업을 관리하는 스크립트 입니다.
/// 
/// 1. 어떤 스킬 관련해서 이 팝업이 열리게된건지 스크롤 아이템 정보 저장
/// 1. 버튼 셋 중에 선택 -> 선택된 슬롯의 스킬에 스크롤 아이템 정보 전달.
/// </summary>
public class PopupSelectSpellUI : MonoBehaviour
{
    // 이 팝업이 열리게된 스크롤 아이템 정보 저장
    [SerializeField] private Scroll scroll;
    
    public Button btnSpell1;
    public Button btnSpell2;
    public Button btnSpell3;



    private void Start()
    {
        btnSpell1.onClick.AddListener(() => {
            Player.LocalInstance.ApplyScrollToSpell(scroll, 0);
        });
        btnSpell2.onClick.AddListener(() => {
            Player.LocalInstance.ApplyScrollToSpell(scroll, 1);
        });
        btnSpell3.onClick.AddListener(() => {
            Player.LocalInstance.ApplyScrollToSpell(scroll, 2);
        });

        Hide();
    }


    public void Show(Scroll scroll)
    {
        gameObject.SetActive(true);
        this.scroll = scroll;
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}