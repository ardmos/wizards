using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupUserInfoUI : MonoBehaviour
{
    public Button btnClose;
    public Button btnUserName;
    public TextFieldUI txtUserName;
    public LobbyUI lobbyUI;

    public TextMeshProUGUI txtUserLevel;
    public TextMeshProUGUI txtHighestKOinOneMatch;
    public TextMeshProUGUI txtMostWins;
    public TextMeshProUGUI txtSoloVictories;
    public TextMeshProUGUI txtKnockOuts;
    public TextMeshProUGUI txtTotalScore;

    public byte userLevel;
    public byte hightestKOinOneMatch;
    public ushort mostWins;
    public ushort soloVictories;
    public uint knockOuts;
    public ulong totalScore;

    // Start is called before the first frame update
    void Start()
    {
        btnClose.onClick.AddListener(Hide);
        btnUserName.onClick.AddListener(OpenPopupNameChange);
    }

    private void OpenPopupNameChange()
    {
        //?
        FindObjectOfType<LobbyUI>().popupNameChangeUI.Show();
    }

    public void UpdateUserName(string userName)
    {
        // 1. Update Player Data
        PlayerDataManager.Instance.SetPlayerName(userName);
        // 2. Update this Popup UI's user name textBox
        txtUserName.UpdateTextValue(userName);
        // 3. Update this In Lobby Scene
        lobbyUI.UpdateUserInfoUI();
    }


    /// <summary>
    /// �˾�UI ���� �ʱ�ȭ
    /// 1. �˾�â ������ ���� ���� ��쵵�� ����
    /// 2. ���� ������ ������Ʈ���ֵ��� ���� 
    /// </summary>
    private void InitPopup()
    {
        txtUserName.UpdateTextValue(PlayerDataManager.Instance.GetPlayerName());

        PlayerOutGameData playerOutGameData = PlayerDataManager.Instance.GetPlayerOutGameData();
        if (playerOutGameData == null) return;

        txtHighestKOinOneMatch.text = playerOutGameData.hightestKOinOneMatch.ToString();
        txtMostWins.text = playerOutGameData.mostWins.ToString();
        txtSoloVictories.text = playerOutGameData.soloVictories.ToString();
        txtKnockOuts.text = playerOutGameData.knockOuts.ToString();
        txtTotalScore.text = playerOutGameData.totalScore.ToString();
        txtUserLevel.text = playerOutGameData.playerLevel.ToString();
    }

    public void Show()
    {
        gameObject.SetActive(true);
        InitPopup();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
