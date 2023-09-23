using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
/// <summary>
/// Unity �����ý����� �����ϴ� ��ũ��Ʈ �Դϴ�.
/// !!���� ���
///     1. �͸�(Anonymous)�α��� ���� ó��
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
