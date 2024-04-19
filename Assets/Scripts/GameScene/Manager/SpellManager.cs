using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 
/// 
/// 
/// 
/// 
/// 여기 분석부터 하면 됩니다. 
/// 단일책임의 원칙. 준수.  기사 스킬 처리 방법 반영. 여러 캐릭터들. 다양한 회피 스킬들. 가능하면 공격마법들도 개별처리 가능하도록. 
/// 기사 이펙트에 콜라이더 붙이는게 맞나
/// 
/// 
/// 
/// 
/// 
/// 마법을 Server Auth 방식으로 시전할 수 있도록 도와주는 스크립트 입니다.
/// Scroll의 획득과 적용의 과정도 관리합니다.
/// 서버에서 동작하는 스크립트들이 모여있어야 합니다. 추후 코드 정리하면서 확인 필요.
/// </summary>
public class SpellManager : NetworkBehaviour
{
    public static SpellManager Instance;

    // Spell Dictionary that the player is casting.
    private Dictionary<ulong, GameObject> playerCastingSpellPairs = new Dictionary<ulong, GameObject>();
    private Dictionary<ulong, Transform> playerMuzzlePairs = new Dictionary<ulong, Transform>();
    private Dictionary<ulong, List<SpellInfo>> spellInfoListOnServer = new Dictionary<ulong, List<SpellInfo>>();
    // 특정 플레이어의 ulong(클라이언트ID)값과, 그 플레이어가 Scroll을 획득할 때마다 Scroll의 효과가 적용될 Spell Slot의 Index를 담아두는 Queue를 갖고있는 변수입니다.
    private Dictionary<ulong, Queue<byte>> playerScrollSpellSlotQueueMapOnServer = new Dictionary<ulong, Queue<byte>>();
/*    private Queue<byte> playerScrollSpellSlotQueueOnClient = new Queue<byte>();*/


    private void Awake()
    {
        Instance = this;
    }

    #region Scroll 관련
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

/*    // On Client
    public void UpdatePlayerScrollSpellSlotQueueOnClient(Queue<byte> scrollQueue)
    {
        // Update Queue
        playerScrollSpellSlotQueueOnClient = new Queue<byte>(scrollQueue);

        Debug.Log($"UpdatePlayerScrollSpellSlotQueueOnClient. count:{playerScrollSpellSlotQueueOnClient.Count}");

        // Update UI
        if (playerScrollSpellSlotQueueOnClient.Count == 0)
            // Spell Scroll Count UI 비활성화
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.DeactivateUI();
        else
            // Spell Scroll Count UI 활성화 
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.ActivateAndUpdateUI();
    }*/

/*    public byte PeekPlayerScrollSpellSlotQueueOnClient()
    {
        return playerScrollSpellSlotQueueOnClient.Peek();
    }

    public int GetPlayerScrollSpellSlotCount()
    {
        return playerScrollSpellSlotQueueOnClient.Count;
    }

    public void DequeuePlayerScrollSpellSlotQueueOnClient()
    {
        // Dequeue
        playerScrollSpellSlotQueueOnClient.Dequeue();

        Debug.Log($"DequeuePlayerScrollSpellSlotQueueOnClient. count:{playerScrollSpellSlotQueueOnClient.Count}");

        // Update UI
        if (playerScrollSpellSlotQueueOnClient.Count == 0)
            // Spell Scroll Count UI 비활성화
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.DeactivateUI();
        else
            // Spell Scroll Count UI 활성화 
            GameSceneUIManager.Instance.buttonReadSpellScrollUIController.ActivateAndUpdateUI();
    }*/
    #endregion

    #region SpellInfo
    /// <summary>
    /// 변경된 SpellState를 Server에 보고합니다.
    /// </summary>
    /// <param name="spellIndex">업데이트하고싶은 마법의 Index</param>
    /// <param name="spellState">마법의 상태</param>
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        if (!spellInfoListOnServer.ContainsKey(clientId))
        {
            Debug.LogError($"{nameof(UpdatePlayerSpellStateServerRPC)} There is no SpellInfoList for this client. clientId:{clientId}");
            return;
        }

        spellInfoListOnServer[clientId][spellIndex].spellState = spellState;

        // 요청한 클라이언트의 currentSpellInfoList 동기화
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellControllerClientWizard>().UpdatePlayerSpellInfoArrayClientRPC(spellInfoListOnServer[clientId].ToArray());
    }

    /// <summary>
    /// Server측에서 보유한 SpellInfo 리스트 초기화 메소드 입니다.
    /// 플레이어 최초 생성시 호출됩니다.
    /// </summary>
    public void InitPlayerSpellInfoArrayOnServer(ulong clientId, SkillName[] spellNames)
    {
        List<SpellInfo> playerSpellInfoList = new List<SpellInfo>();
        foreach (SkillName spellName in spellNames)
        {
            SpellInfo spellInfo = new SpellInfo(SpellSpecifications.Instance.GetSpellDefaultSpec(spellName));
            spellInfo.ownerPlayerClientId = clientId;
            playerSpellInfoList.Add(spellInfo);
        }

        if (spellInfoListOnServer.ContainsKey(clientId))
        {
            spellInfoListOnServer[clientId] = playerSpellInfoList;
        }
        else
        {
            spellInfoListOnServer.Add(clientId, playerSpellInfoList);
        }
    }

    /// <summary>
    /// 특정 스펠의 스펠인포를 업데이트해주는 메소드 입니다.  <----메소드명 변경하는것 고려해보기
    /// 주로 스크롤 획득으로 인한 스펠 강화에 사용됩니다.
    /// 업데이트 후에 자동으로 클라이언트측과 동기화를 합니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdateScrollEffectServerRPC(ItemName scrollName, byte spellIndex, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        if (!spellInfoListOnServer.ContainsKey(clientId))
        {
            Debug.LogError($"UpdateScrollEffectServerRPC. There is no SpellInfoList for this client. clientId:{clientId}");
            return;
        }

        //Debug.Log($"scroll name:{scrollName}, spellIndex:{spellIndex} // spellInfo name:{spellInfoListOnServer[clientId][spellIndex].spellName}, lvl:{spellInfoListOnServer[clientId][spellIndex].level}, cooltime:{spellInfoListOnServer[clientId][spellIndex].coolTime}, moveSpeed:{spellInfoListOnServer[clientId][spellIndex].moveSpeed}");

        SpellInfo newSpellInfo = new SpellInfo(spellInfoListOnServer[clientId][spellIndex]);

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
        spellInfoListOnServer[clientId][spellIndex] = newSpellInfo;

        Debug.Log($"scroll name:{scrollName}, spellIndex:{spellIndex} // new spellInfo name:{spellInfoListOnServer[clientId][spellIndex].spellName}, lvl:{spellInfoListOnServer[clientId][spellIndex].level}, cooltime:{spellInfoListOnServer[clientId][spellIndex].coolTime}, moveSpeed:{spellInfoListOnServer[clientId][spellIndex].moveSpeed}");

        // 변경내용을 요청한 클라이언트와도 동기화
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellControllerClientWizard>().UpdatePlayerSpellInfoArrayClientRPC(spellInfoListOnServer[clientId].ToArray());

        // 적용 완료된 Scroll 정보가 담긴 Spell Slot Queue를 Dequeue.
        DequeuePlayerScrollSpellSlotQueueOnServer(clientId);
        networkClient.PlayerObject.GetComponent<PlayerClient>().GetComponent<PlayerSpellScrollQueueManagerClient>().DequeuePlayerScrollSpellSlotQueueOnClient();
    }

    /// <summary>
    /// 특정 client가 현재 보유중인 특정 마법의 정보를 알려주는 메소드 입니다.  
    /// </summary>
    /// <param name="clientId">알고싶은 Client의 ID</param>
    /// <param name="spellName">알고싶은 마법의 이름</param>
    /// <returns></returns>
    public SpellInfo GetSpellInfo(ulong clientId, SkillName spellName)
    {
        if (!spellInfoListOnServer.ContainsKey(clientId))
        {
            Debug.Log($"GetSpellInfo. 스펠정보를 찾을 수 없습니다. clienId:{clientId}, spellName:{spellName}");
            return null;
        }

        foreach (SpellInfo spellInfo in spellInfoListOnServer[clientId])
        {
            if (spellInfo.spellName == spellName)
            {
                return spellInfo;
            }
        }

        return null;
    }
    #endregion

    #region Defence Spell Cast
    /// <summary>
    /// 방어 마법 시전
    /// </summary>
    /// <param name="player"></param>
    [ServerRpc(RequireOwnership = false)]
    public void StartActivateDefenceSpellServerRPC(SkillName spellName, NetworkObjectReference player)
    {
        // GameObject 얻어내기 실패시 로그 출력
        if (!player.TryGet(out NetworkObject playerObject))
        {
            Debug.LogError("StartActivateDefenceSpellServerRPC Failed to Get NetworkObject from NetwrokObjectRefernce!");
            return;
        }
        ulong clientId = playerObject.OwnerClientId;

        // 마법 시전
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellName), playerObject.transform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.GetComponent<DefenceSpell>().InitSpellInfoDetail(GetSpellInfo(clientId, spellName));
        spellObject.transform.SetParent(playerObject.transform);
        spellObject.transform.localPosition = Vector3.zero;
        spellObject.GetComponent<DefenceSpell>().Activate();
    }
    #endregion

    #region Attack Spell Cast&Fire
    /// <summary>
    /// 공격 마법 생성해주기. 캐스팅 시작 ( NetworkObject는 Server에서만 생성 가능합니다 )
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void StartCastingAttackSpellServerRPC(SkillName spellName, NetworkObjectReference player)
    {
        // GameObject 얻어내기 실패시 로그 출력
        if (!player.TryGet(out NetworkObject playerObject))
        {
            Debug.LogError("StartCastingAttackSpellServerRPC Failed to Get NetworkObject from NetwrokObjectRefernce!");
            return;
        }
        ulong clientId = playerObject.OwnerClientId;

        // 포구 위치 찾기(Local posittion)
        Transform muzzleTransform = playerObject.GetComponentInChildren<MuzzlePos>().transform;
        playerMuzzlePairs[clientId] = muzzleTransform;

        // 포구에 발사체 위치시키기
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellName), muzzleTransform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        // 발사체 스펙 초기화 해주기
        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(clientId, spellName));
        Debug.Log($"{nameof(StartCastingAttackSpellServerRPC)} ownerClientId {spellInfo.ownerPlayerClientId}, spellName: {spellInfo.spellName}, spellLv: {spellInfo.level}");
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        spellObject.transform.SetParent(playerObject.transform);
        spellObject.transform.localPosition = muzzleTransform.localPosition;

        // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기
        spellObject.transform.forward = playerObject.transform.forward;

        // 플레이어가 시전중인 마법 목록에 저장하기
        if (playerCastingSpellPairs.ContainsKey(clientId)) playerCastingSpellPairs[clientId] = spellObject;
        else playerCastingSpellPairs.Add(clientId, spellObject);
    }

    /// <summary>
    /// 플레이어의 요청으로 현재 캐스팅중인 마법을 발사하는 메소드 입니다.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void ShootCastingSpellObjectServerRPC(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        GameObject spellObject = playerCastingSpellPairs[clientId];
        if (spellObject == null)
        {
            Debug.Log($"ShootCastingSpellObjectServerRPC : Wrong Request. Player{clientId} has no casting spell object.");
            return;
        }

        Debug.Log($"{nameof(ShootCastingSpellObjectServerRPC)} ownerClientId {clientId}");

        // 마법 발사
        spellObject.transform.SetParent(this.transform);
        float moveSpeed = spellObject.GetComponent<AttackSpell>().GetSpellInfo().moveSpeed;
        spellObject.GetComponent<AttackSpell>().Shoot(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // 포구 VFX
        MuzzleVFX(spellObject.GetComponent<AttackSpell>().GetMuzzleVFXPrefab(), playerMuzzlePairs[clientId]);
    }
    #endregion

    #region Spell Hit 
    /// <summary>
    /// 2. CollisionEnter 충돌 처리 (서버 권한 방식)
    /// Spell 본인의 입장에서 충돌에대한 판정을 서버에게 요청할 때 호출되는 메소드 입니다.
    /// </summary>
    /// <param name="collision"></param>
    public virtual void SpellHitOnServer(Collision collision, AttackSpell spell)
    {
        // 충돌한것이 마법인지 확인
        bool isSpellCollided = false;
        // 충돌 처리결과 저장
        SpellInfo collisionHandlingResult = null;

        // 일단 마법 정지
        spell.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        // 충돌한 오브젝트의 collider 저장
        Collider collider = collision.collider;

        // 충돌한게 Spell일 경우, 스펠간 충돌 결과 처리를 따로 진행합니다
        // 처리결과를 collisionHandlingResult 변수에 저장해뒀다가 DestroyParticle시에 castSpell 시킵니다.
        if (collider.CompareTag("Attack"))
        {
            isSpellCollided = true;
            SpellInfo thisSpell = spell.GetSpellInfo();
            SpellInfo opponentsSpell = collider.GetComponent<AttackSpell>().GetSpellInfo();

            collisionHandlingResult = spell.CollisionHandling(thisSpell, opponentsSpell);

            Debug.Log($"Spell끼리 충돌! ourSpell: name{thisSpell.spellName}, lvl{thisSpell.level}, owner{thisSpell.ownerPlayerClientId}  // " +
                $"opponentsSpell : name{opponentsSpell.spellName}, lvl{opponentsSpell.level}, owner{opponentsSpell.ownerPlayerClientId}");
        }

        // 충돌한게 플레이어일 경우! 
        // 1. player에게 GetHit() 시킴
        // 2. player가 GameMultiplayer(ServerRPC)에게 보고, player(ClientRPC)업데이트.
        else if (collider.CompareTag("Player"))
        {
            isSpellCollided = false;

            if (spell.GetSpellInfo() == null)
            {
                Debug.Log("AttackSpell Info is null");
            }

            PlayerClient player = collider.GetComponent<PlayerClient>();
            if (player != null)
            {
                byte damage = (byte)spell.GetSpellInfo().level;
                // 플레이어 피격을 서버에서 처리
                PlayerGotHitOnServer(damage, player);
            }
            else Debug.LogError("Player is null!");
        }

        // 기타 오브젝트 충돌
        else
        {
            isSpellCollided = false;
            Debug.Log($"{collider.name} Hit!");
        }

        spell.GetComponent<Collider>().enabled = false;

        List<GameObject> trails = spell.GetTrails();
        if (trails.Count > 0)
        {
            for (int i = 0; i < trails.Count; i++)
            {
                trails[i].transform.parent = null;
                ParticleSystem particleSystem = trails[i].GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    particleSystem.Stop();
                    Destroy(particleSystem.gameObject, particleSystem.main.duration + particleSystem.main.startLifetime.constantMax);
                }
            }
        }

        // 적중 효과 VFX
        SpellManager.Instance.HitVFX(spell.GetHitVFXPrefab(), collision);

        // 스펠끼리 충돌해서 우리 스펠이 이겼을 때 계산 결과에 따라 충돌 위치에 새로운 마법 생성. 
        if (isSpellCollided && collisionHandlingResult.level > 0)
        {
            Debug.Log($"our spell is win! generate spell.name:{collisionHandlingResult.spellName}, spell.level :{collisionHandlingResult.level}, spell.owner: {collisionHandlingResult.ownerPlayerClientId} ");
            SpawnSpellObjectOnServer(collisionHandlingResult, spell.transform);
        }

        Destroy(spell.gameObject, 0.2f);
    }

    /// <summary>
    /// 마법 생성하기.
    /// Spell끼리 충돌 이후 호출되는 메소드 입니다.
    /// 플레이어가 캐스팅한 마법을 발사하는 메소드는 아래 ServerRPC 메소드 입니다.
    /// </summary>
    /// 필요한것.
    ///     1. 포구 Transform
    ///     2. 쏠 발사체 SpellName
    public void SpawnSpellObjectOnServer(SpellInfo spellInfo, Transform spawnPosition)
    {
        // 포구에 마법 발사체 위치시키기
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellInfo.spellName), spawnPosition.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        Debug.Log($"SpawnSpellObjectOnServer!! spellInfo.ownerClientId : {spellInfo.ownerPlayerClientId}, name:{spellInfo.spellName}, lvl:{spellInfo.level}");

        // 마법 발사체 방향 조정하기
        spellObject.transform.forward = spawnPosition.forward;

        // 마법 발사
        spellObject.transform.SetParent(this.transform);
        float moveSpeed = spellInfo.moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // 포구 VFX
        MuzzleVFX(spellObject.GetComponent<AttackSpell>().GetMuzzleVFXPrefab(), spawnPosition);
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
    public void PlayerGotHitOnServer(byte damage, PlayerClient player)
    {
        ulong clientId = player.OwnerClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            // 요청한 플레이어 현재 HP값 가져오기 
            PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
            sbyte playerHP = playerData.hp;
            sbyte playerMaxHP = playerData.maxHp;

            // HP보다 Damage가 클 경우(게임오버 처리는 Player에서 HP잔량 파악해서 알아서 한다.)
            if (playerHP <= damage)
            {
                // HP 0
                playerHP = 0;
            }
            else
            {
                // HP 감소 계산
                playerHP -= (sbyte)damage;
            }

            // 각 Client UI 업데이트 지시. HPBar & Damage Popup
            PlayerHPManager.Instance.UpdatePlayerHP(clientId, playerHP, playerMaxHP);
            player.GetComponent<PlayerClient>().ShowDamagePopupClientRPC(damage);

            Debug.Log($"GameMultiplayer.PlayerGotHitOnServer()  Player{clientId} got {damage} damage new HP:{playerHP}");
        }
    }
    #endregion

    #region Spell VFX
    // 포구 VFX 
    public void MuzzleVFX(GameObject muzzleVFXPrefab, Transform muzzleTransform)
    {
        if (muzzleVFXPrefab == null)
        {
            Debug.Log($"MuzzleVFX muzzleVFXPrefab is null");
            return;
        }

        //Debug.Log($"MuzzleVFX muzzlePos:{muzzleTransform.position}, muzzleLocalPos:{muzzleTransform.localPosition}");
        GameObject muzzleVFX = Instantiate(muzzleVFXPrefab, muzzleTransform.position, Quaternion.identity);
        muzzleVFX.GetComponent<NetworkObject>().Spawn();
        //muzzleVFX.transform.SetParent(transform);
        muzzleVFX.transform.position = muzzleTransform.position;
        muzzleVFX.transform.forward = muzzleTransform.forward;
        var particleSystem = muzzleVFX.GetComponent<ParticleSystem>();

        if (particleSystem == null)
        {
            particleSystem = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
        }

        Destroy(muzzleVFX, particleSystem.main.duration);
    }

    // 적중효과 VFX
    public void HitVFX(GameObject hitVFXPrefab, Collision collision)
    {
        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point;

        if (hitVFXPrefab != null)
        {
            //Debug.Log($"hitVFXPrefab is Not null");
            var hitVFX = Instantiate(hitVFXPrefab, pos, rot) as GameObject;
            hitVFX.GetComponent<NetworkObject>().Spawn();
            var particleSystem = hitVFX.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                particleSystem = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
            }
            Destroy(hitVFX, particleSystem.main.duration);
        }
        else Debug.Log($"hitVFXPrefab is null");
    }
    #endregion
}
