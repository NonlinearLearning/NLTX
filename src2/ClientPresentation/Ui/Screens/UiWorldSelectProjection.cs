namespace Terraria.ClientPresentation.Ui.Screens;

public sealed class UiWorldSelectProjection
{
  public bool TryProject(
    IReadOnlyList<UiWorldSummarySnapshot> worlds,
    int snapshotRevision,
    out Projection projection)
  {
    ArgumentNullException.ThrowIfNull(worlds);
    if (snapshotRevision < 0
      || worlds.Any(world => !world.IsValid || world.Revision != snapshotRevision))
    {
      projection = default;
      return false;
    }

    projection = new Projection(worlds.ToArray(), snapshotRevision);
    return true;
  }

  public bool TryCreateSelection(
    Projection projection,
    string externalWorldKey,
    int expectedRevision,
    out SelectionIntent intent)
  {
    UiWorldSummarySnapshot? world = null;
    foreach (UiWorldSummarySnapshot candidate in projection.Worlds)
    {
      if (candidate.ExternalWorldKey == externalWorldKey)
      {
        world = candidate;
        break;
      }
    }
    if (!world.HasValue
      || expectedRevision != projection.SnapshotRevision
      || world.Value.Revision != expectedRevision)
    {
      intent = default;
      return false;
    }

    intent = new SelectionIntent(externalWorldKey, expectedRevision);
    return true;
  }

  public readonly record struct Projection(
    IReadOnlyList<UiWorldSummarySnapshot> Worlds,
    int SnapshotRevision);

  public readonly record struct SelectionIntent(
    string ExternalWorldKey,
    int SnapshotRevision);
}
