using Unity.Netcode;
using UnityEngine;
/// <summary> 
/// 1. Wizard ½ºÅÈ °ü¸®
/// </summary>
public class Wizard : Player
{
    public SpellName[] ownedSpellList;

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        HandleMovementServerAuth();
    }


    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            Debug.Log($"{nameof(OnNetworkSpawn)}, Player{OwnerClientId} requested Initialize PlayerInGameData.");

            ownedSpellList = new SpellName[]{
                SpellName.FireBallLv1,
                SpellName.WaterBallLv1,
                SpellName.IceBallLv1,
                SpellName.MagicShieldLv1
                };

            InitializePlayerOnServer(ownedSpellList, OwnerClientId);
        }
    }
}
