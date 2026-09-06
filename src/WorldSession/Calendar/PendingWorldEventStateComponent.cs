using System;

namespace Terraria.WorldSession.Calendar;

public sealed class PendingWorldEventStateComponent
{
  public PendingWorldEventStateComponent(
    bool spawnEye = false,
    int spawnHardBoss = 0,
    bool spawnMeteor = false,
    int meteorShowerCount = 0,
    bool afterPartyOfDoom = false,
    bool forceHalloweenForToday = false,
    bool forceChristmasForToday = false)
  {
    SpawnEye = spawnEye;
    SpawnHardBoss = spawnHardBoss;
    SpawnMeteor = spawnMeteor;
    MeteorShowerCount = meteorShowerCount;
    AfterPartyOfDoom = afterPartyOfDoom;
    ForceHalloweenForToday = forceHalloweenForToday;
    ForceChristmasForToday = forceChristmasForToday;
    Validate();
  }

  public bool SpawnEye;
  public int SpawnHardBoss;
  public bool SpawnMeteor;
  public int MeteorShowerCount;
  public bool AfterPartyOfDoom;
  public bool ForceHalloweenForToday;
  public bool ForceChristmasForToday;

  public void Validate()
  {
    ArgumentOutOfRangeException.ThrowIfNegative(SpawnHardBoss);
    ArgumentOutOfRangeException.ThrowIfNegative(MeteorShowerCount);
  }
}
