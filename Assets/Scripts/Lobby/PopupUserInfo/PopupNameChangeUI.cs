using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupNameChangeUI : MonoBehaviour
{
    public Button btnOk;
    public Button btnClose;
    public TMP_InputField inputFieldUserName;
    public PopupUserInfoUI popupUserInfoUI;

    // Start is called before the first frame update
    void Start()
    {
        btnOk.onClick.AddListener(() => {
            SubmitNewUserName();
        });

        btnClose.onClick.AddListener(() => {
            Hide();
        });
    }

    /// <summary>
    /// Submit new UserName to PopupUserInfoUI.
    /// </summary>
    private void SubmitNewUserName()
    {
        popupUserInfoUI.UpdateUserName(inputFieldUserName.text);
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        inputFieldUserName.text = "";
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
