using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScrollManagerServer : NetworkBehaviour
{
    public static ScrollManagerServer Instance { get; private set; }

    // Ư�� �÷��̾��� ulong(Ŭ���̾�ƮID)����, �� �÷��̾ Scroll�� ȹ���� ������ Scroll�� ȿ���� ����� Spell Slot�� Index�� ��Ƶδ� Queue�� �����ִ� �����Դϴ�.
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
    /// ������ũ�� ������ �Դϴ�. �÷��̾ PopupSelectScrollEffectUI�� ������ �� ���� ��û�ؿ��� ������ ��ũ�� ȿ�� 3 ���� �̾��ݴϴ�.
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

        // �������� ������ ��ũ�� ȿ�� ����� ��û�ؿ� �÷��̾�� ����
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().SetScrollEffectsToPopupUIClientRPC(scrollNames);
    }

    /// <summary>
    /// Ư�� ������ ���������� ������Ʈ���ִ� �޼ҵ� �Դϴ�.
    /// �ַ� ��ũ�� ȹ������ ���� ���� ��ȭ�� ���˴ϴ�.
    /// ������Ʈ �Ŀ� �ڵ����� Ŭ���̾�Ʈ���� ����ȭ�� �մϴ�.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdateScrollEffectServerRPC(ItemName scrollName, byte spellIndex, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        SpellManagerServerWizard spellManagerServerWizard = networkClient.PlayerObject.GetComponent<SpellManagerServerWizard>();
        SpellInfo newSpellInfo = new SpellInfo(spellManagerServerWizard.playerOwnedSpellInfoListOnServer[spellIndex]);

        // �⺻ ������ defautl info���� scrollName���� �ٸ� ���� �߰��ؼ� �Ʒ� UpdatePlayerSpellInfo�� �Ѱ��ݴϴ�.
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
            // ���� ���� �̱���
            //case ItemName.Scroll_Guide:
            // break;
            default:
                Debug.Log("UpdateScrollEffectServerRPC. ��ũ�� �̸��� ã�� �� �����ϴ�.");
                break;
        }

        // ���泻�� ������ ����
        spellManagerServerWizard.playerOwnedSpellInfoListOnServer[spellIndex] = newSpellInfo;

        // ���泻���� ��û�� Ŭ���̾�Ʈ�͵� ����ȭ
        networkClient.PlayerObject.GetComponent<SpellManagerClientWizard>().UpdatePlayerSpellInfoArrayClientRPC(spellManagerServerWizard.playerOwnedSpellInfoListOnServer.ToArray());

        // ���� �Ϸ�� Scroll ������ ��� Spell Slot Queue�� Dequeue.
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
