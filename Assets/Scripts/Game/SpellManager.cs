using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static Spell;
/// <summary>
/// ������ Server Auth ������� ������ �� �ֵ��� �����ִ� ��ũ��Ʈ �Դϴ�.
/// </summary>
public class SpellManager : NetworkBehaviour
{
    public static SpellManager instance;

    private void Awake()
    {
        instance = this;
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
    public virtual void MuzzleVFX(GameObject muzzleVFXPrefab, Vector3 muzzlePos)
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

    // ���� ������ �˻� �� ��ȯ
    private GameObject GetSpellObject(SpellInfo spellInfo)
    {
        return GameAssets.instantiate.GetSpellPrefab(spellInfo.spellName);
    }
}
