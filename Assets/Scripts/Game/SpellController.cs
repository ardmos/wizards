using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// �÷��̾� ĳ���� ������Ʈ�� ���̴� ��ũ��Ʈ
/// !!! ���� ���
/// 1. ���� ���� ��Ȳ ����
/// 2. ��ų �ߵ� 
/// </summary>
public class SpellController : MonoBehaviour
{
    [SerializeField]
    private Spell currentSpell;
    public Transform muzzle;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // �߻��� Player.cs�� SpawnProjectiles.cs���� Ȯ���ؼ� �����ϱ�. 
    // Spell.cs�� �� ��ũ��Ʈ. strategy pattern 
}
