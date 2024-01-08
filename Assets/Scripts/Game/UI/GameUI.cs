using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameScene UI 
/// 1. GameScene의 UI들에게 접근이 필요할 때 중계 역할을 합니다.
/// 게임패드는 스크립트가 따로 있습니다
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI instance;

    [SerializeField] private Button btnMenu;
    [SerializeField] private PopupMenuUI popupMenuUI;

    public NotifyUIController notifyUIController;
    public PopupGameOverUI popupGameOverUI;

    void Awake()
    {
        instance = this;
        btnMenu.onClick.AddListener(() =>
        {
            popupMenuUI.Show();
        });
    }

}
