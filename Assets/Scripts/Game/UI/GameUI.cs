using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private Button btnMenu;

    [SerializeField] private PopupMenuUI popupMenuUI;
    // Start is called before the first frame update
    void Start()
    {
        btnMenu.onClick.AddListener(() =>
        {
            popupMenuUI.Show();
        });
    }

}
