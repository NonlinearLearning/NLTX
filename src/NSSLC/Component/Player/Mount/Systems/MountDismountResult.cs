namespace Terraria.Player.Mount;

public enum MountDismountResultKind
{
  Dismounted = 0,
  AlreadyInactive = 1,
}

public readonly record struct MountDismountResult(
  MountDismountResultKind Kind,
  ContentId<MountDefinition>? PreviousMountType)
{
  public bool Changed => Kind == MountDismountResultKind.Dismounted;
}
