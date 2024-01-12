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
    // Ŭ���̾�Ʈ�� ����Ǵ� ����
    [SerializeField] private List<SpellInfo> spellInfoListOnClient;
    [SerializeField] private float[] restTimeCurrentSpellArrayOnClient = new float[3];

    private void Awake()
    {
        spellInfoListOnClient = new List<SpellInfo>(3) {};
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
                SpellManager.Instance.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Ready);
            }
            //Debug.Log($"��Ÿ�� ���� �޼ҵ�. spellState:{spellInfoListOnClient[spellIndex].spellState}, restTime:{restTimeCurrentSpellArrayOnClient[spellIndex]}, coolTime:{spellInfoListOnClient[spellIndex].coolTime}");            
        }
    }

    #region ���� ������ ���� ����
    /// <summary>
    /// ���� ĳ���� ����. Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ�
    /// </summary>
    /// <param name="spellIndex"></param>
    public void StartCastingSpellOnClient(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex].spellState == SpellState.Cooltime || spellInfoListOnClient[spellIndex].spellState == SpellState.Casting)
        {
            //Debug.Log($"���� {spellInfoListOnClient[spellIndex].spellName}�� ���� �����Ұ������Դϴ�.");
            return;
        }

        // ĳ���� ���� Server���� ��û.
        SpellManager.Instance.StartCastingSpellServerRPC(spellInfoListOnClient[spellIndex].spellName, GetComponent<NetworkObject>());
        // ĳ���� �������� ����� SpellState ���� Server�� ����.
        SpellManager.Instance.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Casting);
    }

    /// <summary>
    /// ĳ�������� ���� �߻�. Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ�
    /// </summary>
    public void ShootCurrentCastingSpellOnClient(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex].spellState != SpellState.Casting) return;

        // ������ �߻� ��û
        SpellManager.Instance.ShootSpellObjectServerRPC() ;
        // �߻�� ����� SpellState ���� ������ ����
        SpellManager.Instance.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Cooltime);
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

    //public Spell


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
        //Debug.Log($"UpdatePlayerSpellInfoArrayClientRPC. I'm {OwnerClientId}, IsOwner:{IsOwner}");
        // ServerRPC�� ��û�� Ŭ���̾�Ʈ���Ը� ������Ʈ �ǵ��� ���͸�
        if (!IsOwner) return;
        spellInfoListOnClient = spellInfoArray.ToList<SpellInfo>();
    }
}
