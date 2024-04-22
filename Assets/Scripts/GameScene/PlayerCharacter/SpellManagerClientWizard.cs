using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// �⺻ Wizard_Male �÷��̾� ĳ���� ������Ʈ�� ���̴� ��ũ��Ʈ
/// ���� ���
///   1. ���� ĳ���� ���� ���� ��Ȳ ����
///   2. ĳ���� ���� ���� �ߵ� 
///   3. ���� ���� ������ ���� ������ ����
///   4. ���� ĳ�������� ���� ������Ʈ ����
///   
/// </summary>
public class SpellManagerClientWizard : NetworkBehaviour
{
    public SpellManagerServerWizard spellManagerServerWizard;

    private const byte defenceSpellIndex = 3;
    private const byte totalSpellCount = 4;
    // Ŭ���̾�Ʈ�� ����Ǵ� ����
    // SpellInfoList�� �ε��� 0~2������ ���ݸ���, 3�� ���� �Դϴ�.
    [SerializeField] private List<SpellInfo> spellInfoListOnClient;
    [SerializeField] private float[] restTimeCurrentSpellArrayOnClient;

    public override void OnNetworkSpawn()
    {
        spellInfoListOnClient = new List<SpellInfo>();
        restTimeCurrentSpellArrayOnClient = new float[totalSpellCount];
    }
        
    private void Update()
    {
        // ��Ÿ�� ����
        for (ushort i = 0; i < spellInfoListOnClient.Count; i++)
        {
            Cooltime(i);
        }        
    }

    /// <summary>
    /// ���� ��Ÿ�� ����. Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ�
    /// </summary>
    private void Cooltime(ushort spellIndex)
    {
        //Debug.Log($"spellNumber : {spellIndex}, currentSpellPrefabArray.Length : {currentSpellPrefabArray.Length}");
        if (spellInfoListOnClient[spellIndex] == null) return;
        // ��Ÿ�� ����
        if (spellInfoListOnClient[spellIndex].spellState == SpellState.Cooltime)
        {
            restTimeCurrentSpellArrayOnClient[spellIndex] += Time.deltaTime;
            if (restTimeCurrentSpellArrayOnClient[spellIndex] >= spellInfoListOnClient[spellIndex].coolTime)
            {                
                restTimeCurrentSpellArrayOnClient[spellIndex] = 0f;
                // ���⼭ ������ Ready�� �ٲ� State�� ���� �ϸ� �������� �ٽ� �ݹ��ؼ� �� Ŭ���̾�Ʈ ������Ʈ�� State�� Ready�� �ٲ�� �ϴµ�, �� ���̿� �����̰� �־
                // ���⼭ �� �� Ŭ���̾�Ʈ�� State�� �ٲ��ְ� ������ ���� ���ش�.
                spellInfoListOnClient[spellIndex].spellState = SpellState.Ready;
                spellManagerServerWizard.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Ready);
            }
            //Debug.Log($"��Ÿ�� ���� �޼ҵ�. spellState:{spellInfoListOnClient[spellIndex].spellState}, restTime:{restTimeCurrentSpellArrayOnClient[spellIndex]}, coolTime:{spellInfoListOnClient[spellIndex].coolTime}");            
        }
    }

    #region ���� ���� ĳ����&�߻�
    /// <summary>
    /// ���� ĳ���� ������ ������ ��û�մϴ�. Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ��Դϴ�.
    /// </summary>
    /// <param name="spellIndex"></param>
    public void StartCastingSpellOnClient(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex].spellState != SpellState.Ready) return;

        // ������ ���� ĳ���� ��û
        spellManagerServerWizard.StartCastingAttackSpellServerRPC(spellInfoListOnClient[spellIndex].spellName, GetComponent<NetworkObject>());
        // ������ �ش� �÷��̾��� ���� SpellState ������Ʈ
        spellManagerServerWizard.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Casting);
        // ������ �ִϸ��̼� ���� ��û
        GameMultiplayer.Instance.UpdatePlayerAttackAnimStateOnServerRPC(OwnerClientId, PlayerAttackAnimState.CastingAttackMagic);
    }

    /// <summary>
    /// ĳ�������� ���� �߻�. Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ�
    /// </summary>
    public void ShootCurrentCastingSpellOnClient(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex].spellState != SpellState.Casting) return;

        // ������ ���� �߻� ��û
        spellManagerServerWizard.ShootCastingSpellObjectServerRPC() ;
        // �ش� SpellState ������Ʈ
        spellManagerServerWizard.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Cooltime);
        // ������ �ִϸ��̼� ���� ��û
        GameMultiplayer.Instance.UpdatePlayerAttackAnimStateOnServerRPC(OwnerClientId, PlayerAttackAnimState.ShootingMagic);
    }
    #endregion

    #region ��� ���� ����
    public void ActivateDefenceSpellOnClient()
    {
        if (spellInfoListOnClient[defenceSpellIndex].spellState != SpellState.Ready) return;

        // ������ ���� ���� ��û
        spellManagerServerWizard.StartActivateDefenceSpellServerRPC(spellInfoListOnClient[defenceSpellIndex].spellName, GetComponent<NetworkObject>());
        // �ش� SpellState ������Ʈ
        spellManagerServerWizard.UpdatePlayerSpellStateServerRPC(defenceSpellIndex, SpellState.Cooltime);
        // ������ �ִϸ��̼� ���� ��û
        GameMultiplayer.Instance.UpdatePlayerAttackAnimStateOnServerRPC(OwnerClientId, PlayerAttackAnimState.CastingDefensiveMagic);
    }
    #endregion

    #region ���� ���� restTime/coolTime ���
    /// <summary>
    /// Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ�
    /// </summary>
    /// <param name="spellIndex"></param>
    /// <returns></returns>
    public float GetCurrentSpellCoolTimeRatio(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex] == null) return 0f;
        float coolTimeRatio = restTimeCurrentSpellArrayOnClient[spellIndex] / spellInfoListOnClient[spellIndex].coolTime;
        return coolTimeRatio;
    }
    #endregion

    /// <summary>
    /// Spell state�� ���� �� �ִ� �޼ҵ� �Դϴ�
    /// </summary>
    /// <param name="spellIndex"></param>
    /// <returns></returns>
    public SpellState GetSpellStateFromSpellIndexOnClient(ushort spellIndex)
    {
        return spellInfoListOnClient[spellIndex].spellState;
    }
   
    [ClientRpc]
    public void UpdatePlayerSpellInfoArrayClientRPC(SpellInfo[] spellInfoArray)
    {
        // ServerRPC�� ��û�� Ŭ���̾�Ʈ���Ը� ������Ʈ �ǵ��� ���͸�
        if (!IsOwner) return;

        spellInfoListOnClient = new List<SpellInfo>(spellInfoArray.ToList<SpellInfo>());
    }

    /// <summary>
    /// Player Server���� Player Init�� InitPlayerSpellInfoArrayOnServer�� �Բ� ȣ��Ǵ� �޼��� �Դϴ�. 
    /// �÷��̾��� ��ų ����Ʈ ������ �����մϴ�
    /// </summary>
    /// <param name="skills"></param>
    public void InitPlayerSpellInfoListClient(SkillName[] skills)
    {
        List<SpellInfo> playerSpellInfoList = new List<SpellInfo>();
        foreach (SkillName skill in skills)
        {
            SpellInfo spellInfo = new SpellInfo(SpellSpecifications.Instance.GetSpellDefaultSpec(skill));
            spellInfo.ownerPlayerClientId = OwnerClientId;
            playerSpellInfoList.Add(spellInfo);
        }

        spellInfoListOnClient = playerSpellInfoList;
    }

    /// <summary>
    /// Ư�� client�� ���� �������� ������ ������ �˷��ִ� �޼ҵ� �Դϴ�.  
    /// </summary>
    /// <param name="clientId">�˰���� Client�� ID</param>
    /// <returns></returns>
    public List<SpellInfo> GetSpellInfoList()
    {
        return spellInfoListOnClient;
    }
}
