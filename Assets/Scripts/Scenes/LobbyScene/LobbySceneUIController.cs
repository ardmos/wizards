using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 캐릭터 위치 배치같은거는 Lobby Manager를 만들어서 처리하는게 더 어울려보인다. 
/// 이 스크립트는 UI들만 관리토록 하자.  정리 예약 
/// </summary>
public class LobbySceneUIController : MonoBehaviour
{
    // User Info UI
    public CustomClickSoundButton btnUserInfo;
    public TextMeshProUGUI txtUserName;     // [SerializeField] private ---> public

    [SerializeField] private CustomClickSoundButton btnSettings;
    [SerializeField] private CustomClickSoundButton btnChangeCharacter;
    [SerializeField] private CustomClickSoundButton btnShop;
    [SerializeField] private CustomClickSoundButton btnEquipment;
    [SerializeField] private CustomClickSoundButton btnSkill;
    [SerializeField] private CustomClickSoundButton btnClan;
    [SerializeField] private CustomClickSoundButton btnSinglePlayer;
    [SerializeField] private CustomClickSoundButton btnMultiPlayer;

    [SerializeField] private CustomClickSoundButton btnInbox;
    [SerializeField] private CustomClickSoundButton btnNews;

    [SerializeField] private PopupUserInfoUIController popupUserInfoUI;
    [SerializeField] private PopupSettingsUIController popupSettingsUI;
    [SerializeField] private PopupShopUIController popupShop;
    [SerializeField] private PopupEquipmentUIController popupEquipment;
    [SerializeField] private PopupClanUIController popupClan;
    [SerializeField] private PopupModUIController popupMod;
    [SerializeField] private PopupNameChangeUIController popupNameChangeUI;
    [SerializeField] private PopupInboxUIController popupInbox;
    [SerializeField] private PopupNewsUIController popupNews;

    [SerializeField] private PopupSelectCharacterUIController popupSelectCharacterUI;

    [SerializeField] private GameObject characterPos;
    [SerializeField] private GameObject character;

    [SerializeField] private Sprite iconWizard, iconKnight;

    [SerializeField] private MatchmakerClient matchmakerClient;

    // Start is called before the first frame update
    void Start()
    {
        // 상점기능 잠시 구현중. 주석처리.
        //btnShop.onClick.AddListener(() => { popupShop.Show(); });        
        btnSinglePlayer.AddClickListener(() => {
            // 싱글모드 맵 로드!
            LoadSceneManager.Load(LoadSceneManager.Scene.GameScene_SinglePlayer);
        });
        // 클라이언트 매칭 시작 (매치메이킹 부분 코드 정리중. 02/22)
        btnMultiPlayer.AddClickListener(matchmakerClient.StartMatchmaking);
        btnUserInfo.AddClickListener(popupUserInfoUI.Show);
        btnSettings.AddClickListener(popupSettingsUI.Show);
        btnChangeCharacter.AddClickListener(popupSelectCharacterUI.Show);

        // 지금은 사용하지 않는 버튼들입니다.
        /*        
                btnEquipment.onClick.AddListener(() => { popupEquipment.Show(); });
                btnClan.onClick.AddListener(() => { popupClan.Show(); });
                btnInbox.onClick.AddListener(() => { popupInbox.Show(); });
                btnNews.onClick.AddListener(() => { popupNews.Show(); });*/

        // 임시로 현 스크립트(클라이언트)에서 player class 저장. 잘 가지고 있다가 Server가 Allocate 되면 GameMultiplyer의 서버RPC를 통해 서버에 넘겨준다. 
        // 이후에 UGS 클라우드 서버가 구축되면, 그곳에서 관리할것이다.
        //PlayerProfileData.Instance.SetCurrentSelectedClass(CharacterClasses.Class.Wizard);
        UpdatePlayerCharacter();
        UpdateUserInfoUI();
    }


    //// User Info UI
    ///
    public void UpdateUserInfoUI()
    {
        // 1. Update User Name From PlayerProfileDataManager
        txtUserName.text = LocalPlayerDataManagerClient.Instance.GetPlayerName();
    }

    /// <summary>
    ///  아래 클래스 변경 내용은 클래스스크립트를 따로 만들어서 구현하는것이 보기 좋을  것 같다! 어쨌든 지금은 안씀. 나이트 추가 후에 사용 예정!
    /// </summary>
    public void ChangePlayerCharacter(Character character)
    {
        LocalPlayerDataManagerClient.Instance.SetCurrentPlayerClass(character);

        UpdatePlayerCharacter();
    }

    private void UpdatePlayerCharacter()
    {
        if (character != null) { Destroy(character); }

        switch (LocalPlayerDataManagerClient.Instance.GetCurrentPlayerClass())
        {
            case Character.Wizard:
                btnChangeCharacter.GetComponentsInChildren<Image>()[1].sprite = iconWizard;
                character = Instantiate(GameAssetsManager.Instance.GetCharacterPrefab_NotInGame(LocalPlayerDataManagerClient.Instance.GetCurrentPlayerClass()));
                break;
            case Character.Knight:
                btnChangeCharacter.GetComponentsInChildren<Image>()[1].sprite = iconKnight;
                character = Instantiate(GameAssetsManager.Instance.GetCharacterPrefab_NotInGame(LocalPlayerDataManagerClient.Instance.GetCurrentPlayerClass()));
                break;
            default:
                Debug.LogError("UpdateCurrentClass() Class Error");
                break;
        }
        character.transform.SetParent(characterPos.transform);
        character.transform.localPosition = Vector3.zero;
        character.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    public void PlayAnimCharacterVictory()
    {
        character.GetComponentInChildren<Animator>().SetTrigger("IsVictory");
    }

    public GameObject GetSelectedCharacter3DObject() { return character; }

    public PopupNameChangeUIController GetPopupNameChangeUIController()
    {
        return popupNameChangeUI;
    }
}
