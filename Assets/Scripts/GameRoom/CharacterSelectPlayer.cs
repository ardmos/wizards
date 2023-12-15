using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameRoom Scene에서 현 캐릭터오브젝트의 표시 여부를 조절하는 스크립트
/// </summary>
public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;    
    public GameRoomCharacterPrefab gameRoomCharacterPrefab;


    private void Start()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;

        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, System.EventArgs e)
    {
        //UpdatePlayer();
        UpdatePlayerReadyUI();
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    // 화면에 캐릭터 표시 여부 결정. 내부 기능이 좀 반복되는면이 있어보인다. Ready와 오브젝트 Show를 분리할 수 있어보임. 
    private void UpdatePlayer()
    {
        Debug.Log(nameof(UpdatePlayer) + $"IsPlayer{playerIndex} Connected?: {GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex)}");
        if (GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();
        }
        else
            Hide();

 
    }

    private void UpdatePlayerReadyUI()
    {
        // 화면에 레디상태 표시 여부 결정
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
        readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.clientId));
    }

    private void Show()
    {
        gameObject.SetActive(true);
        gameRoomCharacterPrefab.ShowCharacter3DVisual(playerIndex);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
        gameRoomCharacterPrefab.HideCharacter3DVisual();
    }
}
