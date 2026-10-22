using System;

namespace MinimalImplementation.Server.Player;

public readonly record struct PlayerLuck(
  int LadyBugLuckTime,
  float TorchLuck,
  byte LuckPotion,
  bool HasGardenGnome,
  bool BrokenMirrorBadLuck,
  float EquipmentLuckBonus,
  float CoinLuck,
  byte KiteLuckLevel,
  float CurrentLuck)
{
  public static PlayerLuck Empty => new(0, 0, 0, false, false, 0, 0, 0, 0);

  public PlayerLuck WithCurrentLuck(float currentLuck)
  {
    return this with { CurrentLuck = currentLuck };
  }

  /// <summary>Returns the luck factors after one authoritative coin pickup.</summary>
  public PlayerLuck WithCoinLuck(float coinAmount)
  {
    if (!float.IsFinite(coinAmount) || coinAmount <= 0f)
    {
      throw new ArgumentOutOfRangeException(nameof(coinAmount));
    }

    float updated = Math.Min(1_000_000f, CoinLuck + coinAmount);
    return this with { CoinLuck = updated };
  }

  public PlayerLuck WithAuthoritativeBuffs(
      PlayerBuffs buffs, PlayerBuffs? previousBuffs = null) {
    ArgumentNullException.ThrowIfNull(buffs);
    if (buffs.TryGetAuthoritativeTime(Version4PlayerBuffCatalog.LuckPotionBuffType,
        out int time)) {
      byte tier = time > 36_000 ? (byte)3 : time > 18_000 ? (byte)2 : (byte)1;
      return this with { LuckPotion = tier };
    }

    // Expiring an owned duration clears it. A remote type-only report cannot define a tier.
    if (previousBuffs is not null
        && previousBuffs.TryGetAuthoritativeTime(
          Version4PlayerBuffCatalog.LuckPotionBuffType, out _)) {
      return this with { LuckPotion = 0 };
    }

    return this;
  }

  public PlayerLuck AdvanceTick(int dayRate)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(dayRate);
    int ladyBugTime = LadyBugLuckTime > 0
      ? (int)Math.Max(0L, (long)LadyBugLuckTime - dayRate)
      : (int)Math.Min(0L, (long)LadyBugLuckTime + dayRate);
    float coinLuck = CoinLuck;
    if (coinLuck > 0f)
    {
      coinLuck *= (float)Math.Pow(0.9999, dayRate);
      if ((double)coinLuck < 0.25)
      {
        coinLuck = 0f;
      }
    }

    return this with { LadyBugLuckTime = ladyBugTime, CoinLuck = coinLuck };
  }
}
