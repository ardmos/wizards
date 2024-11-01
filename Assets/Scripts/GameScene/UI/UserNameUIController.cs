using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UserNameUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtUserName;

    public void Setup(string userName, bool isOwner)
    {
        txtUserName.text = userName;

        if (isOwner)
            txtUserName.color = GameAssets.instantiate.color_Owner;
        else
            txtUserName.color = GameAssets.instantiate.color_Enemy;
    }
}
