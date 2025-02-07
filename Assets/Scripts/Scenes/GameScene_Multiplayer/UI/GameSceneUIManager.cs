using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameScene UI 
/// 1. GameScene의 UI들에게 접근이 필요할 때 중계 역할을 합니다.
/// 게임패드는 스크립트가 따로 있습니다
/// </summary>
public class GameSceneUIManager : MonoBehaviour
{
    public static GameSceneUIManager Instance;

    [SerializeField] private CustomClickSoundButton btnMenu;
    [SerializeField] private PopupMenuUIController popupMenuUI;

    public GameOverUIController notifyUIController;
    public PopupGameOverUIController popupGameOverUIController;
    public PopupWinUIController popupWinUIController;
    public PopupSelectScrollEffectUIController popupSelectScrollEffectUIController;
    public ButtonReadSpellScrollUIController buttonReadSpellScrollUIController;
    public ItemAcquireUIController itemAcquireUIController;
    public GamePadUIController gamePadUIController;

    void Awake()
    {
        Instance = this;
        btnMenu.AddClickListener(() =>
        {
            popupMenuUI.Show();
        });
    }

}
