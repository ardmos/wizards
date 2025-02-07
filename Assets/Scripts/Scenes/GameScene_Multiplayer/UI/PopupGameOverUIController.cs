using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopupGameOverUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtScoreCount;
    [SerializeField] private CustomClickSoundButton btnClaim;
    [SerializeField] private CustomClickSoundButton btnClaim2x;

    private WaitForSeconds waitForSeconds = new WaitForSeconds(0.4f);

    private void Start()
    {
        btnClaim.gameObject.SetActive(false);
        btnClaim2x.gameObject.SetActive(false);

        btnClaim.AddClickListener(() =>
        {
            // 로비로 이동. 
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        });
        // claim2x는 광고 기능 구현 후 기능추가
        btnClaim2x.AddClickListener(() => { });


        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

/// <summary>
/// 애니메이션 끝부분에서 AnimationEvent로 호출해주는 메소드 입니다.
/// </summary>
    public void ShowResultDetails()
    {
        StartCoroutine(ShowDetails());
    }

    private IEnumerator ShowDetails()
    {
        // 1. 얻은 트로피 개수 보여주기(킬수 + 등수점수(10-등수))
        yield return waitForSeconds;
        int currentPlayerScore = PlayerClient.Instance.GetPlayerScore();
        txtScoreCount.text = $"+ {currentPlayerScore + (10 - (MultiplayerGameManager.Instance.GetCurrentAlivePlayerCount()))}";
        // 2. 버튼들 등장
        yield return waitForSeconds;
        btnClaim.gameObject.SetActive(true);
        yield return waitForSeconds;
        //btnClaim2x.gameObject.SetActive(true);  
    }
}
