using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    public AudioSource audioSourceBGM;
    public AudioSource audioSourceUI;

    public bool isVolumeUp;

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
            audioSourceBGM.Stop();
            return;
        }

        // Game씬 BGM 리소스가 볼륨이 작은 관계로 이곳에서 특별히 볼륨조절을 해줍니다.
        if (isVolumeUp)
        {
            isVolumeUp = false;
            audioSourceBGM.volume /= 6;
        }
        if (!isVolumeUp && sceneName == LoadingSceneManager.Scene.GameScene.ToString())
        {
            isVolumeUp = true;
            audioSourceBGM.volume *= 6;
        }


        audioSourceBGM.clip = GameAssets.instantiate.GetMusic(sceneName);
        audioSourceBGM.Play();
    }

    public void PlayButtonClickSound()
    {
        audioSourceUI.clip = GameAssets.instantiate.GetButtonClickSound();
        audioSourceUI.Play();
    }

    public void SetVolumeBGM(float volume) { audioSourceBGM.volume = volume; }
    public void SetVolumeUI(float volume) { audioSourceUI.volume = volume; }
    public float GetVolumeBGM() { return audioSourceBGM.volume; }
    public float GetVolumeUI() {  return audioSourceUI.volume; }
}
