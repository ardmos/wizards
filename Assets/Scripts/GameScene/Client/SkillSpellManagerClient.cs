using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ���� ��Ÿ�� ����. ��Ÿ�� ������ Ŭ���̾�Ʈ���� ���ְ��ֽ��ϴ�.
/// </summary>
public class SkillSpellManagerClient : NetworkBehaviour
{
    public SkillSpellManagerServer skillSpellManagerServer;

    protected const byte totalSpellCount = 4;
    [SerializeField] protected List<SpellInfo> spellInfoListOnClient;
    [SerializeField] protected float[] restTimeCurrentSpellArrayOnClient;

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
    /// ���� ��Ÿ�� ����. ��Ÿ�� ������ Ŭ���̾�Ʈ���� ���ְ��ֽ��ϴ�.
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
                skillSpellManagerServer.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Ready);
            }
            //Debug.Log($"��Ÿ�� ���� �޼ҵ�. spellState:{spellInfoListOnClient[spellIndex].spellState}, restTime:{restTimeCurrentSpellArrayOnClient[spellIndex]}, coolTime:{spellInfoListOnClient[spellIndex].coolTime}");            
        }
    }

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
        Debug.Log("2. SkillSpellManagerClient.InitPlayerSpellInfoListClient");
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

    #region ���� ���� restTime/coolTime ���
    /// <summary>
    /// Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ�
    /// </summary>
    /// <param name="spellIndex"></param>
    /// <returns></returns>
    public float GetCurrentSpellCoolTimeRatio(ushort spellIndex)
    {
        if (spellInfoListOnClient.Count <= spellIndex) return 0f;
        if (spellInfoListOnClient[spellIndex] == null) return 0f;
        float coolTimeRatio = restTimeCurrentSpellArrayOnClient[spellIndex] / spellInfoListOnClient[spellIndex].coolTime;
        return coolTimeRatio;
    }
    #endregion
}
