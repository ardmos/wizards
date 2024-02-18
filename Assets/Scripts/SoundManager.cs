using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 사운드 볼륨값 저장해뒀다가 타이틀씬 진입시에 로드해서 적용해야함.  <<< 구현필요
/// 위 내용 구현시 그래픽 설정도 함께 세이브 로드 되도록 구현하기. 
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager instance { get; private set; }

    public AudioSource audioSourceBGM;
    public AudioSource audioSourceSFX;

    public GameObject audioSourceObjectPrefab;

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
        if (audioSourceBGM == null) return;

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

    private void StopMusic()
    {
        if (audioSourceBGM == null) return;

        audioSourceBGM.Stop();
    }

    public void PlayWinPopupSound()
    {
        if (audioSourceSFX == null) return;
        StopMusic();
        StartCoroutine(PlaySFXWithDelay(GameAssets.instantiate.GetWinSFXSound()));
    }

    public void PlayLosePopupSound()
    {
        if (audioSourceSFX == null) return;
        StopMusic();
        StartCoroutine(PlaySFXWithDelay(GameAssets.instantiate.GetLoseSFXSound()));
    }

    private IEnumerator PlaySFXWithDelay(AudioClip[] audioClips)
    {
        foreach (AudioClip audioClip in audioClips)
        {
            audioSourceSFX.clip = audioClip;
            audioSourceSFX.Play();
            Debug.Log($"PlaySFXWithDelay. audioClip:{audioClip.name}, {audioClip.length}");
            yield return new WaitForSeconds(audioClip.length);
        }
    }

    public void PlayButtonClickSound()
    {
        if (audioSourceSFX == null) return;

        audioSourceSFX.clip = GameAssets.instantiate.GetButtonClickSound();
        audioSourceSFX.Play();
    }

    public void PlayOpenScrollSound()
    {
        if (audioSourceSFX == null) return;

        audioSourceSFX.clip = GameAssets.instantiate.GetOpenScrollItemSFXSound();
        audioSourceSFX.Play();
    }

    [ClientRpc]
    public void PlayMagicSFXClientRPC(SpellName spellName, byte state)
    {
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<AudioSourceObject>().Setup(GameAssets.instantiate.GetMagicSFXSound(spellName, state));
    }

    [ClientRpc]
    public void PlayItemSFXClientRPC(ItemName itemName)
    {
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<AudioSourceObject>().Setup(GameAssets.instantiate.GetItemSFXSound(itemName));
    }

    public void PlayItemSFX(ItemName itemName)
    {
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<AudioSourceObject>().Setup(GameAssets.instantiate.GetItemSFXSound(itemName));
    }

    public void SetVolumeBGM(float volume) { audioSourceBGM.volume = volume; }
    public void SetVolumeSFX(float volume) { audioSourceSFX.volume = volume; }
    public float GetVolumeBGM() { return audioSourceBGM.volume; }
    public float GetVolumeSFX() {  return audioSourceSFX.volume; }
}
