using Terraria.WorldStorage;

namespace Terraria.LeashedEntity;

/// <summary>
/// Stores critter-only behavior state for one leashed entity.
/// status: proposed
/// componentOwner: LeashedEntitySimulation
/// crossSubsystemOwner: integration-review
/// </summary>
public struct LeashedCritterBehaviorComponent
{
  /// <summary>
  /// NPC content id, not an NPC runtime instance id.
  /// </summary>
  public int? NpcType;

  /// <summary>
  /// Anchor compatibility/style candidate. Zero is not a source default claim.
  /// </summary>
  public int AnchorStyle;

  /// <summary>
  /// Critter capability candidate. The source default remains unresolved.
  /// </summary>
  public bool IsAquatic;

  /// <summary>
  /// Current behavior target; null means no accepted target yet.
  /// </summary>
  public TileCoordinate? TargetPosition;

  /// <summary>
  /// Wire-compatible random cursor candidate; internal random type is unresolved.
  /// </summary>
  public uint RandomState;

  /// <summary>
  /// Behavior wait timer candidate.
  /// </summary>
  public short WaitTime;

  /// <summary>
  /// Critter behavior state interpreted by the selected definition.
  /// </summary>
  public byte State;

  /// <summary>
  /// Derived target-presence view only.
  /// </summary>
  public bool HasTargetPosition => TargetPosition.HasValue;

  public LeashedCritterBehaviorComponent(
    int? npcType,
    int anchorStyle,
    bool isAquatic,
    TileCoordinate? targetPosition,
    uint randomState,
    short waitTime,
    byte state)
  {
    NpcType = npcType;
    AnchorStyle = anchorStyle;
    IsAquatic = isAquatic;
    TargetPosition = targetPosition;
    RandomState = randomState;
    WaitTime = waitTime;
    State = state;
  }
}
