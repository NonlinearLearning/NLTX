using Terraria.Player.Progression;

var snapshot = new PlayerAccessoryEffectSnapshotComponent
{
  BrokenMirrorBadLuck = true,
  WearsRobe = true,
};
var input = new PlayerAccessoryEffectSnapshotRebuildInput
{
  FlowerBoots = true,
  FairyBoots = true,
  HellfireTreads = true,
  MoonLordLegs = true,
  DeadMansSweater = true,
  ArcticDivingGear = true,
  CoolWhipBuff = true,
  CobWhipBuff = true,
  MagicCuffs = true,
  ColdDash = true,
  SailDash = true,
  DesertDash = true,
  DesertBoots = true,
  EyeSpring = true,
  Scope = true,
};

PlayerAccessoryEffectSnapshotRebuildSystem.Rebuild(input, snapshot);
Assert(TickFlagsSet(snapshot), "Rebuild should copy all 15 tick flags.");

PlayerAccessoryEffectSnapshotRebuildSystem.ResetTickFlags(snapshot);
Assert(TickFlagsCleared(snapshot), "Reset should clear all 15 reset-cycle fields.");
Assert(
  snapshot.BrokenMirrorBadLuck && snapshot.WearsRobe,
  "Reset should preserve fields with separate source lifecycles.");

Console.WriteLine("PASS: accessory tick snapshot rebuild preserves separate-lifecycle fields.");

static bool TickFlagsSet(PlayerAccessoryEffectSnapshotComponent snapshot)
{
  return snapshot.FlowerBoots &&
    snapshot.FairyBoots &&
    snapshot.HellfireTreads &&
    snapshot.MoonLordLegs &&
    snapshot.DeadMansSweater &&
    snapshot.ArcticDivingGear &&
    snapshot.CoolWhipBuff &&
    snapshot.CobWhipBuff &&
    snapshot.MagicCuffs &&
    snapshot.ColdDash &&
    snapshot.SailDash &&
    snapshot.DesertDash &&
    snapshot.DesertBoots &&
    snapshot.EyeSpring &&
    snapshot.Scope;
}

static bool TickFlagsCleared(PlayerAccessoryEffectSnapshotComponent snapshot)
{
  return !snapshot.FlowerBoots &&
    !snapshot.FairyBoots &&
    !snapshot.HellfireTreads &&
    !snapshot.MoonLordLegs &&
    !snapshot.DeadMansSweater &&
    !snapshot.ArcticDivingGear &&
    !snapshot.CoolWhipBuff &&
    !snapshot.CobWhipBuff &&
    !snapshot.MagicCuffs &&
    !snapshot.ColdDash &&
    !snapshot.SailDash &&
    !snapshot.DesertDash &&
    !snapshot.DesertBoots &&
    !snapshot.EyeSpring &&
    !snapshot.Scope;
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
