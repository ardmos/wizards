using TMPro;
using UnityEngine;

/// <summary>
/// �ٸ� �÷��̾ ��ٸ� �� ǥ�õǴ� �˾��Դϴ�.
/// </summary>
public class PopupWatingForPlayersUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtMessage;

    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        txtMessage.text = "Wating for players...";
        // �ڵ� ���� ����
        LocalPlayerReady();
    }

    private void OnDestroy()
    {
        GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(object sender, System.EventArgs e)
    {
        if (GameManager.Instance.IsWatingToStart())
        {
            Show();
        }
        //Debug.Log($"PopupGameReadyUI OnGameStateChanged state.IsWaitingToStart: {GameManager.Instance.IsWatingToStart()}, state.isCountdown: {GameManager.Instance.IsCountdownToStartActive()}");
        if (GameManager.Instance.IsCountdownToStartActive())
        {
            
            Hide();
        }
    }

    private void LocalPlayerReady()
    {
        // report ready state to GameManager???? Complete this work when finish Sync Game State!
        GameManager.Instance.LocalPlayerReadyOnClient();
        // # If every player get ready, the Game State will change.  
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
