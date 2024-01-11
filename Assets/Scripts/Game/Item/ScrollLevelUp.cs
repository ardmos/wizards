using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// ������ �÷��̾� Level Up ��ų ��ų ����â ����ִ� ������. �������� �����մϴ�.
/// </summary>
public class ScrollLevelUp : Scroll
{
    private void OnCollisionEnter(Collision collision)
    {
        if(!IsServer) return;

        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;

        collision.gameObject.GetComponent<Player>().ShowSelectSpellPopupClientRPC(this);
    }

    public override void ApplyScroll(SpellInfo spellInfo)
    {
        SpellSpecifications.Instance.SetSpellSpec(
            spellInfo.spellType,
            spellInfo.coolTime,
            spellInfo.lifeTime,
            spellInfo.moveSpeed,
            spellInfo.price,
            spellInfo.level+1,
            spellInfo.spellName,    // SpellName �ϴ� ���� ����. ���� ���� �ʿ��ϸ� ���⼭ ����.
            spellInfo.spellState
            );
    }
   
}
