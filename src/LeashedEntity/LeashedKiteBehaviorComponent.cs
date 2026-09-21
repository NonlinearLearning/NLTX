namespace Terraria.LeashedEntity;

/// <summary>
/// Stores kite-only behavior state for one leashed entity.
/// status: implemented
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
  /// The Projectile compatibility state is intentionally owned by a separate adapter.
  /// </summary>
  public LeashedKiteBehaviorComponent(
    int? projectileType,
    float rotation,
    float kiteDistance,
    float windTarget,
    float windCurrent,
    float timeCounter,
    int timeWithoutWind)
  {
    ProjectileType = projectileType;
    Rotation = rotation;
    KiteDistance = kiteDistance;
    WindTarget = windTarget;
    WindCurrent = windCurrent;
    TimeCounter = timeCounter;
    TimeWithoutWind = timeWithoutWind;
  }
}
