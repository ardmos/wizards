using UnityEngine;
/// <summary>
/// 
/// 1���� ���̾ ��ũ��Ʈ�Դϴ�.
/// 
/// !!! ���� ���
/// 1. �� �ɷ�ġ ����
/// </summary>
public class FireBallLv1 : FireSpell
{

    /// <summary>
    /// 1. �� �ɷ�ġ ����(���� ���ÿ� Server���� �ο����ִ� �ɷ�ġ �Դϴ�.)
    /// </summary>
    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        //Debug.Log("InitSpellInfoDetail() FireBall Lv1");
        spellInfo = spellInfoFromServer;
    }

    // ���� ����ϴ� ���̾ VFX�� �ڿ������� �ϱ����� �κ�
    public override void OnNetworkSpawn()
    {
        trails[0].SetActive(false);
    }

    public override void Shoot(Vector3 force, ForceMode forceMode)
    {
        base.Shoot(force, forceMode);
        trails[0].SetActive(true);
        Debug.Log($"Fireball Lv1 Shoot");

    }
}
