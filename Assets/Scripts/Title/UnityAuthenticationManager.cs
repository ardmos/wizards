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
/// Unity �����ý����� �����ϴ� ��ũ��Ʈ �Դϴ�.
/// !!���� ���
///     1. �͸�(Anonymous)�α��� ���� ó��
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

            //await SignInCachedUserAsync();   �׽�Ʈ�߿��� �ּ�ó��. �ڵ��α��� ����

            //Initialize PlayGamesPlatform
            PlayGamesPlatform.Activate();
            //LoginGooglePlayGames(); ��ư Ŭ���� ����
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
        
    }

    /// <summary>
    /// �����÷��̰����� �α���
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

                // �κ� ������ �̵�
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
    /// SignIn, SignOut �� �̺�Ʈ ���
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
    /// �͸� �α���
    /// </summary>
    /// <returns> �͸� �α��� ��� </returns>
    private async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Sign in anonymously succeeded!");

            // Shows how to get the playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // �κ� ������ �̵�
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
    /// �α��� ĳ�� Ȯ�� �� ó��
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

            // �κ� ������ �̵�
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

    // �̰� �ε�� �Ŵ��� ���� ���� ó���ϵ��� �����ϱ�
    private void MoveToTheLobbyScene()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}
