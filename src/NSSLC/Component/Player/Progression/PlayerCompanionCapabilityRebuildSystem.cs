namespace Terraria.Player.Progression;

public static class PlayerCompanionCapabilityRebuildSystem
{
  // The caller resolves these flags; projectile/entity lifecycle remains a separate owner.
  public static void Rebuild(
    in PlayerCompanionCapabilityRebuildInput input,
    PlayerCompanionCapabilityComponent capability)
  {
    ArgumentNullException.ThrowIfNull(capability);

    capability.CompanionCube = input.CompanionCube;
    capability.BabyFaceMonster = input.BabyFaceMonster;
    capability.Snowman = input.Snowman;
    capability.Dino = input.Dino;
    capability.Skeletron = input.Skeletron;
    capability.Hornet = input.Hornet;
    capability.Zephyrfish = input.Zephyrfish;
    capability.Tiki = input.Tiki;
    capability.Parrot = input.Parrot;
    capability.Truffle = input.Truffle;
    capability.Sapling = input.Sapling;
    capability.CSapling = input.CSapling;
    capability.Wisp = input.Wisp;
    capability.Lizard = input.Lizard;
  }

  public static void Reset(PlayerCompanionCapabilityComponent capability)
  {
    ArgumentNullException.ThrowIfNull(capability);

    capability.CompanionCube = false;
    capability.BabyFaceMonster = false;
    capability.Snowman = false;
    capability.Dino = false;
    capability.Skeletron = false;
    capability.Hornet = false;
    capability.Zephyrfish = false;
    capability.Tiki = false;
    capability.Parrot = false;
    capability.Truffle = false;
    capability.Sapling = false;
    capability.CSapling = false;
    capability.Wisp = false;
    capability.Lizard = false;
  }
}
