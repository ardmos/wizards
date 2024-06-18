using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

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

public enum BlizzardUpgradeOption
{
    IncreaseSlowSpeed,
    IncreaseDamage,
    IncreaseDuration
}

[System.Serializable]
public class SkillUpgradeOptionDTO : INetworkSerializable
{
    public string SkillType; // Fireball, Waterball, Blizzard ��
    public string UpgradeType; // IncreaseSize, AddDotDamage ��

    public bool Equals(SkillUpgradeOptionDTO other)
    {
        return
        SkillType == other.SkillType &&
        UpgradeType == other.UpgradeType;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        /// ���⼭����!  ISkillUpgradeOption ����ϴ� ������� ������
    }

    public static class SkillUpgradeOptionExtensions
{
    public static SkillUpgradeOptionDTO ToDTO(this ISkillUpgradeOption upgradeOption)
    {
        switch (upgradeOption)
        {
            case FireballUpgrade fireballUpgrade:
                return fireballUpgrade.ToDTO();
            case WaterballUpgrade waterballUpgrade:
                return waterballUpgrade.ToDTO();
            case BlizzardUpgrade blizzardUpgrade:
                return blizzardUpgrade.ToDTO();
            default:
                throw new ArgumentException("Unknown skill upgrade option type");
        }
    }

    public static List<SkillUpgradeOptionDTO> ToDTOList(this List<ISkillUpgradeOption> upgradeOptions)
    {
        return upgradeOptions.Select(option => option.ToDTO()).ToList();
    }
}

public interface ISkillUpgradeOption
{
    public string GetName();
    public string GetDescription();
    public Sprite GetIcon();
}

public class FireballUpgrade : ISkillUpgradeOption
{
    private readonly FireballUpgradeOption _option;

    public FireballUpgrade(FireballUpgradeOption option)
    {
        _option = option;
    }
    public string GetName()
    {
        return _option switch
        {
            FireballUpgradeOption.IncreaseSize => "Increase Size",
            FireballUpgradeOption.AddDotDamage => "Add dot damage",
            FireballUpgradeOption.IncreaseExplosionRadius => "Increase Explosion Radius",
            FireballUpgradeOption.ReduceCooldown => "Reduce cooldown",
            FireballUpgradeOption.AddPiercing => "Add piercing",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    public string GetDescription()
    {
        return _option switch
        {
            FireballUpgradeOption.IncreaseSize => "Increases the fireball's size by 10%.",
            FireballUpgradeOption.AddDotDamage => "Enemies hit by the fireball take 1 fire damage per second for 5 seconds.",
            FireballUpgradeOption.IncreaseExplosionRadius => "Increases the fireball's explosion radius by 1.",
            FireballUpgradeOption.ReduceCooldown => "Reduces the fireball's cooldown by 20%.",
            FireballUpgradeOption.AddPiercing => "The fireball pierces through enemies.",
            /*            FireballUpgradeOption.IncreaseSize => "���̾�� ũ�Ⱑ 10% �����մϴ�.",
                        FireballUpgradeOption.AddDotDamage => "���̾�� ���� ���� 5�� ���� �ʴ� 1�� ȭ�� ���ظ� �Խ��ϴ�.",
                        FireballUpgradeOption.IncreaseExplosionRadius => "���̾�� ���� ������ 1 ������ŵ�ϴ�.",
                        FireballUpgradeOption.ReduceCooldown => "���̾�� ���� ��� �ð��� 20% �����մϴ�.",
                        FireballUpgradeOption.AddPiercing => "���̾�� ���� �����մϴ�.",*/
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Sprite GetIcon()
    {
        return GameAssetsManager.Instance.GetSpellIconImage(SkillName.FireBallLv1);
    }

    public SkillUpgradeOptionDTO ToDTO()
    {
        return new SkillUpgradeOptionDTO
        {
            SkillType = "Fireball",
            UpgradeType = _option.ToString()
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

    public string GetName()
    {
        return _option switch
        {
            WaterballUpgradeOption.IncreaseSpeed => "Increase speed",
            WaterballUpgradeOption.IncreaseHomingRange => "Increase homing range",
            WaterballUpgradeOption.AddSplashDamage => "Add splash damage",
            WaterballUpgradeOption.ReduceCooldown => "Reduce cooldown",
            WaterballUpgradeOption.IncreaseRange => "Increase range", 
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public string GetDescription()
    {
        return _option switch
        {
            WaterballUpgradeOption.IncreaseSpeed => "Increases the waterball's speed by 30%.",
            WaterballUpgradeOption.IncreaseHomingRange => "Increases the waterball's homing target recognition range by 20%.",
            WaterballUpgradeOption.AddSplashDamage => "The waterball deals area damage upon impact.",
            WaterballUpgradeOption.ReduceCooldown => "Reduces the waterball's cooldown by 20%.",
            WaterballUpgradeOption.IncreaseRange => "Increases the waterball's range by 50%.", // Increase duration

            /*            WaterballUpgradeOption.IncreaseSpeed => "���ͺ��� �ӵ��� 30% �����մϴ�.",
                        WaterballUpgradeOption.IncreaseHomingRange => "���ͺ��� ���� Ÿ�� �ν� ������ 20% �����մϴ�.",
                        WaterballUpgradeOption.AddSplashDamage => "���ͺ��� ���� �� �ֺ��� ���� ���ظ� �����ϴ�.",
                        WaterballUpgradeOption.ReduceCooldown => "���ͺ��� ���� ��� �ð��� 20% �����մϴ�.",
                        WaterballUpgradeOption.IncreaseRange => "���ͺ��� ��Ÿ��� 50% �����մϴ�.", // ���� �ð� ����*/
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Sprite GetIcon()
    {
        return GameAssetsManager.Instance.GetSpellIconImage(SkillName.WaterBallLv1);
    }

    public SkillUpgradeOptionDTO ToDTO()
    {
        return new SkillUpgradeOptionDTO
        {
            SkillType = "Waterball",
            UpgradeType = _option.ToString()
        };
    }
}

public class BlizzardUpgrade : ISkillUpgradeOption
{
    private readonly BlizzardUpgradeOption _option;

    public BlizzardUpgrade(BlizzardUpgradeOption option)
    {
        _option = option;
    }

    public string GetName()
    {
        return _option switch
        {
            BlizzardUpgradeOption.IncreaseDuration => "Increase duration",
            BlizzardUpgradeOption.IncreaseDamage => "Increase damage",
            BlizzardUpgradeOption.IncreaseSlowSpeed => "Increase slow speed",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public string GetDescription()
    {
        return _option switch
        {
            BlizzardUpgradeOption.IncreaseDuration => "Increases the blizzard's duration by 1 second.",
            BlizzardUpgradeOption.IncreaseDamage => "Increases the blizzard's damage per second by 1.",
            BlizzardUpgradeOption.IncreaseSlowSpeed => "Increases the blizzard's slowing effect by 20%.",

            /*            BlizzardUpgradeOption.IncreaseDuration => "���ڵ��� ���ӽð��� 1�� �����մϴ�",
                        BlizzardUpgradeOption.IncreaseDamage => "���ڵ��� �ʴ� ���ݷ��� 1��ŭ �����մϴ�",
                        BlizzardUpgradeOption.IncreaseSlowSpeed => "���ڵ��� ����ȿ���� 20%��ŭ �����մϴ�",*/
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public Sprite GetIcon()
    {
        return GameAssetsManager.Instance.GetSpellIconImage(SkillName.BlizzardLv1);
    }

    public SkillUpgradeOptionDTO ToDTO()
    {
        return new SkillUpgradeOptionDTO
        {
            SkillType = "Blizzard",
            UpgradeType = _option.ToString()
        };
    }
}

public static class RandomSkillOptionProviderSystem
{
    private static readonly Random Random = new Random();

    public static List<SkillUpgradeOptionDTO> GetRandomSkillUpgrades()
    {
        var skillUpgrades = Random.Next(3) switch
        {
            0 => GetRandomFireballUpgrades(),
            1 => GetRandomWaterballUpgrades(),
            2 => GetRandomBlizzardUpgrades(),
            _ => throw new ArgumentOutOfRangeException()
        };

        return skillUpgrades.ToDTOList();
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

    private static List<ISkillUpgradeOption> GetRandomBlizzardUpgrades()
    {
        var options = Enum.GetValues(typeof(BlizzardUpgradeOption))
                          .Cast<BlizzardUpgradeOption>()
                          .Select(o => new BlizzardUpgrade(o))
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

public static class SkillUpgradeFactory
{
    public static ISkillUpgradeOption FromDTO(SkillUpgradeOptionDTO dto)
    {
        return dto.SkillType switch
        {
            "Fireball" => new FireballUpgrade(Enum.Parse<FireballUpgradeOption>(dto.UpgradeType)),
            "Waterball" => new WaterballUpgrade(Enum.Parse<WaterballUpgradeOption>(dto.UpgradeType)),
            "Blizzard" => new BlizzardUpgrade(Enum.Parse<BlizzardUpgradeOption>(dto.UpgradeType)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static List<ISkillUpgradeOption> FromDTOList(List<SkillUpgradeOptionDTO> dtoList)
    {
        return dtoList.Select(dto => FromDTO(dto)).ToList();
    }
}