using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ĳ���� ��ġ ��ġ�����Ŵ� Lobby Manager�� ���� ó���ϴ°� �� �������δ�. 
/// �� ��ũ��Ʈ�� UI�鸸 ������� ����.  ���� ���� 
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
        // ������� ��� ������. �ּ�ó��.
        //btnShop.onClick.AddListener(() => { popupShop.Show(); });        
        btnSinglePlayer.AddClickListener(() => {
            // �̱۸�� �� �ε�!
            LoadSceneManager.Load(LoadSceneManager.Scene.GameScene_SinglePlayer);
        });
        // Ŭ���̾�Ʈ ��Ī ���� (��ġ����ŷ �κ� �ڵ� ������. 02/22)
        btnMultiPlayer.AddClickListener(matchmakerClient.StartMatchmaking);
        btnUserInfo.AddClickListener(popupUserInfoUI.Show);
        btnSettings.AddClickListener(popupSettingsUI.Show);
        btnChangeCharacter.AddClickListener(popupSelectCharacterUI.Show);

        // ������ ������� �ʴ� ��ư���Դϴ�.
        /*        
                btnEquipment.onClick.AddListener(() => { popupEquipment.Show(); });
                btnClan.onClick.AddListener(() => { popupClan.Show(); });
                btnInbox.onClick.AddListener(() => { popupInbox.Show(); });
                btnNews.onClick.AddListener(() => { popupNews.Show(); });*/

        // �ӽ÷� �� ��ũ��Ʈ(Ŭ���̾�Ʈ)���� player class ����. �� ������ �ִٰ� Server�� Allocate �Ǹ� GameMultiplyer�� ����RPC�� ���� ������ �Ѱ��ش�. 
        // ���Ŀ� UGS Ŭ���� ������ ����Ǹ�, �װ����� �����Ұ��̴�.
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
    ///  �Ʒ� Ŭ���� ���� ������ Ŭ������ũ��Ʈ�� ���� ���� �����ϴ°��� ���� ����  �� ����! ��·�� ������ �Ⱦ�. ����Ʈ �߰� �Ŀ� ��� ����!
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
