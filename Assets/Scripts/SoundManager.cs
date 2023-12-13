using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    public AudioSource audioSourceMusic;

    private void Start()
    {
        if (instance == null) instance = this;
        DontDestroyOnLoad(gameObject);
 
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

        UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;
        PlayMusic(sceneName);
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene current, UnityEngine.SceneManagement.Scene next)
    {
        UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();

        string sceneName = next.name;
        PlayMusic(sceneName);

        //Debug.Log($"Current Scene : {current.name}, {next.name}");
    }

    private void PlayMusic(string sceneName)
    {
        if (sceneName == "TitleScene")
        {
            audioSourceMusic.clip = GameAssets.instantiate.music_Title;
        }
        if (sceneName == "LobbyScene")
        {
            audioSourceMusic.clip = GameAssets.instantiate.music_Lobby;
        }
        audioSourceMusic.Play();
    }
}
