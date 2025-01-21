using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
/// <summary>
/// 
/// </summary>
public class SoundManager : NetworkBehaviour, ICleanable
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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneCleanupManager.RegisterCleanableObject(this);
        }
        else Destroy(gameObject);
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

    public override void OnDestroy()
    {
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
    }

    private void SceneManager_activeSceneChanged(UnityEngine.SceneManagement.Scene current, UnityEngine.SceneManagement.Scene next)
    {
        //UnityEngine.SceneManagement.Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = next.name;

        // Game씬의 경우 씬이 시작되었을때 자동으로 BGM을 실행시키지 않고, GameState.GamePlaying 상태일 때 BGM을 실행시킵니다. GameManager에서 요청해올겁니다.
        if (sceneName == LoadSceneManager.Scene.GameScene_MultiPlayer.ToString())
        {
            audioSourceBGM.Stop();
            if (bgmCoroutine != null)
            {
                StopCoroutine(bgmCoroutine);
                bgmCoroutine = null;
            }
            return;
        }

        PlayMusic(sceneName);
    }

    public void PlayMusic(string sceneName)
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
            audioSourceBGM.Stop();
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

    public void PlayWizardSpellSFX(SpellName spellName, SFX_Type sFX_Type, Transform position)
    {
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<NetworkObject>().Spawn();
        audioSourceObject.GetComponent<AudioSourceObject>().SetupClientRPC(spellName, sFX_Type, position.position);
        Destroy(audioSourceObject, GameAssetsManager.Instance.GetSkillSFXSound(spellName, sFX_Type).length);
    }

    /// <summary>
    /// 서버 중심적으로 사운드 관리.  사운드 매니저를 서버하고 클라하고 구분할 필요가 있겠다. 
    /// </summary>
    /// <param name="spellName"></param>
    /// <param name="sFX_Type"></param>
    /// <param name="position">사운드 재생 위치</param>
    public void PlayKnightSkillSFX(SpellName spellName, SFX_Type sFX_Type, Transform position)
    {
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<NetworkObject>().Spawn();
        audioSourceObject.GetComponent<AudioSourceObject>().SetupClientRPC(spellName, sFX_Type, position.position);
        Destroy(audioSourceObject, GameAssetsManager.Instance.GetSkillSFXSound(spellName, sFX_Type).length);
    }

    public void PlayItemSFX(ItemName itemName, Transform position)
    {
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<NetworkObject>().Spawn();
        audioSourceObject.GetComponent<AudioSourceObject>().SetupClientRPC(itemName, position.position);
        Destroy(audioSourceObject, GameAssetsManager.Instance.GetItemSFXSound(itemName).length);
    }

    [ServerRpc (RequireOwnership = false)]
    public void PlayItemSFXServerRPC(ItemName itemName, Vector3 position)
    {
        if (audioSourceObjectPrefab == null) return;

        GameObject audioSourceObject = Instantiate(audioSourceObjectPrefab);
        audioSourceObject.GetComponent<NetworkObject>().Spawn();
        audioSourceObject.GetComponent<AudioSourceObject>().SetupClientRPC(itemName, position);
        Destroy(audioSourceObject, GameAssetsManager.Instance.GetItemSFXSound(itemName).length);
    }


    public void PlayCountdownAnnouncer(double countdownTime)
    {
        if (audioSourceSFX == null) return;

        audioSourceSFX.clip = GameAssetsManager.Instance.GetCountdownAnnouncerSFXSound((int)countdownTime);
        audioSourceSFX.Play();
    }

    public void PlayUISFX(UISFX_Type uISFX_Type)
    {
        //Debug.Log($"4.");
        if (audioSourceSFX == null) return;

        audioSourceSFX.clip = GameAssetsManager.Instance.GetUISFX(uISFX_Type);
        audioSourceSFX.Play();
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
        SoundVolumeData soundVolumeData = SaveSystem.LoadData<SoundVolumeData>();
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

    #region ICleanable 구현
    /// <summary>
    /// ICleanable 인터페이스 구현. 객체를 정리합니다.
    /// 이 메서드는 씬 전환 시 호출되어 현재 객체를 파괴합니다.
    /// </summary>
    public void Cleanup()
    {
        Destroy(gameObject);
    }
    #endregion
}
