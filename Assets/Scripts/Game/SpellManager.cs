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

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// ���� �������ֱ� ( NetworkObject�� Server������ ���� �����մϴ� )
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
        // ���� ��ġ ã��
        Vector3 muzzlePos = playerObject.GetComponentInChildren<MuzzlePos>().GetMuzzlePosition();
        // ������ �߻�ü ��ġ��Ű��
        GameObject spellObject = Instantiate(GetSpellObject(spellInfo), muzzlePos, Quaternion.identity);
        spellObject.GetComponent<Spell>().InitSpellInfoDetail();
        spellObject.GetComponent<NetworkObject>().Spawn();
        // �÷��̾ �����ִ� ����� �߻�ü�� �ٶ󺸴� ���� ��ġ��Ű��
        spellObject.transform.forward = playerObject.transform.forward;
        // ��ȯ�ÿ� Impulse�� �߻� ó��
        float speed = 35f;
        spellObject.GetComponent<Rigidbody>().AddForce(spellObject.transform.forward * speed, ForceMode.Impulse);

        // ���� VFX
        MuzzleVFX(spellObject.GetComponent<Spell>().GetMuzzleVFXPrefab(), muzzlePos);
    }

    // ���� VFX 
    public void MuzzleVFX(GameObject muzzleVFXPrefab, Vector3 muzzlePos)
    {
        if (muzzleVFXPrefab == null)
        {
            Debug.Log($"MuzzleVFX muzzleVFXPrefab is null");
            return;
        }

        GameObject muzzleVFX = Instantiate(muzzleVFXPrefab, muzzlePos, Quaternion.identity);
        muzzleVFX.GetComponent<NetworkObject>().Spawn();
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
