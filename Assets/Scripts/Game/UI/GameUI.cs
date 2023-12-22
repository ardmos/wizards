using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameScene UI 
/// �����е�� ��ũ��Ʈ�� ���� �ֽ��ϴ�
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [SerializeField] private Button btnMenu;

    [SerializeField] private PopupMenuUI popupMenuUI;

    void Awake()
    {
        Instance = this;

        btnMenu.onClick.AddListener(() =>
        {
            popupMenuUI.Show();
        });
    }
}
