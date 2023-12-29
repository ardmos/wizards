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
    }

    // ���� ������ �˻� �� ��ȯ
    private GameObject GetSpellObject(SpellInfo spellInfo)
    {
        GameObject resultObject = null;
        GameAssets gameAssets = GameAssets.instantiate;

        switch (spellInfo.level)
        {
            case 1:
                switch (spellInfo.spellType)
                {
                    case SpellType.Fire:
                        resultObject = gameAssets.fireBall_1;
                        break;
                    case SpellType.Water:
                        resultObject = gameAssets.waterBall_1;
                        break;
                    case SpellType.Ice:
                        resultObject = gameAssets.iceBall_1;
                        break;
                    case SpellType.Lightning:
                        break;
                    case SpellType.Arcane:
                        break;
                    case SpellType.Normal:
                        // �׽�Ʈ��.  ������ Type���� �˻��ϴ°� ��� ��������or�̸����� �˻��ؾ���.
                        resultObject = gameAssets.fireBall_1;
                        break;
                    default:
                        break;
                }
                break;
            case 2:
                switch (spellInfo.spellType)
                {
                    case SpellType.Fire:
                        break;
                    case SpellType.Water:
                        break;
                    case SpellType.Ice:
                        break;
                    case SpellType.Lightning:
                        break;
                    case SpellType.Arcane:
                        break;
                    default:
                        break;
                }
                break;
            case 3:
                switch (spellInfo.spellType)
                {
                    case SpellType.Fire:
                        break;
                    case SpellType.Water:
                        break;
                    case SpellType.Ice:
                        break;
                    case SpellType.Lightning:
                        break;
                    case SpellType.Arcane:
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

        return resultObject;
    }

}
