using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
/// <summary>
/// Unity 인증시스템을 관리하는 스크립트 입니다.
/// !!현재 기능
///     1. 익명(Anonymous)로그인 인증 처리
/// </summary>

public class UnityAuthenticationManager : MonoBehaviour
{
    public string Token;
    public string Error;

    private async void Awake()
    {
        try
        {
            await UnityServices.InitializeAsync();
            Debug.Log(UnityServices.State);

            SetupEvents();

            //await SignInCachedUserAsync();   테스트중에는 주석처리. 자동로그인 안함

            //Initialize PlayGamesPlatform
            PlayGamesPlatform.Activate();
            //LoginGooglePlayGames(); 버튼 클릭시 실행
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
        
    }

    /// <summary>
    /// 구글플레이게임즈 로그인
    /// </summary>
    public void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate((success) =>
        {
            if (success == SignInStatus.Success)
            {
                Debug.Log("Login with Google Play games successful.");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, code =>
                {
                    Debug.Log("Authorization code: " + code);
                    Token = code;
                    // This token serves as an example to be used for SignInWithGooglePlayGames
                });

                // 로비 씬으로 이동
                MoveToTheLobbyScene();
            }
            else
            {
                Error = "Failed to retrieve Google play games authorization code";
                Debug.Log("Login Unsuccessful");
            }
        });
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

            // 로비 씬으로 이동
            MoveToTheLobbyScene();
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

    /// <summary>
    /// 로그인 캐쉬 확인 후 처리
    /// </summary>
    async Task SignInCachedUserAsync()
    {
        // Check if a cached player already exists by checking if the session token exists
        if (!AuthenticationService.Instance.SessionTokenExists)
        {
            // if not, then do nothing
            return;
        }

        // Sign in Anonymously
        // This call will sign in the cached player.
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // 로비 씬으로 이동
            MoveToTheLobbyScene();
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

    // 이건 로드씬 매니저 따로 만들어서 처리하도록 수정하기
    private void MoveToTheLobbyScene()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}
