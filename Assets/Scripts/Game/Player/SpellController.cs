using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// �÷��̾� ĳ���� ������Ʈ�� ���̴� ��ũ��Ʈ
/// ���� ���
///   1. ���� ĳ���� ���� ���� ��Ȳ ����
///   2. ĳ���� ���� ���� �ߵ� 
///   3. ���� ���� ������ ���� ������ ����
///   4. ���� ĳ�������� ���� ������Ʈ ����
///   
/// ��Ÿ�Ӱ����� Ŭ���ʿ��� ������.  ���� ������ �� ���� �ʿ�
/// </summary>
public class SpellController : NetworkBehaviour
{
    private const byte defenceSpellIndex = 3;
    private const byte totalSpellCount = 4;
    // Ŭ���̾�Ʈ�� ����Ǵ� ����
    // SpellInfoList�� �ε��� 0~2������ ���ݸ���, 3�� ���� �Դϴ�.
    [SerializeField] private List<SpellInfo> SpellInfoListOnClient;
    [SerializeField] private float[] restTimeCurrentSpellArrayOnClient;

    public override void OnNetworkSpawn()
    {
        SpellInfoListOnClient = new List<SpellInfo>(totalSpellCount);
        restTimeCurrentSpellArrayOnClient = new float[totalSpellCount];
    }
        
    private void Update()
    {
        // ��Ÿ�� ����
        for (ushort i = 0; i < SpellInfoListOnClient.Count; i++)
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
        if (SpellInfoListOnClient[spellIndex] == null) return;
        // ��Ÿ�� ����
        if (SpellInfoListOnClient[spellIndex].spellState == SpellState.Cooltime)
        {
            restTimeCurrentSpellArrayOnClient[spellIndex] += Time.deltaTime;
            if (restTimeCurrentSpellArrayOnClient[spellIndex] >= SpellInfoListOnClient[spellIndex].coolTime)
            {                
                restTimeCurrentSpellArrayOnClient[spellIndex] = 0f;
                // ���⼭ ������ Ready�� �ٲ� State�� ���� �ϸ� �������� �ٽ� �ݹ��ؼ� �� Ŭ���̾�Ʈ ������Ʈ�� State�� Ready�� �ٲ�� �ϴµ�, �� ���̿� �����̰� �־
                // ���⼭ �� �� Ŭ���̾�Ʈ�� State�� �ٲ��ְ� ������ ���� ���ش�.
                SpellInfoListOnClient[spellIndex].spellState = SpellState.Ready;
                SpellManager.Instance.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Ready);
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
        if (SpellInfoListOnClient[spellIndex].spellState != SpellState.Ready) return;

        // ������ ���� ĳ���� ��û
        SpellManager.Instance.StartCastingAttackSpellServerRPC(SpellInfoListOnClient[spellIndex].spellName, GetComponent<NetworkObject>());
        // �ش� SpellState ������Ʈ
        SpellManager.Instance.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Casting);
    }

    /// <summary>
    /// ĳ�������� ���� �߻�. Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ�
    /// </summary>
    public void ShootCurrentCastingSpellOnClient(ushort spellIndex)
    {
        if (SpellInfoListOnClient[spellIndex].spellState != SpellState.Casting) return;

        // ������ ���� �߻� ��û
        SpellManager.Instance.ShootCastingSpellObjectServerRPC() ;
        // �ش� SpellState ������Ʈ
        SpellManager.Instance.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Cooltime);
    }
    #endregion

    #region ��� ���� ����
    public void ActivateDefenceSpellOnClient()
    {
        if (SpellInfoListOnClient[defenceSpellIndex].spellState != SpellState.Ready) return;

        // ������ ���� ���� ��û
        SpellManager.Instance.StartActivateDefenceSpellServerRPC(SpellInfoListOnClient[defenceSpellIndex].spellName, GetComponent<NetworkObject>());
        // �ش� SpellState ������Ʈ
        SpellManager.Instance.UpdatePlayerSpellStateServerRPC(defenceSpellIndex, SpellState.Cooltime);
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
        if (SpellInfoListOnClient[spellIndex] == null) return 0f;
        float coolTimeRatio = restTimeCurrentSpellArrayOnClient[spellIndex] / SpellInfoListOnClient[spellIndex].coolTime;
        return coolTimeRatio;
    }
    #endregion

    //public Spell


    /// <summary>
    /// Spell state�� ���� �� �ִ� �޼ҵ� �Դϴ�
    /// </summary>
    /// <param name="spellIndex"></param>
    /// <returns></returns>
    public SpellState GetSpellStateFromSpellIndexOnClient(ushort spellIndex)
    {
        return SpellInfoListOnClient[spellIndex].spellState;
    }
   
    [ClientRpc]
    public void UpdatePlayerSpellInfoArrayClientRPC(SpellInfo[] spellInfoArray)
    {
        //Debug.Log($"UpdatePlayerSpellInfoArrayClientRPC. I'm {OwnerClientId}, IsOwner:{IsOwner}");
        // ServerRPC�� ��û�� Ŭ���̾�Ʈ���Ը� ������Ʈ �ǵ��� ���͸�
        if (!IsOwner) return;
        SpellInfoListOnClient = spellInfoArray.ToList<SpellInfo>();
    }

    /// <summary>
    /// Ư�� client�� ���� �������� ������ ������ �˷��ִ� �޼ҵ� �Դϴ�.  
    /// </summary>
    /// <param name="clientId">�˰���� Client�� ID</param>
    /// <returns></returns>
    public List<SpellInfo> GetSpellInfoArray()
    {
        return SpellInfoListOnClient;
    }
}
