using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 플레이어 캐릭터 오브젝트에 붙이는 스크립트
/// 현재 기능
///   1. 마법 보유 현황 관리
///   2. 스킬 발동 
///   3. 현재 보유 마법에 대한 정보를 공유
/// </summary>
public class SpellController : MonoBehaviour
{
    public event EventHandler OnSpellChanged;
    [SerializeField] private GameObject[] currentSpellPrefabArray = new GameObject[3];
    [SerializeField] private List<Spell.SpellInfo> currentSpellInfoList;
    [SerializeField] private Player player;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float[] restTimeCurrentSpellArray = new float[3];

    void Start()
    {
        // 각 스펠별 정보 초기화
        foreach (var spell in currentSpellPrefabArray)
        {
            if (spell != null)
            {
                spell.GetComponent<Spell>().InitSpellInfoDetail();
                currentSpellInfoList.Add(spell.GetComponent<Spell>().spellInfo);
            }
        }
    }

    void Update()
    {
        for (int i = 0; i < currentSpellPrefabArray.Length; i++) {
            CheckCastSpell(i);
        }
    }



    #region 현재 설정된 마법 시전
    private void CheckCastSpell(int spellIndex)
    {
        Debug.Log($"spellNumber : {spellIndex}, currentSpellPrefabArray.Length : {currentSpellPrefabArray.Length}");
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

        // 스킬 발동 키 입력 관리
        bool isPlayerInput = false;
        switch (spellIndex)
        {
            case 0:
                isPlayerInput = player.IsAttack1();
                break;
            case 1:
                isPlayerInput = player.IsAttack2();
                break;
            case 2:
                isPlayerInput = player.IsAttack3();
                break;
            default:
                Debug.Log("CheckCastSpell : Wrong spellNumber");
                break;
        }

        if (isPlayerInput)
        {
            currentSpellPrefabArray[spellIndex].GetComponent<Spell>().CastSpell(new Spell.SpellLvlType { level = currentSpellInfoList[spellIndex].level, spellType = currentSpellInfoList[spellIndex].spellType }, muzzle);
            currentSpellInfoList[spellIndex].castAble = false;
        }
    }
    #endregion

    #region 현재 마법 이름 얻기
    public string GetCurrentSpellName(int spellNumber)
    {
        // null 예외처리 필요함
        if (currentSpellInfoList[spellNumber - 1] == null) return "";
        return currentSpellInfoList[spellNumber - 1].spellName;
    }
    #endregion

    #region 현재 마법 이미지 얻기
    public Sprite GetCurrentSpellIcon(int spellNumber)
    {
        if (currentSpellInfoList[spellNumber - 1] == null) return null;
        return currentSpellInfoList[spellNumber - 1].iconImage;
    }
    #endregion

    #region 현재 마법 restTime/coolTime 얻기
    public float GetCurrentSpellCoolTimeRatio(int spellNumber)
    {
        if (currentSpellInfoList[spellNumber - 1] == null) return 0f;
        float coolTimeRatio = restTimeCurrentSpellArray[spellNumber-1] / currentSpellInfoList[spellNumber - 1].coolTime;
        return coolTimeRatio;
    }
    #endregion

    #region 현재 마법 변경
    public void SetCurrentSpell(GameObject spellObjectPrefab, int spellNumber)
    {
        currentSpellPrefabArray[spellNumber - 1] = spellObjectPrefab;
        currentSpellPrefabArray[spellNumber - 1].GetComponent<Spell>().InitSpellInfoDetail();
        currentSpellInfoList[spellNumber - 1] = currentSpellPrefabArray[spellNumber - 1].GetComponent<Spell>().spellInfo;
        //OnSpellChanged?.Invoke(this, spellNumber); ////////////////////////////////////////  <----- GamePadUI 의 업데이트 스킬아이콘을 어쩌면 좋을꼬!!!! 여기부터!
    }
    #endregion
}
