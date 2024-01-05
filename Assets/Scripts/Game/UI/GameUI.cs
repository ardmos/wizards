using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameScene UI 
/// �����е�� ��ũ��Ʈ�� ���� �ֽ��ϴ�
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI instance;

    [SerializeField] private Button btnMenu;
    [SerializeField] private PopupMenuUI popupMenuUI;

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
