namespace Terraria.Player.Mount;

public static class MountRuntimeIdentityAndFrameProjectionQuery
{
  public static MountRuntimeIdentityAndFrameSnapshot Snapshot(
    MountDefinitionCatalog catalog,
    MountRuntimeFrameAndFlightStateComponent frameState)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    ArgumentNullException.ThrowIfNull(frameState);

    if (!frameState.IsActive || !frameState.MountType.HasValue ||
      !catalog.TryGet(frameState.MountType.Value, out MountDefinition? definition))
    {
      return Empty();
    }

    MountFrameRange activeRange = definition.Animation.GetRange(frameState.FrameState);
    return new MountRuntimeIdentityAndFrameSnapshot(
      true,
      frameState.MountType,
      frameState.Frame,
      frameState.FrameCounter,
      frameState.ExtraFrame,
      frameState.ExtraFrameCounter,
      frameState.FrameState,
      activeRange,
      definition.Animation.IdleFrameLoop,
      definition.Geometry.BodyFrame,
      definition.Geometry.XOffset,
      definition.Geometry.YOffset,
      definition.Geometry.PlayerHeadOffset,
      definition.Geometry.HeightBoost,
      definition.Geometry.PlayerXOffset,
      definition.Geometry.PlayerYOffsets);
  }

  private static MountRuntimeIdentityAndFrameSnapshot Empty()
  {
    return new MountRuntimeIdentityAndFrameSnapshot(
      false,
      null,
      0,
      0f,
      0,
      0f,
      MountRuntimeFrameAndFlightStateComponent.MountFrameStateKind.Standing,
      default,
      false,
      0,
      0,
      0,
      0,
      0,
      0,
      Array.Empty<int>());
  }
}
