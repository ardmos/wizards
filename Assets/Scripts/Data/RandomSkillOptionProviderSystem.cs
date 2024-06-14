using System;
using System.Collections.Generic;
using System.Linq;

public enum FireballUpgradeOption
{
    IncreaseSize,
    AddDotDamage,
    IncreaseExplosionRadius,
    ReduceCooldown,
    AddPiercing
}

public enum WaterballUpgradeOption
{
    IncreaseSpeed,
    IncreaseHomingRange,
    AddSplashDamage,
    ReduceCooldown,
    IncreaseRange
}

public interface ISkillUpgradeOption
{
    string GetDescription();
}

public class FireballUpgrade : ISkillUpgradeOption
{
    private readonly FireballUpgradeOption _option;

    public FireballUpgrade(FireballUpgradeOption option)
    {
        _option = option;
    }

    public string GetDescription()
    {
        return _option switch
        {
            FireballUpgradeOption.IncreaseSize => "파이어볼의 크기가 10% 증가합니다.",
            FireballUpgradeOption.AddDotDamage => "파이어볼에 맞은 적이 5초 동안 초당 1의 화염 피해를 입습니다.",
            FireballUpgradeOption.IncreaseExplosionRadius => "파이어볼의 폭발 범위를 1 증가시킵니다.",
            FireballUpgradeOption.ReduceCooldown => "파이어볼의 재사용 대기 시간이 20% 감소합니다.",
            FireballUpgradeOption.AddPiercing => "파이어볼이 적을 관통합니다.",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public class WaterballUpgrade : ISkillUpgradeOption
{
    private readonly WaterballUpgradeOption _option;

    public WaterballUpgrade(WaterballUpgradeOption option)
    {
        _option = option;
    }

    public string GetDescription()
    {
        return _option switch
        {
            WaterballUpgradeOption.IncreaseSpeed => "워터볼의 속도가 30% 증가합니다.",
            WaterballUpgradeOption.IncreaseHomingRange => "워터볼의 유도 타겟 인식 범위가 20% 증가합니다.",
            WaterballUpgradeOption.AddSplashDamage => "워터볼이 적중 시 주변에 범위 피해를 입힙니다.",
            WaterballUpgradeOption.ReduceCooldown => "워터볼의 재사용 대기 시간이 20% 감소합니다.",
            WaterballUpgradeOption.IncreaseRange => "워터볼의 사거리가 50% 증가합니다.", // 유지 시간 증가
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public static class RandomSkillOptionProviderSystem
{
    private static readonly Random Random = new Random();

    public static List<ISkillUpgradeOption> GetRandomSkillUpgrades()
    {
        if (Random.Next(2) == 0)
        {
            return GetRandomFireballUpgrades().Cast<ISkillUpgradeOption>().ToList();
        }
        else
        {
            return GetRandomWaterballUpgrades().Cast<ISkillUpgradeOption>().ToList();
        }
    }

    private static List<ISkillUpgradeOption> GetRandomFireballUpgrades()
    {
        var options = Enum.GetValues(typeof(FireballUpgradeOption))
                          .Cast<FireballUpgradeOption>()
                          .Select(o => new FireballUpgrade(o))
                          .Cast<ISkillUpgradeOption>()
                          .ToList();

        return GetRandomUpgrades(options);
    }

    private static List<ISkillUpgradeOption> GetRandomWaterballUpgrades()
    {
        var options = Enum.GetValues(typeof(WaterballUpgradeOption))
                          .Cast<WaterballUpgradeOption>()
                          .Select(o => new WaterballUpgrade(o))
                          .Cast<ISkillUpgradeOption>()
                          .ToList();

        return GetRandomUpgrades(options);
    }

    private static List<T> GetRandomUpgrades<T>(List<T> options)
    {
        var selectedOptions = new List<T>();

        for (int i = 0; i < 3; i++)
        {
            var randomIndex = Random.Next(options.Count);
            selectedOptions.Add(options[randomIndex]);
            options.RemoveAt(randomIndex);
        }

        return selectedOptions;
    }
}