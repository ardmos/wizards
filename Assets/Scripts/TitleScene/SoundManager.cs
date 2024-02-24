using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// ���� ������ �����ص״ٰ� Ÿ��Ʋ�� ���Խÿ� �ε��ؼ� �����ؾ���.  <<< �����ʿ�
/// �� ���� ������ �׷��� ������ �Բ� ���̺� �ε� �ǵ��� �����ϱ�. 
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioSource audioSourceBGM;
    public AudioSource audioSourceSFX;

    public GameObject audioSourceObjectPrefab;

    public bool isVolumeUp;

    private Coroutine bgmCoroutine = null;

    [SerializeField] private SoundVolumeData volumeData;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;

        UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;

        if (string.IsNullOrEmpty(sceneName)) return;
        if (audioSourceBGM == null) return;
        if (audioSourceSFX == null) return;
        LoadVolumeData();
        volumeData = new SoundVolumeData(audioSourceBGM.volume, audioSourceSFX.volume);

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
        
        // Game�� BGM ���ҽ��� ������ ���� ����� �̰����� Ư���� ���������� ���ݴϴ�.
        if (isVolumeUp)
        {
            isVolumeUp = false;
            audioSourceBGM.volume /= 6;
        }
        if (!isVolumeUp && sceneName == LoadSceneManager.Scene.GameScene.ToString())
        {
            isVolumeUp = true;
            audioSourceBGM.volume *= 6;
        }

        if (bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            bgmCoroutine = null;
            Debug.Log("bgmCoroutine stopped");
        }

        bgmCoroutine = StartCoroutine(AutoPlay(sceneName));
        Debug.Log("bgmCoroutine started");
    }

    // �ڵ� ���� ��� ���
    private IEnumerator AutoPlay(string sceneName)
    {
        while (true)
        {
            Debug.Log("AutoPlay!");
            audioSourceBGM.clip = GameAssets.instantiate.GetMusic(sceneName);
            audioSourceBGM.Play();
            yield return new WaitForSeconds(audioSourceBGM.clip.length);
        }
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
        Debug.Log($"PlayMagicSFXClientRPC.  audioSourceObjectPrefab : {audioSourceObjectPrefab}");
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<AudioSourceObject>().Setup(GameAssets.instantiate.GetMagicSFXSound(spellName, state));
    }

    [ClientRpc]
    public void PlayItemSFXClientRPC(ItemName itemName)
    {
        Debug.Log($"PlayItemSFXClientRPC.  audioSourceObjectPrefab : {audioSourceObjectPrefab}");
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

    public void SetVolumeBGM(float volume) {
        if (audioSourceBGM == null) return;
        if (audioSourceSFX == null) return;
        if (volumeData == null) return;

        audioSourceBGM.volume = volume;
        volumeData.UpdateData(audioSourceBGM.volume, audioSourceSFX.volume);
        // SaveData�� ����.
        SaveSystem.SaveSoundVolumeData(volumeData);
    }

    public void SetVolumeSFX(float volume) {
        if (audioSourceBGM == null) return;
        if (audioSourceSFX == null) return;
        if (volumeData == null) return;  

        audioSourceSFX.volume = volume;
        volumeData.UpdateData(audioSourceBGM.volume, audioSourceSFX.volume);
        // SaveData�� ����.
        SaveSystem.SaveSoundVolumeData(volumeData);
    }

    public float GetVolumeBGM() {
        if (audioSourceBGM == null) { return 0f; }

        return audioSourceBGM.volume; 
    }

    public float GetVolumeSFX() { 
        if (audioSourceSFX == null) { return 0f; }

        return audioSourceSFX.volume; 
    }

    public SoundVolumeData GetSoundVolumeData()
    {
        if(volumeData == null) return null;
        return volumeData;
    }

    private void LoadVolumeData()
    {
        SoundVolumeData soundVolumeData = SaveSystem.LoadSoundVolumeData();
        if (soundVolumeData != null)
        {
            Debug.Log($"���� ���� ���� �ε� ����!");
            if (volumeData == null) return;

            this.volumeData = soundVolumeData;
            audioSourceBGM.volume = volumeData.BGMVolume;
            audioSourceSFX.volume = volumeData.SFXVolume;
            Debug.Log($"���� ���� ���� ���� ����!");
        }
        else
        {
            Debug.Log($"�ε��� ���� ���� ������ �����ϴ�.");
        }
    }
}
