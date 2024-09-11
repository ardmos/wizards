using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class PlayerServerSingleMode : NetworkBehaviour
{
    private const int DEFAULT_SCORE = 300;

    public PlayerClient playerClient;
    public PlayerHPManagerServer playerHPManager;
    public PlayerAnimator playerAnimator;
    public SpellManagerServer skillSpellManagerServer;
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
        transform.position = spawnPointsController.GetSpawnPoint();

        // HP 초기화
        // 현재 HP 저장 및 설정
        playerHPManager.InitPlayerHP(character);

        // 플레이어가 보유한 스킬 목록 저장
        skillSpellManagerServer.InitPlayerSpellInfoArrayOnServer(character.skills);

        Debug.Log("0");
        // 플레이어 InitializePlayer 시작, 스킬 목록을 클라이언트측(SpellController)에 저장 ( 수정해야함
        playerClient.InitializePlayerClientRPC();
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

    public sbyte GetPlayerHP()
    {
        return playerHPManager.GetHP();
    }

    // 게임오버 처리. 서버권한 방식.
    public void GameOver(ulong clientWhoAttacked)
    {
        PlayerInGameData playerData = GameSingleplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);

        if (playerData.playerGameState != PlayerGameState.Playing) return;
        Debug.Log($"Player{OwnerClientId} is GameOver");
        //playerData.playerGameState = PlayerGameState.GameOver;

        // 물리충돌 해제
        rb.isKinematic = true;
        _collider.enabled = false;
        // 유도당하지 않도록 Tag 변경
        tag = "GameOver";

        // 점수 계산
        CalcScore(clientWhoAttacked);
        // 플레이어 게임오버 애니메이션 실행
        playerAnimator.UpdatePlayerMoveAnimationOnServer(PlayerMoveAnimState.GameOver);
        // 해당 플레이어 조작 불가 처리 및 게임오버 팝업 띄우기.
        playerClient.SetPlayerGameOverClientRPC();
        // 플레이어 이름 & HP UI off
        playerClient.OffPlayerUIClientRPC();
        // 게임오버 플레이어 사실을 서버에 기록.
        SingleplayerGameManager.Instance.UpdatePlayerGameOverOnServer(OwnerClientId, clientWhoAttacked);

        // 아이템 드랍
        DropItem();
    }

    private void CalcScore(ulong clientWhoAttacked)
    {
        // 스스로 게임오버 당한 경우, 게임 내 모든 플레이어들에게 점수를 줍니다. 
        if (clientWhoAttacked == OwnerClientId)
        {
            foreach (PlayerInGameData playerInGameData in GameSingleplayer.Instance.GetPlayerDataNetworkList())
            {
                GameSingleplayer.Instance.AddPlayerScore(playerInGameData.clientId, DEFAULT_SCORE);
            }
        }
        // 일반적인 경우 상대 플레이어 300스코어 획득
        else
        {
            GameSingleplayer.Instance.AddPlayerScore(clientWhoAttacked, DEFAULT_SCORE);
        }
    }

    private void DropItem()
    {
        // 제너레이트 아이템 포션
        Vector3 newItemHPPotionPos = new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z);
        GameObject hpPotionObject = Instantiate(GameAssetsManager.Instance.GetItemHPPotionObject(), newItemHPPotionPos, transform.rotation, SingleplayerGameManager.Instance.transform);

        if (!hpPotionObject) return;

        if (hpPotionObject.TryGetComponent<NetworkObject>(out NetworkObject hpPotionObjectNetworkObject))
        {
            hpPotionObjectNetworkObject.Spawn();
            if (SingleplayerGameManager.Instance)
            {
                hpPotionObject.transform.parent = SingleplayerGameManager.Instance.transform;
                hpPotionObject.transform.position = newItemHPPotionPos;
            }
        }

        // 제너레이트 아이템 스크롤
        Vector3 newItemScrollPos = new Vector3(transform.position.x - 0.5f, transform.position.y, transform.position.z);
        GameObject scrollObject = Instantiate(GameAssetsManager.Instance.GetItemScrollObject(), newItemScrollPos, transform.rotation, SingleplayerGameManager.Instance.transform);

        if (!scrollObject) return;

        if (scrollObject.TryGetComponent<NetworkObject>(out NetworkObject scrollObjectNetworkObject))
        {
            scrollObjectNetworkObject.Spawn();
            if (SingleplayerGameManager.Instance)
            {
                scrollObject.transform.parent = SingleplayerGameManager.Instance.transform;
                scrollObject.transform.position = newItemScrollPos;
            }
        }
    }
}
