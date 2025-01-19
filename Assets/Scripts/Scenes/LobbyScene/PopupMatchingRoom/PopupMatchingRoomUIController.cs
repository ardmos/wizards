using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using static ComponentValidator;

/// <summary>
/// 매칭에 참여한 플레이어의 매칭, 준비 상태를 표시하는 UI를 관리하는 클래스입니다.
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
    private const string ERROR_CLIENT_NETWORK_CONNECTION_MANAGER_NOT_SET = "PopupMatchingRoomUIController ClientNetworkConnectionManager.Instance가 설정되지 않았습니다.";
    private const string ERROR_CURRENT_PLAYER_DATA_MANAGER_NOT_SET = "PopupMatchingRoomUIController CurrentPlayerDataManager.Instance가 설정되지 않았습니다.";
    private const string ERROR_GAME_MATCH_READY_MANAGER_NOT_SET = "PopupMatchingRoomUIController GameMatchReadyManager가 설정되지 않았습니다.";
    private const string ERROR_SOUND_MANAGER_NOT_SET = "PopupMatchingRoomUIController SoundManager.Instance가 설정되지 않았습니다.";
    private const string ERROR_SERVER_NETWORK_CONNECTION_MANAGER_NOT_SET = "PopupMatchingRoomUIController ServerNetworkConnectionManager.Instance가 설정되지 않았습니다.";
    private const string ERROR_BUTTON_CANCEL_NOT_SET = "PopupMatchingRoomUIController btnCancel이 설정되지 않았습니다.";
    private const string ERROR_BUTTON_READY_NOT_SET = "PopupMatchingRoomUIController btnReady가 설정되지 않았습니다.";
    private const string ERROR_IMG_READY_COUNTDOWN_NOT_SET = "PopupMatchingRoomUIController imgReadyCountdown가 설정되지 않았습니다.";
    private const string ERROR_TEXT_PLAYER_COUNT_NOT_SET = "PopupMatchingRoomUIController txtPlayerCount가 설정되지 않았습니다.";
    private const string ERROR_TOGGLE_PLAYER_ICON_ARRAY_NOT_SET = "PopupMatchingRoomUIController togglePlayerIconUIArray가 설정되지 않았습니다.";

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
    /// 이벤트 리스너를 초기화합니다.
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
    /// 버튼 리스너를 초기화합니다.
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
    /// 플레이어 대기 상태를 처리합니다.
    /// </summary>
    private void HandleWaitingForPlayersState()
    {
        if (!ValidateComponent(btnReady, ERROR_BUTTON_READY_NOT_SET)) return;

        StopCountdownUIAnimation();
        btnReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// 준비 대기 상태를 처리합니다.
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
    /// 플레이어가 준비 완료 상태로 설정합니다.
    /// </summary>
    private void ReadyMatch()
    {
        if (!ValidateComponent(GameMatchReadyManagerServer.Instance, ERROR_GAME_MATCH_READY_MANAGER_NOT_SET)) return;
        if (!ValidateComponent(btnReady, ERROR_BUTTON_READY_NOT_SET)) return;

        GameMatchReadyManagerServer.Instance.SetPlayerReadyServerRpc();
        btnReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// 현재 매칭을 취소합니다.
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
    /// 플레이어 수에 따라 UI를 업데이트합니다.
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
    /// 플레이어 수에 따라 토글 UI를 활성화합니다.
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
    /// 플레이어 토글 UI들의 상태를 업데이트합니다.
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
                Logger.Log($"해당 인덱스의 플레이어는 접속중이 아닙니다. 토글을 비활성화 합니다. index: {index}");
                togglePlayerIconUIArray[index].isOn = false;
                continue;
            }

            UpdatePlayerIconReadyStatus(togglePlayerIconUIArray[index], index);
        }
    }

    /// <summary>
    /// 플레이어 토글 UI에 플레이어의 준비 상태를 반영하는 내부 메서드입니다.
    /// </summary>
    /// <param name="toggleUI">플레이어 토글 UI</param>
    /// <param name="playerIndex">플레이어 인덱스</param>
    private void UpdatePlayerIconReadyStatus(Toggle toggleUI, int playerIndex)
    {
        if (!ValidateComponent(toggleUI, ERROR_TOGGLE_PLAYER_ICON_ARRAY_NOT_SET)) return;

        if (CurrentPlayerDataManager.Instance.TryGetPlayerDataByPlayerIndex(playerIndex, out var playerData))
        {
            toggleUI.isOn = GameMatchReadyManagerClient.Instance.IsPlayerReady(playerData.clientId);
        }
        else
        {
            Logger.LogError($"플레이어 데이터 획득을 실패했습니다. index: {playerIndex}");
        }
    }

    /// <summary>
    /// UI 상태를 초기화합니다.
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
    /// UI를 표시합니다.
    /// </summary>
    private void Show()
    {
        gameObject.SetActive(true);
        ResetUIState();
    }

    /// <summary>
    /// UI를 숨깁니다.
    /// </summary>
    private void Hide()
    {
        Debug.Log($"Hide");
        gameObject.SetActive(false);
    }
    #endregion

    #region UI Animation
    /// <summary>
    /// 카운트다운UI 애니메이션을 시작합니다.
    /// </summary>
    private void StartCountdownUIAnimation()
    {
        if (isReadyCountdownUIAnimStarted) return;

        StartCoroutine(AnimateCountdownUI());
        isReadyCountdownUIAnimStarted = true;
    }

    /// <summary>
    /// 카운트다운UI 애니메이션을 중지합니다.
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
    /// 카운트다운 UI를 애니메이션화하는 코루틴입니다.
    /// 이 메서드는 지정된 시간 동안 이미지의 fillAmount를 점진적으로 증가시킵니다.
    /// 시간이 다 되면 매치를 취소합니다.
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
    /// 이벤트 리스너를 제거합니다.
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
    /// 매치에 참여했을 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void OnMatchJoined(object sender, EventArgs e)
    {
        Show();
    }

    /// <summary>
    /// 매치에서 나갔을 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void OnMatchExited(object sender, EventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// 플레이어 목록이 변경됐을 때 호출되는 이벤트 핸들러입니다.
    /// 현재 플레이어 수에 따라 매칭상태를 변경합니다.
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
    /// 준비 완료 플레이어 목록이 변경됐을 때 호출되는 이벤트 핸들러입니다.
    /// </summary>
    private void OnReadyPlayerListClientChanged(object sender, EventArgs e)
    {
        UpdatePlayerIconToggleUI();
    }
    #endregion
}