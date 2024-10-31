using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �κ������ �����ϴ� UI �Դϴ�.
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
public class PopupGameRoomUIController : NetworkBehaviour
{
    public enum MatchingState
    {
        WatingForPlayers,
        WatingForReady
    }

    public MatchingState matchingState = MatchingState.WatingForPlayers;

    public CustomClickSoundButton btnCancel;
    public CustomClickSoundButton btnReady;
    public Image imgReadyCountdown;
    public TextMeshProUGUI txtPlayerCount;
    public Toggle[] toggleArrayPlayerJoined;
    //public bool isCancellationRequested;
    public bool isReadyCountdownUIAnimStarted;

    // Start is called before the first frame update
    void Start()
    {
        // �������� �������� �ʿ� ���� �����Դϴ�.
        //Debug.Log($"Start() Is Server? : {IsServer}");
        if (IsServer) return;
        ClientNetworkConnectionManager.Instance.OnMatchJoined += OnMatchJoined;
        ClientNetworkConnectionManager.Instance.OnMatchExited += OnMatchExited;
        CurrentPlayerDataManager.Instance.OnCurrentPlayerListOnServerChanged += OnCurrentPlayerListChanged;
        GameMatchReadyManagerClient.Instance.OnPlayerReadyDictionaryClientChanged += OnReadyPlayerListClientChanged;

        btnCancel.AddClickListener(CancelMatch);
        btnReady.AddClickListener(ReadyMatch);

        Hide();
    }

    public override void OnDestroy()
    {
        // �������� �������� �ʿ� ���� �����Դϴ�.
        //Debug.Log($"OnDestroy() Is Server? : {IsServer}");
        if (IsServer) return;
        ClientNetworkConnectionManager.Instance.OnMatchJoined -= OnMatchJoined;
        ClientNetworkConnectionManager.Instance.OnMatchExited -= OnMatchExited;
        CurrentPlayerDataManager.Instance.OnCurrentPlayerListOnServerChanged -= OnCurrentPlayerListChanged;
        GameMatchReadyManagerClient.Instance.OnPlayerReadyDictionaryClientChanged -= OnReadyPlayerListClientChanged;
    }

    private void RunStateMachine()
    {
        // Unity Editor������ ���� Ȯ���� �ȵǱ� ������ �̷��� ó�����ݴϴ�.
        if (!gameObject.activeSelf) return;

        // 2. ���¿� ���� UI ����
        switch (matchingState)
        {
            // ���� �ο��� �� ���� �ܰ�
            case MatchingState.WatingForPlayers:
                // ī��Ʈ�ٿ� UI ��Ȱ��ȭ
                if (isReadyCountdownUIAnimStarted)
                {
                    StopAllCoroutines();
                    imgReadyCountdown.fillAmount = 0f;
                    isReadyCountdownUIAnimStarted = false;
                }
                // ���� ��ư ��Ȱ��ȭ
                btnReady.gameObject.SetActive(false);
                break;

            // �ο��� ��� ����! ���� ��ٸ��� �ܰ�
            case MatchingState.WatingForReady:
                // ī��Ʈ�ٿ� UI Ȱ��ȭ
                if (!isReadyCountdownUIAnimStarted)
                {
                    ActivateReadyCountdownUI();
                    isReadyCountdownUIAnimStarted = true;
                }
                // ���� ��ư Ȱ��ȭ
                Debug.Log($"�����ư Ȱ��ȭ"); 
                btnReady.gameObject.SetActive(true);
                // ��Ī ���� SFX ���
                SoundManager.Instance?.PlayUISFX(UISFX_Type.Succeeded_Match);
                break;
            default:
                break;
        }

        // 3. �������� �ο� ���� ǥ��
        SetUIs(CurrentPlayerDataManager.Instance.GetCurrentPlayerCount());
    }

    private void ActivateReadyCountdownUI()
    {        
        float countdownMaxTime = GameMatchReadyManagerClient.readyCountdownMaxTime;

        StartCoroutine(StartCountdownAnim(countdownMaxTime));
    }

    private IEnumerator StartCountdownAnim(float countdownMaxTime)
    {
        float time = 0f;
        while (time <= countdownMaxTime) 
        {
            imgReadyCountdown.fillAmount = time / countdownMaxTime;
            yield return new WaitForSeconds(Time.deltaTime);
            time += Time.deltaTime;
        }

        // �̶����� �����ư�� �ȴ����� SetActive�����̸� �� �÷��̾ ������ġ �մϴ�
        if(btnReady.gameObject.activeSelf) 
        {
            CancelMatch();
        }
    }

    private void OnReadyPlayerListClientChanged(object sender, System.EventArgs e)
    {
        SetUIs(CurrentPlayerDataManager.Instance.GetCurrentPlayerCount());
    }

    /// <summary>
    /// ���� �÷��̾ ��Ī Ƽ��(���� GameRoom) ������ �����ϸ� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// �� UI�� �����ݴϴ�.
    /// </summary>
    private void OnMatchJoined(object sender, System.EventArgs e)
    {
        Show();
    }

    /// <summary>
    /// ���� �÷��̾ ��Ī Ƽ��(���� GameRoom)���� ��Ż�� ȣ��Ǵ� �޼��� �Դϴ�. 
    /// �� UI�� �ݾ��ݴϴ�. ���� ���� ����� PopupConnectionResponseUI���� ���ݴϴ�.
    /// </summary>
    private void OnMatchExited(object sender, System.EventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// ���� �������� �÷��̾� ���ڿ� ������ ���� �� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// ȣ��� �� ���� RunStateMachine()�� ȣ���ؼ� �� State�� �´� UI ������Ʈ�� �������ݴ�1��.
    /// </summary>
    private void OnCurrentPlayerListChanged(object sender, System.EventArgs e)     // �̸� ���� ����ϱ� 
    {
        // Unity Editor������ ���� Ȯ���� �ȵǱ� ������ �̷��� ó�����ݴϴ�.
        if (!gameObject.activeSelf) return;

        //Debug.Log($"���� �������� �� �÷��̾� �� : {playerCount}, matchingState:{matchingState}");

        // ���� �ο� �� ���̸� ��Ī���� ����
        byte playerCount = CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
        if (playerCount == ConnectionApprovalHandler.MaxPlayers) //�׽�Ʈ�� �ּ�
        {
            // ���� ��� ���·� ����
            matchingState = MatchingState.WatingForReady;
        }
        // �����ο��� �� ���̱� ����
        else
        {
            // �ٽ� �÷��̾� ���� ���·� ����
            matchingState = MatchingState.WatingForPlayers;
        }

        RunStateMachine();
    }

    public void SetUIs(byte playerCount)
    {
        //Debug.Log($"RunStateMachine(). SetUIs(). ���� �������� �� �÷��̾� �� : {playerCount}");
        txtPlayerCount.text = $"Wating For Players... ({playerCount.ToString()}/{ConnectionApprovalHandler.MaxPlayers})";
        ActivateToggleUI(playerCount);
        UpdateToggleUIState();
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
    /// Ready UI(���) ���� ���� ������Ʈ 
    /// </summary>
    private void UpdateToggleUIState()
    {
        if(ServerNetworkConnectionManager.Instance == null) return;
        if(CurrentPlayerDataManager.Instance == null) return;   
        if(GameMatchReadyManagerClient.Instance == null) return;

        for (int toggleArrayPlayerIndex = 0; toggleArrayPlayerIndex < toggleArrayPlayerJoined.Length; toggleArrayPlayerIndex++)
        {
            if (ServerNetworkConnectionManager.Instance.IsPlayerIndexConnected(toggleArrayPlayerIndex))
            {
                PlayerInGameData playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByPlayerIndex(toggleArrayPlayerIndex);
                toggleArrayPlayerJoined[toggleArrayPlayerIndex].isOn = GameMatchReadyManagerClient.Instance.IsPlayerReady(playerData.clientId);
            }
            else
            {
                toggleArrayPlayerJoined[toggleArrayPlayerIndex].isOn = false;
            }
        }
    }

    /// <summary>
    /// �÷��̾� �����ư Ŭ���� �����ϴ� �޼ҵ� �Դϴ�.
    /// 1. ������ ������� ����
    /// 2. �����ư ��Ȱ��ȭ
    /// </summary>
    private void ReadyMatch()
    {
        GameMatchReadyManagerServer.Instance.SetPlayerReadyServerRpc();
        btnReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// ���� ��Ī���� Ƽ�Ͽ��� �����ϴ�. 
    /// </summary>
    private void CancelMatch()
    {
        // Client GameMatchReadyManager ����Ʈ �ʱ�ȭ. ������ �����ߴ� ���� ������ ���ܵ��� �ʱ� �����Դϴ�.
        GameMatchReadyManagerClient.Instance.ClearPlayerReadyList();

        // �� �÷��̾ ��Ī Ƽ�Ͽ��� �����Ϸ��� �ܰ�
        // 1. ���� ����
        ClientNetworkConnectionManager.Instance.StopClient();
        // ��Ī ���� SFX ���
        SoundManager.Instance?.PlayUISFX(UISFX_Type.Failed_Match);
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);

        //isCancellationRequested = false;
        isReadyCountdownUIAnimStarted = false;
        matchingState = MatchingState.WatingForPlayers;
        imgReadyCountdown.fillAmount = 0;
        ActivateToggleUI(0);
        btnReady.gameObject.SetActive(false);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
