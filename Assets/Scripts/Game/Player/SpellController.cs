using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
/// <summary>
/// 플레이어 캐릭터 오브젝트에 붙이는 스크립트
/// 현재 기능
///   1. 현재 캐릭터 마법 보유 현황 관리
///   2. 캐릭터 보유 마법 발동 
///   3. 현재 보유 마법에 대한 정보를 공유
///   4. 현재 캐스팅중인 마법 오브젝트 관리
/// </summary>
public class SpellController : MonoBehaviour
{
    [SerializeField] private GameObject[] currentSpellPrefabArray = new GameObject[3];
    [SerializeField] private SpellInfo[] currentSpellInfoList = new SpellInfo[3];
    [SerializeField] private Player player;
    [SerializeField] private float[] restTimeCurrentSpellArray = new float[3];

    [SerializeField] private GameObject currentCastingSpellObject;

    private void Update()
    {
        // 쿨타임 관리
        for (ushort i = 0; i < currentSpellPrefabArray.Length; i++)
        {
            Cooltime(i);
        }        
    }

    /// <summary>
    /// 마법 쿨타임 관리
    /// </summary>
    private void Cooltime(ushort spellIndex)
    {
        //Debug.Log($"spellNumber : {spellIndex}, currentSpellPrefabArray.Length : {currentSpellPrefabArray.Length}");
        if (currentSpellPrefabArray[spellIndex] == null) return;
        // 쿨타임 관리
        if (currentSpellInfoList[spellIndex].castAble == false)
        {
            restTimeCurrentSpellArray[spellIndex] += Time.deltaTime;
            if (restTimeCurrentSpellArray[spellIndex] >= currentSpellInfoList[spellIndex].coolTime)
            {
                currentSpellInfoList[spellIndex].castAble = true;
                restTimeCurrentSpellArray[spellIndex] = 0f;
            }
            return;
        }
    }

    #region 현재 설정된 마법 시전
    public void StartCastingSpell(int spellIndex)
    {
        /*        // 스킬 발동 키 입력 관리
                bool isPlayerInput = false;
                switch (spellIndex)
                {
                    case 0:
                        isPlayerInput = player.IsAttack1Casting();
                        break;
                    case 1:
                        isPlayerInput = player.IsAttack2Casting();
                        break;
                    case 2:
                        isPlayerInput = player.IsAttack3Casting();
                        break;
                    default:
                        Debug.Log("CheckCastSpell : Wrong spellNumber");
                        break;
                }

                if (isPlayerInput)
                {
                    currentSpellPrefabArray[spellIndex].GetComponent<Spell>()
                        .CastSpell(
                        currentSpellInfoList[spellIndex], 
                        player.GetComponent<NetworkObject>());
                    currentSpellInfoList[spellIndex].castAble = false;
                }*/
        if (!currentSpellInfoList[spellIndex].castAble)
        {
            Debug.Log($"마법 {currentSpellInfoList[spellIndex].spellName}은 현재 시전불가상태입니다.");
            return;
        }

        currentSpellPrefabArray[spellIndex].GetComponent<Spell>().CastSpell(currentSpellInfoList[spellIndex],player.GetComponent<NetworkObject>());
    }

    /// <summary>
    /// 캐스팅중인 마법 발사
    /// </summary>
    public void ShootCurrentCastingSpell(ulong spellIndex)
    {
        SpellManager.Instance.ShootSpellObject() ; 
        currentSpellInfoList[spellIndex].castAble = false;
    }
    #endregion

    #region 현재 마법 restTime/coolTime 얻기
    public float GetCurrentSpellCoolTimeRatio(int spellIndex)
    {
        if (currentSpellInfoList[spellIndex] == null) return 0f;
        float coolTimeRatio = restTimeCurrentSpellArray[spellIndex] / currentSpellInfoList[spellIndex].coolTime;
        return coolTimeRatio;
    }
    #endregion

    #region 현재 보유 마법 변경
    public void SetCurrentSpell(GameObject spellObjectPrefab, int spellIndex)
    {
        //Debug.Log($"SetCurrentSpell spellIndex:{spellIndex}, currentSpellPrefabArray.Length: {currentSpellPrefabArray.Length}");
        currentSpellPrefabArray[spellIndex] = spellObjectPrefab;
        currentSpellPrefabArray[spellIndex].GetComponent<Spell>().InitSpellInfoDetail();
        currentSpellInfoList[spellIndex] = currentSpellPrefabArray[spellIndex].GetComponent<Spell>().spellInfo;
        Sprite spellIconImage = GameAssets.instantiate.GetSpellIconImage(currentSpellInfoList[spellIndex].spellName);
        FindObjectOfType<GamePadUI>().UpdateSpellUI(spellIconImage, spellIndex);
    }
    #endregion
}
