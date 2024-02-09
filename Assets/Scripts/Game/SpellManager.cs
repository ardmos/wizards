using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// ������ Server Auth ������� ������ �� �ֵ��� �����ִ� ��ũ��Ʈ �Դϴ�.
/// �������� �����ϴ� ��ũ��Ʈ���� ���־�� �մϴ�. ���� �ڵ� �����ϸ鼭 Ȯ�� �ʿ�.
/// </summary>
public class SpellManager : NetworkBehaviour
{
    public static SpellManager Instance;

    // Spell Dictionary that the player is casting.
    private Dictionary<ulong, GameObject> playerCastingSpellPairs = new Dictionary<ulong, GameObject>();
    private Dictionary<ulong, Transform> playerMuzzlePairs = new Dictionary<ulong, Transform>();

    // ������ ����Ǵ� ����
    [SerializeField] private Dictionary<ulong, List<SpellInfo>> spellInfoListOnServer;

    private void Awake()
    {      
        Instance = this;
        spellInfoListOnServer = new Dictionary<ulong, List<SpellInfo>>();
    }

    #region SpellInfo
    /// <summary>
    /// ����� SpellState�� Server�� �����մϴ�.
    /// </summary>
    /// <param name="spellIndex">������Ʈ�ϰ���� ������ Index</param>
    /// <param name="spellState">������ ����</param>
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

        // ��û�� Ŭ���̾�Ʈ�� currentSpellInfoList ����ȭ
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellController>().UpdatePlayerSpellInfoArrayClientRPC(spellInfoListOnServer[clientId].ToArray());
    }

    /// <summary>
    /// Server������ ������ SpellInfo ����Ʈ �ʱ�ȭ �޼ҵ� �Դϴ�.
    /// �÷��̾� ���� ������ ȣ��˴ϴ�.
    /// ������Ʈ�� ������ Ŭ���̾�Ʈ���� SpellInfo ������ ����ȭ�մϴ�.
    /// </summary>
    public void InitPlayerSpellInfoArrayOnServer(ulong clientId, SpellName[] spellNames)
    {
        List<SpellInfo> playerSpellInfoList = new List<SpellInfo>();
        foreach (SpellName spellName in spellNames)
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

        // ��û�� Ŭ���̾�Ʈ�� currentSpellInfoList ����ȭ
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellController>().UpdatePlayerSpellInfoArrayClientRPC(playerSpellInfoList.ToArray());       
    }

    /// <summary>
    /// Ư�� ������ ���������� ������Ʈ���ִ� �޼ҵ� �Դϴ�.  <----�޼ҵ�� �����ϴ°� ����غ���
    /// �ַ� ��ũ�� ȹ������ ���� ���� ��ȭ�� ���˴ϴ�.
    /// ������Ʈ �Ŀ� �ڵ����� Ŭ���̾�Ʈ���� ����ȭ�� �մϴ�.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void UpdateScrollEffectServerRPC(ItemName scrollName, sbyte spellIndex, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        if (!SpellManager.Instance.spellInfoListOnServer.ContainsKey(clientId))
        {
            Debug.LogError($"UpdateScrollEffectServerRPC. There is no SpellInfoList for this client. clientId:{clientId}");
            return;
        }

        SpellInfo newSpellInfo = spellInfoListOnServer[clientId][spellIndex];

        // �⺻ ������ defautl info���� scrollName���� �ٸ� ���� �߰��ؼ� �Ʒ� UpdatePlayerSpellInfo�� �Ѱ��ݴϴ�.
        switch (scrollName)
        {           
            case ItemName.Scroll_LevelUp:
                newSpellInfo.level += 1;
                break;
            case ItemName.Scroll_FireRateUp:
                if(newSpellInfo.coolTime > 0.2f) newSpellInfo.coolTime -= 0.2f;
                else newSpellInfo.coolTime = 0f;
                break;
            case ItemName.Scroll_FlySpeedUp:
                newSpellInfo.moveSpeed += 1f;
                break;
            case ItemName.Scroll_Attach:
                newSpellInfo.moveSpeed = 0f;
                break;
            case ItemName.Scroll_Guide:
                // ���� ���� �̱���
                break;
            default:
                Debug.Log("UpdateScrollEffectServerRPC. ��ũ�� �̸��� ã�� �� �����ϴ�.");
                break;
        }

        // ���泻�� ������ ����
        spellInfoListOnServer[clientId][spellIndex] = newSpellInfo;

        // ���泻���� ��û�� Ŭ���̾�Ʈ�͵� ����ȭ
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellController>().UpdatePlayerSpellInfoArrayClientRPC(spellInfoListOnServer[clientId].ToArray());
    }

    /// <summary>
    /// Ư�� client�� ���� �������� Ư�� ������ ������ �˷��ִ� �޼ҵ� �Դϴ�.  
    /// </summary>
    /// <param name="clientId">�˰���� Client�� ID</param>
    /// <param name="spellName">�˰���� ������ �̸�</param>
    /// <returns></returns>
    public SpellInfo GetSpellInfo(ulong clientId, SpellName spellName)
    {
        if (!spellInfoListOnServer.ContainsKey(clientId))
        {
            Debug.Log($"GetSpellInfo. ���������� ã�� �� �����ϴ�. clienId:{clientId}, spellName:{spellName}");
            return null;
        }

        foreach (SpellInfo spellInfo in spellInfoListOnServer[clientId])
        {
            if(spellInfo.spellName == spellName)
            {
                return spellInfo;
            }
                
        }

        return null;
    }
    #endregion

    #region Defence Spell Cast
    /// <summary>
    /// ��� ���� ����
    /// </summary>
    /// <param name="player"></param>
    [ServerRpc(RequireOwnership = false)]
    public void StartActivateDefenceSpellServerRPC(SpellName spellName, NetworkObjectReference player)
    {
        // GameObject ���� ���н� �α� ���
        if (!player.TryGet(out NetworkObject playerObject))
        {
            Debug.LogError("StartActivateDefenceSpellServerRPC Failed to Get NetworkObject from NetwrokObjectRefernce!");
            return;
        }
        ulong clientId = playerObject.OwnerClientId;

        // ���� ����
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
    /// ���� ���� �������ֱ�. ĳ���� ���� ( NetworkObject�� Server������ ���� �����մϴ� )
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void StartCastingAttackSpellServerRPC(SpellName spellName, NetworkObjectReference player)
    {
        // GameObject ���� ���н� �α� ���
        if (!player.TryGet(out NetworkObject playerObject))
        {
            Debug.LogError("StartCastingAttackSpellServerRPC Failed to Get NetworkObject from NetwrokObjectRefernce!");
            return;
        }
        ulong clientId = playerObject.OwnerClientId;

        // ���� ��ġ ã��(Local posittion)
        Transform muzzleTransform = playerObject.GetComponentInChildren<MuzzlePos>().transform;
        playerMuzzlePairs[clientId] = muzzleTransform;
 
        // ������ �߻�ü ��ġ��Ű��
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellName), muzzleTransform.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();

        SpellInfo spellInfo = new SpellInfo(GetSpellInfo(clientId, spellName));
        Debug.Log($"{nameof(StartCastingAttackSpellServerRPC)} ownerClientId {spellInfo.ownerPlayerClientId}, spellName: {spellInfo.spellName}, spellLv: {spellInfo.level}");
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        spellObject.transform.SetParent(playerObject.transform);
        spellObject.transform.localPosition = muzzleTransform.localPosition;

        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��
        spellObject.transform.forward = playerObject.transform.forward;

        // �÷��̾ �������� ���� ��Ͽ� �����ϱ�
        if (playerCastingSpellPairs.ContainsKey(clientId)) playerCastingSpellPairs[clientId] = spellObject;
        else playerCastingSpellPairs.Add(clientId, spellObject);
    }   

    /// <summary>
    /// �÷��̾��� ��û���� ���� ĳ�������� ������ �߻��ϴ� �޼ҵ� �Դϴ�.
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

        // ���� �߻�
        spellObject.transform.SetParent(this.transform);
        float moveSpeed = spellObject.GetComponent<AttackSpell>().GetSpellInfo().moveSpeed;
        spellObject.GetComponent<AttackSpell>().Shoot(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // ���� VFX
        MuzzleVFX(spellObject.GetComponent<AttackSpell>().GetMuzzleVFXPrefab(), playerMuzzlePairs[clientId]);
    }
    #endregion

    #region Spell Hit 
    /// <summary>
    /// 2. CollisionEnter �浹 ó�� (���� ���� ���)
    /// Spell ������ ���忡�� �浹������ ������ �������� ��û�� �� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// </summary>
    /// <param name="collision"></param>
    public virtual void SpellHitOnServer(Collision collision, AttackSpell spell)
    {
        // �浹�Ѱ��� �������� Ȯ��
        bool isSpellCollided = false;   
        // �浹 ó����� ����
        SpellInfo collisionHandlingResult = null;

        // �ϴ� ���� ����
        spell.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        // �浹�� ������Ʈ�� collider ����
        Collider collider = collision.collider;

        // �浹�Ѱ� Spell�� ���, ���簣 �浹 ��� ó���� ���� �����մϴ�
        // ó������� collisionHandlingResult ������ �����ص״ٰ� DestroyParticle�ÿ� castSpell ��ŵ�ϴ�.
        if (collider.CompareTag("Attack"))
        {       
            isSpellCollided = true;            
            SpellInfo thisSpell = spell.GetSpellInfo();
            SpellInfo opponentsSpell = collider.GetComponent<AttackSpell>().GetSpellInfo();

            collisionHandlingResult = spell.CollisionHandling(thisSpell, opponentsSpell);

            Debug.Log($"Spell���� �浹! ourSpell: name{thisSpell.spellName}, lvl{thisSpell.level}, owner{thisSpell.ownerPlayerClientId}  // " +
                $"opponentsSpell : name{opponentsSpell.spellName}, lvl{opponentsSpell.level}, owner{opponentsSpell.ownerPlayerClientId}");
        }

        // �浹�Ѱ� �÷��̾��� ���! 
        // 1. player���� GetHit() ��Ŵ
        // 2. player�� GameMultiplayer(ServerRPC)���� ����, player(ClientRPC)������Ʈ.
        else if (collider.CompareTag("Player"))
        {
            isSpellCollided = false;

            if (spell.GetSpellInfo() == null)
            {
                Debug.Log("AttackSpell Info is null");
            }

            Player player = collider.GetComponent<Player>();
            if (player != null)
            {
                byte damage = (byte)spell.GetSpellInfo().level;                
                // �÷��̾� �ǰ��� �������� ó��
                PlayerGotHitOnServer(damage, player);
            }
            else Debug.LogError("Player is null!");
        }

        // ��Ÿ ������Ʈ �浹
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

        // ���� ȿ�� VFX
        SpellManager.Instance.HitVFX(spell.GetHitVFXPrefab(), collision);

        // ���糢�� �浹�ؼ� �츮 ������ �̰��� �� ��� ����� ���� �浹 ��ġ�� ���ο� ���� ����. 
        if (isSpellCollided && collisionHandlingResult.level > 0)
        {
            Debug.Log($"our spell is win! generate spell.name:{collisionHandlingResult.spellName}, spell.level :{collisionHandlingResult.level}, spell.owner: {collisionHandlingResult.ownerPlayerClientId} ");
            SpawnSpellObjectOnServer(collisionHandlingResult, spell.transform);
        }

        Destroy(spell.gameObject, 0.2f);
    }

    /// <summary>
    /// ���� �����ϱ�.
    /// Spell���� �浹 ���� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// �÷��̾ ĳ������ ������ �߻��ϴ� �޼ҵ�� �Ʒ� ServerRPC �޼ҵ� �Դϴ�.
    /// </summary>
    /// �ʿ��Ѱ�.
    ///     1. ���� Transform
    ///     2. �� �߻�ü SpellName
    public void SpawnSpellObjectOnServer(SpellInfo spellInfo, Transform spawnPosition)
    {
        // ������ ���� �߻�ü ��ġ��Ű��
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellInfo.spellName), spawnPosition.position, Quaternion.identity);
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.GetComponent<AttackSpell>().InitSpellInfoDetail(spellInfo);
        Debug.Log($"SpawnSpellObjectOnServer!! spellInfo.ownerClientId : {spellInfo.ownerPlayerClientId}, name:{spellInfo.spellName}, lvl:{spellInfo.level}");

        // ���� �߻�ü ���� �����ϱ�
        spellObject.transform.forward = spawnPosition.forward;

        // ���� �߻�
        spellObject.transform.SetParent(this.transform);
        float moveSpeed = spellInfo.moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // ���� VFX
        MuzzleVFX(spellObject.GetComponent<AttackSpell>().GetMuzzleVFXPrefab(), spawnPosition);
    }

    /// <summary>
    /// ��ų �浹 ó��(�������� ����)
    /// �÷��̾� ���߽� ( �ٸ� �����̳� ���������� �浹 ó���� Spell.cs�� �ִ�. �ڵ� ���� �ʿ�)
    /// clientID�� HP �����ؼ� ó��. 
    /// �浹 �༮�� �÷��̾��� ��� ����. 
    /// ClientID�� ����Ʈ �˻� �� HP ������Ű�� ������Ʈ�� ���� ��ε�ĳ����.
    /// �������� ClientID�� �÷��̾� HP ������Ʈ. 
    /// �������� �����Ǵ� ��ũ��Ʈ.
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="clientId"></param>
    public void PlayerGotHitOnServer(byte damage, Player player)
    {
        ulong clientId = player.OwnerClientId;
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            // ��û�� �÷��̾� ���� HP�� �������� 
            PlayerInGameData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
            sbyte playerHP = playerData.playerHP;
            sbyte playerMaxHP = playerData.playerMaxHP;

            // HP���� Damage�� Ŭ ���(���ӿ��� ó���� Player���� HP�ܷ� �ľ��ؼ� �˾Ƽ� �Ѵ�.)
            if (playerHP <= damage)
            {
                // HP 0
                playerHP = 0;
            }
            else
            {
                // HP ���� ���
                playerHP -= (sbyte)damage;
            }

            // �� Client UI ������Ʈ ����. HPBar & Damage Popup
            PlayerHPManager.Instance.UpdatePlayerHP(clientId, playerHP, playerMaxHP);
            player.ShowDamagePopupClientRPC(damage);

            Debug.Log($"GameMultiplayer.PlayerGotHitOnServer()  Player{clientId} got {damage} damage new HP:{playerHP}");
        }
    }
    #endregion

    #region Spell VFX
    // ���� VFX 
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

    // ����ȿ�� VFX
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
