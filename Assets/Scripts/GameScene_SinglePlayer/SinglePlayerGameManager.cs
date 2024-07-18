using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 1. �÷��̾� ����
/// 2. AI �÷��̾� ����
/// 3. GameManager�� gameState ����
/// </summary>

public class SinglePlayerGameManager : NetworkBehaviour
{
    public GameManager gameManager;
    public PlayerSpawnPointsController spawnPointsController;

    public List<Transform> spawnPoints; // Inspector���� 6���� ���� ����Ʈ�� �Ҵ��մϴ�.
    public List<Transform> selectedSpawnPoints; // ���õ� 4���� ���� ����Ʈ�� ������ ����Ʈ

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.StartHost();
        SelectRandomSpawnPoints();
        SpawnPlayer();
        SpawnAIPlayer();
        // ���� �ٷ� ����!
        StartSingleModeGame();
    }

    private void StartSingleModeGame()
    {
        gameManager.SetGameStateStart();
    }

    private void SpawnPlayer()
    {
        // Host �÷��̾� ĳ���ʹ� NetworkManager�� "Player Prefab" ������ �Բ� "Auto Create Player"�� �ڵ� ����. 
        // �� �κ�. StartHost ���� ��, �÷��̾� ĳ���� �����Ǵ� �κ� ������
        /*        GameObject playerObject = Instantiate(GameAssetsManager.Instance.GetCharacterPrefab_SinglePlayerInGame(PlayerDataManager.Instance.GetPlayerInGameData().characterClass));
                //playerObject.GetComponent<NetworkObject>().Spawn();
                playerObject.transform.SetParent(transform);
                // 0�� �÷��̾��� ���� ��ġ
                playerObject.transform.position = selectedSpawnPoints[0].position;*/
    }

    private void SpawnAIPlayer()
    {
        // 1~3�� AI�� ���� ��ġ
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
        // ���� ����Ʈ ����Ʈ�� �����մϴ�.
        List<Transform> tempList = new List<Transform>(spawnPointsController.spawnPoints);

        // Fisher-Yates ���� �˰����� ����Ͽ� ����Ʈ�� �����ϴ�.
        for (int i = tempList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            Transform temp = tempList[i];
            tempList[i] = tempList[randomIndex];
            tempList[randomIndex] = temp;
        }

        // ���� ����Ʈ���� ó�� 4���� ��Ҹ� �����մϴ�.
        selectedSpawnPoints = tempList.GetRange(0, 4);
    }
}
