using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamColorController : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public UserNameUIController userNameUIController;
    public HPBarUIController hPBarUIController;

    public void Setup(bool isOwner)
    {
        userNameUIController.SetColor(isOwner);
        hPBarUIController.SetBGColor(isOwner);

        // 배경 바닥 컬러 설정
        if (isOwner)
            spriteRenderer.color = GameAssetsManager.Instance.gameAssets.color_Owner;
        else
            spriteRenderer.color = GameAssetsManager.Instance.gameAssets.color_Enemy;
    }
}
