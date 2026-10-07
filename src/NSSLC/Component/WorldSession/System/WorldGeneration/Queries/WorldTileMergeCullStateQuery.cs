using System;
using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Queries;

public static class WorldTileMergeCullStateQuery
{
  public enum MissingNeighborPolicy : byte
  {
    DoNotCull,
    Cull,
  }

  public readonly record struct TileMergeRegion(TilePosition Center);

  public readonly record struct TileMergeNeighborhood(
    bool CenterInvisible,
    bool? TopInvisible,
    bool? BottomInvisible,
    bool? LeftInvisible,
    bool? RightInvisible,
    bool? TopLeftInvisible,
    bool? TopRightInvisible,
    bool? BottomLeftInvisible,
    bool? BottomRightInvisible,
    ulong FramingRevision);

  public readonly record struct CullRules(
    ulong ExpectedFramingRevision,
    MissingNeighborPolicy MissingNeighborPolicy = MissingNeighborPolicy.DoNotCull,
    bool ShowInvisibleBlocks = false);

  public readonly record struct CullResult(
    bool CullTop,
    bool CullBottom,
    bool CullLeft,
    bool CullRight,
    bool CullTopLeft,
    bool CullTopRight,
    bool CullBottomLeft,
    bool CullBottomRight,
    TileMergeRegion Region,
    ulong FramingRevision);

  public static CullResult Calculate(
    TileMergeNeighborhood snapshot,
    CullRules rules)
  {
    return Calculate(
      snapshot,
      new TileMergeRegion(new TilePosition(0, 0)),
      rules);
  }

  public static CullResult Calculate(
    TileMergeNeighborhood snapshot,
    TileMergeRegion region,
    CullRules rules)
  {
    if (snapshot.FramingRevision != rules.ExpectedFramingRevision)
    {
      throw new ArgumentException(
        "The tile merge snapshot does not match the requested framing revision.",
        nameof(snapshot));
    }

    if (rules.ShowInvisibleBlocks)
    {
      return new CullResult(
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        false,
        region,
        snapshot.FramingRevision);
    }

    return new CullResult(
      ShouldCull(snapshot.CenterInvisible, snapshot.TopInvisible, rules),
      ShouldCull(snapshot.CenterInvisible, snapshot.BottomInvisible, rules),
      ShouldCull(snapshot.CenterInvisible, snapshot.LeftInvisible, rules),
      ShouldCull(snapshot.CenterInvisible, snapshot.RightInvisible, rules),
      ShouldCull(snapshot.CenterInvisible, snapshot.TopLeftInvisible, rules),
      ShouldCull(snapshot.CenterInvisible, snapshot.TopRightInvisible, rules),
      ShouldCull(snapshot.CenterInvisible, snapshot.BottomLeftInvisible, rules),
      ShouldCull(snapshot.CenterInvisible, snapshot.BottomRightInvisible, rules),
      region,
      snapshot.FramingRevision);
  }

  private static bool ShouldCull(
    bool centerInvisible,
    bool? neighborInvisible,
    CullRules rules)
  {
    if (!neighborInvisible.HasValue)
    {
      return rules.MissingNeighborPolicy == MissingNeighborPolicy.Cull;
    }

    return centerInvisible != neighborInvisible.Value;
  }
}
