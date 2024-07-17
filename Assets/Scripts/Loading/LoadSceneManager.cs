using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 로딩씬을 활용한 로딩을 관리하는 스크립트 입니다. 그리고 CleanUp 기능을 여기에 담아서, Title이나 Lobby화면으로 이동시 자동 클린업 되도록 하는게 깔끔해보임.
/// !!!현재 기능
///     1. 로딩씬 열기
///     2. 타겟씬 백그라운드에서 로딩
/// </summary>
public static class LoadSceneManager
{
    // 코루틴 사용을 위한 더미 클래스
    private class LoadingMonoBehaviour : MonoBehaviour { }
    public enum Scene
    {
        TitleScene,
        LoadingScene,
        LobbyScene,
        GameScene_MultiPlayer,
        GameScene_SinglePlayer
    }

    private static Action onLoaderCallback;
    private static AsyncOperation loadingAsyncOperation;

    /// <summary>
    /// 로딩씬을 열면서 타겟씬을 백그라운드에서 로딩하게 만드는 메서드 
    /// </summary>
    /// <param name="targetScene">백그라운드에서 로딩시킬 타겟씬</param>
    public static void Load(Scene targetScene)
    {
        // 타겟씬 백그라운드에서 로딩
        onLoaderCallback = () =>
        {
            GameObject loadingGameObject = new GameObject("Loading Game Object");
            loadingGameObject.AddComponent<LoadingMonoBehaviour>().StartCoroutine(LoadSceneAsync(targetScene));
        };

        // 로딩씬 열기
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    /// <summary>
    /// Lobby씬에서 사용. 게임 호스트가 클라이언트들과 함께 씬 이동을 하려면 이걸 사용해야함. 단, 이걸 사용하면 로딩화면은 안뜬다. 추후 구현 필요
    /// </summary>
    /// <param name="targetScene"></param>
    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    /// <summary>
    /// 씬의 비동기 로딩 상태 확인을 위한 코루틴
    /// </summary>
    /// <param name="targetScene"></param>
    /// <returns></returns>
    private static IEnumerator LoadSceneAsync(Scene targetScene)
    {
        yield return null;
        loadingAsyncOperation = SceneManager.LoadSceneAsync(targetScene.ToString());

        while (!loadingAsyncOperation.isDone)
        {
            yield return null;
        }
    }

    /// <summary>
    /// 로딩씬의 프로그레스바에서 현재 타겟씬 로딩 진행상황을 얻기 위해 호출할 메서드
    /// </summary>
    /// <returns>현재 로딩 진행상황을 리턴해줍니다</returns>
    public static float GetLoadingProgress()
    {
        if(loadingAsyncOperation != null)
        {
            return loadingAsyncOperation.progress;
        } else
        {
            return 0f;
        }
    }

    /// <summary>
    /// 열린 로딩 씬을 화면에 올려주기 위한 메서드. 
    ///     1. 다른 게임 오브젝트의 Update 함수로부터 최초 Update시 1회 호출받는다.
    ///     2. 호출받으면 타겟 씬의 비동기 로딩을 시작시킨다.
    /// </summary>
    public static void LoaderCallback()
    {
        if(onLoaderCallback != null)
        {
            onLoaderCallback();
            onLoaderCallback = null;
        }
    }
}
