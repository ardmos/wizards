using Unity.Netcode;
using UnityEngine;
/// <summary> 
/// 1. Wizard ½ºÅÈ °ü¸®
/// </summary>
public class Wizard : Player
{
    public SkillName[] ownedSpellList;

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            Debug.Log($"OwnerClientId{OwnerClientId} requested Initialize PlayerInGameData.");

            ownedSpellList = new SkillName[]{
                SkillName.FireBallLv1,
                SkillName.WaterBallLv1,
                SkillName.IceBallLv1,
                SkillName.MagicShieldLv1
                };

            InitializePlayerOnServer(ownedSpellList, OwnerClientId);
        }
    }
}
