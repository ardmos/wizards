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
        //Debug.Log($"Current Scene : {sceneName}");
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
        if (GameAssets.instantiate.GetMusic(sceneName) == null)
        {
            audioSourceMusic.Stop();
            return;
        }

        audioSourceMusic.clip = GameAssets.instantiate.GetMusic(sceneName);
        audioSourceMusic.Play();
    }
}
