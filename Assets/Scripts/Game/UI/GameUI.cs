using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Button btnMenu;

    [SerializeField] private PopupMenuUI popupMenuUI;

    void Awake()
    {
        btnMenu.onClick.AddListener(() =>
        {
            popupMenuUI.Show();
        });
    }

}
