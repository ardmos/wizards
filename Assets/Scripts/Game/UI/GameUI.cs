using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameScene UI 
/// 게임패드는 스크립트가 따로 있습니다
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
