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
    [SerializeField] private GameObject[] currentSpellPrefabArray;
    [SerializeField] private List<Spell.SpellInfo> currentSpellInfoList;
    [SerializeField] private Player player;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float restTimeCurrentSpell_1 = 0f;
    [SerializeField] private float restTimeCurrentSpell_2 = 0f;
    [SerializeField] private float restTimeCurrentSpell_3 = 0f;

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
        CheckCastSpell(1);
        CheckCastSpell(2);
        CheckCastSpell(3);
    }



    #region 현재 설정된 마법 시전
    private void CheckCastSpell(int spellNumber)
    {
        if (currentSpellPrefabArray[spellNumber - 1] == null) return;
        // 쿨타임 관리
        if (currentSpellInfoList[spellNumber - 1].castAble == false)
        {
            restTimeCurrentSpell_1 += Time.deltaTime;
            if (restTimeCurrentSpell_1 >= currentSpellInfoList[spellNumber - 1].coolTime)
            {
                currentSpellInfoList[spellNumber - 1].castAble = true;
                restTimeCurrentSpell_1 = 0f;
            }
            return;
        }

        // 스킬 발동 키 입력 관리
        bool isPlayerInput = false;
        switch (spellNumber)
        {
            case 1:
                isPlayerInput = player.IsAttack1();
                break;
            case 2:
                isPlayerInput = player.IsAttack2();
                break;
            case 3:
                isPlayerInput = player.IsAttack3();
                break;
            default:
                Debug.Log("CheckCastSpell : Wrong spellNumber");
                break;
        }

        if (isPlayerInput)
        {
            currentSpellPrefabArray[spellNumber - 1].GetComponent<Spell>().CastSpell(new Spell.SpellLvlType { level = currentSpellInfoList[spellNumber - 1].level, spellType = currentSpellInfoList[spellNumber - 1].spellType }, muzzle);
            currentSpellInfoList[spellNumber - 1].castAble = false;
        }
    }
    #endregion

    #region 현재 마법 이름 얻기
    public string GetCurrentSpellName(int spellNumber)
    {
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
        float coolTimeRatio = restTimeCurrentSpell_1 / currentSpellInfoList[spellNumber - 1].coolTime;
        return coolTimeRatio;
    }
    #endregion

    #region 현재 마법 변경
    public void SetCurrentSpell(GameObject spellObjectPrefab, int spellNumber)
    {
        currentSpellPrefabArray[spellNumber - 1] = spellObjectPrefab;
        currentSpellPrefabArray[spellNumber - 1].GetComponent<Spell>().InitSpellInfoDetail();
        currentSpellInfoList[spellNumber - 1] = currentSpellPrefabArray[spellNumber - 1].GetComponent<Spell>().spellInfo;
    }
    #endregion
}
