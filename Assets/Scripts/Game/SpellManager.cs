using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.ParticleSystem;
/// <summary>
/// 마법을 Server Auth 방식으로 시전할 수 있도록 도와주는 스크립트 입니다.
/// 서버에서 동작하는 스크립트들이 모여있어야 합니다. 추후 코드 정리하면서 확인 필요.
/// </summary>
public class SpellManager : NetworkBehaviour
{
    public static SpellManager Instance;

    public event EventHandler OnSpellStateChanged;


    // Spell Dictionary that the player is casting.
    private Dictionary<ulong, GameObject> playerCastingSpellPairs = new Dictionary<ulong, GameObject>();
    private Dictionary<ulong, Transform> playerMuzzlePairs = new Dictionary<ulong, Transform>();

    // 서버에 저장되는 내용
    [SerializeField] private Dictionary<ulong, List<SpellInfo>> spellInfoListOnServer = new Dictionary<ulong, List<SpellInfo>>();


    private void Awake()
    {      
        Instance = this;
    }
    
    #region SpellInfo
    /// <summary>
    /// Spell state를 얻을 수 있는 메소드 입니다
    /// </summary>
    /// <param name="spellIndex"></param>
    /// <returns></returns>
    public SpellState GetSpellStateFromSpellIndexOnServer(ulong clientId, ushort spellIndex)
    {
        return spellInfoListOnServer[clientId][spellIndex].spellState;
    }
    
    /// <summary>
    /// 변경된 SpellState를 Server에 보고합니다. 보고받은 Server는 Server의 PlayerAnimator에게 애니메이션 변경을 지시합니다.
    /// </summary>
    /// <param name="spellIndex"></param>
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        if (!spellInfoListOnServer.ContainsKey(clientId))
        {
            Debug.LogError($"UpdatePlayerSpellStateServerRPC. There is no SpellInfoList for this client. clientId:{clientId}");
            return;
        }

        spellInfoListOnServer[clientId][spellIndex].spellState = spellState;
        OnSpellStateChanged?.Invoke(this, new SpellStateEventData(clientId, spellState));
        Debug.Log($"SpellController UpdatePlayerSpellStateServerRPC: {spellIndex}");

        // 요청한 클라이언트의 currentSpellInfoList 동기화

        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellController>().UpdatePlayerSpellInfoArrayClientRPC(spellInfoListOnServer[clientId].ToArray());
    }

    /// <summary>
    /// Server측에서 보유한 SpellInfo 리스트 초기화 메소드 입니다.
    /// 플레이어 최초 생성시 호출됩니다.
    /// 업데이트가 끝나면 클라이언트측과 SpellInfo 정보를 동기화합니다.
    /// </summary>
    public void InitPlayerSpellInfoArrayOnServer(ulong clientId, SpellName[] spellNames)
    {
        List<SpellInfo> playerSpellInfoList = new List<SpellInfo>();
        foreach (SpellName spellName in spellNames)
        {
            Debug.Log($"spellName: {spellName}");
            SpellInfo spellInfo = SpellSpecifications.Instance.GetSpellDefaultSpec(spellName);
            // 마법 실명제. 이 마법에 누군가 당한 경우 알림 띄울 때 사용
            spellInfo.ownerPlayerClientId = clientId;
            playerSpellInfoList.Add(spellInfo);
        }

        if (!spellInfoListOnServer.ContainsKey(clientId))
        {
            spellInfoListOnServer.Add(clientId, playerSpellInfoList);
        }
        else
        {
            spellInfoListOnServer[clientId] = playerSpellInfoList;
        }

        // 요청한 클라이언트의 currentSpellInfoList 동기화
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellController>().UpdatePlayerSpellInfoArrayClientRPC(playerSpellInfoList.ToArray());       
    }

    /// <summary>
    /// 특정 스펠의 스펠인포를 업데이트해주는 메소드 입니다.
    /// 주로 스크롤 획득으로 인한 스펠 강화에 사용됩니다.
    /// 업데이트 후에 자동으로 클라이언트측과 동기화를 합니다.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdateScrollEffectServerRPC(Item.ItemName scrollName, sbyte spellIndex, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        if (!SpellManager.Instance.spellInfoListOnServer.ContainsKey(clientId))
        {
            Debug.LogError($"UpdateScrollEffectServerRPC. There is no SpellInfoList for this client. clientId:{clientId}");
            return;
        }

        SpellInfo newSpellInfo = spellInfoListOnServer[clientId][spellIndex];

        // 기본 스펠의 defautl info값에 scrollName별로 다른 값을 추가해서 아래 UpdatePlayerSpellInfo에 넘겨줍니다.
        switch (scrollName)
        {           
            case Item.ItemName.Scroll_LevelUp:
                newSpellInfo.level += 1;
                break;
            case Item.ItemName.Scroll_FireRateUp:
                if(newSpellInfo.coolTime > 0.2f) newSpellInfo.coolTime -= 0.2f;
                else newSpellInfo.coolTime = 0f;
                break;
            case Item.ItemName.Scroll_FlySpeedUp:
                newSpellInfo.moveSpeed += 1f;
                break;
            case Item.ItemName.Scroll_Attach:
                newSpellInfo.moveSpeed = 0f;
                break;
            case Item.ItemName.Scroll_Guide:
                // 유도 마법 미구현
                break;
            default:
                Debug.Log("UpdateScrollEffectServerRPC. 스크롤 이름을 찾을 수 없습니다.");
                break;
        }

        // 변경내용 서버에 저장
        spellInfoListOnServer[clientId][spellIndex] = newSpellInfo;

        // 변경내용을 요청한 클라이언트와도 동기화
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellController>().UpdatePlayerSpellInfoArrayClientRPC(spellInfoListOnServer[clientId].ToArray());
    }

    /// <summary>
    /// 특정 client가 현재 보유중인 특정 마법의 정보를 알려주는 메소드 입니다.  
    /// </summary>
    /// <param name="clientId">알고싶은 Client의 ID</param>
    /// <param name="spellName">알고싶은 마법의 이름</param>
    /// <returns></returns>
    public SpellInfo GetSpellInfo(ulong clientId, SpellName spellName)
    {
        if (!spellInfoListOnServer.ContainsKey(clientId))
        {
            Debug.Log($"GetSpellInfo. 스펠정보를 찾을 수 없습니다. clienId:{clientId}, spellName:{spellName}");
            return null;
        }

        foreach (SpellInfo spellInfo in spellInfoListOnServer[clientId])
        {
            if(spellInfo.spellName == spellName)
                return spellInfo;
        }

        return null;
    }
    #endregion

    #region Spell Cast&Fire
    /// <summary>
    /// 마법 생성해주기. 캐스팅 시작 ( NetworkObject는 Server에서만 생성 가능합니다 )
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void StartCastingSpellServerRPC(SpellName spellName, NetworkObjectReference player)
    {
        // GameObject 얻어내기 실패시 로그 출력
        if (!player.TryGet(out NetworkObject playerObject))
        {
            Debug.Log("StartCastingSpellServerRPC Failed to Get NetworkObject from NetwrokObjectRefernce!");
        }
        ulong clientId = playerObject.OwnerClientId;

        // 포구 위치 찾기(Local posittion)
        Transform muzzleTransform = playerObject.GetComponentInChildren<MuzzlePos>().transform;
        playerMuzzlePairs[clientId] = muzzleTransform;
 
        // 포구에 발사체 위치시키기
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellName), muzzleTransform.position, Quaternion.identity);
        spellObject.GetComponent<Spell>().InitSpellInfoDetail(GetSpellInfo(clientId, spellName));
        spellObject.GetComponent<NetworkObject>().Spawn();
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
    public void ShootSpellObjectServerRPC(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        GameObject spellObject = playerCastingSpellPairs[clientId];
        if (spellObject == null)
        {
            Debug.Log($"ShootSpellObjectServerRPC : Wrong Request. Player{clientId} has no casting spell object.");
            return;
        }

        // 마법 발사
        spellObject.transform.SetParent(this.transform);
        float moveSpeed = GetSpellInfo(clientId, spellObject.GetComponent<Spell>().GetSpellInfo().spellName).moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // 포구 VFX
        MuzzleVFX(spellObject.GetComponent<Spell>().GetMuzzleVFXPrefab(), playerMuzzlePairs[clientId]);
    }
    #endregion

    #region Spell Hit 
    /// <summary>
    /// 2. CollisionEnter 충돌 처리 (서버 권한 방식)
    /// </summary>
    /// <param name="collision"></param>
    public virtual void SpellHitOnServer(Collision collision, Spell spell)
    {
        if (spell.IsCollided())
        {
            Debug.Log("이미 동일한 마법 충돌을 처리중입니다.");
            return;
        }

        // 충돌한것이 마법인지 확인
        bool isSpellCollided = false;   
        // 충돌 처리결과 저장
        SpellInfo collisionHandlingResult = null;

        // 일단 마법 정지
        spell.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        Debug.Log($"collision.gameObject.tag: {collision.gameObject.tag}");

        // 충돌한게 Spell일 경우, 스펠간 충돌 결과 처리를 따로 진행합니다
        // 처리결과를 collisionHandlingResult 변수에 저장해뒀다가 DestroyParticle시에 castSpell 시킵니다.
        if (collision.gameObject.tag == "Spell")
        {       
            isSpellCollided = true;
            Debug.Log($"Spell Hit!");
            SpellInfo thisSpell = GetSpellInfo(spell.GetSpellInfo().ownerPlayerClientId, spell.GetSpellInfo().spellName);
            SpellInfo opponentsSpell = GetSpellInfo(collision.gameObject.GetComponent<Spell>().GetSpellInfo().ownerPlayerClientId, collision.gameObject.GetComponent<Spell>().GetSpellInfo().spellName);

            collisionHandlingResult = spell.CollisionHandling(thisSpell, opponentsSpell);
        }

        // 충돌한게 플레이어일 경우! 
        // 1. player에게 GetHit() 시킴
        // 2. player가 GameMultiplayer(ServerRPC)에게 보고, player(ClientRPC)업데이트.
        else if (collision.gameObject.tag == "Player")
        {
            isSpellCollided = false;
            Debug.Log($"Player Hit!");

            if (spell.GetSpellInfo() == null)
            {
                Debug.Log("Spell Info is null");
            }
            //Debug.Log($"Hit!! spell level: {spellInfo.level}");

            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                sbyte damage = (sbyte) GetSpellInfo(spell.GetSpellInfo().ownerPlayerClientId, spell.GetSpellInfo().spellName).level;
                //Debug.Log("Spell.GotHit()");
                PlayerGotHitOnServer(damage, player.GetComponent<NetworkObject>().OwnerClientId);
            }
            else Debug.LogError("Player is null!");
        }

        // 기타 오브젝트 충돌
        else
        {
            isSpellCollided = false;
            Debug.Log($"Object Hit!");
        }

        spell.SetSpellIsCollided(true);

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

        Debug.Log($"Collided Obejct name: {collision.gameObject.name}");

        // 스펠끼리 충돌해서 우리 스펠이 이겼을 때
        if (isSpellCollided)
        {
            Debug.Log($"our spell({spell.GetSpellInfo().spellName}) is win! collisionHandlingResult.level :{collisionHandlingResult.level} ");
            if (collisionHandlingResult.level > 0)
            {             
                SpawnSpellObjectOnServer(collisionHandlingResult.ownerPlayerClientId, spell.transform, collisionHandlingResult.spellName);
            }
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
    public void SpawnSpellObjectOnServer(ulong spellOwnerClientId, Transform spawnPosition, SpellName spellName)
    {
        // 포구에 마법 발사체 위치시키기
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellName), spawnPosition.position, Quaternion.identity);
        spellObject.GetComponent<Spell>().InitSpellInfoDetail(GetSpellInfo(spellOwnerClientId, spellName));
        spellObject.GetComponent<NetworkObject>().Spawn();

        // 마법 발사체 방향 조정하기
        spellObject.transform.forward = spawnPosition.forward;

        // 마법 발사
        spellObject.transform.SetParent(this.transform);
        float moveSpeed = GetSpellInfo(spellOwnerClientId, spellName).moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // 포구 VFX
        MuzzleVFX(spellObject.GetComponent<Spell>().GetMuzzleVFXPrefab(), spawnPosition);
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
    public void PlayerGotHitOnServer(sbyte damage, ulong clientId)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            // 요청한 플레이어 현재 HP값 가져오기 
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
            sbyte playerHP = playerData.playerHP;

            // HP보다 Damage가 클 경우(게임오버 처리는 Player에서 HP잔량 파악해서 알아서 한다.)
            if (playerHP <= damage)
            {
                // HP 0
                playerHP = 0;
            }
            else
            {
                // HP 감소 계산
                playerHP -= damage;
            }

            // 각 Client에 변경HP 전달
            PlayerHPManager.Instance.UpdatePlayerHP(clientId, playerHP);
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
        muzzleVFX.transform.SetParent(muzzleTransform.GetComponentInParent<Player>().transform);
        muzzleVFX.transform.localPosition = muzzleTransform.localPosition;
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
            Debug.Log($"hitVFXPrefab is Not null");
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
