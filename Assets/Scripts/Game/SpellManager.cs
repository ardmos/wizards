using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Spell;
using static UnityEngine.ParticleSystem;
/// <summary>
/// ������ Server Auth ������� ������ �� �ֵ��� �����ִ� ��ũ��Ʈ �Դϴ�.
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
    /// ���� �������ֱ�. ĳ���� ���� ( NetworkObject�� Server������ ���� �����մϴ� )
    /// </summary>
    public void SpawnSpellObject(SpellInfo spellInfo, NetworkObjectReference player)
    {
        SpawnSpellObjectServerRPC(spellInfo, player);
    }
    [ServerRpc(RequireOwnership = false)]
    private void SpawnSpellObjectServerRPC(SpellInfo spellInfo, NetworkObjectReference player)
    {
        // GameObject ���� ���н� �α� ���
        if(!player.TryGet(out NetworkObject playerObject))
        {
            Debug.Log("SpawnSpellObjectServerRPC Failed to Get NetworkObject from NetwrokObjectRefernce!");
        }
        // ���� ��ġ ã��(Local posittion)
        Transform muzzleTransform = playerObject.GetComponentInChildren<MuzzlePos>().transform;
        playerMuzzlePairs[playerObject.OwnerClientId] = muzzleTransform;
        //Debug.Log($"SpawnSpell muzzlePos:{muzzleTransform.position}, muzzleLocalPos:{muzzleTransform.localPosition}");
        // ������ �߻�ü ��ġ��Ű��
        GameObject spellObject = Instantiate(GetSpellObject(spellInfo), muzzleTransform.position, Quaternion.identity);
        //GameObject spellObject = Instantiate(GetSpellObject(spellInfo));
        spellObject.GetComponent<Spell>().InitSpellInfoDetail();
        spellObject.GetComponent<NetworkObject>().Spawn();
        spellObject.transform.SetParent(playerObject.transform);
        spellObject.transform.localPosition = muzzleTransform.localPosition;
        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��
        spellObject.transform.forward = playerObject.transform.forward;

        // �÷��̾ �������� ���� ��Ͽ� �����ϱ�
        if (playerSpellPairs.ContainsKey(playerObject.OwnerClientId)) playerSpellPairs[playerObject.OwnerClientId] = spellObject;
        else playerSpellPairs.Add(playerObject.OwnerClientId, spellObject);



        ///////////�������!!! 
        /// 1. ��ȯ�ϴµ� ��ġ��°� ������ ����   �ذ�
        /// 2. muzzle vfx���� ����� �������� ������Ʈ�� �ȵ�   �ذ�
        /// 3. ��ų�������ε� ���̽�ƽ���� ������ȯ�� ��
        /// 4. �߻�~ź�� ���̿� ��ų��ư Ŭ���� muzzle vfx�� ������ �ߵ���.      �ذ�
        /// 5. �߻�~ź�� ���̿� �÷��̾ ȸ���ϸ� �������� ��ų�� ���� ȸ����.    �ذ�
        /// 6. ��ų�ߵ� ���콺 �����ε��� ��Ÿ�� ���� ���� (�߻�� �ȵ�). �׳� ������ �Է� ������ �����ϰ��ϸ� ��     �ذ�
    }

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
        muzzleVFX.transform.forward = gameObject.transform.forward;
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
    /// <summary>
    /// ���� �߻��ϱ�
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

        // ���� �߻�
        spellObject.transform.SetParent(this.transform);
        float moveSpeed = spellObject.GetComponent<Spell>().spellInfo.moveSpeed;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * moveSpeed, ForceMode.Impulse);

        // ���� VFX
        MuzzleVFX(spellObject.GetComponent<Spell>().GetMuzzleVFXPrefab(), playerMuzzlePairs[clientId]);
    }


    // ���� ������ �˻� �� ��ȯ
    private GameObject GetSpellObject(SpellInfo spellInfo)
    {
        return GameAssets.instantiate.GetSpellPrefab(spellInfo.spellName);
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
    public void PlayerGotHit(sbyte damage, ulong clientId)
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
            Debug.Log($"GameMultiplayer.PlayerGotHit()  Player{clientId} got {damage} damage new HP:{playerHP}");
        }
    }
}
