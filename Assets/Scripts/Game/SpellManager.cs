using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.ParticleSystem;
/// <summary>
/// ������ Server Auth ������� ������ �� �ֵ��� �����ִ� ��ũ��Ʈ �Դϴ�.
/// �������� �����ϴ� ��ũ��Ʈ���� ���־�� �մϴ�. ���� �ڵ� �����ϸ鼭 Ȯ�� �ʿ�.
/// </summary>
public class SpellManager : NetworkBehaviour
{
    public static SpellManager Instance;

    public event EventHandler OnSpellStateChanged;


    // Spell Dictionary that the player is casting.
    private Dictionary<ulong, GameObject> playerCastingSpellPairs = new Dictionary<ulong, GameObject>();
    private Dictionary<ulong, Transform> playerMuzzlePairs = new Dictionary<ulong, Transform>();

    // ������ ����Ǵ� ����
    [SerializeField] private Dictionary<ulong, List<SpellInfo>> spellInfoListOnServer = new Dictionary<ulong, List<SpellInfo>>();


    private void Awake()
    {      
        Instance = this;
    }
    
    #region SpellInfo
    /// <summary>
    /// Spell state�� ���� �� �ִ� �޼ҵ� �Դϴ�
    /// </summary>
    /// <param name="spellIndex"></param>
    /// <returns></returns>
    public SpellState GetSpellStateFromSpellIndexOnServer(ulong clientId, ushort spellIndex)
    {
        return spellInfoListOnServer[clientId][spellIndex].spellState;
    }
    
    /// <summary>
    /// ����� SpellState�� Server�� �����մϴ�. ������� Server�� Server�� PlayerAnimator���� �ִϸ��̼� ������ �����մϴ�.
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
            Debug.Log($"spellName: {spellName}");
            SpellInfo spellInfo = SpellSpecifications.Instance.GetSpellDefaultSpec(spellName);
            // ���� �Ǹ���. �� ������ ������ ���� ��� �˸� ��� �� ���
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

        // ��û�� Ŭ���̾�Ʈ�� currentSpellInfoList ����ȭ
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellController>().UpdatePlayerSpellInfoArrayClientRPC(playerSpellInfoList.ToArray());       
    }

    /// <summary>
    /// Ư�� ������ ���������� ������Ʈ���ִ� �޼ҵ� �Դϴ�.
    /// �ַ� ��ũ�� ȹ������ ���� ���� ��ȭ�� ���˴ϴ�.
    /// ������Ʈ �Ŀ� �ڵ����� Ŭ���̾�Ʈ���� ����ȭ�� �մϴ�.
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

        // �⺻ ������ defautl info���� scrollName���� �ٸ� ���� �߰��ؼ� �Ʒ� UpdatePlayerSpellInfo�� �Ѱ��ݴϴ�.
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
                return spellInfo;
        }

        return null;
    }
    #endregion

    #region Spell Cast&Fire
    /// <summary>
    /// ���� �������ֱ�. ĳ���� ���� ( NetworkObject�� Server������ ���� �����մϴ� )
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void StartCastingSpellServerRPC(SpellName spellName, NetworkObjectReference player)
    {
        // GameObject ���� ���н� �α� ���
        if (!player.TryGet(out NetworkObject playerObject))
        {
            Debug.Log("StartCastingSpellServerRPC Failed to Get NetworkObject from NetwrokObjectRefernce!");
        }
        ulong clientId = playerObject.OwnerClientId;

        // ���� ��ġ ã��(Local posittion)
        Transform muzzleTransform = playerObject.GetComponentInChildren<MuzzlePos>().transform;
        playerMuzzlePairs[clientId] = muzzleTransform;
 
        // ������ �߻�ü ��ġ��Ű��
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellName), muzzleTransform.position, Quaternion.identity);
        spellObject.GetComponent<Spell>().InitSpellInfoDetail(GetSpellInfo(clientId, spellName));
        spellObject.GetComponent<NetworkObject>().Spawn();
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
    public void ShootSpellObjectServerRPC(ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        GameObject spellObject = playerCastingSpellPairs[clientId];
        if (spellObject == null)
        {
            Debug.Log($"ShootSpellObjectServerRPC : Wrong Request. Player{clientId} has no casting spell object.");
            return;
        }

        // ���� �߻�
        spellObject.transform.SetParent(this.transform);
        float moveSpeed = GetSpellInfo(clientId, spellObject.GetComponent<Spell>().GetSpellInfo().spellName).moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // ���� VFX
        MuzzleVFX(spellObject.GetComponent<Spell>().GetMuzzleVFXPrefab(), playerMuzzlePairs[clientId]);
    }
    #endregion

    #region Spell Hit 
    /// <summary>
    /// 2. CollisionEnter �浹 ó�� (���� ���� ���)
    /// </summary>
    /// <param name="collision"></param>
    public virtual void SpellHitOnServer(Collision collision, Spell spell)
    {
        if (spell.IsCollided())
        {
            Debug.Log("�̹� ������ ���� �浹�� ó�����Դϴ�.");
            return;
        }

        // �浹�Ѱ��� �������� Ȯ��
        bool isSpellCollided = false;   
        // �浹 ó����� ����
        SpellInfo collisionHandlingResult = null;

        // �ϴ� ���� ����
        spell.gameObject.GetComponent<Rigidbody>().isKinematic = true;

        Debug.Log($"collision.gameObject.tag: {collision.gameObject.tag}");

        // �浹�Ѱ� Spell�� ���, ���簣 �浹 ��� ó���� ���� �����մϴ�
        // ó������� collisionHandlingResult ������ �����ص״ٰ� DestroyParticle�ÿ� castSpell ��ŵ�ϴ�.
        if (collision.gameObject.tag == "Spell")
        {       
            isSpellCollided = true;
            Debug.Log($"Spell Hit!");
            SpellInfo thisSpell = GetSpellInfo(spell.GetSpellInfo().ownerPlayerClientId, spell.GetSpellInfo().spellName);
            SpellInfo opponentsSpell = GetSpellInfo(collision.gameObject.GetComponent<Spell>().GetSpellInfo().ownerPlayerClientId, collision.gameObject.GetComponent<Spell>().GetSpellInfo().spellName);

            collisionHandlingResult = spell.CollisionHandling(thisSpell, opponentsSpell);
        }

        // �浹�Ѱ� �÷��̾��� ���! 
        // 1. player���� GetHit() ��Ŵ
        // 2. player�� GameMultiplayer(ServerRPC)���� ����, player(ClientRPC)������Ʈ.
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

        // ��Ÿ ������Ʈ �浹
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

        // ���� ȿ�� VFX
        SpellManager.Instance.HitVFX(spell.GetHitVFXPrefab(), collision);

        Debug.Log($"Collided Obejct name: {collision.gameObject.name}");

        // ���糢�� �浹�ؼ� �츮 ������ �̰��� ��
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
    /// ���� �����ϱ�.
    /// Spell���� �浹 ���� ȣ��Ǵ� �޼ҵ� �Դϴ�.
    /// �÷��̾ ĳ������ ������ �߻��ϴ� �޼ҵ�� �Ʒ� ServerRPC �޼ҵ� �Դϴ�.
    /// </summary>
    /// �ʿ��Ѱ�.
    ///     1. ���� Transform
    ///     2. �� �߻�ü SpellName
    public void SpawnSpellObjectOnServer(ulong spellOwnerClientId, Transform spawnPosition, SpellName spellName)
    {
        // ������ ���� �߻�ü ��ġ��Ű��
        GameObject spellObject = Instantiate(GameAssets.instantiate.GetSpellPrefab(spellName), spawnPosition.position, Quaternion.identity);
        spellObject.GetComponent<Spell>().InitSpellInfoDetail(GetSpellInfo(spellOwnerClientId, spellName));
        spellObject.GetComponent<NetworkObject>().Spawn();

        // ���� �߻�ü ���� �����ϱ�
        spellObject.transform.forward = spawnPosition.forward;

        // ���� �߻�
        spellObject.transform.SetParent(this.transform);
        float moveSpeed = GetSpellInfo(spellOwnerClientId, spellName).moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // ���� VFX
        MuzzleVFX(spellObject.GetComponent<Spell>().GetMuzzleVFXPrefab(), spawnPosition);
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
    public void PlayerGotHitOnServer(sbyte damage, ulong clientId)
    {
        if (NetworkManager.ConnectedClients.ContainsKey(clientId))
        {
            // ��û�� �÷��̾� ���� HP�� �������� 
            PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(clientId);
            sbyte playerHP = playerData.playerHP;

            // HP���� Damage�� Ŭ ���(���ӿ��� ó���� Player���� HP�ܷ� �ľ��ؼ� �˾Ƽ� �Ѵ�.)
            if (playerHP <= damage)
            {
                // HP 0
                playerHP = 0;
            }
            else
            {
                // HP ���� ���
                playerHP -= damage;
            }

            // �� Client�� ����HP ����
            PlayerHPManager.Instance.UpdatePlayerHP(clientId, playerHP);
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

    // ����ȿ�� VFX
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
