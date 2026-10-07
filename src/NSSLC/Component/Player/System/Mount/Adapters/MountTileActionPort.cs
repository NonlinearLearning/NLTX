namespace Terraria.Player.Mount;

public readonly record struct MountTileActionIntent(
  DrillMountRuntimeComponent.DrillTileTarget Target,
  DrillMountRuntimeComponent.DrillBeamPurpose Purpose,
  int PickPower,
  int PickTime)
{
  public bool IsValid =>
    Purpose != DrillMountRuntimeComponent.DrillBeamPurpose.Unknown &&
    PickPower > 0 && PickTime > 0;
}

public interface IMountTileActionPort
{
  MountAdapterCommitResult Commit(in MountTileActionIntent intent);
}
