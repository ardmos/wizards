using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopupGameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtScoreCount;
    [SerializeField] private Button btnClaim;
    [SerializeField] private Button btnClaim2x;

    private void Start()
    {
        btnClaim.gameObject.SetActive(false);
        btnClaim2x.gameObject.SetActive(false);

        btnClaim.onClick.AddListener(() =>
        {
            CleanUp();
            // 로비로 이동. 
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });
        // claim2x는 광고 기능 구현 후 기능추가


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

    private void CleanUp()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
        if (GameMultiplayer.Instance != null)
        {
            Destroy(GameMultiplayer.Instance.gameObject);
        }
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
        yield return new WaitForSeconds(0.4f);
        txtScoreCount.text = $"+ {Player.LocalInstance.GetScore()+(10 - (GameManager.Instance.GetCurrentAlivePlayerCount()))}";
        // 2. 버튼들 등장
        yield return new WaitForSeconds(0.4f);
        btnClaim.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        btnClaim2x.gameObject.SetActive(true);  
    }
}
