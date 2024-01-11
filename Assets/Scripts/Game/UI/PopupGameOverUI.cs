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
            // �κ�� �̵�. 
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });
        // claim2x�� ���� ��� ���� �� ����߰�


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
/// �ִϸ��̼� ���κп��� AnimationEvent�� ȣ�����ִ� �޼ҵ� �Դϴ�.
/// </summary>
    public void ShowResultDetails()
    {
        StartCoroutine(ShowDetails());
    }

    private IEnumerator ShowDetails()
    {
        // 1. ���� Ʈ���� ���� �����ֱ�(ų�� + �������(10-���))
        yield return new WaitForSeconds(0.4f);
        txtScoreCount.text = $"+ {Player.LocalInstance.GetScore()+(10 - (GameManager.Instance.GetCurrentAlivePlayerCount()))}";
        // 2. ��ư�� ����
        yield return new WaitForSeconds(0.4f);
        btnClaim.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        btnClaim2x.gameObject.SetActive(true);  
    }
}
