using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupNameChangeUIController : MonoBehaviour
{
    public CustomButton btnOk;
    public CustomButton btnClose;
    public TMP_InputField inputFieldUserName;
    public PopupUserInfoUIController popupUserInfoUI;

    // Start is called before the first frame update
    void Start()
    {
        btnOk.AddClickListener(SubmitNewUserName);

        btnClose.AddClickListener(Hide);
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
