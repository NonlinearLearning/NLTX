namespace Terraria.Player;

public sealed class ManaActivityPolicyDefinition
{
  public static ManaActivityPolicyDefinition Default { get; } =
    new(
      manaSickTime: 300,
      manaSickLessDamage: 0.25f,
      afkTimeNeededForNoWormSpawns: 300,
      afkTimeNeededForNoLuckyStars: 10800);

  public ManaActivityPolicyDefinition(
    int manaSickTime,
    float manaSickLessDamage,
    int afkTimeNeededForNoWormSpawns,
    int afkTimeNeededForNoLuckyStars)
  {
    if (manaSickTime <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(manaSickTime));
    }

    if (!float.IsFinite(manaSickLessDamage) ||
      manaSickLessDamage < 0f ||
      manaSickLessDamage > 1f)
    {
      throw new ArgumentOutOfRangeException(nameof(manaSickLessDamage));
    }

    if (afkTimeNeededForNoWormSpawns < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(afkTimeNeededForNoWormSpawns));
    }

    if (afkTimeNeededForNoLuckyStars < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(afkTimeNeededForNoLuckyStars));
    }

    ManaSickTime = manaSickTime;
    ManaSickLessDamage = manaSickLessDamage;
    AfkTimeNeededForNoWormSpawns = afkTimeNeededForNoWormSpawns;
    AfkTimeNeededForNoLuckyStars = afkTimeNeededForNoLuckyStars;
  }

  public int ManaSickTime { get; }

  public float ManaSickLessDamage { get; }

  public int AfkTimeNeededForNoWormSpawns { get; }

  public int AfkTimeNeededForNoLuckyStars { get; }

  public float CalculateManaSickReduction(bool isActive, int buffTime)
  {
    if (!isActive || buffTime <= 0)
    {
      return 0f;
    }

    int boundedBuffTime = Math.Min(buffTime, ManaSickTime);
    return ManaSickLessDamage * boundedBuffTime / ManaSickTime;
  }
}
