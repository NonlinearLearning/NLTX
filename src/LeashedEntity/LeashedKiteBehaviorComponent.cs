namespace Terraria.LeashedEntity;

/// <summary>
/// Stores kite-only behavior state for one leashed entity.
/// status: proposed
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
public struct LeashedKiteBehaviorComponent
{
  /// <summary>
  /// Source-confirmed candidate initialization from the complete LeashedKite field.
  /// </summary>
  public const float DefaultKiteDistance = 250.0f;

  /// <summary>
  /// Projectile content id, not a projectile runtime entity id.
  /// </summary>
  public int? ProjectileType;

  /// <summary>
  /// Rotation candidate; authority versus presentation ownership is unresolved.
  /// </summary>
  public float Rotation;

  /// <summary>
  /// Kite distance in the anchor/world distance unit.
  /// </summary>
  public float KiteDistance;

  /// <summary>
  /// Kite wind target candidate.
  /// </summary>
  public float WindTarget;

  /// <summary>
  /// Kite wind current candidate.
  /// </summary>
  public float WindCurrent;

  /// <summary>
  /// Kite behavior timer candidate, not a global tick.
  /// </summary>
  public float TimeCounter;

  /// <summary>
  /// No-wind timer candidate.
  /// </summary>
  public int TimeWithoutWind;

  /// <summary>
  /// Kite-local projectile-like AI state, not ordinary projectile authority.
  /// </summary>
  public float ProjectileLocalAI0;

  /// <summary>
  /// Kite-local projectile-like AI state, not ordinary projectile authority.
  /// </summary>
  public float ProjectileLocalAI1;

  public LeashedKiteBehaviorComponent(
    int? projectileType,
    float rotation,
    float kiteDistance,
    float windTarget,
    float windCurrent,
    float timeCounter,
    int timeWithoutWind,
    float projectileLocalAI0,
    float projectileLocalAI1)
  {
    ProjectileType = projectileType;
    Rotation = rotation;
    KiteDistance = kiteDistance;
    WindTarget = windTarget;
    WindCurrent = windCurrent;
    TimeCounter = timeCounter;
    TimeWithoutWind = timeWithoutWind;
    ProjectileLocalAI0 = projectileLocalAI0;
    ProjectileLocalAI1 = projectileLocalAI1;
  }
}
