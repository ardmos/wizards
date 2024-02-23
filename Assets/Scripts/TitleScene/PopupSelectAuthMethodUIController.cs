using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ÿ��Ʋ�� �α��� ���� ��� ���� �˾�(PopupSelectAuthMethod)�� ��Ʈ���ϴ� ��ũ��Ʈ �Դϴ�.
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
