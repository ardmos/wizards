using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameScene UI 
/// 1. GameScene�� UI�鿡�� ������ �ʿ��� �� �߰� ������ �մϴ�.
/// �����е�� ��ũ��Ʈ�� ���� �ֽ��ϴ�
/// </summary>
public class GameUIController : MonoBehaviour
{
    public static GameUIController instance;

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
        instance = this;
        btnMenu.AddClickListener(() =>
        {
            popupMenuUI.Show();
        });
    }

}