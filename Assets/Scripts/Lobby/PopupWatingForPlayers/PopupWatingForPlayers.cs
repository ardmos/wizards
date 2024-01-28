using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static GameMatchReadyManager;

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
        WatingForReady
    }

    public MatchingState matchingState = MatchingState.WatingForPlayers;

    public Button btnCancel;
    public Button btnReady;
    public Image imgReadyCountdown;
    public TextMeshProUGUI txtPlayerCount;
    public Toggle[] toggleArrayPlayerJoined;
    public bool isCancellationRequested;
    public bool isReadyCountdownUIAnimStarted;



    // Start is called before the first frame update
    void Start()
    {
        GameMultiplayer.Instance.OnSucceededToJoinMatch += OnSucceededToJoinMatch;
        GameMultiplayer.Instance.OnFailedToJoinMatch += OnFailedToJoinMatch;
        GameMultiplayer.Instance.OnPlayerListOnServerChanged += OnPlayerListOnServerChanged;
        GameMatchReadyManager.Instance.OnClintPlayerReadyDictionaryChanged += OnReadyChanged;

        btnCancel.onClick.AddListener(CancelMatch);
        btnReady.onClick.AddListener(ReadyMatch);

        Hide();
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnSucceededToJoinMatch -= OnSucceededToJoinMatch;
        GameMultiplayer.Instance.OnFailedToJoinMatch -= OnFailedToJoinMatch;
        GameMultiplayer.Instance.OnPlayerListOnServerChanged -= OnPlayerListOnServerChanged;
        GameMatchReadyManager.Instance.OnClintPlayerReadyDictionaryChanged -= OnReadyChanged;
    }


    private void RunStateMachine()
    {
        // 1. ������ �����ǻ� ���� �Ϸ� ������ ���� �ܰ� ����
        if (isCancellationRequested)
        {
            // �� �÷��̾ ��Ī Ƽ�Ͽ��� �����Ϸ��� �ܰ�
            // 1. ���� ����
            GameMultiplayer.Instance.StopClient();
            Hide();
        }

        switch (matchingState)
        {
            case MatchingState.WatingForPlayers:
                // ���� �ο��� �� ���� �ܰ�

                // 1. Ȥ�� ī��Ʈ�ٿ��� �������̾��ٸ� ����.
                if (isReadyCountdownUIAnimStarted)
                {
                    StopAllCoroutines();
                }
                imgReadyCountdown.fillAmount = 0;
                isReadyCountdownUIAnimStarted = false;

                // 2. �������� �ο� ǥ��
                byte playerCount = GameMultiplayer.Instance.GetPlayerCount();
                Debug.Log($"(Ŭ���̾�Ʈ)���� �������� �� �÷��̾� �� : {playerCount}");
                txtPlayerCount.text = $"Wating For Players... ({playerCount.ToString()}/4)";

                // UI�� ���� �ݿ� 
                ActivateToggleUI(playerCount);

                break;

            case MatchingState.WatingForReady:
                // �ο��� ��� �𿩼� ���� ��ٸ��� �ܰ�

                // 1. ī��Ʈ�ٿ� UI Ȱ��ȭ
                if(!isReadyCountdownUIAnimStarted)
                    ActivateReadyCountdownUI();
                break;
            default:
                break;
        }
    }

    private void ActivateReadyCountdownUI()
    {
        isReadyCountdownUIAnimStarted = true;
        float countdownMaxTime = GameMatchReadyManager.readyCountdownMaxTime;

        StartCoroutine(StartCountdownAnim(countdownMaxTime));
    }

    private IEnumerator StartCountdownAnim(float countdownMaxTime)
    {
        float time = 0f;
        while (time <= countdownMaxTime) 
        {
            yield return new WaitForSeconds(Time.deltaTime);
            time += Time.deltaTime;

            imgReadyCountdown.fillAmount = time / countdownMaxTime;
        }

        // �̶����� �����ư�� �ȴ����� SetActive�����̸� �� �÷��̾ ������ġ �մϴ�
        if(btnReady.gameObject.activeSelf) 
        {
            CancelMatch();
        }
    }

    private void OnReadyChanged(object sender, System.EventArgs e)
    {
        // �� �˾� ������Ʈ�� Ȱ��ȭ���� �ʾҴܰ� ������ ��. �������� ��ġ �������� �ʿ� ���� �����Դϴ�.
        if (!gameObject.activeSelf) return;

        Debug.Log("�˾� OnReadyChanged()");
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
        //Debug.Log($"(OnSucceededToJoinMatch)���� �������� �� �÷��̾� �� : {GameMultiplayer.Instance.GetPlayerCount()}");
    }

    /// <summary>
    /// ���� �������� �÷��̾� ���ڿ� ������ ���� �� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// ȣ��� �� ���� RunStateMachine()�� ȣ���ؼ� �� State�� �´� UI ������Ʈ�� �������ݴϴ�.
    /// </summary>
    private void OnPlayerListOnServerChanged(object sender, System.EventArgs e)
    {
        // �� �˾� ������Ʈ�� Ȱ��ȭ���� �ʾҴܰ� ������ ��. �������� ��ġ �������� �ʿ� ���� �����Դϴ�.
        if (!gameObject.activeSelf) return;

        // �������� �÷��̾� ��
        byte playerCount = GameMultiplayer.Instance.GetPlayerCount();

        // ���� �ο� �� �𿴴��� ���ο� ���� ó��
        if (playerCount == ConnectionApprovalHandler.MaxPlayers)
        {
            RunStateMachine();
            // 1. ���� ��ư Ȱ��ȭ
            btnReady.gameObject.SetActive(true);
            matchingState = MatchingState.WatingForReady;
        }
        else
        {
            // 1. ���� ��ư Ȱ��ȭ
            btnReady.gameObject.SetActive(false);
            matchingState = MatchingState.WatingForPlayers;
        }

        Debug.Log($"���� �������� �� �÷��̾� �� : {playerCount}, matchingState:{matchingState}");
        RunStateMachine();

        /*        Debug.Log($"Client�� PlayerList �ݿ� �Ϸ�. ���� �������� �� �÷��̾� �� : {GameMultiplayer.Instance.GetPlayerCount()}");
                matchingState = GameMatchReadyManager.Instance.GetMatchingStateOnClientSide();

                RunStateMachine();*/
    }


    /// <summary>
    /// �÷��̾� �����ư Ŭ���� �����ϴ� �޼ҵ� �Դϴ�.
    /// 1. ������ ������� ����
    /// 2. �����ư ��Ȱ��ȭ
    /// </summary>
    private void ReadyMatch()
    {
        GameMatchReadyManager.Instance.SetPlayerReadyServerRpc();
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
                toggleArrayPlayerJoined[playerIndex].isOn = GameMatchReadyManager.Instance.IsPlayerReady(playerData.clientId);
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
        GameMatchReadyManager.Instance.SetPlayerUnReadyServerRPC();
    }

    private void Show()
    {
        gameObject.SetActive(true);

        isCancellationRequested = false;
        isReadyCountdownUIAnimStarted = false;
        matchingState = MatchingState.WatingForPlayers;
        imgReadyCountdown.fillAmount = 0;

        Debug.Log("�˾� Show()");
        //RunStateMachine();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
