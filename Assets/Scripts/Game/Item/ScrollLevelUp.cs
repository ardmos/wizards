using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
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
}
