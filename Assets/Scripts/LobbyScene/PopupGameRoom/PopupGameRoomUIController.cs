using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비씬에서 동작하는 UI 입니다.
/// 게임룸씬을 UI로 대체하기로 했습니다.
/// 게임룸씬을 대신하는 UI입니다. 
/// 클라이언트 입장에서 동작하는 UI 입니다.
///
/// 1. 매칭 인원 빈공간 동그라미
/// 2. 매칭 인원 추가시 동그라미 체크 표시 
/// 3. 매칭 인원 퇴장시 해당 체크표시 제거
/// 4. 매칭 인원 꽉 찼을 시 게임씬으로 이동
/// 5. 매칭 취소 버튼 추가. 
/// 6. 매칭 취소 버튼 클릭시 해당 유저 매칭 취소.
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
        // 서버에선 실행해줄 필요 없는 내용입니다.
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
        // 서버에선 실행해줄 필요 없는 내용입니다.
        //Debug.Log($"OnDestroy() Is Server? : {IsServer}");
        if (IsServer) return;
        ClientNetworkConnectionManager.Instance.OnMatchJoined -= OnMatchJoined;
        ClientNetworkConnectionManager.Instance.OnMatchExited -= OnMatchExited;
        CurrentPlayerDataManager.Instance.OnCurrentPlayerListOnServerChanged -= OnCurrentPlayerListChanged;
        GameMatchReadyManagerClient.Instance.OnPlayerReadyDictionaryClientChanged -= OnReadyPlayerListClientChanged;
    }

    private void RunStateMachine()
    {
        // Unity Editor에서는 서버 확인이 안되기 때문에 이렇게 처리해줍니다.
        if (!gameObject.activeSelf) return;

        // 2. 상태에 따른 UI 설정
        switch (matchingState)
        {
            // 아직 인원이 덜 모인 단계
            case MatchingState.WatingForPlayers:
                // 카운트다운 UI 비활성화
                if (isReadyCountdownUIAnimStarted)
                {
                    StopAllCoroutines();
                    imgReadyCountdown.fillAmount = 0f;
                    isReadyCountdownUIAnimStarted = false;
                }
                // 레디 버튼 비활성화
                btnReady.gameObject.SetActive(false);
                break;

            // 인원이 모두 모임! 레디를 기다리는 단계
            case MatchingState.WatingForReady:
                // 카운트다운 UI 활성화
                if (!isReadyCountdownUIAnimStarted)
                {
                    ActivateReadyCountdownUI();
                    isReadyCountdownUIAnimStarted = true;
                }
                // 레디 버튼 활성화
                Debug.Log($"레디버튼 활성화"); 
                btnReady.gameObject.SetActive(true);
                // 매칭 성공 SFX 재생
                SoundManager.Instance?.PlayUISFX(UISFX_Type.Succeeded_Match);
                break;
            default:
                break;
        }

        // 3. 접속중인 인원 숫자 표시
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

        // 이때까지 레디버튼이 안눌려서 SetActive상태이면 현 플레이어를 퇴장조치 합니다
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
    /// 현재 플레이어가 매칭 티켓(현재 GameRoom) 참여에 성공하면 호출되는 메소드 입니다.
    /// 현 UI를 보여줍니다.
    /// </summary>
    private void OnMatchJoined(object sender, System.EventArgs e)
    {
        Show();
    }

    /// <summary>
    /// 현재 플레이어가 매칭 티켓(현재 GameRoom)에서 이탈시 호출되는 메서드 입니다. 
    /// 현 UI를 닫아줍니다. 실패 문구 출력은 PopupConnectionResponseUI에서 해줍니다.
    /// </summary>
    private void OnMatchExited(object sender, System.EventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// 현재 참여중인 플레이어 숫자에 변동이 있을 때 호출되는 메소드 입니다.
    /// 호출될 때 마다 RunStateMachine()을 호출해서 현 State에 맞는 UI 업데이트를 진행해줍니1다.
    /// </summary>
    private void OnCurrentPlayerListChanged(object sender, System.EventArgs e)     // 이름 수정 고려하기 
    {
        // Unity Editor에서는 서버 확인이 안되기 때문에 이렇게 처리해줍니다.
        if (!gameObject.activeSelf) return;

        //Debug.Log($"현재 참여중인 총 플레이어 수 : {playerCount}, matchingState:{matchingState}");

        // 게임 인원 다 모이면 매칭상태 변경
        byte playerCount = CurrentPlayerDataManager.Instance.GetCurrentPlayerCount();
        if (playerCount == ConnectionApprovalHandler.MaxPlayers) //테스트용 주석
        {
            // 레디 대기 상태로 변경
            matchingState = MatchingState.WatingForReady;
        }
        // 게임인원이 다 모이기 이전
        else
        {
            // 다시 플레이어 모집 상태로 변경
            matchingState = MatchingState.WatingForPlayers;
        }

        RunStateMachine();
    }

    public void SetUIs(byte playerCount)
    {
        //Debug.Log($"RunStateMachine(). SetUIs(). 현재 참여중인 총 플레이어 수 : {playerCount}");
        txtPlayerCount.text = $"Wating For Players... ({playerCount.ToString()}/{ConnectionApprovalHandler.MaxPlayers})";
        ActivateToggleUI(playerCount);
        UpdateToggleUIState();
    }

    /// <summary>
    /// 매개변수로 전달된 숫자만큼 토글 오브젝트를 활성화시켜줍니다. 
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
    /// Ready UI(토글) 점등 상태 업데이트 
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
    /// 플레이어 레디버튼 클릭시 동작하는 메소드 입니다.
    /// 1. 서버에 레디상태 보고
    /// 2. 레디버튼 비활성화
    /// </summary>
    private void ReadyMatch()
    {
        GameMatchReadyManagerServer.Instance.SetPlayerReadyServerRpc();
        btnReady.gameObject.SetActive(false);
    }

    /// <summary>
    /// 현재 매칭중인 티켓에서 나갑니다. 
    /// </summary>
    private void CancelMatch()
    {
        // Client GameMatchReadyManager 리스트 초기화. 이전에 참가했던 방의 정보를 남겨두지 않기 위함입니다.
        GameMatchReadyManagerClient.Instance.ClearPlayerReadyList();

        // 현 플레이어가 매칭 티켓에서 퇴장하려는 단계
        // 1. 퇴장 실행
        ClientNetworkConnectionManager.Instance.StopClient();
        // 매칭 실패 SFX 재생
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
