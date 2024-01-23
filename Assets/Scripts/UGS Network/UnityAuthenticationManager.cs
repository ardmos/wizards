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
/// Unity 인증시스템을 관리하는 스크립트 입니다. 타이틀씬에서의 인증과정을 관리합니다
/// !!현재 기능
///     1. 클라이언트인 경우 한정 이곳에서 UnityServices 사용 준비. 서버의 경우는 ServerStartUp.cs에서 함. (UnityServices.Initialize) 
///     2. 익명(Anonymous)로그인 인증 처리
///     3. GooglePlay 로그인 인증 처리
/// </summary>
public class UnityAuthenticationManager : MonoBehaviour
{
    public static UnityAuthenticationManager Instance { get; private set; }

    public string Token;
    public string Error;

    // 내부 await SignInCachedUserAsync(); 사용할 때 async도 다시 사용
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

            // 혹시나 이미 Initialized되어있는경우엔 패스! 아닌 경우는 이곳을 지나 OnEnable에서 Initialize 된다
            if (UnityServices.State == ServicesInitializationState.Initialized)
            {
                return;
            }

            //await SignInCachedUserAsync();  // 테스트중에는 주석처리. 자동로그인 안함. 추후에 사용시에는 ServerStartUp 스크립트의 StartServerServices() 타이밍을 잘 고려해서 사용하기.
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
    /// 유니티서비스를 사용하기 위한 준비 메서드입니다.
    /// </summary>
    private async void ReadyForUseUnityServices()
    {
        // 인자로 넘겨주는건 테스트용!.  아래 InitializeUnityServices의 테스트용 구간 삭제할 때 같이 삭제해준다.
        await InitializeUnityServices("WizardsandKnightsPlayer");
    }
    private async Task InitializeUnityServices(string serviceProfileName = null)
    {
        #region 테스트용 구간. 컴퓨터에서 테스트를 위한 처리. 한 컴퓨터에서 여러개의 클라이언트를 띄워주기 위한 준비 
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
            //Initialize Unity Services. 이건 원래 여기서 하는 일.
            await UnityServices.InitializeAsync();
        }
        Debug.Log($"Unity Services Initialized as {serviceProfileName}({GetPlayerID()})");

        SetupEvents();
    }

    /// <summary>
    /// UnityServices를 초기화하면서 발급받은 AuthenticationService PlayerId를 리턴해줍니다. 
    /// </summary>
    /// <returns>AuthenticationService.Instance.PlayerId</returns>
    public string GetPlayerID()
    {
        return AuthenticationService.Instance.PlayerId;
    }

    #if UNITY_EDITOR
    /// <summary>
    /// 테스트용. 멀티플레이 기능 테스트를 위한 다중클라이언트가 가능하게 해줍니다.  
    /// </summary>
    /// <returns>현재 클라이언트 프로그램의 마지막 글자를 반환해줍니다.</returns>
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
    /// Google Play Games의 로그인 처리를 해줍니다.
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

                // 로비 씬으로 이동
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
    /// 로그인 성공, 실패 등 AuthenticationService 상황에 따른 이벤트를 등록해줍니다.
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
            // 로비 씬으로 이동
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
    /// UnityServices 익명 로그인 처리를 해줍니다. 
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
    /// UnityServices AuthenticationService에 남아있는 토큰을 확인해 토큰이 남아있는경우 바로 로그인처리를 해줍니다.
    /// 자동 로그인 메서드 입니다.
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

            // 로비 씬으로 이동
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
