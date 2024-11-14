using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameScene UI 
/// 1. GameScene�� UI�鿡�� ������ �ʿ��� �� �߰� ������ �մϴ�.
/// �����е�� ��ũ��Ʈ�� ���� �ֽ��ϴ�
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
