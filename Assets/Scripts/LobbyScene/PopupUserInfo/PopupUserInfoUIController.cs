using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupUserInfoUIController : MonoBehaviour
{
    public CustomClickSoundButton btnClose;
    public CustomClickSoundButton btnUserName;
    public TextFieldUI txtUserName;
    public LobbySceneUIController lobbyUI;

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
        btnClose.AddClickListener(Hide);
        btnUserName.AddClickListener(OpenPopupNameChange);
    }

    private void OpenPopupNameChange()
    {
        FindObjectOfType<LobbySceneUIController>().GetPopupNameChangeUIController().Show();
    }

    public void UpdateUserName(string userName)
    {
        // 1. Update Player Data
        PlayerDataManager.Instance.UpdatePlayerName(userName);
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
