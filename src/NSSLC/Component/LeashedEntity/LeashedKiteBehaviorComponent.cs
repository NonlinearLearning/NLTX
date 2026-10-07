namespace Terraria.LeashedEntity;

/// <summary>
/// Stores kite-only behavior state for one leashed entity.
/// status: implemented
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
/// <remarks>
/// <para>职责：保存拴系风筝的距离、风力、旋转和计时状态。</para>
/// <para>拆分来源：Terraria.GameContent.LeashedEntities.LeashedKite。</para>
/// <para>
/// 原始文件：D:/TRbackup/Version4/Terraria.GameContent.LeashedEntities/LeashedKite.cs。
/// </para>
/// <para>
/// 主要源成员：rotation（第 21 行）； kiteDistance（第 25 行）； windTarget（第 27 行）； windCurrent（第 29 行）；
/// timeCounter（第 31 行）； timeWithoutWind（第 35 行）。
/// </para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P02-leashed-entity-component-design.md。</para>
/// <para>依据位置：第 1194 行。</para>
/// </remarks>
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
