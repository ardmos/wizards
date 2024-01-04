using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private Button btnUserInfo;
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

    // Start is called before the first frame update
    void Start()
    {
        btnUserInfo.onClick.AddListener(() => { popupUserInfoUI.Show(); });
        btnSettings.onClick.AddListener(() => { popupSettingsUI.Show(); });
        btnClassChange.onClick.AddListener(ChangePlayerClass);
        btnShop.onClick.AddListener(() => { popupShop.Show(); });
        btnEquipment.onClick.AddListener(() => { popupEquipment.Show(); });
        btnClan.onClick.AddListener(() => { popupClan.Show(); });
        btnMod.onClick.AddListener(() => { popupMod.Show(); });

        btnInbox.onClick.AddListener(() => { popupInbox.Show(); });
        btnNews.onClick.AddListener(() => { popupNews.Show(); });

        // 임시로 현 스크립트(클라이언트)에서 player class 저장. 잘 가지고 있다가 Server가 Allocate 되면 GameMultiplyer의 서버RPC를 통해 서버에 넘겨준다. 
        // 이후에 UGS 클라우드 서버가 구축되면, 그곳에서 관리할것이다.
        //PlayerProfileData.Instance.SetCurrentSelectedClass(CharacterClasses.Class.Wizard);
        UpdatePlayerClass();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ChangePlayerClass()
    {
        switch (PlayerProfileData.Instance.GetCurrentSelectedClass())
        {
            case CharacterClass.Wizard:
                PlayerProfileData.Instance.SetCurrentSelectedClass(CharacterClass.Knight);
                break;
            case CharacterClass.Knight:
                PlayerProfileData.Instance.SetCurrentSelectedClass(CharacterClass.Wizard);
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

        switch (PlayerProfileData.Instance.GetCurrentSelectedClass())
        {
            case CharacterClass.Wizard:
                btnClassChange.GetComponentsInChildren<Image>()[1].sprite = iconWizard;
                character = Instantiate(PlayerProfileData.Instance.GetCurrentSelectedCharacterPrefab_NotInGame());
                break; 
            case CharacterClass.Knight:
                btnClassChange.GetComponentsInChildren<Image>()[1].sprite = iconKnight;
                character = Instantiate(PlayerProfileData.Instance.GetCurrentSelectedCharacterPrefab_NotInGame());
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
