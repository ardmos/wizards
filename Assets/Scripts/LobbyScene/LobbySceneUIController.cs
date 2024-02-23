using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ĳ���� ��ġ ��ġ�����Ŵ� Lobby Manager�� ���� ó���ϴ°� �� �������δ�. 
/// �� ��ũ��Ʈ�� UI�鸸 ������� ����.  ���� ���� 
/// </summary>
public class LobbySceneUIController : MonoBehaviour
{
    // User Info UI
    public CustomButton btnUserInfo;
    public TextMeshProUGUI txtUserName;     // [SerializeField] private ---> public

    [SerializeField] private CustomButton btnSettings;
    [SerializeField] private CustomButton btnClassChange;
    [SerializeField] private CustomButton btnShop;
    [SerializeField] private CustomButton btnEquipment;
    [SerializeField] private CustomButton btnSkill;
    [SerializeField] private CustomButton btnClan;
    [SerializeField] private CustomButton btnMod;
    [SerializeField] private CustomButton btnStartPlay;

    [SerializeField] private CustomButton btnInbox;
    [SerializeField] private CustomButton btnNews;

    [SerializeField] private PopupUserInfoUIController popupUserInfoUI;
    [SerializeField] private PopupSettingsUIController popupSettingsUI;
    [SerializeField] private PopupShopUIController popupShop;
    [SerializeField] private PopupEquipmentUIController popupEquipment;
    [SerializeField] private PopupClanUIController popupClan;
    [SerializeField] private PopupModUIController popupMod;
    [SerializeField] private PopupNameChangeUIController popupNameChangeUI;
    [SerializeField] private PopupInboxUIController popupInbox;
    [SerializeField] private PopupNewsUIController popupNews;

    [SerializeField] private GameObject characterPos;
    [SerializeField] private GameObject character;

    [SerializeField] private Sprite iconWizard, iconKnight;

    [SerializeField] private MatchmakerClient matchmakerClient;

    // Start is called before the first frame update
    void Start()
    {
        // ������� ��� ������. �ּ�ó��.
        //btnShop.onClick.AddListener(() => { popupShop.Show(); });        
        btnMod.AddClickListener(popupMod.Show);
        // Ŭ���̾�Ʈ ��Ī ���� (��ġ����ŷ �κ� �ڵ� ������. 02/22)
        btnStartPlay.AddClickListener(matchmakerClient.StartMatchmaking);
        btnUserInfo.AddClickListener(popupUserInfoUI.Show);
        btnSettings.AddClickListener(popupSettingsUI.Show);

        // ������ ������� �ʴ� ��ư���Դϴ�.
        /*        btnClassChange.onClick.AddListener(ChangePlayerClass);
                btnEquipment.onClick.AddListener(() => { popupEquipment.Show(); });
                btnClan.onClick.AddListener(() => { popupClan.Show(); });
                btnInbox.onClick.AddListener(() => { popupInbox.Show(); });
                btnNews.onClick.AddListener(() => { popupNews.Show(); });*/

        // �ӽ÷� �� ��ũ��Ʈ(Ŭ���̾�Ʈ)���� player class ����. �� ������ �ִٰ� Server�� Allocate �Ǹ� GameMultiplyer�� ����RPC�� ���� ������ �Ѱ��ش�. 
        // ���Ŀ� UGS Ŭ���� ������ ����Ǹ�, �װ����� �����Ұ��̴�.
        //PlayerProfileData.Instance.SetCurrentSelectedClass(CharacterClasses.Class.Wizard);
        UpdatePlayerClass();
        UpdateUserInfoUI();
    }


    //// User Info UI
    ///
    public void UpdateUserInfoUI()
    {
        // 1. Update User Name From PlayerProfileDataManager
        txtUserName.text = PlayerDataManager.Instance.GetPlayerName();
    }


    /// <summary>
    ///  �Ʒ� Ŭ���� ���� ������ Ŭ������ũ��Ʈ�� ���� ���� �����ϴ°��� ���� ����  �� ����! ��·�� ������ �Ⱦ�. ����Ʈ �߰� �Ŀ� ��� ����!
    /// </summary>
    private void ChangePlayerClass()
    {
        switch (PlayerDataManager.Instance.GetCurrentPlayerClass())
        {
            case CharacterClass.Wizard:
                PlayerDataManager.Instance.SetCurrentPlayerClass(CharacterClass.Knight);
                break;
            case CharacterClass.Knight:
                PlayerDataManager.Instance.SetCurrentPlayerClass(CharacterClass.Wizard);
                break;
            default:
                Debug.LogError("UpdateCurrentClass() Class Error");
                break;
        }

        UpdatePlayerClass();      
    }

    private void UpdatePlayerClass()
    {
        if (character != null) { Destroy(character); }        

        switch (PlayerDataManager.Instance.GetCurrentPlayerClass())
        {
            case CharacterClass.Wizard:
                btnClassChange.GetComponentsInChildren<Image>()[1].sprite = iconWizard;
                character = Instantiate(GameAssets.instantiate.GetCharacterPrefab_NotInGame(PlayerDataManager.Instance.GetCurrentPlayerClass()));
                break; 
            case CharacterClass.Knight:
                btnClassChange.GetComponentsInChildren<Image>()[1].sprite = iconKnight;
                character = Instantiate(GameAssets.instantiate.GetCharacterPrefab_NotInGame(PlayerDataManager.Instance.GetCurrentPlayerClass()));
                break; 
            default:
                Debug.LogError("UpdateCurrentClass() Class Error");
                break;
        }
        character.transform.SetParent(characterPos.transform);
        character.transform.localPosition = Vector3.zero;
        character.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);        
    }

    public PopupNameChangeUIController GetPopupNameChangeUIController()
    {
        return popupNameChangeUI;
    }
}