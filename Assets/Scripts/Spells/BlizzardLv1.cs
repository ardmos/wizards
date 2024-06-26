using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlizzardLv1 : AoESpell
{
    public override void InitAoESpell(SpellInfo spellInfoFromServer)
    {
        base.InitAoESpell(spellInfoFromServer);

        업그레이드효과적용();

        StartCoroutine(DestroyAfterDelay(spellInfo.lifeTime));
    }

    private void 업그레이드효과적용()
    {
        foreach (BlizzardUpgradeOption upgradeOption in System.Enum.GetValues(typeof(BlizzardUpgradeOption)))
        {
            if (spellInfo.upgradeOptions[(int)upgradeOption] != 0)
            {
                switch (upgradeOption)
                {
                    case BlizzardUpgradeOption.IncreaseDuration:
                        // 블리자드의 지속시간이 1초 증가합니다
                        spellInfo.lifeTime += spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case BlizzardUpgradeOption.IncreaseDamage:
                        // "블리자드의 초당 공격력이 1만큼 증가합니다"
                        spellInfo.damage += spellInfo.upgradeOptions[(int)upgradeOption];
                        break;
                    case BlizzardUpgradeOption.IncreaseSlowSpeed:
                        // "블리자드의 감속효과가 20%만큼 증가합니다"
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
