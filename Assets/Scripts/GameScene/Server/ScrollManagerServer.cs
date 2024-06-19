using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���Ӿ����� �����ϴ� ��ũ�� �Ŵ��� ��ũ��Ʈ
/// Server������ �����ϵ��� ������ �ɾ��� �ʿ䰡 �ֽ��ϴ�.
/// </summary>

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
        // �������� ��ũ�� ȹ�� ������ ����
        if (playerScrollSpellSlotQueueMapOnServer.ContainsKey(clientId))
        {
            playerScrollSpellSlotQueueMapOnServer[clientId].Enqueue(queueElement);
        }
        else
            playerScrollSpellSlotQueueMapOnServer.Add(clientId, new Queue<byte>(new byte[] { queueElement }));

        // Ŭ���̾�Ʈ��UI ������Ʈ
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().UpdateScrollGetUIClientRPC(playerScrollSpellSlotQueueMapOnServer[clientId].Count);
    }

    private void DequeuePlayerScrollSpellSlotQueueOnServer(ulong clientId)
    {
        playerScrollSpellSlotQueueMapOnServer[clientId].Dequeue();
    }

    /// <summary>
    /// ������ũ�� ������ �Դϴ�. �÷��̾ PopupSelectScrollEffectUI�� ������ �� ���� ��û�ؿ��� ������ ��ũ�� ȿ�� 3 ���� �̾��ݴϴ�.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void GetUniqueRandomAblilitiesServerRPC(ServerRpcParams serverRpcParams = default)
    {
        // �ɷ��� �� �� �̾Ƽ� ClientRPC�� ���� ����������Ѵ�. 

        // �ɷ� �̱�
        List<SkillUpgradeOptionDTO> randomSkillUpgradesDTO = RandomSkillOptionProviderSystem.GetRandomSkillUpgrades();

/*        Debug.Log("���� ��ų ���׷��̵� �ɼ�:");
        foreach (var upgrade in randomSkillUpgrades)
        {
            Debug.Log(upgrade.GetDescription());
        }*/

        // �������� ������ ��ũ�� ȿ�� ����� ��û�ؿ� �÷��̾�� ����
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().InitSelectScrollEffectsPopupUIClientRPC(randomSkillUpgradesDTO.ToArray());
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
        // Wizard Ruke Player
        if (networkClient.PlayerObject.TryGetComponent<SpellManagerServerWizard>(out SpellManagerServerWizard spellManagerServerWizard))
        {
            SpellInfo newSpellInfo = new SpellInfo(spellManagerServerWizard.GetSpellInfo(spellIndex));

/*            // �⺻ ������ defautl info���� scrollName���� �ٸ� ���� �߰��ؼ� �Ʒ� UpdatePlayerSpellInfo�� �Ѱ��ݴϴ�.
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
            }*/

            // ���泻�� ������ ����
            spellManagerServerWizard.SetSpellInfo(spellIndex, newSpellInfo);

            // ���泻���� ��û�� Ŭ���̾�Ʈ�͵� ����ȭ
            networkClient.PlayerObject.GetComponent<SkillSpellManagerClient>().UpdatePlayerSpellInfoArrayClientRPC(spellManagerServerWizard.GetSpellInfoList().ToArray());

            // ���� �Ϸ�� Scroll ������ ��� Spell Slot Queue�� Dequeue.
            DequeuePlayerScrollSpellSlotQueueOnServer(clientId);
            // Client������ ����ȭ
            //networkClient.PlayerObject.GetComponent<PlayerClient>().GetComponent<PlayerSpellScrollQueueManagerClient>().DequeuePlayerScrollSpellSlotQueueOnClient();
        }

        // Knight Buzz Player
        else if (networkClient.PlayerObject.TryGetComponent<SkillManagerServerKnight>(out SkillManagerServerKnight skillManagerServerKnight))
        {
            SpellInfo newSpellInfo = new SpellInfo(skillManagerServerKnight.GetSpellInfo(spellIndex));

/*            // �⺻ ������ defautl info���� scrollName���� �ٸ� ���� �߰��ؼ� �Ʒ� UpdatePlayerSpellInfo�� �Ѱ��ݴϴ�.
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
            }*/

            // ���泻�� ������ ����
            skillManagerServerKnight.SetSpellInfo(spellIndex, newSpellInfo);

            // ���泻���� ��û�� Ŭ���̾�Ʈ�͵� ����ȭ
            networkClient.PlayerObject.GetComponent<SkillSpellManagerClient>().UpdatePlayerSpellInfoArrayClientRPC(skillManagerServerKnight.GetSpellInfoList().ToArray());

            // ���� �Ϸ�� Scroll ������ ��� Spell Slot Queue�� Dequeue.
            DequeuePlayerScrollSpellSlotQueueOnServer(clientId);
            //networkClient.PlayerObject.GetComponent<PlayerClient>().GetComponent<PlayerSpellScrollQueueManagerClient>().DequeuePlayerScrollSpellSlotQueueOnClient();
        }

    }

/*    private List<int> GenerateUniqueRandomNumbers()
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
    }*/
}
