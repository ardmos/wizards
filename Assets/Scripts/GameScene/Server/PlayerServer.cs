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
    public Rigidbody rb;

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
        Debug.Log($"OwnerClientId{OwnerClientId} Player (class : {character.characterClass.ToString()}) InitializeAIPlayerOnServer");

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

        // 플레이어 Layer 설정
        switch (OwnerClientId)
        {
            case 0:
                gameObject.layer = LayerMask.NameToLayer("Player0");
                break;
            case 1:
                gameObject.layer = LayerMask.NameToLayer("Player1");
                break;
            case 2:
                gameObject.layer = LayerMask.NameToLayer("Player2");
                break;
            case 3:
                gameObject.layer = LayerMask.NameToLayer("Player3");
                break;
            default: 
                break;
        }
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
    public void PlayerGotHitOnServer(sbyte damage, ulong clientWhoAttacked)
    {
        // 피격 처리 총괄.
        playerHPManager.TakingDamage(damage, clientWhoAttacked);
        // 각 Client UI 업데이트 지시 Damage Text Popup. 이젠 HPManager.TakingDamage에서 해줍니다.
        //playerClient.ShowDamageTextPopupClientRPC(damage);
        // 맞춘 플레이어 카메라 쉐이크
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientWhoAttacked];
        networkClient.PlayerObject.GetComponent<PlayerClient>().ActivateHitCameraShakeClientRPC();
    }

    public sbyte GetPlayerHP()
    {
        return playerHPManager.GetHP();
    }
}
