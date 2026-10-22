namespace MinimalImplementation.Server.Protocol;

public static class Version4ModulePayloads
{
  public sealed record TextCommandData(string CommandName, string Text);

  public sealed record CreativeSacrificeReportData(byte Player, ushort ItemId, ushort Amount);

  public sealed record CraftingMaterialEntry(int ItemIdOrRecipeGroup, int Stack);

  public sealed record CraftingRequestData(
    IReadOnlyList<CraftingMaterialEntry> Materials,
    IReadOnlyList<int> ChestIndexes);

  public sealed record CraftingApprovalData(bool Approved);

  public sealed record TagEffectMessage(
    byte Player,
    byte Action,
    short? EffectType,
    IReadOnlyList<int>? NpcTimes,
    IReadOnlyList<int>? ProcTimes,
    byte? NpcIndex);

  public abstract record LeashedEntityState;

  public sealed record KiteLeashedEntityState(
    int? ProjectileType,
    float PositionX,
    float PositionY,
    uint PackedVelocity,
    byte Rotation,
    float WindTarget,
    float CloudAlpha,
    float TimeCounter) : LeashedEntityState;

  public sealed record CritterLeashedEntityState(
    int? NpcType,
    float? SizeX,
    float? SizeY,
    uint PackedRelativePosition,
    bool DirectionPositive,
    uint RandomState,
    short WaitTime,
    byte State,
    sbyte TargetRelativeX,
    sbyte TargetRelativeY,
    byte? ButterflyVariant,
    byte? OldPositionsLength) : LeashedEntityState;

  public sealed record LeashedEntityMessage(
    byte Action,
    int Slot,
    int? EntityType,
    short? AnchorX,
    short? AnchorY,
    LeashedEntityState? State);

  /// <summary>
  /// Server-to-client projection of a committed unbreakable-wall state transition. The caller
  /// must obtain the value from the authoritative scanner and publish the intent only after its
  /// owner commit succeeds; this record never represents a client-authored state update.
  /// </summary>
  public sealed record UnbreakableWallNotificationData(byte Player, bool Inside);
}
