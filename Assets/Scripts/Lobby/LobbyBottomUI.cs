using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 이제는 안쓰는 스크립트! 삭제 가능
/// </summary>
public class LobbyBottomUI : MonoBehaviour
{
    [SerializeField] private Button btnCreateGame;
    [SerializeField] private Button btnJoinGame;

    private void Awake()
    {
        btnCreateGame.onClick.AddListener(()=>
        {
            NetworkManager.Singleton.StartHost();
            //GameMultiplayer.Instance.StartHost();
            //LoadingSceneManager.Load(LoadingSceneManager.Scene.GameRoomScene);
            //LoadingSceneManager.LoadNetwork(LoadingSceneManager.Scene.GameRoomScene);
        });

        btnJoinGame.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            //GameMultiplayer.Instance.StartClient();
        });
    }
}
