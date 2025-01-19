using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static ComponentValidator;

/// <summary>
/// ��Ī�� ������ �÷��̾��� ��Ī, �غ� ���¸� ǥ���ϴ� UI�� �����ϴ� Ŭ�����Դϴ�.
/// </summary>
public class PopupMatchingRoomUIController : NetworkBehaviour
{
    #region Enums
    private enum MatchingRoomUIState
    {
        WaitingForPlayers,
        WaitingForReady
    }
    #endregion

    #region Constants & Fields
    private const string ERROR_CLIENT_NETWORK_CONNECTION_MANAGER_NOT_SET = "PopupMatchingRoomUIController ClientNetworkConnectionManager.Instance�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET = "PopupMatchingRoomUIController CurrentPlayerDataManager.Instance�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_GAME_MATCH_READY_MANAGER_NOT_SET = "PopupMatchingRoomUIController GameMatchReadyManager�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_SOUND_MANAGER_NOT_SET = "PopupMatchingRoomUIController SoundManager.Instance�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_SERVER_NETWORK_CONNECTION_MANAGER_NOT_SET = "PopupMatchingRoomUIController ServerNetworkConnectionManager.Instance�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_BUTTON_CANCEL_NOT_SET = "PopupMatchingRoomUIController btnCancel�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_BUTTON_READY_NOT_SET = "PopupMatchingRoomUIController btnReady�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_IMG_READY_COUNTDOWN_NOT_SET = "PopupMatchingRoomUIController imgReadyCountdown�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_TEXT_PLAYER_COUNT_NOT_SET = "PopupMatchingRoomUIController txtPlayerCount�� �������� �ʾҽ��ϴ�.";
    private const string ERROR_TOGGLE_PLAYER_ICON_ARRAY_NOT_SET = "PopupMatchingRoomUIController togglePlayerIconUIArray�� �������� �ʾҽ��ϴ�.";

    [SerializeField] private CustomClickSoundButton btnCancel;
    [SerializeField] private CustomClickSoundButton btnReady;
    [SerializeField] private Image imgReadyCountdown;
    [SerializeField] private TextMeshProUGUI txtPlayerCount;
    [SerializeField] private Toggle[] togglePlayerIconUIArray;
    [SerializeField] private bool isReadyCountdownUIAnimStarted;

    private MatchingRoomUIState matchingRoomUIState = MatchingRoomUIState.WaitingForPlayers;
    #endregion


    #region Unity Lifecycle
    public void Start()
    {
        InitializeEventListeners();
        InitializeButtonListeners();
        Hide();
    }

    public override void OnDestroy()
    {
        RemoveEventListeners();
    }
    #endregion

    #region Initialization
    /// <summary>
    /// �̺�Ʈ �����ʸ� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void InitializeEventListeners()
    {
        if (!ValidateComponent(ClientNetworkConnectionManager.Instance, ERROR_CLIENT_NETWORK_CONNECTION_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(GameMatchReadyManagerClient.Instance, ERROR_GAME_MATCH_READY_MANAGER_NOT_SET)) return;

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
        if (!ValidateComponent(btnCancel, ERROR_BUTTON_CANCEL_NOT_SET)) return;
        if (!ValidateComponent(btnReady, ERROR_BUTTON_READY_NOT_SET)) return;

        btnCancel.AddClickListener(CancelMatch);
        btnReady.AddClickListener(ReadyMatch);
    }
    #endregion

    #region State Machine
    private void RunUIStateMachine()
    {
        if (!gameObject.activeSelf) return;

        switch (matchingRoomUIState)
        {
            case MatchingRoomUIState.WaitingForPlayers:
                HandleWaitingForPlayersState();
                break;
            case MatchingRoomUIState.WaitingForReady:
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
        if (!ValidateComponent(btnReady, ERROR_BUTTON_READY_NOT_SET)) return;

        StopCountdownUIAnimation();
        btnReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// �غ� ��� ���¸� ó���մϴ�.
    /// </summary>
    private void HandleWaitingForReadyState()
    {
        if (!ValidateComponent(btnReady, ERROR_BUTTON_READY_NOT_SET)) return;
        if (!ValidateComponent(SoundManager.Instance, ERROR_SOUND_MANAGER_NOT_SET)) return;

        StartCountdownUIAnimation();
        btnReady.gameObject.SetActive(true);
        SoundManager.Instance.PlayUISFX(UISFX_Type.Succeeded_Match);
    }
    #endregion

    #region Match Control
    /// <summary>
    /// �÷��̾ �غ� �Ϸ� ���·� �����մϴ�.
    /// </summary>
    private void ReadyMatch()
    {
        if (!ValidateComponent(GameMatchReadyManagerServer.Instance, ERROR_GAME_MATCH_READY_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(btnReady, ERROR_BUTTON_READY_NOT_SET)) return;

        GameMatchReadyManagerServer.Instance.SetPlayerReadyServerRpc();
        btnReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// ���� ��Ī�� ����մϴ�.
    /// </summary>
    private void CancelMatch()
    {
        if (!ValidateComponent(GameMatchReadyManagerClient.Instance, ERROR_GAME_MATCH_READY_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(ClientNetworkConnectionManager.Instance, ERROR_CLIENT_NETWORK_CONNECTION_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(SoundManager.Instance, ERROR_SOUND_MANAGER_NOT_SET)) return;

        GameMatchReadyManagerClient.Instance.ClearPlayerReadyList();
        ClientNetworkConnectionManager.Instance.StopClient();
        SoundManager.Instance.PlayUISFX(UISFX_Type.Failed_Match);
        Hide();
    }
    #endregion

    #region UI Update
    /// <summary>
    /// �÷��̾� ���� ���� UI�� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdatePlayerIconToggleUI()
    {
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(txtPlayerCount, ERROR_TEXT_PLAYER_COUNT_NOT_SET)) return;

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
        for (int index = 0; index < togglePlayerIconUIArray.Length; index++)
        {
            if (!ValidateComponent(togglePlayerIconUIArray[index], ERROR_TOGGLE_PLAYER_ICON_ARRAY_NOT_SET)) continue;

            togglePlayerIconUIArray[index].gameObject.SetActive(index < playerCount);
        }
    }

    /// <summary>
    /// �÷��̾� ��� UI���� ���¸� ������Ʈ�մϴ�.
    /// </summary>
    private void UpdatePlayerIconToggleUIState()
    {
        if (!ValidateComponent(ServerNetworkConnectionManager.Instance, ERROR_SERVER_NETWORK_CONNECTION_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(GameMatchReadyManagerClient.Instance, ERROR_GAME_MATCH_READY_MANAGER_NOT_SET)) return;

        for (int index = 0; index < togglePlayerIconUIArray.Length; index++)
        {
            if (!ServerNetworkConnectionManager.Instance.IsPlayerIndexConnected(index))
            {
                Logger.Log($"�ش� �ε����� �÷��̾�� �������� �ƴմϴ�. ����� ��Ȱ��ȭ �մϴ�. index: {index}");
                togglePlayerIconUIArray[index].isOn = false;
                continue;
            }

            UpdatePlayerIconReadyStatus(togglePlayerIconUIArray[index], index);
        }
    }

    /// <summary>
    /// �÷��̾� ��� UI�� �÷��̾��� �غ� ���¸� �ݿ��ϴ� ���� �޼����Դϴ�.
    /// </summary>
    /// <param name="toggleUI">�÷��̾� ��� UI</param>
    /// <param name="playerIndex">�÷��̾� �ε���</param>
    private void UpdatePlayerIconReadyStatus(Toggle toggleUI, int playerIndex)
    {
        if (!ValidateComponent(toggleUI, ERROR_TOGGLE_PLAYER_ICON_ARRAY_NOT_SET)) return;

        if (CurrentPlayerDataManager.Instance.TryGetPlayerDataByPlayerIndex(playerIndex, out var playerData))
        {
            toggleUI.isOn = GameMatchReadyManagerClient.Instance.IsPlayerReady(playerData.clientId);
        }
        else
        {
            Logger.LogError($"�÷��̾� ������ ȹ���� �����߽��ϴ�. index: {playerIndex}");
        }
    }

    /// <summary>
    /// UI ���¸� �ʱ�ȭ�մϴ�.
    /// </summary>
    private void ResetUIState()
    {
        if (!ValidateComponent(imgReadyCountdown, ERROR_IMG_READY_COUNTDOWN_NOT_SET)) return;
        if (!ValidateComponent(btnReady, ERROR_BUTTON_READY_NOT_SET)) return;

        isReadyCountdownUIAnimStarted = false;
        matchingRoomUIState = MatchingRoomUIState.WaitingForPlayers;
        imgReadyCountdown.fillAmount = 0;
        ActivatePlayerIconToggleUI(0);
        btnReady.gameObject.SetActive(false);
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
        Debug.Log($"Hide");
        gameObject.SetActive(false);
    }
    #endregion

    #region UI Animation
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
        if (!ValidateComponent(imgReadyCountdown, ERROR_IMG_READY_COUNTDOWN_NOT_SET)) return;

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
        if (!ValidateComponent(GameMatchReadyManagerClient.Instance, ERROR_GAME_MATCH_READY_MANAGER_NOT_SET)) yield break;
        if (!ValidateComponent(imgReadyCountdown, ERROR_IMG_READY_COUNTDOWN_NOT_SET)) yield break;
        if (!ValidateComponent(btnReady, ERROR_BUTTON_READY_NOT_SET)) yield break;

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
    #endregion

    #region Event Handlers
    /// <summary>
    /// �̺�Ʈ �����ʸ� �����մϴ�.
    /// </summary>
    private void RemoveEventListeners()
    {
        if (!ValidateComponent(ClientNetworkConnectionManager.Instance, ERROR_CLIENT_NETWORK_CONNECTION_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(GameMatchReadyManagerClient.Instance, ERROR_GAME_MATCH_READY_MANAGER_NOT_SET)) return;

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
        if (!ValidateComponent(CurrentPlayerDataManager.Instance, ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET)) return;

        byte currentPlayerCount = CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
        matchingRoomUIState = currentPlayerCount == ConnectionApprovalHandler.MaxPlayers ? MatchingRoomUIState.WaitingForReady : MatchingRoomUIState.WaitingForPlayers;

        RunUIStateMachine();
    }

    /// <summary>
    /// �غ� �Ϸ� �÷��̾� ����� ������� �� ȣ��Ǵ� �̺�Ʈ �ڵ鷯�Դϴ�.
    /// </summary>
    private void OnReadyPlayerListClientChanged(object sender, EventArgs e)
    {
        UpdatePlayerIconToggleUI();
    }
    #endregion
}