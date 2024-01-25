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
    /// 팝업UI 정보 초기화
    /// 1. 팝업창 열릴시 유저 네임 띄우도록 변경
    /// 2. 숫자 변수들 업데이트해주도록 변경 
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
