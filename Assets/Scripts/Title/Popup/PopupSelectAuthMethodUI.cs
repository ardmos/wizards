using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 타이틀씬 로그인 인증 방법 선택 팝업(PopupSelectAuthMethod)을 컨트롤하는 스크립트 입니다.
/// !!현재 기능
///     1. 외곽 어두운 영역(BtnCloseBackShadow) 클릭시 팝업 닫기
/// </summary>

public class PopupSelectAuthMethodUI : MonoBehaviour
{
    [SerializeField] private Button btnSignInAnonymous;
    [SerializeField] private Button btnSignInGooglePlayGames;
    [SerializeField] private Button btnClose;

    private void Start()
    {
        btnSignInAnonymous.onClick.AddListener(UnityAuthenticationManager.Instance.OnBtnSignInAnonymousClicked);
        btnSignInGooglePlayGames.onClick.AddListener(UnityAuthenticationManager.Instance.OnBtnSignInGooglePlayGamesClicked);
        btnClose.onClick.AddListener(OnBtnCloseClicked);
    }

    private void OnBtnCloseClicked()
    {
        gameObject.transform.localScale = Vector3.zero;
    }
}
