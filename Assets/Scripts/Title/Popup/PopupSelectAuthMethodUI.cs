using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀씬 로그인 인증 방법 선택 팝업(PopupSelectAuthMethod)을 컨트롤하는 스크립트 입니다.
/// </summary>

public class PopupSelectAuthMethodUI : MonoBehaviour
{
    [SerializeField] private Button btnSignInAnonymous;
    [SerializeField] private Button btnSignInGooglePlayGames;
    [SerializeField] private Button btnClose;

    private void Start()
    {
        btnSignInAnonymous.onClick.AddListener(() => {
            SoundManager.instance.PlayButtonClickSound();
            UnityAuthenticationManager.Instance.OnBtnSignInAnonymousClicked();
        });
        btnSignInGooglePlayGames.onClick.AddListener(() => {
            SoundManager.instance.PlayButtonClickSound();
            UnityAuthenticationManager.Instance.OnBtnSignInGooglePlayGamesClicked(); 
        });
        btnClose.onClick.AddListener(() => {
            SoundManager.instance.PlayButtonClickSound();
            Hide(); 
        });
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
