namespace Terraria.Player.Mount;

public readonly record struct MountRuntimeIdentityAndFrameSnapshot(
  bool IsActive,
  ContentId<MountDefinition>? MountType,
  int Frame,
  float FrameCounter,
  int ExtraFrame,
  float ExtraFrameCounter,
  MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind FrameState,
  MountFrameRange ActiveFrameRange,
  bool IdleFrameLoop,
  int BodyFrame,
  int XOffset,
  int YOffset,
  int PlayerHeadOffset,
  int HeightBoost,
  int PlayerXOffset,
  IReadOnlyList<int> PlayerYOffsets)
{
  public bool HasDefinition => IsActive && MountType.HasValue;
}
