using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameRoom Scene���� �� ĳ���Ϳ�����Ʈ�� ǥ�� ���θ� �����ϴ� ��ũ��Ʈ
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

    // ȭ�鿡 ĳ���� ǥ�� ���� ����. ���� ����� �� �ݺ��Ǵ¸��� �־�δ�. Ready�� ������Ʈ Show�� �и��� �� �־��. 
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
        // ȭ�鿡 ������� ǥ�� ���� ����
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
