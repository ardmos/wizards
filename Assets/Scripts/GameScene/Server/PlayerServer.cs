using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class PlayerServer : NetworkBehaviour
{
    public PlayerClient playerClient;
    public PlayerHPManagerServer playerHPManager;
    public SkillSpellManagerServer skillSpellManagerServer;
    [Header("물리 관련")]
    public Rigidbody rb;
    public Collider _collider;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        ICharacter character = (ICharacter)playerClient; //GetComponent<ICharacter>();
        InitializePlayerOnServer(character);
    }

    /// <summary>
    /// 서버측 InitializePlayer
    /// 1. 스폰위치 초기화
    /// 2. HP 초기화 & 브로드캐스팅
    /// 3. 특정 플레이어가 보유한 스킬 목록 저장 & 해당플레이어에게 공유
    /// </summary>
    public void InitializePlayerOnServer(ICharacter character)
    {
        //Debug.Log($"OwnerClientId{OwnerClientId} Player (class : {character.characterClass.ToString()}) InitializeAIPlayerOnServer");

        PlayerSpawnPointsController spawnPointsController = FindObjectOfType<PlayerSpawnPointsController>();

        if (GameAssetsManager.Instance == null)
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
        //transform.position = spawnPointsController.GetSpawnPoint(GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId));
        transform.position = spawnPointsController.GetSpawnPoint();

        // HP 초기화
        // 현재 HP 저장 및 설정
        playerHPManager.InitPlayerHP(character);

        // 플레이어가 보유한 스킬 목록 저장
        skillSpellManagerServer.InitPlayerSpellInfoArrayOnServer(character.skills);

        // 플레이어 InitializePlayer 시작, 스킬 목록을 클라이언트측(SpellController)에 저장 ( 수정해야함
        playerClient.InitializePlayerClientRPC(character.skills);
    }

    // 스크롤 활용. 스킬 강화 VFX 실행
    [ServerRpc(RequireOwnership = false)]
    public void StartApplyScrollVFXServerRPC()
    {
        GameObject vfxHeal = Instantiate(GameAssetsManager.Instance.gameAssets.vfx_SpellUpgrade, transform);
        vfxHeal.GetComponent<NetworkObject>().Spawn();
        vfxHeal.transform.SetParent(transform);
        //vfxHeal.transform.localPosition = new Vector3(0f, 0.1f, 0f);
    }

    /// <summary>
    /// 스킬 충돌 처리(서버에서 동작)
    /// 플레이어 적중시 ( 다른 마법이나 구조물과의 충돌 처리는 Spell.cs에 있다. 코드 정리 필요)
    /// clientID와 HP 연계해서 처리. 
    /// 충돌 녀석이 플레이어일 경우 실행. 
    /// ClientID로 리스트 검색 후 HP 수정시키고 업데이트된 내용 브로드캐스팅.
    /// 수신측은 ClientID의 플레이어 HP 업데이트. 
    /// 서버에서 구동되는 스크립트.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="clientId"></param>
    public void TakingDamageWithCameraShake(sbyte damage, ulong clientWhoAttacked)
    {
        // 피격 처리 총괄.
        playerHPManager.TakingDamage(damage, clientWhoAttacked);

        // 공격자가 Player인가? AI인가? 
        if (GameMultiplayer.Instance.GetPlayerDataFromClientId(clientWhoAttacked).isAI)
        {
            // AI라면 카메라 쉐이크는 하지 않는다.
            return;
        }

        // 공격자가 Player라면 카메라 쉐이크 
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientWhoAttacked];
        if(networkClient.PlayerObject.TryGetComponent<PlayerClient>(out PlayerClient playerClient)){
            playerClient.ActivateHitCameraShakeClientRPC();
        }
    }

    // 파이어볼 도트 대미지를 받는 Coroutine
    public IEnumerator TakeDamageOverTime(sbyte damagePerSecond, float duration, ulong clientWhoAttacked)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            // 1초 대기
            yield return new WaitForSeconds(1);

            playerHPManager.TakingDamage(damagePerSecond, clientWhoAttacked);

            elapsed += 1;
        }
    }

    public sbyte GetPlayerHP()
    {
        return playerHPManager.GetHP();
    }

    public void GameOver()
    {
        PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);

        if (playerData.playerGameState != PlayerGameState.Playing) return;
        Debug.Log($"Player{OwnerClientId} is GameOver");
        //playerData.playerGameState = PlayerGameState.GameOver;

        // 물리충돌 해제
        rb.isKinematic = true;
        _collider.enabled = false;
        // 유도당하지 않도록 Tag 변경
        tag = "GameOver";
    }
}
