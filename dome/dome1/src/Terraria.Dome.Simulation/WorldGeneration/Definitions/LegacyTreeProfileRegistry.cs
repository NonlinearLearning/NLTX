using System;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyTreeProfileRegistry
{
  private const int MinimumHeight = 7;
  private const int MaximumHeight = 12;
  private const int TopPaddingNeeded = 4;

  private static readonly LegacyTreeProfileDefinition GemTreeTopaz = new(
    LegacyTreeProfileKind.GemTreeTopaz,
    583,
    590,
    MinimumHeight,
    MaximumHeight,
    TopPaddingNeeded);

  private static readonly LegacyTreeProfileDefinition GemTreeAmethyst = new(
    LegacyTreeProfileKind.GemTreeAmethyst,
    584,
    590,
    MinimumHeight,
    MaximumHeight,
    TopPaddingNeeded);

  private static readonly LegacyTreeProfileDefinition GemTreeSapphire = new(
    LegacyTreeProfileKind.GemTreeSapphire,
    585,
    590,
    MinimumHeight,
    MaximumHeight,
    TopPaddingNeeded);

  private static readonly LegacyTreeProfileDefinition GemTreeEmerald = new(
    LegacyTreeProfileKind.GemTreeEmerald,
    586,
    590,
    MinimumHeight,
    MaximumHeight,
    TopPaddingNeeded);

  private static readonly LegacyTreeProfileDefinition GemTreeRuby = new(
    LegacyTreeProfileKind.GemTreeRuby,
    587,
    590,
    MinimumHeight,
    MaximumHeight,
    TopPaddingNeeded);

  private static readonly LegacyTreeProfileDefinition GemTreeDiamond = new(
    LegacyTreeProfileKind.GemTreeDiamond,
    588,
    590,
    MinimumHeight,
    MaximumHeight,
    TopPaddingNeeded);

  private static readonly LegacyTreeProfileDefinition GemTreeAmber = new(
    LegacyTreeProfileKind.GemTreeAmber,
    589,
    590,
    MinimumHeight,
    MaximumHeight,
    TopPaddingNeeded);

  private static readonly LegacyTreeProfileDefinition VanityTreeSakura = new(
    LegacyTreeProfileKind.VanityTreeSakura,
    596,
    595,
    MinimumHeight,
    MaximumHeight,
    TopPaddingNeeded);

  private static readonly LegacyTreeProfileDefinition VanityTreeWillow = new(
    LegacyTreeProfileKind.VanityTreeWillow,
    616,
    615,
    MinimumHeight,
    MaximumHeight,
    TopPaddingNeeded);

  private static readonly LegacyTreeProfileDefinition TreeAsh = new(
    LegacyTreeProfileKind.TreeAsh,
    634,
    20,
    MinimumHeight,
    MaximumHeight,
    TopPaddingNeeded);

  private static readonly IReadOnlyList<LegacyTreeProfileDefinition> DefaultProfiles =
    Array.AsReadOnly<LegacyTreeProfileDefinition>(
    [
      GemTreeTopaz,
      GemTreeAmethyst,
      GemTreeSapphire,
      GemTreeEmerald,
      GemTreeRuby,
      GemTreeDiamond,
      GemTreeAmber,
      VanityTreeSakura,
      VanityTreeWillow,
      TreeAsh
    ]);

  public static IReadOnlyList<LegacyTreeProfileDefinition> RegisterDefaults()
  {
    return DefaultProfiles;
  }

  public static bool TryGet(ushort treeTileType, out LegacyTreeProfileDefinition profile)
  {
    switch (treeTileType)
    {
      case 583:
        profile = GemTreeTopaz;
        return true;
      case 584:
        profile = GemTreeAmethyst;
        return true;
      case 585:
        profile = GemTreeSapphire;
        return true;
      case 586:
        profile = GemTreeEmerald;
        return true;
      case 587:
        profile = GemTreeRuby;
        return true;
      case 588:
        profile = GemTreeDiamond;
        return true;
      case 589:
        profile = GemTreeAmber;
        return true;
      case 596:
        profile = VanityTreeSakura;
        return true;
      case 616:
        profile = VanityTreeWillow;
        return true;
      case 634:
        profile = TreeAsh;
        return true;
      default:
        profile = default;
        return false;
    }
  }

  public static bool TryGet(
    LegacyTreeProfileKind treeProfileKind,
    out LegacyTreeProfileDefinition profile)
  {
    return TryGet(treeProfileKind switch
    {
      LegacyTreeProfileKind.GemTreeTopaz => 583,
      LegacyTreeProfileKind.GemTreeAmethyst => 584,
      LegacyTreeProfileKind.GemTreeSapphire => 585,
      LegacyTreeProfileKind.GemTreeEmerald => 586,
      LegacyTreeProfileKind.GemTreeRuby => 587,
      LegacyTreeProfileKind.GemTreeDiamond => 588,
      LegacyTreeProfileKind.GemTreeAmber => 589,
      LegacyTreeProfileKind.VanityTreeSakura => 596,
      LegacyTreeProfileKind.VanityTreeWillow => 616,
      LegacyTreeProfileKind.TreeAsh => 634,
      _ => 0
    }, out profile);
  }
}
