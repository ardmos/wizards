using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameScene UI 
/// 1. GameScene의 UI들에게 접근이 필요할 때 중계 역할을 합니다.
/// 게임패드는 스크립트가 따로 있습니다
/// </summary>
public class GameSceneUIController : MonoBehaviour
{
    public static GameSceneUIController Instance;

    [SerializeField] private CustomButton btnMenu;
    [SerializeField] private PopupMenuUIController popupMenuUI;

    public GameOverUIController notifyUIController;
    public PopupGameOverUIController popupGameOverUIController;
    public PopupWinUIController popupWinUIController;
    public PopupSelectScrollEffectUIController popupSelectScrollEffectUIController;
    public ButtonReadSpellScrollUIController buttonReadSpellScrollUIController;
    public ItemAcquireUIController itemAcquireUIController;

    void Awake()
    {
        Instance = this;
        btnMenu.AddClickListener(() =>
        {
            popupMenuUI.Show();
        });
    }

}
