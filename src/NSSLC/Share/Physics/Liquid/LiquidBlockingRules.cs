using Terraria.World;

namespace Terraria.Liquid;

/// <summary>Deterministic liquid blocking rule used by the tile-level liquid algorithm.</summary>
/// <remarks>
/// A tile blocks liquid when it is an active, non-inactive solid tile that is not a "solid top"
/// tile. The per-type tables come from <see cref="WorldFacts" /> (the world domain owns the
/// Version4 tile facts); this type only owns the liquid-specific exceptions and normalization, so
/// the tile tables are not duplicated per consumer.
/// </remarks>
internal static class LiquidBlockingRules
{
  private static readonly WorldFacts Facts = WorldFacts.CreateVersion4();

  /// <summary>Every liquid kind the world model understands.</summary>
  internal const LiquidContactFlags KnownLiquids =
      LiquidContactFlags.Water |
      LiquidContactFlags.Honey |
      LiquidContactFlags.Lava |
      LiquidContactFlags.Shimmer;

  /// <summary>
  /// Validates a requested liquid fact: the kind must be one known flag and the amount must agree
  /// with it. Returns false with a caller-facing message when the request is malformed.
  /// </summary>
  internal static bool TryNormalize(
      LiquidContactFlags liquid,
      byte amount,
      out LiquidContactFlags normalized,
      out string? failure)
  {
    normalized = liquid;
    failure = null;
    byte liquidValue = (byte)liquid;
    if ((liquid & ~KnownLiquids) != LiquidContactFlags.None ||
        (liquidValue != 0 && (liquidValue & (liquidValue - 1)) != 0))
    {
      failure = "The supplied liquid kind is not supported.";
      return false;
    }

    if (liquid == LiquidContactFlags.None && amount > 0)
    {
      failure = "A nonzero liquid amount requires a liquid kind.";
      return false;
    }

    if (amount == 0)
    {
      normalized = LiquidContactFlags.None;
    }

    return true;
  }

  internal static bool BlocksLiquid(WorldTileState tile)
  {
    if (!tile.Active || tile.Inactive)
    {
      return false;
    }

    // Liquid.UpdateLiquid ignores boulder solidity and bubbles for its whole update pass.
    if (tile.Type is 138 or 484 or 546 or 664 or 711 or 712 or 713 or 714 or 715 or 716)
    {
      return false;
    }

    return tile.Type < WorldFacts.TileCount &&
        Facts.Solid[tile.Type] && !Facts.SolidTop[tile.Type];
  }
}
