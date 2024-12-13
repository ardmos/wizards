using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserNameUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtUserName;

    public void SetName(string userName)
    {
        txtUserName.text = userName;
    }

    public void SetColor(bool isOwner)
    {
        if (isOwner)
            txtUserName.color = GameAssetsManager.Instance.gameAssets.color_Owner;
        else
            txtUserName.color = GameAssetsManager.Instance.gameAssets.color_Enemy;
    }
}
