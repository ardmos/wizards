using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �κ������ �����ϴ� ��Ī�� UI ��Ʈ�ѷ��Դϴ�.
/// �÷��̾� ��Ī, �غ� ���¸� UI�� ���� ǥ���մϴ�.
/// </summary>
public class PopupMatchingRoomUIController : NetworkBehaviour
{
    public enum MatchingRoomState
    {
        WaitingForPlayers,
        WaitingForReady
    }

    public MatchingRoomState matchingRoomState = MatchingRoomState.WaitingForPlayers;

    public CustomClickSoundButton btnCancel;
    public CustomClickSoundButton btnReady;
    public Image imgReadyCountdown;
    public TextMeshProUGUI txtPlayerCount;
    public Toggle[] togglePlayerIconUIArray;
    public bool isReadyCountdownUIAnimStarted;

    private void Start()
    {
        if (IsServer) return;

        InitializeEventListeners();
        InitializeButtonListeners();

        Hide();
    }

    public override void OnDestroy()
    {
        if (IsServer) return;

        RemoveEventListeners();        
    }

    /// <summary>
    /// �̺�Ʈ �����ʸ� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeEventListeners()
    {
        if (ClientNetworkConnectionManager.Instance == null) return;
        if (CurrentPlayerDataManager.Instance == null) return;
        if (GameMatchReadyManagerClient.Instance == null) return;

        ClientNetworkConnectionManager.Instance.OnMatchJoined += OnMatchJoined;
        ClientNetworkConnectionManager.Instance.OnMatchExited += OnMatchExited;
        CurrentPlayerDataManager.Instance.OnCurrentPlayerListOnServerChanged += OnCurrentPlayerListChanged;
        GameMatchReadyManagerClient.Instance.OnPlayerReadyDictionaryClientChanged += OnReadyPlayerListClientChanged;
    }

    /// <summary>
    /// ��ư �����ʸ� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeButtonListeners()
    {
        btnCancel.AddClickListener(CancelMatch);
        btnReady.AddClickListener(ReadyMatch);
    }

    /// <summary>
    /// �̺�Ʈ �����ʸ� �����մϴ�.
    /// </summary>
    private void RemoveEventListeners()
    {
        if (ClientNetworkConnectionManager.Instance == null) return;
        if (CurrentPlayerDataManager.Instance == null) return;
        if (GameMatchReadyManagerClient.Instance == null) return;

        ClientNetworkConnectionManager.Instance.OnMatchJoined -= OnMatchJoined;
        ClientNetworkConnectionManager.Instance.OnMatchExited -= OnMatchExited;
        CurrentPlayerDataManager.Instance.OnCurrentPlayerListOnServerChanged -= OnCurrentPlayerListChanged;
        GameMatchReadyManagerClient.Instance.OnPlayerReadyDictionaryClientChanged -= OnReadyPlayerListClientChanged;
    }

    /// <summary>
    /// ��ġ�� �������� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// </summary>
    private void OnMatchJoined(object sender, EventArgs e)
    {
        Show();
    }

    /// <summary>
    /// ��ġ���� ������ �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// </summary>
    private void OnMatchExited(object sender, EventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// �÷��̾� ����� ������� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// ���� �÷��̾� ���� ���� ��Ī���¸� �����մϴ�.
    /// </summary>
    private void OnCurrentPlayerListChanged(object sender, EventArgs e)
    {
        if (!gameObject.activeSelf) return;
        if (CurrentPlayerDataManager.Instance == null) return;

        byte playerCount = CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
        matchingRoomState = playerCount == ConnectionApprovalHandler.MaxPlayers ? MatchingRoomState.WaitingForReady : MatchingRoomState.WaitingForPlayers;

        RunStateMachine();
    }

    /// <summary>
    /// �غ� �Ϸ� �÷��̾� ����� ������� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// </summary>
    private void OnReadyPlayerListClientChanged(object sender, EventArgs e)
    {
        UpdatePlayerIconToggleUI();
    }

    private void RunStateMachine()
    {
        if (!gameObject.activeSelf) return;

        switch (matchingRoomState)
        {
            case MatchingRoomState.WaitingForPlayers:
                HandleWaitingForPlayersState();
                break;
            case MatchingRoomState.WaitingForReady:
                HandleWaitingForReadyState();
                break;
        }

        UpdatePlayerIconToggleUI();
    }

    /// <summary>
    /// �÷��̾� ��� ���¸� ó���մϴ�.
    /// </summary>
    private void HandleWaitingForPlayersState()
    {
        StopCountdownUIAnimation();
        btnReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// �غ� ��� ���¸� ó���մϴ�.
    /// </summary>
    private void HandleWaitingForReadyState()
    {
        if (SoundManager.Instance == null) return;

        StartCountdownUIAnimation();
        btnReady.gameObject.SetActive(true);
        SoundManager.Instance.PlayUISFX(UISFX_Type.Succeeded_Match);
    }

    /// <summary>
    /// ī��Ʈ�ٿ�UI �ִϸ��̼��� �����մϴ�.
    /// </summary>
    private void StartCountdownUIAnimation()
    {
        if (isReadyCountdownUIAnimStarted) return;

        StartCoroutine(AnimateCountdownUI());
        isReadyCountdownUIAnimStarted = true;
    }

    /// <summary>
    /// ī��Ʈ�ٿ�UI �ִϸ��̼��� �����մϴ�.
    /// </summary>
    private void StopCountdownUIAnimation()
    {
        if (!isReadyCountdownUIAnimStarted) return;

        StopCoroutine(AnimateCountdownUI());
        isReadyCountdownUIAnimStarted = false;
        imgReadyCountdown.fillAmount = 0f;
    }

    /// <summary>
    /// ī��Ʈ�ٿ� UI�� �ִϸ��̼�ȭ�ϴ� �ڷ�ƾ�Դϴ�.
    /// �� �޼���� ������ �ð� ���� �̹����� fillAmount�� ���������� ������ŵ�ϴ�.
    /// �ð��� �� �Ǹ� ��ġ�� ����մϴ�.
    /// </summary>
    private IEnumerator AnimateCountdownUI()
    {
        float elapsedTime = 0f;
        float countdownMaxTime = GameMatchReadyManagerClient.readyCountdownMaxTime;
        while (elapsedTime <= countdownMaxTime)
        {
            imgReadyCountdown.fillAmount = elapsedTime / countdownMaxTime;
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        if (btnReady.gameObject.activeSelf)
        {
            CancelMatch();
        }
    }

    /// <summary>
    /// �÷��̾� ���� ���� UI�� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdatePlayerIconToggleUI()
    {
        if(CurrentPlayerDataManager.Instance == null) return;

        byte playerCount = CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
        txtPlayerCount.text = $"Waiting For Players... ({playerCount}/{ConnectionApprovalHandler.MaxPlayers})";
        ActivatePlayerIconToggleUI(playerCount);
        UpdatePlayerIconToggleUIState();
    }

    /// <summary>
    /// �÷��̾� ���� ���� ��� UI�� Ȱ��ȭ�մϴ�.
    /// </summary>
    private void ActivatePlayerIconToggleUI(byte playerCount)
    {
        for (int i = 0; i < togglePlayerIconUIArray.Length; i++)
        {
            togglePlayerIconUIArray[i].gameObject.SetActive(i < playerCount);
        }
    }

    /// <summary>
    /// �÷��̾� ��� UI�� ���¸� ������Ʈ�մϴ�.
    /// �÷��̾��� ���� ���¸� �ݿ��մϴ�.
    /// </summary>
    private void UpdatePlayerIconToggleUIState()
    {
        if (ServerNetworkConnectionManager.Instance == null) return;
        if (CurrentPlayerDataManager.Instance == null) return;
        if (GameMatchReadyManagerClient.Instance == null) return;

        for (int toggleArrayPlayerIndex = 0; toggleArrayPlayerIndex < togglePlayerIconUIArray.Length; toggleArrayPlayerIndex++)
        {
            if (ServerNetworkConnectionManager.Instance.IsPlayerIndexConnected(toggleArrayPlayerIndex))
            {
                PlayerInGameData playerData = CurrentPlayerDataManager.Instance.GetPlayerDataByPlayerIndex(toggleArrayPlayerIndex);
                togglePlayerIconUIArray[toggleArrayPlayerIndex].isOn = GameMatchReadyManagerClient.Instance.IsPlayerReady(playerData.clientId);
            }
            else
            {
                togglePlayerIconUIArray[toggleArrayPlayerIndex].isOn = false;
            }
        }
    }

    /// <summary>
    /// �÷��̾ �غ� �Ϸ� ���·� �����մϴ�.
    /// </summary>
    private void ReadyMatch()
    {
        if (GameMatchReadyManagerServer.Instance == null) return;

        GameMatchReadyManagerServer.Instance.SetPlayerReadyServerRpc();
        btnReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// ���� ��Ī�� ����մϴ�.
    /// </summary>
    private void CancelMatch()
    {
        if (GameMatchReadyManagerClient.Instance == null) return;
        if (ClientNetworkConnectionManager.Instance == null) return;
        if (SoundManager.Instance == null) return;

        GameMatchReadyManagerClient.Instance.ClearPlayerReadyList();
        ClientNetworkConnectionManager.Instance.StopClient();
        SoundManager.Instance.PlayUISFX(UISFX_Type.Failed_Match);
        Hide();
    }

    /// <summary>
    /// UI�� ǥ���մϴ�.
    /// </summary>
    private void Show()
    {
        gameObject.SetActive(true);
        ResetUIState();
    }

    /// <summary>
    /// UI�� ����ϴ�.
    /// </summary>
    private void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// UI ���¸� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void ResetUIState()
    {
        isReadyCountdownUIAnimStarted = false;
        matchingRoomState = MatchingRoomState.WaitingForPlayers;
        imgReadyCountdown.fillAmount = 0;
        ActivatePlayerIconToggleUI(0);
        btnReady.gameObject.SetActive(false);
    }
}
