using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 마법을 Server Auth 방식으로 시전할 수 있도록 도와주는 스크립트 입니다.
/// Scroll의 획득과 적용의 과정도 관리합니다.
/// 서버에서 동작하는 스크립트들이 모여있어야 합니다. 추후 코드 정리하면서 확인 필요.
/// </summary>
public class SpellManagerServerWizard : NetworkBehaviour
{
    public SpellManagerClientWizard spellManagerClientWizard;
    public List<SpellInfo> playerOwnedSpellInfoListOnServer = new List<SpellInfo>();

    // Spell Dictionary that the player is casting.
    private GameObject playerCastingSpell;

    #region SpellInfo
    /// <summary>
    /// 변경된 SpellState를 Server에 보고합니다.
    /// </summary>
    /// <param name="spellIndex">업데이트하고싶은 마법의 Index</param>
    /// <param name="spellState">마법의 상태</param>
    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {

        playerOwnedSpellInfoListOnServer[spellIndex].spellState = spellState;

        // 요청한 클라이언트의 playerOwnedSpellInfoList와 동기화
        spellManagerClientWizard.UpdatePlayerSpellInfoArrayClientRPC(playerOwnedSpellInfoListOnServer.ToArray());
    }

    /// <summary>
    /// Server측에서 보유한 SpellInfo 리스트 초기화 메소드 입니다.
    /// 플레이어 최초 생성시 호출됩니다.
    /// </summary>
    public void InitPlayerSpellInfoArrayOnServer(SkillName[] skillNames)
    {
        List<SpellInfo> playerSpellInfoList = new List<SpellInfo>();
        foreach (SkillName spellName in skillNames)
        {
            SpellInfo spellInfo = new SpellInfo(SpellSpecifications.Instance.GetSpellDefaultSpec(spellName));
            spellInfo.ownerPlayerClientId = OwnerClientId;
            playerSpellInfoList.Add(spellInfo);
        }

        playerOwnedSpellInfoListOnServer = playerSpellInfoList;
    }

    /// <summary>
    /// 현재 client가 보유중인 특정 마법의 정보를 알려주는 메소드 입니다.  
    /// </summary>
    /// <param name="spellName">알고싶은 마법의 이름</param>
    /// <returns></returns>
    public SpellInfo GetSpellInfo(SkillName spellName)
    {
        foreach (SpellInfo spellInfo in playerOwnedSpellInfoListOnServer)
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
        spellObject.GetComponent<DefenceSpell>().InitSpellInfoDetail(GetSpellInfo(spellName));
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
        // 포구 위치 찾기(Local posittion)
        Transform muzzleTransform = GetComponentInChildren<MuzzlePos>().transform;

        // 포구에 발사체 위치시키기
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellName), muzzleTransform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        // 발사체 스펙 초기화 해주기
        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(spellName));
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        spellObject.transform.SetParent(transform);
        spellObject.transform.localPosition = muzzleTransform.localPosition;

        // 플레이어가 보고있는 방향과 발사체가 바라보는 방향 일치시키기
        spellObject.transform.forward = transform.forward;

        // 플레이어가 시전중인 마법에 저장하기
        playerCastingSpell = spellObject;
    }

    /// <summary>
    /// 플레이어의 요청으로 현재 캐스팅중인 마법을 발사하는 메소드 입니다.
    /// </summary>
    /// <param name="serverRpcParams"></param>
    [ServerRpc(RequireOwnership = false)]
    public void ShootCastingSpellObjectServerRPC(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        GameObject spellObject = playerCastingSpell;
        if (spellObject == null)
        {
            Debug.Log($"ShootCastingSpellObjectServerRPC : Wrong Request. Player{clientId} has no casting spell object.");
            return;
        }

        Debug.Log($"{nameof(ShootCastingSpellObjectServerRPC)} ownerClientId {clientId}");

        spellObject.transform.SetParent(GameManager.Instance.transform);

        // 마법 발사 (기본 직선 비행 마법)
        float moveSpeed = spellObject.GetComponent<AttackSpell>().GetSpellInfo().moveSpeed;
        spellObject.GetComponent<AttackSpell>().Shoot(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // 포구 VFX
        MuzzleVFX(spellObject.GetComponent<AttackSpell>().GetMuzzleVFXPrefab(), GetComponentInChildren<MuzzlePos>().transform);
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
    #endregion
}
