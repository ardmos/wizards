using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameScene UI 
/// �����е�� ��ũ��Ʈ�� ���� �ֽ��ϴ�
/// </summary>
public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    [SerializeField] private Button btnMenu;

    [SerializeField] private PopupMenuUI popupMenuUI;
    public PopupGameOverUI popupGameOverUI;

    void Awake()
    {
        Instance = this;

        btnMenu.onClick.AddListener(() =>
        {
            popupMenuUI.Show();
        });

        GameManager.Instance.playerGameOverListServer.OnListChanged += OnPlayerGameOverListChanged;
    }

    private void OnPlayerGameOverListChanged(Unity.Netcode.NetworkListEvent<ulong> changeEvent)
    {

        PlayerData gameOverPlayerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(changeEvent.Value);
        // playerName ���� �Ⱦ��� �����ϱ�. (Lobby�� ��� ���� �������ϱ�) Player + ClientId�� ǥ��������
        //UpdateKillLog(gameOverPlayerData.playerName.ToString()); 
        UpdateKillLog($"Player {changeEvent.Value}");
    }

    // ų�α� ��� ���׷��̵�� ���ӿ�����Ų �÷��̾� & ���ӿ��� ���� �� �߰��ϸ� ����. 
    private void UpdateKillLog(string gameOverPlayerName)
    {
        // ų�α� ������Ʈ �ؿ� ų�α� ������ ���ø� �ν��Ͻÿ���Ʈ. (�ð� ������ �ڵ����� �ı���)
    }
}
