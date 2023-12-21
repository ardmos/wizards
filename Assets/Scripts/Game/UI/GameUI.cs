using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameScene UI 
/// 게임패드는 스크립트가 따로 있습니다
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
        // playerName 아직 안쓰고 있으니까. (Lobby에 기능 구현 안했으니까) Player + ClientId로 표시해주자
        //UpdateKillLog(gameOverPlayerData.playerName.ToString()); 
        UpdateKillLog($"Player {changeEvent.Value}");
    }

    // 킬로그 기능 업그레이드로 게임오버시킨 플레이어 & 게임오버 사유 등 추가하면 좋음. 
    private void UpdateKillLog(string gameOverPlayerName)
    {
        // 킬로그 오브젝트 밑에 킬로그 아이템 템플릿 인스턴시에이트. (시간 지나면 자동으로 파괴됨)
    }
}
