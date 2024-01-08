using System;
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
/// </summary>
public class SpellController : NetworkBehaviour
{
    public event EventHandler OnSpellStateChanged;

    // ������ ����Ǵ� ����
    [SerializeField] private Dictionary<ulong, List<SpellInfo>> spellInfoListOnServer = new Dictionary<ulong, List<SpellInfo>>();

    // Ŭ���̾�Ʈ�� ����Ǵ� ����
    [SerializeField] protected List<GameObject> ownedSpellPrefabListOnClient;
    [SerializeField] private List<SpellInfo> spellInfoListOnClient;
    [SerializeField] private float[] restTimeCurrentSpellArrayOnClient = new float[3];

    private void Awake()
    {
        spellInfoListOnClient = new List<SpellInfo>(3) {};
    }

    private void Update()
    {
        // ��Ÿ�� ����
        for (ushort i = 0; i < ownedSpellPrefabListOnClient.Count; i++)
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
        if (ownedSpellPrefabListOnClient[spellIndex] == null) return;
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
                UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Ready);
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
        ownedSpellPrefabListOnClient[spellIndex].GetComponent<Spell>().CastSpell(spellInfoListOnClient[spellIndex], GetComponent<NetworkObject>());
        // ĳ���� �������� ����� SpellState ���� Server�� ����.
        UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Casting);
    }

    /// <summary>
    /// ĳ�������� ���� �߻�. Ŭ���̾�Ʈ���� �����ϴ� �޼ҵ�
    /// </summary>
    public void ShootCurrentCastingSpellOnClient(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex].spellState != SpellState.Casting) return;

        // ������ �߻� ��û
        SpellManager.Instance.ShootSpellObject() ; 
        // �߻�� ����� SpellState ���� ������ ����
        UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Cooltime);
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
    public SpellState GetSpellStateFromSpellIndexOnServer(ulong clientId, ushort spellIndex)
    {
        return spellInfoListOnServer[clientId][spellIndex].spellState;
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

    #region ���� ���� ���� ����
    // GameAsset���κ��� Spell Prefab �ε�. playerClass�� spellName���� �ʿ��� �����ո� �ҷ��´�. 
    // �ҷ��� SpellInfoList�� Server���� �����Ѵ�.
    public void SetCurrentSpellOnClient(SpellName[] ownedSpellNameList)
    {
        foreach (var spellName in ownedSpellNameList)
        {
            Debug.Log($"spellName: {spellName}");
            ownedSpellPrefabListOnClient.Add(GameAssets.instantiate.GetSpellPrefab(spellName));
        }
        for (ushort spellIndex = 0; spellIndex < ownedSpellPrefabListOnClient.Count; spellIndex++)
        {
            ownedSpellPrefabListOnClient[spellIndex].GetComponent<Spell>().InitSpellInfoDetail();
            // ���� ������
            if(spellInfoListOnClient.Count <= spellIndex)
            {
                spellInfoListOnClient.Add(ownedSpellPrefabListOnClient[spellIndex].GetComponent<Spell>().spellInfo);
            }
            // ��ų ��� �����
            else
            {
                spellInfoListOnClient[spellIndex] = ownedSpellPrefabListOnClient[spellIndex].GetComponent<Spell>().spellInfo;
            }           
            Sprite spellIconImage = GameAssets.instantiate.GetSpellIconImage(spellInfoListOnClient[spellIndex].spellName);
            FindObjectOfType<GamePadUI>().UpdateSpellUI(spellIconImage, spellIndex);
        }
            
        // �������� �÷��̾��� '���� ���� ���� ����Ʈ'�� ����.
        UpdatePlayerSpellInfoServerRPC(spellInfoListOnClient.ToArray());  
    }
    #endregion

    /// <summary>
    /// ����� SpellState�� Server�� �����մϴ�. ������� Server�� Server�� PlayerAnimator���� �ִϸ��̼� ������ �����մϴ�.
    /// </summary>
    /// <param name="spellIndex"></param>
    [ServerRpc]
    private void UpdatePlayerSpellStateServerRPC(ushort spellIndex, SpellState spellState, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        if (!spellInfoListOnServer.ContainsKey(clientId))
        {
            Debug.LogError($"UpdatePlayerSpellStateServerRPC. There is no SpellInfoList for this client. clientId:{clientId}");
            return;
        }

        spellInfoListOnServer[clientId][spellIndex].spellState = spellState;
        OnSpellStateChanged?.Invoke(this, new SpellStateEventData(clientId, spellState));
        Debug.Log($"SpellController UpdatePlayerSpellStateServerRPC: {spellIndex}");

        // ��û�� Ŭ���̾�Ʈ�� currentSpellInfoList ����ȭ
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellController>().UpdatePlayerSpellInfoClientRPC(spellInfoListOnServer[clientId].ToArray());
    }

    /// <summary>
    /// Server������ ������ ClientId�� SpellInfo ����Ʈ ������Ʈ.
    /// ������Ʈ�� ������ Ŭ���̾�Ʈ���� SpellInfo ���� ����ȭ�� �����մϴ�.
    /// </summary>
    /// <param name="spellInfoArray"></param>
    /// <param name="serverRpcParams"></param>
    [ServerRpc]
    private void UpdatePlayerSpellInfoServerRPC(SpellInfo[] spellInfoArray, ServerRpcParams serverRpcParams = default)
    {
        ulong clientId = serverRpcParams.Receive.SenderClientId;
        if (!spellInfoListOnServer.ContainsKey(clientId))
        {
            spellInfoListOnServer.Add(clientId, spellInfoArray.ToList<SpellInfo>());
        }
        else
        {
            spellInfoListOnServer[clientId] = spellInfoArray.ToList<SpellInfo>();
        }

        // ��û�� Ŭ���̾�Ʈ�� currentSpellInfoList ����ȭ
        NetworkClient networkClient = NetworkManager.ConnectedClients[clientId];
        networkClient.PlayerObject.GetComponent<SpellController>().UpdatePlayerSpellInfoClientRPC(spellInfoListOnServer[clientId].ToArray());
    }

    [ClientRpc]
    private void UpdatePlayerSpellInfoClientRPC(SpellInfo[] spellInfoArray)
    {
        spellInfoListOnClient = spellInfoArray.ToList<SpellInfo>();
    }

}
