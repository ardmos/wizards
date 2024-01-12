using UnityEngine;
/// <summary>
/// 1���� ���ͺ� ��ũ��Ʈ�Դϴ�.
/// 
/// !!! ���� ���
/// 1. �� �ɷ�ġ ����
/// 2. CollisionEnter �浹 ó��
/// </summary>
public class WaterBallLv1 : WaterSpell
{
    /// <summary>
    /// 1. �� �ɷ�ġ ����(���� ���ÿ� Server���� �ο����ִ� �ɷ�ġ �Դϴ�.)
    /// </summary>
    public override void InitSpellInfoDetail(SpellInfo spellInfoFromServer)
    {
        spellInfo = spellInfoFromServer;
    }

    /// <summary>
    /// 2. CollisionEnter �浹 ó��
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        SpellManager.Instance.SpellHitOnServer(collision, this);
    }
}
