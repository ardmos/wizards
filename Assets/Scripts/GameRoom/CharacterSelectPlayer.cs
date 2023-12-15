using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// GameRoom Scene에 캐릭터의 표시 여부를 조절하는 스크립트
/// </summary>
public class CharacterSelectPlayer : MonoBehaviour
{
    [SerializeField] private int playerIndex;
    [SerializeField] private GameObject readyGameObject;
    [SerializeField] private Button btnKick;
    public GameRoomCharacterPrefab gameRoomCharacterPrefab;

    private void Awake()
    {
        btnKick.onClick.AddListener(() => { 
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            GameMultiplayer.Instance.KickPlayer(playerData.clientId);
        });
    }

    private void Start()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged += GameMultiplayer_OnPlayerDataNetworkListChanged;
        CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;

        // 방장(Player Index 0) 캐릭터에게만 Kick 버튼을 보여줍니다. 방장 본인 캐릭터는 제외
        btnKick.gameObject.SetActive(playerIndex!=0);

        UpdatePlayer();
    }

    private void CharacterSelectReady_OnReadyChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    private void OnDestroy()
    {
        GameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= GameMultiplayer_OnPlayerDataNetworkListChanged;
    }

    private void GameMultiplayer_OnPlayerDataNetworkListChanged(object sender, System.EventArgs e)
    {
        UpdatePlayer();
    }

    // 화면에 캐릭터 표시 여부 결정
    private void UpdatePlayer()
    {
        Debug.Log(nameof(UpdatePlayer) + $"IsPlayer{playerIndex} Connected: {GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex)}");
        if (GameMultiplayer.Instance.IsPlayerIndexConnected(playerIndex))
        {
            Show();

            // 화면에 레디상태 표시 여부 결정
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(playerIndex);
            readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.clientId));
        }
        else
            Hide();
        {
        }
    }

    private void Show()
    {
        gameObject.SetActive(true);
        gameRoomCharacterPrefab.UpdateCharacter3DVisual();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
