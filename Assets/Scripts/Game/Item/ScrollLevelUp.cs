using Unity.Netcode;
using UnityEngine;

public class ScrollLevelUp : Scroll
{
    public override void OnNetworkSpawn()
    {
        scrollName = Item.ItemName.Scroll_LevelUp;
    }

    /// <summary>
    /// �浹�� �������� ó��
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if(!IsServer) return;

        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;

        collision.gameObject.GetComponent<Player>().ShowSelectSpellPopupClientRPC(this);
    }

    /// <summary>
    /// Ŭ���̾�Ʈ���� ó���Ǵ� �޼ҵ��Դϴ�.
    /// ������ ������ 1 �÷��ݴϴ�. 
    /// ������ �ø������ ������ SpellInfo�� �Ķ���ͷ� �Ѱ��ּ���.
    /// </summary>
    /// <param name="spellInfo"></param>
    public override void UpdateScrollEffectToServer(sbyte spellIndex)
    {
        // �������� ���ο� ���� ������ ���ε�
        SpellManager.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);
    }
   
}
