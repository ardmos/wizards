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
    public bool isCancellationRequested;
    public bool isReadyCountdownUIAnimStarted;

    // Start is called before the first frame update
    void Start()
    {
        // �������� �������� �ʿ� ���� �����Դϴ�.
        //Debug.Log($"Start() Is Server? : {IsServer}");
        if (IsServer) return;
        GameMultiplayer.Instance.OnSucceededToJoinMatch += OnSucceededToJoinMatch;
        GameMultiplayer.Instance.OnFailedToJoinMatch += OnFailedToJoinMatch;
        GameMultiplayer.Instance.OnPlayerListOnServerChanged += OnPlayerListOnServerChanged;
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
        GameMultiplayer.Instance.OnSucceededToJoinMatch -= OnSucceededToJoinMatch;
        GameMultiplayer.Instance.OnFailedToJoinMatch -= OnFailedToJoinMatch;
        GameMultiplayer.Instance.OnPlayerListOnServerChanged -= OnPlayerListOnServerChanged;
        GameMatchReadyManagerClient.Instance.OnPlayerReadyDictionaryClientChanged -= OnReadyPlayerListClientChanged;
    }

    private void RunStateMachine()
    {
        // Unity Editor������ ���� Ȯ���� �ȵǱ� ������ �̷��� ó�����ݴϴ�.
        if (!gameObject.activeSelf) return;

        // 1. ������ �����ǻ� ���� �Ϸ� ������ ���� �ܰ� ����
        if (isCancellationRequested)
        {
            // �� �÷��̾ ��Ī Ƽ�Ͽ��� �����Ϸ��� �ܰ�
            // 1. ���� ����
            GameMultiplayer.Instance.StopClient();
            // ��Ī ���� SFX ���
            SoundManager.Instance?.PlayUISFX(UISFX_Type.Failed_Match);
            Hide();
        }

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
                btnReady.gameObject.SetActive(true);
                // ��Ī ���� SFX ���
                SoundManager.Instance?.PlayUISFX(UISFX_Type.Succeeded_Match);
                break;
            default:
                break;
        }

        // 3. �������� �ο� ���� ǥ��
        SetUIs(GameMultiplayer.Instance.GetPlayerCount());
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
        Debug.Log("�˾� OnReadyChanged()");
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
        //Debug.Log($"(OnSucceededToJoinMatch)���� �������� �� �÷��̾� �� : {GameMultiplayer.Instance.GetPlayerCount()}");
    }

    /// <summary>
    /// ���� �������� �÷��̾� ���ڿ� ������ ���� �� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// ȣ��� �� ���� RunStateMachine()�� ȣ���ؼ� �� State�� �´� UI ������Ʈ�� �������ݴ�1��.
    /// </summary>
    private void OnPlayerListOnServerChanged(object sender, System.EventArgs e)     // �̸� ���� ����ϱ� 
    {
        // Unity Editor������ ���� Ȯ���� �ȵǱ� ������ �̷��� ó�����ݴϴ�.
        if (!gameObject.activeSelf) return;

        // �������� �÷��̾� ��
        byte playerCount = GameMultiplayer.Instance.GetPlayerCount();
        //Debug.Log($"���� �������� �� �÷��̾� �� : {playerCount}, matchingState:{matchingState}");

        // ���� �ο� �� ���̸� �����ư Ȱ��ȭ
        if (playerCount == ConnectionApprovalHandler.MaxPlayers) //�׽�Ʈ�� �ּ�
        //if(true)
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
        //Debug.Log($"UpdateToggleUIState(), Ŭ���̾�Ʈ������ Ȯ�εǴ� currentConnectedPlayer:{GameMultiplayer.Instance.GetPlayerCount()} ");
        for (int playerIndex = 0; playerIndex < toggleArrayPlayerJoined.Length; playerIndex++)
        {
            if (GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
            {
                PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
                //Debug.Log($"Player Index{playerIndex} is ready? {GameMatchReadyManagerClient.Instance.IsPlayerReady(playerData.clientId)}");
                toggleArrayPlayerJoined[playerIndex].isOn = GameMatchReadyManagerClient.Instance.IsPlayerReady(playerData.clientId);
            }
            else
            {
                toggleArrayPlayerJoined[playerIndex].isOn = false;
            }
        }
    }

    /// <summary>
    /// ���� ��Ī���� Ƽ�Ͽ��� �����ϴ�. 
    /// SetPlayerUnReadyServerRPC()-> OnReadyChanged() -> RunStateMachine() ������ ����ż� ������ �Ϸ�˴ϴ�.
    /// </summary>
    private void CancelMatch()
    {
        isCancellationRequested = true;
        GameMatchReadyManagerServer.Instance.SetPlayerUnReadyServerRPC();
    }

    private void Show()
    {
        gameObject.SetActive(true);

        isCancellationRequested = false;
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
