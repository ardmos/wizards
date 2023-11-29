using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupSettingsUI : MonoBehaviour
{
    [SerializeField] private Button btnClose;

    // Start is called before the first frame update
    void Start()
    {
        InitPopupSettings();

        btnClose.onClick.AddListener(Hide);

        Hide();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitPopupSettings()
    {
        // Sound Manager 추가 이후 구현
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
