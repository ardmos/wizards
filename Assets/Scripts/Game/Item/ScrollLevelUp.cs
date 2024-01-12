using Unity.Netcode;
using UnityEngine;

public class ScrollLevelUp : Scroll
{
    public override void OnNetworkSpawn()
    {
        scrollName = Item.ItemName.Scroll_LevelUp;
    }

    /// <summary>
    /// 충돌은 서버에서 처리
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        if(!IsServer) return;

        ulong collisionedClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;

        collision.gameObject.GetComponent<Player>().ShowSelectSpellPopupClientRPC(this);
    }

    /// <summary>
    /// 클라이언트에서 처리되는 메소드입니다.
    /// 스펠의 레벨을 1 올려줍니다. 
    /// 레벨을 올리고싶은 스펠의 SpellInfo를 파라미터로 넘겨주세요.
    /// </summary>
    /// <param name="spellInfo"></param>
    public override void UpdateScrollEffectToServer(sbyte spellIndex)
    {
        // 레벨업된 새로운 스펠 정보를 업로드
        SpellManager.Instance.UpdateScrollEffectServerRPC(scrollName, spellIndex);
    }
   
}
