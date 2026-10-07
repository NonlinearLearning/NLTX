namespace Terraria.Player.Mount;

public enum MountPlayerCommitIntentKind
{
  Activate = 0,
  Dismount = 1,
  Reset = 2,
  Reconcile = 3,
}

public readonly record struct MountPlayerCommitIntent(
  MountPlayerCommitIntentKind Kind,
  ContentId<MountDefinition>? MountType)
{
  public bool HasMountType => MountType.HasValue;

  public static MountPlayerCommitIntent Activate(ContentId<MountDefinition> mountType)
  {
    return new MountPlayerCommitIntent(
      MountPlayerCommitIntentKind.Activate,
      mountType);
  }

  public static MountPlayerCommitIntent Dismount(
    ContentId<MountDefinition>? previousMountType)
  {
    return new MountPlayerCommitIntent(
      MountPlayerCommitIntentKind.Dismount,
      previousMountType);
  }

  public static MountPlayerCommitIntent Reset()
  {
    return new MountPlayerCommitIntent(
      MountPlayerCommitIntentKind.Reset,
      null);
  }
}

public interface IPlayerMountCommitPort
{
  MountAdapterCommitResult Commit(in MountPlayerCommitIntent intent);
}
