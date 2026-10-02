namespace Terraria.Player.Environment;

// status: implemented
// componentId: PLAYER.COMP.ENVIRONMENT_DETECTION_AND_SPAWN_STATE
// source-members: P08-1261..P08-1265, P08-1269..P08-1274, P08-1282..P08-1283
// crossSubsystemOwner: integration-review
public sealed class PlayerEnvironmentDetectionAndSpawnStateComponent
{
  public bool KillGuide { get; internal set; }

  public bool KillClothier { get; internal set; }

  public float EquipmentBasedLuckBonus { get; internal set; }

  public float LastEquipmentBasedLuckBonus { get; internal set; }

  public bool HasCreditsSceneMusicBox { get; internal set; }

  public bool FindTreasure { get; internal set; }

  public bool BiomeSight { get; internal set; }

  public bool IsInvisible { get; internal set; }

  public bool DetectCreature { get; internal set; }

  public bool NightVision { get; internal set; }

  public bool EnemySpawns { get; internal set; }

  public bool InsideUnbreakableWalls { get; internal set; }

  public bool CanSeeInvisibleBlocks { get; internal set; }
}
