using System;
using System.Collections.Generic;
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
public class SpellController : MonoBehaviour
{
    [SerializeField] private GameObject[] currentSpellPrefabArray = new GameObject[3];
    [SerializeField] private SpellInfo[] currentSpellInfoList = new SpellInfo[3];
    [SerializeField] private Player player;
    [SerializeField] private float[] restTimeCurrentSpellArray = new float[3];

    [SerializeField] private GameObject currentCastingSpellObject;

    private void Update()
    {
        // ��Ÿ�� ����
        for (ushort i = 0; i < currentSpellPrefabArray.Length; i++)
        {
            Cooltime(i);
        }        
    }

    /// <summary>
    /// ���� ��Ÿ�� ����
    /// </summary>
    private void Cooltime(ushort spellIndex)
    {
        //Debug.Log($"spellNumber : {spellIndex}, currentSpellPrefabArray.Length : {currentSpellPrefabArray.Length}");
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
    }

    #region ���� ������ ���� ����
    public void StartCastingSpell(int spellIndex)
    {
        /*        // ��ų �ߵ� Ű �Է� ����
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
            Debug.Log($"���� {currentSpellInfoList[spellIndex].spellName}�� ���� �����Ұ������Դϴ�.");
            return;
        }

        currentSpellPrefabArray[spellIndex].GetComponent<Spell>().CastSpell(currentSpellInfoList[spellIndex],player.GetComponent<NetworkObject>());
    }

    /// <summary>
    /// ĳ�������� ���� �߻�
    /// </summary>
    public void ShootCurrentCastingSpell(ulong spellIndex)
    {
        SpellManager.Instance.ShootSpellObject() ; 
        currentSpellInfoList[spellIndex].castAble = false;
    }
    #endregion

    #region ���� ���� restTime/coolTime ���
    public float GetCurrentSpellCoolTimeRatio(int spellIndex)
    {
        if (currentSpellInfoList[spellIndex] == null) return 0f;
        float coolTimeRatio = restTimeCurrentSpellArray[spellIndex] / currentSpellInfoList[spellIndex].coolTime;
        return coolTimeRatio;
    }
    #endregion

    #region ���� ���� ���� ����
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
