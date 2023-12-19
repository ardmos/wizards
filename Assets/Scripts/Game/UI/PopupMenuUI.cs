using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PopupMenuUI : MonoBehaviour
{
    // 메뉴팝업 닫기
    [SerializeField] private Button btnClose;
    // 로비로
    [SerializeField] private Button btnRestart;
    // 게임 포기. 관전하기 ( 관전시스템 추후 구축 )

    private void Awake()
    {
        btnClose.onClick.AddListener(Hide);
        btnRestart.onClick.AddListener(() =>
        {
            CleanUp();
            // 로비씬으로 이동
            LoadingSceneManager.Load(LoadingSceneManager.Scene.LobbyScene);
        });
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
}
