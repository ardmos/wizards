using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameScene UI 
/// 1. GameScene의 UI들에게 접근이 필요할 때 중계 역할을 합니다.
/// 게임패드는 스크립트가 따로 있습니다
/// </summary>
public class GameUIController : MonoBehaviour
{
    public static GameUIController instance;

    [SerializeField] private CustomButton btnMenu;
    [SerializeField] private PopupMenuUIController popupMenuUI;

    public NotifyUIController notifyUIController;
    public PopupGameOverUIController popupGameOverUI;
    public PopupWinUIController popupWinUI;
    public PopupSelectSpellUIController popupSelectSpell;


    void Awake()
    {
        instance = this;
        btnMenu.AddClickListener(() =>
        {
            popupMenuUI.Show();
        });
    }

}
