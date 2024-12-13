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
            // �κ�� �̵�. 
            LoadSceneManager.Load(LoadSceneManager.Scene.LobbyScene);
        });
        // claim2x�� ���� ��� ���� �� ����߰�
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
/// �ִϸ��̼� ���κп��� AnimationEvent�� ȣ�����ִ� �޼ҵ� �Դϴ�.
/// </summary>
    public void ShowResultDetails()
    {
        StartCoroutine(ShowDetails());
    }

    private IEnumerator ShowDetails()
    {
        // 1. ���� Ʈ���� ���� �����ֱ�(ų�� + �������(10-���))
        yield return waitForSeconds;
        int currentPlayerScore = PlayerClient.Instance.GetPlayerScore();
        txtScoreCount.text = $"+ {currentPlayerScore + (10 - (MultiplayerGameManager.Instance.GetCurrentAlivePlayerCount()))}";
        // 2. ��ư�� ����
        yield return waitForSeconds;
        btnClaim.gameObject.SetActive(true);
        yield return waitForSeconds;
        //btnClaim2x.gameObject.SetActive(true);  
    }
}
