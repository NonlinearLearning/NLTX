namespace Terraria.Player.Mount;

public static class MountQualificationQuery
{
  public static MountQualificationResult Evaluate(
    MountDefinitionCatalog catalog,
    ContentId<MountDefinition> mountType,
    in MountQualificationInput input)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    if (!catalog.TryGet(mountType, out MountDefinition? definition))
    {
      return new MountQualificationResult(
        MountQualificationResultKind.UnknownDefinition,
        0,
        0);
    }

    if (input.PlayerWidth <= 0 || input.PlayerHeight <= 0)
    {
      return new MountQualificationResult(
        MountQualificationResultKind.InvalidPlayerSize,
        input.PlayerWidth,
        input.PlayerHeight + definition.Geometry.HeightBoost);
    }

    int requestedWidth = input.PlayerWidth;
    int requestedHeight = input.PlayerHeight + definition.Geometry.HeightBoost;
    if (!input.CanFitInRequestedSpace)
    {
      return new MountQualificationResult(
        MountQualificationResultKind.BlockedBySpace,
        requestedWidth,
        requestedHeight);
    }

    if (input.IsWet || input.IsDripping || input.HasWetCollision)
    {
      return new MountQualificationResult(
        MountQualificationResultKind.BlockedByWetness,
        requestedWidth,
        requestedHeight);
    }

    if (input.IsGrappling && !input.MountCanUseHooks &&
      !definition.Movement.IsMinecart)
    {
      return new MountQualificationResult(
        MountQualificationResultKind.BlockedByGrapple,
        requestedWidth,
        requestedHeight);
    }

    return new MountQualificationResult(
      MountQualificationResultKind.Qualified,
      requestedWidth,
      requestedHeight);
  }
}
