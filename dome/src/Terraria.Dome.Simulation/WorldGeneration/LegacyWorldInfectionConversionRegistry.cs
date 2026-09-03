using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace Terraria.Dome.Simulation.WorldGeneration;

public static class LegacyWorldInfectionConversionRegistry
{
  private static readonly IReadOnlySet<ushort> EmptySourceTypes =
    new HashSet<ushort>().ToFrozenSet();

  private static readonly IReadOnlySet<ushort> TorchTileTypes =
    new HashSet<ushort> { 4 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> MossTileTypes =
    MossTileTypeRegistry.TileTypes.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> StoneTileTypes =
    new HashSet<ushort> { 1, 25, 117, 203 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> MossOrStoneTileTypes = CreateUnion(
    MossTileTypes,
    StoneTileTypes);

  private static readonly IReadOnlySet<ushort> JungleGrassTileTypes =
    new HashSet<ushort> { 60, 661, 662 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> GolfGrassTileTypes =
    new HashSet<ushort> { 477, 492 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> GrassTileTypes =
    new HashSet<ushort> { 2, 23, 199, 109, 477, 492 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> IceTileTypes =
    new HashSet<ushort> { 161, 163, 164, 200 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> SandTileTypes =
    new HashSet<ushort> { 53, 112, 116, 234 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> HardenedSandTileTypes =
    new HashSet<ushort> { 397, 398, 402, 399 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> SandstoneTileTypes =
    new HashSet<ushort> { 396, 400, 403, 401 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> ThornTileTypes =
    new HashSet<ushort> { 32, 352, 69, 655 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> DirtTileTypes =
    new HashSet<ushort> { 0 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> SnowTileTypes =
    new HashSet<ushort> { 147 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> MushroomGrassTileTypes =
    new HashSet<ushort> { 60 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> ChlorophyteMushroomSurfaceTileTypes =
    new HashSet<ushort> { 59, 60 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> ChlorophyteGrassTileTypes =
    new HashSet<ushort> { 2, 23, 109, 199, 477, 492, 661, 662 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> ChlorophyteSecondaryGrassTileTypes =
    new HashSet<ushort> { 23, 199, 661, 662 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> ChlorophyteStoneTileTypes =
    new HashSet<ushort> { 25, 203 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> ChlorophyteSandTileTypes =
    new HashSet<ushort> { 112, 234 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> ChlorophyteHardenedSandTileTypes =
    new HashSet<ushort> { 398, 399 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> ChlorophyteSandstoneTileTypes =
    new HashSet<ushort> { 400, 401 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> ChlorophyteKillTileTypes =
    new HashSet<ushort> { 24, 32, 201, 205, 352, 636 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> GrassWallTypes =
    new HashSet<ushort> { 63, 64, 65, 66, 67, 68, 69, 70, 81, 264, 265, 268 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> StoneWallTypes =
    new HashSet<ushort> { 1, 61, 185, 3, 28, 83, 262, 274, 246, 248, 269, 349 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> IceWallTypes =
    new HashSet<ushort> { 71, 266 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> SandWallTypes = EmptySourceTypes;

  private static readonly IReadOnlySet<ushort> DirtWallTypes =
    new HashSet<ushort> { 2, 16 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> SnowWallTypes =
    new HashSet<ushort> { 40, 249 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> GlowingMushroomWallTypes =
    new HashSet<ushort> { 15, 64, 67, 247 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> HardenedSandWallTypes =
    new HashSet<ushort> { 216, 217, 219, 218, 304, 305, 307, 306 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> SandstoneWallTypes =
    new HashSet<ushort> { 187, 220, 222, 221, 275, 308, 310, 309 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> NewWall1Types =
    new HashSet<ushort> { 188, 192, 200, 204, 212, 276, 280, 288, 292, 300 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> NewWall2Types =
    new HashSet<ushort> { 189, 193, 201, 205, 213, 277, 281, 289, 293, 301 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> NewWall3Types =
    new HashSet<ushort> { 190, 194, 202, 206, 214, 278, 282, 290, 294, 302 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> NewWall4Types =
    new HashSet<ushort> { 191, 195, 203, 207, 215, 279, 283, 291, 295, 303 }.ToFrozenSet();

  private static readonly IReadOnlySet<ushort> StoneFamilyWallTypes = CreateUnion(
    StoneWallTypes,
    NewWall1Types,
    NewWall2Types,
    NewWall3Types,
    NewWall4Types,
    IceWallTypes,
    SandstoneWallTypes);

  private static readonly IReadOnlySet<ushort> HardenedSandDirtSnowWallTypes = CreateUnion(
    HardenedSandWallTypes,
    DirtWallTypes,
    SnowWallTypes);

  private static readonly IReadOnlySet<ushort> GrassSandSnowDirtTileTypes = CreateUnion(
    GrassTileTypes,
    SandTileTypes,
    SnowTileTypes,
    DirtTileTypes);

  private static readonly IReadOnlySet<ushort> GrassSandHardenedSandSnowDirtTileTypes = CreateUnion(
    GrassTileTypes,
    SandTileTypes,
    HardenedSandTileTypes,
    SnowTileTypes,
    DirtTileTypes);

  private static readonly IReadOnlySet<ushort> MossStoneIceSandstoneTileTypes = CreateUnion(
    MossTileTypes,
    StoneTileTypes,
    IceTileTypes,
    SandstoneTileTypes);

  private static readonly IReadOnlyList<LegacyWorldInfectionConversionRule> DefaultRules =
    Array.AsReadOnly(
    [
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Torch,
        0,
        TorchTileTypes,
        0,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.MossOrStone,
        1,
        MossOrStoneTileTypes,
        25),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.JungleGrass,
        2,
        JungleGrassTileTypes,
        661),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Grass,
        3,
        GrassTileTypes,
        23),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Ice,
        4,
        IceTileTypes,
        163),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Sand,
        5,
        SandTileTypes,
        112),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.HardenedSand,
        6,
        HardenedSandTileTypes,
        398),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Sandstone,
        7,
        SandstoneTileTypes,
        400),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Thorn,
        8,
        ThornTileTypes,
        32,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.Grass,
        0,
        GrassWallTypes,
        69),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.Stone,
        1,
        StoneWallTypes,
        3),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.HardenedSand,
        2,
        HardenedSandWallTypes,
        217),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.Sandstone,
        3,
        SandstoneWallTypes,
        220),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall1,
        4,
        NewWall1Types,
        188),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall2,
        5,
        NewWall2Types,
        189),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall3,
        6,
        NewWall3Types,
        190),
      new LegacyWorldInfectionConversionRule(
        1,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall4,
        7,
        NewWall4Types,
        191),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Torch,
        0,
        TorchTileTypes,
        0,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.MossOrStone,
        1,
        MossOrStoneTileTypes,
        117),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.GolfGrass,
        2,
        GolfGrassTileTypes,
        492),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Grass,
        3,
        GrassTileTypes,
        109),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Ice,
        4,
        IceTileTypes,
        164),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Sand,
        5,
        SandTileTypes,
        116),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.HardenedSand,
        6,
        HardenedSandTileTypes,
        402),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Sandstone,
        7,
        SandstoneTileTypes,
        403),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Thorn,
        8,
        ThornTileTypes,
        0,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.Grass,
        0,
        GrassWallTypes,
        70),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.Stone,
        1,
        StoneWallTypes,
        28),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.HardenedSand,
        2,
        HardenedSandWallTypes,
        219),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.Sandstone,
        3,
        SandstoneWallTypes,
        222),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall1,
        4,
        NewWall1Types,
        200),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall2,
        5,
        NewWall2Types,
        201),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall3,
        6,
        NewWall3Types,
        202),
      new LegacyWorldInfectionConversionRule(
        2,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall4,
        7,
        NewWall4Types,
        203),
      new LegacyWorldInfectionConversionRule(
        3,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Torch,
        0,
        TorchTileTypes,
        0,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        3,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.MushroomGrass,
        1,
        MushroomGrassTileTypes,
        70),
      new LegacyWorldInfectionConversionRule(
        3,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Thorn,
        2,
        ThornTileTypes,
        0,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        3,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.GlowingMushroomWall,
        0,
        GlowingMushroomWallTypes,
        80),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Torch,
        0,
        TorchTileTypes,
        0,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.MossOrStone,
        1,
        MossOrStoneTileTypes,
        203),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.JungleGrass,
        2,
        JungleGrassTileTypes,
        662),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Grass,
        3,
        GrassTileTypes,
        199),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Ice,
        4,
        IceTileTypes,
        200),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Sand,
        5,
        SandTileTypes,
        234),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.HardenedSand,
        6,
        HardenedSandTileTypes,
        399),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Sandstone,
        7,
        SandstoneTileTypes,
        401),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Thorn,
        8,
        ThornTileTypes,
        352,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.Grass,
        0,
        GrassWallTypes,
        81),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.Stone,
        1,
        StoneWallTypes,
        83),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.HardenedSand,
        2,
        HardenedSandWallTypes,
        218),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.Sandstone,
        3,
        SandstoneWallTypes,
        221),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall1,
        4,
        NewWall1Types,
        192),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall2,
        5,
        NewWall2Types,
        193),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall3,
        6,
        NewWall3Types,
        194),
      new LegacyWorldInfectionConversionRule(
        4,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.NewWall4,
        7,
        NewWall4Types,
        195),
      new LegacyWorldInfectionConversionRule(
        5,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Torch,
        0,
        TorchTileTypes,
        0,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        5,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.GrassSandSnowDirt,
        1,
        GrassSandSnowDirtTileTypes,
        53),
      new LegacyWorldInfectionConversionRule(
        5,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.HardenedSand,
        2,
        HardenedSandTileTypes,
        397),
      new LegacyWorldInfectionConversionRule(
        5,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.MossStoneIceSandstone,
        3,
        MossStoneIceSandstoneTileTypes,
        396),
      new LegacyWorldInfectionConversionRule(
        5,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Thorn,
        4,
        ThornTileTypes,
        69,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        5,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.StoneFamilyWall,
        0,
        StoneFamilyWallTypes,
        187),
      new LegacyWorldInfectionConversionRule(
        5,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.HardenedSandDirtSnowWall,
        1,
        HardenedSandDirtSnowWallTypes,
        216),
      new LegacyWorldInfectionConversionRule(
        6,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Torch,
        0,
        TorchTileTypes,
        0,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        6,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.GrassSandHardenedSandSnowDirt,
        1,
        GrassSandHardenedSandSnowDirtTileTypes,
        147),
      new LegacyWorldInfectionConversionRule(
        6,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.MossStoneIceSandstone,
        2,
        MossStoneIceSandstoneTileTypes,
        161),
      new LegacyWorldInfectionConversionRule(
        6,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Thorn,
        3,
        ThornTileTypes,
        69,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        6,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.StoneFamilyWall,
        0,
        StoneFamilyWallTypes,
        71),
      new LegacyWorldInfectionConversionRule(
        6,
        LegacyWorldInfectionConversionChannel.Wall,
        LegacyWorldInfectionConversionCategory.HardenedSandDirtSnowWall,
        1,
        HardenedSandDirtSnowWallTypes,
        40),
      new LegacyWorldInfectionConversionRule(
        8,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteMushroomSurface,
        0,
        ChlorophyteMushroomSurfaceTileTypes,
        211),
      new LegacyWorldInfectionConversionRule(
        9,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteGrass,
        0,
        ChlorophyteGrassTileTypes,
        60),
      new LegacyWorldInfectionConversionRule(
        9,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.Dirt,
        1,
        DirtTileTypes,
        59),
      new LegacyWorldInfectionConversionRule(
        9,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteStone,
        2,
        ChlorophyteStoneTileTypes,
        1),
      new LegacyWorldInfectionConversionRule(
        9,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteSand,
        3,
        ChlorophyteSandTileTypes,
        53),
      new LegacyWorldInfectionConversionRule(
        9,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteHardenedSand,
        4,
        ChlorophyteHardenedSandTileTypes,
        397),
      new LegacyWorldInfectionConversionRule(
        9,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteSandstone,
        5,
        ChlorophyteSandstoneTileTypes,
        396),
      new LegacyWorldInfectionConversionRule(
        9,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteKill,
        6,
        ChlorophyteKillTileTypes,
        0,
        IsDeferred: true),
      new LegacyWorldInfectionConversionRule(
        10,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteSecondaryGrass,
        0,
        ChlorophyteSecondaryGrassTileTypes,
        60),
      new LegacyWorldInfectionConversionRule(
        10,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteStone,
        1,
        ChlorophyteStoneTileTypes,
        1),
      new LegacyWorldInfectionConversionRule(
        10,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteSand,
        2,
        ChlorophyteSandTileTypes,
        53),
      new LegacyWorldInfectionConversionRule(
        10,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteHardenedSand,
        3,
        ChlorophyteHardenedSandTileTypes,
        397),
      new LegacyWorldInfectionConversionRule(
        10,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteSandstone,
        4,
        ChlorophyteSandstoneTileTypes,
        396),
      new LegacyWorldInfectionConversionRule(
        10,
        LegacyWorldInfectionConversionChannel.Tile,
        LegacyWorldInfectionConversionCategory.ChlorophyteKill,
        5,
        ChlorophyteKillTileTypes,
        0,
        IsDeferred: true)
    ]);

  public static IReadOnlyList<LegacyWorldInfectionConversionRule> RegisterDefaults()
  {
    return DefaultRules;
  }

  public static IReadOnlyList<LegacyWorldInfectionConversionRule> Rules => DefaultRules;

  public static IReadOnlySet<ushort> GetSourceTypes(
    LegacyWorldInfectionConversionChannel channel,
    LegacyWorldInfectionConversionCategory category)
  {
    if (channel == LegacyWorldInfectionConversionChannel.Tile)
    {
      return category switch
      {
        LegacyWorldInfectionConversionCategory.Torch => TorchTileTypes,
        LegacyWorldInfectionConversionCategory.Moss => MossTileTypes,
        LegacyWorldInfectionConversionCategory.Stone => StoneTileTypes,
        LegacyWorldInfectionConversionCategory.MossOrStone => MossOrStoneTileTypes,
        LegacyWorldInfectionConversionCategory.JungleGrass => JungleGrassTileTypes,
        LegacyWorldInfectionConversionCategory.GolfGrass => GolfGrassTileTypes,
        LegacyWorldInfectionConversionCategory.Grass => GrassTileTypes,
        LegacyWorldInfectionConversionCategory.Ice => IceTileTypes,
        LegacyWorldInfectionConversionCategory.Sand => SandTileTypes,
        LegacyWorldInfectionConversionCategory.HardenedSand => HardenedSandTileTypes,
        LegacyWorldInfectionConversionCategory.Sandstone => SandstoneTileTypes,
        LegacyWorldInfectionConversionCategory.Thorn => ThornTileTypes,
        LegacyWorldInfectionConversionCategory.Dirt => DirtTileTypes,
        LegacyWorldInfectionConversionCategory.Snow => SnowTileTypes,
        LegacyWorldInfectionConversionCategory.MushroomGrass => MushroomGrassTileTypes,
        LegacyWorldInfectionConversionCategory.ChlorophyteMushroomSurface =>
          ChlorophyteMushroomSurfaceTileTypes,
        LegacyWorldInfectionConversionCategory.ChlorophyteGrass => ChlorophyteGrassTileTypes,
        LegacyWorldInfectionConversionCategory.ChlorophyteSecondaryGrass =>
          ChlorophyteSecondaryGrassTileTypes,
        LegacyWorldInfectionConversionCategory.ChlorophyteStone => ChlorophyteStoneTileTypes,
        LegacyWorldInfectionConversionCategory.ChlorophyteSand => ChlorophyteSandTileTypes,
        LegacyWorldInfectionConversionCategory.ChlorophyteHardenedSand =>
          ChlorophyteHardenedSandTileTypes,
        LegacyWorldInfectionConversionCategory.ChlorophyteSandstone =>
          ChlorophyteSandstoneTileTypes,
        LegacyWorldInfectionConversionCategory.ChlorophyteKill => ChlorophyteKillTileTypes,
        LegacyWorldInfectionConversionCategory.GrassSandSnowDirt => GrassSandSnowDirtTileTypes,
        LegacyWorldInfectionConversionCategory.GrassSandHardenedSandSnowDirt =>
          GrassSandHardenedSandSnowDirtTileTypes,
        LegacyWorldInfectionConversionCategory.MossStoneIceSandstone =>
          MossStoneIceSandstoneTileTypes,
        _ => EmptySourceTypes
      };
    }

    return category switch
    {
      LegacyWorldInfectionConversionCategory.Grass => GrassWallTypes,
      LegacyWorldInfectionConversionCategory.Stone => StoneWallTypes,
      LegacyWorldInfectionConversionCategory.Ice => IceWallTypes,
      LegacyWorldInfectionConversionCategory.Sand => SandWallTypes,
      LegacyWorldInfectionConversionCategory.Dirt => DirtWallTypes,
      LegacyWorldInfectionConversionCategory.Snow => SnowWallTypes,
      LegacyWorldInfectionConversionCategory.GlowingMushroomWall =>
        GlowingMushroomWallTypes,
      LegacyWorldInfectionConversionCategory.HardenedSand => HardenedSandWallTypes,
      LegacyWorldInfectionConversionCategory.Sandstone => SandstoneWallTypes,
      LegacyWorldInfectionConversionCategory.NewWall1 => NewWall1Types,
      LegacyWorldInfectionConversionCategory.NewWall2 => NewWall2Types,
      LegacyWorldInfectionConversionCategory.NewWall3 => NewWall3Types,
      LegacyWorldInfectionConversionCategory.NewWall4 => NewWall4Types,
      LegacyWorldInfectionConversionCategory.StoneFamilyWall => StoneFamilyWallTypes,
      LegacyWorldInfectionConversionCategory.HardenedSandDirtSnowWall =>
        HardenedSandDirtSnowWallTypes,
      _ => EmptySourceTypes
    };
  }

  public static bool TryGetTileRule(
    int conversionType,
    ushort sourceType,
    out LegacyWorldInfectionConversionRule rule)
  {
    return TryGetRule(
      conversionType,
      LegacyWorldInfectionConversionChannel.Tile,
      sourceType,
      out rule);
  }

  public static bool TryResolveTile(
    int conversionType,
    ushort sourceType,
    out LegacyWorldInfectionConversionRule rule)
  {
    return TryGetTileRule(conversionType, sourceType, out rule);
  }

  public static bool IsTileTarget(int conversionType, ushort tileType)
  {
    return IsTarget(conversionType, LegacyWorldInfectionConversionChannel.Tile, tileType);
  }

  public static bool TryGetWallRule(
    int conversionType,
    ushort sourceType,
    out LegacyWorldInfectionConversionRule rule)
  {
    return TryGetRule(
      conversionType,
      LegacyWorldInfectionConversionChannel.Wall,
      sourceType,
      out rule);
  }

  public static bool TryResolveWall(
    int conversionType,
    ushort sourceType,
    out LegacyWorldInfectionConversionRule rule)
  {
    return TryGetWallRule(conversionType, sourceType, out rule);
  }

  public static bool IsWallTarget(int conversionType, ushort wallType)
  {
    return IsTarget(conversionType, LegacyWorldInfectionConversionChannel.Wall, wallType);
  }

  private static bool TryGetRule(
    int conversionType,
    LegacyWorldInfectionConversionChannel channel,
    ushort sourceType,
    out LegacyWorldInfectionConversionRule rule)
  {
    for (int index = 0; index < DefaultRules.Count; index++)
    {
      LegacyWorldInfectionConversionRule candidate = DefaultRules[index];
      if (candidate.ConversionType == conversionType && candidate.Channel == channel &&
          candidate.SourceTypes.Contains(sourceType))
      {
        rule = candidate;
        return true;
      }
    }

    rule = default;
    return false;
  }

  private static bool IsTarget(
    int conversionType,
    LegacyWorldInfectionConversionChannel channel,
    ushort sourceType)
  {
    for (int index = 0; index < DefaultRules.Count; index++)
    {
      LegacyWorldInfectionConversionRule candidate = DefaultRules[index];
      if (candidate.ConversionType == conversionType && candidate.Channel == channel &&
          !candidate.IsDeferred && candidate.TargetType == sourceType)
      {
        return true;
      }
    }

    return false;
  }

  private static IReadOnlySet<ushort> CreateUnion(
    params IReadOnlySet<ushort>[] sets)
  {
    HashSet<ushort> union = new();
    for (int index = 0; index < sets.Length; index++)
    {
      union.UnionWith(sets[index]);
    }

    return union.ToFrozenSet();
  }
}
