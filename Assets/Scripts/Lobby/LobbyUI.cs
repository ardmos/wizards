using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button btnSettings;
    [SerializeField] private PopupSettingsUI popupSettingsUI;

    // Start is called before the first frame update
    void Start()
    {
        btnSettings.onClick.AddListener(OpenSettingsPopupUI);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OpenSettingsPopupUI()
    {
        popupSettingsUI.Show();
    }
}
