using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비씬에서 동작하는 매칭룸 UI 컨트롤러입니다.
/// 플레이어 매칭, 준비 상태를 UI를 통해 표시합니다.
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
    /// 이벤트 리스너를 초기화합니다.
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
    /// 버튼 리스너를 초기화합니다.
    /// </summary>
    private void InitializeButtonListeners()
    {
        btnCancel.AddClickListener(CancelMatch);
        btnReady.AddClickListener(ReadyMatch);
    }

    /// <summary>
    /// 이벤트 리스너를 제거합니다.
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
        if (CurrentPlayerDataManager.Instance == null) return;

        byte playerCount = CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
        matchingRoomState = playerCount == ConnectionApprovalHandler.MaxPlayers ? MatchingRoomState.WaitingForReady : MatchingRoomState.WaitingForPlayers;

        RunStateMachine();
    }

    /// <summary>
    /// 준비 완료 플레이어 목록이 변경됐을 때 호출되는 이벤트 핸들러입니다.
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
    /// 플레이어 대기 상태를 처리합니다.
    /// </summary>
    private void HandleWaitingForPlayersState()
    {
        StopCountdownUIAnimation();
        btnReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// 준비 대기 상태를 처리합니다.
    /// </summary>
    private void HandleWaitingForReadyState()
    {
        if (SoundManager.Instance == null) return;

        StartCountdownUIAnimation();
        btnReady.gameObject.SetActive(true);
        SoundManager.Instance.PlayUISFX(UISFX_Type.Succeeded_Match);
    }

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
    /// 플레이어 수에 따라 UI를 업데이트합니다.
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
    /// 플레이어 수에 따라 토글 UI를 활성화합니다.
    /// </summary>
    private void ActivatePlayerIconToggleUI(byte playerCount)
    {
        for (int i = 0; i < togglePlayerIconUIArray.Length; i++)
        {
            togglePlayerIconUIArray[i].gameObject.SetActive(i < playerCount);
        }
    }

    /// <summary>
    /// 플레이어 토글 UI의 상태를 업데이트합니다.
    /// 플레이어의 레디 상태를 반영합니다.
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
    /// 플레이어가 준비 완료 상태로 설정합니다.
    /// </summary>
    private void ReadyMatch()
    {
        if (GameMatchReadyManagerServer.Instance == null) return;

        GameMatchReadyManagerServer.Instance.SetPlayerReadyServerRpc();
        btnReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// 현재 매칭을 취소합니다.
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
        gameObject.SetActive(false);
    }

    /// <summary>
    /// UI 상태를 초기화합니다.
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
