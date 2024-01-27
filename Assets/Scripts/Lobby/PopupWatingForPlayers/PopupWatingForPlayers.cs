using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���ӷ���� UI�� ��ü�ϱ�� �߽��ϴ�.
/// ���ӷ���� ����ϴ� UI�Դϴ�. 
/// Ŭ���̾�Ʈ ���忡�� �����ϴ� UI �Դϴ�.
///
/// 1. ��Ī �ο� ����� ���׶��
/// 2. ��Ī �ο� �߰��� ���׶�� üũ ǥ�� 
/// 3. ��Ī �ο� ����� �ش� üũǥ�� ����
/// 4. ��Ī �ο� �� á�� �� ���Ӿ����� �̵�
/// 5. ��Ī ��� ��ư �߰�. 
/// 6. ��Ī ��� ��ư Ŭ���� �ش� ���� ��Ī ���.
/// </summary>
public class PopupWatingForPlayers : MonoBehaviour
{
    public enum MatchingState
    {
        WatingForPlayers,
        WatingForReady,
        CancelMatch,
    }

    public MatchingState matchingState = MatchingState.WatingForPlayers;

    public Button btnCancel;
    public Button btnReady;

    public TextMeshProUGUI txtPlayerCount;
    public Toggle[] toggleArrayPlayerJoined;
    public bool isCancellationRequested;


    private byte playerCount;


    // Start is called before the first frame update
    void Start()
    {
        GameMultiplayer.Instance.OnSucceededToJoinMatch += OnSucceededToJoinMatch;
        GameMultiplayer.Instance.OnFailedToJoinMatch += OnFailedToJoinMatch;
        GameMultiplayer.Instance.OnPlayerListOnServerChanged += OnPlayerListOnServerChanged;
        GameRoomReadyManager.Instance.OnClintPlayerReadyDictionaryChanged += OnReadyChanged;

        btnCancel.onClick.AddListener(CancelMatch);
        btnReady.onClick.AddListener(ReadyMatch);

        Hide();
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnSucceededToJoinMatch -= OnSucceededToJoinMatch;
        GameMultiplayer.Instance.OnFailedToJoinMatch -= OnFailedToJoinMatch;
        GameMultiplayer.Instance.OnPlayerListOnServerChanged -= OnPlayerListOnServerChanged;
        GameRoomReadyManager.Instance.OnClintPlayerReadyDictionaryChanged -= OnReadyChanged;
    }


    private void RunStateMachine()
    {
        // 1. ������ �����ǻ� ���� �Ϸ� ������ ���� �ܰ� ����
        if (isCancellationRequested) matchingState = MatchingState.CancelMatch;

        switch (matchingState)
        {
            case MatchingState.WatingForPlayers:
                // ���� �ο��� �� ���� �ܰ�
                // 1. ���� ��ư ��Ȱ��ȭ
                btnReady.gameObject.SetActive(false);

                // 2. �������� �ο� ǥ��
                Debug.Log($"(Ŭ���̾�Ʈ)���� �������� �� �÷��̾� �� : {playerCount}");
                txtPlayerCount.text = $"Wating For Players... ({playerCount.ToString()}/4)";

                // UI�� ���� �ݿ� 
                ActivateToggleUI(playerCount);
                break;

            case MatchingState.WatingForReady:
                // �ο��� ��� �𿩼� ���� ��ٸ��� �ܰ�
                break;

            case MatchingState.CancelMatch:
                // �� �÷��̾ ��Ī Ƽ�Ͽ��� �����Ϸ��� �ܰ�
                // 1. ���� ����
                GameMultiplayer.Instance.StopClient();
                Hide();
                break;
            default:
                break;
        }
    }

    private void OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdateToggleUIState();
        RunStateMachine();
    }

    /// <summary>
    /// ���� �÷��̾ ��Ī Ƽ�� ������ �����ϸ� ȣ��� �޼ҵ� �Դϴ�. 
    /// �� UI�� �ݾ��ݴϴ�. ���� ���� ����� PopupConnectionResponseUI���� ���ݴϴ�.
    /// </summary>
    private void OnFailedToJoinMatch(object sender, System.EventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// ���� �÷��̾ ��Ī Ƽ�� ������ �����ϸ� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// �� UI�� �����ݴϴ�.
    /// </summary>
    private void OnSucceededToJoinMatch(object sender, System.EventArgs e)
    {
        Show();       
    }

    /// <summary>
    /// ���� �������� �÷��̾� ���ڿ� ������ ���� �� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// �������� �÷��̾� ���ڿ� ���缭 UI�� ������Ʈ���ݴϴ�.
    /// �÷��̾� ���ڸ�ŭ ����ǥ�ø� �մϴ�.
    /// �÷��̾� �ο��� �� ���� ���� Ȯ���� �����մϴ�.
    /// </summary>
    private void OnPlayerListOnServerChanged(object sender, System.EventArgs e)
    {
        // �������� �÷��̾� ��
        playerCount = GameMultiplayer.Instance.GetPlayerCount();
        RunStateMachine();

        // ���� �ο� �� �𿴴��� ���ο� ���� ó��
        if (playerCount == ConnectionApprovalHandler.MaxPlayers)
        {
            btnReady.gameObject.SetActive(true);
            matchingState = MatchingState.WatingForReady;
        }
        else matchingState = MatchingState.WatingForPlayers;
    }

    /// <summary>
    /// �÷��̾� �����ư Ŭ���� �����ϴ� �޼ҵ� �Դϴ�.
    /// 1. ������ ������� ����
    /// 2. �����ư ��Ȱ��ȭ
    /// </summary>
    private void ReadyMatch()
    {
        GameRoomReadyManager.Instance.SetPlayerReadyServerRpc();
        btnReady.gameObject.SetActive(false);   
    }

    /// <summary>
    /// �Ű������� ���޵� ���ڸ�ŭ ��� ������Ʈ�� Ȱ��ȭ�����ݴϴ�. 
    /// </summary>
    private void ActivateToggleUI(byte playerCount)
    {
        for (int i = 0; i < toggleArrayPlayerJoined.Length; i++)
        {
            toggleArrayPlayerJoined[i].gameObject.SetActive(false);

            if (i < playerCount)
            {
                toggleArrayPlayerJoined[i].gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Ready UI(���) on/off ���� ������Ʈ 
    /// </summary>
    private void UpdateToggleUIState()
    {
        for (int playerIndex = 0; playerIndex < toggleArrayPlayerJoined.Length; playerIndex++)
        {
            if (GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
            {
                PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
                toggleArrayPlayerJoined[playerIndex].isOn = GameRoomReadyManager.Instance.IsPlayerReady(playerData.clientId);
            }
            else
            {
                toggleArrayPlayerJoined[playerIndex].isOn = false;
            }
        }
    }

    /// <summary>
    /// ���� ��Ī���� Ƽ�Ͽ��� �����ϴ�. 
    /// </summary>
    private void CancelMatch()
    {
        GameRoomReadyManager.Instance.SetPlayerUnReadyServerRPC();
        isCancellationRequested = true;
    }

    private void Show()
    {
        gameObject.SetActive(true);

        isCancellationRequested = false;
        matchingState = MatchingState.WatingForPlayers;

        RunStateMachine();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
