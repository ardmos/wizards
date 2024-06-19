using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 게임씬에서 동작하는 스크롤 매니저 스크립트
/// Server에서만 동작하도록 제한을 걸어줄 필요가 있습니다.
/// </summary>

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
        // 서버측에 스크롤 획득 정보를 저장
        if (playerScrollSpellSlotQueueMapOnServer.ContainsKey(clientId))
        {
            playerScrollSpellSlotQueueMapOnServer[clientId].Enqueue(queueElement);
        }
        else
            playerScrollSpellSlotQueueMapOnServer.Add(clientId, new Queue<byte>(new byte[] { queueElement }));

        // 클라이언트측UI 업데이트
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().UpdateScrollGetUIClientRPC(playerScrollSpellSlotQueueMapOnServer[clientId].Count);
    }

    private void DequeuePlayerScrollSpellSlotQueueOnServer(ulong clientId)
    {
        playerScrollSpellSlotQueueMapOnServer[clientId].Dequeue();
    }

    /// <summary>
    /// 랜덤스크롤 생성기 입니다. 플레이어가 PopupSelectScrollEffectUI를 실행할 때 마다 요청해오면 랜덤한 스크롤 효과 3 개를 뽑아줍니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void GetUniqueRandomAblilitiesServerRPC(ServerRpcParams serverRpcParams = default)
    {
        // 능력을 세 개 뽑아서 ClientRPC를 통해 리턴해줘야한다. 

        // 능력 뽑기
        List<SkillUpgradeOptionDTO> randomSkillUpgradesDTO = RandomSkillOptionProviderSystem.GetRandomSkillUpgrades();

/*        Debug.Log("랜덤 스킬 업그레이드 옵션:");
        foreach (var upgrade in randomSkillUpgrades)
        {
            Debug.Log(upgrade.GetDescription());
        }*/

        // 랜덤으로 생성된 스크롤 효과 목록을 요청해온 플레이어에게 공유
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<PlayerClient>().InitSelectScrollEffectsPopupUIClientRPC(randomSkillUpgradesDTO.ToArray());
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
        // Wizard Ruke Player
        if (networkClient.PlayerObject.TryGetComponent<SpellManagerServerWizard>(out SpellManagerServerWizard spellManagerServerWizard))
        {
            SpellInfo newSpellInfo = new SpellInfo(spellManagerServerWizard.GetSpellInfo(spellIndex));

/*            // 기본 스펠의 defautl info값에 scrollName별로 다른 값을 추가해서 아래 UpdatePlayerSpellInfo에 넘겨줍니다.
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
            }*/

            // 변경내용 서버에 저장
            spellManagerServerWizard.SetSpellInfo(spellIndex, newSpellInfo);

            // 변경내용을 요청한 클라이언트와도 동기화
            networkClient.PlayerObject.GetComponent<SkillSpellManagerClient>().UpdatePlayerSpellInfoArrayClientRPC(spellManagerServerWizard.GetSpellInfoList().ToArray());

            // 적용 완료된 Scroll 정보가 담긴 Spell Slot Queue를 Dequeue.
            DequeuePlayerScrollSpellSlotQueueOnServer(clientId);
            // Client측에도 동기화
            //networkClient.PlayerObject.GetComponent<PlayerClient>().GetComponent<PlayerSpellScrollQueueManagerClient>().DequeuePlayerScrollSpellSlotQueueOnClient();
        }

        // Knight Buzz Player
        else if (networkClient.PlayerObject.TryGetComponent<SkillManagerServerKnight>(out SkillManagerServerKnight skillManagerServerKnight))
        {
            SpellInfo newSpellInfo = new SpellInfo(skillManagerServerKnight.GetSpellInfo(spellIndex));

/*            // 기본 스펠의 defautl info값에 scrollName별로 다른 값을 추가해서 아래 UpdatePlayerSpellInfo에 넘겨줍니다.
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
            }*/

            // 변경내용 서버에 저장
            skillManagerServerKnight.SetSpellInfo(spellIndex, newSpellInfo);

            // 변경내용을 요청한 클라이언트와도 동기화
            networkClient.PlayerObject.GetComponent<SkillSpellManagerClient>().UpdatePlayerSpellInfoArrayClientRPC(skillManagerServerKnight.GetSpellInfoList().ToArray());

            // 적용 완료된 Scroll 정보가 담긴 Spell Slot Queue를 Dequeue.
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
