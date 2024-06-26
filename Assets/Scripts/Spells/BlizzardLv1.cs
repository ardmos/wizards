using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlizzardLv1 : AoESpell
{
    public override void InitAoESpell(SpellInfo spellInfoFromServer)
    {
        base.InitAoESpell(spellInfoFromServer);

        ���׷��̵�ȿ������();

        StartCoroutine(DestroyAfterDelay(spellInfo.lifeTime));
    }

    private void ���׷��̵�ȿ������()
    {
        foreach (BlizzardUpgradeOption upgradeOption in System.Enum.GetValues(typeof(BlizzardUpgradeOption)))
        {
            if (spellInfo.upgradeOptions[(int)upgradeOption] != 0)
            {
                switch (upgradeOption)
                {
                    case BlizzardUpgradeOption.IncreaseDuration:
                        // ���ڵ��� ���ӽð��� 1�� �����մϴ�
                        spellInfo.lifeTime += spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case BlizzardUpgradeOption.IncreaseDamage:
                        // "���ڵ��� �ʴ� ���ݷ��� 1��ŭ �����մϴ�"
                        spellInfo.damage += spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case BlizzardUpgradeOption.IncreaseSlowSpeed:
                        // "���ڵ��� ����ȿ���� 20%��ŭ �����մϴ�"
                        slowValue += slowValue * (1f + 0.2f * spellInfo.upgradeOptions[(int)upgradeOption]);
                        Debug.Log($"new slowValue:{slowValue}");
                        break;
                }
            }
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }
}
