using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀씬 로그인 인증 방법 선택 팝업(PopupSelectAuthMethod)을 컨트롤하는 스크립트 입니다.
/// </summary>

public class PopupSelectAuthMethodUIController : MonoBehaviour
{
    [SerializeField] private CustomButton btnSignInAnonymous;
    [SerializeField] private CustomButton btnSignInGooglePlayGames;
    [SerializeField] private CustomButton btnClose;

    private void Start()
    {
        btnSignInAnonymous.AddClickListener(UnityAuthenticationManager.Instance.OnBtnSignInAnonymousClicked);
        btnSignInGooglePlayGames.AddClickListener(UnityAuthenticationManager.Instance.OnBtnSignInGooglePlayGamesClicked);
        btnClose.AddClickListener(Hide);
    }

    public void Show()
    {
        gameObject.transform.localScale = Vector3.one;
    }

    private void Hide() 
    {
        gameObject.transform.localScale = Vector3.zero;
    }
}
