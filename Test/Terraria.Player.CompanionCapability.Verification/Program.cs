using Terraria.Player.Progression;

var capability = new PlayerCompanionCapabilityComponent();
var input = new PlayerCompanionCapabilityRebuildInput
{
  CompanionCube = true,
  BabyFaceMonster = true,
  Snowman = true,
  Dino = true,
  Skeletron = true,
  Hornet = true,
  Zephyrfish = true,
  Tiki = true,
  Parrot = true,
  Truffle = true,
  Sapling = true,
  CSapling = true,
  Wisp = true,
  Lizard = true,
};

PlayerCompanionCapabilityRebuildSystem.Rebuild(input, capability);
Assert(AllFlagsSet(capability), "Rebuild should copy all 14 companion flags.");

PlayerCompanionCapabilityRebuildSystem.Reset(capability);
Assert(AllFlagsCleared(capability), "Reset should clear all 14 companion flags.");

Console.WriteLine("PASS: companion capability rebuild and reset.");

static bool AllFlagsSet(PlayerCompanionCapabilityComponent capability)
{
  return capability.CompanionCube &&
    capability.BabyFaceMonster &&
    capability.Snowman &&
    capability.Dino &&
    capability.Skeletron &&
    capability.Hornet &&
    capability.Zephyrfish &&
    capability.Tiki &&
    capability.Parrot &&
    capability.Truffle &&
    capability.Sapling &&
    capability.CSapling &&
    capability.Wisp &&
    capability.Lizard;
}

static bool AllFlagsCleared(PlayerCompanionCapabilityComponent capability)
{
  return !capability.CompanionCube &&
    !capability.BabyFaceMonster &&
    !capability.Snowman &&
    !capability.Dino &&
    !capability.Skeletron &&
    !capability.Hornet &&
    !capability.Zephyrfish &&
    !capability.Tiki &&
    !capability.Parrot &&
    !capability.Truffle &&
    !capability.Sapling &&
    !capability.CSapling &&
    !capability.Wisp &&
    !capability.Lizard;
}

static void Assert(bool condition, string message)
{
  if (!condition)
  {
    throw new InvalidOperationException(message);
  }
}
