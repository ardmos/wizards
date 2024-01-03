using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Spell;
using static UnityEngine.ParticleSystem;
/// <summary>
/// 마법을 Server Auth 방식으로 시전할 수 있도록 도와주는 스크립트 입니다.
/// </summary>
public class SpellManager : NetworkBehaviour
{
    public static SpellManager Instance;

    // Spell Dictionary that the player is casting.
    private Dictionary<ulong, GameObject> playerSpellPairs = new Dictionary<ulong, GameObject>();
    private Dictionary<ulong, Transform> playerMuzzlePairs = new Dictionary<ulong, Transform>();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 마법 생성해주기. 캐스팅 시작 ( NetworkObject는 Server에서만 생성 가능합니다 )
    /// </summary>
    public void SpawnSpellObject(SpellInfo spellInfo, NetworkObjectReference player)
    {
        SpawnSpellObjectServerRPC(spellInfo, player);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnSpellObjectServerRPC(SpellInfo spellInfo, NetworkObjectReference player)
    {
        // GameObject 얻어내기 실패시 로그 출력
        if(!player.TryGet(out NetworkObject playerObject))
        {
            Debug.Log("SpawnSpellObjectServerRPC Failed to Get NetworkObject from NetwrokObjectRefernce!");
        }
        // 포구 위치 찾기(Local posittion)
        Transform muzzleTransform = playerObject.GetComponentInChildren<MuzzlePos>().transform;
        playerMuzzlePairs[playerObject.OwnerClientId] = muzzleTransform;
        //Debug.Log($"SpawnSpell muzzlePos:{muzzleTransform.position}, muzzleLocalPos:{muzzleTransform.localPosition}");
        // 포구에 발사체 위치시키기
        GameObject spellObject = Instantiate(GetSpellObject(spellInfo), muzzleTransform.position, Quaternion.identity);
        //GameObject spellObject = Instantiate(GetSpellObject(spellInfo));
        spellObject.GetComponent<Spell>().InitSpellInfoDetail();
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.transform.SetParent(playerObject.transform);
        spellObject.transform.localPosition = muzzleTransform.localPosition;
        // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기
        spellObject.transform.forward = playerObject.transform.forward;

        // 플레이어가 시전중인 마법 목록에 저장하기
        if (playerSpellPairs.ContainsKey(playerObject.OwnerClientId)) playerSpellPairs[playerObject.OwnerClientId] = spellObject;
        else playerSpellPairs.Add(playerObject.OwnerClientId, spellObject);



        ///////////여기부터!!! 
        /// 1. 소환하는데 위치잡는게 느려서 보임   해결
        /// 2. muzzle vfx에서 사용할 포지션이 업데이트가 안됨   해결
        /// 3. 스킬시전중인데 조이스틱으로 방향전환이 됨
        /// 4. 발사~탄착 사이에 스킬버튼 클릭시 muzzle vfx가 여러번 발동됨.      해결
        /// 5. 발사~탄착 사이에 플레이어가 회전하면 비행중인 스킬도 같이 회전함.    해결
        /// 6. 스킬발동 마우스 뗄때로들어가도 쿨타임 도는 문제 (발사는 안됨). 그냥 뗄때만 입력 들어오면 무시하게하면 됨     해결
    }

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
        muzzleVFX.transform.forward = gameObject.transform.forward;
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
    /// <summary>
    /// 마법 발사하기
    /// </summary>
    public void ShootSpellObject()
    {
        ShootSpellObjectServerRPC();
    }
    [ServerRpc (RequireOwnership = false)]
    public void ShootSpellObjectServerRPC(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        GameObject spellObject = playerSpellPairs[clientId];
        if (spellObject == null) { 
            Debug.Log($"ShootSpellObjectServerRPC : Wrong Request. Player{clientId} has no casting spell object.");
            return;
        }

        // 마법 발사
        spellObject.transform.SetParent(this.transform);
        float moveSpeed = spellObject.GetComponent<Spell>().spellInfo.moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // 포구 VFX
        MuzzleVFX(spellObject.GetComponent<Spell>().GetMuzzleVFXPrefab(), playerMuzzlePairs[clientId]);
    }


    // 마법 프리팹 검색 및 반환
    private GameObject GetSpellObject(SpellInfo spellInfo)
    {
        return GameAssets.instantiate.GetSpellPrefab(spellInfo.spellName);
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
    public void PlayerGotHit(sbyte damage, ulong clientId)
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
            Debug.Log($"GameMultiplayer.PlayerGotHit()  Player{clientId} got {damage} damage new HP:{playerHP}");
        }
    }
}
