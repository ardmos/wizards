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

    [SerializeField] private PopupUserInfoUI popupUserInfoUI;
    [SerializeField] private PopupSettingsUI popupSettingsUI;
    [SerializeField] private PopupShopUI popupShop;
    [SerializeField] private PopupEquipmentUI popupEquipment;
    [SerializeField] private PopupClanUI popupClan;
    [SerializeField] private PopupModUI popupMod;

    [SerializeField] private PopupInboxUI popupInbox;

    // 임시로 현 스크립트에서 상태 저장
    private enum MyClass
    {
        Wizard,
        Knight
    }
    [SerializeField] private MyClass myClass;
    [SerializeField] private Sprite iconWizard, iconKnight;

    // Start is called before the first frame update
    void Start()
    {
        btnUserInfo.onClick.AddListener(() => { popupUserInfoUI.Show(); });
        btnSettings.onClick.AddListener(() => { popupSettingsUI.Show(); });
        btnClassChange.onClick.AddListener(ChangeClass);
        btnShop.onClick.AddListener(() => { popupShop.Show(); });
        btnEquipment.onClick.AddListener(() => { popupEquipment.Show(); });
        btnClan.onClick.AddListener(() => { popupClan.Show(); });
        btnMod.onClick.AddListener(() => { popupMod.Show(); });

        btnInbox.onClick.AddListener(() => { popupInbox.Show(); });

        // 임시로 현 스크립트에서 상태 저장
        myClass = MyClass.Wizard;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ChangeClass()
    {
        switch (myClass)
        {
            case MyClass.Wizard:
                myClass = MyClass.Knight;
                btnClassChange.GetComponentsInChildren<Image>()[1].sprite = iconKnight;
                break; 
            case MyClass.Knight:
                myClass = MyClass.Wizard;
                btnClassChange.GetComponentsInChildren<Image>()[1].sprite = iconWizard;
                break; 
            default:
                break;
        }
    }
}
