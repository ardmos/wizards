using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button btnUserInfo;
    [SerializeField] private Button btnSettings;
    [SerializeField] private Button btnShop;

    [SerializeField] private PopupUserInfoUI popupUserInfoUI;
    [SerializeField] private PopupSettingsUI popupSettingsUI;
    [SerializeField] private PopupShopUI popupShop;

    // Start is called before the first frame update
    void Start()
    {
        btnUserInfo.onClick.AddListener(() => { popupUserInfoUI.Show(); });
        btnSettings.onClick.AddListener(() => { popupSettingsUI.Show(); });
        btnShop.onClick.AddListener(() => { popupShop.Show(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
