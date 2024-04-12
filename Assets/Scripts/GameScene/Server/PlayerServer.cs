using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerServer : NetworkBehaviour
{

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        ICharacter character = GetComponent<ICharacter>();
        InitializePlayerOnServer(character, OwnerClientId);
    }

    /// <summary>
    /// 서버측 InitializePlayer
    /// 1. 스폰위치 초기화
    /// 2. HP 초기화 & 브로드캐스팅
    /// 3. 특정 플레이어가 보유한 스킬 목록 저장 & 해당플레이어에게 공유
    /// </summary>
    public void InitializePlayerOnServer(ICharacter character, ulong requestedInitializeClientId)
    {
        Debug.Log($"OwnerClientId{OwnerClientId} Player InitializePlayerOnServer");

        PlayerSpawnPointsController spawnPointsController = FindObjectOfType<PlayerSpawnPointsController>();

        if (GameAssets.instantiate == null)
        {
            Debug.Log($"{nameof(InitializePlayerOnServer)}, GameAssets를 찾지 못했습니다.");
            return;
        }

        if (spawnPointsController == null)
        {
            Debug.LogError($"{nameof(InitializePlayerOnServer)}, 스폰위치를 특정하지 못했습니다.");
            return;
        }

        // 스폰 위치 초기화   
        transform.position = spawnPointsController.GetSpawnPoint(GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));

        // HP 초기화
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(requestedInitializeClientId);
        playerData.playerHP = character.hp;
        playerData.playerMaxHP = character.hp;
        GameMultiplayer.Instance.SetPlayerDataFromClientId(requestedInitializeClientId, playerData);

        // 현재 HP 저장 및 설정
        PlayerHPManager.Instance.UpdatePlayerHP(requestedInitializeClientId, playerData.playerHP, playerData.playerMaxHP);

        // 특정 플레이어가 보유한 스킬 목록 저장
        SpellManager.Instance.InitPlayerSpellInfoArrayOnServer(requestedInitializeClientId, character.skills);

        // Spawn된 클라이언트측 InitializePlayer 시작
        GetComponent<PlayerClient>().InitializePlayerClientRPC(character);
    }

    // 스크롤 활용. 스킬 강화 VFX 실행
    [ServerRpc (RequireOwnership = false)]
    public void StartApplyScrollVFXServerRPC()
    {
        GameObject vfxHeal = Instantiate(GameAssets.instantiate.vfx_SpellUpgrade, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
        vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);
    }
}
