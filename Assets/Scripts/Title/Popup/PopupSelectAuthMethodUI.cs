using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ÿ��Ʋ�� �α��� ���� ��� ���� �˾�(PopupSelectAuthMethod)�� ��Ʈ���ϴ� ��ũ��Ʈ �Դϴ�.
/// !!���� ���
///     1. �ܰ� ��ο� ����(BtnCloseBackShadow) Ŭ���� �˾� �ݱ�
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
