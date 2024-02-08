using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// ���� ������ �����ص״ٰ� Ÿ��Ʋ�� ���Խÿ� �ε��ؼ� �����ؾ���.  <<< �����ʿ�
/// �� ���� ������ �׷��� ������ �Բ� ���̺� �ε� �ǵ��� �����ϱ�. 
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    public AudioSource audioSourceBGM;
    public AudioSource audioSourceSFX;

    public bool isVolumeUp;

    private void Awake()
    {
        if (instance == null) instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

        UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;

        PlayMusic(sceneName);
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene current, UnityEngine.SceneManagement.Scene next)
    {
        UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();

        string sceneName = next.name;
        PlayMusic(sceneName);
    }

    private void PlayMusic(string sceneName)
    {
        if (GameAssets.instantiate.GetMusic(sceneName) == null)
        {
            audioSourceBGM.Stop();
            return;
        }

        // Game�� BGM ���ҽ��� ������ ���� ����� �̰����� Ư���� ���������� ���ݴϴ�.
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
        audioSourceSFX.clip = GameAssets.instantiate.GetButtonClickSound();
        audioSourceSFX.Play();
    }

    public void SetVolumeBGM(float volume) { audioSourceBGM.volume = volume; }
    public void SetVolumeSFX(float volume) { audioSourceSFX.volume = volume; }
    public float GetVolumeBGM() { return audioSourceBGM.volume; }
    public float GetVolumeSFX() {  return audioSourceSFX.volume; }
}
