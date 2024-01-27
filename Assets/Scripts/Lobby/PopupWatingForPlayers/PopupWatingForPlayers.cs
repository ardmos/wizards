using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
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
        // 1. 서버에 퇴장의사 보고 완료 됐으면 퇴장 단계 진행
        if (isCancellationRequested) matchingState = MatchingState.CancelMatch;

        switch (matchingState)
        {
            case MatchingState.WatingForPlayers:
                // 아직 인원이 덜 모인 단계
                // 1. 레디 버튼 비활성화
                btnReady.gameObject.SetActive(false);

                // 2. 접속중인 인원 표시
                Debug.Log($"(클라이언트)현재 참여중인 총 플레이어 수 : {playerCount}");
                txtPlayerCount.text = $"Wating For Players... ({playerCount.ToString()}/4)";

                // UI에 숫자 반영 
                ActivateToggleUI(playerCount);
                break;

            case MatchingState.WatingForReady:
                // 인원이 모두 모여서 레디를 기다리는 단계
                break;

            case MatchingState.CancelMatch:
                // 현 플레이어가 매칭 티켓에서 퇴장하려는 단계
                // 1. 퇴장 실행
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
    /// 현재 플레이어가 매칭 티켓 참여에 실패하면 호출되 메소드 입니다. 
    /// 현 UI를 닫아줍니다. 실패 문구 출력은 PopupConnectionResponseUI에서 해줍니다.
    /// </summary>
    private void OnFailedToJoinMatch(object sender, System.EventArgs e)
    {
        Hide();
    }

    /// <summary>
    /// 현재 플레이어가 매칭 티켓 참여에 성공하면 호출되는 메소드 입니다.
    /// 현 UI를 보여줍니다.
    /// </summary>
    private void OnSucceededToJoinMatch(object sender, System.EventArgs e)
    {
        Show();       
    }

    /// <summary>
    /// 현재 참여중인 플레이어 숫자에 변동이 있을 때 호출되는 메소드 입니다.
    /// 접속중인 플레이어 숫자에 맞춰서 UI를 업데이트해줍니다.
    /// 플레이어 숫자만큼 접속표시를 합니다.
    /// 플레이어 인원이 꽉 차면 레디 확인을 시작합니다.
    /// </summary>
    private void OnPlayerListOnServerChanged(object sender, System.EventArgs e)
    {
        // 접속중인 플레이어 수
        playerCount = GameMultiplayer.Instance.GetPlayerCount();
        RunStateMachine();

        // 게임 인원 다 모였는지 여부에 따라 처리
        if (playerCount == ConnectionApprovalHandler.MaxPlayers)
        {
            btnReady.gameObject.SetActive(true);
            matchingState = MatchingState.WatingForReady;
        }
        else matchingState = MatchingState.WatingForPlayers;
    }

    /// <summary>
    /// 플레이어 레디버튼 클릭시 동작하는 메소드 입니다.
    /// 1. 서버에 레디상태 보고
    /// 2. 레디버튼 비활성화
    /// </summary>
    private void ReadyMatch()
    {
        GameRoomReadyManager.Instance.SetPlayerReadyServerRpc();
        btnReady.gameObject.SetActive(false);   
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
    /// Ready UI(토글) on/off 상태 업데이트 
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
    /// 현재 매칭중인 티켓에서 나갑니다. 
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
