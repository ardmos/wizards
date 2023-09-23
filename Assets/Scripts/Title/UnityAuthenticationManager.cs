using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
/// <summary>
/// Unity 인증시스템을 관리하는 스크립트 입니다.
/// !!현재 기능
///     1. 익명(Anonymous)로그인 인증 처리
/// </summary>

public class UnityAuthenticationManager : MonoBehaviour
{

    private async void Awake()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log(UnityServices.State);

            SetupEvents();
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
        
    }

    /// <summary>
    /// SignIn, SignOut 등 이벤트 등록
    /// </summary>
    private void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
        };

        AuthenticationService.Instance.SignInFailed += (err) =>
        {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired");
        };
    }

    /// <summary>
    /// 익명 로그인
    /// </summary>
    /// <returns> 익명 로그인 결과 </returns>
    private async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Debug.LogException(ex);
        }
    }

    public async void OnBtnSignInAnonymousClicked()
    {
        await SignInAnonymouslyAsync();
    }
}
