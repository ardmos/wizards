using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupEquipmentUIController : MonoBehaviour
{
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnHome;

    // Start is called before the first frame update
    void Start()
    {
        btnClose.onClick.AddListener(Hide);
        btnHome.onClick.AddListener(Hide);
    }


    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
