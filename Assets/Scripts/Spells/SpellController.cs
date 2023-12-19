using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �÷��̾� ĳ���� ������Ʈ�� ���̴� ��ũ��Ʈ
/// ���� ���
///   1. ���� ���� ��Ȳ ����
///   2. ��ų �ߵ� 
///   3. ���� ���� ������ ���� ������ ����
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
        // �� ���纰 ���� �ʱ�ȭ
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



    #region ���� ������ ���� ����
    private void CheckCastSpell(int spellIndex)
    {
        Debug.Log($"spellNumber : {spellIndex}, currentSpellPrefabArray.Length : {currentSpellPrefabArray.Length}");
        if (currentSpellPrefabArray[spellIndex] == null) return;
        // ��Ÿ�� ����
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

        // ��ų �ߵ� Ű �Է� ����
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

    #region ���� ���� �̸� ���
    public string GetCurrentSpellName(int spellNumber)
    {
        // null ����ó�� �ʿ���
        if (currentSpellInfoList[spellNumber - 1] == null) return "";
        return currentSpellInfoList[spellNumber - 1].spellName;
    }
    #endregion

    #region ���� ���� �̹��� ���
    public Sprite GetCurrentSpellIcon(int spellNumber)
    {
        if (currentSpellInfoList[spellNumber - 1] == null) return null;
        return currentSpellInfoList[spellNumber - 1].iconImage;
    }
    #endregion

    #region ���� ���� restTime/coolTime ���
    public float GetCurrentSpellCoolTimeRatio(int spellNumber)
    {
        if (currentSpellInfoList[spellNumber - 1] == null) return 0f;
        float coolTimeRatio = restTimeCurrentSpellArray[spellNumber-1] / currentSpellInfoList[spellNumber - 1].coolTime;
        return coolTimeRatio;
    }
    #endregion

    #region ���� ���� ����
    public void SetCurrentSpell(GameObject spellObjectPrefab, int spellNumber)
    {
        currentSpellPrefabArray[spellNumber - 1] = spellObjectPrefab;
        currentSpellPrefabArray[spellNumber - 1].GetComponent<Spell>().InitSpellInfoDetail();
        currentSpellInfoList[spellNumber - 1] = currentSpellPrefabArray[spellNumber - 1].GetComponent<Spell>().spellInfo;
        //OnSpellChanged?.Invoke(this, spellNumber); ////////////////////////////////////////  <----- GamePadUI �� ������Ʈ ��ų�������� ��¼�� ������!!!! �������!
    }
    #endregion
}
