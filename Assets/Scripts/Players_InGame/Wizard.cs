using Unity.Netcode;
using UnityEngine;
/// <summary> 
/// 1. Wizard ���� ����
/// </summary>
public class Wizard : Player
{
    public SpellName[] ownedSpellList;

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        // == �̵� ==
        // Server Auth ����� �̵� ó��
        HandleMovementServerAuth();
        //�׽�Ʈ�� Client Auth ��� �̵� ó��
        //HandleMovement(); 

        // == ���� == 
        // ��ų�ߵ�üũ��ĵ� ���� Update���� �� �� ���� ���� ������� �ٲ� �ʿ䰡 ����.!  �ϴ� �׽�Ʈ������ Update���� ����. 
/*        UpdateAttackInput();
        for (int i = 0; i < ownedSpellPrefabList.Count; i++)
        {
            spellController.CheckCastSpell(i);
        }*/
    }


    public override void OnNetworkSpawn()
    {
        
        // Ŭ���̾�Ʈ ���ڰ� ������ player data �ʱ�ȭ ��û. �������� �ʱ�ȭ���ش�
        if(IsClient&&IsOwner)
        {
            Debug.Log($"Player{OwnerClientId} OnNetworkSpawn!");
            InitializePlayerServerRPC();
        }
            
    }

    [ServerRpc]
    private void InitializePlayerServerRPC(ServerRpcParams serverRpcParams = default)
    {
        Debug.Log($"InitializePlayerServerRPC. Player{serverRpcParams.Receive.SenderClientId} requested Initialize PlayerInGameData.");
        // �׽�Ʈ �������� ���縮��Ʈ �ϵ��ڵ�. �κ������ �����������. (��ųƮ��)
        ownedSpellList = new SpellName[]{
                SpellName.FireBallLv1,
                SpellName.WaterBallLv1,
                SpellName.IceBallLv1,
                SpellName.MagicShieldLv1
                };
        InitializePlayerOnServer(ownedSpellList, serverRpcParams.Receive.SenderClientId);
    }
}
