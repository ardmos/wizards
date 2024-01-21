using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupUserInfoUI : MonoBehaviour
{
    public Button btnClose;
    public Button btnUserName;
    public TextFieldUI txtUserName;
    public LobbyUI lobbyUI;
    
    // Start is called before the first frame update
    void Start()
    {
        btnClose.onClick.AddListener(Hide);
        btnUserName.onClick.AddListener(OpenPopupNameChange);
    }

    private void OpenPopupNameChange()
    {
        //?
        FindObjectOfType<LobbyUI>().popupNameChangeUI.Show();
    }

    public void UpdateUserName(string userName)
    {
        // 1. Update Player Data
        PlayerProfileDataManager.Instance.SetPlayerName(userName);
        // 2. Update this Popup UI's user name textBox
        txtUserName.UpdateTextValue(userName);
        // 3. Update this In Lobby Scene
        lobbyUI.UpdateUserInfoUI();
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
