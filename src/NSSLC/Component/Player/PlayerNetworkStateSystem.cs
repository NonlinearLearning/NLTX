using System.Numerics;

using Terraria.Player.Environment;

namespace Terraria.Player;

/// <summary>Detached results and state updates used by the network player owner.</summary>
public static class PlayerNetworkStateSystem
{
  public const int MinimumLifeMaximum = 20;
  public const int NetworkBuffDurationTicks = 60;

  public static bool ApplyIdentity(
    PlayerIdentityComponent identity,
    PlayerNetworkStateComponent network,
    PlayerAppearanceCustomizationComponent appearance,
    in PlayerNetworkIdentityInput input)
  {
    ArgumentNullException.ThrowIfNull(identity);
    ArgumentNullException.ThrowIfNull(network);
    ArgumentNullException.ThrowIfNull(appearance);

    string characterName = input.CharacterName.Trim();
    if (characterName.Length == 0 || characterName.Length > 20 ||
        !float.IsFinite(input.VoicePitchOffset) ||
        !Enum.IsDefined(input.Difficulty))
    {
      return false;
    }

    identity.CharacterName = characterName;
    identity.Difficulty = input.Difficulty;
    network.SetIdentityNetworkState(
      input.SkinVariant,
      input.VoiceVariant,
      input.VoicePitchOffset,
      input.HiddenAccessories,
      input.HideMiscBits,
      input.DifficultyAndAccessoryFlags,
      input.BiomeAndCartFlags,
      input.PermanentUpgradeFlags);
    _ = new PlayerAppearanceCustomizationSystem().Update(
      appearance,
      new PlayerAppearanceCustomizationInput(
        input.HairDye,
        0,
        input.HairColor,
        input.SkinColor,
        input.EyeColor,
        input.ShirtColor,
        input.UnderShirtColor,
        input.PantsColor,
        input.ShoeColor,
        input.Hair));
    return true;
  }

  public static void ApplyIdentitySelection(
    PlayerAppearanceSelectionComponent selection,
    in PlayerNetworkIdentityInput input)
  {
    ArgumentNullException.ThrowIfNull(selection);
    selection.VoiceOverride = unchecked((sbyte)input.VoiceVariant);
    selection.HideMiscBits = input.HideMiscBits;
    for (int index = 0; index < selection.HiddenVisibleAccessories.Length; index++)
    {
      selection.HiddenVisibleAccessories[index] =
        (input.HiddenAccessories & (1u << index)) != 0;
    }
  }

  public static bool ApplyLifeMana(
    PlayerVitalStateComponent vitals,
    int life,
    int maximumLife)
  {
    ArgumentNullException.ThrowIfNull(vitals);
    if (maximumLife < MinimumLifeMaximum || life < 0 || life > maximumLife)
    {
      return false;
    }

    vitals.StatLifeMax = maximumLife;
    vitals.StatLifeMax2 = maximumLife;
    vitals.StatLife = life;
    return true;
  }

  public static bool ApplyMana(
    PlayerVitalStateComponent vitals,
    int mana,
    int maximumMana)
  {
    ArgumentNullException.ThrowIfNull(vitals);
    // Packet 42 carries the base maximum, which excludes equipment and buff bonuses.
    if (maximumMana < 0 || mana < 0)
    {
      return false;
    }

    vitals.StatManaMax = maximumMana;
    vitals.StatManaMax2 = maximumMana;
    vitals.StatMana = mana;
    return true;
  }

  public static int ApplyHeal(PlayerVitalStateComponent vitals, int requestedAmount)
  {
    ArgumentNullException.ThrowIfNull(vitals);
    return Math.Max(0, requestedAmount);
  }

  /// <summary>Applies a positive heal to the formal life value and clamps it to StatLifeMax2.</summary>
  public static int ApplyHealAndClamp(
    PlayerVitalStateComponent vitals,
    int requestedAmount)
  {
    ArgumentNullException.ThrowIfNull(vitals);
    int amount = ApplyHeal(vitals, requestedAmount);
    int previousLife = vitals.StatLife;
    vitals.StatLife = Math.Min(vitals.StatLifeMax2, previousLife + amount);
    return vitals.StatLife - previousLife;
  }

  public static bool ApplyBuffs(
    PlayerBuffSlotsComponent buffs,
    IReadOnlyList<ushort> types)
  {
    ArgumentNullException.ThrowIfNull(buffs);
    ArgumentNullException.ThrowIfNull(types);
    if (types.Count > PlayerBuffSlotsComponent.MaximumSlotCount)
    {
      return false;
    }

    buffs.ReplaceNetworkSlots(types, NetworkBuffDurationTicks);
    return true;
  }

  public static bool ApplyPvpBuff(
    PlayerBuffComponent buffs,
    PlayerNetworkStateComponent network,
    ushort buffType,
    int buffTime)
  {
    ArgumentNullException.ThrowIfNull(buffs);
    ArgumentNullException.ThrowIfNull(network);
    if (buffTime <= 0)
    {
      return false;
    }

    buffs.SetNetworkBuff(buffType, buffTime);
    network.SetPvPBuff(buffType, true);
    return true;
  }

  public static void ApplyZone(
    PlayerZoneAndEnvironmentStateComponent environment,
    byte zone1,
    byte zone2,
    byte zone3,
    byte zone4,
    byte zone5,
    byte townNpcCount)
  {
    ArgumentNullException.ThrowIfNull(environment);
    environment.ApplyNetworkZones(zone1, zone2, zone3, zone4, zone5);
  }

  public static void ApplyTownNpcCount(
    PlayerNetworkStateComponent network,
    byte townNpcCount)
  {
    ArgumentNullException.ThrowIfNull(network);
    network.SetTownNpcCount(townNpcCount);
  }

  public static void ApplySpawnDeathCounts(
    PlayerDeathRecordComponent deathRecord,
    int pveDeathCount,
    int pvpDeathCount)
  {
    ArgumentNullException.ThrowIfNull(deathRecord);
    if (pveDeathCount < 0 || pvpDeathCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(pveDeathCount));
    }

    deathRecord.PveDeathCount = pveDeathCount;
    deathRecord.PvpDeathCount = pvpDeathCount;
  }

  public static bool ApplyStealth(PlayerNetworkStateComponent network, float stealth)
  {
    ArgumentNullException.ThrowIfNull(network);
    if (!float.IsFinite(stealth) || stealth < 0.0f || stealth > 1.0f)
    {
      return false;
    }

    network.SetStealth(stealth);
    return true;
  }

  public static bool ApplyItemAnimation(
    PlayerNetworkStateComponent network,
    float itemRotation,
    int itemAnimation,
    byte channel)
  {
    ArgumentNullException.ThrowIfNull(network);
    if (!float.IsFinite(itemRotation) || itemAnimation < 0)
    {
      return false;
    }

    network.SetItemAnimation(itemRotation, itemAnimation, channel);
    return true;
  }

  public static bool ApplyMinionRestTarget(
    PlayerNetworkStateComponent network,
    Vector2 target)
  {
    ArgumentNullException.ThrowIfNull(network);
    if (!float.IsFinite(target.X) || !float.IsFinite(target.Y))
    {
      return false;
    }

    network.SetMinionRestTarget(target);
    return true;
  }

  public static bool ApplyMinionAttackTarget(
    PlayerNetworkStateComponent network,
    int npcSlot)
  {
    ArgumentNullException.ThrowIfNull(network);
    if (npcSlot < -1)
    {
      return false;
    }

    network.SetMinionAttackTarget(npcSlot);
    return true;
  }

  public static bool ApplyHostile(PlayerNetworkStateComponent network, bool hostile)
  {
    ArgumentNullException.ThrowIfNull(network);
    network.SetHostile(hostile);
    return true;
  }

  public static bool ApplyTalkNpc(PlayerNetworkStateComponent network, int npcSlot)
  {
    ArgumentNullException.ThrowIfNull(network);
    if (npcSlot < -1)
    {
      return false;
    }

    network.SetTalkNpc(npcSlot);
    return true;
  }

  public static bool BeginUnacknowledgedTeleport(
    PlayerNetworkStateComponent network,
    Vector2 position)
  {
    ArgumentNullException.ThrowIfNull(network);
    if (!float.IsFinite(position.X) || !float.IsFinite(position.Y))
    {
      return false;
    }

    network.MarkTeleportRequested(position);
    return true;
  }

  public static void AcknowledgeTeleport(PlayerNetworkStateComponent network)
  {
    ArgumentNullException.ThrowIfNull(network);
    network.AcknowledgeTeleport();
  }
}

