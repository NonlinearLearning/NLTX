namespace Terraria.Player.Mount;

public enum MountCollisionCheckKind
{
  Fit = 0,
  DismountSpace = 1,
}

public readonly record struct MountCollisionRequest(
  MountCollisionCheckKind Kind,
  int Width,
  int Height)
{
  public bool IsValid => Width > 0 && Height > 0;
}

public readonly record struct MountCollisionResult(
  bool IsAllowed,
  MountCollisionCheckKind Kind,
  string? FailureReason)
{
  public static MountCollisionResult Allowed(MountCollisionCheckKind kind)
  {
    return new MountCollisionResult(true, kind, null);
  }

  public static MountCollisionResult Blocked(
    MountCollisionCheckKind kind,
    string reason)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(reason);
    return new MountCollisionResult(false, kind, reason);
  }
}

public interface IMountCollisionPort
{
  MountCollisionResult Check(in MountCollisionRequest request);
}
