using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScrollManagerServer : NetworkBehaviour
{
    public static ScrollManagerServer Instance { get; private set; }

    // 특정 플레이어의 ulong(클라이언트ID)값과, 그 플레이어가 Scroll을 획득할 때마다 Scroll의 효과가 적용될 Spell Slot의 Index를 담아두는 Queue를 갖고있는 변수입니다.
    private Dictionary<ulong, Queue<byte>> playerScrollSpellSlotQueueMapOnServer = new Dictionary<ulong, Queue<byte>>();

    private void Awake()
    {
        Instance = this; 
    }

    // On Server
    public void EnqueuePlayerScrollSpellSlotQueueOnServer(ulong clientId, byte queueElement)
    {
        if (playerScrollSpellSlotQueueMapOnServer.ContainsKey(clientId))
        {
            playerScrollSpellSlotQueueMapOnServer[clientId].Enqueue(queueElement);
        }
        else
            playerScrollSpellSlotQueueMapOnServer.Add(clientId, new Queue<byte>(new byte[] { queueElement }));


        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().UpdateScrollQueueClientRPC(playerScrollSpellSlotQueueMapOnServer[clientId].ToArray());
        networkClient.PlayerObject.GetComponent<PlayerClient>().ShowItemAcquiredUIClientRPC();
    }

    private void DequeuePlayerScrollSpellSlotQueueOnServer(ulong clientId)
    {
        playerScrollSpellSlotQueueMapOnServer[clientId].Dequeue();
    }

    /// <summary>
    /// 랜덤스크롤 생성기 입니다. 플레이어가 PopupSelectScrollEffectUI를 실행할 때 마다 요청해오면 랜덤한 스크롤 효과 3 개를 뽑아줍니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void GetUniqueRandomScrollsServerRPC(ServerRpcParams serverRpcParams = default)
    {
        List<int> randomNumbers = GenerateUniqueRandomNumbers();
        ItemName[] scrollNames = new ItemName[3] {
              ItemName.ScrollStart+1+randomNumbers[0],
              ItemName.ScrollStart+1+randomNumbers[1],
              ItemName.ScrollStart+1+randomNumbers[2]
        };

        // 랜덤으로 생성된 스크롤 효과 목록을 요청해온 플레이어에게 공유
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().SetScrollEffectsToPopupUIClientRPC(scrollNames);
    }

    /// <summary>
    /// 특정 스펠의 스펠인포를 업데이트해주는 메소드 입니다.
    /// 주로 스크롤 획득으로 인한 스펠 강화에 사용됩니다.
    /// 업데이트 후에 자동으로 클라이언트측과 동기화를 합니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdateScrollEffectServerRPC(ItemName scrollName, byte spellIndex, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        SpellManagerServerWizard spellManagerServerWizard = networkClient.PlayerObject.GetComponent<SpellManagerServerWizard>();
        SpellInfo newSpellInfo = new SpellInfo(spellManagerServerWizard.playerOwnedSpellInfoListOnServer[spellIndex]);

        // 기본 스펠의 defautl info값에 scrollName별로 다른 값을 추가해서 아래 UpdatePlayerSpellInfo에 넘겨줍니다.
        switch (scrollName)
        {
            case ItemName.Scroll_LevelUp:
                newSpellInfo.level += 1;
                break;
            case ItemName.Scroll_FireRateUp:
                if (newSpellInfo.coolTime > 0.2f) newSpellInfo.coolTime -= 0.4f;
                else newSpellInfo.coolTime = 0f;
                break;
            case ItemName.Scroll_FlySpeedUp:
                newSpellInfo.moveSpeed += 10f;
                break;
            case ItemName.Scroll_Deploy:
                newSpellInfo.moveSpeed = 0f;
                break;
            // 유도 마법 미구현
            //case ItemName.Scroll_Guide:
            // break;
            default:
                Debug.Log("UpdateScrollEffectServerRPC. 스크롤 이름을 찾을 수 없습니다.");
                break;
        }

        // 변경내용 서버에 저장
        spellManagerServerWizard.playerOwnedSpellInfoListOnServer[spellIndex] = newSpellInfo;

        // 변경내용을 요청한 클라이언트와도 동기화
        networkClient.PlayerObject.GetComponent<SpellManagerClientWizard>().UpdatePlayerSpellInfoArrayClientRPC(spellManagerServerWizard.playerOwnedSpellInfoListOnServer.ToArray());

        // 적용 완료된 Scroll 정보가 담긴 Spell Slot Queue를 Dequeue.
        DequeuePlayerScrollSpellSlotQueueOnServer(clientId);
        networkClient.PlayerObject.GetComponent<PlayerClient>().GetComponent<PlayerSpellScrollQueueManagerClient>().DequeuePlayerScrollSpellSlotQueueOnClient();
    }

    private List<int> GenerateUniqueRandomNumbers()
    {
        List<int> numbers = new List<int>();
        int scrollItemMaxIndex = ItemName.ScrollEnd - (ItemName.ScrollStart + 1);
        while (numbers.Count < 3)
        {
            int randomNumber = UnityEngine.Random.Range(0, scrollItemMaxIndex);
            if (!numbers.Contains(randomNumber))
            {
                numbers.Add(randomNumber);
            }
        }

        return numbers;
    }
}
