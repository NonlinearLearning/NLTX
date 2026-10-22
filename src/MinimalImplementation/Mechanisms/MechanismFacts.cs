using System.Collections.ObjectModel;
using Terraria.Items;
using Terraria.WorldStorage;

namespace MinimalImplementation.Server.Mechanisms;

public enum TileEntityPayloadKind
{
  None,
  NpcIndex,
  SingleItem,
  LogicSensor,
  DisplayDoll,
  PylonItems,
  DisplayJar,
  AnchorItem
}

public enum TileEntityAnchorRule
{
  TrainingDummy,
  ItemFrame,
  LogicSensor,
  DisplayDoll,
  WeaponsRack,
  HatRack,
  FoodPlatter,
  TeleportationPylon,
  DisplayJar,
  KiteAnchor,
  CritterAnchor
}

public sealed record TileEntityDefinition(
  byte Type,
  string Name,
  ushort TileType,
  short FrameXModulo,
  short FrameYModulo,
  bool RequiresFrameYZero,
  TileEntityPayloadKind PayloadKind,
  TileEntityAnchorRule AnchorRule,
  int ItemSlotCount,
  bool RequiresUpdates = false);

public sealed record TeleportPylonDefinition(
  byte Type,
  string Name,
  bool RequiresTownNpc);

public enum LeashedPrototypeKind
{
  Kite,
  Walker,
  Crawler,
  Snail,
  Runner,
  Flyer,
  NormalButterfly,
  EmpressButterfly,
  HellButterfly,
  Firefly,
  ShimmerFly,
  Dragonfly,
  CrawlingFly,
  Bird,
  Waterfowl,
  Fish,
  Fairy,
  Jumper,
  WaterStrider
}

public sealed record LeashedPrototypeDefinition(
  byte Type,
  string Name,
  LeashedPrototypeKind Kind,
  IReadOnlyList<short> NpcTypes,
  bool HasProjectileType,
  bool HasVariantByte,
  bool HasOldPositionsLength);

public sealed record WiringFact(
  ushort TileType,
  bool IsMechanism,
  bool IsTrigger,
  string Name);

public sealed class MechanismFactsCatalog
{
  private readonly IReadOnlyDictionary<byte, TileEntityDefinition> _tileEntities;
  private readonly IReadOnlyDictionary<byte, TeleportPylonDefinition> _pylons;
  private readonly IReadOnlyDictionary<byte, LeashedPrototypeDefinition> _leashed;
  private readonly IReadOnlyDictionary<ushort, WiringFact> _wiring;

  private MechanismFactsCatalog(
    IReadOnlyDictionary<byte, TileEntityDefinition> tileEntities,
    IReadOnlyDictionary<byte, TeleportPylonDefinition> pylons,
    IReadOnlyDictionary<byte, LeashedPrototypeDefinition> leashed,
    IReadOnlyDictionary<ushort, WiringFact> wiring)
  {
    _tileEntities = tileEntities;
    _pylons = pylons;
    _leashed = leashed;
    _wiring = wiring;
  }

  public IReadOnlyDictionary<byte, TileEntityDefinition> TileEntities => _tileEntities;

  public IReadOnlyDictionary<byte, TeleportPylonDefinition> Pylons => _pylons;

  public IReadOnlyDictionary<byte, LeashedPrototypeDefinition> LeashedPrototypes => _leashed;

  public IReadOnlyDictionary<ushort, WiringFact> Wiring => _wiring;

  public TileEntityDefinition GetTileEntity(byte type)
  {
    return _tileEntities.TryGetValue(type, out TileEntityDefinition? definition)
      ? definition
      : throw new KeyNotFoundException($"Unknown Version4 TileEntity type {type}.");
  }

  public LeashedPrototypeDefinition GetLeashedPrototype(byte type)
  {
    return _leashed.TryGetValue(type, out LeashedPrototypeDefinition? definition)
      ? definition
      : throw new KeyNotFoundException($"Unknown Version4 leashed prototype type {type}.");
  }

  public static MechanismFactsCatalog CreateVersion4()
  {
    var tileEntities = new Dictionary<byte, TileEntityDefinition>
    {
      [0] = new(0, "TrainingDummy", 378, 36, 0, true, TileEntityPayloadKind.NpcIndex,
        TileEntityAnchorRule.TrainingDummy, 0, true),
      [1] = new(1, "ItemFrame", 395, 36, 0, true, TileEntityPayloadKind.SingleItem,
        TileEntityAnchorRule.ItemFrame, 1),
      [2] = new(2, "LogicSensor", 423, 18, 18, false, TileEntityPayloadKind.LogicSensor,
        TileEntityAnchorRule.LogicSensor, 0, true),
      [3] = new(3, "DisplayDoll", 470, 36, 0, true, TileEntityPayloadKind.DisplayDoll,
        TileEntityAnchorRule.DisplayDoll, 19),
      [4] = new(4, "WeaponsRack", 471, 54, 0, true, TileEntityPayloadKind.SingleItem,
        TileEntityAnchorRule.WeaponsRack, 1),
      [5] = new(5, "HatRack", 475, 54, 0, true, TileEntityPayloadKind.PylonItems,
        TileEntityAnchorRule.HatRack, 4),
      [6] = new(6, "FoodPlatter", 520, 0, 0, true, TileEntityPayloadKind.SingleItem,
        TileEntityAnchorRule.FoodPlatter, 1),
      [7] = new(7, "TeleportationPylon", 597, 54, 0, true, TileEntityPayloadKind.None,
        TileEntityAnchorRule.TeleportationPylon, 0),
      [8] = new(8, "DeadCellsDisplayJar", 698, 18, 0, true, TileEntityPayloadKind.DisplayJar,
        TileEntityAnchorRule.DisplayJar, 1),
      [9] = new(9, "KiteAnchor", 723, 0, 0, false, TileEntityPayloadKind.AnchorItem,
        TileEntityAnchorRule.KiteAnchor, 0),
      [10] = new(10, "CritterAnchor", 724, 0, 0, false, TileEntityPayloadKind.AnchorItem,
        TileEntityAnchorRule.CritterAnchor, 0)
    };

    var pylons = new Dictionary<byte, TeleportPylonDefinition>
    {
      [0] = new(0, "SurfacePurity", true),
      [1] = new(1, "Jungle", true),
      [2] = new(2, "Hallow", true),
      [3] = new(3, "Underground", true),
      [4] = new(4, "Beach", true),
      [5] = new(5, "Desert", true),
      [6] = new(6, "Snow", true),
      [7] = new(7, "GlowingMushroom", true),
      [8] = new(8, "Victory", false),
      [9] = new(9, "Underworld", true),
      [10] = new(10, "Shimmer", true)
    };

    var leashed = new Dictionary<byte, LeashedPrototypeDefinition>
    {
      [1] = new(1, "LeashedKite", LeashedPrototypeKind.Kite, [], true, false, false),
      [2] = new(2, "WalkerLeashedCritter", LeashedPrototypeKind.Walker, [], false, false, false),
      [3] = new(3, "CrawlerLeashedCritter", LeashedPrototypeKind.Crawler,
        [357, 448, 484, 485, 486, 487, 606, 616, 617], false, false, false),
      [4] = new(4, "SnailLeashedCritter", LeashedPrototypeKind.Snail,
        [359, 360, 655], false, false, false),
      [5] = new(5, "RunnerLeashedCritter", LeashedPrototypeKind.Runner,
        [300, 447, 610], false, false, false),
      [6] = new(6, "FlyerLeashedCritter", LeashedPrototypeKind.Flyer,
        [444, 653, 661], false, false, false),
      [7] = new(7, "NormalButterflyLeashedCritter", LeashedPrototypeKind.NormalButterfly,
        [356], false, true, false),
      [8] = new(8, "EmpressButterflyLeashedCritter", LeashedPrototypeKind.EmpressButterfly,
        [661], false, false, false),
      [9] = new(9, "HellButterflyLeashedCritter", LeashedPrototypeKind.HellButterfly,
        [653], false, false, false),
      [10] = new(10, "FireflyLeashedCritter", LeashedPrototypeKind.Firefly,
        [355, 358, 654], false, false, false),
      [11] = new(11, "ShimmerFlyLeashedCritter", LeashedPrototypeKind.ShimmerFly,
        [677], false, false, true),
      [12] = new(12, "DragonflyLeashedCritter", LeashedPrototypeKind.Dragonfly,
        [595, 596, 601, 597, 598, 599, 600], false, false, false),
      [13] = new(13, "CrawlingFlyLeashedCritter", LeashedPrototypeKind.CrawlingFly,
        [604, 605, 669], false, false, false),
      [14] = new(14, "BirdLeashedCritter", LeashedPrototypeKind.Bird,
        [74, 297, 298, 442, 611, 671, 672, 673, 675, 674], false, false, false),
      [15] = new(15, "WaterfowlLeashedCritter", LeashedPrototypeKind.Waterfowl,
        [362, 364, 602, 608], false, false, false),
      [16] = new(16, "FishLeashedCritter", LeashedPrototypeKind.Fish,
        [55, 592, 607, 626, 627, 688], false, false, false),
      [17] = new(17, "FairyLeashedCritter", LeashedPrototypeKind.Fairy,
        [585, 584, 583], false, false, false),
      [18] = new(18, "JumperLeashedCritter", LeashedPrototypeKind.Jumper,
        [377, 446], false, false, false),
      [19] = new(19, "WaterStriderLeashedCritter", LeashedPrototypeKind.WaterStrider,
        [612, 613], false, false, false)
    };

    var wiring = new Dictionary<ushort, WiringFact>();
    AddWiringFacts(wiring, isMechanism: true,
      [420, 419, 411, 4, 33, 100, 93, 42, 34, 215, 405, 92, 35, 126, 95, 149,
        593, 594, 564, 10, 11, 387, 386, 388, 389, 137, 443, 141, 130, 131, 546,
        557, 421, 422, 209, 212, 216, 338, 335, 497, 406, 244, 452, 565, 139,
        506, 105, 349, 531, 429, 142, 143, 235, 210, 425, 21, 467, 219, 642,
        356, 663, 255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266,
        267, 268, 207, 480, 455, 509, 657, 658, 720, 721, 725, 733, 314]);
    AddWiringFacts(wiring, isMechanism: false,
      [135, 136, 132, 144, 411, 423, 476, 440, 441, 468]);

    var immutableLeashed = leashed.ToDictionary(
      pair => pair.Key,
      pair => pair.Value with
      {
        NpcTypes = Array.AsReadOnly(pair.Value.NpcTypes.ToArray())
      });

    ValidateComplete(tileEntities, pylons, immutableLeashed, wiring);
    return new MechanismFactsCatalog(
      new ReadOnlyDictionary<byte, TileEntityDefinition>(tileEntities),
      new ReadOnlyDictionary<byte, TeleportPylonDefinition>(pylons),
      new ReadOnlyDictionary<byte, LeashedPrototypeDefinition>(immutableLeashed),
      new ReadOnlyDictionary<ushort, WiringFact>(wiring));
  }

  private static void AddWiringFacts(
    IDictionary<ushort, WiringFact> target,
    bool isMechanism,
    IReadOnlyList<ushort> tileTypes)
  {
    foreach (ushort tileType in tileTypes)
    {
      if (target.TryGetValue(tileType, out WiringFact? current))
      {
        target[tileType] = current with
        {
          IsMechanism = current.IsMechanism || isMechanism,
          IsTrigger = current.IsTrigger || !isMechanism
        };
      }
      else
      {
        target.Add(tileType, new WiringFact(tileType, isMechanism, !isMechanism,
          $"Tile{tileType}"));
      }
    }
  }

  private static void ValidateComplete(
    IReadOnlyDictionary<byte, TileEntityDefinition> tileEntities,
    IReadOnlyDictionary<byte, TeleportPylonDefinition> pylons,
    IReadOnlyDictionary<byte, LeashedPrototypeDefinition> leashed,
    IReadOnlyDictionary<ushort, WiringFact> wiring)
  {
    if (tileEntities.Count != 11 || Enumerable.Range(0, 11).Any(id => !tileEntities.ContainsKey((byte)id)))
    {
      throw new InvalidOperationException("Version4 TileEntity facts must contain exactly types 0 through 10.");
    }
    if (pylons.Count != 11 || Enumerable.Range(0, 11).Any(id => !pylons.ContainsKey((byte)id)))
    {
      throw new InvalidOperationException("Version4 pylon facts must contain exactly types 0 through 10.");
    }
    if (leashed.Count != 19 || Enumerable.Range(1, 19).Any(id => !leashed.ContainsKey((byte)id)))
    {
      throw new InvalidOperationException("Version4 leashed facts must contain exactly types 1 through 19.");
    }
    if (wiring.Count != 95)
    {
      throw new InvalidOperationException("Version4 wiring facts must contain 95 unique mechanism and trigger tile types.");
    }
  }
}

public readonly record struct MechanismTileSnapshot(
  bool Active,
  ushort Type,
  short FrameX,
  short FrameY);

public readonly record struct MechanismSectionCoordinate(int X, int Y);

public readonly record struct MechanismEntityHandle(
  MinimalImplementation.Contracts.EntityUuid EntityUuid,
  long WorldScope,
  long Generation)
{
  public bool IsValid => EntityUuid.IsValid && WorldScope > 0 && Generation > 0;
}
