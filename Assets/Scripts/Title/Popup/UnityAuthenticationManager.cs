using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif
/// <summary>
/// Unity �����ý����� �����ϴ� ��ũ��Ʈ �Դϴ�. Ÿ��Ʋ�������� ���������� �����մϴ�
/// !!���� ���
///     1. �͸�(Anonymous)�α��� ���� ó��
/// </summary>

public class UnityAuthenticationManager : MonoBehaviour
{
    public static UnityAuthenticationManager Instance { get; private set; }

    public string Token;
    public string Error;

    private async void Awake()
    {
        Instance = this;

        try
        {
#if UNITY_ANDROID
            //Initialize PlayGamesPlatform
            PlayGamesPlatform.Activate();
#endif

            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                return;
            }
            // �� ��ǻ�Ϳ��� ���� ȭ�� ������� �׽�Ʈ �ϱ� ���� �ڵ�           
            InitializationOptions initializationOptions = new InitializationOptions();
#if !DEDICATED_SERVER
            initializationOptions.SetProfile(UnityEngine.Random.Range(0, 10000).ToString());
#endif
            await UnityServices.InitializeAsync(initializationOptions);
            Debug.Log(UnityServices.State);

            SetupEvents();

            //await SignInCachedUserAsync();  // �׽�Ʈ�߿��� �ּ�ó��. �ڵ��α��� ����


        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    /// <summary>
    /// �����÷��̰����� �α���
    /// </summary>
    private void LoginGooglePlayGames()
    {
#if UNITY_ANDROID
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
                LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
            }
            else
            {
                Error = "Failed to retrieve Google play games authorization code";
                Debug.Log("Login Unsuccessful");
            }
        });
#endif
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
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
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
    private async Task SignInCachedUserAsync()
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
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
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

    public void OnBtnSignInGooglePlayGamesClicked()
    {
        LoginGooglePlayGames();
    }
}
