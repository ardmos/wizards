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
    public Button btnCancel;

    // Start is called before the first frame update
    void Start()
    {
        GameMultiplayer.Instance.OnSucceededToJoinMatch += OnSucceededToJoinMatch;
        GameMultiplayer.Instance.OnFailedToJoinMatch += OnFailedToJoinMatch;

        btnCancel.onClick.AddListener(CancelMatch);

        Hide();
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
        
        // 아래 총 플레이어숫자를 좀 더 똘똘하게 받아와보자. 예를들면 GameMultiplayer에게 함수로 요청한다던지./ <<< 여기부터!
        Debug.Log($"(클라이언트)현재 참여중인 총 플레이어 수 : {GameMultiplayer.Instance.GetPlayerDataNetworkList().Count}");
    }

    /// <summary>
    /// 현재 매칭중인 티켓에서 나갑니다. 
    /// </summary>
    private void CancelMatch()
    {
        GameMultiplayer.Instance.StopClient();
        Hide();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
