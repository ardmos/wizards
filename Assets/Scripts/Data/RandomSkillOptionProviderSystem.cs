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
            FireballUpgradeOption.IncreaseSize => "���̾�� ũ�Ⱑ 10% �����մϴ�.",
            FireballUpgradeOption.AddDotDamage => "���̾�� ���� ���� 5�� ���� �ʴ� 1�� ȭ�� ���ظ� �Խ��ϴ�.",
            FireballUpgradeOption.IncreaseExplosionRadius => "���̾�� ���� ������ 1 ������ŵ�ϴ�.",
            FireballUpgradeOption.ReduceCooldown => "���̾�� ���� ��� �ð��� 20% �����մϴ�.",
            FireballUpgradeOption.AddPiercing => "���̾�� ���� �����մϴ�.",
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
            WaterballUpgradeOption.IncreaseSpeed => "���ͺ��� �ӵ��� 30% �����մϴ�.",
            WaterballUpgradeOption.IncreaseHomingRange => "���ͺ��� ���� Ÿ�� �ν� ������ 20% �����մϴ�.",
            WaterballUpgradeOption.AddSplashDamage => "���ͺ��� ���� �� �ֺ��� ���� ���ظ� �����ϴ�.",
            WaterballUpgradeOption.ReduceCooldown => "���ͺ��� ���� ��� �ð��� 20% �����մϴ�.",
            WaterballUpgradeOption.IncreaseRange => "���ͺ��� ��Ÿ��� 50% �����մϴ�.", // ���� �ð� ����
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