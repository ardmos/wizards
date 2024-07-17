using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// �ε����� Ȱ���� �ε��� �����ϴ� ��ũ��Ʈ �Դϴ�. �׸��� CleanUp ����� ���⿡ ��Ƽ�, Title�̳� Lobbyȭ������ �̵��� �ڵ� Ŭ���� �ǵ��� �ϴ°� ����غ���.
/// !!!���� ���
///     1. �ε��� ����
///     2. Ÿ�پ� ��׶��忡�� �ε�
/// </summary>
public static class LoadSceneManager
{
    // �ڷ�ƾ ����� ���� ���� Ŭ����
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
    /// �ε����� ���鼭 Ÿ�پ��� ��׶��忡�� �ε��ϰ� ����� �޼��� 
    /// </summary>
    /// <param name="targetScene">��׶��忡�� �ε���ų Ÿ�پ�</param>
    public static void Load(Scene targetScene)
    {
        // Ÿ�پ� ��׶��忡�� �ε�
        onLoaderCallback = () =>
        {
            GameObject loadingGameObject = new GameObject("Loading Game Object");
            loadingGameObject.AddComponent<LoadingMonoBehaviour>().StartCoroutine(LoadSceneAsync(targetScene));
        };

        // �ε��� ����
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    /// <summary>
    /// Lobby������ ���. ���� ȣ��Ʈ�� Ŭ���̾�Ʈ��� �Բ� �� �̵��� �Ϸ��� �̰� ����ؾ���. ��, �̰� ����ϸ� �ε�ȭ���� �ȶ��. ���� ���� �ʿ�
    /// </summary>
    /// <param name="targetScene"></param>
    public static void LoadNetwork(Scene targetScene)
    {
        NetworkManager.Singleton.SceneManager.LoadScene(targetScene.ToString(), LoadSceneMode.Single);
    }

    /// <summary>
    /// ���� �񵿱� �ε� ���� Ȯ���� ���� �ڷ�ƾ
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
    /// �ε����� ���α׷����ٿ��� ���� Ÿ�پ� �ε� �����Ȳ�� ��� ���� ȣ���� �޼���
    /// </summary>
    /// <returns>���� �ε� �����Ȳ�� �������ݴϴ�</returns>
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
    /// ���� �ε� ���� ȭ�鿡 �÷��ֱ� ���� �޼���. 
    ///     1. �ٸ� ���� ������Ʈ�� Update �Լ��κ��� ���� Update�� 1ȸ ȣ��޴´�.
    ///     2. ȣ������� Ÿ�� ���� �񵿱� �ε��� ���۽�Ų��.
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
