using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// 
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
#if UNITY_SERVER
        Debug.Log("Server에서는 SoundManager를 실행하지 않습니다.");
        return;
#endif
        if (audioSourceBGM == null) return;

        if (GameAssetsManager.Instance.GetMusic(sceneName) == null)
        {
            audioSourceBGM.Stop();
            return;
        }
        
/*        // Game씬 BGM 리소스가 볼륨이 작은 관계로 이곳에서 특별히 볼륨조절을 해줍니다.
        if (isVolumeUp)
        {
            isVolumeUp = false;
            audioSourceBGM.volume /= 6;
        }
        if (!isVolumeUp && sceneName == LoadSceneManager.Scene.GameScene.ToString())
        {
            isVolumeUp = true;
            audioSourceBGM.volume *= 6;
        }*/

        if (bgmCoroutine != null)
        {
            StopCoroutine(bgmCoroutine);
            bgmCoroutine = null;
            //Debug.Log("bgmCoroutine stopped");
        }

        bgmCoroutine = StartCoroutine(AutoPlay(sceneName));
        //Debug.Log("bgmCoroutine started");
    }

    // 자동 다음 재생 기능
    private IEnumerator AutoPlay(string sceneName)
    {
        while (true)
        {
            //Debug.Log("AutoPlay!");
            audioSourceBGM.clip = GameAssetsManager.Instance.GetMusic(sceneName);
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
        StartCoroutine(PlaySequentialSFXWithDelay(GameAssetsManager.Instance.GetWinSFXSound()));
    }

    public void PlayLosePopupSound()
    {
        if (audioSourceSFX == null) return;
        StopMusic();
        StartCoroutine(PlaySequentialSFXWithDelay(GameAssetsManager.Instance.GetLoseSFXSound()));
    }

    private IEnumerator PlaySequentialSFXWithDelay(AudioClip[] audioClips)
    {
        foreach (AudioClip audioClip in audioClips)
        {
            audioSourceSFX.clip = audioClip;
            audioSourceSFX.Play();
            //Debug.Log($"PlaySFXWithDelay. audioClip:{audioClip.name}, {audioClip.length}");
            yield return new WaitForSeconds(audioClip.length);
        }
    }

    public void PlayButtonClickSound()
    {
        if (audioSourceSFX == null) return;

        audioSourceSFX.clip = GameAssetsManager.Instance.GetButtonClickSound();
        audioSourceSFX.Play();
    }

    public void PlayOpenScrollSound()
    {
        if (audioSourceSFX == null) return;

        audioSourceSFX.clip = GameAssetsManager.Instance.GetOpenScrollItemSFXSound();
        audioSourceSFX.Play();
    }

    [ClientRpc]
    public void PlayWizardSpellSFXClientRPC(SkillName spellName, SFX_Type sFX_Type)
    {
        //Debug.Log($"PlayMagicSFXClientRPC.  audioSourceObjectPrefab : {audioSourceObjectPrefab}");
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<AudioSourceObject>().Setup(GameAssetsManager.Instance.GetMagicSFXSound(spellName, sFX_Type));
    }

    [ClientRpc]
    public void PlayKnightSkillSFXClientRPC(SkillName spellName, SFX_Type sFX_Type)
    {
        //Debug.Log($"PlayMagicSFXClientRPC.  audioSourceObjectPrefab : {audioSourceObjectPrefab}");
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<AudioSourceObject>().Setup(GameAssetsManager.Instance.GetSkillSFXSound(spellName, sFX_Type));
    }

    [ClientRpc]
    public void PlayItemSFXClientRPC(ItemName itemName)
    {
        //Debug.Log($"PlayItemSFXClientRPC.  audioSourceObjectPrefab : {audioSourceObjectPrefab}, itemName : {itemName}, audioClip : {GameAssets.instantiate.GetItemSFXSound(itemName)}");
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<AudioSourceObject>().Setup(GameAssetsManager.Instance.GetItemSFXSound(itemName));
    }

    public void PlayItemSFX(ItemName itemName)
    {
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<AudioSourceObject>().Setup(GameAssetsManager.Instance.GetItemSFXSound(itemName));
    }

    public void PlayCountdownAnnouncer(double countdownTime)
    {
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<AudioSourceObject>().Setup(GameAssetsManager.Instance.GetCountdownAnnouncerSFXSound((int)countdownTime));
    }

    public void PlayUISFX(UISFX_Type uISFX_Type)
    {
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<AudioSourceObject>().Setup(GameAssetsManager.Instance.GetUISFX(uISFX_Type));
    }

    public void SetVolumeBGM(float volume) {
        if (audioSourceBGM == null) return;
        if (audioSourceSFX == null) return;
        if (volumeData == null) return;

        audioSourceBGM.volume = volume;
        volumeData.UpdateData(audioSourceBGM.volume, audioSourceSFX.volume);
        // SaveData에 저장.
        SaveSystem.SaveSoundVolumeData(volumeData);
    }

    public void SetVolumeSFX(float volume) {
        if (audioSourceBGM == null) return;
        if (audioSourceSFX == null) return;
        if (volumeData == null) return;  

        audioSourceSFX.volume = volume;
        volumeData.UpdateData(audioSourceBGM.volume, audioSourceSFX.volume);
        // SaveData에 저장.
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
            Debug.Log($"사운드 볼륨 정보 로드 성공!");
            if (volumeData == null) return;

            this.volumeData = soundVolumeData;
            audioSourceBGM.volume = volumeData.BGMVolume;
            audioSourceSFX.volume = volumeData.SFXVolume;
            Debug.Log($"사운드 볼륨 정보 적용 성공!");
        }
        else
        {
            Debug.Log($"로드할 사운드 볼륨 정보가 없습니다.");
        }
    }
}
