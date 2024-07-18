using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 1. 플레이어 스폰
/// 2. AI 플레이어 스폰
/// 3. GameManager의 gameState 관리
/// </summary>

public class SinglePlayerGameManager : NetworkBehaviour
{
    public GameManager gameManager;
    public PlayerSpawnPointsController spawnPointsController;

    public List<Transform> spawnPoints; // Inspector에서 6개의 스폰 포인트를 할당합니다.
    public List<Transform> selectedSpawnPoints; // 선택된 4개의 스폰 포인트를 저장할 리스트

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.StartHost();
        SelectRandomSpawnPoints();
        SpawnPlayer();
        SpawnAIPlayer();
        // 게임 바로 시작!
        StartSingleModeGame();
    }

    private void StartSingleModeGame()
    {
        gameManager.SetGameStateStart();
    }

    private void SpawnPlayer()
    {
        // Host 플레이어 캐릭터는 NetworkManager의 "Player Prefab" 설정과 함께 "Auto Create Player"로 자동 스폰. 
        // 이 부분. StartHost 했을 때, 플레이어 캐릭터 스폰되는 부분 구현중
        /*        GameObject playerObject = Instantiate(GameAssetsManager.Instance.GetCharacterPrefab_SinglePlayerInGame(PlayerDataManager.Instance.GetPlayerInGameData().characterClass));
                //playerObject.GetComponent<NetworkObject>().Spawn();
                playerObject.transform.SetParent(transform);
                // 0이 플레이어의 스폰 위치
                playerObject.transform.position = selectedSpawnPoints[0].position;*/
    }

    private void SpawnAIPlayer()
    {
        // 1~3이 AI의 스폰 위치
        for (int i = 1; i < 4; i++)
        {
            GameObject playerObject = Instantiate(GameAssetsManager.Instance.GetSingleModeAIPlayerCharacterPrefab());
            playerObject.GetComponent<NetworkObject>().Spawn();
            playerObject.transform.SetParent(transform);
            playerObject.transform.position = selectedSpawnPoints[i].position;
        }
    }

    private void SelectRandomSpawnPoints()
    {
        // 스폰 포인트 리스트를 복사합니다.
        List<Transform> tempList = new List<Transform>(spawnPointsController.spawnPoints);

        // Fisher-Yates 셔플 알고리즘을 사용하여 리스트를 섞습니다.
        for (int i = tempList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Transform temp = tempList[i];
            tempList[i] = tempList[randomIndex];
            tempList[randomIndex] = temp;
        }

        // 섞인 리스트에서 처음 4개의 요소를 선택합니다.
        selectedSpawnPoints = tempList.GetRange(0, 4);
    }
}
