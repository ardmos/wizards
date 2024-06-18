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
    public string SkillType; // Fireball, Waterball, Blizzard 등
    public string UpgradeType; // IncreaseSize, AddDotDamage 등

    public bool Equals(SkillUpgradeOptionDTO other)
    {
        return
        SkillType == other.SkillType &&
        UpgradeType == other.UpgradeType;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        /// 여기서부터!  ISkillUpgradeOption 상속하는 방식으로 변경중
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
            /*            FireballUpgradeOption.IncreaseSize => "파이어볼의 크기가 10% 증가합니다.",
                        FireballUpgradeOption.AddDotDamage => "파이어볼에 맞은 적이 5초 동안 초당 1의 화염 피해를 입습니다.",
                        FireballUpgradeOption.IncreaseExplosionRadius => "파이어볼의 폭발 범위를 1 증가시킵니다.",
                        FireballUpgradeOption.ReduceCooldown => "파이어볼의 재사용 대기 시간이 20% 감소합니다.",
                        FireballUpgradeOption.AddPiercing => "파이어볼이 적을 관통합니다.",*/
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

            /*            WaterballUpgradeOption.IncreaseSpeed => "워터볼의 속도가 30% 증가합니다.",
                        WaterballUpgradeOption.IncreaseHomingRange => "워터볼의 유도 타겟 인식 범위가 20% 증가합니다.",
                        WaterballUpgradeOption.AddSplashDamage => "워터볼이 적중 시 주변에 범위 피해를 입힙니다.",
                        WaterballUpgradeOption.ReduceCooldown => "워터볼의 재사용 대기 시간이 20% 감소합니다.",
                        WaterballUpgradeOption.IncreaseRange => "워터볼의 사거리가 50% 증가합니다.", // 유지 시간 증가*/
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

            /*            BlizzardUpgradeOption.IncreaseDuration => "블리자드의 지속시간이 1초 증가합니다",
                        BlizzardUpgradeOption.IncreaseDamage => "블리자드의 초당 공격력이 1만큼 증가합니다",
                        BlizzardUpgradeOption.IncreaseSlowSpeed => "블리자드의 감속효과가 20%만큼 증가합니다",*/
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