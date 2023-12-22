using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupGameReadyUI : MonoBehaviour
{
    [SerializeField] private Button btnReady;
    [SerializeField] private TextMeshProUGUI txtGameReady;

    private void Awake()
    {    
        btnReady.onClick.AddListener(() =>
        {
            LocalPlayerReady();
        });
    }
    private void Start()
    {
        GameManager.Instance.OnStateChanged += OnGameStateChanged;
        txtGameReady.text = "Please Ready";
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
        // hide ready button UI
        btnReady.gameObject.SetActive(false);
        // report ready state to GameManager???? Complete this work when finish Sync Game State!
        GameManager.Instance.LocalPlayerReady();
        // show "Wating for players" text
        txtGameReady.text = "Wating for players...";
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
