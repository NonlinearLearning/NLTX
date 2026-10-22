using System;

namespace MinimalImplementation.Server.Player;

/// <summary>Consequences of an already chosen dodge; this code never rolls a second chance.</summary>
public static class PlayerDodgeRules {
  public const byte Ninja = 1;
  public const byte Shadow = 2;
  public const byte Brain = 4;
  public const ushort ShadowDodgeBuff = 59;
  public const ushort BrainDodgeBuff = 321;
  public const int ShadowCooldownTicks = 1800;
  public const int BrainBuffTicks = 240;

  public static PlayerImmunities ApplyImmunity(
      PlayerImmunities immunities, byte dodgeType, bool longInvincibility) {
    int ticks = longInvincibility ? 120 : 80;
    return dodgeType switch {
      Ninja => immunities.ApplyNinjaDodge(ticks),
      Shadow => immunities.ApplyShadowDodge(ticks),
      Brain => immunities.ApplyBrainDodge(ticks),
      _ => immunities
    };
  }

  /// <summary>
  /// Mirrors ShadowDodge's controlling-player branch. Packet 62 reception on a dedicated
  /// server only applies immunity; a server-originated dodge also owns these consequences.
  /// </summary>
  public static PlayerImmunities ApplyShadowCooldown(PlayerImmunities immunities) {
    return immunities with { ShadowDodgeCooldownTicks = ShadowCooldownTicks };
  }

  public static PlayerBuffs ApplyControlledBuffs(
      PlayerBuffs buffs, byte dodgeType, PlayerBuffApplicationContext brainBuffContext) {
    ArgumentNullException.ThrowIfNull(buffs);
    ArgumentNullException.ThrowIfNull(brainBuffContext);
    if (dodgeType == Shadow) {
      return buffs.Remove(ShadowDodgeBuff);
    }

    // Original AddBuff can leave the list unchanged (immunity or no available slot).
    if (dodgeType == Brain && buffs.TryAddAuthoritative(
        BrainDodgeBuff, BrainBuffTicks, brainBuffContext, out PlayerBuffs updated)) {
      return updated;
    }

    return buffs;
  }

  public static bool IsRecognized(byte dodgeType) {
    return dodgeType is Ninja or Shadow or Brain;
  }
}
