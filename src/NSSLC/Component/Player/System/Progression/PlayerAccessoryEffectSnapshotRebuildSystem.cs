namespace Terraria.Player.Progression;

public static class PlayerAccessoryEffectSnapshotRebuildSystem
{
  // Robe and mirror-luck flags have distinct source lifecycles.
  public static void Rebuild(
    in PlayerAccessoryEffectSnapshotRebuildInput input,
    PlayerAccessoryEffectSnapshotComponent snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);

    snapshot.FlowerBoots = input.FlowerBoots;
    snapshot.FairyBoots = input.FairyBoots;
    snapshot.HellfireTreads = input.HellfireTreads;
    snapshot.MoonLordLegs = input.MoonLordLegs;
    snapshot.DeadMansSweater = input.DeadMansSweater;
    snapshot.ArcticDivingGear = input.ArcticDivingGear;
    snapshot.CoolWhipBuff = input.CoolWhipBuff;
    snapshot.CobWhipBuff = input.CobWhipBuff;
    snapshot.MagicCuffs = input.MagicCuffs;
    snapshot.ColdDash = input.ColdDash;
    snapshot.SailDash = input.SailDash;
    snapshot.DesertDash = input.DesertDash;
    snapshot.DesertBoots = input.DesertBoots;
    snapshot.EyeSpring = input.EyeSpring;
    snapshot.Scope = input.Scope;
  }

  public static void ResetTickFlags(PlayerAccessoryEffectSnapshotComponent snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);

    snapshot.FlowerBoots = false;
    snapshot.FairyBoots = false;
    snapshot.HellfireTreads = false;
    snapshot.MoonLordLegs = false;
    snapshot.DeadMansSweater = false;
    snapshot.ArcticDivingGear = false;
    snapshot.CoolWhipBuff = false;
    snapshot.CobWhipBuff = false;
    snapshot.MagicCuffs = false;
    snapshot.ColdDash = false;
    snapshot.SailDash = false;
    snapshot.DesertDash = false;
    snapshot.DesertBoots = false;
    snapshot.EyeSpring = false;
    snapshot.Scope = false;
  }
}
