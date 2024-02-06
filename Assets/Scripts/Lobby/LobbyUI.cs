using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ĳ���� ��ġ ��ġ�����Ŵ� Lobby Manager�� ���� ó���ϴ°� �� �������δ�. 
/// �� ��ũ��Ʈ�� UI�鸸 ������� ����.  ���� ���� 
/// </summary>
public class LobbyUI : MonoBehaviour
{
    // User Info UI
    public Button btnUserInfo;
    public TextMeshProUGUI txtUserName;     // [SerializeField] private ---> public

    [SerializeField] private Button btnSettings;
    [SerializeField] private Button btnClassChange;
    [SerializeField] private Button btnShop;
    [SerializeField] private Button btnEquipment;
    [SerializeField] private Button btnSkill;
    [SerializeField] private Button btnClan;
    [SerializeField] private Button btnMod;

    [SerializeField] private Button btnInbox;
    [SerializeField] private Button btnNews;

    [SerializeField] private PopupUserInfoUI popupUserInfoUI;
    [SerializeField] private PopupSettingsUI popupSettingsUI;
    [SerializeField] private PopupShopUI popupShop;
    [SerializeField] private PopupEquipmentUI popupEquipment;
    [SerializeField] private PopupClanUI popupClan;
    [SerializeField] private PopupModUI popupMod;

    [SerializeField] private PopupInboxUI popupInbox;
    [SerializeField] private PopupInboxUI popupNews;

    [SerializeField] private GameObject characterPos;
    [SerializeField] private GameObject character;

    [SerializeField] private Sprite iconWizard, iconKnight;

    public PopupNameChangeUI popupNameChangeUI;

    // Start is called before the first frame update
    void Start()
    {        
        // ������� ��� ������. �ּ�ó��.
        //btnShop.onClick.AddListener(() => { popupShop.Show(); });        
        btnMod.onClick.AddListener(() => { popupMod.Show(); });
        btnUserInfo.onClick.AddListener(() => { popupUserInfoUI.Show(); });
        btnSettings.onClick.AddListener(() => { popupSettingsUI.Show(); });

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
    /// //////
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
}
