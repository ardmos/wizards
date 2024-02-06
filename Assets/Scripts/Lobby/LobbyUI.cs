using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 캐릭터 위치 배치같은거는 Lobby Manager를 만들어서 처리하는게 더 어울려보인다. 
/// 이 스크립트는 UI들만 관리토록 하자.  정리 예약 
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
        // 상점기능 잠시 구현중. 주석처리.
        //btnShop.onClick.AddListener(() => { popupShop.Show(); });        
        btnMod.onClick.AddListener(() => { popupMod.Show(); });
        btnUserInfo.onClick.AddListener(() => { popupUserInfoUI.Show(); });
        btnSettings.onClick.AddListener(() => { popupSettingsUI.Show(); });

        // 지금은 사용하지 않는 버튼들입니다.
/*        btnClassChange.onClick.AddListener(ChangePlayerClass);
        btnEquipment.onClick.AddListener(() => { popupEquipment.Show(); });
        btnClan.onClick.AddListener(() => { popupClan.Show(); });
        btnInbox.onClick.AddListener(() => { popupInbox.Show(); });
        btnNews.onClick.AddListener(() => { popupNews.Show(); });*/

        // 임시로 현 스크립트(클라이언트)에서 player class 저장. 잘 가지고 있다가 Server가 Allocate 되면 GameMultiplyer의 서버RPC를 통해 서버에 넘겨준다. 
        // 이후에 UGS 클라우드 서버가 구축되면, 그곳에서 관리할것이다.
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
