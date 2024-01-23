using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

#if UNITY_EDITOR
using ParrelSync;
#endif
#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

/// <summary>
/// Unity �����ý����� �����ϴ� ��ũ��Ʈ �Դϴ�. Ÿ��Ʋ�������� ���������� �����մϴ�
/// !!���� ���
///     1. Ŭ���̾�Ʈ�� ��� ���� �̰����� UnityServices ��� �غ�. ������ ���� ServerStartUp.cs���� ��. (UnityServices.Initialize) 
///     2. �͸�(Anonymous)�α��� ���� ó��
///     3. GooglePlay �α��� ���� ó��
/// </summary>
public class UnityAuthenticationManager : MonoBehaviour
{
    public static UnityAuthenticationManager Instance { get; private set; }

    public string Token;
    public string Error;

    // ���� await SignInCachedUserAsync(); ����� �� async�� �ٽ� ���
    //private async void Awake()
    private void Awake()
    {
        Instance = this;

        try
        {
            #if UNITY_ANDROID
            //Initialize PlayGamesPlatform
            PlayGamesPlatform.Activate();
            #endif

            // Ȥ�ó� �̹� Initialized�Ǿ��ִ°�쿣 �н�! �ƴ� ���� �̰��� ���� OnEnable���� Initialize �ȴ�
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                return;
            }

            //await SignInCachedUserAsync();  // �׽�Ʈ�߿��� �ּ�ó��. �ڵ��α��� ����. ���Ŀ� ���ÿ��� ServerStartUp ��ũ��Ʈ�� StartServerServices() Ÿ�̹��� �� ����ؼ� ����ϱ�.
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void OnEnable()
    {
        ReadyForUseUnityServices();
        //ServerStartUp.ClientInstance += ReadyForUseUnityServices;
    }

    private void OnDisable()
    {
        //ServerStartUp.ClientInstance -= ReadyForUseUnityServices;
    }

    /// <summary>
    /// ����Ƽ���񽺸� ����ϱ� ���� �غ� �޼����Դϴ�.
    /// </summary>
    private async void ReadyForUseUnityServices()
    {
        // ���ڷ� �Ѱ��ִ°� �׽�Ʈ��!.  �Ʒ� InitializeUnityServices�� �׽�Ʈ�� ���� ������ �� ���� �������ش�.
        await InitializeUnityServices("WizardsandKnightsPlayer");
    }
    private async Task InitializeUnityServices(string serviceProfileName = null)
    {
        #region �׽�Ʈ�� ����. ��ǻ�Ϳ��� �׽�Ʈ�� ���� ó��. �� ��ǻ�Ϳ��� �������� Ŭ���̾�Ʈ�� ����ֱ� ���� �غ� 
        if (serviceProfileName != null)
        {
#if UNITY_EDITOR
            serviceProfileName = $"{serviceProfileName}{GetCloneNumberSuffix()}";
#endif
            var initOptions = new InitializationOptions();
            initOptions.SetProfile(serviceProfileName);
            await UnityServices.InitializeAsync(initOptions);
        }
        #endregion
        else
        {
            //Initialize Unity Services. �̰� ���� ���⼭ �ϴ� ��.
            await UnityServices.InitializeAsync();
        }
        Debug.Log($"Unity Services Initialized as {serviceProfileName}({GetPlayerID()})");

        SetupEvents();
    }

    /// <summary>
    /// UnityServices�� �ʱ�ȭ�ϸ鼭 �߱޹��� AuthenticationService PlayerId�� �������ݴϴ�. 
    /// </summary>
    /// <returns>AuthenticationService.Instance.PlayerId</returns>
    public string GetPlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    #if UNITY_EDITOR
    /// <summary>
    /// �׽�Ʈ��. ��Ƽ�÷��� ��� �׽�Ʈ�� ���� ����Ŭ���̾�Ʈ�� �����ϰ� ���ݴϴ�.  
    /// </summary>
    /// <returns>���� Ŭ���̾�Ʈ ���α׷��� ������ ���ڸ� ��ȯ���ݴϴ�.</returns>
    private string GetCloneNumberSuffix()
    {
        string projectPath = ClonesManager.GetCurrentProjectPath();
        int lastUnderscore = projectPath.LastIndexOf('_');
        string projectCloneSuffix = projectPath.Substring(lastUnderscore + 1);
        if (projectCloneSuffix.Length != 1)
        {
            projectCloneSuffix = "";
        }
        return projectCloneSuffix;
    }
    #endif


    /// <summary>
    /// Google Play Games�� �α��� ó���� ���ݴϴ�.
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
    /// �α��� ����, ���� �� AuthenticationService ��Ȳ�� ���� �̺�Ʈ�� ������ݴϴ�.
    /// </summary>
    private void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

            Debug.Log("Sign in anonymously succeeded!");
            // �κ� ������ �̵�
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
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
    /// UnityServices �͸� �α��� ó���� ���ݴϴ�. 
    /// </summary>
    private async Task SignInAnonymouslyAsync()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();           
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
    /// UnityServices AuthenticationService�� �����ִ� ��ū�� Ȯ���� ��ū�� �����ִ°�� �ٷ� �α���ó���� ���ݴϴ�.
    /// �ڵ� �α��� �޼��� �Դϴ�.
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
