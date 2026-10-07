namespace Terraria.Player.Mount;

public enum MountQualificationResultKind
{
  Qualified = 0,
  UnknownDefinition = 1,
  InvalidPlayerSize = 2,
  BlockedBySpace = 3,
  BlockedByWetness = 4,
  BlockedByGrapple = 5,
}

public readonly record struct MountQualificationResult(
  MountQualificationResultKind Kind,
  int RequestedWidth,
  int RequestedHeight)
{
  public bool IsQualified => Kind == MountQualificationResultKind.Qualified;
}
