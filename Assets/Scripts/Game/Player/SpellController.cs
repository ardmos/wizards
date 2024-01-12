using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 플레이어 캐릭터 오브젝트에 붙이는 스크립트
/// 현재 기능
///   1. 현재 캐릭터 마법 보유 현황 관리
///   2. 캐릭터 보유 마법 발동 
///   3. 현재 보유 마법에 대한 정보를 공유
///   4. 현재 캐스팅중인 마법 오브젝트 관리
///   
/// 쿨타임같은건 클라쪽에서 관리중.  추후 정리할 때 점검 필요
/// </summary>
public class SpellController : NetworkBehaviour
{
    // 클라이언트에 저장되는 내용
    [SerializeField] private List<SpellInfo> spellInfoListOnClient;
    [SerializeField] private float[] restTimeCurrentSpellArrayOnClient = new float[3];

    private void Awake()
    {
        spellInfoListOnClient = new List<SpellInfo>(3) {};
    }

    private void Update()
    {
        // 쿨타임 관리
        for (ushort i = 0; i < spellInfoListOnClient.Count; i++)
        {
            Cooltime(i);
        }        
    }

    /// <summary>
    /// 마법 쿨타임 관리. 클라이언트에서 동작하는 메소드
    /// </summary>
    private void Cooltime(ushort spellIndex)
    {
        //Debug.Log($"spellNumber : {spellIndex}, currentSpellPrefabArray.Length : {currentSpellPrefabArray.Length}");
        if (spellInfoListOnClient[spellIndex] == null) return;
        // 쿨타임 관리
        if (spellInfoListOnClient[spellIndex].spellState == SpellState.Cooltime)
        {
            restTimeCurrentSpellArrayOnClient[spellIndex] += Time.deltaTime;
            if (restTimeCurrentSpellArrayOnClient[spellIndex] >= spellInfoListOnClient[spellIndex].coolTime)
            {                
                restTimeCurrentSpellArrayOnClient[spellIndex] = 0f;
                // 여기서 서버에 Ready로 바뀐 State를 보고 하면 서버에서 다시 콜백해서 현 클라이언트 오브젝트의 State가 Ready로 바뀌긴 하는데, 그 사이에 딜레이가 있어서
                // 여기서 한 번 클라이언트의 State를 바꿔주고 서버에 보고 해준다.
                spellInfoListOnClient[spellIndex].spellState = SpellState.Ready;
                SpellManager.Instance.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Ready);
            }
            //Debug.Log($"쿨타임 관리 메소드. spellState:{spellInfoListOnClient[spellIndex].spellState}, restTime:{restTimeCurrentSpellArrayOnClient[spellIndex]}, coolTime:{spellInfoListOnClient[spellIndex].coolTime}");            
        }
    }

    #region 현재 설정된 마법 시전
    /// <summary>
    /// 마법 캐스팅 시작. 클라이언트에서 동작하는 메소드
    /// </summary>
    /// <param name="spellIndex"></param>
    public void StartCastingSpellOnClient(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex].spellState == SpellState.Cooltime || spellInfoListOnClient[spellIndex].spellState == SpellState.Casting)
        {
            //Debug.Log($"마법 {spellInfoListOnClient[spellIndex].spellName}은 현재 시전불가상태입니다.");
            return;
        }

        // 캐스팅 시작 Server에게 요청.
        SpellManager.Instance.StartCastingSpellServerRPC(spellInfoListOnClient[spellIndex].spellName, GetComponent<NetworkObject>());
        // 캐스팅 시작으로 변경된 SpellState 정보 Server에 보고.
        SpellManager.Instance.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Casting);
    }

    /// <summary>
    /// 캐스팅중인 마법 발사. 클라이언트에서 동작하는 메소드
    /// </summary>
    public void ShootCurrentCastingSpellOnClient(ushort spellIndex)
    {
        if (spellInfoListOnClient[spellIndex].spellState != SpellState.Casting) return;

        // 서버에 발사 요청
        SpellManager.Instance.ShootSpellObjectServerRPC() ;
        // 발사로 변경된 SpellState 상태 서버에 보고
        SpellManager.Instance.UpdatePlayerSpellStateServerRPC(spellIndex, SpellState.Cooltime);
    }
    #endregion

    #region 현재 마법 restTime/coolTime 얻기
    /// <summary>
    /// 클라이언트에서 동작하는 메소드
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
    /// Spell state를 얻을 수 있는 메소드 입니다
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
        // ServerRPC를 요청한 클라이언트에게만 업데이트 되도록 필터링
        if (!IsOwner) return;
        spellInfoListOnClient = spellInfoArray.ToList<SpellInfo>();
    }
}
