using System;
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Npc.Definitions;
using Terraria.Dome.Simulation.Liquid.Definitions;
using Terraria.Dome.Simulation.Projectile.Definitions;
using Terraria.Dome.Simulation.World;
using Terraria.Dome.Simulation.WorldGeneration;
using Terraria.Dome.Simulation.WorldGeneration.Commands;
using Terraria.Dome.Simulation.WorldGeneration.Definitions;
using Terraria.Dome.Simulation.WorldGeneration.Systems;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldModel.Definitions;
using Terraria.Dome.Simulation.WorldObjects.FoodPlatter;
using Terraria.Dome.Simulation.WorldObjects.FoodPlatter.Commands;
using Terraria.Dome.Simulation.WorldObjects.FoodPlatter.Systems;
using Terraria.Dome.Simulation.WorldObjects.Definitions;
using Terraria.WorldFile.V319;
using Terraria.WorldFile.V319.Model;

bool tileMergeOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--tile-merge-only"));
bool wallLargeFramesOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--wall-large-frames-only"));
bool anglerQuestItemsOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--angler-quest-items-only"));
bool npcFrameCountOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--npc-frame-count-only"));
bool slimeRainNpcOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--slime-rain-npc-only"));
bool petProjectileOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--projectile-pets-only"));
bool dirtLayerCavesOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--dirt-layer-caves-only"));
bool rocksInDirtOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--rocks-in-dirt-only"));
bool dirtInRocksOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--dirt-in-rocks-only"));
bool clayOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--clay-only"));
bool rockLayerCavesOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--rock-layer-caves-only"));
bool surfaceCavesOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--surface-caves-only"));
bool mountainCavesOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--mountain-caves-only"));
bool wavyCavesOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--wavy-caves-only"));
bool tunnelsOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--tunnels-only"));
bool oceanSandOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--ocean-sand-only"));
bool sandPatchesOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--sand-patches-only"));
bool underworldEvilOnly = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--underworld-evil-bounds-only"));

if (dirtLayerCavesOnly)
{
  RunDirtLayerCavesFocusedVerification();
  Environment.Exit(0);
}

if (rocksInDirtOnly)
{
  RunRocksInDirtFocusedVerification();
  Environment.Exit(0);
}

if (dirtInRocksOnly)
{
  RunDirtInRocksFocusedVerification();
  Environment.Exit(0);
}

if (clayOnly)
{
  RunClayFocusedVerification();
  Environment.Exit(0);
}

if (rockLayerCavesOnly)
{
  RunRockLayerCavesFocusedVerification();
  Environment.Exit(0);
}

if (surfaceCavesOnly)
{
  RunSurfaceCavesFocusedVerification();
  Environment.Exit(0);
}

if (mountainCavesOnly)
{
  RunMountainCavesFocusedVerification();
  Environment.Exit(0);
}

if (wavyCavesOnly)
{
  RunWavyCavesFocusedVerification();
  Environment.Exit(0);
}

if (tunnelsOnly)
{
  RunTunnelsFocusedVerification();
  Environment.Exit(0);
}

if (oceanSandOnly)
{
  RunOceanSandFocusedVerification();
  Environment.Exit(0);
}

if (sandPatchesOnly)
{
  RunSandPatchesFocusedVerification();
  Environment.Exit(0);
}

if (underworldEvilOnly)
{
  RunUnderworldEvilBoundsFocusedVerification();
  Environment.Exit(0);
}

Dictionary<ushort, byte> expectedLegacyProjectileFrames = new()
{
  [34] = 6,
  [72] = 4,
  [86] = 4,
  [87] = 4,
  [102] = 2,
  [111] = 8,
  [112] = 6,
  [127] = 16,
  [175] = 2,
  [181] = 4,
  [189] = 4,
  [190] = 4,
  [191] = 18,
  [192] = 18,
  [193] = 18,
  [194] = 18,
  [198] = 4,
  [199] = 8,
  [200] = 10,
  [206] = 5,
  [208] = 5,
  [209] = 12,
  [210] = 12,
  [211] = 10,
  [221] = 3,
  [228] = 5,
  [229] = 4,
  [236] = 13,
  [237] = 4,
  [238] = 6,
  [243] = 4,
  [244] = 6,
  [249] = 5,
  [252] = 4,
  [254] = 5,
  [266] = 12,
  [268] = 8,
  [269] = 7,
  [270] = 3,
  [275] = 2,
  [276] = 2,
  [307] = 2,
  [308] = 10,
  [313] = 12,
  [314] = 13,
  [316] = 4,
  [317] = 8,
  [319] = 11,
  [321] = 3,
  [324] = 10,
  [334] = 11,
  [335] = 4,
  [337] = 5,
  [344] = 3,
  [346] = 2,
  [347] = 2,
  [349] = 5,
  [351] = 2,
  [353] = 14,
  [373] = 3,
  [375] = 8,
  [377] = 9,
  [379] = 4,
  [380] = 4,
  [384] = 6,
  [385] = 3,
  [386] = 6,
  [387] = 3,
  [388] = 3,
  [390] = 11,
  [391] = 11,
  [392] = 11,
  [393] = 15,
  [394] = 15,
  [395] = 15,
  [398] = 11,
  [407] = 6,
  [408] = 2,
  [409] = 3,
  [423] = 4,
  [435] = 4,
  [436] = 4,
  [439] = 6,
  [443] = 4,
  [447] = 4,
  [448] = 3,
  [450] = 5,
  [454] = 2,
  [456] = 4,
  [459] = 3,
  [462] = 5,
  [465] = 4,
  [467] = 4,
  [468] = 4,
  [485] = 5,
  [492] = 8,
  [499] = 12,
  [500] = 4,
  [509] = 2,
  [518] = 4,
  [519] = 4,
  [525] = 5,
  [533] = 21,
  [535] = 12,
  [539] = 4,
  [565] = 4,
  [566] = 4,
  [574] = 2,
  [575] = 4,
  [585] = 4,
  [593] = 4,
  [595] = 28,
  [596] = 4,
  [601] = 2,
  [602] = 4,
  [612] = 5,
  [613] = 4,
  [614] = 4,
  [615] = 7,
  [623] = 19,
  [633] = 5,
  [634] = 4,
  [635] = 4,
  [643] = 8,
  [645] = 7,
  [650] = 4,
  [652] = 6,
  [659] = 4,
  [663] = 7,
  [665] = 9,
  [667] = 9,
  [677] = 6,
  [678] = 6,
  [679] = 6,
  [682] = 4,
  [688] = 6,
  [689] = 6,
  [690] = 8,
  [691] = 4,
  [692] = 4,
  [693] = 4,
  [694] = 4,
  [695] = 4,
  [696] = 5,
  [700] = 4,
  [701] = 3,
  [702] = 4,
  [703] = 8,
  [706] = 8,
  [709] = 3,
  [712] = 8,
  [714] = 7,
  [731] = 4,
  [732] = 4,
  [734] = 8,
  [735] = 28,
  [736] = 3,
  [737] = 3,
  [738] = 3,
  [755] = 5,
  [758] = 24,
  [759] = 5,
  [765] = 10,
  [766] = 4,
  [767] = 4,
  [768] = 4,
  [769] = 4,
  [770] = 4,
  [773] = 4,
  [774] = 8,
  [779] = 4,
  [783] = 4,
  [815] = 10,
  [816] = 17,
  [817] = 18,
  [820] = 4,
  [821] = 23,
  [824] = 4,
  [825] = 26,
  [826] = 3,
  [828] = 2,
  [829] = 2,
  [831] = 6,
  [833] = 10,
  [834] = 12,
  [835] = 12,
  [836] = 4,
  [837] = 3,
  [839] = 4,
  [840] = 4,
  [851] = 4,
  [853] = 4,
  [854] = 19,
  [855] = 4,
  [858] = 14,
  [859] = 24,
  [860] = 14,
  [861] = 4,
  [862] = 4,
  [863] = 4,
  [864] = 2,
  [866] = 4,
  [870] = 4,
  [875] = 11,
  [880] = 8,
  [881] = 12,
  [882] = 20,
  [883] = 3,
  [884] = 14,
  [885] = 10,
  [886] = 8,
  [887] = 3,
  [888] = 36,
  [889] = 11,
  [890] = 12,
  [891] = 15,
  [892] = 6,
  [893] = 4,
  [894] = 8,
  [895] = 6,
  [896] = 16,
  [897] = 11,
  [898] = 16,
  [899] = 14,
  [900] = 14,
  [901] = 12,
  [908] = 12,
  [909] = 6,
  [916] = 6,
  [920] = 3,
  [929] = 8,
  [934] = 12,
  [951] = 12,
  [953] = 5,
  [956] = 11,
  [957] = 12,
  [958] = 17,
  [959] = 12,
  [960] = 20,
  [961] = 1,
  [962] = 3,
  [963] = 13,
  [964] = 1,
  [965] = 1,
  [966] = 3,
  [967] = 8,
  [968] = 24,
  [969] = 8,
  [970] = 6,
  [978] = 5,
  [994] = 16,
  [995] = 20,
  [998] = 10,
  [1003] = 16,
  [1004] = 15,
  [1014] = 11,
  [1022] = 16,
  [1024] = 8,
  [1025] = 4,
  [1026] = 4,
  [1027] = 5,
  [1036] = 4,
  [1038] = 8,
  [1046] = 12,
  [1050] = 16,
  [1055] = 10,
  [1078] = 3,
  [1088] = 2,
  [1092] = 54,
  [1093] = 28,
  [1095] = 12,
  [1096] = 12,
  [1098] = 11,
  [1105] = 7,
  [1110] = 4,
};
IReadOnlyDictionary<ushort, byte> actualLegacyProjectileFrames =
  LegacyProjectileFrameRegistry.RegisterDefaults();
if (LegacyProjectileFrameRegistry.ProjectileTypeCount != 1111 ||
    LegacyProjectileFrameRegistry.DefaultFrameCount != 1 ||
    actualLegacyProjectileFrames.Count != expectedLegacyProjectileFrames.Count ||
    expectedLegacyProjectileFrames.Any(pair =>
      !actualLegacyProjectileFrames.TryGetValue(pair.Key, out byte value) ||
      value != pair.Value) ||
    LegacyProjectileFrameRegistry.GetFrameCount(0) != 1 ||
    LegacyProjectileFrameRegistry.GetFrameCount(34) != 6 ||
    LegacyProjectileFrameRegistry.GetFrameCount(221) != 3 ||
    LegacyProjectileFrameRegistry.GetFrameCount(961) != 1 ||
    LegacyProjectileFrameRegistry.GetFrameCount(1110) != 4 ||
    LegacyProjectileFrameRegistry.GetFrameCount(1111) != 1 ||
    actualLegacyProjectileFrames is not FrozenDictionary<ushort, byte>)
{
  throw new InvalidOperationException(
    "Legacy projectile frame defaults drifted from Version4 or lost default semantics.");
}

Console.WriteLine(
  "PASS: legacy projectile frame defaults preserve exact immutable override map");

if (args.Any(argument => StringComparer.Ordinal.Equals(argument, "--projectile-frames-only")))
{
  Console.WriteLine(
    "SUMMARY: projectile frame focused verification completed; remaining WorldGeneration " +
    "checks were intentionally not run");
  Environment.Exit(0);
}

byte[] expectedLegacyNpcFrameCounts =
[
  1, 2, 2, 3, 6, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1,
  2, 25, 23, 25, 21, 15, 26, 2, 10, 1, 16, 16, 16, 3, 1, 15,
  6, 1, 3, 2, 2, 21, 25, 1, 1, 1, 3, 3, 15, 3, 7, 7,
  6, 5, 6, 5, 3, 3, 23, 6, 3, 6, 6, 2, 5, 6, 5, 7,
  7, 4, 5, 8, 1, 5, 1, 2, 4, 16, 5, 4, 4, 15, 16, 16,
  16, 2, 4, 6, 6, 18, 16, 1, 1, 1, 1, 1, 1, 4, 3, 1,
  1, 1, 1, 1, 1, 5, 6, 7, 16, 1, 1, 25, 23, 12, 20, 21,
  1, 2, 2, 3, 6, 1, 1, 1, 15, 4, 11, 1, 23, 6, 6, 6,
  1, 2, 2, 1, 3, 4, 1, 2, 1, 4, 2, 1, 15, 3, 25, 4,
  5, 7, 3, 2, 12, 12, 4, 4, 4, 8, 8, 13, 5, 6, 4, 15,
  23, 3, 15, 8, 5, 4, 13, 15, 12, 4, 14, 14, 3, 2, 5, 3,
  2, 3, 23, 5, 14, 16, 5, 2, 2, 12, 3, 3, 3, 3, 2, 2,
  2, 2, 2, 7, 14, 15, 16, 8, 3, 15, 15, 16, 2, 3, 20, 25,
  23, 26, 4, 4, 16, 16, 20, 20, 20, 2, 2, 2, 2, 8, 12, 3,
  4, 2, 4, 25, 26, 26, 6, 3, 3, 3, 3, 3, 5, 4, 4, 5,
  4, 6, 7, 15, 4, 7, 6, 1, 1, 2, 4, 3, 5, 3, 3, 3,
  4, 5, 6, 4, 2, 1, 8, 4, 4, 1, 8, 1, 4, 15, 15, 15,
  15, 15, 15, 16, 15, 15, 15, 15, 15, 3, 3, 3, 3, 3, 3, 16,
  3, 6, 12, 21, 21, 20, 16, 15, 15, 5, 5, 6, 6, 5, 2, 7,
  2, 6, 6, 6, 6, 6, 15, 15, 15, 15, 15, 11, 4, 2, 2, 3,
  3, 3, 16, 15, 16, 10, 14, 12, 1, 10, 8, 3, 3, 2, 2, 2,
  2, 7, 15, 15, 15, 6, 3, 10, 10, 6, 9, 8, 9, 8, 20, 10,
  6, 23, 1, 4, 24, 2, 4, 6, 6, 13, 15, 15, 15, 15, 4, 4,
  26, 23, 8, 2, 4, 4, 4, 4, 2, 2, 4, 12, 12, 9, 9, 9,
  1, 9, 11, 2, 2, 9, 5, 6, 4, 18, 8, 11, 1, 4, 5, 8,
  4, 1, 1, 1, 1, 4, 2, 5, 4, 11, 5, 11, 1, 1, 1, 10,
  10, 15, 8, 17, 6, 6, 1, 12, 12, 13, 15, 9, 5, 10, 7, 7,
  7, 7, 7, 7, 7, 4, 4, 16, 16, 25, 5, 7, 3, 13, 2, 6,
  2, 19, 19, 19, 20, 26, 3, 1, 1, 1, 1, 1, 16, 21, 9, 16,
  7, 6, 18, 13, 20, 12, 12, 20, 6, 14, 14, 14, 14, 6, 1, 3,
  25, 19, 20, 22, 2, 4, 4, 4, 11, 9, 8, 1, 9, 1, 8, 8,
  12, 12, 11, 11, 11, 11, 11, 11, 11, 11, 11, 1, 6, 9, 1, 1,
  1, 1, 1, 1, 4, 1, 10, 1, 8, 4, 1, 5, 8, 8, 8, 8,
  9, 9, 5, 4, 8, 16, 8, 2, 3, 3, 6, 6, 7, 13, 4, 4,
  4, 4, 1, 1, 1, 8, 25, 11, 14, 14, 14, 17, 17, 17, 5, 5,
  5, 14, 14, 14, 9, 9, 9, 9, 17, 17, 16, 16, 18, 18, 10, 10,
  10, 10, 4, 1, 6, 9, 6, 4, 4, 4, 14, 4, 25, 13, 3, 7,
  6, 6, 1, 4, 4, 4, 4, 4, 4, 4, 15, 15, 8, 8, 2, 6,
  15, 15, 6, 13, 5, 5, 7, 5, 14, 14, 4, 6, 21, 1, 1, 1,
  11, 12, 6, 6, 17, 6, 16, 21, 16, 23, 5, 16, 2, 28, 28, 6,
  6, 6, 6, 6, 6, 6, 7, 7, 7, 7, 7, 7, 7, 3, 4, 6,
  27, 16, 2, 2, 4, 3, 4, 23, 6, 1, 1, 2, 8, 8, 14, 6,
  6, 6, 6, 6, 2, 4, 14, 14, 14, 14, 14, 14, 14, 1, 1, 13,
  6, 13, 1, 3, 16, 3, 30, 3, 1,
];
IReadOnlyDictionary<int, byte> actualLegacyNpcFrameCounts =
  LegacyNpcFrameRegistry.RegisterDefaults();
if (LegacyNpcFrameRegistry.NpcTypeCount != expectedLegacyNpcFrameCounts.Length ||
    actualLegacyNpcFrameCounts.Count != expectedLegacyNpcFrameCounts.Length ||
    actualLegacyNpcFrameCounts is not FrozenDictionary<int, byte>)
{
  throw new InvalidOperationException(
    "Legacy NPC frame-count registry shape drifted from Version4.");
}

for (int npcType = 0; npcType < expectedLegacyNpcFrameCounts.Length; npcType++)
{
  if (!actualLegacyNpcFrameCounts.TryGetValue(npcType, out byte actualFrameCount) ||
      actualFrameCount != expectedLegacyNpcFrameCounts[npcType])
  {
    throw new InvalidOperationException(
      $"Legacy NPC frame-count mismatch at type {npcType}.");
  }
}

bool negativeNpcTypeRejected = false;
try
{
  _ = LegacyNpcFrameRegistry.GetFrameCount(-1);
}
catch (ArgumentOutOfRangeException)
{
  negativeNpcTypeRejected = true;
}

bool upperNpcTypeRejected = false;
try
{
  _ = LegacyNpcFrameRegistry.GetFrameCount(LegacyNpcFrameRegistry.NpcTypeCount);
}
catch (ArgumentOutOfRangeException)
{
  upperNpcTypeRejected = true;
}

if (!negativeNpcTypeRejected ||
    !upperNpcTypeRejected ||
    LegacyNpcFrameRegistry.GetFrameCount(0) != 1 ||
    LegacyNpcFrameRegistry.GetFrameCount(17) != 25 ||
    LegacyNpcFrameRegistry.GetFrameCount(696) != 1)
{
  throw new InvalidOperationException(
    "Legacy NPC frame-count range or representative values drifted from Version4.");
}

Console.WriteLine(
  "PASS: legacy NPC frame-count defaults preserve exact immutable 697-entry map");

if (npcFrameCountOnly)
{
  Console.WriteLine(
    "SUMMARY: NPC frame-count focused verification completed; remaining WorldGeneration " +
    "checks were intentionally not run");
  Environment.Exit(0);
}

const int expectedNpcTypeCount = 697;
const int expectedBlueSlimeNpcType = 1;
FrozenSet<int> expectedLegacySlimeRainNpcTypes =
  new HashSet<int> { expectedBlueSlimeNpcType }.ToFrozenSet();
IReadOnlySet<int> actualLegacySlimeRainNpcTypes =
  LegacySlimeRainNpcRegistry.RegisterDefaults();
if (LegacySlimeRainNpcRegistry.NpcTypeCount != expectedNpcTypeCount ||
    actualLegacySlimeRainNpcTypes.Count != expectedLegacySlimeRainNpcTypes.Count ||
    actualLegacySlimeRainNpcTypes is not FrozenSet<int> ||
    !expectedLegacySlimeRainNpcTypes.SetEquals(actualLegacySlimeRainNpcTypes) ||
    !LegacySlimeRainNpcRegistry.IsSlimeRainNpc(expectedBlueSlimeNpcType) ||
    LegacySlimeRainNpcRegistry.IsSlimeRainNpc(0) ||
    LegacySlimeRainNpcRegistry.IsSlimeRainNpc(expectedNpcTypeCount - 1) ||
    LegacySlimeRainNpcRegistry.IsSlimeRainNpc(-1) ||
    LegacySlimeRainNpcRegistry.IsSlimeRainNpc(LegacySlimeRainNpcRegistry.NpcTypeCount))
{
  throw new InvalidOperationException(
    "Legacy Slime Rain NPC defaults drifted from Version4 or lost default semantics.");
}

Console.WriteLine(
  "PASS: legacy Slime Rain NPC defaults preserve exact immutable one-entry set");

if (slimeRainNpcOnly)
{
  Console.WriteLine(
    "SUMMARY: Slime Rain NPC focused verification completed; remaining WorldGeneration " +
    "checks were intentionally not run");
  Environment.Exit(0);
}

FrozenSet<int> expectedLegacyPetProjectileTypes = new HashSet<int>
{
  111, 112, 127, 175, 191, 192, 193, 194, 197, 198, 199, 200,
  208, 209, 210, 211, 236, 266, 268, 269, 313, 314, 317, 319,
  324, 334, 353, 373, 375, 380, 387, 388, 390, 391, 392, 393,
  394, 395, 398, 407, 423, 492, 499, 533, 613, 623, 625, 626,
  627, 628, 653, 701, 702, 703, 755, 758, 759, 764, 765, 774,
  815, 816, 817, 821, 825, 831, 833, 834, 835, 854, 858, 859,
  860, 864, 875, 881, 882, 883, 884, 885, 886, 887, 888, 889,
  890, 891, 892, 893, 894, 895, 896, 897, 898, 899, 900, 901,
  934, 946, 951, 956, 957, 958, 959, 960, 963, 970, 994, 998,
  1003, 1004, 1018, 1022, 1027, 1046, 1050, 1056, 1090, 1093,
  1094, 1095, 1096
}.ToFrozenSet();
IReadOnlySet<int> actualLegacyPetProjectileTypes =
  LegacyPetProjectileRegistry.RegisterDefaults();
if (LegacyPetProjectileRegistry.ProjectileTypeCount != 1111 ||
    actualLegacyPetProjectileTypes.Count != expectedLegacyPetProjectileTypes.Count ||
    actualLegacyPetProjectileTypes is not FrozenSet<int> ||
    !expectedLegacyPetProjectileTypes.SetEquals(actualLegacyPetProjectileTypes) ||
    !LegacyPetProjectileRegistry.IsPet(111) ||
    !LegacyPetProjectileRegistry.IsPet(1096) ||
    LegacyPetProjectileRegistry.IsPet(0) ||
    LegacyPetProjectileRegistry.IsPet(1109) ||
    LegacyPetProjectileRegistry.IsPet(-1) ||
    LegacyPetProjectileRegistry.IsPet(LegacyPetProjectileRegistry.ProjectileTypeCount))
{
  throw new InvalidOperationException(
    "Legacy projectile pet defaults drifted from Version4 or lost default semantics.");
}

Console.WriteLine(
  "PASS: legacy projectile pet defaults preserve exact immutable 121-entry set");

if (petProjectileOnly)
{
  Console.WriteLine(
    "SUMMARY: projectile pet focused verification completed; remaining WorldGeneration " +
    "checks were intentionally not run");
  Environment.Exit(0);
}

Dictionary<byte, ushort> expectedLegacyAnglerQuestItems = new()
{
  [0] = 2450,
  [1] = 2451,
  [2] = 2452,
  [3] = 2453,
  [4] = 2454,
  [5] = 2455,
  [6] = 2456,
  [7] = 2457,
  [8] = 2458,
  [9] = 2459,
  [10] = 2460,
  [11] = 2461,
  [12] = 2462,
  [13] = 2463,
  [14] = 2464,
  [15] = 2465,
  [16] = 2466,
  [17] = 2467,
  [18] = 2468,
  [19] = 2469,
  [20] = 2470,
  [21] = 2471,
  [22] = 2472,
  [23] = 2473,
  [24] = 2474,
  [25] = 2475,
  [26] = 2476,
  [27] = 2477,
  [28] = 2478,
  [29] = 2479,
  [30] = 2480,
  [31] = 2481,
  [32] = 2482,
  [33] = 2483,
  [34] = 2484,
  [35] = 2485,
  [36] = 2486,
  [37] = 2487,
  [38] = 2488,
  [39] = 4393,
  [40] = 4394
};
IReadOnlyDictionary<byte, ushort> actualLegacyAnglerQuestItems =
  LegacyAnglerQuestItemRegistry.RegisterDefaults();
if (LegacyAnglerQuestItemRegistry.QuestCount != 41 ||
    LegacyAnglerQuestItemRegistry.ItemTypeCount != 6147 ||
    actualLegacyAnglerQuestItems.Count != expectedLegacyAnglerQuestItems.Count ||
    expectedLegacyAnglerQuestItems.Any(pair =>
      !actualLegacyAnglerQuestItems.TryGetValue(pair.Key, out ushort value) ||
      value != pair.Value) ||
    LegacyAnglerQuestItemRegistry.TryGetItemType(41, out _) ||
    !LegacyAnglerQuestItemRegistry.TryGetItemType(0, out ushort firstItemType) ||
    firstItemType != 2450 ||
    !LegacyAnglerQuestItemRegistry.TryGetItemType(40, out ushort lastItemType) ||
    lastItemType != 4394 ||
    actualLegacyAnglerQuestItems is not FrozenDictionary<byte, ushort>)
{
  throw new InvalidOperationException(
    "Legacy Angler quest item defaults drifted from Version4 or lost immutable ordering.");
}

Console.WriteLine(
  "PASS: legacy Angler quest item defaults preserve exact immutable index mapping");

if (anglerQuestItemsOnly)
{
  Console.WriteLine(
    "SUMMARY: Angler quest item focused verification completed; remaining WorldGeneration " +
    "checks were intentionally not run");
  Environment.Exit(0);
}

HashSet<ushort> expectedLegacySolidTop =
[
  14, 16, 18, 19, 87, 88, 101, 114, 134, 239, 275, 276, 277, 278, 279, 280, 281,
  285, 286, 296, 297, 298, 299, 309, 310, 339, 358, 359, 361, 362, 363, 364, 376,
  380, 391, 392, 393, 394, 405, 413, 414, 427, 469, 532, 533, 538, 542, 544, 550,
  551, 553, 554, 555, 556, 558, 559, 582, 599, 600, 601, 602, 603, 604, 605, 606,
  607, 608, 609, 610, 611, 612, 619, 629, 632, 640, 643, 644, 645, 710
];
if (!LegacySolidTopTileRegistry.RegisterDefaults().SetEquals(expectedLegacySolidTop) ||
    LegacySolidTopTileRegistry.IsSolidTop(435))
{
  throw new InvalidOperationException("Legacy solid-top defaults drifted from Version4.");
}

HashSet<ushort> expectedLegacyHammer = [26, 31, 695, 696];
if (!LegacyHammerTileRegistry.RegisterDefaults().SetEquals(expectedLegacyHammer) ||
    LegacyHammerTileRegistry.IsHammerTarget(25) ||
    !LegacyHammerTileRegistry.IsHammerTarget(696))
{
  throw new InvalidOperationException("Legacy hammer defaults drifted from Version4.");
}

if (LegacyHammerTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy hammer definition projection was not frozen.");
}

Console.WriteLine("PASS: legacy hammer defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyAxe =
[
  5, 72, 80, 323, 488, 583, 584, 585, 586, 587, 588, 589, 596, 616, 634, 704
];
if (!LegacyAxeTileRegistry.RegisterDefaults().SetEquals(expectedLegacyAxe) ||
    LegacyAxeTileRegistry.IsAxeTarget(4) ||
    !LegacyAxeTileRegistry.IsAxeTarget(704))
{
  throw new InvalidOperationException("Legacy axe defaults drifted from Version4.");
}

if (LegacyAxeTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy axe definition projection was not frozen.");
}

Console.WriteLine("PASS: legacy axe defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyTable =
[
  14, 18, 19, 87, 88, 101, 114, 275, 276, 277, 278, 279, 280, 281, 285, 286,
  296, 297, 298, 299, 309, 310, 339, 358, 359, 361, 362, 363, 364, 376, 380,
  391, 392, 393, 394, 405, 413, 414, 427, 469, 532, 533, 538, 542, 544, 550,
  551, 553, 554, 555, 556, 558, 559, 582, 599, 600, 601, 602, 603, 604, 605,
  606, 607, 608, 609, 610, 611, 612, 619, 629, 632, 640, 643, 644, 645, 710
];
if (!LegacyTileTableRegistry.RegisterDefaults().SetEquals(expectedLegacyTable) ||
    LegacyTileTableRegistry.IsTable(16) || !LegacyTileTableRegistry.IsTable(710) ||
    LegacyTileTableRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy tile-table defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy tile-table defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyNoFail =
[
  3, 4, 24, 32, 50, 51, 52, 61, 62, 69, 73, 74, 81, 82, 83, 84, 110, 113, 115,
  129, 162, 165, 184, 185, 186, 187, 192, 201, 205, 227, 233, 254, 324, 330, 331,
  332, 333, 352, 373, 374, 375, 382, 384, 461, 481, 482, 483, 484, 485, 518, 519,
  528, 529, 530, 549, 624, 636, 637, 638, 654, 655, 656, 666, 697, 700, 701, 705,
  709
];
if (!LegacyNoFailTileRegistry.RegisterDefaults().SetEquals(expectedLegacyNoFail) ||
    LegacyNoFailTileRegistry.IsNoFail(2) || !LegacyNoFailTileRegistry.IsNoFail(709) ||
    LegacyNoFailTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy no-fail defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy no-fail defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyBouncy = [371, 446, 447, 448];
if (!LegacyBouncyTileRegistry.RegisterDefaults().SetEquals(expectedLegacyBouncy) ||
    LegacyBouncyTileRegistry.IsBouncy(370) || !LegacyBouncyTileRegistry.IsBouncy(448) ||
    LegacyBouncyTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy bouncy defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy bouncy defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyAlchemical = [82, 83, 84];
if (!LegacyAlchemicalTileRegistry.RegisterDefaults().SetEquals(expectedLegacyAlchemical) ||
    LegacyAlchemicalTileRegistry.IsAlchemical(81) ||
    !LegacyAlchemicalTileRegistry.IsAlchemical(84) ||
    LegacyAlchemicalTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy alchemical defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy alchemical defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyWallLight =
[
  0, 21, 106, 107, 138, 139, 140, 141, 145, 150, 152, 168, 245, 315, 317, 318
];
if (!LegacyWallLightRegistry.RegisterDefaults().SetEquals(expectedLegacyWallLight) ||
    LegacyWallLightRegistry.IsLightWall(1) || !LegacyWallLightRegistry.IsLightWall(318) ||
    LegacyWallLightRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy wall-light defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy wall-light defaults preserve exact immutable registry");

Dictionary<ushort, byte> expectedLegacyLargeFrames = new()
{
  [273] = 1, [274] = 1, [284] = 1, [325] = 1, [357] = 1, [409] = 2,
  [618] = 1, [669] = 2, [670] = 2, [671] = 2, [672] = 2, [673] = 2,
  [674] = 2, [675] = 2, [676] = 2, [735] = 2, [736] = 1, [737] = 2,
  [741] = 2, [742] = 2, [743] = 2, [745] = 2, [746] = 2, [749] = 2
};
if (!LegacyLargeFrameTileRegistry.RegisterDefaults().OrderBy(pair => pair.Key)
      .SequenceEqual(expectedLegacyLargeFrames.OrderBy(pair => pair.Key)) ||
    LegacyLargeFrameTileRegistry.GetFrameSize(272) != 0 ||
    LegacyLargeFrameTileRegistry.GetFrameSize(749) != 2 ||
    LegacyLargeFrameTileRegistry.RegisterDefaults() is not FrozenDictionary<ushort, byte>)
{
  throw new InvalidOperationException("Legacy large-frame defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy large-frame defaults preserve exact immutable registry");

Dictionary<ushort, byte> expectedLegacyWallLargeFrames = new()
{
  [146] = 1, [147] = 1, [167] = 1, [179] = 1, [185] = 2, [224] = 2,
  [274] = 2, [323] = 2, [324] = 2, [325] = 2, [326] = 2, [327] = 2,
  [328] = 2, [329] = 2, [330] = 2, [354] = 1, [355] = 2, [358] = 2,
  [359] = 2, [362] = 2, [363] = 2, [366] = 2
};
IReadOnlyDictionary<ushort, byte> actualLegacyWallLargeFrames =
  LegacyLargeFrameWallRegistry.RegisterDefaults();
if (LegacyLargeFrameWallRegistry.WallTypeCount != 367 ||
    actualLegacyWallLargeFrames.Count != expectedLegacyWallLargeFrames.Count ||
    expectedLegacyWallLargeFrames.Any(pair =>
      !actualLegacyWallLargeFrames.TryGetValue(pair.Key, out byte value) || value != pair.Value) ||
    LegacyLargeFrameWallRegistry.GetFrameSize(0) != 0 ||
    LegacyLargeFrameWallRegistry.GetFrameSize(146) != 1 ||
    LegacyLargeFrameWallRegistry.GetFrameSize(367) != 0 ||
    actualLegacyWallLargeFrames is not FrozenDictionary<ushort, byte>)
{
  throw new InvalidOperationException("Legacy wall large-frame defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy wall large-frame defaults preserve exact immutable registry");

if (wallLargeFramesOnly)
{
  Console.WriteLine(
    "SUMMARY: wall large-frame focused verification completed; remaining WorldGeneration " +
    "checks were intentionally not run");
  Environment.Exit(0);
}

HashSet<ushort> expectedLegacyNoSunLight = [11, 197, 386, 389, 630, 631];
if (!LegacyNoSunLightTileRegistry.RegisterDefaults().SetEquals(expectedLegacyNoSunLight) ||
    LegacyNoSunLightTileRegistry.IsNoSunLight(10) ||
    !LegacyNoSunLightTileRegistry.IsNoSunLight(631) ||
    LegacyNoSunLightTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy no-sun-light defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy no-sun-light defaults preserve exact immutable registry");

HashSet<ushort> expectedLegacySpelunker =
[
  6, 7, 8, 9, 12, 21, 28, 37, 63, 64, 65, 66, 67, 68, 83, 84, 105, 107, 108,
  111, 166, 167, 168, 169, 178, 211, 221, 222, 223, 227, 236, 240, 242, 245,
  246, 337, 349, 404, 407, 441, 467, 468, 506, 531, 566, 639, 702, 751, 752
];
if (!LegacySpelunkerTileRegistry.RegisterDefaults().SetEquals(expectedLegacySpelunker) ||
    LegacySpelunkerTileRegistry.IsSpelunkerTile(5) ||
    !LegacySpelunkerTileRegistry.IsSpelunkerTile(752) ||
    LegacySpelunkerTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy spelunker defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy spelunker defaults preserve exact immutable registry");

HashSet<ushort> expectedLegacyNoAttach =
[
  3, 4, 10, 13, 14, 15, 16, 17, 18, 19, 20, 21, 27, 50, 86, 87, 88, 89, 90, 91,
  92, 93, 94, 95, 96, 97, 98, 99, 101, 102, 110, 114, 134, 387, 388, 390, 427,
  435, 436, 437, 438, 439, 441, 467, 468, 469, 486, 487, 488, 489, 490, 497, 564,
  565, 568, 569, 570, 572, 580, 590, 593, 594, 595, 615, 620, 704, 707
];
TileDefinitionRegistry tileDefinitionRegistry = TileDefinitionRegistry.RegisterDefaults();
HashSet<ushort> actualLegacyNoAttach = tileDefinitionRegistry.Definitions
  .Where(definition => definition.IsNoAttach)
  .Select(definition => definition.TileType)
  .ToHashSet();
if (!actualLegacyNoAttach.SetEquals(expectedLegacyNoAttach) ||
    !tileDefinitionRegistry.TryGet(435, out TileDefinition noAttachDefinition) ||
    !noAttachDefinition.IsNoAttach ||
    tileDefinitionRegistry.TryGet(434, out TileDefinition attachedDefinition) &&
      attachedDefinition.IsNoAttach)
{
  throw new InvalidOperationException("Legacy no-attach defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy no-attach defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyPile = [330, 331, 332, 333];
if (!LegacyPileTileRegistry.RegisterDefaults().SetEquals(expectedLegacyPile) ||
    LegacyPileTileRegistry.IsPileTile(329) || !LegacyPileTileRegistry.IsPileTile(333) ||
    LegacyPileTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy pile defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy pile defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyFlame =
[
  4, 33, 34, 35, 42, 49, 93, 98, 100, 173, 174, 372, 646
];
if (!LegacyFlameTileRegistry.RegisterDefaults().SetEquals(expectedLegacyFlame) ||
    LegacyFlameTileRegistry.IsFlameTile(3) || !LegacyFlameTileRegistry.IsFlameTile(646) ||
    LegacyFlameTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy flame defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy flame defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyLighted =
[
  4, 17, 19, 20, 22, 26, 27, 31, 33, 34, 35, 37, 42, 49, 58, 61, 70, 71, 72, 76,
  77, 83, 84, 92, 93, 95, 96, 98, 100, 109, 125, 126, 129, 133, 140, 149, 160, 171,
  173, 174, 184, 190, 204, 209, 215, 237, 238, 262, 263, 264, 265, 266, 267, 268, 270,
  271, 286, 302, 316, 317, 318, 327, 336, 340, 341, 342, 343, 344, 346, 347, 348, 349,
  350, 354, 356, 370, 372, 381, 390, 391, 405, 415, 416, 417, 418, 429, 463, 491, 500,
  501, 502, 503, 517, 519, 528, 534, 535, 536, 537, 539, 540, 548, 564, 568, 569, 570,
  572, 578, 580, 581, 582, 592, 593, 594, 597, 598, 613, 614, 619, 620, 625, 626, 627,
  628, 633, 634, 637, 638, 646, 656, 658, 659, 660, 663, 667, 684, 687, 688, 689, 690,
  691, 692, 695, 696, 699, 701, 703, 708, 711, 717, 718, 719, 739
];
if (!LegacyLightedTileRegistry.RegisterDefaults().SetEquals(expectedLegacyLighted) ||
    LegacyLightedTileRegistry.IsLighted(262) == false ||
    LegacyLightedTileRegistry.IsLighted(261) ||
    LegacyLightedTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy lighted defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy lighted defaults preserve exact immutable tile registry");

HashSet<int> expectedLegacyCatchableNpc =
[
  46, 55, 74, 148, 149, 297, 298, 299, 300, 355, 356, 357, 358, 359, 360, 361, 362,
  363, 364, 365, 366, 367, 374, 377, 442, 443, 444, 445, 446, 447, 448, 484, 485,
  486, 487, 538, 539, 583, 584, 585, 592, 593, 595, 596, 597, 598, 599, 600, 601,
  602, 603, 604, 605, 606, 607, 608, 609, 610, 611, 612, 613, 614, 616, 617,
  626, 627, 639, 640, 641, 642, 643, 644, 645, 646, 647, 648, 649, 650, 651, 652,
  653, 654, 655, 661, 669, 671, 672, 673, 674, 675, 677, 688
];
if (!LegacyCatchableNpcRegistry.RegisterDefaults().SetEquals(expectedLegacyCatchableNpc) ||
    LegacyCatchableNpcRegistry.IsCatchable(45) ||
    !LegacyCatchableNpcRegistry.IsCatchable(688) ||
    LegacyCatchableNpcRegistry.RegisterDefaults() is not FrozenSet<int>)
{
  throw new InvalidOperationException("Legacy catchable NPC defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy catchable NPC defaults preserve exact immutable registry");

HashSet<ushort> expectedLegacyObsidianKill =
[
  3, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 24, 27, 28, 29, 32, 33, 34, 35, 36,
  42, 49, 50, 51, 52, 55, 61, 62, 69, 71, 72, 73, 74, 77, 78, 79, 80, 81, 82, 83,
  84, 85, 86, 87, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98, 100, 101, 102, 103,
  104, 105, 106, 110, 113, 115, 125, 126, 128, 129, 132, 133, 134, 135, 136, 139,
  149, 165, 172, 173, 174, 178, 184, 185, 186, 187, 201, 205, 209, 210, 212, 213,
  215, 216, 217, 218, 219, 220, 227, 228, 231, 233, 236, 238, 240, 241, 242, 243,
  244, 245, 246, 247, 254, 269, 270, 271, 275, 276, 277, 278, 279, 280, 281, 282,
  283, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295, 296, 297, 298, 299,
  300, 301, 302, 303, 304, 305, 306, 307, 308, 309, 310, 314, 316, 317, 318, 319,
  323, 324, 335, 337, 338, 339, 349, 352, 353, 354, 355, 382, 413, 425, 453, 456,
  463, 464, 465, 469, 484, 485, 486, 487, 488, 489, 490, 493, 497, 499, 506, 510,
  511, 528, 529, 530, 532, 533, 538, 544, 546, 547, 548, 550, 551, 552, 553, 554,
  555, 556, 558, 559, 560, 564, 565, 567, 568, 569, 570, 571, 572, 573, 579, 580,
  581, 582, 591, 599, 600, 601, 602, 603, 604, 605, 606, 607, 608, 609, 610, 611,
  612, 619, 620, 621, 622, 623, 624, 629, 630, 631, 632, 636, 640, 642, 643, 644,
  645, 647, 648, 649, 650, 651, 652, 654, 655, 656, 660, 693, 694, 697, 698, 699,
  700, 701, 702, 703, 704, 705, 706, 707, 710
];
if (!LegacyObsidianKillTileRegistry.RegisterDefaults().SetEquals(expectedLegacyObsidianKill) ||
    LegacyObsidianKillTileRegistry.IsObsidianKillTile(88) ||
    !LegacyObsidianKillTileRegistry.IsObsidianKillTile(706) ||
    LegacyObsidianKillTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy obsidian-kill defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy obsidian-kill defaults preserve exact immutable tile registry");

Dictionary<ushort, short> expectedLegacyOreFinderPriority = new()
{
  [6] = 220, [7] = 200, [8] = 260, [9] = 240, [12] = 550, [21] = 500, [22] = 300,
  [28] = 100, [37] = 400, [107] = 600, [108] = 620, [111] = 640, [129] = 675,
  [166] = 210, [167] = 230, [168] = 250, [169] = 270, [204] = 310, [211] = 700,
  [221] = 610, [222] = 630, [223] = 650, [227] = 750, [236] = 810, [404] = 150,
  [407] = 150, [441] = 500, [467] = 500, [468] = 500, [639] = 550, [656] = 760,
  [665] = 550, [701] = 760, [702] = 810, [751] = 770, [752] = 770
};
IReadOnlyDictionary<ushort, short> actualLegacyOreFinderPriority =
  LegacyOreFinderPriorityRegistry.RegisterDefaults();
if (actualLegacyOreFinderPriority.Count != expectedLegacyOreFinderPriority.Count ||
    expectedLegacyOreFinderPriority.Any(pair =>
      !actualLegacyOreFinderPriority.TryGetValue(pair.Key, out short value) ||
      value != pair.Value) ||
    LegacyOreFinderPriorityRegistry.TryGetPriority(5, out _) ||
    !LegacyOreFinderPriorityRegistry.TryGetPriority(752, out short priority752) ||
    priority752 != 770 ||
    actualLegacyOreFinderPriority is not FrozenDictionary<ushort, short>)
{
  throw new InvalidOperationException("Legacy ore-finder priorities drifted from Version4.");
}

Console.WriteLine("PASS: legacy ore-finder priorities preserve exact immutable value registry");

HashSet<ushort> expectedLegacyMergeDirt =
[
  1, 6, 7, 8, 9, 22, 25, 30, 37, 38, 39, 40, 41, 43, 44, 45, 46, 47, 53, 56,
  107, 108, 111, 112, 116, 117, 118, 119, 120, 121, 122, 123, 140, 145, 146, 148,
  150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 166, 167, 168, 169,
  175, 176, 177, 188, 190, 193, 195, 197, 198, 202, 203, 204, 206, 208, 221,
  222, 223, 229, 230, 234, 249, 250, 251, 252, 253, 311, 315, 321, 322, 346,
  347, 348, 350, 367, 368, 369, 370, 371, 408, 472, 473, 474, 478, 479, 481,
  482, 483, 495, 496, 498, 500, 501, 502, 503, 562, 563, 635, 641, 666, 667,
  677, 678, 679, 680, 681, 682, 683, 684, 685, 686, 722, 734, 740, 744, 750
];
if (!LegacyMergeDirtTileRegistry.RegisterDefaults().SetEquals(expectedLegacyMergeDirt) ||
    LegacyMergeDirtTileRegistry.IsMergeDirtTile(2) ||
    !LegacyMergeDirtTileRegistry.IsMergeDirtTile(750) ||
    LegacyMergeDirtTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy merge-dirt defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy merge-dirt defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyBrick =
[
  1, 2, 23, 25, 30, 37, 38, 39, 41, 43, 44, 45, 46, 47, 53, 54, 57, 59, 60,
  70, 75, 76, 109, 112, 116, 117, 118, 119, 120, 121, 122, 140, 147, 148, 150,
  151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 162, 163, 164, 175,
  176, 177, 179, 180, 181, 182, 183, 188, 190, 191, 193, 194, 195, 198, 199,
  200, 202, 203, 206, 208, 225, 226, 229, 234, 248, 249, 250, 252, 253, 255,
  256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 273, 274,
  311, 315, 321, 322, 326, 327, 328, 329, 345, 346, 347, 348, 350, 357, 369,
  370, 381, 383, 385, 408, 409, 415, 416, 417, 418, 458, 459, 472, 473, 474,
  477, 478, 479, 481, 482, 483, 492, 495, 496, 498, 500, 501, 502, 503, 507,
  508, 512, 513, 514, 515, 516, 517, 534, 535, 536, 537, 539, 540, 562, 563,
  625, 626, 627, 628, 633, 635, 641, 659, 661, 662, 666, 667, 669, 670, 671,
  672, 673, 674, 675, 676, 677, 678, 679, 680, 681, 682, 683, 684, 685, 686,
  687, 688, 689, 690, 691, 692, 708, 722, 734, 735, 736, 737, 738, 740, 741,
  742, 743, 744, 745, 746, 747, 748, 749, 750
];
if (!LegacyBrickTileRegistry.RegisterDefaults().SetEquals(expectedLegacyBrick) ||
    LegacyBrickTileRegistry.IsBrickTile(0) ||
    LegacyBrickTileRegistry.IsBrickTile(3) ||
    LegacyBrickTileRegistry.IsBrickTile(24) ||
    !LegacyBrickTileRegistry.IsBrickTile(750) ||
    LegacyBrickTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy brick defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy brick defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyHouseWall =
[
  1, 4, 5, 6, 10, 11, 12, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 29,
  30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 41, 42, 43, 44, 45, 46, 47, 60, 66,
  67, 68, 72, 73, 74, 75, 76, 77, 78, 82, 84, 85, 88, 89, 90, 91, 92, 93,
  100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114,
  115, 116, 117, 118, 119, 120, 121, 122, 123, 124, 125, 126, 127, 128, 129,
  130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 143, 144,
  145, 146, 147, 148, 149, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159,
  160, 161, 162, 163, 164, 165, 166, 167, 168, 169, 172, 173, 174, 175, 176,
  177, 179, 181, 182, 183, 184, 186, 224, 225, 226, 227, 228, 229, 230, 231,
  232, 233, 234, 235, 236, 237, 238, 239, 240, 241, 242, 243, 245, 246, 247,
  248, 249, 250, 251, 252, 253, 254, 255, 256, 257, 258, 259, 260, 261, 262,
  263, 264, 265, 266, 267, 268, 269, 270, 271, 272, 273, 274, 275, 276, 277,
  278, 279, 280, 281, 282, 283, 284, 285, 286, 287, 288, 289, 290, 291, 292,
  293, 294, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 305, 306, 307,
  308, 309, 310, 311, 312, 313, 314, 315, 316, 317, 318, 319, 320, 321, 322,
  323, 324, 325, 326, 327, 328, 329, 330, 331, 332, 333, 334, 335, 336, 337,
  338, 339, 340, 341, 342, 343, 344, 345, 346, 347, 348, 351, 352, 353, 354,
  355, 356, 357, 358, 359, 360, 361, 362, 363, 364, 365, 366
];
if (!LegacyHouseWallRegistry.RegisterDefaults().SetEquals(expectedLegacyHouseWall) ||
    LegacyHouseWallRegistry.IsHouseWall(0) ||
    LegacyHouseWallRegistry.IsHouseWall(2) ||
    LegacyHouseWallRegistry.IsHouseWall(3) ||
    !LegacyHouseWallRegistry.IsHouseWall(153) ||
    !LegacyHouseWallRegistry.IsHouseWall(166) ||
    !LegacyHouseWallRegistry.IsHouseWall(366) ||
    LegacyHouseWallRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy house-wall defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy house-wall defaults preserve exact immutable tile registry");

HashSet<ushort> expectedLegacyBlockLight =
[
  0, 1, 2, 6, 7, 8, 9, 10, 22, 23, 25, 30, 32, 37, 38, 39,
  40, 41, 43, 44, 45, 46, 47, 51, 52, 53, 56, 57, 58, 59, 60, 62,
  63, 64, 65, 66, 67, 68, 70, 75, 76, 107, 108, 109, 111, 112, 115, 116,
  117, 118, 119, 120, 121, 122, 123, 130, 131, 137, 140, 145, 146, 147, 148, 150,
  151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161, 163, 164, 165, 166, 167,
  168, 169, 170, 175, 176, 177, 179, 180, 181, 182, 183, 188, 190, 191, 192, 193,
  194, 195, 197, 198, 199, 200, 202, 203, 204, 205, 206, 208, 211, 221, 222, 223,
  224, 225, 226, 229, 230, 234, 235, 248, 249, 250, 251, 252, 253, 272, 273, 274,
  284, 311, 312, 313, 315, 321, 322, 325, 326, 327, 329, 345, 346, 347, 348, 350,
  352, 357, 367, 368, 369, 370, 371, 381, 382, 383, 384, 385, 387, 388, 396, 397,
  398, 399, 400, 401, 402, 403, 404, 407, 408, 409, 415, 416, 417, 418, 421, 422,
  426, 430, 431, 432, 433, 434, 458, 472, 473, 474, 477, 478, 479, 481, 482, 483,
  492, 495, 496, 498, 500, 501, 502, 503, 507, 508, 512, 513, 514, 515, 516, 517,
  534, 535, 536, 537, 539, 540, 549, 557, 562, 563, 566, 618, 625, 626, 627, 628,
  633, 635, 641, 659, 661, 662, 666, 667, 668, 669, 670, 671, 672, 673, 674, 675,
  676, 677, 678, 679, 680, 681, 682, 683, 684, 685, 686, 687, 688, 689, 690, 691,
  692, 697, 708, 722, 726, 727, 728, 729, 730, 731, 732, 734, 735, 736, 737, 738,
  739, 740, 741, 742, 743, 744, 745, 746, 747, 749, 750
];
if (!LegacyBlockLightTileRegistry.RegisterDefaults().SetEquals(expectedLegacyBlockLight) ||
    LegacyBlockLightTileRegistry.IsBlockLightTile(162) ||
    LegacyBlockLightTileRegistry.IsBlockLightTile(541) ||
    LegacyBlockLightTileRegistry.IsBlockLightTile(634) ||
    LegacyBlockLightTileRegistry.IsBlockLightTile(718) ||
    !LegacyBlockLightTileRegistry.IsBlockLightTile(727) ||
    !LegacyBlockLightTileRegistry.IsBlockLightTile(732) ||
    !LegacyBlockLightTileRegistry.IsBlockLightTile(750) ||
    LegacyBlockLightTileRegistry.RegisterDefaults() is not FrozenSet<ushort>)
{
  throw new InvalidOperationException("Legacy block-light defaults drifted from Version4.");
}

Console.WriteLine("PASS: legacy block-light defaults preserve exact immutable tile registry");

HashSet<LegacyTileMergePair> expectedLegacyTileMerge =
[
  new(426, 727),
  new(727, 426),
  new(430, 728),
  new(728, 430),
  new(431, 729),
  new(729, 431),
  new(432, 730),
  new(730, 432),
  new(433, 731),
  new(731, 433),
  new(434, 732),
  new(732, 434)
];
if (LegacyTileMergeRegistry.TileTypeCount != 753 ||
    !LegacyTileMergeRegistry.RegisterDefaults().SetEquals(expectedLegacyTileMerge) ||
    LegacyTileMergeRegistry.RegisterDefaults().Count != 12 ||
    !LegacyTileMergeRegistry.CanMerge(426, 727) ||
    !LegacyTileMergeRegistry.CanMerge(727, 426) ||
    !LegacyTileMergeRegistry.CanMerge(434, 732) ||
    !LegacyTileMergeRegistry.CanMerge(732, 434) ||
    LegacyTileMergeRegistry.CanMerge(426, 728) ||
    LegacyTileMergeRegistry.CanMerge(0, 0) ||
    LegacyTileMergeRegistry.CanMerge(753, 426) ||
    LegacyTileMergeRegistry.RegisterDefaults() is not FrozenSet<LegacyTileMergePair>)
{
  throw new InvalidOperationException("Legacy tile-merge defaults drifted from Version4.");
}

Console.WriteLine(
  "PASS: legacy tile-merge defaults preserve the complete immutable sparse matrix projection");

if (tileMergeOnly)
{
  Console.WriteLine(
    "SUMMARY: tile-merge focused verification completed; remaining WorldGeneration checks " +
    "were intentionally not run");
  Environment.Exit(0);
}

HashSet<ushort> expectedLegacySolid =
[
  0, 1, 2, 6, 7, 8, 9, 10, 19, 22, 23, 25, 30, 37, 38, 39, 40, 41, 43, 44, 45, 46,
  47, 48, 53, 54, 56, 57, 58, 59, 60, 63, 64, 65, 66, 67, 68, 70, 75, 76, 107, 108,
  109, 111, 112, 116, 117, 118, 119, 120, 121, 122, 123, 127, 130, 137, 138, 140,
  145, 146, 147, 148, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161,
  162, 163, 164, 166, 167, 168, 169, 170, 175, 176, 177, 179, 180, 181, 182, 183,
  188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 202, 203, 204,
  206, 208, 211, 221, 222, 223, 224, 225, 226, 229, 230, 232, 234, 235, 239, 272,
  273, 274, 284, 311, 312, 313, 315, 321, 322, 325, 326, 327, 328, 329, 345, 346,
  347, 348, 350, 357, 367, 368, 369, 370, 371, 379, 380, 381, 383, 384, 385, 387,
  388, 396, 397, 398, 399, 400, 401, 402, 403, 404, 407, 408, 409, 415, 416, 417,
  418, 421, 422, 426, 427, 430, 431, 432, 433, 434, 446, 447, 448, 458, 459, 460,
  472, 473, 474, 476, 477, 478, 479, 481, 482, 483, 484, 492, 495, 496, 498, 500,
  501, 502, 503, 507, 508, 512, 513, 514, 515, 516, 517, 534, 535, 536, 537, 539,
  540, 541, 546, 557, 562, 563, 566, 618, 625, 626, 627, 628, 633, 635, 641, 659,
  661, 662, 664, 666, 667, 668, 669, 670, 671, 672, 673, 674, 675, 676, 677, 678,
  679, 680, 681, 682, 683, 684, 685, 686, 687, 688, 689, 690, 691, 692, 708, 711,
  712, 713, 714, 715, 716, 717, 718, 719, 722, 726, 734, 735, 736, 737, 738, 739,
  740, 741, 742, 743, 744, 745, 746, 747, 748, 749, 750
];
if (!LegacySolidTileRegistry.RegisterDefaults().SetEquals(expectedLegacySolid) ||
    LegacySolidTileRegistry.IsSolid(255))
{
  throw new InvalidOperationException("Legacy solid defaults drifted from Version4.");
}

HashSet<string> variationSeeds = new(StringComparer.Ordinal)
{
  "paint-everything-gray",
  "coat-everything-illuminant",
  "world-is-frozen",
  "surface-is-desert",
  "extra-floating-islands"
};
SecretSeedVariationSnapshot variations = SecretSeedVariationQuery.Evaluate(
  variationSeeds,
  activeSecretSeedCount: 3,
  skyblockWorld: true);
if (!variations.PaintEverythingGrayJustTheSurface || variations.PaintEverythingGrayJustTreasure ||
    !variations.PaintEverythingGrayUseWhite || !variations.ExtraFloatingIslandsNormalAmount ||
    variations.ExtraFloatingIslandsReducedAmount || !variations.SurfaceIsDesertNormalFunction ||
    variations.SurfaceIsDesertSwapDesertAndSnowBiomes || variations.ErrorWorldAdjustment(12) != 12)
{
  throw new InvalidOperationException("SecretSeed variation formulas did not preserve the legacy boundary.");
}

variations = SecretSeedVariationQuery.Evaluate(
  new HashSet<string>(StringComparer.Ordinal) { "no-spider-caves", "actually-no-traps" },
  activeSecretSeedCount: 4,
  skyblockWorld: false);
if (variations.NoSpiderCavesActuallyNoSpiderCaves || !variations.NoSpiderCavesILiedMoreSpiderCaves ||
    variations.ActuallyNoTrapsForRealIMeanIt || variations.ErrorWorldAdjustment(12) != 12)
{
  throw new InvalidOperationException("SecretSeed threshold formulas did not preserve the legacy boundary.");
}

variations = SecretSeedVariationQuery.Evaluate(
  new HashSet<string>(StringComparer.Ordinal),
  activeSecretSeedCount: 0,
  skyblockWorld: false);
if (variations.ErrorWorldAdjustment(999) != 4)
{
  throw new InvalidOperationException("SecretSeed adjustment did not preserve the inactive default.");
}

WorldMetadata skyblockMetadata = new(
  "skyblock-scan",
  new WorldSeed(1456),
  width: 200,
  height: 150);
WorldGrid skyblockWorld = new(200, 150);
if (!skyblockWorld.TrySetTile(40, 40, new WorldTile(true, Type: 26, WallType: 87)) ||
    !skyblockWorld.TrySetTile(41, 40, new WorldTile(true, Type: 58, WallType: 7)) ||
    !skyblockWorld.TrySetTile(42, 40, new WorldTile(true, Type: 77)))
{
  throw new InvalidOperationException("Skyblock verifier fixture could not write its scan input.");
}

TilePresenceScanResult skyblockScan = TilePresenceScanQuery.Scan(
  skyblockWorld.CreateSnapshot(skyblockMetadata));
SkyblockRuleSnapshot skyblockRules = SkyblockRuleQuery.Calculate(
  skyblockScan,
  new HashSet<ushort> { 41 },
  new HashSet<ushort> { 7 },
  isSkyblockWorld: true);
if (skyblockScan.ActiveTileCount != 3 || skyblockScan.WorldTileCount != 30000 ||
    skyblockRules.NoAltars || skyblockRules.NoDungeon || skyblockRules.NoTemple ||
    skyblockRules.NoHellstone || skyblockRules.NoHellforge || !skyblockRules.NoFossils ||
    !skyblockRules.NoLifeCrystals || !skyblockRules.LowTiles)
{
  throw new InvalidOperationException("Skyblock scan did not preserve the Version4 rule boundary.");
}

SkyblockPolicySnapshot skyblockPolicy = SkyblockPolicyQuery.Evaluate(
  isSkyblockWorld: true,
  new HashSet<string>(StringComparer.Ordinal) { "extra-living-trees" });
if (!skyblockPolicy.DenyFloatingIslands || !skyblockPolicy.DenyAllGeneration ||
    skyblockPolicy.DenySomeGeneration ||
    SkyblockPolicyQuery.Evaluate(false, new HashSet<string>(StringComparer.Ordinal)) != default)
{
  throw new InvalidOperationException("Skyblock policy did not preserve explicit rule inputs.");
}

SkyblockPolicySnapshot anniversarySkyblock = SkyblockPolicyQuery.Evaluate(
  true,
  new HashSet<string>(StringComparer.Ordinal),
  tenthAnniversaryWorld: true,
  getGoodWorld: true);
SkyblockPolicySnapshot goodSkyblock = SkyblockPolicyQuery.Evaluate(
  true,
  new HashSet<string>(StringComparer.Ordinal),
  tenthAnniversaryWorld: false,
  getGoodWorld: true);
if (!anniversarySkyblock.SpawnSolidifier || !anniversarySkyblock.SpawnShimmerPool ||
    goodSkyblock.SpawnSolidifier || goodSkyblock.SpawnShimmerPool)
{
  throw new InvalidOperationException("Skyblock special spawn properties diverged from legacy.");
}
Console.WriteLine("PASS: skyblock special spawn properties preserve rule gates");

if (!WorldDropPolicyQuery.ShouldDropItems(stopDrops: false, effectOnly: false, noItem: false) ||
    WorldDropPolicyQuery.ShouldDropItems(stopDrops: true, effectOnly: false, noItem: false) ||
    WorldDropPolicyQuery.ShouldDropItems(stopDrops: false, effectOnly: true, noItem: false) ||
    WorldDropPolicyQuery.ShouldDropItems(stopDrops: false, effectOnly: false, noItem: true) ||
    !WorldDropPolicyQuery.ShouldDropEffects(stopDrops: false, noItem: false) ||
    WorldDropPolicyQuery.ShouldDropEffects(stopDrops: true, noItem: false) ||
    WorldDropPolicyQuery.ShouldDropEffects(stopDrops: false, noItem: true))
{
  throw new InvalidOperationException("World drop policy did not preserve stopDrops/effectOnly gates.");
}
Console.WriteLine("PASS: world drop policy preserves stopDrops and effect-only gates");

ObjectDestructionGuardDecision destructionEntry = ObjectDestructionGuardPolicy.Evaluate(
  destroyObject: false);
ObjectDestructionGuardDecision nestedDestruction = ObjectDestructionGuardPolicy.Evaluate(
  destroyObject: true);
if (!destructionEntry.CanBegin || destructionEntry.SuppressNestedChecks ||
    nestedDestruction.CanBegin || !nestedDestruction.SuppressNestedChecks)
{
  throw new InvalidOperationException("Object destruction guard did not preserve nested-check suppression.");
}
Console.WriteLine("PASS: object destruction guard preserves nested reentrancy boundary");

List<ExploitDestroyQueueEntry> exploitDestroyQueue = new();
if (!ExploitDestroyQueuePolicy.TryEnqueue(exploitDestroyQueue, 10, 20, 4, capacity: 2) ||
    ExploitDestroyQueuePolicy.TryEnqueue(exploitDestroyQueue, 10, 20, 5, capacity: 2) ||
    !ExploitDestroyQueuePolicy.TryEnqueue(exploitDestroyQueue, 11, 20, 5, capacity: 2) ||
    ExploitDestroyQueuePolicy.TryEnqueue(exploitDestroyQueue, 12, 20, 6, capacity: 2) ||
    exploitDestroyQueue[0] != new ExploitDestroyQueueEntry(4, 10, 20))
{
  throw new InvalidOperationException("Exploit destroy queue contract drifted from bounded source semantics.");
}
Console.WriteLine("PASS: exploit destroy queue preserves typed capacity and coordinate deduplication");

WorldMetadata exploitDispatchMetadata = new(
  "exploit-dispatch", new WorldSeed(1456), width: 200, height: 150);
WorldGrid exploitDispatchWorld = new(
  exploitDispatchMetadata.Width,
  exploitDispatchMetadata.Height,
  initializeLegacyEmptyFrames: true);
_ = exploitDispatchWorld.TrySetTile(10, 20, new WorldTile(IsActive: true, Type: 1));
List<ExploitDestroyQueueEntry> exploitDispatchQueue = new()
{
  new ExploitDestroyQueueEntry(1, 10, 20),
  new ExploitDestroyQueueEntry(2, 11, 20),
  new ExploitDestroyQueueEntry(3, 500, 20)
};
ExploitDestroyQueueDispatch exploitDispatch = ExploitDestroyQueueDispatchPolicy.Create(
  exploitDispatchQueue,
  exploitDispatchWorld.CreateSnapshot(exploitDispatchMetadata));
if (exploitDispatch.ActiveEntries.Count != 1 ||
    exploitDispatch.ActiveEntries[0] != exploitDispatchQueue[0] ||
    exploitDispatch.FrameRequests.Count != 1 ||
    exploitDispatch.FrameRequests[0].Source != "worldgen.exploit-destroy" ||
    exploitDispatch.TileSquareIntents.Count != 1 ||
    exploitDispatch.TileSquareIntents[0] != new ExploitDestroyTileSquareIntent(10, 20))
{
  throw new InvalidOperationException(
    "Exploit destroy dispatch did not preserve active-tile framing and notification gates.");
}
Console.WriteLine(
  "PASS: exploit destroy dispatch emits frame and tile-square intents only for active tiles");

Queue<ExploitDestroyQueueEntry> drainQueue = new(exploitDispatchQueue);
ExploitDestroyQueueDispatch suppressedDispatch = ExploitDestroyQueueDispatchPolicy.Drain(
  drainQueue,
  exploitDispatchWorld.CreateSnapshot(exploitDispatchMetadata),
  destroyObject: true);
if (suppressedDispatch.ActiveEntries.Count != 0 || drainQueue.Count != exploitDispatchQueue.Count)
{
  throw new InvalidOperationException(
    "Exploit destroy dispatch drained the queue while nested object destruction was active.");
}

ExploitDestroyQueueDispatch drainedDispatch = ExploitDestroyQueueDispatchPolicy.Drain(
  drainQueue,
  exploitDispatchWorld.CreateSnapshot(exploitDispatchMetadata),
  destroyObject: false);
if (drainQueue.Count != 0 || drainedDispatch.ActiveEntries.Count != 1)
{
  throw new InvalidOperationException("Exploit destroy dispatch did not preserve dequeue semantics.");
}
Console.WriteLine("PASS: exploit destroy dispatch preserves nested suppression and dequeue lifecycle");

WorldGenerationStateComponent exploitFrameState = new(41);
List<TileFrameCommand> exploitFrameCommands = new();
if (!new ExploitDestroyQueueFrameSystem().TryCommitFrames(
      exploitDispatchWorld,
      drainedDispatch,
      ref exploitFrameState,
      exploitFrameCommands,
      out TileFrameCommitResult exploitFrameResult) ||
    !exploitFrameResult.Succeeded ||
    exploitFrameResult.AppliedCount != 1 ||
    exploitFrameCommands.Count != 1 ||
    exploitDispatchWorld.GetTile(10, 20).FrameX != 0)
{
  throw new InvalidOperationException(
    "Exploit destroy dispatch did not commit its typed frame command.");
}
Console.WriteLine("PASS: exploit destroy dispatch commits typed frame commands");

SavedOreTierDefaults oreTierDefaults = SavedOreTierDefaults.Version4;
if (oreTierDefaults != new SavedOreTierDefaults(7, 6, 9, 8, 107, 108, 111))
{
  throw new InvalidOperationException("SavedOreTiers did not preserve all Version4 defaults.");
}

WorldGenerationDistanceDefaults distanceDefaults = WorldGenerationDistanceDefaults.Version4;
if (distanceDefaults != new WorldGenerationDistanceDefaults(250, 380, 150, 50, 25, 25, 10))
{
  throw new InvalidOperationException("World-generation distance defaults changed from Version4.");
}

WorldMetadata distanceRequestMetadata = new(
  "DistanceRequest", new WorldSeed(1456), width: 200, height: 150,
  worldId: 52, seedVariant: "default");
WorldGenerationRequest distanceRequest = new(
  distanceRequestMetadata, spawnX: 100, surfaceY: 50);
if (distanceRequest.DistanceDefaults != distanceDefaults)
{
  throw new InvalidOperationException("World-generation requests did not freeze distance defaults.");
}

WorldGenerationRequest forcedEvilRequest = new(
  distanceRequestMetadata,
  spawnX: 100,
  surfaceY: 50,
  worldGenParamEvil: 1);
if (forcedEvilRequest.WorldGenParamEvil != 1)
{
  throw new InvalidOperationException("World-generation requests did not preserve evil selection input.");
}

try
{
  _ = new WorldGenerationDistanceDefaults(-1, 380, 150, 50, 25, 25, 10);
  throw new InvalidOperationException("Negative world-generation distances were accepted.");
}
catch (ArgumentOutOfRangeException)
{
}

WorldBoundsComponent distanceBounds = new(6400, 1800);
if (!WorldGenerationBoundaryQuery.IsInOceanBand(distanceBounds, 249, distanceDefaults) ||
    WorldGenerationBoundaryQuery.IsInOceanBand(distanceBounds, 250, distanceDefaults) ||
    !WorldGenerationBoundaryQuery.IsInOceanBand(distanceBounds, 6151, distanceDefaults) ||
    WorldGenerationBoundaryQuery.IsInBeachBand(distanceBounds, 380, distanceDefaults) ||
    !WorldGenerationBoundaryQuery.IsInBeachBand(distanceBounds, 379, distanceDefaults))
{
  throw new InvalidOperationException("World-generation distance boundaries changed from source predicates.");
}

try
{
  _ = WorldGenerationBoundaryQuery.IsInBeachBand(distanceBounds, -1, distanceDefaults);
  throw new InvalidOperationException("Out-of-bounds world-generation coordinates were accepted.");
}
catch (ArgumentOutOfRangeException)
{
}

WorldMetadata cactusSafetyMetadata = new(
  "CactusWaterSafety", new WorldSeed(1456), width: 200, height: 150,
  worldId: 51, seedVariant: "default");
WorldGrid cactusSafetyWorld = new(cactusSafetyMetadata.Width, cactusSafetyMetadata.Height);
for (int index = 0; index < 26; index++)
{
  _ = cactusSafetyWorld.TrySetLiquid(100 + index, 75, byte.MaxValue, 0);
}

CactusWaterSafetyResult cactusWater = CactusWaterSafetyQuery.Evaluate(
  cactusSafetyWorld.CreateSnapshot(cactusSafetyMetadata), 100, 75, distanceDefaults);
if (!cactusWater.ExceedsLimit || cactusWater.LiquidUnits != 26 * byte.MaxValue ||
    cactusWater.ScannedCellCount != 100 * 50)
{
  throw new InvalidOperationException("Cactus water accumulation diverged from source bounds.");
}

if (!CactusWaterSafetyQuery.ShouldBlockGrowth(cactusWater, false, 75, 100) ||
    !CactusWaterSafetyQuery.ShouldBlockGrowth(cactusWater, true, 101, 100) ||
    CactusWaterSafetyQuery.ShouldBlockGrowth(cactusWater, true, 99, 100))
{
  throw new InvalidOperationException("Cactus remix water exception diverged from source policy.");
}

if (!ShimmerSafetyQuery.IsWithinSafetyRadius(0, 149, 0, 0, distanceDefaults) ||
    ShimmerSafetyQuery.IsWithinSafetyRadius(0, 150, 0, 0, distanceDefaults) ||
    !ShimmerSafetyQuery.IsWithinSafetyRadius(90, 90, 0, 0, distanceDefaults))
{
  throw new InvalidOperationException("Shimmer safety distance diverged from source predicate.");
}

for (int y = 74; y <= 76; y++)
{
  for (int x = 99; x <= 101; x++)
  {
    _ = cactusSafetyWorld.TrySetTile(x, y, new WorldTile(true, 1));
  }
}

PreviousWorldBoundsSnapshot previousBounds = new(8400, 2400);
WorldBoundsComponent currentBounds = new(6400, 1800);
WorldBoundsRestartResult restartResult = WorldBoundsRestartQuery.Evaluate(
  previousBounds,
  currentBounds);
if (!restartResult.RequiresClear ||
    WorldBoundsRestartQuery.Evaluate(
      new PreviousWorldBoundsSnapshot(6400, 1800),
      currentBounds).RequiresClear)
{
  throw new InvalidOperationException("Previous world bounds restart policy diverged from source.");
}

WorldEvilSelection forcedCorruption = WorldEvilSelectionPolicy.Select(
  0,
  new LegacyPassRandomState(1456));
WorldEvilSelection forcedCrimson = WorldEvilSelectionPolicy.Select(
  1,
  new LegacyPassRandomState(1456));
WorldEvilSelection randomEvil = WorldEvilSelectionPolicy.Select(
  -1,
  new LegacyPassRandomState(1456));
if (forcedCorruption.Crimson || forcedCorruption.GeneratingRandomEvil ||
    !forcedCrimson.Crimson || forcedCrimson.GeneratingRandomEvil ||
    !randomEvil.GeneratingRandomEvil)
{
  throw new InvalidOperationException("World evil selection diverged from source branches.");
}

int[] alignmentCounts = new int[404];
alignmentCounts[23] = 5;
alignmentCounts[199] = 3;
alignmentCounts[109] = 10;
WorldInfectionAlignmentSnapshot alignment = WorldInfectionAlignmentQuery.Evaluate(
  alignmentCounts,
  totalSolid: 1000);
if (alignment.TotalEvil != 5 || alignment.TotalBlood != 3 || alignment.TotalGood != 10 ||
    alignment.EvilPercent != 1 || alignment.BloodPercent != 1 || alignment.GoodPercent != 1)
{
  throw new InvalidOperationException("World infection alignment percentages diverged from source rules.");
}

int[] remixAlignmentCounts = new int[TileDefinitionRegistry.Version4TileCount];
remixAlignmentCounts[474] = 7;
remixAlignmentCounts[195] = 11;
WorldInfectionAlignmentSnapshot remixAlignment = WorldInfectionAlignmentQuery.Evaluate(
  remixAlignmentCounts,
  totalSolid: 18,
  remixWorld: true);
if (remixAlignment.TotalEvil != 7 || remixAlignment.TotalBlood != 11)
{
  throw new InvalidOperationException("Remix world alignment IDs were not counted.");
}

TileTypeCountSnapshot tileCountSnapshot = TileTypeCountSnapshotQuery.Count(
  cactusSafetyWorld.CreateSnapshot(cactusSafetyMetadata),
  startX: 99,
  endX: 101,
  startY: 74,
  endY: 76);
if (tileCountSnapshot.Counts[1] != 9 || tileCountSnapshot.TotalCount != 9)
{
  throw new InvalidOperationException("Tile count snapshot diverged from bounded scan semantics.");
}

int[] mutableCounts = new int[753];
mutableCounts[23] = 2;
TileTypeCountSnapshot copiedTileCounts = new(mutableCounts);
mutableCounts[23] = 0;
if (copiedTileCounts.Counts[23] != 2)
{
  throw new InvalidOperationException("Tile count snapshot retained mutable source storage.");
}

TileTypeCountSnapshot emptyTileCounts = default;
if (emptyTileCounts.TotalCount != 0 || emptyTileCounts.Counts.Count != 0)
{
  throw new InvalidOperationException("Default tile count snapshot was not an empty immutable value.");
}

ShadowOrbBreakProgression secondOrb = ShadowOrbBreakProgressionPolicy.Apply(
  1,
  dontStarveWorld: false,
  getGoodWorld: false,
  remixWorld: false);
ShadowOrbBreakProgression specialWorldOrb = ShadowOrbBreakProgressionPolicy.Apply(
  0,
  dontStarveWorld: true,
  getGoodWorld: true,
  remixWorld: false);
if (!secondOrb.ShadowOrbSmashed || secondOrb.NextShadowOrbCount != 2 ||
    secondOrb.ShouldAttemptBossSpawn || !specialWorldOrb.ShouldAttemptBossSpawn)
{
  throw new InvalidOperationException("Shadow orb progression diverged from source threshold rules.");
}

AltarBreakProgression altarBreak = AltarBreakProgressionPolicy.Apply(4);
if (altarBreak.PreviousCount != 4 || altarBreak.NextCount != 5 ||
    altarBreak.CycleIndex != 1 || altarBreak.CycleNumber != 2)
{
  throw new InvalidOperationException("Altar progression diverged from source cycle arithmetic.");
}

HardmodeOreTierSelection cobaltSelection = HardmodeOreTierSelectionPolicy.Apply(
  AltarBreakProgressionPolicy.Apply(0),
  HardmodeOreTierState.Uninitialized,
  isDrunkWorld: false,
  new LegacyPassRandomState(1456));
if (!cobaltSelection.WasInitialized ||
    (cobaltSelection.SelectedTileType != 107 && cobaltSelection.SelectedTileType != 221) ||
    cobaltSelection.State.CobaltTileType != cobaltSelection.SelectedTileType)
{
  throw new InvalidOperationException("Cobalt altar selection diverged from source initialization.");
}

HardmodeOreTierSelection mythrilSelection = HardmodeOreTierSelectionPolicy.Apply(
  AltarBreakProgressionPolicy.Apply(1),
  cobaltSelection.State,
  isDrunkWorld: false,
  new LegacyPassRandomState(1456));
if (!mythrilSelection.WasInitialized ||
    (mythrilSelection.SelectedTileType != 108 && mythrilSelection.SelectedTileType != 222))
{
  throw new InvalidOperationException("Mythril altar selection diverged from source initialization.");
}

HardmodeOreTierSelection adamantiteSelection = HardmodeOreTierSelectionPolicy.Apply(
  AltarBreakProgressionPolicy.Apply(2),
  mythrilSelection.State,
  isDrunkWorld: true,
  new LegacyPassRandomState(1456));
if (!adamantiteSelection.WasInitialized || !adamantiteSelection.WasToggled ||
    (adamantiteSelection.SelectedTileType != 111 &&
     adamantiteSelection.SelectedTileType != 223))
{
  throw new InvalidOperationException("Drunk-world hardmode ore selection diverged from source toggles.");
}

HardmodeOreTierSelection repeatedAdamantiteSelection = HardmodeOreTierSelectionPolicy.Apply(
  AltarBreakProgressionPolicy.Apply(5),
  adamantiteSelection.State,
  isDrunkWorld: true,
  new LegacyPassRandomState(1456));
if (repeatedAdamantiteSelection.WasInitialized ||
    repeatedAdamantiteSelection.SelectedTileType == adamantiteSelection.SelectedTileType)
{
  throw new InvalidOperationException("Drunk-world hardmode ore repeat did not toggle Adamantite.");
}
Console.WriteLine("PASS: hardmode ore altar policy preserves saved tier initialization and toggles");

WorldUpdatePolicySnapshot normalWorldUpdate = WorldUpdatePolicyQuery.Evaluate(
  hardMode: false,
  remixWorld: false,
  getGoodWorld: false,
  tenthAnniversaryWorld: false,
  stopBiomeSpreadPowerEnabled: false,
  notTheBeesWorld: true,
  phase: WorldUpdatePhase.Overground);
WorldUpdatePolicySnapshot remixWorldUpdate = WorldUpdatePolicyQuery.Evaluate(
  hardMode: false,
  remixWorld: true,
  getGoodWorld: true,
  tenthAnniversaryWorld: false,
  stopBiomeSpreadPowerEnabled: true,
  notTheBeesWorld: false,
  phase: WorldUpdatePhase.Underground);
WorldUpdatePolicySnapshot anniversaryWorldUpdate = WorldUpdatePolicyQuery.Evaluate(
  hardMode: false,
  remixWorld: true,
  getGoodWorld: true,
  tenthAnniversaryWorld: true,
  stopBiomeSpreadPowerEnabled: false,
  notTheBeesWorld: false,
  phase: WorldUpdatePhase.Underground);
if (normalWorldUpdate.HardModeWorldUpdates || normalWorldUpdate.GrowGrassUnderground ||
    !normalWorldUpdate.AllowedToSpreadInfections ||
    !remixWorldUpdate.HardModeWorldUpdates || !remixWorldUpdate.GrowGrassUnderground ||
    remixWorldUpdate.AllowedToSpreadInfections || anniversaryWorldUpdate.HardModeWorldUpdates)
{
  throw new InvalidOperationException("World update policy diverged from source phase rules.");
}
Console.WriteLine("PASS: world update policy preserves hardmode, infection, and grass phase rules");

FossilBreakDecision fossilBreakStart = FossilBreakPolicy.Evaluate(
  FossilBreakPolicy.FossilTileType,
  fossilBreak: false,
  belowTileIsSolid: true,
  isTopNeighbor: true,
  fail: false);
FossilBreakDecision fossilBreakBlocked = FossilBreakPolicy.Evaluate(
  FossilBreakPolicy.FossilTileType,
  fossilBreak: true,
  belowTileIsSolid: true,
  isTopNeighbor: false,
  fail: true);
FossilBreakDecision fossilBreakOpen = FossilBreakPolicy.Evaluate(
  tileType: 1,
  fossilBreak: false,
  belowTileIsSolid: false,
  isTopNeighbor: false,
  fail: false);
if (!fossilBreakStart.CanBegin || fossilBreakStart.RollExclusiveUpperBound != 4 ||
    fossilBreakBlocked.CanBegin || fossilBreakOpen.CanBegin ||
    fossilBreakOpen.RollExclusiveUpperBound != 4)
{
  throw new InvalidOperationException("Fossil break policy diverged from source guards.");
}
Console.WriteLine("PASS: fossil break policy preserves reentrancy and neighborhood roll guards");

RainingBoulderStateTransition rainingBoulders = RainingBoulderStatePolicy.Evaluate(
  previousIsRainingBoulders: false,
  drunkWorld: true,
  getGoodWorld: true,
  remixWorld: false,
  isStorming: true);
RainingBoulderStateTransition endedBoulders = RainingBoulderStatePolicy.Evaluate(
  previousIsRainingBoulders: true,
  drunkWorld: true,
  getGoodWorld: true,
  remixWorld: false,
  isStorming: false);
RainingBoulderStateTransition remixBoulders = RainingBoulderStatePolicy.Evaluate(
  previousIsRainingBoulders: true,
  drunkWorld: true,
  getGoodWorld: true,
  remixWorld: true,
  isStorming: true);
if (!rainingBoulders.IsRainingBoulders || rainingBoulders.HasEnded ||
    endedBoulders.IsRainingBoulders || !endedBoulders.HasEnded ||
    remixBoulders.IsRainingBoulders || !remixBoulders.HasEnded)
{
  throw new InvalidOperationException("Raining boulder state diverged from source eligibility.");
}
Console.WriteLine("PASS: raining boulder policy preserves world-rule eligibility and end transition");

TrapGenerationGate normalTrapGate = TrapGenerationGatePolicy.Evaluate(
  denySomeGeneration: false,
  actuallyNoTrapsForReal: false,
  notTheBees: false,
  noTrapsWorldGen: false,
  remixWorldGen: false);
TrapGenerationGate beesTrapGate = TrapGenerationGatePolicy.Evaluate(
  denySomeGeneration: false,
  actuallyNoTrapsForReal: false,
  notTheBees: true,
  noTrapsWorldGen: false,
  remixWorldGen: false);
TrapGenerationGate remixBeesTrapGate = TrapGenerationGatePolicy.Evaluate(
  denySomeGeneration: false,
  actuallyNoTrapsForReal: false,
  notTheBees: true,
  noTrapsWorldGen: false,
  remixWorldGen: true);
TrapGenerationGate deniedTrapGate = TrapGenerationGatePolicy.Evaluate(
  denySomeGeneration: true,
  actuallyNoTrapsForReal: false,
  notTheBees: false,
  noTrapsWorldGen: true,
  remixWorldGen: true);
if (!normalTrapGate.ShouldRun || !normalTrapGate.PlacingTraps ||
    beesTrapGate.ShouldRun || !remixBeesTrapGate.ShouldRun || deniedTrapGate.ShouldRun)
{
  throw new InvalidOperationException("Trap generation gate diverged from source conditions.");
}
Console.WriteLine("PASS: trap generation gate preserves skyblock and secret-seed exclusions");

TileCountSchedulingState tileCountState = TileCountSchedulingState.Initial;
for (int update = 0; update < TileCountSchedulingPolicy.UpdatesPerColumnScan - 1; update++)
{
  TileCountSchedulingResult pendingScan = TileCountSchedulingPolicy.Advance(tileCountState, 3);
  if (pendingScan.ShouldScanColumn || pendingScan.State.UpdateCounter != update + 1)
  {
    throw new InvalidOperationException("Tile count cadence advanced before the source threshold.");
  }

  tileCountState = pendingScan.State;
}

TileCountSchedulingResult firstColumnScan = TileCountSchedulingPolicy.Advance(tileCountState, 3);
tileCountState = firstColumnScan.State;
for (int update = 0; update < TileCountSchedulingPolicy.UpdatesPerColumnScan - 1; update++)
{
  tileCountState = TileCountSchedulingPolicy.Advance(tileCountState, 3).State;
}

TileCountSchedulingResult secondColumnScan = TileCountSchedulingPolicy.Advance(tileCountState, 3);
if (!firstColumnScan.ShouldScanColumn || firstColumnScan.ColumnX != 0 ||
    !secondColumnScan.ShouldScanColumn || secondColumnScan.ColumnX != 1 ||
    secondColumnScan.State.ColumnX != 2)
{
  throw new InvalidOperationException("Tile count scheduling diverged from source cadence or wrap.");
}
Console.WriteLine("PASS: tile count scheduling preserves 30-update cadence and column wrap");

(TileCountColumnRange aboveSurface, TileCountColumnRange belowSurface) =
  TileCountColumnRangePolicy.Create(worldHeight: 240, worldSurfaceY: 100);
if (aboveSurface.StartY != 40 || aboveSurface.EndExclusiveY != 101 || aboveSurface.Weight != 5 ||
    belowSurface.StartY != 101 || belowSurface.EndExclusiveY != 200 || belowSurface.Weight != 1)
{
  throw new InvalidOperationException("Tile count column ranges diverged from source bounds.");
}
Console.WriteLine("PASS: tile count column ranges preserve source surface and border bounds");

WorldInfectionAlignmentSnapshot alignmentColumn = new(2, 1, 3, 100);
WorldInfectionAlignmentAccumulatorResult accumulatedAlignment =
  WorldInfectionAlignmentAccumulatorPolicy.AddColumn(
    WorldInfectionAlignmentAccumulator.Empty,
    alignmentColumn,
    publishBeforeColumn: false);
WorldInfectionAlignmentAccumulatorResult publishedAlignment =
  WorldInfectionAlignmentAccumulatorPolicy.AddColumn(
    accumulatedAlignment.State,
    alignmentColumn,
    publishBeforeColumn: true);
if (!publishedAlignment.HasPublished || publishedAlignment.Published.TotalEvil != 2 ||
    publishedAlignment.Published.TotalBlood != 1 || publishedAlignment.Published.TotalGood != 3 ||
    publishedAlignment.Published.TotalSolid != 100 ||
    publishedAlignment.State.TotalEvil != 2 || publishedAlignment.State.TotalSolid != 100)
{
  throw new InvalidOperationException("World infection alignment accumulation diverged from source reset.");
}
Console.WriteLine("PASS: world infection alignment accumulator preserves column publish and reset");

WorldMetadata alignmentScanMetadata = new(
  "alignment-scan", new WorldSeed(1456), width: 200, height: 150);
WorldGrid alignmentScanWorld = new(
  alignmentScanMetadata.Width,
  alignmentScanMetadata.Height,
  initializeLegacyEmptyFrames: true);
_ = alignmentScanWorld.TrySetTile(10, 40, new WorldTile(IsActive: true, Type: 109));
_ = alignmentScanWorld.TrySetTile(10, 81, new WorldTile(IsActive: true, Type: 1));
WorldInfectionAlignmentSnapshot scannedAlignment = WorldInfectionAlignmentScanQuery.ScanColumn(
  alignmentScanWorld.CreateSnapshot(alignmentScanMetadata),
  columnX: 10,
  surfaceY: 80);
if (scannedAlignment.TotalGood != 5 ||
    scannedAlignment.TotalSolid != 6 ||
    scannedAlignment.GoodPercent != 83)
{
  throw new InvalidOperationException(
    "World infection alignment scan did not preserve surface/cavern weighting.");
}
Console.WriteLine(
  "PASS: world infection alignment scan preserves weighted ranges and source denominator");

WorldMetadata remixAlignmentMetadata = new(
  "alignment-remix-scan",
  new WorldSeed(1456),
  width: 200,
  height: 150,
  isRemixWorld: true);
WorldGrid remixAlignmentWorld = new(
  remixAlignmentMetadata.Width,
  remixAlignmentMetadata.Height,
  initializeLegacyEmptyFrames: true);
_ = remixAlignmentWorld.TrySetTile(10, 40, new WorldTile(IsActive: true, Type: 474));
_ = remixAlignmentWorld.TrySetTile(10, 81, new WorldTile(IsActive: true, Type: 195));
WorldInfectionAlignmentSnapshot scannedRemixAlignment =
  WorldInfectionAlignmentScanQuery.ScanColumn(
    remixAlignmentWorld.CreateSnapshot(remixAlignmentMetadata),
    columnX: 10,
    surfaceY: 80);
if (scannedRemixAlignment.TotalEvil != 5 ||
    scannedRemixAlignment.TotalBlood != 1 ||
    scannedRemixAlignment.TotalSolid != 6)
{
  throw new InvalidOperationException(
    "Remix world infection alignment scan did not preserve special tile IDs.");
}
Console.WriteLine("PASS: remix world infection alignment scan preserves 474/195 branches");

WorldGrid alignmentPipelineWorld = new(
  alignmentScanMetadata.Width,
  alignmentScanMetadata.Height,
  initializeLegacyEmptyFrames: true);
_ = alignmentPipelineWorld.TrySetTile(0, 40, new WorldTile(IsActive: true, Type: 109));
_ = alignmentPipelineWorld.TrySetTile(0, 81, new WorldTile(IsActive: true, Type: 1));
_ = alignmentPipelineWorld.TrySetTile(1, 40, new WorldTile(IsActive: true, Type: 23));
_ = alignmentPipelineWorld.TrySetTile(1, 81, new WorldTile(IsActive: true, Type: 1));
WorldGridSnapshot alignmentPipelineSnapshot = alignmentPipelineWorld.CreateSnapshot(
  alignmentScanMetadata);
WorldInfectionAlignmentScanState alignmentPipelineState =
  new(new TileCountSchedulingState(29, 0), WorldInfectionAlignmentAccumulator.Empty);
WorldInfectionAlignmentScanResult firstAlignmentColumn =
  WorldInfectionAlignmentScanPolicy.Advance(
    alignmentPipelineState,
    alignmentPipelineSnapshot,
    surfaceY: 80);
if (!firstAlignmentColumn.Scanned || firstAlignmentColumn.ColumnX != 0 ||
    !firstAlignmentColumn.HasPublished || firstAlignmentColumn.Published.TotalSolid != 0 ||
    firstAlignmentColumn.Column.TotalGood != 5 ||
    firstAlignmentColumn.State.Accumulator.TotalSolid != 6)
{
  throw new InvalidOperationException(
    "World infection alignment scan policy did not preserve the first-column boundary.");
}

WorldInfectionAlignmentScanResult pendingAlignmentColumn =
  WorldInfectionAlignmentScanPolicy.Advance(
    firstAlignmentColumn.State,
    alignmentPipelineSnapshot,
    surfaceY: 80);
if (pendingAlignmentColumn.Scanned || pendingAlignmentColumn.State.Scheduling.UpdateCounter != 1)
{
  throw new InvalidOperationException(
    "World infection alignment scan policy did not preserve the 30-update cadence.");
}

WorldInfectionAlignmentScanState secondColumnState = new(
  new TileCountSchedulingState(29, 1),
  firstAlignmentColumn.State.Accumulator);
WorldInfectionAlignmentScanResult secondAlignmentColumn =
  WorldInfectionAlignmentScanPolicy.Advance(
    secondColumnState,
    alignmentPipelineSnapshot,
    surfaceY: 80);
WorldInfectionAlignmentScanState publishColumnState = new(
  new TileCountSchedulingState(29, 0),
  secondAlignmentColumn.State.Accumulator);
WorldInfectionAlignmentScanResult publishedAlignmentCycle =
  WorldInfectionAlignmentScanPolicy.Advance(
    publishColumnState,
    alignmentPipelineSnapshot,
    surfaceY: 80);
if (!secondAlignmentColumn.Scanned || secondAlignmentColumn.ColumnX != 1 ||
    secondAlignmentColumn.HasPublished ||
    !publishedAlignmentCycle.HasPublished ||
    publishedAlignmentCycle.Published.TotalEvil != 5 ||
    publishedAlignmentCycle.Published.TotalGood != 5 ||
    publishedAlignmentCycle.Published.TotalSolid != 12 ||
    publishedAlignmentCycle.State.Accumulator.TotalGood != 5)
{
  throw new InvalidOperationException(
    "World infection alignment scan policy did not preserve cross-column publication.");
}
Console.WriteLine(
  "PASS: world infection alignment scan policy preserves cadence, accumulation, and publish reset");

WorldLoadRecoveryDecision initialLoadSuccess = WorldLoadRecoveryPolicy.Evaluate(
  initialLoadFailed: false,
  retryLoadFailed: false,
  backupExists: false,
  backupLoadFailed: false);
WorldLoadRecoveryDecision primaryRetrySuccess = WorldLoadRecoveryPolicy.Evaluate(
  initialLoadFailed: true,
  retryLoadFailed: false,
  backupExists: false,
  backupLoadFailed: false);
WorldLoadRecoveryDecision noBackupFailure = WorldLoadRecoveryPolicy.Evaluate(
  initialLoadFailed: true,
  retryLoadFailed: true,
  backupExists: false,
  backupLoadFailed: false);
WorldLoadRecoveryDecision backupSuccess = WorldLoadRecoveryPolicy.Evaluate(
  initialLoadFailed: true,
  retryLoadFailed: true,
  backupExists: true,
  backupLoadFailed: false);
WorldLoadRecoveryDecision backupFailure = WorldLoadRecoveryPolicy.Evaluate(
  initialLoadFailed: true,
  retryLoadFailed: true,
  backupExists: true,
  backupLoadFailed: true);
if (!initialLoadSuccess.LoadSucceeded || primaryRetrySuccess.RestoreBackup ||
    !primaryRetrySuccess.RetryPrimary || noBackupFailure.LoadSucceeded ||
    !backupSuccess.RestoreBackup || !backupSuccess.RetryBackup ||
    !backupSuccess.LoadSucceeded || backupFailure.LoadSucceeded)
{
  throw new InvalidOperationException("World load recovery policy diverged from source retries.");
}
Console.WriteLine("PASS: world load recovery policy preserves retry and backup boundaries");

GrassSpreadRecursionState grassSpreadState = GrassSpreadRecursionState.Initial;
if (!GrassSpreadRecursionPolicy.ShouldRecurse(grassSpreadState, repeat: true))
{
  throw new InvalidOperationException("Grass spread recursion did not open at depth zero.");
}

grassSpreadState = GrassSpreadRecursionPolicy.Enter(grassSpreadState);
grassSpreadState = GrassSpreadRecursionPolicy.Exit(grassSpreadState);
GrassSpreadRecursionState maximumGrassSpreadState =
  new(GrassSpreadRecursionPolicy.MaximumDepth);
if (GrassSpreadRecursionPolicy.ShouldRecurse(maximumGrassSpreadState, repeat: true))
{
  throw new InvalidOperationException("Grass spread recursion exceeded the source depth cap.");
}
Console.WriteLine("PASS: grass spread recursion policy preserves repeat depth cap and unwind");

TileCountVisitDecision countVisitAllowed = TileCountVisitPolicy.Evaluate(
  x: 10,
  y: 10,
  worldWidth: 100,
  worldHeight: 100,
  wallType: 0,
  hasShimmer: false,
  hasLiquid: false,
  jungle: false,
  lavaAllowed: false,
  hasLava: false);
TileCountVisitDecision countVisitWall = TileCountVisitPolicy.Evaluate(
  10, 10, 100, 100, 1, false, false, false, false, false);
TileCountVisitDecision countVisitLava = TileCountVisitPolicy.Evaluate(
  10, 10, 100, 100, 0, false, true, false, false, true);
TileCountVisitDecision countVisitEdge = TileCountVisitPolicy.Evaluate(
  1, 10, 100, 100, 0, false, false, false, false, false);
if (countVisitAllowed.RejectionReason != TileCountVisitRejectionReason.None ||
    countVisitWall.RejectionReason != TileCountVisitRejectionReason.Wall ||
    countVisitLava.RejectionReason != TileCountVisitRejectionReason.LavaLiquid ||
    countVisitEdge.RejectionReason != TileCountVisitRejectionReason.OutsideInterior)
{
  throw new InvalidOperationException("Tile count visit gates diverged from nextCount source guards.");
}
Console.WriteLine("PASS: tile count visit policy preserves edge, wall, shimmer, and lava gates");

TileCountVisitedSnapshot visitedTiles = new(
  new HashSet<(int X, int Y)> { (12, 18) });
TileCountVisitedResult existingVisitedTile = TileCountVisitedPolicy.Visit(visitedTiles, 12, 18);
TileCountVisitedResult newVisitedTile = TileCountVisitedPolicy.Visit(existingVisitedTile.State, 13, 18);
if (!existingVisitedTile.WasAlreadyVisited || newVisitedTile.WasAlreadyVisited ||
    !newVisitedTile.State.Contains(13, 18) || newVisitedTile.State.Coordinates.Count != 2)
{
  throw new InvalidOperationException("Tile count visited snapshot diverged from CountedTiles semantics.");
}
Console.WriteLine("PASS: tile count visited policy preserves immutable deduplication");

TileCountCapacityDecision openTileCountCapacity = TileCountCapacityPolicy.Evaluate(2, 3);
TileCountCapacityDecision incrementedTileCountCapacity = TileCountCapacityPolicy.Increment(
  openTileCountCapacity);
TileCountCapacityDecision saturatedTileCountCapacity = TileCountCapacityPolicy.Increment(
  incrementedTileCountCapacity);
if (!openTileCountCapacity.CanVisit || incrementedTileCountCapacity.Count != 3 ||
    !saturatedTileCountCapacity.IsSaturated || saturatedTileCountCapacity.CanVisit ||
    saturatedTileCountCapacity.Count != 3 ||
    !TileCountCapacityPolicy.Evaluate(3, 3).IsSaturated)
{
  throw new InvalidOperationException("Tile count capacity diverged from nextCount saturation semantics.");
}
Console.WriteLine("PASS: tile count capacity policy preserves maximum saturation");

TileCountEnvironmentCounters environmentCounters =
  TileCountEnvironmentCounterPolicy.AddActiveTile(
    TileCountEnvironmentCounters.Empty,
    tileType: 70);
environmentCounters = TileCountEnvironmentCounterPolicy.AddActiveTile(environmentCounters, 1);
environmentCounters = TileCountEnvironmentCounterPolicy.AddActiveTile(environmentCounters, 147);
environmentCounters = TileCountEnvironmentCounterPolicy.AddActiveTile(environmentCounters, 53);
environmentCounters = TileCountEnvironmentCounterPolicy.AddLavaLiquid(environmentCounters);
if (environmentCounters.ShroomCount != 1 || environmentCounters.RockCount != 1 ||
    environmentCounters.IceCount != 1 || environmentCounters.SandCount != 1 ||
    environmentCounters.LavaCount != 1)
{
  throw new InvalidOperationException("Tile count environment counters diverged from source types.");
}
Console.WriteLine("PASS: tile count environment counters preserve source classification");

WallSpreadBudgetDecision wallSpreadBudget = WallSpreadBudgetPolicy.Create(maximumCount: 2);
wallSpreadBudget = WallSpreadBudgetPolicy.Apply(wallSpreadBudget);
WallSpreadBudgetDecision exhaustedWallSpreadBudget = WallSpreadBudgetPolicy.Apply(wallSpreadBudget);
if (wallSpreadBudget.AppliedCount != 1 || !wallSpreadBudget.CanSpread ||
    !exhaustedWallSpreadBudget.IsExhausted || exhaustedWallSpreadBudget.CanSpread ||
    exhaustedWallSpreadBudget.AppliedCount != 2 ||
    WallSpreadBudgetPolicy.Create().MaximumCount != WallSpreadBudgetPolicy.Version4MaximumCount)
{
  throw new InvalidOperationException("Wall spread budget diverged from Wall2 limit semantics.");
}
Console.WriteLine("PASS: wall spread budget preserves maxWallOut2 saturation");

DirtCountVisitDecision dirtCountAllowed = DirtCountVisitPolicy.Evaluate(
  10, 10, 100, 100, false, 0, 2, false);
DirtCountVisitDecision dirtCountIce = DirtCountVisitPolicy.Evaluate(
  10, 10, 100, 100, true, 147, 2, false);
DirtCountVisitDecision dirtCountProtected = DirtCountVisitPolicy.Evaluate(
  10, 10, 100, 100, false, 0, 244, false);
DirtCountVisitDecision dirtCountSolid = DirtCountVisitPolicy.Evaluate(
  10, 10, 100, 100, false, 0, 2, true);
DirtCountVisitDecision dirtCountEdge = DirtCountVisitPolicy.Evaluate(
  1, 10, 100, 100, false, 0, 2, false);
if (!dirtCountAllowed.CanCount || dirtCountAllowed.RejectionReason != DirtCountRejectionReason.None ||
    dirtCountIce.RejectionReason != DirtCountRejectionReason.IceTile ||
    dirtCountProtected.RejectionReason != DirtCountRejectionReason.ProtectedWall ||
    dirtCountSolid.RejectionReason != DirtCountRejectionReason.SolidTile ||
    dirtCountEdge.RejectionReason != DirtCountRejectionReason.OutsideInterior)
{
  throw new InvalidOperationException("Dirt count visit gates diverged from nextDirtCount.");
}
Console.WriteLine("PASS: dirt count visit policy preserves ice, wall, solid, and edge gates");

HousingRoomStartDecision validRoomStart = HousingRoomStartPolicy.Evaluate(
  20, 20, 100, 100, isActiveSolid: false);
HousingRoomStartDecision edgeRoomStart = HousingRoomStartPolicy.Evaluate(
  9, 20, 100, 100, isActiveSolid: false);
HousingRoomStartDecision solidRoomStart = HousingRoomStartPolicy.Evaluate(
  20, 20, 100, 100, isActiveSolid: true);
if (!validRoomStart.CanStart || edgeRoomStart.RejectionReason !=
    HousingRoomStartRejectionReason.TooCloseToWorldEdge ||
    solidRoomStart.RejectionReason != HousingRoomStartRejectionReason.StartedInSolidTile)
{
  throw new InvalidOperationException("Housing room start gates diverged from CheckRoom.");
}
Console.WriteLine("PASS: housing room start policy preserves edge and solid-start gates");

HousingWallSafetyDecision safeHousingWall = HousingWallSafetyPolicy.Evaluate(
  hasHorizontalHousingBoundary: true,
  hasVerticalHousingBoundary: true,
  currentTileHasWall: false);
HousingWallSafetyDecision unsafeHousingWall = HousingWallSafetyPolicy.Evaluate(
  hasHorizontalHousingBoundary: false,
  hasVerticalHousingBoundary: true,
  currentTileHasWall: true);
HousingWallSafetyDecision missingHousingWall = HousingWallSafetyPolicy.Evaluate(
  hasHorizontalHousingBoundary: true,
  hasVerticalHousingBoundary: false,
  currentTileHasWall: false);
if (!safeHousingWall.IsSafe || unsafeHousingWall.RejectionReason !=
    HousingWallSafetyRejectionReason.TooManyUnsafeWalls ||
    missingHousingWall.RejectionReason != HousingWallSafetyRejectionReason.HoleInWallIsTooBig)
{
  throw new InvalidOperationException("Housing wall safety diverged from CheckRoom boundary rules.");
}
Console.WriteLine("PASS: housing wall safety policy preserves unsafe-wall and missing-wall reasons");

HousingRoomMinimumSizeDecision smallRoom = HousingRoomMinimumSizePolicy.Evaluate(59);
HousingRoomMinimumSizeDecision validRoom = HousingRoomMinimumSizePolicy.Evaluate(60);
if (!smallRoom.IsTooSmall || smallRoom.CanSpawn || validRoom.IsTooSmall ||
    !validRoom.CanSpawn || validRoom.MinimumRoomTileCount != 60)
{
  throw new InvalidOperationException("Housing room minimum size diverged from CheckRoom.");
}
Console.WriteLine("PASS: housing room minimum-size policy preserves 60-tile spawn threshold");

HousingBlockingTileDecision blockingSolid = HousingBlockingTilePolicy.Evaluate(1, true, 0, 0);
HousingBlockingTileDecision openGate = HousingBlockingTilePolicy.Evaluate(11, false, 54, 0);
HousingBlockingTileDecision closedGate = HousingBlockingTilePolicy.Evaluate(11, false, 36, 0);
HousingBlockingTileDecision specialGate = HousingBlockingTilePolicy.Evaluate(386, false, 36, 0);
if (blockingSolid.Reason != HousingBlockingTileReason.BlockingWall ||
    openGate.Reason != HousingBlockingTileReason.BlockingOpenGate ||
    closedGate.ShouldStop || specialGate.Reason != HousingBlockingTileReason.BlockingOpenGate)
{
  throw new InvalidOperationException("Housing blocking tile policy diverged from CheckRoom gates.");
}
Console.WriteLine("PASS: housing blocking tile policy preserves solid and open-gate stops");

HousingStinkbugSpawnDecision stinkbugBlocked = HousingStinkbugSpawnPolicy.Evaluate(
  hasStinkbug: true,
  hasEchoStinkbug: false,
  hasTownPetRoom: false);
HousingStinkbugSpawnDecision petRoomAllowed = HousingStinkbugSpawnPolicy.Evaluate(
  hasStinkbug: true,
  hasEchoStinkbug: true,
  hasTownPetRoom: true);
HousingStinkbugSpawnDecision cleanRoom = HousingStinkbugSpawnPolicy.Evaluate(
  hasStinkbug: false,
  hasEchoStinkbug: false,
  hasTownPetRoom: false);
if (!stinkbugBlocked.IsBlocked || !stinkbugBlocked.HasStinkbug ||
    petRoomAllowed.IsBlocked || !petRoomAllowed.HasEchoStinkbug || cleanRoom.IsBlocked)
{
  throw new InvalidOperationException("Housing stinkbug spawn gate diverged from source.");
}
Console.WriteLine("PASS: housing stinkbug spawn policy preserves town-pet exception");

HousingScoreEligibilityDecision positiveHousingScore = HousingScoreEligibilityPolicy.Evaluate(1);
HousingScoreEligibilityDecision zeroHousingScore = HousingScoreEligibilityPolicy.Evaluate(0);
HousingScoreEligibilityDecision negativeHousingScore = HousingScoreEligibilityPolicy.Evaluate(-1);
if (!positiveHousingScore.CanSpawn || positiveHousingScore.IsRejected ||
    zeroHousingScore.CanSpawn || !zeroHousingScore.IsRejected ||
    negativeHousingScore.CanSpawn || !negativeHousingScore.IsRejected)
{
  throw new InvalidOperationException("Housing score gate diverged from SpawnTownNPC.");
}
Console.WriteLine("PASS: housing score eligibility preserves positive-score spawn gate");

HousingAlternateSpotDecision alternateSpot = HousingAlternateSpotPolicy.Evaluate(true, false);
HousingAlternateSpotDecision guardedAlternateSpot = HousingAlternateSpotPolicy.Evaluate(true, true);
HousingAlternateSpotDecision missingAlternateSpot = HousingAlternateSpotPolicy.Evaluate(false, false);
if (!alternateSpot.CanTryAlternateSpot || !alternateSpot.HasAlternateSpot || alternateSpot.IsAlreadyTrying ||
    guardedAlternateSpot.CanTryAlternateSpot || !guardedAlternateSpot.IsAlreadyTrying ||
    missingAlternateSpot.CanTryAlternateSpot)
{
  throw new InvalidOperationException("Housing alternate spot gate diverged from SpawnTownNPC.");
}
Console.WriteLine("PASS: housing alternate spot policy preserves recursion guard");

SmallConsecutiveClumpMetrics clumpMetrics = new(0, 0);
clumpMetrics = SmallConsecutiveClumpMetricsPolicy.Record(clumpMetrics, 3, 4, 20);
clumpMetrics = SmallConsecutiveClumpMetricsPolicy.Record(clumpMetrics, 20, 4, 20);
clumpMetrics = SmallConsecutiveClumpMetricsPolicy.Record(clumpMetrics, 2, 20, 20);
if (clumpMetrics.FoundCount != 2 || clumpMetrics.EliminatedCount != 1)
{
  throw new InvalidOperationException("Small consecutive clump metrics diverged from ScanTileColumnAndRemoveClumps.");
}
Console.WriteLine("PASS: small consecutive clump metrics preserve bounded scan accounting");

HousingRoomQualityDecision occupiedRoom = HousingRoomQualityPolicy.Evaluate(-1, true, true, false);
HousingRoomQualityDecision evilRoom = HousingRoomQualityPolicy.Evaluate(-1, false, true, true);
HousingRoomQualityDecision standingRoom = HousingRoomQualityPolicy.Evaluate(-1, false, false, false);
HousingRoomQualityDecision validRoomQuality = HousingRoomQualityPolicy.Evaluate(1, true, true, false);
if (occupiedRoom.IsEligible || occupiedRoom.FailureReason != HousingRoomQualityFailureReason.Occupied ||
    evilRoom.FailureReason != HousingRoomQualityFailureReason.Evil ||
    standingRoom.FailureReason != HousingRoomQualityFailureReason.NoStandingSpace ||
    !validRoomQuality.IsEligible || validRoomQuality.FailureReason != HousingRoomQualityFailureReason.None)
{
  throw new InvalidOperationException("Housing room quality failure precedence diverged from ScoreRoom.");
}
Console.WriteLine("PASS: housing room quality preserves failure precedence");

TileReframeDecision shallowReframe = TileReframePolicy.Evaluate(23);
TileReframeDecision saturatedReframe = TileReframePolicy.Evaluate(24);
TileReframeDecision deepReframe = TileReframePolicy.Evaluate(25);
if (!shallowReframe.ShouldReframeNeighbors || saturatedReframe.ShouldReframeNeighbors ||
    deepReframe.ShouldReframeNeighbors || shallowReframe.MaximumDepth != 25)
{
  throw new InvalidOperationException("Tile reframe recursion budget diverged from framing source.");
}
Console.WriteLine("PASS: tile reframe policy preserves bounded neighbor recursion");

CrimsonHeartPositionSnapshot emptyHeartPositions = new(Array.Empty<(int X, int Y)>());
if (!CrimsonHeartPositionPolicy.TryAppend(emptyHeartPositions, 12, 34, out CrimsonHeartPositionSnapshot firstHeartPositions) ||
    firstHeartPositions.Positions.Count != 1 || firstHeartPositions.Positions[0] != (12, 34) ||
    firstHeartPositions.Positions == emptyHeartPositions.Positions)
{
  throw new InvalidOperationException("Crimson heart position snapshot did not append immutably.");
}

var fullHeartPositions = new List<(int X, int Y)>();
for (int index = 0; index < CrimsonHeartPositionSnapshot.MaximumCount; index++)
{
  fullHeartPositions.Add((index, index));
}

CrimsonHeartPositionSnapshot fullHeartSnapshot = new(fullHeartPositions);
if (CrimsonHeartPositionPolicy.TryAppend(fullHeartSnapshot, 101, 101, out CrimsonHeartPositionSnapshot overflowHeartSnapshot) ||
    overflowHeartSnapshot.Positions.Count != CrimsonHeartPositionSnapshot.MaximumCount)
{
  throw new InvalidOperationException("Crimson heart position capacity diverged from legacy storage.");
}
Console.WriteLine("PASS: crimson heart positions preserve bounded immutable storage");

CatTailDistanceDecision validCatTailDistance = CatTailDistancePolicy.Evaluate(7);
CatTailDistanceDecision shortCatTailDistance = CatTailDistancePolicy.Evaluate(1);
CatTailDistanceDecision longCatTailDistance = CatTailDistancePolicy.Evaluate(9);
if (!validCatTailDistance.IsValidPlacementDistance || validCatTailDistance.ExceedsCleanupDistance ||
    shortCatTailDistance.IsValidPlacementDistance || longCatTailDistance.IsValidPlacementDistance ||
    !longCatTailDistance.ExceedsCleanupDistance || CatTailDistancePolicy.DefaultDistance != 8)
{
  throw new InvalidOperationException("Cat tail distance policy diverged from legacy bounds.");
}
Console.WriteLine("PASS: cat tail distance preserves placement and cleanup bounds");

TreeShakeWorklistSnapshot emptyTreeShakes = new(Array.Empty<TreeShakeWorkItem>());
TreeShakeWorkItem firstTreeShake = new(40, 80);
if (!TreeShakeWorklistPolicy.TryEnqueue(emptyTreeShakes, firstTreeShake, out TreeShakeWorklistSnapshot queuedTreeShakes) ||
    !queuedTreeShakes.Items.Contains(firstTreeShake) ||
    TreeShakeWorklistPolicy.TryEnqueue(queuedTreeShakes, firstTreeShake, out _))
{
  throw new InvalidOperationException("Tree shake worklist did not preserve coordinate deduplication.");
}

var fullTreeShakeItems = new List<TreeShakeWorkItem>();
for (int index = 0; index < TreeShakeWorklistSnapshot.MaximumCount; index++)
{
  fullTreeShakeItems.Add(new TreeShakeWorkItem(index, index + 1));
}

TreeShakeWorklistSnapshot fullTreeShakes = new(fullTreeShakeItems);
if (TreeShakeWorklistPolicy.TryEnqueue(fullTreeShakes, new TreeShakeWorkItem(999, 999), out _) ||
    fullTreeShakes.Items.Count != TreeShakeWorklistSnapshot.MaximumCount)
{
  throw new InvalidOperationException("Tree shake worklist capacity diverged from legacy storage.");
}
Console.WriteLine("PASS: tree shake worklist preserves bounded deduplicated storage");

var treeTopStyles = new List<int>();
for (int areaId = 0; areaId < TreeTopStyleSnapshot.AreaCount; areaId++)
{
  treeTopStyles.Add(areaId + 10);
}

TreeTopStyleSnapshot treeTopSnapshot = new(treeTopStyles);
if (TreeTopStyleQuery.GetStyle(treeTopSnapshot, 0) != 10 ||
    TreeTopStyleQuery.GetStyle(treeTopSnapshot, 12) != 22)
{
  throw new InvalidOperationException("Tree top style snapshot did not preserve area ordering.");
}

bool rejectedTreeTopShape = false;
try
{
  _ = new TreeTopStyleSnapshot(new[] { 1, 2 });
}
catch (ArgumentException)
{
  rejectedTreeTopShape = true;
}

if (!rejectedTreeTopShape)
{
  throw new InvalidOperationException("Tree top style snapshot accepted an invalid area count.");
}
Console.WriteLine("PASS: tree top styles preserve fixed 13-area snapshot shape");

HalloweenPumpkinGenerationDecision halloweenPumpkins =
  HalloweenPumpkinGenerationPolicy.Evaluate(true, false);
HalloweenPumpkinGenerationDecision endlessPumpkins =
  HalloweenPumpkinGenerationPolicy.Evaluate(false, true);
HalloweenPumpkinGenerationDecision ordinarySurface =
  HalloweenPumpkinGenerationPolicy.Evaluate(false, false);
if (!halloweenPumpkins.ShouldGeneratePumpkins || !endlessPumpkins.ShouldGeneratePumpkins ||
    ordinarySurface.ShouldGeneratePumpkins)
{
  throw new InvalidOperationException("Halloween pumpkin generation gate diverged from legacy.");
}
Console.WriteLine("PASS: Halloween pumpkin generation preserves seed gate");

if (TownNpcSpawnCadencePolicy.CalculatePeriod(3) != 60)
{
  throw new InvalidOperationException("Town NPC spawn period diverged from world update rate.");
}

TownNpcSpawnCadenceDecision waitingSpawn = TownNpcSpawnCadencePolicy.Advance(58, 60, false, false);
TownNpcSpawnCadenceDecision readySpawn = TownNpcSpawnCadencePolicy.Advance(59, 60, false, false);
TownNpcSpawnCadenceDecision blockedSpawn = TownNpcSpawnCadencePolicy.Advance(59, 60, true, false);
if (waitingSpawn.NextDelay != 59 || waitingSpawn.ShouldAttemptSpawn ||
    readySpawn.NextDelay != 0 || !readySpawn.ShouldAttemptSpawn ||
    blockedSpawn.NextDelay != 59 || blockedSpawn.ShouldAttemptSpawn || !blockedSpawn.IsBlockedByEvent)
{
  throw new InvalidOperationException("Town NPC spawn cadence diverged from TrySpawningTownNPC.");
}
Console.WriteLine("PASS: town NPC spawn cadence preserves event blocking and period reset");

MeteorShowerProgression initializedMeteorShower = MeteorShowerInitializationPolicy.Initialize(
  new LegacyPassRandomState(1456));
if (initializedMeteorShower.RemainingCount < 2600 || initializedMeteorShower.RemainingCount > 3000)
{
  throw new InvalidOperationException("Meteor shower initialization diverged from source range.");
}

MeteorShowerProgression failedImpact = MeteorShowerAdvancePolicy.Advance(
  initializedMeteorShower,
  reset: false,
  fastForward: false,
  impactCommitted: false);
MeteorShowerProgression committedImpact = MeteorShowerAdvancePolicy.Advance(
  initializedMeteorShower,
  reset: false,
  fastForward: false,
  impactCommitted: true);
MeteorShowerProgression fastForwarded = MeteorShowerAdvancePolicy.Advance(
  initializedMeteorShower,
  reset: false,
  fastForward: true,
  impactCommitted: false);
MeteorShowerProgression resetMeteorShower = MeteorShowerAdvancePolicy.Advance(
  initializedMeteorShower,
  reset: true,
  fastForward: false,
  impactCommitted: false);
if (failedImpact.RemainingCount != initializedMeteorShower.RemainingCount ||
    committedImpact.RemainingCount != initializedMeteorShower.RemainingCount - 1 ||
    fastForwarded.RemainingCount != 0 ||
    resetMeteorShower.RemainingCount != 0)
{
  throw new InvalidOperationException("Meteor shower counter transition diverged from source rules.");
}

WorldGenerationLifecycleSnapshot lifecycle = WorldGenerationLifecycleQuery.FromLegacyFlags(
  generatingWorld: true,
  isGeneratingOrLoadingWorld: true);
if (!lifecycle.IsGenerating || !lifecycle.IsGeneratingOrLoading ||
    WorldGenerationLifecycleQuery.FromLegacyFlags(false, false) != default)
{
  throw new InvalidOperationException("World-generation lifecycle flags lost their explicit state.");
}

try
{
  _ = WorldGenerationLifecycleQuery.FromLegacyFlags(true, false);
  throw new InvalidOperationException("Inconsistent world-generation lifecycle flags were accepted.");
}
catch (ArgumentException)
{
}

Dictionary<ushort, TreeGroundTileDefinition> checkSettingsGroundDefinitions = new()
{
  [2] = new TreeGroundTileDefinition(2, IsStone: true, IsMoss: false, IsGrass: false),
  [23] = new TreeGroundTileDefinition(23, IsStone: false, IsMoss: false, IsGrass: true),
  [633] = new TreeGroundTileDefinition(633, IsStone: false, IsMoss: false, IsGrass: false)
};
Dictionary<ushort, TreeWallDefinition> checkSettingsWallDefinitions = new()
{
  [2] = new TreeWallDefinition(2, AllowsPlantsToGrow: false),
  [700] = new TreeWallDefinition(700, AllowsPlantsToGrow: true)
};
if (!TreeCheckSettingsQuery.IsGroundValid(
      LegacyTreeProfileKind.GemTreeTopaz, 2, checkSettingsGroundDefinitions) ||
    TreeCheckSettingsQuery.IsGroundValid(
      LegacyTreeProfileKind.VanityTreeSakura, 23, checkSettingsGroundDefinitions) ||
    !TreeCheckSettingsQuery.IsGroundValid(
      LegacyTreeProfileKind.TreeAsh, 633, checkSettingsGroundDefinitions) ||
    !TreeCheckSettingsQuery.IsWallValid(
      LegacyTreeProfileKind.GemTreeTopaz, 2, checkSettingsWallDefinitions) ||
    !TreeCheckSettingsQuery.IsWallValid(
      LegacyTreeProfileKind.VanityTreeSakura, 700, checkSettingsWallDefinitions) ||
    TreeCheckSettingsQuery.IsGroundValid(
      LegacyTreeProfileKind.TreeAsh, -1, checkSettingsGroundDefinitions))
{
  throw new InvalidOperationException("Tree check-settings predicates did not preserve source boundaries.");
}

if (WorldTransformationStateQuery.FromActiveCount(0).IsTransforming ||
    !WorldTransformationStateQuery.FromActiveCount(2).IsTransforming ||
    WorldTransformationStateQuery.FromActiveCount(2).ActiveTransformations != 2)
{
  throw new InvalidOperationException("World transformation snapshot did not preserve active-count semantics.");
}

if (OceanLevelQuery.Evaluate(80.0, 150.0) != 155.0 ||
    OceanLevelQuery.Evaluate(new TerrainProfileComponent(80, 150, 270)) != 155.0)
{
  throw new InvalidOperationException("Ocean level query did not preserve the legacy midpoint formula.");
}

try
{
  _ = OceanLevelQuery.Evaluate(double.NaN, 150.0);
  throw new InvalidOperationException("Non-finite ocean level input was accepted.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = WorldTransformationStateQuery.FromActiveCount(-1);
  throw new InvalidOperationException("Negative world transformation count was accepted.");
}
catch (ArgumentOutOfRangeException)
{
}

TileHousingRuleSnapshot housingRules = TileHousingRuleQuery.Create(
  preventInfiniteRopeFraming: true);
if (!housingRules.PreventInfiniteRopeFraming || !TileHousingRuleSnapshot.BubblesAreSolidForHousing ||
    TileHousingRuleQuery.Create(false).PreventInfiniteRopeFraming)
{
  throw new InvalidOperationException("Tile housing rule snapshot did not preserve source constants.");
}

if (ItemSpawnProtectionPolicy.Version4DurationTicks != 18000 ||
    ItemSpawnProtectionPolicy.ValidateDuration(18000) != 18000)
{
  throw new InvalidOperationException("Item spawn protection did not preserve the Version4 duration.");
}

SecretSeedRuleSnapshotSet secretSeedRuleSet = SecretSeedRuleSnapshotQuery.Create(
  new HashSet<string>(StringComparer.Ordinal) { "error-world", "no-surface" });
if (secretSeedRuleSet.Rules.Count != 35 || secretSeedRuleSet.ActiveSecretSeedCount != 2 ||
    !secretSeedRuleSet.Rules.Any(rule => rule.Definition.Variant == "error-world" && rule.Enabled) ||
    secretSeedRuleSet.Rules.Any(rule =>
      rule.Definition.Variant == "rainbow-stuff" && rule.Enabled))
{
  throw new InvalidOperationException("SecretSeed rule snapshot did not preserve ordered enabled state.");
}

TileFrameBudget frameBudget = new(requestedCount: 4, committedCount: 3, maximumCount: 8);
if (frameBudget.RequestedCount != 4 || frameBudget.CommittedCount != 3 || frameBudget.MaximumCount != 8)
{
  throw new InvalidOperationException("Tile frame budget did not preserve diagnostic counts.");
}

try
{
  _ = new TileFrameBudget(requestedCount: 2, committedCount: 3, maximumCount: 4);
  throw new InvalidOperationException("Invalid tile frame budget was accepted.");
}
catch (ArgumentOutOfRangeException)
{
}

try
{
  _ = ItemSpawnProtectionPolicy.ValidateDuration(-1);
  throw new InvalidOperationException("Negative item spawn protection duration was accepted.");
}
catch (ArgumentOutOfRangeException)
{
}

HashSet<string> generationPolicySeeds = new(StringComparer.Ordinal) { "bigger-abandoned-houses" };
if (!SecretSeedGenerationPolicyQuery.ShouldGenerateBiggerAbandonedHouses(
      generationPolicySeeds,
      new LegacyPassRandomState(1456)) ||
    SecretSeedGenerationPolicyQuery.ShouldGenerateRainbowGlowsticks(
      new HashSet<string>(StringComparer.Ordinal),
      isTenthAnniversaryWorld: false) ||
    !SecretSeedGenerationPolicyQuery.ShouldGenerateRainbowGlowsticks(
      new HashSet<string>(StringComparer.Ordinal),
      isTenthAnniversaryWorld: true) ||
    !SecretSeedGenerationPolicyQuery.ShouldGenerateRainbowGlowsticks(
      new HashSet<string>(StringComparer.Ordinal) { "rainbow-stuff" },
      isTenthAnniversaryWorld: false))
{
  throw new InvalidOperationException("SecretSeed generation policies did not preserve source branches.");
}

IReadOnlySet<ushort> ropeDefaults = TileRopeQuery.RegisterDefaults();
if (ropeDefaults.Count != 9 || !ropeDefaults.Contains(213) || !ropeDefaults.Contains(504) ||
    !ropeDefaults.SetEquals(TileRopeQuery.RegisterDefaults()))
{
  throw new InvalidOperationException("Rope Tile defaults were mutable or unstable.");
}

IReadOnlySet<ushort> ordinaryTreeGroundDefaults = OrdinaryTreeGroundQuery.RegisterDefaults();
if (ordinaryTreeGroundDefaults.Count != 12 || !ordinaryTreeGroundDefaults.Contains(2) ||
    !ordinaryTreeGroundDefaults.Contains(662) ||
    !ordinaryTreeGroundDefaults.SetEquals(OrdinaryTreeGroundQuery.RegisterDefaults()))
{
  throw new InvalidOperationException("Ordinary tree ground defaults were mutable or unstable.");
}

IReadOnlySet<ushort> nonSlopingDefaults = TileSlopingQuery.RegisterDefaults();
if (nonSlopingDefaults.Count != 13 || !nonSlopingDefaults.Contains(21) ||
    !nonSlopingDefaults.Contains(597) ||
    !nonSlopingDefaults.SetEquals(TileSlopingQuery.RegisterDefaults()))
{
  throw new InvalidOperationException("Non-sloping Tile defaults were mutable or unstable.");
}

IReadOnlySet<ushort> platformSupportDefaults =
  Tile1x2TopValidationQuery.RegisterPlatformSupportDefaults();
if (platformSupportDefaults.Count != 7 || !platformSupportDefaults.Contains(42) ||
    !platformSupportDefaults.Contains(698) ||
    !platformSupportDefaults.SetEquals(Tile1x2TopValidationQuery.RegisterPlatformSupportDefaults()))
{
  throw new InvalidOperationException("Platform-support Tile defaults were mutable or unstable.");
}

IReadOnlySet<ushort> twoByXTopSupportDefaults = Tile2xXValidationQuery.RegisterTopSupportDefaults();
if (twoByXTopSupportDefaults.Count != 4 || !twoByXTopSupportDefaults.Contains(465) ||
    !twoByXTopSupportDefaults.Contains(592) ||
    !twoByXTopSupportDefaults.SetEquals(Tile2xXValidationQuery.RegisterTopSupportDefaults()))
{
  throw new InvalidOperationException("2xX top-support defaults were mutable or unstable.");
}

IReadOnlySet<ushort> poundingBlockedDefaults = TilePoundingEligibilityQuery.RegisterBlockedDefaults();
IReadOnlySet<ushort> poundingGenerationBlockedDefaults =
  TilePoundingEligibilityQuery.RegisterGenerationBlockedDefaults();
if (poundingBlockedDefaults.Count != 9 || poundingGenerationBlockedDefaults.Count != 2 ||
    !poundingBlockedDefaults.Contains(10) || !poundingBlockedDefaults.Contains(484) ||
    !poundingGenerationBlockedDefaults.Contains(190) ||
    !poundingGenerationBlockedDefaults.Contains(30) ||
    !poundingBlockedDefaults.SetEquals(TilePoundingEligibilityQuery.RegisterBlockedDefaults()) ||
    !poundingGenerationBlockedDefaults.SetEquals(
      TilePoundingEligibilityQuery.RegisterGenerationBlockedDefaults()))
{
  throw new InvalidOperationException("Tile pounding defaults were mutable or unstable.");
}

IReadOnlySet<ushort> twoByOneTableSupportDefaults =
  Tile2x1ValidationQuery.RegisterTableSupportDefaults();
if (twoByOneTableSupportDefaults.Count != 3 || !twoByOneTableSupportDefaults.Contains(29) ||
    !twoByOneTableSupportDefaults.Contains(462) ||
    !twoByOneTableSupportDefaults.SetEquals(Tile2x1ValidationQuery.RegisterTableSupportDefaults()))
{
  throw new InvalidOperationException("2x1 table-support defaults were mutable or unstable.");
}

IReadOnlySet<ushort> threeByThreeBottomSupportDefaults =
  Tile3x3ValidationQuery.RegisterBottomSupportDefaults();
if (threeByThreeBottomSupportDefaults.Count != 27 ||
    !threeByThreeBottomSupportDefaults.Contains(106) ||
    !threeByThreeBottomSupportDefaults.Contains(733) ||
    !threeByThreeBottomSupportDefaults.SetEquals(
      Tile3x3ValidationQuery.RegisterBottomSupportDefaults()))
{
  throw new InvalidOperationException("3x3 bottom-support defaults were mutable or unstable.");
}

IReadOnlySet<ushort> threeByTwoDeferredDefaults =
  Tile3x2ValidationQuery.RegisterDeferredSpecialCaseDefaults();
if (threeByTwoDeferredDefaults.Count != 7 || !threeByTwoDeferredDefaults.Contains(186) ||
    !threeByTwoDeferredDefaults.Contains(695) ||
    !threeByTwoDeferredDefaults.SetEquals(
      Tile3x2ValidationQuery.RegisterDeferredSpecialCaseDefaults()))
{
  throw new InvalidOperationException("3x2 deferred special-case defaults were mutable or unstable.");
}

IReadOnlySet<ushort> treeFrameGroundDefaults =
  TreeFrameValidationQuery.RegisterGroundNormalizationDefaults();
if (treeFrameGroundDefaults.Count != 11 || !treeFrameGroundDefaults.Contains(234) ||
    !treeFrameGroundDefaults.Contains(662) ||
    !treeFrameGroundDefaults.SetEquals(TreeFrameValidationQuery.RegisterGroundNormalizationDefaults()))
{
  throw new InvalidOperationException("Tree-frame ground defaults were mutable or unstable.");
}

IReadOnlySet<ushort> twoByTwoDeferredDefaults =
  Tile2x2ValidationQuery.RegisterDeferredSpecialCaseDefaults();
if (twoByTwoDeferredDefaults.Count != 4 || !twoByTwoDeferredDefaults.Contains(132) ||
    !twoByTwoDeferredDefaults.Contains(652) ||
    !twoByTwoDeferredDefaults.SetEquals(
      Tile2x2ValidationQuery.RegisterDeferredSpecialCaseDefaults()))
{
  throw new InvalidOperationException("2x2 deferred special-case defaults were mutable or unstable.");
}

IReadOnlySet<ushort> vineDefaults = VineFrameQuery.RegisterDefaults();
if (vineDefaults.Count != 8 || !vineDefaults.Contains(52) || !vineDefaults.Contains(638) ||
    !vineDefaults.SetEquals(VineFrameQuery.RegisterDefaults()))
{
  throw new InvalidOperationException("Vine Tile defaults were mutable or unstable.");
}

IReadOnlyDictionary<ushort, int> profilePassStyles =
  TreeLeafPassStyleQuery.RegisterProfilePassStyles();
if (profilePassStyles.Count != 10 || profilePassStyles[583] != 1249 ||
    profilePassStyles[633] != 1278 || profilePassStyles[596] != 1248 ||
    !profilePassStyles.ContainsKey(589))
{
  throw new InvalidOperationException("Tree leaf profile pass styles were unstable.");
}

IReadOnlySet<ushort> cactusGroundDefaults = CactusFrameQuery.RegisterSupportedGroundDefaults();
if (cactusGroundDefaults.Count != 4 || !cactusGroundDefaults.Contains(53) ||
    !cactusGroundDefaults.Contains(234) ||
    !cactusGroundDefaults.SetEquals(CactusFrameQuery.RegisterSupportedGroundDefaults()))
{
  throw new InvalidOperationException("Cactus supported-ground defaults were mutable or unstable.");
}

IReadOnlySet<ushort> pumpkinSupportDefaults =
  Tile2x2StyleValidationQuery.RegisterPumpkinSupportDefaults();
if (pumpkinSupportDefaults.Count != 4 || !pumpkinSupportDefaults.Contains(2) ||
    !pumpkinSupportDefaults.Contains(492) ||
    !pumpkinSupportDefaults.SetEquals(Tile2x2StyleValidationQuery.RegisterPumpkinSupportDefaults()))
{
  throw new InvalidOperationException("Pumpkin support defaults were mutable or unstable.");
}

const int replayWidth = 400;
const int replayHeight = 300;
const int replaySpawnX = 200;
const int replaySurfaceY = 80;
string repositoryRoot = FindRepositoryRoot();
string sourcePath = Path.Combine(
  repositoryRoot,
  "..",
  "Version4物理删除了某些文件",
  "Terraria",
  "WorldGen.cs");
sourcePath = Path.GetFullPath(sourcePath);
if (!File.Exists(sourcePath))
{
  throw new FileNotFoundException("WorldGen fact source was not found.", sourcePath);
}

string? legacyOracleArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--legacy-oracle=", StringComparison.Ordinal));
string? legacyDifferentialArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--legacy-differential=", StringComparison.Ordinal));
string? legacyDifferentialSpawnArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--legacy-differential-spawn=", StringComparison.Ordinal));
string? legacyDifferentialSurfaceYArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--legacy-differential-surface-y=", StringComparison.Ordinal));
string? legacyTerrainProfileArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--legacy-terrain-profile=", StringComparison.Ordinal));
string? dirtWallOffsetsArgument = args.FirstOrDefault(
  argument => argument.StartsWith("--dirt-wall-offsets=", StringComparison.Ordinal));
bool reducedVerification = args.Any(
  argument => StringComparer.Ordinal.Equals(argument, "--reduced"));
if (legacyDifferentialArgument is not null)
{
  string legacyDifferentialPath = legacyDifferentialArgument["--legacy-differential=".Length..];
  if (!Path.IsPathFullyQualified(legacyDifferentialPath))
  {
    throw new ArgumentException(
      "The legacy differential path must be absolute.",
      nameof(legacyDifferentialArgument));
  }

  bool useLegacyMetadataSpawn = legacyDifferentialSpawnArgument is not null &&
    legacyDifferentialSpawnArgument["--legacy-differential-spawn=".Length..] == "metadata";
  if (legacyDifferentialSpawnArgument is not null && !useLegacyMetadataSpawn &&
      legacyDifferentialSpawnArgument["--legacy-differential-spawn=".Length..] != "center")
  {
    throw new ArgumentException(
      "The legacy differential spawn must be either 'center' or 'metadata'.",
      nameof(legacyDifferentialSpawnArgument));
  }

  int? requestedSurfaceY = null;
  bool useLegacyMetadataSurfaceY = legacyDifferentialSurfaceYArgument is not null &&
    legacyDifferentialSurfaceYArgument["--legacy-differential-surface-y=".Length..] == "metadata";
  if (legacyDifferentialSurfaceYArgument is not null && !useLegacyMetadataSurfaceY)
  {
    string surfaceYText =
      legacyDifferentialSurfaceYArgument["--legacy-differential-surface-y=".Length..];
    if (!int.TryParse(surfaceYText, out int parsedSurfaceY) || parsedSurfaceY < 0)
    {
      throw new ArgumentException(
        "The legacy differential surface Y must be 'metadata' or a non-negative integer.",
        nameof(legacyDifferentialSurfaceYArgument));
    }

    requestedSurfaceY = parsedSurfaceY;
  }

  string? dirtWallOffsetsPath = null;
  if (dirtWallOffsetsArgument is not null)
  {
    dirtWallOffsetsPath = dirtWallOffsetsArgument["--dirt-wall-offsets=".Length..];
    if (!Path.IsPathFullyQualified(dirtWallOffsetsPath) ||
        !File.Exists(dirtWallOffsetsPath))
    {
      throw new ArgumentException(
        "The dirt wall offset artifact path must be an existing absolute file.",
        nameof(dirtWallOffsetsArgument));
    }
  }

  string? terrainProfilePath = null;
  if (legacyTerrainProfileArgument is not null)
  {
    terrainProfilePath = legacyTerrainProfileArgument["--legacy-terrain-profile=".Length..];
    if (!Path.IsPathFullyQualified(terrainProfilePath) || !File.Exists(terrainProfilePath))
    {
      throw new ArgumentException(
        "The legacy terrain profile artifact path must be an existing absolute file.",
        nameof(legacyTerrainProfileArgument));
    }
  }

  RunLegacyDifferential(
    legacyDifferentialPath,
    repositoryRoot,
    useLegacyMetadataSpawn,
    useLegacyMetadataSurfaceY,
    requestedSurfaceY,
    dirtWallOffsetsPath,
    terrainProfilePath);
  Environment.Exit(0);
}

if (legacyOracleArgument is not null)
{
  string legacyOraclePath = legacyOracleArgument["--legacy-oracle=".Length..];
  if (!Path.IsPathFullyQualified(legacyOraclePath))
  {
    throw new ArgumentException(
      "The legacy oracle path must be absolute.",
      nameof(legacyOracleArgument));
  }

  using FileStream oracleStream = new(
    legacyOraclePath,
    FileMode.Open,
    FileAccess.Read,
    FileShare.Read);
  LegacyWorldDocument legacyOracle = WldWorldReader.Read(oracleStream);
  LegacyOracleEvidence oracleEvidence = new(
    legacyOraclePath,
    new FileInfo(legacyOraclePath).Length,
    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(legacyOraclePath))),
    legacyOracle.Version,
    legacyOracle.FormatVersion.ToString(),
    legacyOracle.Metadata,
    legacyOracle.Tiles.Count,
    legacyOracle.Tiles.Count(tile => tile.LiquidAmount > 0),
    CreateLegacyOracleFingerprint(legacyOracle));
  string oracleEvidenceDirectory = Path.Combine(repositoryRoot, "docs", "worldgen");
  Directory.CreateDirectory(oracleEvidenceDirectory);
  JsonSerializerOptions oracleOptions = new() { WriteIndented = true };
  File.WriteAllText(
    Path.Combine(oracleEvidenceDirectory, "legacy-worldgen-oracle.json"),
    JsonSerializer.Serialize(oracleEvidence, oracleOptions));
  Console.WriteLine(
    $"PASS: legacy oracle WLD v{legacyOracle.Version} " +
    $"{legacyOracle.Metadata.Width}x{legacyOracle.Metadata.Height} " +
    $"fingerprint {oracleEvidence.Fingerprint}");
  Environment.Exit(0);
}

string source = File.ReadAllText(sourcePath);
byte[] sourceBytes = File.ReadAllBytes(sourcePath);
SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source, path: sourcePath);
CompilationUnitSyntax root = syntaxTree.GetCompilationUnitRoot();
List<MethodInventory> methods = root.DescendantNodes()
  .OfType<MethodDeclarationSyntax>()
  .Select(CreateMethodInventory)
  .OrderBy(method => method.Line)
  .ThenBy(method => method.Name, StringComparer.Ordinal)
  .ToList();
List<FieldInventory> fields = root.DescendantNodes()
  .OfType<FieldDeclarationSyntax>()
  .SelectMany(CreateFieldInventory)
  .OrderBy(field => field.Line)
  .ThenBy(field => field.Name, StringComparer.Ordinal)
  .ToList();
List<ReferenceInventory> references = CreateReferenceInventory(root);
WorldGenerationRequest replayRequest = new(
  new WorldMetadata("worldgen-stage0", new WorldSeed(1456), replayWidth, replayHeight),
  replaySpawnX,
  replaySurfaceY);
WorldGrid firstWorld = new WorldGenerationPipeline().Generate(replayRequest);
WorldGrid secondWorld = new WorldGenerationPipeline().Generate(replayRequest);
WorldGridSnapshot firstSnapshot = firstWorld.CreateSnapshot(replayRequest.Metadata);
WorldGridSnapshot secondSnapshot = secondWorld.CreateSnapshot(replayRequest.Metadata);
string firstFingerprint = CreateSnapshotFingerprint(firstSnapshot);
string secondFingerprint = CreateSnapshotFingerprint(secondSnapshot);
if (!StringComparer.Ordinal.Equals(firstFingerprint, secondFingerprint))
{
  throw new InvalidOperationException("Stage 0 replay was not deterministic.");
}

WorldGenerationTrace tracedGeneration = new WorldGenerationPipeline().GenerateWithTrace(replayRequest);
WorldGenerationStage[] expectedTraceStages =
{
  WorldGenerationStage.Terrain,
  WorldGenerationStage.Cave,
  WorldGenerationStage.Biome,
  WorldGenerationStage.Ore,
  WorldGenerationStage.Structure,
  WorldGenerationStage.Tree,
  WorldGenerationStage.Liquid,
  WorldGenerationStage.Framing,
  WorldGenerationStage.Committed
};
if (tracedGeneration.Stages.Count != expectedTraceStages.Length ||
    !tracedGeneration.Stages.Select(stage => stage.Stage).SequenceEqual(expectedTraceStages) ||
    tracedGeneration.Stages.Any(stage => stage.Snapshot is null) ||
    !tracedGeneration.InitialLifecycle.IsGenerating ||
    !tracedGeneration.InitialLifecycle.IsGeneratingOrLoading ||
    tracedGeneration.Lifecycle.IsGenerating || tracedGeneration.Lifecycle.IsGeneratingOrLoading ||
    tracedGeneration.DistanceDefaults != replayRequest.DistanceDefaults ||
    tracedGeneration.TerrainProfile !=
      new TerrainProfileComponent(replayRequest.SurfaceY, replayRequest.RockLayerY,
        replayRequest.Metadata.Height - 1))
{
  throw new InvalidOperationException(
    "World-generation trace did not expose every deterministic commit boundary.");
}

WorldGenerationStageSnapshot framingStage = tracedGeneration.Stages.Single(
  stage => stage.Stage == WorldGenerationStage.Framing);
if (framingStage.FrameBudget is null ||
    framingStage.FrameBudget.Value.CommittedCount > framingStage.FrameBudget.Value.RequestedCount ||
    tracedGeneration.Stages.Any(
      stage => stage.Stage != WorldGenerationStage.Framing && stage.FrameBudget is not null))
{
  throw new InvalidOperationException("World-generation trace did not preserve framing budget provenance.");
}

WorldGrid expectedTerrainWorld = new(replayWidth, replayHeight, initializeLegacyEmptyFrames: true);
WorldGenerationBootstrap expectedTerrainBootstrap =
  new WorldGenerationStageSystem().Initialize(replayRequest);
WorldGenerationStateComponent expectedTerrainState = expectedTerrainBootstrap.State;
List<TileChangeCommand> expectedTerrainCommands = new();
new TerrainBaseSystem().AppendLegacyCommands(
  expectedTerrainWorld.CreateSnapshot(replayRequest.Metadata),
  replayRequest,
  new TerrainProfileComponent(
    replayRequest.SurfaceY,
    replayRequest.RockLayerY,
    replayRequest.Metadata.Height - 1),
  ref expectedTerrainState,
  expectedTerrainCommands);
if (!new TileChangeCommitSystem().TryCommit(
      expectedTerrainWorld,
      expectedTerrainCommands,
      out TileChangeCommitResult expectedTerrainCommit) ||
    expectedTerrainCommit.AppliedCount != expectedTerrainCommands.Count)
{
  throw new InvalidOperationException("The standalone legacy Terrain stage could not be committed.");
}

WorldGridSnapshot expectedTerrainSnapshot =
  expectedTerrainWorld.CreateSnapshot(replayRequest.Metadata);
WorldGridSnapshot actualTerrainSnapshot = tracedGeneration.Stages.Single(
  stage => stage.Stage == WorldGenerationStage.Terrain).Snapshot;
if (!StringComparer.Ordinal.Equals(
      CreateSnapshotWorldGridFingerprint(actualTerrainSnapshot),
      CreateSnapshotWorldGridFingerprint(expectedTerrainSnapshot)))
{
  throw new InvalidOperationException(
    "The ECS Terrain stage included mutations outside the legacy Terrain pass boundary.");
}

Console.WriteLine("PASS: Terrain stage snapshot contains only legacy Terrain pass mutations");

string stageTraceDirectory = Path.Combine(
  repositoryRoot,
  "Build",
  "diagnostics",
  "server-ecs-convergence",
  "P9-worldgen",
  "current-stage-trace");
Directory.CreateDirectory(stageTraceDirectory);
var stageTraceEvidence = tracedGeneration.Stages.Select(stage => new
{
  stage = stage.Stage.ToString(),
  fingerprint = CreateSnapshotWorldGridFingerprint(stage.Snapshot),
  wallTileCount = CountWallTiles(stage.Snapshot),
  nextSequence = stage.NextSequence,
  initialLifecycle = new
  {
    isGenerating = tracedGeneration.InitialLifecycle.IsGenerating,
    isGeneratingOrLoading = tracedGeneration.InitialLifecycle.IsGeneratingOrLoading
  },
  finalLifecycle = new
  {
    isGenerating = tracedGeneration.Lifecycle.IsGenerating,
    isGeneratingOrLoading = tracedGeneration.Lifecycle.IsGeneratingOrLoading
  },
  terrainProfile = new
  {
    surfaceY = tracedGeneration.TerrainProfile.SurfaceY,
    rockLayerY = tracedGeneration.TerrainProfile.RockLayerY,
    underworldY = tracedGeneration.TerrainProfile.UnderworldY
  },
  oracleParity = "not-compared"
});
File.WriteAllText(
  Path.Combine(stageTraceDirectory, "stage-fingerprints.json"),
  JsonSerializer.Serialize(stageTraceEvidence, new JsonSerializerOptions { WriteIndented = true }));

Console.WriteLine(
  $"PASS stage trace exposes {tracedGeneration.Stages.Count} ECS commit fingerprints " +
  "without claiming legacy oracle parity");

VerifyTorchDefinitions();
VerifyTileFrameImportantRegistry();
VerifyTileEntityDefinitions();

MethodInventory? generateWorldMethod = methods.SingleOrDefault(method =>
  method.Name == "GenerateWorld" && method.Line == 10108);
if (generateWorldMethod?.Status != "Partial" || generateWorldMethod.Mapping is null ||
    generateWorldMethod.Mapping.TargetMembers.Count != 1 ||
    generateWorldMethod.Mapping.ExcludedLegacyResponsibilities.Count != 3)
{
  throw new InvalidOperationException(
    "The legacy GenerateWorld partial mapping did not retain its provenance boundary.");
}

MethodInventory? emptyLiquidMethod = methods.SingleOrDefault(method =>
  method.Name == "EmptyLiquid" && method.Line == 4454);
MethodInventory? placeLiquidMethod = methods.SingleOrDefault(method =>
  method.Name == "PlaceLiquid" && method.Line == 4478);
MethodInventory? liquidChangeTypeMethod = methods.SingleOrDefault(method =>
  method.Name == "GetLiquidChangeType" && method.Line == 4527);
MethodInventory? pointInWorldMethod = methods.SingleOrDefault(method =>
  method.Name == "InWorld" && method.Line == 8944);
MethodInventory? coordinateInWorldMethod = methods.SingleOrDefault(method =>
  method.Name == "InWorld" && method.Line == 8951);
MethodInventory? rectangleInWorldMethod = methods.SingleOrDefault(method =>
  method.Name == "InWorld" && method.Line == 8962);
MethodInventory? setWorldSizeMethod = methods.SingleOrDefault(method =>
  method.Name == "setWorldSize" && method.Line == 6221);
MethodInventory? getWorldSizeMethod = methods.SingleOrDefault(method =>
  method.Name == "GetWorldSize" && method.Line == 6231);
MethodInventory? setWorldSizeIndexMethod = methods.SingleOrDefault(method =>
  method.Name == "SetWorldSize" && method.Line == 6246);
MethodInventory? areAnyTilesInSetNearbyMethod = methods.SingleOrDefault(method =>
  method.Name == "AreAnyTilesInSetNearby" && method.Line == 8153);
MethodInventory? isTileNearbyMethod = methods.SingleOrDefault(method =>
  method.Name == "IsTileNearby" && method.Line == 8183);
MethodInventory? countTilesMethod = methods.SingleOrDefault(method =>
  method.Name == "countTiles" && method.Line == 8799);
MethodInventory? nextCountMethod = methods.SingleOrDefault(method =>
  method.Name == "nextCount" && method.Line == 8814);
MethodInventory? countDirtTilesMethod = methods.SingleOrDefault(method =>
  method.Name == "countDirtTiles" && method.Line == 8894);
MethodInventory? nextDirtCountMethod = methods.SingleOrDefault(method =>
  method.Name == "nextDirtCount" && method.Line == 8904);
MethodInventory? solidTileValueMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile" && method.Line == 58615);
MethodInventory? tileEmptyMethod = methods.SingleOrDefault(method =>
  method.Name == "TileEmpty" && method.Line == 58636);
MethodInventory? solidOrSlopedTileValueMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidOrSlopedTile" && method.Line == 58647);
MethodInventory? tileTypeMethod = methods.SingleOrDefault(method =>
  method.Name == "TileType" && method.Line == 58658);
MethodInventory? solidOrSlopedTileCoordinateMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidOrSlopedTile" && method.Line == 58669);
MethodInventory? solidTileCoordinateMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile" && method.Line == 58770);
MethodInventory? solidTile2ValueMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile2" && method.Line == 58795);
MethodInventory? platformProperTopFrameMethod = methods.SingleOrDefault(method =>
  method.Name == "PlatformProperTopFrame" && method.Line == 58816);
MethodInventory? solidTileAllowBottomSlopeMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTileAllowBottomSlope" && method.Line == 58832);
MethodInventory? solidTile2CoordinateMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile2" && method.Line == 59064);
MethodInventory? solidTileNoPlatformsMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTileNoPlatforms" && method.Line == 58858);
MethodInventory? solidTileAllowTopSlopeMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTileAllowTopSlope" && method.Line == 58884);
MethodInventory? solidTileAllowLeftSlopeMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTileAllowLeftSlope" && method.Line == 58906);
MethodInventory? solidTileAllowRightSlopeMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTileAllowRightSlope" && method.Line == 58928);
MethodInventory? solidTile3CoordinateMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile3" && method.Line == 59038);
MethodInventory? solidTile3ValueMethod = methods.SingleOrDefault(method =>
  method.Name == "SolidTile3" && method.Line == 59049);
MethodInventory? hasAnyWireNearbyMethod = methods.SingleOrDefault(method =>
  method.Name == "HasAnyWireNearby" && method.Line == 60717);
MethodInventory? getRopeEndsMethod = methods.SingleOrDefault(method =>
  method.Name == "GetRopeEnds" && method.Line == 58676);
MethodInventory? isRopeCoordinateMethod = methods.SingleOrDefault(method =>
  method.Name == "IsRope" && method.Line == 58736);
MethodInventory? isRopeConvenienceMethod = methods.SingleOrDefault(method =>
  method.Name == "IsRope" && method.Line == 58727);
MethodInventory? countNearBlocksTypesMethod = methods.SingleOrDefault(method =>
  method.Name == "CountNearBlocksTypes" && method.Line == 58214);
MethodInventory? getWorldUpdateRateMethod = methods.SingleOrDefault(method =>
  method.Name == "GetWorldUpdateRate" && method.Line == 59850);
MethodInventory? topAttachMethod = methods.SingleOrDefault(method =>
  method.Name == "TopEdgeCanBeAttachedTo" && method.Line == 58950);
MethodInventory? rightAttachMethod = methods.SingleOrDefault(method =>
  method.Name == "RightEdgeCanBeAttachedTo" && method.Line == 58972);
MethodInventory? leftAttachMethod = methods.SingleOrDefault(method =>
  method.Name == "LeftEdgeCanBeAttachedTo" && method.Line == 58994);
MethodInventory? bottomAttachMethod = methods.SingleOrDefault(method =>
  method.Name == "BottomEdgeCanBeAttachedTo" && method.Line == 59016);
MethodInventory? isSafeFromRainMethod = methods.SingleOrDefault(method =>
  method.Name == "IsSafeFromRain" && method.Line == 60739);
MethodInventory? errorWorldAdjustmentMethod = methods.SingleOrDefault(method =>
  method.Name == "errorWorldAdjustment" && method.Line == 328);
MethodInventory? randomRectangleMethod = methods.SingleOrDefault(method =>
  method.Name == "RandomRectanglePoint" && method.Line == 22181);
MethodInventory? randomRectangleCoordinatesMethod = methods.SingleOrDefault(method =>
  method.Name == "RandomRectanglePoint" && method.Line == 22188);
MethodInventory? randomWorldPointMethod = methods.SingleOrDefault(method =>
  method.Name == "RandomWorldPoint" && method.Line == 22195);
MethodInventory? randomGemMethod = methods.SingleOrDefault(method =>
  method.Name == "randGem" && method.Line == 8997);
MethodInventory? randomGemTileMethod = methods.SingleOrDefault(method =>
  method.Name == "randGemTile" && method.Line == 9009);
MethodInventory? randomMossMethod = methods.SingleOrDefault(method =>
  method.Name == "randMoss" && method.Line == 9028);
MethodInventory? tryGetTreeProfileMethod = methods.SingleOrDefault(method =>
  method.Name == "TryGetFromTreeId" && method.Line == 3951);
MethodInventory? gemTreeGroundTestMethod = methods.SingleOrDefault(method =>
  method.Name == "GemTreeGroundTest" && method.Line == 24863);
MethodInventory? vanityTreeGroundTestMethod = methods.SingleOrDefault(method =>
  method.Name == "VanityTreeGroundTest" && method.Line == 24878);
MethodInventory? ashTreeGroundTestMethod = methods.SingleOrDefault(method =>
  method.Name == "AshTreeGroundTest" && method.Line == 24893);
MethodInventory? defaultTreeWallTestMethod = methods.SingleOrDefault(method =>
  method.Name == "DefaultTreeWallTest" && method.Line == 24815);
MethodInventory? gemTreeWallTestMethod = methods.SingleOrDefault(method =>
  method.Name == "GemTreeWallTest" && method.Line == 24826);
MethodInventory? tryGrowingTreeByTypeMethod = methods.SingleOrDefault(method =>
  method.Name == "TryGrowingTreeByType" && method.Line == 24908);
MethodInventory? growTreeWithSettingsMethod = methods.SingleOrDefault(method =>
  method.Name == "GrowTreeWithSettings" && method.Line == 24955);
MethodInventory? emptyTileCheckMethod = methods.SingleOrDefault(method =>
  method.Name == "EmptyTileCheck" && method.Line == 25960);
MethodInventory? tileTypeFitForTreeMethod = methods.SingleOrDefault(method =>
  method.Name == "IsTileTypeFitForTree" && method.Line == 24245);
MethodInventory? growTreeMethod = methods.SingleOrDefault(method =>
  method.Name == "GrowTree" && method.Line == 24323);
MethodInventory? treeBranchMethod = methods.SingleOrDefault(method =>
  method.Name == "IsTileATreeBranch" && method.Line == 24269);
MethodInventory? treeRootMethod = methods.SingleOrDefault(method =>
  method.Name == "IsTileATreeRoot" && method.Line == 24296);
MethodInventory? leafyTreeTopMethod = methods.SingleOrDefault(method =>
  method.Name == "IsTileALeafyTreeTop" && method.Line == 24227);
MethodInventory? growUndergroundTreeMethod = methods.SingleOrDefault(method =>
  method.Name == "GrowUndergroundTree" && method.Line == 25416);
MethodInventory? getTreeLeafMethod = methods.SingleOrDefault(method =>
  method.Name == "GetTreeLeaf" && method.Line == 24008);
MethodInventory? treeGrowFxCheckMethod = methods.SingleOrDefault(method =>
  method.Name == "TreeGrowFXCheck" && method.Line == 23974);
MethodInventory? oreHelperMethod = methods.SingleOrDefault(method =>
  method.Name == "OreHelper" && method.Line == 9263);
MethodInventory? orePatchMethod = methods.SingleOrDefault(method =>
  method.Name == "OrePatch" && method.Line == 9624);
MethodInventory? oreRunnerMethod = methods.SingleOrDefault(method =>
  method.Name == "OreRunner" && method.Line == 41739);
MethodInventory? isItATrapMethod = methods.SingleOrDefault(method =>
  method.Name == "IsItATrap" && method.Line == 22015);
MethodInventory? isItATriggerMethod = methods.SingleOrDefault(method =>
  method.Name == "IsItATrigger" && method.Line == 22034);
MethodInventory? dungeonPlatformMethod = methods.SingleOrDefault(method =>
  method.Name == "IsDungeonPlatformOrShelf" && method.Line == 10533);
MethodInventory? atmosphericSurfaceMethod = methods.SingleOrDefault(method =>
  method.Name == "IsSurfaceForAtmospherics" && method.Line == 10033);
MethodInventory? pressurePlateMethod = methods.SingleOrDefault(method =>
  method.Name == "CanGeneratePressurePlateAt" && method.Line == 10072);
MethodInventory? initializeSecretSeedsMethod = methods.SingleOrDefault(method =>
  method.Name == "InitializeSecretSeeds" && method.Line == 556);
MethodInventory? finalizeSecretSeedsMethod = methods.SingleOrDefault(method =>
  method.Name == "FinalizeSecretSeeds" && method.Line == 586);
MethodInventory? setupDungeonGenVarsMethod = methods.SingleOrDefault(method =>
  method.Name == "GenerateWorld_SetupDungeonGenVars" && method.Line == 10096);
MethodInventory? disablePassesForSpecialSeedsMethod = methods.SingleOrDefault(method =>
  method.Name == "DisablePassesForSpecialSeeds" && method.Line == 21674);
MethodInventory? errorWorldRandomBlockMethod = methods.SingleOrDefault(method =>
  method.Name == "DoErrorWorldGetRandomBlock" && method.Line == 980);
MethodInventory? errorWorldShuffleBlocksMethod = methods.SingleOrDefault(method =>
  method.Name == "DoErrorWorldShuffleBlocks" && method.Line == 992);
MethodInventory? errorWorldFinishMethod = methods.SingleOrDefault(method =>
  method.Name == "DoErrorWorldFinish" && method.Line == 1201);
MethodInventory? errorWorldChestItemMethod = methods.SingleOrDefault(method =>
  method.Name == "DoErrorWorldFindChestItem" && method.Line == 1463);
MethodInventory? extraLiquidAddLiquidMethod = methods.SingleOrDefault(method =>
  method.Name == "DoExtraLiquidAddLiquid" && method.Line == 1508);
MethodInventory? extraLiquidFinishMethod = methods.SingleOrDefault(method =>
  method.Name == "DoExtraLiquidFinish" && method.Line == 1744);
MethodInventory? rainsForAYearMethod = methods.SingleOrDefault(method =>
  method.Name == "DoRainsForAYear" && method.Line == 1788);
MethodInventory? randomSpawnMethod = methods.SingleOrDefault(method =>
  method.Name == "DoRandomSpawn" && method.Line == 1798);
MethodInventory? addTeleportersMethod = methods.SingleOrDefault(method =>
  method.Name == "DoAddTeleporters" && method.Line == 1809);
MethodInventory? startInHardmodeMethod = methods.SingleOrDefault(method =>
  method.Name == "DoStartInHardmode" && method.Line == 1997);
MethodInventory? statueStyleItemMethod = methods.SingleOrDefault(method =>
  method.Name == "StatueStyleToItem" && method.Line == 31437);
MethodInventory? nonHammeredPlatformMethod = methods.SingleOrDefault(method =>
  method.Name == "IsBelowANonHammeredPlatform" && method.Line == 31812);
MethodInventory? candleItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Candles" && method.Line == 32845);
MethodInventory? picnicTableItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_PicnicTables" && method.Line == 33374);
MethodInventory? bottleItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Bottles" && method.Line == 34364);
MethodInventory? benchItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Benches" && method.Line == 33296);
MethodInventory? clockItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Clocks" && method.Line == 33214);
MethodInventory? bedItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Beds" && method.Line == 33028);
MethodInventory? candelabraItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Candelabras" && method.Line == 33385);
MethodInventory? bookcaseItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Bookcases" && method.Line == 33567);
MethodInventory? chandelierItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Chandeliers" && method.Line == 33769);
MethodInventory? lanternItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Lanterns" && method.Line == 33968);
MethodInventory? lampItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Lamps" && method.Line == 34184);
MethodInventory? pianoItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Pianos" && method.Line == 34399);
MethodInventory? sinkItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Sinks" && method.Line == 34572);
MethodInventory? tableItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Tables" && method.Line == 35240);
MethodInventory? bathtubItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Bathtubs" && method.Line == 35433);
MethodInventory? workbenchItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Workbenches" && method.Line == 35602);
MethodInventory? chairItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Chair" && method.Line == 35792);
MethodInventory? toiletItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Toilet" && method.Line == 35932);
MethodInventory? platformItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Platforms" && method.Line == 36046);
MethodInventory? musicBoxItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_MusicBoxes" && method.Line == 36259);
MethodInventory? dresserItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Dressers" && method.Line == 42757);
MethodInventory? chestItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_Chests" && method.Line == 34701);
MethodInventory? fakeChestItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetItemDrop_FakeChests" && method.Line == 34990);
MethodInventory? campfireItemDropMethod = methods.SingleOrDefault(method =>
  method.Name == "GetCampfireItemDrop" && method.Line == 42943);
MethodInventory? rainbowPaintMethod = methods.SingleOrDefault(method =>
  method.Name == "GetRainbowPaintIDForPosition" && method.Line == 21860);
MethodInventory? lockedDungeonBiomeChestMethod = methods.SingleOrDefault(method =>
  method.Name == "IsLockedDungeonBiomeChest" && method.Line == 29381);
MethodInventory? pileGenerationAttemptsMethod = methods.SingleOrDefault(method =>
  method.Name == "GetPileGenerationAttempts" && method.Line == 21715);
MethodInventory? plantPlacementMethod = methods.SingleOrDefault(method =>
  method.Name == "PlantCheck_CanPlaceHook" && method.Line == 67332);
MethodInventory? plantCheckMethod = methods.SingleOrDefault(method =>
  method.Name == "PlantCheck" && method.Line == 67360);
MethodInventory? plantTypeConversionMethod = methods.SingleOrDefault(method =>
  method.Name == "PlantCheck_TryGetNewType" && method.Line == 67430);
MethodInventory? plantTypeMatchMethod = methods.SingleOrDefault(method =>
  method.Name == "PlantCheck_IsBadTypeMatch" && method.Line == 67434);
MethodInventory? canPoundTileMethod = methods.SingleOrDefault(method =>
  method.Name == "CanPoundTile" && method.Line == 67449);
MethodInventory? forbidsSlopingMethod = methods.SingleOrDefault(method =>
  method.Name == "ForbidsSloping" && method.Line == 67501);
MethodInventory? slopeTileMethod = methods.SingleOrDefault(method =>
  method.Name == "SlopeTile" && method.Line == 67526);
MethodInventory? poundTileMethod = methods.SingleOrDefault(method =>
  method.Name == "PoundTile" && method.Line == 67565);
MethodInventory? tileMergeFrametestSingleMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttemptFrametest" && method.Line == 67602);
MethodInventory? tileMergeFrametestSetMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttemptFrametest" && method.Line == 67656);
MethodInventory? tileMergeCardinalSingleMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67710);
MethodInventory? tileMergeCardinalSetMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67732);
MethodInventory? tileMergeAllSingleMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67754);
MethodInventory? tileMergeAllSetMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67792);
MethodInventory? tileMergeAllSingleExcludeMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67830);
MethodInventory? tileMergeAllSetExcludeMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttempt" && method.Line == 67868);
MethodInventory? tileMergeWeirdMethod = methods.SingleOrDefault(method =>
  method.Name == "TileMergeAttemptWeird" && method.Line == 67906);
MethodInventory? tileMossColorMethod = methods.SingleOrDefault(method =>
  method.Name == "GetTileMossColor" && method.Line == 67944);
if (emptyLiquidMethod?.Status != "Partial" || emptyLiquidMethod.Mapping is null ||
    !emptyLiquidMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/Systems/LiquidChangeCommitSystem.cs:TryCommit"
    }) ||
    placeLiquidMethod?.Status != "Partial" || placeLiquidMethod.Mapping is null ||
    !placeLiquidMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/LiquidPropagationSession.cs:" +
      "Advance",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/LiquidPropagationSystem.cs:" +
      "TryAppendCommands",
      "src/Terraria.Dome.Simulation/World/Systems/LiquidChangeCommitSystem.cs:TryCommit"
    }) ||
    liquidChangeTypeMethod?.Status != "Partial" || liquidChangeTypeMethod.Mapping is null ||
    !liquidChangeTypeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/Liquid/Definitions/" +
      "LiquidInteractionClassifier.cs:GetKind"
    }) ||
    pointInWorldMethod?.Status != "Partial" || pointInWorldMethod.Mapping is null ||
    coordinateInWorldMethod?.Status != "Partial" || coordinateInWorldMethod.Mapping is null ||
    rectangleInWorldMethod?.Status != "Partial" || rectangleInWorldMethod.Mapping is null ||
    setWorldSizeMethod?.Status != "Partial" || setWorldSizeMethod.Mapping is null ||
    getWorldSizeMethod?.Status != "Partial" || getWorldSizeMethod.Mapping is null ||
    setWorldSizeIndexMethod?.Status != "Partial" || setWorldSizeIndexMethod.Mapping is null ||
    areAnyTilesInSetNearbyMethod?.Status != "Partial" ||
    areAnyTilesInSetNearbyMethod.Mapping is null ||
    !areAnyTilesInSetNearbyMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:" +
      "AreAnyTilesInSetNearby"
    }) ||
    isTileNearbyMethod?.Status != "Partial" || isTileNearbyMethod.Mapping is null ||
    !isTileNearbyMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:IsTileNearby"
    }) ||
    hasAnyWireNearbyMethod?.Status != "Partial" || hasAnyWireNearbyMethod.Mapping is null ||
    !hasAnyWireNearbyMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileWireQuery.cs:HasAnyWireNearby"
    }) ||
    getRopeEndsMethod?.Status != "Partial" || getRopeEndsMethod.Mapping is null ||
    !getRopeEndsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:FindEnds"
    }) ||
    isRopeCoordinateMethod?.Status != "Partial" || isRopeCoordinateMethod.Mapping is null ||
    !isRopeCoordinateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:IsRope"
    }) ||
    isRopeConvenienceMethod?.Status != "Partial" ||
    isRopeConvenienceMethod.Mapping is null ||
    !isRopeConvenienceMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:IsRope"
    }) ||
    countNearBlocksTypesMethod?.Status != "Partial" || countNearBlocksTypesMethod.Mapping is null ||
    !countNearBlocksTypesMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:CountNearbyTileTypes"
    }) ||
    getWorldUpdateRateMethod?.Status != "Partial" || getWorldUpdateRateMethod.Mapping is null ||
    !getWorldUpdateRateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/WorldUpdateRatePolicy.cs:GetRate"
    }) ||
    topAttachMethod?.Status != "Partial" || rightAttachMethod?.Status != "Partial" ||
    leftAttachMethod?.Status != "Partial" || bottomAttachMethod?.Status != "Partial" ||
    topAttachMethod.Mapping is null || rightAttachMethod.Mapping is null ||
    leftAttachMethod.Mapping is null || bottomAttachMethod.Mapping is null ||
    !topAttachMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:CanAttachToTop"
    }) ||
    !rightAttachMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:CanAttachToRight"
    }) ||
    !leftAttachMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:CanAttachToLeft"
    }) ||
    !bottomAttachMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:CanAttachToBottom"
    }) ||
    isSafeFromRainMethod?.Status != "Partial" || isSafeFromRainMethod.Mapping is null ||
    !isSafeFromRainMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileLineTraceQuery.cs:IsSafeFromRainPath"
    }) ||
    errorWorldAdjustmentMethod?.Status != "Partial" || errorWorldAdjustmentMethod.Mapping is null ||
    !errorWorldAdjustmentMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/SecretSeedAdjustmentPolicy.cs:Adjust"
    }) ||
    randomRectangleMethod?.Status != "Partial" || randomRectangleMethod.Mapping is null ||
    randomRectangleCoordinatesMethod?.Status != "Partial" ||
    randomRectangleCoordinatesMethod.Mapping is null ||
    !randomRectangleMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/RandomRectanglePointPolicy.cs:Next"
    }) ||
    !randomRectangleCoordinatesMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/RandomRectanglePointPolicy.cs:Next"
    }) ||
    randomWorldPointMethod?.Status != "Partial" || randomWorldPointMethod.Mapping is null ||
    !randomWorldPointMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/RandomWorldPointPolicy.cs:Next"
    }) ||
    randomGemMethod?.Status != "Partial" || randomGemMethod.Mapping is null ||
    randomGemTileMethod?.Status != "Partial" || randomGemTileMethod.Mapping is null ||
    !randomGemMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/GemTileRandomPolicy.cs:NextGemIndex"
    }) ||
    !randomGemTileMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/GemTileRandomPolicy.cs:NextTile"
    }) ||
    randomMossMethod?.Status != "Partial" || randomMossMethod.Mapping is null ||
    !randomMossMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/MossSelectionPolicy.cs:Next"
    }) ||
    tryGetTreeProfileMethod?.Status != "Partial" || tryGetTreeProfileMethod.Mapping is null ||
    !tryGetTreeProfileMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/Definitions/" +
      "LegacyTreeProfileRegistry.cs:TryGet"
    }) ||
    tryGetTreeProfileMethod.Mapping.ExcludedLegacyResponsibilities.Count != 2 ||
    gemTreeGroundTestMethod?.Status != "Partial" || gemTreeGroundTestMethod.Mapping is null ||
    vanityTreeGroundTestMethod?.Status != "Partial" ||
    vanityTreeGroundTestMethod.Mapping is null ||
    ashTreeGroundTestMethod?.Status != "Partial" || ashTreeGroundTestMethod.Mapping is null ||
    !gemTreeGroundTestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeGroundSuitabilityQuery.cs:IsSuitable"
    }) ||
    !vanityTreeGroundTestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeGroundSuitabilityQuery.cs:IsSuitable"
    }) ||
    !ashTreeGroundTestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeGroundSuitabilityQuery.cs:IsSuitable"
    }) ||
    defaultTreeWallTestMethod?.Status != "Partial" ||
    defaultTreeWallTestMethod.Mapping is null ||
    gemTreeWallTestMethod?.Status != "Partial" || gemTreeWallTestMethod.Mapping is null ||
    !defaultTreeWallTestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeWallSuitabilityQuery.cs:IsSuitable"
    }) ||
    !gemTreeWallTestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeWallSuitabilityQuery.cs:IsSuitable"
    }) ||
    tryGrowingTreeByTypeMethod?.Status != "Partial" ||
    tryGrowingTreeByTypeMethod.Mapping is null ||
    !tryGrowingTreeByTypeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeGrowthDispatchQuery.cs:TryGet"
    }) ||
    growTreeWithSettingsMethod?.Status != "Partial" ||
    growTreeWithSettingsMethod.Mapping is null ||
    !growTreeWithSettingsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeProfileGrowthEligibilityQuery.cs:Evaluate",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "TreeProfileTrunkCommandSystem.cs:TryAppendCommands"
    }) ||
    emptyTileCheckMethod?.Status != "Partial" || emptyTileCheckMethod.Mapping is null ||
    !emptyTileCheckMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeCanopyClearanceQuery.cs:IsClear"
    }) ||
    tileTypeFitForTreeMethod?.Status != "Partial" ||
    tileTypeFitForTreeMethod.Mapping is null ||
    !tileTypeFitForTreeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "OrdinaryTreeGroundQuery.cs:IsSuitable"
    }) ||
    growTreeMethod?.Status != "Partial" || growTreeMethod.Mapping is null ||
    !growTreeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "OrdinaryTreeGrowthEligibilityQuery.cs:Evaluate",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "OrdinaryTreeHeightPolicy.cs:Next",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrdinaryTreeTrunkCommandSystem.cs:TryAppendCommands",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrdinaryTreePlacementSystem.cs:TryPrepare",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrdinaryTreePlacementSystem.cs:TryPrepareWithHeightSelection",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrdinaryTreePlacementSystem.cs:TryAppendCommands"
    }) ||
    treeBranchMethod?.Status != "Partial" || treeBranchMethod.Mapping is null ||
    !treeBranchMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeTrunkFrameQuery.cs:TryGetBranchOffset"
    }) ||
    treeRootMethod?.Status != "Partial" || treeRootMethod.Mapping is null ||
    !treeRootMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeTrunkFrameQuery.cs:TryGetRootOffset"
    }) ||
    leafyTreeTopMethod?.Status != "Partial" || leafyTreeTopMethod.Mapping is null ||
    !leafyTreeTopMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeLeafFrameQuery.cs:IsLeafyTreeTop"
    }) ||
    growUndergroundTreeMethod?.Status != "Partial" ||
    growUndergroundTreeMethod.Mapping is null ||
    !growUndergroundTreeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "UndergroundTreeGrowthEligibilityQuery.cs:Evaluate",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "UndergroundTreeHeightPolicy.cs:Next",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "UndergroundTreeTrunkCommandSystem.cs:TryAppendCommands"
    }) ||
    getTreeLeafMethod?.Status != "Partial" || getTreeLeafMethod.Mapping is null ||
    !getTreeLeafMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeLeafPassStyleQuery.cs:Evaluate"
    }) ||
    treeGrowFxCheckMethod?.Status != "Partial" || treeGrowFxCheckMethod.Mapping is null ||
    !treeGrowFxCheckMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "TreeLeafScanQuery.cs:Scan"
    }) ||
    oreHelperMethod?.Status != "Partial" || oreHelperMethod.Mapping is null ||
    !oreHelperMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyOreHelper.cs:AppendCommands"
    }) ||
    orePatchMethod?.Status != "Partial" || orePatchMethod.Mapping is null ||
    !orePatchMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "OrePatchEligibilityQuery.cs:Evaluate",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrePatchPlacementSystem.cs:TryPrepare",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrePatchPlacementSystem.cs:TryAppendCommands",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
      "OrePatchPlacementSystem.cs:TryAppendLegacyTrailAndBlobCommands",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyOrePatchTypePolicy.cs:SelectTileType",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyOreTierSelectionPolicy.cs:Select",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyOrePatchTrail.cs:TryAppendCommands",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyOrePatchBlob.cs:TryAppendCommands"
    }) ||
    oreRunnerMethod?.Status != "Partial" || oreRunnerMethod.Mapping is null ||
    !oreRunnerMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyOreRunner.cs:AppendCommands",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "MossTileTypeRegistry.cs:TileTypes"
    }) ||
    isItATrapMethod?.Status != "Partial" || isItATrapMethod.Mapping is null ||
    !isItATrapMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileWiringQuery.cs:IsItATrap"
    }) ||
    isItATriggerMethod?.Status != "Partial" || isItATriggerMethod.Mapping is null ||
    !isItATriggerMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileWiringQuery.cs:IsItATrigger"
    }) ||
    dungeonPlatformMethod?.Status != "Partial" || dungeonPlatformMethod.Mapping is null ||
    !dungeonPlatformMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/DungeonPlatformQuery.cs:IsPlatformOrShelf"
    }) ||
    atmosphericSurfaceMethod?.Status != "Partial" || atmosphericSurfaceMethod.Mapping is null ||
    !atmosphericSurfaceMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/AtmosphericSurfaceQuery.cs:" +
      "IsSurfaceForAtmospherics"
    }) ||
    pressurePlateMethod?.Status != "Partial" || pressurePlateMethod.Mapping is null ||
    !pressurePlateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PressurePlatePlacementQuery.cs:" +
      "CanGenerateAt"
    }) ||
    initializeSecretSeedsMethod?.Status != "Partial" ||
    initializeSecretSeedsMethod.Mapping is null ||
    !initializeSecretSeedsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "SecretSeedRuntimeProjection.cs:Create"
    }) ||
    finalizeSecretSeedsMethod?.Status != "Partial" ||
    finalizeSecretSeedsMethod.Mapping is null ||
    !finalizeSecretSeedsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacySecretSeedFinalizePolicy.cs:CreateActions"
    }) ||
    setupDungeonGenVarsMethod?.Status != "Partial" ||
    setupDungeonGenVarsMethod.Mapping is null ||
    !setupDungeonGenVarsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/DungeonGenerationState.cs:SetUp"
    }) ||
    disablePassesForSpecialSeedsMethod?.Status != "Partial" ||
    disablePassesForSpecialSeedsMethod.Mapping is null ||
    !disablePassesForSpecialSeedsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyDualDungeonPassPolicy.cs:ShouldDisable"
    }) ||
    errorWorldRandomBlockMethod?.Status != "Partial" ||
    errorWorldRandomBlockMethod.Mapping is null ||
    !errorWorldRandomBlockMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldRandomBlock.cs:TrySelect"
    }) ||
    errorWorldShuffleBlocksMethod?.Status != "Partial" ||
    errorWorldShuffleBlocksMethod.Mapping is null ||
    !errorWorldShuffleBlocksMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldTileSwap.cs:AppendCommands",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldTileRectangleSwap.cs:AppendCommands",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldCandidateQuery.cs:IsSwapEligible",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldCandidateSelector.cs:TrySelect",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldSpawnExclusionPolicy.cs:IsSingleTileExcluded",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldSpawnExclusionPolicy.cs:IsRectangleExcluded",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldSwapOperation.cs:TryAppendSingleTileSwap",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldSwapOperation.cs:TryAppendRectangleSwap",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldAxisTrail.cs:AppendCommands",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldAxisTrailPolicy.cs:Create",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldPassCountPolicy.cs:Create",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldRandomBlockRewrite.cs:TryAppendCommand",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldShufflePass.cs:AppendCommands"
    }) ||
    errorWorldFinishMethod?.Status != "Partial" || errorWorldFinishMethod.Mapping is null ||
    !errorWorldFinishMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldFinishCleanup.cs:AppendCommands"
    }) ||
    errorWorldChestItemMethod?.Status != "Partial" || errorWorldChestItemMethod.Mapping is null ||
    !errorWorldChestItemMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyErrorWorldChestItemPolicy.cs:SelectItemType"
    }) ||
    extraLiquidAddLiquidMethod?.Status != "Partial" || extraLiquidAddLiquidMethod.Mapping is null ||
    !extraLiquidAddLiquidMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyExtraLiquidAddLiquid.cs:AppendCommands"
    }) ||
    extraLiquidFinishMethod?.Status != "Partial" || extraLiquidFinishMethod.Mapping is null ||
    !extraLiquidFinishMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyExtraLiquidFinish.cs:AppendCommands"
    }) ||
    rainsForAYearMethod?.Status != "Partial" || rainsForAYearMethod.Mapping is null ||
    !rainsForAYearMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/LegacyRainsForAYearPolicy.cs:Apply"
    }) ||
    randomSpawnMethod?.Status != "Partial" || randomSpawnMethod.Mapping is null ||
    !randomSpawnMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyRandomSpawnPolicy.cs:Create"
    }) ||
    addTeleportersMethod?.Status != "Partial" || addTeleportersMethod.Mapping is null ||
    !addTeleportersMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyTeleporterPlacementQuery.cs:CanPlace",
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyTeleporterClearArea.cs:TryAppendCommands"
    }) ||
    startInHardmodeMethod?.Status != "Partial" || startInHardmodeMethod.Mapping is null ||
    !startInHardmodeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
      "LegacyStartInHardmodePolicy.cs:Apply"
    }) ||
    statueStyleItemMethod?.Status != "Partial" || statueStyleItemMethod.Mapping is null ||
    !statueStyleItemMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/StatueStyleItemQuery.cs:ToItem"
    }) ||
    nonHammeredPlatformMethod?.Status != "Partial" ||
    nonHammeredPlatformMethod.Mapping is null ||
    !nonHammeredPlatformMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlatformSupportQuery.cs:" +
      "IsBelowANonHammeredPlatform"
    }) ||
    candleItemDropMethod?.Status != "Partial" || candleItemDropMethod.Mapping is null ||
    !candleItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/CandleItemDropQuery.cs:ToItem"
    }) ||
    picnicTableItemDropMethod?.Status != "Partial" ||
    picnicTableItemDropMethod.Mapping is null ||
    !picnicTableItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PicnicTableItemDropQuery.cs:ToItem"
    }) ||
    bottleItemDropMethod?.Status != "Partial" || bottleItemDropMethod.Mapping is null ||
    !bottleItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/BottleItemDropQuery.cs:ToItem"
    }) ||
    benchItemDropMethod?.Status != "Partial" || benchItemDropMethod.Mapping is null ||
    !benchItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/BenchItemDropQuery.cs:ToItem"
    }) ||
    clockItemDropMethod?.Status != "Partial" || clockItemDropMethod.Mapping is null ||
    !clockItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/ClockItemDropQuery.cs:ToItem"
    }) ||
    bedItemDropMethod?.Status != "Partial" || bedItemDropMethod.Mapping is null ||
    !bedItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/BedItemDropQuery.cs:ToItem"
    }) ||
    candelabraItemDropMethod?.Status != "Partial" ||
    candelabraItemDropMethod.Mapping is null ||
    !candelabraItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/CandelabraItemDropQuery.cs:ToItem"
    }) ||
    bookcaseItemDropMethod?.Status != "Partial" || bookcaseItemDropMethod.Mapping is null ||
    !bookcaseItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/BookcaseItemDropQuery.cs:ToItem"
    }) ||
    chandelierItemDropMethod?.Status != "Partial" ||
    chandelierItemDropMethod.Mapping is null ||
    !chandelierItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/ChandelierItemDropQuery.cs:ToItem"
    }) ||
    lanternItemDropMethod?.Status != "Partial" || lanternItemDropMethod.Mapping is null ||
    !lanternItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/LanternItemDropQuery.cs:ToItem"
    }) ||
    lampItemDropMethod?.Status != "Partial" || lampItemDropMethod.Mapping is null ||
    !lampItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/LampItemDropQuery.cs:ToItem"
    }) ||
    pianoItemDropMethod?.Status != "Partial" || pianoItemDropMethod.Mapping is null ||
    !pianoItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PianoItemDropQuery.cs:ToItem"
    }) ||
    sinkItemDropMethod?.Status != "Partial" || sinkItemDropMethod.Mapping is null ||
    !sinkItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/SinkItemDropQuery.cs:ToItem"
    }) ||
    tableItemDropMethod?.Status != "Partial" || tableItemDropMethod.Mapping is null ||
    !tableItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TableItemDropQuery.cs:ToItem"
    }) ||
    bathtubItemDropMethod?.Status != "Partial" || bathtubItemDropMethod.Mapping is null ||
    !bathtubItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/BathtubItemDropQuery.cs:ToItem"
    }) ||
    workbenchItemDropMethod?.Status != "Partial" || workbenchItemDropMethod.Mapping is null ||
    !workbenchItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/WorkbenchItemDropQuery.cs:ToItem"
    }) ||
    chairItemDropMethod?.Status != "Partial" || chairItemDropMethod.Mapping is null ||
    !chairItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/ChairItemDropQuery.cs:ToItem"
    }) ||
    toiletItemDropMethod?.Status != "Partial" || toiletItemDropMethod.Mapping is null ||
    !toiletItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/ToiletItemDropQuery.cs:ToItem"
    }) ||
    platformItemDropMethod?.Status != "Partial" || platformItemDropMethod.Mapping is null ||
    !platformItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlatformItemDropQuery.cs:ToItem"
    }) ||
    musicBoxItemDropMethod?.Status != "Partial" || musicBoxItemDropMethod.Mapping is null ||
    !musicBoxItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/MusicBoxItemDropQuery.cs:ToItem"
    }) ||
    dresserItemDropMethod?.Status != "Partial" || dresserItemDropMethod.Mapping is null ||
    !dresserItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/DresserItemDropQuery.cs:ToItem"
    }) ||
    chestItemDropMethod?.Status != "Partial" || chestItemDropMethod.Mapping is null ||
    !chestItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/ChestItemDropQuery.cs:ToItem"
    }) ||
    fakeChestItemDropMethod?.Status != "Partial" || fakeChestItemDropMethod.Mapping is null ||
    !fakeChestItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/FakeChestItemDropQuery.cs:ToItem"
    }) ||
    campfireItemDropMethod?.Status != "Partial" || campfireItemDropMethod.Mapping is null ||
    !campfireItemDropMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/CampfireItemDropQuery.cs:ToItem"
    }) ||
    rainbowPaintMethod?.Status != "Partial" || rainbowPaintMethod.Mapping is null ||
    !rainbowPaintMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/RainbowPaintQuery.cs:ToPaintId"
    }) ||
    lockedDungeonBiomeChestMethod?.Status != "Partial" ||
    lockedDungeonBiomeChestMethod.Mapping is null ||
    !lockedDungeonBiomeChestMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/DungeonChestQuery.cs:" +
      "IsLockedBiomeChest"
    }) ||
    pileGenerationAttemptsMethod?.Status != "Partial" ||
    pileGenerationAttemptsMethod.Mapping is null ||
    !pileGenerationAttemptsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PileGenerationAttemptPolicy.cs:" +
      "GetAttempts"
    }) ||
    plantPlacementMethod?.Status != "Partial" || plantPlacementMethod.Mapping is null ||
    !plantPlacementMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlantPlacementQuery.cs:CanPlace"
    }) ||
    plantCheckMethod?.Status != "Partial" || plantCheckMethod.Mapping is null ||
    !plantCheckMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlantCheckQuery.cs:Evaluate",
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/PlantCheckCommandSystem.cs:" +
      "TryCreateCommand",
      "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
    }) ||
    plantTypeConversionMethod?.Status != "Partial" ||
    plantTypeConversionMethod.Mapping is null ||
    !plantTypeConversionMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlantTypeConversionQuery.cs:Evaluate"
    }) ||
    plantTypeMatchMethod?.Status != "Partial" || plantTypeMatchMethod.Mapping is null ||
    !plantTypeMatchMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/PlantTypeConversionQuery.cs:" +
      "IsBadTypeMatch"
    }) ||
    canPoundTileMethod?.Status != "Partial" || canPoundTileMethod.Mapping is null ||
    !canPoundTileMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TilePoundingEligibilityQuery.cs:" +
      "CanPound"
    }) ||
    forbidsSlopingMethod?.Status != "Partial" || forbidsSlopingMethod.Mapping is null ||
    !forbidsSlopingMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileSlopingQuery.cs:ForbidsSloping"
    }) ||
    slopeTileMethod?.Status != "Partial" || slopeTileMethod.Mapping is null ||
    !slopeTileMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/TilePoundingCommandSystem.cs:" +
      "TryCreateSlopeCommand",
      "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
    }) ||
    poundTileMethod?.Status != "Partial" || poundTileMethod.Mapping is null ||
    !poundTileMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/Systems/TilePoundingCommandSystem.cs:" +
      "TryCreatePoundCommand",
      "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
    }) ||
    tileMergeFrametestSingleMethod?.Status != "Partial" ||
    tileMergeFrametestSingleMethod.Mapping is null ||
    !tileMergeFrametestSingleMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ApplyFrametest"
    }) ||
    tileMergeFrametestSetMethod?.Status != "Partial" ||
    tileMergeFrametestSetMethod.Mapping is null ||
    !tileMergeFrametestSetMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ApplyFrametest"
    }) ||
    tileMergeCardinalSingleMethod?.Status != "Partial" ||
    tileMergeCardinalSingleMethod.Mapping is null ||
    !tileMergeCardinalSingleMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceCardinal"
    }) ||
    tileMergeCardinalSetMethod?.Status != "Partial" ||
    tileMergeCardinalSetMethod.Mapping is null ||
    !tileMergeCardinalSetMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceCardinal"
    }) ||
    tileMergeAllSingleMethod?.Status != "Partial" || tileMergeAllSingleMethod.Mapping is null ||
    !tileMergeAllSingleMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAll"
    }) ||
    tileMergeAllSetMethod?.Status != "Partial" || tileMergeAllSetMethod.Mapping is null ||
    !tileMergeAllSetMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAll"
    }) ||
    tileMergeAllSingleExcludeMethod?.Status != "Partial" ||
    tileMergeAllSingleExcludeMethod.Mapping is null ||
    !tileMergeAllSingleExcludeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAllExcept"
    }) ||
    tileMergeAllSetExcludeMethod?.Status != "Partial" ||
    tileMergeAllSetExcludeMethod.Mapping is null ||
    !tileMergeAllSetExcludeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAllExcept"
    }) ||
    tileMergeWeirdMethod?.Status != "Partial" || tileMergeWeirdMethod.Mapping is null ||
    !tileMergeWeirdMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceDifferentExcept"
    }) ||
    tileMossColorMethod?.Status != "Partial" || tileMossColorMethod.Mapping is null ||
    !tileMossColorMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/WorldGeneration/MossColorQuery.cs:GetColor"
    }) ||
    countTilesMethod?.Status != "Partial" || countTilesMethod.Mapping is null ||
    !countTilesMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileRegionProbe.cs:CountOpenTiles"
    }) ||
    nextCountMethod?.Status != "Partial" || nextCountMethod.Mapping is null ||
    !nextCountMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileRegionProbe.cs:CountOpenTiles"
    }) ||
    countDirtTilesMethod?.Status != "Partial" || countDirtTilesMethod.Mapping is null ||
    !countDirtTilesMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileDirtRegionProbe.cs:CountTiles"
    }) ||
    nextDirtCountMethod?.Status != "Partial" || nextDirtCountMethod.Mapping is null ||
    !nextDirtCountMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileDirtRegionProbe.cs:CountTiles"
    }) ||
    solidTileValueMethod?.Status != "Partial" || solidTileValueMethod.Mapping is null ||
    tileEmptyMethod?.Status != "Partial" || tileEmptyMethod.Mapping is null ||
    solidOrSlopedTileValueMethod?.Status != "Partial" ||
    solidOrSlopedTileValueMethod.Mapping is null ||
    tileTypeMethod?.Status != "Partial" || tileTypeMethod.Mapping is null ||
    solidOrSlopedTileCoordinateMethod?.Status != "Partial" ||
    solidOrSlopedTileCoordinateMethod.Mapping is null ||
    solidTileCoordinateMethod?.Status != "Partial" || solidTileCoordinateMethod.Mapping is null ||
    !solidTileValueMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolid"
    }) ||
    !tileEmptyMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsEmpty"
    }) ||
    !solidOrSlopedTileValueMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidOrSloped"
    }) ||
    !tileTypeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:GetActiveTileType"
    }) ||
    !solidOrSlopedTileCoordinateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidOrSloped"
    }) ||
    !solidTileCoordinateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolid"
    }) ||
    solidTile2ValueMethod?.Status != "Partial" || solidTile2ValueMethod.Mapping is null ||
    platformProperTopFrameMethod?.Status != "Partial" ||
    platformProperTopFrameMethod.Mapping is null ||
    solidTileAllowBottomSlopeMethod?.Status != "Partial" ||
    solidTileAllowBottomSlopeMethod.Mapping is null ||
    solidTile2CoordinateMethod?.Status != "Partial" ||
    solidTile2CoordinateMethod.Mapping is null ||
    !solidTile2ValueMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatformTop"
    }) ||
    !platformProperTopFrameMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsPlatformProperTopFrame"
    }) ||
    !solidTileAllowBottomSlopeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingBottomSlope"
    }) ||
    !solidTile2CoordinateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatformTop"
    }) ||
    solidTileNoPlatformsMethod?.Status != "Partial" ||
    solidTileNoPlatformsMethod.Mapping is null ||
    solidTileAllowTopSlopeMethod?.Status != "Partial" ||
    solidTileAllowTopSlopeMethod.Mapping is null ||
    solidTileAllowLeftSlopeMethod?.Status != "Partial" ||
    solidTileAllowLeftSlopeMethod.Mapping is null ||
    solidTileAllowRightSlopeMethod?.Status != "Partial" ||
    solidTileAllowRightSlopeMethod.Mapping is null ||
    !solidTileNoPlatformsMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatforms"
    }) ||
    !solidTileAllowTopSlopeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingTopSlope"
    }) ||
    !solidTileAllowLeftSlopeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingLeftSlope"
    }) ||
    !solidTileAllowRightSlopeMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingRightSlope"
    }) ||
    solidTile3CoordinateMethod?.Status != "Partial" ||
    solidTile3CoordinateMethod.Mapping is null ||
    solidTile3ValueMethod?.Status != "Partial" || solidTile3ValueMethod.Mapping is null ||
    !solidTile3CoordinateMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:" +
      "IsSolidWithLegacyTile3Semantics"
    }) ||
    !solidTile3ValueMethod.Mapping.TargetMembers.SequenceEqual(new[]
    {
      "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:" +
      "IsSolidWithLegacyTile3Semantics"
    }))
{
  throw new InvalidOperationException(
    "Bounded legacy method mappings did not retain their ECS targets.");
}

WorldBoundsComponent boundsContract = new(replayWidth, replayHeight);
if (!boundsContract.Contains(0, 0) ||
    boundsContract.Contains(-1, 0) ||
    boundsContract.Contains(0, 0, fluff: 1) ||
    !boundsContract.Contains(1, 1, fluff: 1) ||
    boundsContract.Contains(replayWidth - 1, replayHeight - 1, fluff: 1) ||
    !boundsContract.ContainsRectangle(10, 10, width: 20, height: 20, fluff: 1) ||
    boundsContract.ContainsRectangle(10, 10, width: 390, height: 290, fluff: 1))
{
  throw new InvalidOperationException(
    "World bounds queries did not preserve coordinate, fluff, and rectangle semantics.");
}

WorldGrid neighborhoodWorld = new(replayWidth, replayHeight);
WorldMetadata neighborhoodMetadata = new(
  "worldgen-neighborhood",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
neighborhoodWorld.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 5));
neighborhoodWorld.TrySetTile(12, 10, new WorldTile(IsActive: true, Type: 235));
neighborhoodWorld.TrySetTile(10, 11, new WorldTile(IsActive: false, Type: 5));
WorldGridSnapshot neighborhoodSnapshot = neighborhoodWorld.CreateSnapshot(neighborhoodMetadata);
bool[] neighborhoodTileSet = new bool[6];
neighborhoodTileSet[5] = true;
if (!TileNeighborhoodQuery.AreAnyTilesInSetNearby(
      neighborhoodSnapshot,
      10,
      10,
      neighborhoodTileSet,
      distance: 1) ||
    TileNeighborhoodQuery.AreAnyTilesInSetNearby(
      neighborhoodSnapshot,
      10,
      10,
      neighborhoodTileSet,
      distance: 0) ||
    TileNeighborhoodQuery.IsTileNearby(neighborhoodSnapshot, 10, 10, 235, distance: 2))
{
  throw new InvalidOperationException(
    "Tile neighborhood set and special-stride queries did not preserve snapshot semantics.");
}

neighborhoodWorld.TrySetTile(11, 10, new WorldTile(IsActive: false, Type: 5));
neighborhoodWorld.TrySetTile(12, 10, new WorldTile(IsActive: false, Type: 235));
neighborhoodWorld.TrySetTile(11, 11, new WorldTile(IsActive: true, Type: 5));
WorldGridSnapshot boundaryNeighborhoodSnapshot =
  neighborhoodWorld.CreateSnapshot(neighborhoodMetadata);
if (!TileNeighborhoodQuery.IsTileNearby(
      boundaryNeighborhoodSnapshot,
      10,
      10,
      5,
      distance: 1) ||
    TileNeighborhoodQuery.IsTileNearby(
      boundaryNeighborhoodSnapshot,
      10,
      10,
      235,
      distance: 2))
{
  throw new InvalidOperationException(
    "Tile neighborhood queries did not require active tiles or preserve the tile-235 stride.");
}

neighborhoodWorld.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 235));
WorldGridSnapshot strideNeighborhoodSnapshot = neighborhoodWorld.CreateSnapshot(neighborhoodMetadata);
if (!TileNeighborhoodQuery.IsTileNearby(
      strideNeighborhoodSnapshot,
      10,
      10,
      235,
      distance: 2))
{
  throw new InvalidOperationException(
    "Tile neighborhood queries did not find a tile-235 match on a stride column.");
}

if (!TileNeighborhoodQuery.AreAnyTilesInSetNearby(
      neighborhoodSnapshot,
      0,
      0,
      neighborhoodTileSet,
      distance: 11) ||
    TileNeighborhoodQuery.AreAnyTilesInSetNearby(
      neighborhoodSnapshot,
      10,
      10,
      new[] { true },
      distance: 1))
{
  throw new InvalidOperationException(
    "Tile neighborhood queries did not handle world edges and short tile sets deterministically.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  TileNeighborhoodQuery.AreAnyTilesInSetNearby(
    neighborhoodSnapshot,
    10,
    10,
    neighborhoodTileSet,
    distance: -1));
AssertThrows<ArgumentOutOfRangeException>(() =>
  TileNeighborhoodQuery.IsTileNearby(neighborhoodSnapshot, 10, 10, -1, distance: 1));
Console.WriteLine("PASS: tile neighborhood queries preserve snapshot and legacy stride semantics");

WorldGrid regionProbeWorld = new(replayWidth, replayHeight);
WorldMetadata regionProbeMetadata = new(
  "worldgen-region-probe",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
_ = regionProbeWorld.TrySetTile(9, 10, new WorldTile(IsActive: true, Type: 70));
_ = regionProbeWorld.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 1));
_ = regionProbeWorld.TrySetTile(10, 9, new WorldTile(IsActive: true, Type: 147));
_ = regionProbeWorld.TrySetTile(10, 11, new WorldTile(IsActive: true, Type: 53));
WorldGridSnapshot regionProbeSnapshot = regionProbeWorld.CreateSnapshot(regionProbeMetadata);
TileRegionProbeResult categorizedRegion = TileRegionProbe.CountOpenTiles(
  regionProbeSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(maximumTiles: 20));
if (categorizedRegion.TileCount != 1 || categorizedRegion.ReachedLimit ||
    categorizedRegion.ShroomTileCount != 1 || categorizedRegion.RockTileCount != 1 ||
    categorizedRegion.IceTileCount != 1 || categorizedRegion.SandTileCount != 1 ||
    categorizedRegion.LavaTileCount != 0)
{
  throw new InvalidOperationException(
    "Tile region probing did not preserve legacy tile categories and solid boundaries.");
}

_ = regionProbeWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: false, Type: 0, LiquidAmount: 1, LiquidType: 1));
WorldGridSnapshot lavaRegionSnapshot = regionProbeWorld.CreateSnapshot(regionProbeMetadata);
TileRegionProbeResult blockedLavaRegion = TileRegionProbe.CountOpenTiles(
  lavaRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(maximumTiles: 20));
TileRegionProbeResult permittedLavaRegion = TileRegionProbe.CountOpenTiles(
  lavaRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(lavaOk: true, maximumTiles: 20));
if (!blockedLavaRegion.ReachedLimit || blockedLavaRegion.LavaTileCount != 1 ||
    permittedLavaRegion.ReachedLimit || permittedLavaRegion.TileCount != 1 ||
    permittedLavaRegion.LavaTileCount != 1)
{
  throw new InvalidOperationException(
    "Tile region probing did not preserve lava acceptance and termination semantics.");
}

_ = regionProbeWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: false, Type: 0, LiquidAmount: 1, LiquidType: 3));
WorldGridSnapshot shimmerRegionSnapshot = regionProbeWorld.CreateSnapshot(regionProbeMetadata);
TileRegionProbeResult shimmerRegion = TileRegionProbe.CountOpenTiles(
  shimmerRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(jungle: true, lavaOk: true, maximumTiles: 20));
if (!shimmerRegion.ReachedLimit || shimmerRegion.TileCount != 20)
{
  throw new InvalidOperationException(
    "Tile region probing did not preserve shimmer termination semantics.");
}

_ = regionProbeWorld.TrySetTile(10, 10, new WorldTile(IsActive: false, Type: 0, WallType: 1));
WorldGridSnapshot walledRegionSnapshot = regionProbeWorld.CreateSnapshot(regionProbeMetadata);
TileRegionProbeResult walledRegion = TileRegionProbe.CountOpenTiles(
  walledRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(maximumTiles: 20));
TileRegionProbeResult jungleRegion = TileRegionProbe.CountOpenTiles(
  walledRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(jungle: true, maximumTiles: 20));
if (!walledRegion.ReachedLimit || jungleRegion.ReachedLimit || jungleRegion.TileCount != 1)
{
  throw new InvalidOperationException(
    "Tile region probing did not preserve jungle wall handling.");
}

WorldGrid openRegionWorld = new(replayWidth, replayHeight);
WorldGridSnapshot openRegionSnapshot = openRegionWorld.CreateSnapshot(regionProbeMetadata);
TileRegionProbeResult limitedRegion = TileRegionProbe.CountOpenTiles(
  openRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileRegionProbeOptions(maximumTiles: 3));
TileRegionProbeResult edgeRegion = TileRegionProbe.CountOpenTiles(
  openRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 1,
  y: 10,
  new TileRegionProbeOptions(maximumTiles: 20));
if (!limitedRegion.ReachedLimit || limitedRegion.TileCount != 3 ||
    !edgeRegion.ReachedLimit || edgeRegion.TileCount != 20)
{
  throw new InvalidOperationException(
    "Tile region probing did not preserve bounded traversal and edge termination.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  new TileRegionProbeOptions(maximumTiles: 0));
Console.WriteLine("PASS: tile region probing preserves bounded legacy traversal semantics");

WorldGrid dirtRegionWorld = new(replayWidth, replayHeight);
WorldMetadata dirtRegionMetadata = new(
  "worldgen-dirt-region-probe",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
_ = dirtRegionWorld.TrySetTile(10, 10, new WorldTile(IsActive: false, Type: 0, WallType: 2));
_ = dirtRegionWorld.TrySetTile(11, 11, new WorldTile(IsActive: false, Type: 0, WallType: 59));
_ = dirtRegionWorld.TrySetTile(13, 11, new WorldTile(IsActive: false, Type: 0, WallType: 2));
_ = dirtRegionWorld.TrySetTile(9, 10, new WorldTile(IsActive: true, Type: 1, WallType: 2));
WorldGridSnapshot dirtRegionSnapshot = dirtRegionWorld.CreateSnapshot(dirtRegionMetadata);
TileDirtRegionProbeResult dirtRegion = TileDirtRegionProbe.CountTiles(
  dirtRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileDirtRegionProbeOptions(maximumTiles: 20));
if (dirtRegion.TileCount != 3 || dirtRegion.ReachedLimit)
{
  throw new InvalidOperationException(
    "Dirt region probing did not preserve wall eligibility and diagonal/two-column traversal.");
}

TileDirtRegionProbeResult limitedDirtRegion = TileDirtRegionProbe.CountTiles(
  dirtRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileDirtRegionProbeOptions(maximumTiles: 2));
if (!limitedDirtRegion.ReachedLimit || limitedDirtRegion.TileCount != 2)
{
  throw new InvalidOperationException(
    "Dirt region probing did not preserve its maximum-count termination.");
}

_ = dirtRegionWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 147, WallType: 2));
WorldGridSnapshot icyDirtRegionSnapshot = dirtRegionWorld.CreateSnapshot(dirtRegionMetadata);
TileDirtRegionProbeResult icyDirtRegion = TileDirtRegionProbe.CountTiles(
  icyDirtRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileDirtRegionProbeOptions(maximumTiles: 20));
_ = dirtRegionWorld.TrySetTile(10, 10, new WorldTile(IsActive: false, Type: 0, WallType: 83));
WorldGridSnapshot blockedDirtRegionSnapshot = dirtRegionWorld.CreateSnapshot(dirtRegionMetadata);
TileDirtRegionProbeResult blockedDirtRegion = TileDirtRegionProbe.CountTiles(
  blockedDirtRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 10,
  y: 10,
  new TileDirtRegionProbeOptions(maximumTiles: 20));
if (!icyDirtRegion.ReachedLimit || icyDirtRegion.TileCount != 20 ||
    !blockedDirtRegion.ReachedLimit || blockedDirtRegion.TileCount != 20)
{
  throw new InvalidOperationException(
    "Dirt region probing did not preserve ice and wall termination semantics.");
}

TileDirtRegionProbeResult edgeDirtRegion = TileDirtRegionProbe.CountTiles(
  dirtRegionSnapshot,
  TileDefinitionRegistry.CreateVersion4Base(),
  x: 1,
  y: 10,
  new TileDirtRegionProbeOptions(maximumTiles: 20));
if (!edgeDirtRegion.ReachedLimit || edgeDirtRegion.TileCount != 20)
{
  throw new InvalidOperationException(
    "Dirt region probing did not preserve world-edge termination.");
}

AssertThrows<ArgumentOutOfRangeException>(() =>
  new TileDirtRegionProbeOptions(maximumTiles: 0));
Console.WriteLine("PASS: dirt region probing preserves bounded legacy traversal semantics");

WorldGrid tileStateWorld = new(replayWidth, replayHeight);
WorldMetadata tileStateMetadata = new(
  "worldgen-tile-state-query",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
WorldGridSnapshot emptyTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
TileDefinitionRegistry tileDefinitions = TileDefinitionRegistry.CreateVersion4Base();
if (!TileStateQuery.IsEmpty(emptyTileStateSnapshot, 10, 10) ||
    TileStateQuery.GetActiveTileType(emptyTileStateSnapshot, 10, 10) != -1 ||
    TileStateQuery.IsSolid(emptyTileStateSnapshot, tileDefinitions, 10, 10) ||
    TileStateQuery.IsSolidOrSloped(emptyTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve the empty inactive tile semantics.");
}

_ = tileStateWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 1));
WorldGridSnapshot solidTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsEmpty(solidTileStateSnapshot, 10, 10) ||
    TileStateQuery.GetActiveTileType(solidTileStateSnapshot, 10, 10) != 1 ||
    !TileStateQuery.IsSolid(solidTileStateSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidOrSloped(solidTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve active solid tile semantics.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, IsInactive: true));
WorldGridSnapshot inactiveTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (!TileStateQuery.IsEmpty(inactiveTileStateSnapshot, 10, 10) ||
    TileStateQuery.GetActiveTileType(inactiveTileStateSnapshot, 10, 10) != 1 ||
    TileStateQuery.IsSolid(inactiveTileStateSnapshot, tileDefinitions, 10, 10) ||
    TileStateQuery.IsSolidOrSloped(inactiveTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve inactive active-tile semantics.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, IsHalfBrick: true));
WorldGridSnapshot halfBrickTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolid(halfBrickTileStateSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidOrSloped(halfBrickTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not distinguish solid tiles from half-brick tiles.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, Slope: 1));
WorldGridSnapshot slopedTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolid(slopedTileStateSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidOrSloped(slopedTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not distinguish solid tiles from sloped tiles.");
}

_ = tileStateWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 19));
WorldGridSnapshot platformTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolid(platformTileStateSnapshot, tileDefinitions, 10, 10) ||
    TileStateQuery.IsSolidOrSloped(platformTileStateSnapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not exclude solid-top platform tiles.");
}

_ = tileStateWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 10));
WorldGridSnapshot doorTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (!TileStateQuery.IsSolid(doorTileStateSnapshot, tileDefinitions, 10, 10) ||
    TileStateQuery.IsSolid(doorTileStateSnapshot, tileDefinitions, 10, 10, noDoors: true))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve the no-doors solid tile option.");
}

_ = tileStateWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 19));
WorldGridSnapshot platformSolid2Snapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (!TileStateQuery.IsSolidWithoutPlatformTop(platformSolid2Snapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidAllowingBottomSlope(platformSolid2Snapshot, tileDefinitions, 10, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve platform solid variants.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 19, Slope: 1, FrameX: 0));
WorldGridSnapshot properPlatformSlopeSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
_ = tileStateWorld.TrySetTile(
  11,
  10,
  new WorldTile(IsActive: true, Type: 19, Slope: 1, FrameX: 144));
WorldGridSnapshot improperPlatformSlopeSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (!TileStateQuery.IsSolidWithoutPlatformTop(properPlatformSlopeSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidAllowingBottomSlope(properPlatformSlopeSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidWithoutPlatformTop(improperPlatformSlopeSnapshot, tileDefinitions, 11, 10) ||
    TileStateQuery.IsSolidAllowingBottomSlope(improperPlatformSlopeSnapshot, tileDefinitions, 11, 10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve platform top-slope frame rules.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, Slope: 3));
WorldGridSnapshot bottomSlopeSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolidWithoutPlatformTop(bottomSlopeSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidAllowingBottomSlope(bottomSlopeSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.IsSolidAllowingBottomSlope(
      bottomSlopeSnapshot,
      tileDefinitions,
      -1,
      10) ||
    !TileStateQuery.IsPlatformProperTopFrame(0) ||
    TileStateQuery.IsPlatformProperTopFrame(144) ||
    !TileStateQuery.IsPlatformProperTopFrame(468))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve bottom-slope and platform-frame semantics.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, Slope: 1));
WorldGridSnapshot rightSlopeTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
_ = tileStateWorld.TrySetTile(
  11,
  10,
  new WorldTile(IsActive: true, Type: 1, Slope: 2));
WorldGridSnapshot leftSlopeTileStateSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
_ = tileStateWorld.TrySetTile(
  12,
  10,
  new WorldTile(IsActive: true, Type: 1, Slope: 3));
WorldGridSnapshot bottomRightSlopeTileStateSnapshot =
  tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolidAllowingLeftSlope(
      rightSlopeTileStateSnapshot,
      tileDefinitions,
      10,
      10) ||
    !TileStateQuery.IsSolidAllowingRightSlope(
      rightSlopeTileStateSnapshot,
      tileDefinitions,
      10,
      10) ||
    !TileStateQuery.IsSolidAllowingLeftSlope(
      leftSlopeTileStateSnapshot,
      tileDefinitions,
      11,
      10) ||
    TileStateQuery.IsSolidAllowingRightSlope(
      leftSlopeTileStateSnapshot,
      tileDefinitions,
      11,
      10) ||
    TileStateQuery.IsSolidAllowingTopSlope(
      bottomRightSlopeTileStateSnapshot,
      tileDefinitions,
      12,
      10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve directional slope predicates.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 19, IsHalfBrick: true));
WorldGridSnapshot platformTopSlopeSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
_ = tileStateWorld.TrySetTile(
  11,
  10,
  new WorldTile(IsActive: true, Type: 1, IsHalfBrick: true));
WorldGridSnapshot halfBrickTopSlopeSnapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolidWithoutPlatforms(
      platformTopSlopeSnapshot,
      tileDefinitions,
      10,
      10) ||
    !TileStateQuery.IsSolidAllowingTopSlope(
      platformTopSlopeSnapshot,
      tileDefinitions,
      10,
      10) ||
    !TileStateQuery.IsSolidAllowingTopSlope(
      halfBrickTopSlopeSnapshot,
      tileDefinitions,
      11,
      10) ||
    TileStateQuery.IsSolidAllowingLeftSlope(
      halfBrickTopSlopeSnapshot,
      tileDefinitions,
      11,
      10) ||
    !TileStateQuery.IsSolidWithoutPlatforms(
      halfBrickTopSlopeSnapshot,
      tileDefinitions,
      11,
      10) ||
    !TileStateQuery.IsSolidWithoutPlatforms(
      halfBrickTopSlopeSnapshot,
      tileDefinitions,
      -1,
      10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve platform and half-brick slope predicates.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 1, IsHalfBrick: true, Slope: 2));
WorldGridSnapshot solidTile3Snapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (!TileStateQuery.IsSolidWithLegacyTile3Semantics(
      solidTile3Snapshot,
      tileDefinitions,
      10,
      10) ||
    TileStateQuery.IsSolidWithLegacyTile3Semantics(
      solidTile3Snapshot,
      tileDefinitions,
      1,
      10) ||
    TileStateQuery.IsSolidWithLegacyTile3Semantics(
      solidTile3Snapshot,
      tileDefinitions,
      replayWidth - 1,
      10))
{
  throw new InvalidOperationException(
    "Tile state queries did not preserve SolidTile3 value and one-tile boundary semantics.");
}

_ = tileStateWorld.TrySetTile(
  10,
  10,
  new WorldTile(IsActive: true, Type: 19, Slope: 0));
WorldGridSnapshot platformTile3Snapshot = tileStateWorld.CreateSnapshot(tileStateMetadata);
if (TileStateQuery.IsSolidWithLegacyTile3Semantics(
      platformTile3Snapshot,
      tileDefinitions,
      10,
      10))
{
  throw new InvalidOperationException(
    "Tile state queries did not exclude platforms from SolidTile3 semantics.");
}

Console.WriteLine("PASS: tile state queries preserve bounded legacy solid semantics");

WorldGrid wireQueryWorld = new(replayWidth, replayHeight);
WorldMetadata wireQueryMetadata = new(
  "wire-query",
  new WorldSeed(1456),
  replayWidth,
  replayHeight,
  spawnX: replaySpawnX,
  spawnY: replaySurfaceY);
_ = wireQueryWorld.TrySetTile(0, 0, new WorldTile(false, 0, HasWire: true));
_ = wireQueryWorld.TrySetTile(8, 8, new WorldTile(false, 0, HasWire2: true));
_ = wireQueryWorld.TrySetTile(12, 8, new WorldTile(false, 0, HasWire3: true));
_ = wireQueryWorld.TrySetTile(8, 12, new WorldTile(false, 0, HasWire4: true));
WorldGridSnapshot wireQuerySnapshot = wireQueryWorld.CreateSnapshot(wireQueryMetadata);
if (!TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, 0, 0, boxSpread: 0) ||
    !TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, 10, 10, boxSpread: 2) ||
    !TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, 10, 10, boxSpread: 3) ||
    !TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, -10, -10, boxSpread: 0) ||
    TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, 10, 10, boxSpread: 1) ||
    TileWireQuery.HasAnyWireNearby(wireQuerySnapshot, 20, 20, boxSpread: 2))
{
  throw new InvalidOperationException(
    "Tile wire queries did not preserve bounded legacy rectangle and wire-channel semantics.");
}

Console.WriteLine("PASS: tile wire query preserves bounded legacy wire semantics");

WorldGrid ropeQueryWorld = new(replayWidth, replayHeight);
WorldMetadata ropeQueryMetadata = new(
  "rope-query",
  new WorldSeed(1456),
  replayWidth,
  replayHeight,
  spawnX: replaySpawnX,
  spawnY: replaySurfaceY);
_ = ropeQueryWorld.TrySetTile(20, 19, new WorldTile(true, 213));
_ = ropeQueryWorld.TrySetTile(20, 21, new WorldTile(true, 449));
_ = ropeQueryWorld.TrySetTile(30, 29, new WorldTile(true, 213));
_ = ropeQueryWorld.TrySetTile(30, 31, new WorldTile(true, 353));
_ = ropeQueryWorld.TrySetTile(30, 30, new WorldTile(true, 19));
_ = ropeQueryWorld.TrySetTile(40, 39, new WorldTile(true, 213));
_ = ropeQueryWorld.TrySetTile(40, 41, new WorldTile(true, 1));
WorldGridSnapshot ropeQuerySnapshot = ropeQueryWorld.CreateSnapshot(ropeQueryMetadata);
TileRopeEnds directRopeEnds = TileRopeQuery.FindEnds(ropeQuerySnapshot, 20, 20);
TileRopeEnds emptyRopeEnds = TileRopeQuery.FindEnds(
  ropeQuerySnapshot,
  10,
  10,
  treatEmptyAsRopeEnd: true);
if (directRopeEnds.TopY != 19 || directRopeEnds.BottomY != 21 ||
    emptyRopeEnds.TopY != 9 || emptyRopeEnds.BottomY != 11 ||
    !TileRopeQuery.IsRope(ropeQuerySnapshot, tileDefinitions, 20, 19) ||
    !TileRopeQuery.IsRope(ropeQuerySnapshot, tileDefinitions, 30, 30) ||
    TileRopeQuery.IsRope(ropeQuerySnapshot, tileDefinitions, 40, 40) ||
    TileRopeQuery.IsRope(ropeQuerySnapshot, tileDefinitions, 50, 50))
{
  throw new InvalidOperationException(
    "Tile rope queries did not preserve bounded rope and platform-bridge semantics.");
}

Console.WriteLine("PASS: tile rope queries preserve bounded legacy rope semantics");

IReadOnlyList<TileFrameRequest> ropeFrameRequests = TileRopeEndFramingQuery.CreateRequests(
  ropeQuerySnapshot,
  tileDefinitions,
  30,
  30,
  Array.Empty<TileChangeCommand>());
if (ropeFrameRequests.Count != 2 || ropeFrameRequests[0].Y != 29 ||
    ropeFrameRequests[1].Y != 31 ||
    ropeFrameRequests.Any(request => request.MutationKind != TileFrameMutationKind.RopeEnd))
{
  throw new InvalidOperationException("Rope endpoints did not become deterministic frame requests.");
}

WorldGrid countNearbyWorld = new(replayWidth, replayHeight);
WorldMetadata countNearbyMetadata = new(
  "count-nearby",
  new WorldSeed(1456),
  replayWidth,
  replayHeight,
  spawnX: replaySpawnX,
  spawnY: replaySurfaceY);
_ = countNearbyWorld.TrySetTile(0, 0, new WorldTile(true, 7));
_ = countNearbyWorld.TrySetTile(1, 0, new WorldTile(true, 7));
_ = countNearbyWorld.TrySetTile(1, 1, new WorldTile(true, 8));
_ = countNearbyWorld.TrySetTile(2, 2, new WorldTile(false, 7));
WorldGridSnapshot countNearbySnapshot = countNearbyWorld.CreateSnapshot(countNearbyMetadata);
if (TileNeighborhoodQuery.CountNearbyTileTypes(
      countNearbySnapshot,
      0,
      0,
      1,
      Array.Empty<int>()) != 0 ||
    TileNeighborhoodQuery.CountNearbyTileTypes(
      countNearbySnapshot,
      0,
      0,
      1,
      new[] { 7, 7 }) != 2 ||
    TileNeighborhoodQuery.CountNearbyTileTypes(
      countNearbySnapshot,
      0,
      0,
      1,
      new[] { 7, 8 }, cap: 2) != 2 ||
    TileNeighborhoodQuery.CountNearbyTileTypes(
      countNearbySnapshot,
      0,
      0,
      1,
      new[] { 7, 8 }) != 3)
{
  throw new InvalidOperationException(
    "Tile neighborhood counting did not preserve clamping, duplicate-type, and cap semantics.");
}

Console.WriteLine("PASS: tile neighborhood counting preserves bounded legacy semantics");

if (WorldUpdateRatePolicy.GetRate(7, isTimeFrozen: false) != 7 ||
    WorldUpdateRatePolicy.GetRate(30, isTimeFrozen: false) != 24 ||
    WorldUpdateRatePolicy.GetRate(7, isTimeFrozen: true) != 0)
{
  throw new InvalidOperationException("World update-rate policy did not preserve legacy caps.");
}

Console.WriteLine("PASS: world update-rate policy preserves bounded legacy semantics");

WorldGrid attachmentWorld = new(replayWidth, replayHeight);
WorldMetadata attachmentMetadata = new("attachment", new WorldSeed(1456), replayWidth, replayHeight);
_ = attachmentWorld.TrySetTile(10, 10, new WorldTile(true, 1));
_ = attachmentWorld.TrySetTile(11, 10, new WorldTile(true, 387));
_ = attachmentWorld.TrySetTile(12, 10, new WorldTile(true, 1, Slope: 3));
WorldGridSnapshot attachmentSnapshot = attachmentWorld.CreateSnapshot(attachmentMetadata);
if (!TileStateQuery.CanAttachToTop(attachmentSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.CanAttachToRight(attachmentSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.CanAttachToLeft(attachmentSnapshot, tileDefinitions, 10, 10) ||
    !TileStateQuery.CanAttachToBottom(attachmentSnapshot, tileDefinitions, 10, 10) ||
    TileStateQuery.CanAttachToRight(attachmentSnapshot, tileDefinitions, 11, 10) ||
    TileStateQuery.CanAttachToBottom(attachmentSnapshot, tileDefinitions, 12, 10))
{
  throw new InvalidOperationException("Tile attachment queries did not preserve legacy edge semantics.");
}

Console.WriteLine("PASS: tile attachment queries preserve bounded legacy edge semantics");

WorldGrid rainTraceWorld = new(replayWidth, replayHeight);
WorldMetadata rainTraceMetadata = new("rain-trace", new WorldSeed(1456), replayWidth, replayHeight);
WorldGridSnapshot clearRainSnapshot = rainTraceWorld.CreateSnapshot(rainTraceMetadata);
if (!TileLineTraceQuery.IsSafeFromRainPath(
      clearRainSnapshot,
      tileDefinitions,
      100,
      100,
      windSpeedCurrent: 0.0f))
{
  throw new InvalidOperationException("Clear rain trace was incorrectly blocked.");
}

_ = rainTraceWorld.TrySetTile(100, 95, new WorldTile(true, 1));
WorldGridSnapshot blockedRainSnapshot = rainTraceWorld.CreateSnapshot(rainTraceMetadata);
if (TileLineTraceQuery.IsSafeFromRainPath(
      blockedRainSnapshot,
      tileDefinitions,
      100,
      100,
      windSpeedCurrent: 0.0f))
{
  throw new InvalidOperationException("Solid rain trace blocker was not detected.");
}

_ = rainTraceWorld.TrySetTile(100, 95, new WorldTile(true, 1, IsInactive: true));
_ = rainTraceWorld.TrySetTile(100, 94, new WorldTile(true, 19));
WorldGridSnapshot ignoredRainSnapshot = rainTraceWorld.CreateSnapshot(rainTraceMetadata);
if (!TileLineTraceQuery.IsSafeFromRainPath(
      ignoredRainSnapshot,
      tileDefinitions,
      100,
      100,
      windSpeedCurrent: 0.0f))
{
  throw new InvalidOperationException("Inactive or platform rain trace tile was treated as solid.");
}

Console.WriteLine("PASS: snapshot rain trace preserves legacy solid-stop semantics");

if (SecretSeedAdjustmentPolicy.Adjust(10.0, 0) != 4 ||
    SecretSeedAdjustmentPolicy.Adjust(10.0, 1) != 10 ||
    SecretSeedAdjustmentPolicy.Adjust(10.0, 4) != 10 ||
    SecretSeedAdjustmentPolicy.Adjust(10.0, 5) != 20 ||
    SecretSeedAdjustmentPolicy.Adjust(2.5, 5) != 5)
{
  throw new InvalidOperationException("Secret-seed adjustment policy diverged from legacy scaling.");
}

Console.WriteLine("PASS: secret-seed adjustment policy preserves bounded legacy scaling");

RandomRectanglePointResult firstRandomPoint = RandomRectanglePointPolicy.Next(
  new GenerationRandomState(1456),
  10,
  20,
  5,
  7);
RandomRectanglePointResult secondRandomPoint = RandomRectanglePointPolicy.Next(
  new GenerationRandomState(1456),
  10,
  20,
  5,
  7);
if (firstRandomPoint != secondRandomPoint ||
    firstRandomPoint.X < 10 || firstRandomPoint.X >= 15 ||
    firstRandomPoint.Y < 20 || firstRandomPoint.Y >= 27 ||
    firstRandomPoint.State == new GenerationRandomState(1456))
{
  throw new InvalidOperationException(
    "Deterministic rectangle point generation did not preserve bounds and replay state.");
}

Console.WriteLine("PASS: deterministic rectangle point generation preserves bounded replay state");

LegacyPassRandomState legacyTerrainRandom = new(1456);
if (legacyTerrainRandom.Next(0, 5) != 1 ||
    legacyTerrainRandom.Next(0, 5) != 0 ||
    legacyTerrainRandom.Next(0, 5) != 2)
{
  throw new InvalidOperationException(
    "Pass-scoped legacy UnifiedRandom replay diverged from seed-1456 sequence.");
}

Console.WriteLine("PASS: pass-scoped legacy UnifiedRandom replay preserves seed sequence");

LegacyPassRandomState terrainOffsetRandom = new(1456);
double plateauOffset = LegacyTerrainSurfaceOffsetPolicy.Next(
  terrainOffsetRandom,
  LegacyTerrainFeatureKind.Plateau,
  specialWorld: false);
double mountainOffset = LegacyTerrainSurfaceOffsetPolicy.Next(
  terrainOffsetRandom,
  LegacyTerrainFeatureKind.Mountain,
  specialWorld: false);
if (Math.Abs(plateauOffset) > 0.0001 || Math.Abs(mountainOffset + 3.0) > 0.0001)
{
  throw new InvalidOperationException(
    $"Legacy terrain surface offsets diverged: plateau={plateauOffset}, mountain={mountainOffset}.");
}

Console.WriteLine("PASS: default TerrainPass surface offsets preserve source predicates");

LegacyBeachBoundsDefinitionSourceCheck();
LegacyTerrainPassContractSourceCheck();
LegacyTerrainFeatureRunSourceCheck();
LegacyTerrainCentralFeatureGuardSourceCheck();
LegacyTerrainWorldSizeClampSourceCheck();
LegacyTerrainRightBeachFeatureResetSourceCheck();
LegacyTerrainSurfaceClampSourceCheck();
LegacyTerrainColumnContractSourceCheck();
LegacyCavePassContractSourceCheck();
LegacyCavePassVerticalRangeSourceCheck();
LegacyTileRunnerMutationSourceCheck();
LegacyTileRunnerEnvelopeSourceCheck();
LegacyTileRunnerDistanceSourceCheck();
LegacyTileRunnerCandidateSourceCheck();
LegacyGenerationClearabilitySourceCheck();
LegacyTileRunnerOverrideSourceCheck();
LegacyTileRunnerTargetRegistrySourceCheck();
LegacyTileRunnerCandidateRegistrySourceCheck();
LegacyTileRunnerNoYChangeWallSourceCheck();
LegacySmallHolesPassSourceCheck();
LegacySmallHolesBatchSourceCheck();
LegacyTileRunnerLiquidTraversalSourceCheck();
LegacyTileRunnerSideEffectSourceCheck();
LegacyTileRunnerRandomAdjustmentSourceCheck();
LegacyTileRunnerInitializationSourceCheck();
LegacyTileRunnerPerturbationSourceCheck();
LegacyTileRunnerDriftSourceCheck();
LegacyTileRunnerDriftBatchSourceCheck();
LegacyTileRunnerDirectionSourceCheck();
LegacyCavePassPipelineSourceCheck();
LegacyWavyCavererSourceCheck();
LegacyCaveTunnelSourceCheck();
LegacyCavererSourceCheck();
LegacySurfaceCavesCavererPassSourceCheck();
LegacyMountinaterSourceCheck();
LegacyMountainCavesPassSourceCheck();
LegacySurfaceCavesVerticalPassSourceCheck();
LegacySurfaceCavesVerticalTraversalSourceCheck();
LegacyEvilReplacementQuerySourceCheck();
LegacyChasmRunnerSidewaysSourceCheck();
LegacyShadowOrbPlacementSourceCheck();
LegacyChasmRunnerSourceCheck();
LegacyPlace3x2SourceCheck();
LegacyNoSurfaceTopFillSourceCheck();
LegacySurfaceIsInSpaceSourceCheck();
LegacySurfaceIsMushroomsSourceCheck();
LegacyWorldIsFrozenSourceCheck();
LegacyHallowOnSurfaceSourceCheck();
LegacyWorldInfectionPolicySourceCheck();
LegacyWorldInfectionConversionCommandSourceCheck();
LegacyWorldInfectionConversionRegistryAndCommitSourceCheck();
LegacyNoInfectionPolicySourceCheck();
LegacyWorldIsFrozenFinishPolicySourceCheck();
LegacyBiomeCleanupSourceCheck();
LegacyActuallyNoTrapsSourceCheck();
LegacyRainbowStaticRewriteSourceCheck();
LegacyPaintEverythingGraySourceCheck();
LegacyPaintEverythingNegativeSourceCheck();
LegacyCoatEverythingEchoSourceCheck();
LegacyCoatEverythingIlluminantSourceCheck();
LegacyNoSurfacePolicySourceCheck();
LegacyErrorWorldRandomBlockSourceCheck();
LegacyErrorWorldSingleTileSwapSourceCheck();
LegacyErrorWorldRectangleSwapSourceCheck();
LegacyErrorWorldCandidateSourceCheck();
LegacyErrorWorldCandidateSelectionSourceCheck();
LegacyErrorWorldSwapOperationSourceCheck();
LegacyErrorWorldAxisTrailSourceCheck();
LegacyErrorWorldAxisTrailPolicySourceCheck();
LegacyErrorWorldPassCountSourceCheck();
LegacyErrorWorldRandomBlockRewriteSourceCheck();
LegacyErrorWorldShufflePassSourceCheck();
LegacyErrorWorldFinishCleanupSourceCheck();
LegacyErrorWorldChestItemPolicySourceCheck();
LegacyExtraLiquidAddLiquidSourceCheck();
LegacyExtraLiquidAddBubbleBlocksSourceCheck();
LegacyExtraLiquidBubbleExecutionSourceCheck();
LegacyExtraLiquidFinishSourceCheck();
LegacyRainsForAYearSourceCheck();
LegacyRandomSpawnPolicySourceCheck();
LegacyTeleporterPlacementQuerySourceCheck();
LegacyTeleporterClearAreaSourceCheck();
LegacyStartInHardmodePolicySourceCheck();
LegacyDigExtraHolesSourceCheck();
LegacyRoundLandmassSeedDefinitionsSourceCheck();
LegacyPortalGunAndPooPolicySourceCheck();
LegacyOreRunnerSourceCheck();
LegacyOreHelperSourceCheck();
LegacyOrePatchTypePolicySourceCheck();
LegacyOreTierSelectionPolicySourceCheck();
LegacyOrePatchTrailSourceCheck();
LegacyOrePatchBlobSourceCheck();

LegacySurfaceHistory surfaceHistory = new(3);
surfaceHistory.Record(10.0);
surfaceHistory.Record(11.0);
surfaceHistory.Record(12.0);
surfaceHistory.Record(13.0);
if (surfaceHistory.Get(0) != 11.0 || surfaceHistory.Get(2) != 13.0)
{
  throw new InvalidOperationException("Terrain surface history ring ordering diverged from oracle.");
}

List<(int X, double Height)> retargetedColumns = new();
surfaceHistory.Retarget(20, 11.0, (x, height) => retargetedColumns.Add((x, height)));
if (retargetedColumns.Count != 3 || retargetedColumns[0] != (20, 12.0))
{
    throw new InvalidOperationException(
      $"Terrain surface history retargeting diverged: count={retargetedColumns.Count}, " +
      $"first={(retargetedColumns.Count == 0 ? "none" : retargetedColumns[0].ToString())}.");
}

Console.WriteLine("PASS: TerrainPass surface history preserves ring and retarget semantics");

LegacySurfaceHistory batchedSurfaceHistory = new(4);
batchedSurfaceHistory.Record(10.0);
batchedSurfaceHistory.Record(12.0);
batchedSurfaceHistory.Record(14.0);
batchedSurfaceHistory.Record(16.0);
IReadOnlyList<LegacySurfaceRetargetCommand> retargetBatch =
  batchedSurfaceHistory.PrepareRetargetBatch(30, 11.0);
if (retargetBatch.Count != 4 ||
    retargetBatch[0] != new LegacySurfaceRetargetCommand(30, 14.0) ||
    retargetBatch[1] != new LegacySurfaceRetargetCommand(29, 12.0) ||
    retargetBatch[2] != new LegacySurfaceRetargetCommand(28, 11.0) ||
    retargetBatch[3] != new LegacySurfaceRetargetCommand(27, 10.0))
{
  throw new InvalidOperationException(
    "Terrain surface retarget batch diverged from bounded decrement and reverse-order semantics.");
}

Console.WriteLine("PASS: Terrain surface retarget batch preserves bounded command ordering");

RandomRectanglePointResult randomWorldPoint = RandomWorldPointPolicy.Next(
  new GenerationRandomState(1456),
  replayRequest.Metadata,
  top: 10,
  right: 20,
  bottom: 30,
  left: 40);
if (randomWorldPoint.X < 40 || randomWorldPoint.X >= replayWidth - 20 ||
    randomWorldPoint.Y < 10 || randomWorldPoint.Y >= replayHeight - 30)
{
  throw new InvalidOperationException("Deterministic world point generation did not preserve insets.");
}

Console.WriteLine("PASS: deterministic world point generation preserves bounded replay state");

bool[] gemFlags = [false, false, true, false, false, false];
(GenerationRandomState gemState, int gemIndex) = GemTileRandomPolicy.NextGemIndex(
  new GenerationRandomState(1456),
  gemFlags);
GemTileRandomResult firstGemTile = GemTileRandomPolicy.NextTile(
  new GenerationRandomState(1456),
  gemFlags);
GemTileRandomResult secondGemTile = GemTileRandomPolicy.NextTile(
  new GenerationRandomState(1456),
  gemFlags);
if (gemIndex != 2 || gemState == new GenerationRandomState(1456) ||
    firstGemTile != secondGemTile ||
    firstGemTile.TileType is not (1 or 63) ||
    firstGemTile.TileType == 63 && firstGemTile.GemIndex != 2)
{
  throw new InvalidOperationException("Deterministic gem selection did not preserve legacy retry rules.");
}

Console.WriteLine("PASS: deterministic gem selection preserves bounded legacy retry semantics");

MossSelectionResult fullMossSelection = MossSelectionPolicy.Next(new GenerationRandomState(1456));
MossSelectionResult neonMossSelection = MossSelectionPolicy.Next(
  new GenerationRandomState(1456),
  justNeon: true);
if (fullMossSelection.NeonMossTileType is not (534 or 536 or 539 or 625) ||
    fullMossSelection.FirstMossType == fullMossSelection.SecondMossType ||
    fullMossSelection.FirstMossType == fullMossSelection.ThirdMossType ||
    fullMossSelection.SecondMossType == fullMossSelection.ThirdMossType ||
    neonMossSelection.FirstMossType != -1 || neonMossSelection.SecondMossType != -1 ||
    neonMossSelection.ThirdMossType != -1 ||
    neonMossSelection.State == fullMossSelection.State)
{
  throw new InvalidOperationException("Deterministic moss selection did not preserve legacy draws.");
}

Console.WriteLine("PASS: deterministic moss selection preserves bounded legacy retry semantics");

Type treeProfileRegistryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.LegacyTreeProfileRegistry") ??
  throw new InvalidOperationException("The immutable legacy tree profile registry was not found.");
Type treeProfileType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.LegacyTreeProfileDefinition") ??
  throw new InvalidOperationException("The immutable legacy tree profile definition was not found.");
IReadOnlyList<LegacyTreeProfileDefinition> defaultTreeProfiles =
  LegacyTreeProfileRegistry.RegisterDefaults();
if (defaultTreeProfiles.Count != 10 ||
    defaultTreeProfiles[0].TreeTileType != 583 ||
    defaultTreeProfiles[^1].TreeTileType != 634 ||
    !defaultTreeProfiles.SequenceEqual(LegacyTreeProfileRegistry.RegisterDefaults()))
{
  throw new InvalidOperationException("Legacy tree profile defaults were not stable or ordered.");
}

if (!((IList<LegacyTreeProfileDefinition>)defaultTreeProfiles).IsReadOnly)
{
  throw new InvalidOperationException("Legacy tree profile projection was mutable.");
}
System.Reflection.MethodInfo tryGetTreeProfile = treeProfileRegistryType.GetMethod(
  "TryGet",
  new[] { typeof(ushort), treeProfileType.MakeByRefType() }) ??
  throw new InvalidOperationException("The legacy tree profile lookup contract was not found.");
(ushort TreeTileType, ushort SaplingTileType, string Kind)[] expectedTreeProfiles =
[
  (583, 590, "GemTreeTopaz"),
  (584, 590, "GemTreeAmethyst"),
  (585, 590, "GemTreeSapphire"),
  (586, 590, "GemTreeEmerald"),
  (587, 590, "GemTreeRuby"),
  (588, 590, "GemTreeDiamond"),
  (589, 590, "GemTreeAmber"),
  (596, 595, "VanityTreeSakura"),
  (616, 615, "VanityTreeWillow"),
  (634, 20, "TreeAsh")
];
foreach ((ushort treeTileType, ushort saplingTileType, string kind) in expectedTreeProfiles)
{
  object?[] arguments = [treeTileType, null];
  bool found = (bool)(tryGetTreeProfile.Invoke(null, arguments) ?? false);
  object profile = arguments[1] ??
    throw new InvalidOperationException("A successful tree profile lookup returned no profile.");
  if (!found ||
      (ushort)(treeProfileType.GetProperty("TreeTileType")?.GetValue(profile) ?? -1) !=
        treeTileType ||
      (ushort)(treeProfileType.GetProperty("SaplingTileType")?.GetValue(profile) ?? -1) !=
        saplingTileType ||
      (int)(treeProfileType.GetProperty("MinimumHeight")?.GetValue(profile) ?? -1) != 7 ||
      (int)(treeProfileType.GetProperty("MaximumHeight")?.GetValue(profile) ?? -1) != 12 ||
      (int)(treeProfileType.GetProperty("TopPaddingNeeded")?.GetValue(profile) ?? -1) != 4 ||
      !StringComparer.Ordinal.Equals(
        treeProfileType.GetProperty("Kind")?.GetValue(profile)?.ToString(),
        kind))
  {
    throw new InvalidOperationException("Legacy tree profile lookup diverged from the source profiles.");
  }
}

IReadOnlySet<ushort> commonSaplingProfileDefaults = CommonSaplingTileRegistry.RegisterDefaults();
if (defaultTreeProfiles.Any(
      profile => !commonSaplingProfileDefaults.Contains(profile.SaplingTileType)))
{
  throw new InvalidOperationException(
    "Legacy tree profile saplings diverged from the shared common-sapling registry.");
}

Console.WriteLine("PASS: legacy tree profiles use shared common-sapling registry values");

object?[] unknownTreeProfileArguments = [ushort.MaxValue, null];
if ((bool)(tryGetTreeProfile.Invoke(null, unknownTreeProfileArguments) ?? true) ||
    (ushort)(treeProfileType.GetProperty("TreeTileType")?.GetValue(
      unknownTreeProfileArguments[1]) ?? -1) != 0)
{
  throw new InvalidOperationException("Unknown tree profile IDs must not resolve.");
}

Console.WriteLine("PASS: immutable tree profile lookup preserves bounded legacy profile data");

Type treeGroundDefinitionType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeGroundTileDefinition") ??
  throw new InvalidOperationException("The tree ground definition contract was not found.");
Type treeGroundQueryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeGroundSuitabilityQuery") ??
  throw new InvalidOperationException("The tree ground suitability query was not found.");
System.Reflection.MethodInfo isTreeGroundSuitable = treeGroundQueryType.GetMethod(
  "IsSuitable",
  new[] { typeof(LegacyTreeProfileKind), treeGroundDefinitionType }) ??
  throw new InvalidOperationException("The tree ground suitability contract was not found.");
System.Reflection.ConstructorInfo treeGroundDefinitionConstructor =
  treeGroundDefinitionType.GetConstructor(
    new[] { typeof(ushort), typeof(bool), typeof(bool), typeof(bool) }) ??
  throw new InvalidOperationException("The tree ground definition constructor was not found.");
object CreateTreeGroundDefinition(ushort tileType, bool isStone, bool isMoss, bool isGrass)
{
  return treeGroundDefinitionConstructor.Invoke([tileType, isStone, isMoss, isGrass]);
}

bool IsTreeGroundSuitable(
  LegacyTreeProfileKind kind,
  ushort tileType,
  bool isStone,
  bool isMoss,
  bool isGrass)
{
  object definition = CreateTreeGroundDefinition(tileType, isStone, isMoss, isGrass);
  return (bool)(isTreeGroundSuitable.Invoke(null, [kind, definition]) ?? false);
}

if (!IsTreeGroundSuitable(LegacyTreeProfileKind.GemTreeTopaz, 1, true, false, false) ||
    !IsTreeGroundSuitable(LegacyTreeProfileKind.GemTreeAmber, 182, false, true, false) ||
    IsTreeGroundSuitable(LegacyTreeProfileKind.GemTreeRuby, 2, false, false, true) ||
    !IsTreeGroundSuitable(LegacyTreeProfileKind.VanityTreeSakura, 2, false, false, true) ||
    IsTreeGroundSuitable(LegacyTreeProfileKind.VanityTreeWillow, 23, false, false, true) ||
    IsTreeGroundSuitable(LegacyTreeProfileKind.VanityTreeWillow, 199, false, false, true) ||
    !IsTreeGroundSuitable(LegacyTreeProfileKind.TreeAsh, 633, false, false, false) ||
    IsTreeGroundSuitable(LegacyTreeProfileKind.TreeAsh, 634, true, true, true))
{
  throw new InvalidOperationException("Tree ground suitability diverged from legacy profile rules.");
}

Console.WriteLine("PASS: tree ground suitability preserves bounded legacy profile rules");

Type treeWallDefinitionType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeWallDefinition") ??
  throw new InvalidOperationException("The tree wall definition contract was not found.");
Type treeWallQueryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeWallSuitabilityQuery") ??
  throw new InvalidOperationException("The tree wall suitability query was not found.");
System.Reflection.MethodInfo isTreeWallSuitable = treeWallQueryType.GetMethod(
  "IsSuitable",
  new[] { typeof(LegacyTreeProfileKind), treeWallDefinitionType }) ??
  throw new InvalidOperationException("The tree wall suitability contract was not found.");
System.Reflection.ConstructorInfo treeWallDefinitionConstructor =
  treeWallDefinitionType.GetConstructor(new[] { typeof(ushort), typeof(bool) }) ??
  throw new InvalidOperationException("The tree wall definition constructor was not found.");
bool IsTreeWallSuitable(
  LegacyTreeProfileKind kind,
  ushort wallType,
  bool allowsPlantsToGrow)
{
  object wall = treeWallDefinitionConstructor.Invoke([wallType, allowsPlantsToGrow]);
  return (bool)(isTreeWallSuitable.Invoke(null, [kind, wall]) ?? false);
}

if (!IsTreeWallSuitable(LegacyTreeProfileKind.VanityTreeSakura, 700, true) ||
    IsTreeWallSuitable(LegacyTreeProfileKind.VanityTreeWillow, 2, false) ||
    !IsTreeWallSuitable(LegacyTreeProfileKind.GemTreeTopaz, 2, false) ||
    !IsTreeWallSuitable(LegacyTreeProfileKind.GemTreeRuby, 215, false) ||
    IsTreeWallSuitable(LegacyTreeProfileKind.GemTreeDiamond, 216, false) ||
    !IsTreeWallSuitable(LegacyTreeProfileKind.GemTreeAmber, 700, true) ||
    !IsTreeWallSuitable(LegacyTreeProfileKind.TreeAsh, 701, true))
{
  throw new InvalidOperationException("Tree wall suitability diverged from legacy profile rules.");
}

Console.WriteLine("PASS: tree wall suitability preserves bounded legacy profile rules");

Type treeGrowthDispatchType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeGrowthDispatch") ??
  throw new InvalidOperationException("The tree growth dispatch contract was not found.");
Type treeGrowthDispatchKindType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeGrowthDispatchKind") ??
  throw new InvalidOperationException("The tree growth dispatch kind was not found.");
Type treeGrowthDispatchQueryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeGrowthDispatchQuery") ??
  throw new InvalidOperationException("The tree growth dispatch query was not found.");
System.Reflection.MethodInfo tryGetTreeGrowthDispatch = treeGrowthDispatchQueryType.GetMethod(
  "TryGet",
  new[] { typeof(int), treeGrowthDispatchType.MakeByRefType() }) ??
  throw new InvalidOperationException("The tree growth dispatch lookup contract was not found.");
System.Reflection.PropertyInfo dispatchKindProperty = treeGrowthDispatchType.GetProperty("Kind") ??
  throw new InvalidOperationException("The tree growth dispatch kind property was not found.");
System.Reflection.PropertyInfo dispatchProfileProperty =
  treeGrowthDispatchType.GetProperty("ProfileKind") ??
  throw new InvalidOperationException("The tree growth dispatch profile property was not found.");
object?[] dispatchArguments = [5, null];
if (!(bool)(tryGetTreeGrowthDispatch.Invoke(null, dispatchArguments) ?? false) ||
    !StringComparer.Ordinal.Equals(dispatchKindProperty.GetValue(dispatchArguments[1])?.ToString(), "Ordinary"))
{
  throw new InvalidOperationException("Ordinary tree dispatch did not preserve the legacy handler.");
}

dispatchArguments = [323, null];
if (!(bool)(tryGetTreeGrowthDispatch.Invoke(null, dispatchArguments) ?? false) ||
    !StringComparer.Ordinal.Equals(dispatchKindProperty.GetValue(dispatchArguments[1])?.ToString(), "Palm"))
{
  throw new InvalidOperationException("Palm tree dispatch did not preserve the legacy handler.");
}

dispatchArguments = [634, null];
if (!(bool)(tryGetTreeGrowthDispatch.Invoke(null, dispatchArguments) ?? false) ||
    !StringComparer.Ordinal.Equals(dispatchKindProperty.GetValue(dispatchArguments[1])?.ToString(), "Profile") ||
    !StringComparer.Ordinal.Equals(
      dispatchProfileProperty.GetValue(dispatchArguments[1])?.ToString(),
      "TreeAsh"))
{
  throw new InvalidOperationException("Ash tree dispatch did not preserve the legacy profile.");
}

dispatchArguments = [631, null];
if ((bool)(tryGetTreeGrowthDispatch.Invoke(null, dispatchArguments) ?? true))
{
  throw new InvalidOperationException("Unsupported tree dispatch IDs must not resolve.");
}

Console.WriteLine("PASS: tree growth dispatch preserves bounded legacy handler selection");

Type treeEligibilityReasonType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeProfileGrowthEligibilityReason") ??
  throw new InvalidOperationException("The tree profile eligibility reason was not found.");
Type treeEligibilityResultType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeProfileGrowthEligibilityResult") ??
  throw new InvalidOperationException("The tree profile eligibility result was not found.");
Type treeEligibilityQueryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeProfileGrowthEligibilityQuery") ??
  throw new InvalidOperationException("The tree profile eligibility query was not found.");
System.Reflection.MethodInfo evaluateTreeEligibility = treeEligibilityQueryType.GetMethod(
  "Evaluate",
  new[]
  {
    typeof(WorldGridSnapshot),
    typeof(LegacyTreeProfileKind),
    typeof(int),
    typeof(int),
    typeof(IReadOnlyDictionary<ushort, TreeGroundTileDefinition>),
    typeof(IReadOnlyDictionary<ushort, TreeWallDefinition>),
    typeof(bool)
  }) ?? throw new InvalidOperationException("The tree profile eligibility contract was not found.");
System.Reflection.PropertyInfo eligibilityReasonProperty =
  treeEligibilityResultType.GetProperty("Reason") ??
  throw new InvalidOperationException("The tree profile eligibility reason property was not found.");
System.Reflection.PropertyInfo eligibilityGroundYProperty =
  treeEligibilityResultType.GetProperty("GroundY") ??
  throw new InvalidOperationException("The tree profile eligibility ground Y property was not found.");
Dictionary<ushort, TreeGroundTileDefinition> treeGroundDefinitions = new()
{
  [1] = new TreeGroundTileDefinition(1, IsStone: true, IsMoss: false, IsGrass: false)
};
Dictionary<ushort, TreeWallDefinition> treeWallDefinitions = new()
{
  [2] = new TreeWallDefinition(2, AllowsPlantsToGrow: false)
};
WorldGrid treeEligibilityWorld = new(replayWidth, replayHeight);
_ = treeEligibilityWorld.TrySetTile(100, 100, new WorldTile(true, 590));
_ = treeEligibilityWorld.TrySetTile(100, 101, new WorldTile(true, 590));
_ = treeEligibilityWorld.TrySetTile(100, 102, new WorldTile(true, 1));
_ = treeEligibilityWorld.TrySetTile(99, 102, new WorldTile(true, 1));
_ = treeEligibilityWorld.TrySetTile(100, 101, new WorldTile(true, 590, WallType: 2));
object EvaluateTreeEligibility(WorldGridSnapshot snapshot, bool ignoreWalls = false)
{
  return evaluateTreeEligibility.Invoke(
    null,
    [
      snapshot,
      LegacyTreeProfileKind.GemTreeTopaz,
      100,
      100,
      treeGroundDefinitions,
      treeWallDefinitions,
      ignoreWalls
    ]) ?? throw new InvalidOperationException("Tree profile eligibility returned no result.");
}

object eligibleTreeResult = EvaluateTreeEligibility(
  treeEligibilityWorld.CreateSnapshot(replayRequest.Metadata));
if (!StringComparer.Ordinal.Equals(eligibilityReasonProperty.GetValue(eligibleTreeResult)?.ToString(), "Eligible") ||
    (int)(eligibilityGroundYProperty.GetValue(eligibleTreeResult) ?? -1) != 102)
{
  throw new InvalidOperationException("Tree profile eligibility did not follow the sapling chain.");
}

_ = treeEligibilityWorld.TrySetLiquid(99, 101, 1, 0);
object liquidBlockedTreeResult = EvaluateTreeEligibility(
  treeEligibilityWorld.CreateSnapshot(replayRequest.Metadata));
if (!StringComparer.Ordinal.Equals(
      eligibilityReasonProperty.GetValue(liquidBlockedTreeResult)?.ToString(),
      "LiquidAboveGround"))
{
  throw new InvalidOperationException("Tree profile eligibility did not reject liquid above ground.");
}

_ = treeEligibilityWorld.TrySetLiquid(99, 101, 0, 0);
_ = treeEligibilityWorld.TrySetTile(100, 102, new WorldTile(true, 1, Slope: 1));
object slopeBlockedTreeResult = EvaluateTreeEligibility(
  treeEligibilityWorld.CreateSnapshot(replayRequest.Metadata));
if (!StringComparer.Ordinal.Equals(
      eligibilityReasonProperty.GetValue(slopeBlockedTreeResult)?.ToString(),
      "GroundShape"))
{
  throw new InvalidOperationException("Tree profile eligibility did not reject sloped ground.");
}

_ = treeEligibilityWorld.TrySetTile(100, 102, new WorldTile(true, 1));
_ = treeEligibilityWorld.TrySetTile(99, 102, new WorldTile());
object neighborBlockedTreeResult = EvaluateTreeEligibility(
  treeEligibilityWorld.CreateSnapshot(replayRequest.Metadata));
if (!StringComparer.Ordinal.Equals(
      eligibilityReasonProperty.GetValue(neighborBlockedTreeResult)?.ToString(),
      "NoSuitableNeighbor"))
{
  throw new InvalidOperationException("Tree profile eligibility did not require a suitable neighbor.");
}

Console.WriteLine("PASS: profile tree eligibility preserves bounded legacy root checks");

Type treeCanopyQueryType = typeof(WorldGenerationRequest).Assembly.GetType(
  "Terraria.Dome.Simulation.WorldGeneration.TreeCanopyClearanceQuery") ??
  throw new InvalidOperationException("The tree canopy clearance query was not found.");
IReadOnlySet<ushort> plantExceptions = TreeCanopyClearanceQuery.RegisterPlantExceptionDefaults();
if (plantExceptions.Count != 23 || !plantExceptions.Contains(3) || plantExceptions.Contains(0) ||
    !plantExceptions.SetEquals(TreeCanopyClearanceQuery.RegisterPlantExceptionDefaults()))
{
  throw new InvalidOperationException("Tree canopy plant exceptions were not stable or bounded.");
}
System.Reflection.MethodInfo isTreeCanopyClear = treeCanopyQueryType.GetMethod(
  "IsClear",
  new[]
  {
    typeof(WorldGridSnapshot),
    typeof(int),
    typeof(int),
    typeof(int),
    typeof(int),
    typeof(int),
    typeof(IReadOnlySet<ushort>)
  }) ?? throw new InvalidOperationException("The tree canopy clearance contract was not found.");
HashSet<ushort> commonSaplingTypes = [20, 590, 595, 615];
WorldGrid canopyWorld = new(replayWidth, replayHeight);
WorldGridSnapshot clearCanopySnapshot = canopyWorld.CreateSnapshot(replayRequest.Metadata);
bool InvokeTreeCanopyClear(
  WorldGridSnapshot snapshot,
  int startX,
  int endX,
  int startY,
  int endY,
  int ignoreId)
{
  return (bool)(isTreeCanopyClear.Invoke(
    null,
    [snapshot, startX, endX, startY, endY, ignoreId, commonSaplingTypes]) ?? false);
}

if (!InvokeTreeCanopyClear(clearCanopySnapshot, 10, 12, 10, 12, 20))
{
  throw new InvalidOperationException("Empty tree canopy was incorrectly rejected.");
}

_ = canopyWorld.TrySetTile(11, 11, new WorldTile(true, 3));
if (!InvokeTreeCanopyClear(
      canopyWorld.CreateSnapshot(replayRequest.Metadata),
      10,
      12,
      10,
      12,
      20))
{
  throw new InvalidOperationException("A legacy plant exception was incorrectly rejected.");
}

_ = canopyWorld.TrySetTile(11, 11, new WorldTile(true, 1));
if (InvokeTreeCanopyClear(
      canopyWorld.CreateSnapshot(replayRequest.Metadata),
      10,
      12,
      10,
      12,
      20))
{
  throw new InvalidOperationException("A non-sapling active tile was incorrectly accepted.");
}

if (InvokeTreeCanopyClear(clearCanopySnapshot, -1, 12, 10, 12, 20) ||
    InvokeTreeCanopyClear(clearCanopySnapshot, 10, 12, 10, replayHeight, 20))
{
  throw new InvalidOperationException("Out-of-bounds tree canopy was incorrectly accepted.");
}

Console.WriteLine("PASS: tree canopy clearance preserves bounded legacy empty-tile rules");

IReadOnlySet<ushort> commonSaplingDefaults = CommonSaplingTileRegistry.RegisterDefaults();
if (commonSaplingDefaults.Count != 4 || !commonSaplingDefaults.Contains(20) ||
    !commonSaplingDefaults.Contains(590) || !commonSaplingDefaults.Contains(595) ||
    !commonSaplingDefaults.Contains(615))
{
  throw new InvalidOperationException("Common-sapling registry diverged from legacy TileID sets.");
}

Console.WriteLine("PASS: common-sapling registry preserves legacy TileID sets");

WorldGrid profileTrunkWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent profileTrunkState = new(44);
List<TileChangeCommand> profileTrunkCommands = new();
if (!new TreeProfileTrunkCommandSystem().TryAppendCommands(
      profileTrunkWorld.CreateSnapshot(replayRequest.Metadata),
      LegacyTreeProfileKind.GemTreeTopaz,
      originX: 120,
      groundY: 120,
      height: 7,
      new TileProtectionComponent(120, 120, 0, 0),
      ref profileTrunkState,
      profileTrunkCommands) ||
    profileTrunkCommands.Count != 7 ||
    profileTrunkCommands[0].TileType != 583 ||
    profileTrunkCommands[^1].Y != 113)
{
  throw new InvalidOperationException("Profile tree trunk commands were not generated deterministically.");
}

WorldGrid protectedTrunkWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent protectedTrunkState = new(45);
List<TileChangeCommand> protectedTrunkCommands = new();
if (new TreeProfileTrunkCommandSystem().TryAppendCommands(
      protectedTrunkWorld.CreateSnapshot(replayRequest.Metadata),
      LegacyTreeProfileKind.TreeAsh,
      originX: 120,
      groundY: 120,
      height: 7,
      new TileProtectionComponent(120, 113, 2, 7),
      ref protectedTrunkState,
      protectedTrunkCommands) ||
    protectedTrunkCommands.Count != 0)
{
  throw new InvalidOperationException("Protected profile tree trunk cells were not rejected atomically.");
}

Console.WriteLine("PASS: profile tree trunk commands preserve bounded command-only placement");

if (!OrdinaryTreeGroundQuery.IsSuitable(2) ||
    !OrdinaryTreeGroundQuery.IsSuitable(70) ||
    !OrdinaryTreeGroundQuery.IsSuitable(662) ||
    OrdinaryTreeGroundQuery.IsSuitable(1) ||
    OrdinaryTreeGroundQuery.IsSuitable(583))
{
  throw new InvalidOperationException("Ordinary tree ground tile classification diverged from legacy.");
}

Console.WriteLine("PASS: ordinary tree ground classification preserves bounded legacy tile rules");

if (!OrdinaryTreeGrowthEligibilityQuery.IsSuitableWall(
      new TreeWallDefinition(0, true)) ||
    OrdinaryTreeGrowthEligibilityQuery.IsSuitableWall(
      new TreeWallDefinition(1, false)))
{
  throw new InvalidOperationException("Ordinary tree wall classification diverged from legacy.");
}

Console.WriteLine("PASS: ordinary tree wall classification preserves bounded legacy rules");

WorldGrid ordinaryTreeWorld = new(replayWidth, replayHeight);
_ = ordinaryTreeWorld.TrySetTile(160, 100, new WorldTile(true, 20));
_ = ordinaryTreeWorld.TrySetTile(160, 101, new WorldTile(true, 2));
_ = ordinaryTreeWorld.TrySetTile(159, 101, new WorldTile(true, 2));
OrdinaryTreeGrowthEligibilityResult ordinaryTreeEligibility =
  OrdinaryTreeGrowthEligibilityQuery.Evaluate(
    ordinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
    160,
    100,
    new Dictionary<ushort, TreeWallDefinition>(),
    ignoreWalls: false);
if (!ordinaryTreeEligibility.IsEligible || ordinaryTreeEligibility.GroundY != 101)
{
  throw new InvalidOperationException("Ordinary tree eligibility did not preserve legacy root checks.");
}

_ = ordinaryTreeWorld.TrySetLiquid(159, 100, 1, 0);
ordinaryTreeEligibility = OrdinaryTreeGrowthEligibilityQuery.Evaluate(
  ordinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
  160,
  100,
  new Dictionary<ushort, TreeWallDefinition>(),
  ignoreWalls: false);
if (ordinaryTreeEligibility.Reason != OrdinaryTreeGrowthEligibilityReason.LiquidAboveGround)
{
  throw new InvalidOperationException("Ordinary tree eligibility did not reject liquid above ground.");
}

_ = ordinaryTreeWorld.TrySetLiquid(159, 100, 0, 0);
_ = ordinaryTreeWorld.TrySetTile(160, 100, new WorldTile(true, 20, WallType: 1));
ordinaryTreeEligibility = OrdinaryTreeGrowthEligibilityQuery.Evaluate(
  ordinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
  160,
  100,
  new Dictionary<ushort, TreeWallDefinition>(),
  ignoreWalls: false);
if (ordinaryTreeEligibility.Reason != OrdinaryTreeGrowthEligibilityReason.WallUnsuitable)
{
  throw new InvalidOperationException("Ordinary tree eligibility did not reject unsuitable walls.");
}

ordinaryTreeEligibility = OrdinaryTreeGrowthEligibilityQuery.Evaluate(
  ordinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
  160,
  100,
  new Dictionary<ushort, TreeWallDefinition>(),
  ignoreWalls: true);
if (!ordinaryTreeEligibility.IsEligible)
{
  throw new InvalidOperationException("Ordinary tree eligibility did not honor ignoreWalls.");
}

Console.WriteLine("PASS: ordinary tree eligibility preserves bounded legacy root checks");

WorldGrid ordinaryTreeTrunkWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent ordinaryTreeTrunkState = new(46);
List<TileChangeCommand> ordinaryTreeTrunkCommands = new();
if (!new OrdinaryTreeTrunkCommandSystem().TryAppendCommands(
      ordinaryTreeTrunkWorld.CreateSnapshot(replayRequest.Metadata),
      originX: 180,
      groundY: 120,
      height: 5,
      new TileProtectionComponent(180, 120, 0, 0),
      ref ordinaryTreeTrunkState,
      ordinaryTreeTrunkCommands) ||
    ordinaryTreeTrunkCommands.Count != 5 ||
    ordinaryTreeTrunkCommands[0].TileType != 5 ||
    ordinaryTreeTrunkCommands[^1].Y != 115 ||
    ordinaryTreeTrunkWorld.GetTile(180, 119).IsActive)
{
  throw new InvalidOperationException("Ordinary tree trunk commands were not isolated or stable.");
}

WorldGrid blockedOrdinaryTreeWorld = new(replayWidth, replayHeight);
_ = blockedOrdinaryTreeWorld.TrySetTile(180, 114, new WorldTile(true, 1));
WorldGenerationStateComponent blockedOrdinaryTreeState = new(47);
List<TileChangeCommand> blockedOrdinaryTreeCommands = new();
if (new OrdinaryTreeTrunkCommandSystem().TryAppendCommands(
      blockedOrdinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
      originX: 180,
      groundY: 120,
      height: 5,
      new TileProtectionComponent(180, 120, 0, 0),
      ref blockedOrdinaryTreeState,
      blockedOrdinaryTreeCommands) ||
    blockedOrdinaryTreeCommands.Count != 0)
{
  throw new InvalidOperationException("Blocked ordinary tree trunk was not rejected atomically.");
}

Console.WriteLine("PASS: ordinary tree trunk commands preserve bounded command-only placement");

OrdinaryTreePlacementSystem ordinaryTreePlacementSystem = new();
WorldGrid preparedOrdinaryTreeWorld = new(replayWidth, replayHeight);
_ = preparedOrdinaryTreeWorld.TrySetTile(160, 100, new WorldTile(true, 20));
_ = preparedOrdinaryTreeWorld.TrySetTile(160, 101, new WorldTile(true, 2));
_ = preparedOrdinaryTreeWorld.TrySetTile(159, 101, new WorldTile(true, 2));
if (!ordinaryTreePlacementSystem.TryPrepare(
      preparedOrdinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
      160,
      101,
      height: 5,
      new Dictionary<ushort, TreeWallDefinition>(),
      ignoreWalls: false,
      out OrdinaryTreePlacementPreparation ordinaryPreparation,
      out string? ordinaryPreparationFailure) ||
    ordinaryPreparation.GroundY != 101 ||
    ordinaryPreparation.Height != 5 ||
    ordinaryPreparationFailure is not null)
{
  throw new InvalidOperationException(
    $"Ordinary tree placement did not prepare atomically: {ordinaryPreparationFailure}");
}

WorldGenerationStateComponent preparedOrdinaryTreeState = new(48);
List<TileChangeCommand> preparedOrdinaryTreeCommands = new();
if (!ordinaryTreePlacementSystem.TryAppendCommands(
      preparedOrdinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
      ordinaryPreparation,
      new TileProtectionComponent(160, 101, 0, 0),
      ref preparedOrdinaryTreeState,
      preparedOrdinaryTreeCommands) ||
    preparedOrdinaryTreeCommands.Count != 5)
{
  throw new InvalidOperationException("Prepared ordinary tree did not append trunk commands.");
}

Console.WriteLine("PASS: ordinary tree prepare and command phases remain separate");

GenerationRandomState ordinaryTreeHeightState = new(1456);
OrdinaryTreeHeightResult ordinaryTreeHeight = OrdinaryTreeHeightPolicy.Next(
  ordinaryTreeHeightState,
  treeHeightAddon: 3);
OrdinaryTreeHeightResult repeatedOrdinaryTreeHeight = OrdinaryTreeHeightPolicy.Next(
  ordinaryTreeHeightState,
  treeHeightAddon: 3);
if (ordinaryTreeHeight.Height < 8 || ordinaryTreeHeight.Height > 19 ||
    ordinaryTreeHeight.State == ordinaryTreeHeightState ||
    ordinaryTreeHeight != repeatedOrdinaryTreeHeight)
{
  throw new InvalidOperationException("Ordinary tree height was not deterministic or range bounded.");
}

Console.WriteLine("PASS: ordinary tree height preserves bounded deterministic selection");

GenerationRandomState ordinaryTreePrepareRandom = new(73);
if (!ordinaryTreePlacementSystem.TryPrepareWithHeightSelection(
      preparedOrdinaryTreeWorld.CreateSnapshot(replayRequest.Metadata),
      160,
      101,
      treeHeightAddon: 0,
      new Dictionary<ushort, TreeWallDefinition>(),
      ignoreWalls: false,
      ref ordinaryTreePrepareRandom,
      out OrdinaryTreePlacementPreparation randomizedOrdinaryPreparation,
      out string? randomizedOrdinaryPreparationFailure) ||
    randomizedOrdinaryPreparation.Height < 5 ||
    randomizedOrdinaryPreparation.Height > 16 ||
    randomizedOrdinaryPreparationFailure is not null ||
    ordinaryTreePrepareRandom == new GenerationRandomState(73))
{
  throw new InvalidOperationException("Ordinary tree preparation did not consume bounded height state.");
}

Console.WriteLine("PASS: ordinary tree preparation consumes bounded height state");

WorldGrid treeFrameWorld = new(replayWidth, replayHeight);
HashSet<ushort> treeTrunkTypes = [5];
_ = treeFrameWorld.TrySetTile(180, 180, new WorldTile(true, 5, FrameX: 44, FrameY: 220));
if (!TreeTrunkFrameQuery.TryGetBranchOffset(
      treeFrameWorld.CreateSnapshot(replayRequest.Metadata),
      180,
      180,
      out int branchOffset) ||
    branchOffset != 1)
{
  throw new InvalidOperationException("Tree branch frame classification diverged from legacy.");
}

_ = treeFrameWorld.TrySetTile(181, 180, new WorldTile(true, 5, FrameX: 22, FrameY: 154));
if (!TreeTrunkFrameQuery.TryGetRootOffset(
      treeFrameWorld.CreateSnapshot(replayRequest.Metadata),
      181,
      180,
      out int rootOffset) ||
    rootOffset != -1 ||
    TreeTrunkFrameQuery.TryGetBranchOffset(
      treeFrameWorld.CreateSnapshot(replayRequest.Metadata),
      1,
      180,
      treeTrunkTypes,
      out _))
{
  throw new InvalidOperationException("Tree root or margin classification diverged from legacy.");
}

Console.WriteLine("PASS: tree branch and root frames preserve bounded legacy offsets");

HashSet<ushort> leafCheckedTypes = [5, 323];
if (!TreeLeafFrameQuery.IsLeafyTreeTop(
      new WorldTile(true, 5, FrameX: 22, FrameY: 220),
      leafCheckedTypes) ||
    !TreeLeafFrameQuery.IsLeafyTreeTop(
      new WorldTile(true, 323, FrameX: 88, FrameY: 0),
      leafCheckedTypes) ||
    TreeLeafFrameQuery.IsLeafyTreeTop(
      new WorldTile(true, 5, FrameX: 66, FrameY: 100),
      leafCheckedTypes) ||
    TreeLeafFrameQuery.IsLeafyTreeTop(
      new WorldTile(false, 5, FrameX: 22, FrameY: 220),
      leafCheckedTypes))
{
  throw new InvalidOperationException("Leafy tree top classification diverged from legacy.");
}

Console.WriteLine("PASS: leafy tree top frames preserve bounded legacy rules");

WorldGrid undergroundTreeWorld = new(replayWidth, replayHeight);
_ = undergroundTreeWorld.TrySetTile(190, 180, new WorldTile(true, 60));
_ = undergroundTreeWorld.TrySetTile(189, 180, new WorldTile(true, 60));
UndergroundTreeGrowthEligibilityResult undergroundTreeEligibility =
  UndergroundTreeGrowthEligibilityQuery.Evaluate(
    undergroundTreeWorld.CreateSnapshot(replayRequest.Metadata),
    190,
    180,
    height: 5);
if (!undergroundTreeEligibility.IsEligible ||
    undergroundTreeEligibility.CanopyTopY != 168)
{
  throw new InvalidOperationException("Underground tree eligibility diverged from legacy roots.");
}

_ = undergroundTreeWorld.TrySetTile(190, 175, new WorldTile(true, 1));
undergroundTreeEligibility = UndergroundTreeGrowthEligibilityQuery.Evaluate(
  undergroundTreeWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  180,
  height: 5);
if (undergroundTreeEligibility.Reason != UndergroundTreeGrowthEligibilityReason.CanopyBlocked)
{
  throw new InvalidOperationException("Underground tree canopy did not reject active tiles.");
}

Console.WriteLine("PASS: underground tree eligibility preserves bounded legacy roots");

if (reducedVerification)
{
  Console.WriteLine(
    "SUMMARY: reduced world-generation verification completed 32 of 81 test sections " +
    "(39.5%); full verification remains available without --reduced");
  Environment.Exit(0);
}

HashSet<int> classifiedTreeTrunkTypes = [5, 323, 583];
if (!TreeTypeClassificationQuery.IsTreeType(5, classifiedTreeTrunkTypes) ||
    !TreeTypeClassificationQuery.IsTreeType(5) ||
    TreeTypeClassificationQuery.IsTreeType(-1, classifiedTreeTrunkTypes) ||
    TreeTypeClassificationQuery.IsTreeType(6, classifiedTreeTrunkTypes))
{
  throw new InvalidOperationException(
    "Tree type classification did not preserve the explicit trunk registry semantics.");
}

Console.WriteLine("CHECK: tree type classification preserves bounded registry semantics");

PaintColorValue redPaint = PaintColorQuery.GetColor(1);
PaintColorValue transparentPaint = PaintColorQuery.GetColor(30);
PaintColorValue defaultPaint = PaintColorQuery.GetColor(0);
if (redPaint != new PaintColorValue(byte.MaxValue, 0, 0, byte.MaxValue) ||
    transparentPaint != new PaintColorValue(200, 200, 200, 150) ||
    defaultPaint != new PaintColorValue(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue))
{
  throw new InvalidOperationException("Paint color mapping diverged from legacy RGBA values.");
}

IReadOnlyDictionary<int, PaintColorValue> paintDefaults = PaintColorQuery.RegisterDefaults();
if (paintDefaults.Count != 30 ||
    !paintDefaults.SequenceEqual(PaintColorQuery.RegisterDefaults()) ||
    !((IReadOnlyDictionary<int, PaintColorValue>)paintDefaults).Keys.Any())
{
  throw new InvalidOperationException("Paint color defaults were not stable and complete.");
}

if (paintDefaults is not System.Collections.Frozen.FrozenDictionary<int, PaintColorValue>)
{
  throw new InvalidOperationException("Paint color defaults were not frozen.");
}

Console.WriteLine("CHECK: paint color mapping preserves bounded legacy RGBA values");

CoatingColorValue illuminantCoating = CoatingColorQuery.GetColor(1);
CoatingColorValue invisibleCoating = CoatingColorQuery.GetColor(2);
CoatingColorValue clearCoating = CoatingColorQuery.GetColor(0);
if (illuminantCoating != new CoatingColorValue(235, 170, byte.MaxValue, byte.MaxValue) ||
    invisibleCoating != new CoatingColorValue(180, 245, byte.MaxValue, byte.MaxValue) ||
    clearCoating != default)
{
  throw new InvalidOperationException("Coating color mapping diverged from legacy RGBA values.");
}

IReadOnlyDictionary<int, CoatingColorValue> coatingDefaults = CoatingColorQuery.RegisterDefaults();
if (coatingDefaults.Count != 2 ||
    !coatingDefaults.SequenceEqual(CoatingColorQuery.RegisterDefaults()) ||
    coatingDefaults is not System.Collections.Frozen.FrozenDictionary<int, CoatingColorValue>)
{
  throw new InvalidOperationException("Coating color defaults were not stable and frozen.");
}

Console.WriteLine("CHECK: coating color mapping preserves bounded legacy RGBA values");

WorldTile coatedTile = new(
  IsActive: true,
  Type: 1,
  IsInvisibleBlock: true,
  IsFullbrightBlock: true,
  IsInvisibleWall: true,
  IsFullbrightWall: false);
CoatingColorSelection blockCoatings = CoatingColorSelectionQuery.Evaluate(coatedTile, block: true);
CoatingColorSelection wallCoatings = CoatingColorSelectionQuery.Evaluate(coatedTile, block: false);
if (blockCoatings != new CoatingColorSelection(true, true) ||
    wallCoatings != new CoatingColorSelection(false, true) ||
    CoatingColorSelectionQuery.Evaluate(null, block: true) != default)
{
  throw new InvalidOperationException("Coating color selection diverged from legacy tile flags.");
}

Console.WriteLine("CHECK: coating color selection preserves bounded tile flag semantics");

ForestBackgroundSet forestStyle = ForestBackgroundSetQuery.Evaluate(72);
ForestBackgroundSet forestDefault = ForestBackgroundSetQuery.Evaluate(0);
ForestBackgroundSet forestSpecial = ForestBackgroundSetQuery.Evaluate(13);
if (forestStyle != new ForestBackgroundSet(176, 177, 178, -1, 52) ||
    forestDefault != new ForestBackgroundSet(7, 8, 9, 10, 11) ||
    forestSpecial != new ForestBackgroundSet(7, -1, 343, 342, 341))
{
  throw new InvalidOperationException("Forest background style mapping diverged from legacy.");
}

Console.WriteLine("CHECK: forest background style mapping preserves bounded legacy sets");

if (HollowTreeFoliageStyleQuery.GetStyle(4) != 19 ||
    HollowTreeFoliageStyleQuery.GetStyle(2) != 20 ||
    HollowTreeFoliageStyleQuery.GetStyle(3) != 20 ||
    HollowTreeFoliageStyleQuery.GetStyle(0) != 3)
{
  throw new InvalidOperationException("Hollow-tree foliage style diverged from legacy mapping.");
}

IReadOnlyDictionary<int, int> hollowTreeStyles = HollowTreeFoliageStyleQuery.RegisterDefaults();
if (hollowTreeStyles.Count != 3 ||
    !hollowTreeStyles.SequenceEqual(HollowTreeFoliageStyleQuery.RegisterDefaults()) ||
    hollowTreeStyles is not System.Collections.Frozen.FrozenDictionary<int, int>)
{
  throw new InvalidOperationException("Hollow-tree foliage defaults were not stable and frozen.");
}

Console.WriteLine("CHECK: hollow-tree foliage style preserves bounded background mapping");

WorldGrid pileInvalidityWorld = new(replayWidth, replayHeight);
_ = pileInvalidityWorld.TrySetTile(100, 100, new WorldTile(IsActive: true, Type: 26));
HashSet<ushort> boulderTypes = [26, 665];
if (!PilesOrSpeleothemsInvalidityQuery.Evaluate(
      pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
      100,
      100,
      boulderTypes) ||
    PilesOrSpeleothemsInvalidityQuery.Evaluate(
      pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
      100,
      101,
      boulderTypes) ||
    PilesOrSpeleothemsInvalidityQuery.Evaluate(
      pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
      1,
      100,
      boulderTypes))
{
  throw new InvalidOperationException("Pile or speleothem invalidity diverged from legacy.");
}

Console.WriteLine("CHECK: pile or speleothem invalidity preserves bounded boulder rules");

IReadOnlyList<TileFrameRequest> squareFrameRequests = SquareTileFrameRequestQuery.CreateRequests(
  pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
  100,
  100,
  TileFrameMutationKind.TileMergeFrametest,
  Array.Empty<TileChangeCommand>());
if (squareFrameRequests.Count != 9 ||
    squareFrameRequests[0].X != 99 || squareFrameRequests[0].Y != 99 ||
    squareFrameRequests[8].X != 101 || squareFrameRequests[8].Y != 101)
{
  throw new InvalidOperationException("Square tile framing request topology diverged from legacy.");
}

Console.WriteLine("CHECK: square tile framing preserves bounded nine-point request order");

IReadOnlyList<WallFrameCoordinate> squareWallCoordinates =
  SquareWallFrameRequestQuery.CreateCoordinates(
    pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
    100,
    100);
if (squareWallCoordinates.Count != 9 ||
    squareWallCoordinates[0] != new WallFrameCoordinate(99, 99) ||
    squareWallCoordinates[8] != new WallFrameCoordinate(101, 101))
{
  throw new InvalidOperationException("Square wall framing topology diverged from legacy.");
}

Console.WriteLine("CHECK: square wall framing preserves bounded nine-point topology");

IReadOnlyList<WallFrameCoordinate> rangeFrameCoordinates = RangeFrameCoordinateQuery.CreateCoordinates(
  pileInvalidityWorld.CreateSnapshot(replayRequest.Metadata),
  100,
  100,
  101,
  102);
if (rangeFrameCoordinates.Count != 20 ||
    rangeFrameCoordinates[0] != new WallFrameCoordinate(99, 99) ||
    rangeFrameCoordinates[19] != new WallFrameCoordinate(102, 103))
{
  throw new InvalidOperationException("Range frame coordinate topology diverged from legacy.");
}

Console.WriteLine("CHECK: range framing preserves bounded expanded-rectangle order");

TileMergeNeighbors culledNeighbors = TileMergeCullApplyQuery.Apply(
  new TileMergeCullMask(
    CullUp: true,
    CullDown: false,
    CullLeft: true,
    CullRight: false,
    CullUpLeft: false,
    CullUpRight: true,
    CullDownLeft: false,
    CullDownRight: true),
  new TileMergeNeighbors(1, 2, 3, 4, 5, 6, 7, 8));
if (culledNeighbors != new TileMergeNeighbors(-1, 2, -1, 4, 5, -1, 7, -1))
{
  throw new InvalidOperationException("Tile merge culling application diverged from legacy.");
}

Console.WriteLine("CHECK: tile merge culling application preserves bounded mask semantics");

if (!SpawnAreaClassificationQuery.IsConsidered(
      y: 50,
      isRemixWorld: false,
      remixSurfaceLayerLow: 40,
      remixSurfaceLayerHigh: 80,
      worldSpawnHasBeenRandomized: false,
      hasWorldSurface: true,
      worldSurface: 100,
      underworldLayer: 250) ||
    SpawnAreaClassificationQuery.IsConsidered(
      y: 150,
      isRemixWorld: false,
      remixSurfaceLayerLow: 40,
      remixSurfaceLayerHigh: 80,
      worldSpawnHasBeenRandomized: false,
      hasWorldSurface: true,
      worldSurface: 100,
      underworldLayer: 250) ||
    !SpawnAreaClassificationQuery.IsConsidered(
      y: 150,
      isRemixWorld: false,
      remixSurfaceLayerLow: 40,
      remixSurfaceLayerHigh: 80,
      worldSpawnHasBeenRandomized: true,
      hasWorldSurface: false,
      worldSurface: 100,
      underworldLayer: 250) ||
    !SpawnAreaClassificationQuery.IsConsidered(
      y: 60,
      isRemixWorld: true,
      remixSurfaceLayerLow: 40,
      remixSurfaceLayerHigh: 80,
      worldSpawnHasBeenRandomized: false,
      hasWorldSurface: true,
      worldSurface: 100,
      underworldLayer: 250))
{
  throw new InvalidOperationException("Spawn area classification diverged from legacy rules.");
}

Console.WriteLine("CHECK: spawn area classification preserves bounded world rules");

int[] tileTypeCounts = new int[404];
tileTypeCounts[23] = 3;
tileTypeCounts[27] = 1;
tileTypeCounts[109] = 8;
tileTypeCounts[110] = 2;
if (TileTypeCategoryCountQuery.Evaluate(tileTypeCounts, TileScanGroupKind.Corruption) != 3 - 5 ||
    TileTypeCategoryCountQuery.Evaluate(tileTypeCounts, TileScanGroupKind.Hallow) != 10 ||
    TileTypeCategoryCountQuery.Evaluate(tileTypeCounts, TileScanGroupKind.None) != 0)
{
  throw new InvalidOperationException("Tile category count mapping diverged from legacy formulas.");
}

IReadOnlyDictionary<TileScanGroupKind, IReadOnlyList<int>> categoryDefaults =
  TileTypeCategoryCountQuery.RegisterDefaults();
if (categoryDefaults.Count != 3 ||
    !categoryDefaults.SequenceEqual(TileTypeCategoryCountQuery.RegisterDefaults()) ||
    categoryDefaults is not System.Collections.Frozen.FrozenDictionary<
      TileScanGroupKind, IReadOnlyList<int>> ||
    categoryDefaults.Values.Any(value => value is int[]))
{
  throw new InvalidOperationException("Tile category defaults were not stable and frozen.");
}

Console.WriteLine("CHECK: tile category counts preserve bounded legacy formulas");

WorldGrid countWorld = new(replayWidth, replayHeight);
_ = countWorld.TrySetTile(20, 20, new WorldTile(IsActive: true, Type: 23));
_ = countWorld.TrySetTile(21, 20, new WorldTile(IsActive: true, Type: 23));
_ = countWorld.TrySetTile(20, 21, new WorldTile(IsActive: false, Type: 23));
IReadOnlyList<int> countedArea = TileTypeCountAreaQuery.Count(
  countWorld.CreateSnapshot(replayRequest.Metadata),
  20,
  21,
  20,
  21);
if (countedArea[23] != 2 || countedArea[1] != 0)
{
  throw new InvalidOperationException("Tile type area counting diverged from legacy active rules.");
}

Console.WriteLine("CHECK: tile type area counting preserves bounded active-tile semantics");

HousingTestBounds housingBounds = HousingTestBoundsQuery.Calculate(
  roomStartX: 100,
  roomEndX: 120,
  roomStartY: 80,
  roomEndY: 90,
  worldWidth: replayWidth,
  worldHeight: replayHeight);
HousingTestBounds clampedHousingBounds = HousingTestBoundsQuery.Calculate(
  roomStartX: 0,
  roomEndX: 390,
  roomStartY: 0,
  roomEndY: 290,
  worldWidth: replayWidth,
  worldHeight: replayHeight);
if (housingBounds != new HousingTestBounds(54, 166, 36, 134) ||
    clampedHousingBounds != new HousingTestBounds(5, 394, 5, 294))
{
  throw new InvalidOperationException("Housing tested-room bounds diverged from legacy.");
}

Console.WriteLine("CHECK: housing tested-room bounds preserve bounded expansion and clamps");

if (!HousingHomeSpotQuery.IsEligible(new WorldTile(IsActive: false, Type: 379)) ||
    !HousingHomeSpotQuery.IsEligible(new WorldTile(IsActive: true, Type: 1)) ||
    HousingHomeSpotQuery.IsEligible(new WorldTile(IsActive: true, Type: 379)))
{
  throw new InvalidOperationException("Housing home-spot eligibility diverged from legacy.");
}

Console.WriteLine("CHECK: housing home-spot eligibility preserves bounded tile rule");

RoomNeedsResult roomNeeds = RoomNeedsQuery.Evaluate(
  new HashSet<int> { 15, 18, 19, 33 });
RoomNeedsResult missingRoomNeed = RoomNeedsQuery.Evaluate(
  new HashSet<int> { 15, 18, 19 });
if (!roomNeeds.CanSpawn || !roomNeeds.HasChair || !roomNeeds.HasTable ||
    !roomNeeds.HasDoor || !roomNeeds.HasTorch || missingRoomNeed.CanSpawn)
{
  throw new InvalidOperationException("Room-needs classification diverged from legacy.");
}

Console.WriteLine("CHECK: room-needs classification preserves bounded registry semantics");

if (RoomNeedsTileRegistry.RegisterChairDefaults().Count != 6 ||
    RoomNeedsTileRegistry.RegisterTableDefaults().Count != 12 ||
    RoomNeedsTileRegistry.RegisterDoorDefaults().Count != 13 ||
    RoomNeedsTileRegistry.RegisterTorchDefaults().Count != 26)
{
  throw new InvalidOperationException("Room-needs registries diverged from legacy TileID sets.");
}

Console.WriteLine("PASS: room-needs registries preserve legacy TileID sets");

HashSet<(int X, int Y)> roomTileCoordinates = [(101, 81), (102, 81)];
if (!HousingRoomOccupancyQuery.Contains(roomTileCoordinates, 101, 81) ||
    HousingRoomOccupancyQuery.Contains(roomTileCoordinates, 100, 81))
{
  throw new InvalidOperationException("Housing room occupancy diverged from legacy membership.");
}

Console.WriteLine("CHECK: housing room occupancy preserves bounded coordinate membership");

WorldGrid onTableWorld = new(replayWidth, replayHeight);
_ = onTableWorld.TrySetTile(190, 181, new WorldTile(true, 2));
WorldGridSnapshot onTableSnapshot = onTableWorld.CreateSnapshot(replayRequest.Metadata);
TileDefinitionRegistry onTableDefinitions = TileDefinitionRegistry.CreateVersion4Base();
OnTable1x1ValidationResult onTableSupport = OnTable1x1ValidationQuery.Evaluate(
  onTableSnapshot,
  onTableDefinitions,
  190,
  180,
  type: 49,
  hasTableAnchor: false,
  hasPlatformSideJoin: false);
if (!onTableSupport.IsSupported || onTableSupport.ShouldKill ||
    onTableSupport.UsedTableSupport)
{
  throw new InvalidOperationException("On-table 1x1 solid support contract diverged.");
}

OnTable1x1ValidationResult type78Support = OnTable1x1ValidationQuery.Evaluate(
  onTableSnapshot,
  onTableDefinitions,
  190,
  180,
  type: 78,
  hasTableAnchor: false,
  hasPlatformSideJoin: false);
if (!type78Support.IsSupported || !type78Support.UsedType78BottomSlope)
{
  throw new InvalidOperationException("On-table type-78 bottom-slope contract diverged.");
}

WorldGrid slopedSupportWorld = new(replayWidth, replayHeight);
_ = slopedSupportWorld.TrySetTile(190, 181, new WorldTile(true, 2, Slope: 1));
OnTable1x1ValidationResult slopedSupport = OnTable1x1ValidationQuery.Evaluate(
  slopedSupportWorld.CreateSnapshot(replayRequest.Metadata),
  onTableDefinitions,
  190,
  180,
  type: 49,
  hasTableAnchor: false,
  hasPlatformSideJoin: false);
if (!slopedSupport.ShouldKill || !slopedSupport.HasUnsupportedShape)
{
  throw new InvalidOperationException("On-table unsupported-shape contract diverged.");
}

OnTable1x1ValidationResult repeatedOnTableSupport = OnTable1x1ValidationQuery.Evaluate(
  onTableSnapshot,
  onTableDefinitions,
  190,
  180,
  type: 49,
  hasTableAnchor: false,
  hasPlatformSideJoin: false);
if (repeatedOnTableSupport != onTableSupport)
{
  throw new InvalidOperationException("On-table support evaluation was not deterministic.");
}

Console.WriteLine("CHECK: on-table 1x1 support contract remains deterministic");

WorldGrid sunflowerWorld = new(replayWidth, replayHeight);
TileDefinitionRegistry sunflowerDefinitions = TileDefinitionRegistry.CreateVersion4Base();
for (int offsetX = 0; offsetX < 2; offsetX++)
{
  for (int offsetY = 0; offsetY < 4; offsetY++)
  {
    _ = sunflowerWorld.TrySetTile(
      190 + offsetX,
      170 + offsetY,
      new WorldTile(true, 27, FrameX: (short)(offsetX * 18), FrameY: (short)(offsetY * 18)));
  }

  _ = sunflowerWorld.TrySetTile(190 + offsetX, 174, new WorldTile(true, 2));
}

SunflowerValidationResult sunflower = SunflowerValidationQuery.Evaluate(
  sunflowerWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170);
if (!sunflower.IsValid || sunflower.ShouldKill || !sunflower.HasAllowedGround ||
    sunflower.InvalidTiles != 0)
{
  throw new InvalidOperationException("Sunflower footprint contract diverged.");
}

_ = sunflowerWorld.TrySetTile(191, 173, new WorldTile(true, 27, FrameX: 0, FrameY: 54));
SunflowerValidationResult invalidSunflower = SunflowerValidationQuery.Evaluate(
  sunflowerWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170);
if (invalidSunflower.IsValid || !invalidSunflower.ShouldKill ||
    invalidSunflower.InvalidTiles == 0)
{
  throw new InvalidOperationException("Sunflower invalid-frame contract diverged.");
}

Console.WriteLine("CHECK: sunflower footprint and ground contract remains deterministic");

WorldGrid gnomeWorld = new(replayWidth, replayHeight);
_ = gnomeWorld.TrySetTile(190, 170, new WorldTile(true, 567, FrameY: 0));
_ = gnomeWorld.TrySetTile(190, 171, new WorldTile(true, 567, FrameY: 20));
_ = gnomeWorld.TrySetTile(190, 172, new WorldTile(true, 2));
GnomeValidationResult gnome = GnomeValidationQuery.Evaluate(
  gnomeWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170);
if (!gnome.IsValid || gnome.ShouldKill || !gnome.HasExpectedFootprint ||
    !gnome.HasSupportedGround)
{
  throw new InvalidOperationException("Gnome footprint and ground contract diverged.");
}

_ = gnomeWorld.TrySetTile(190, 171, new WorldTile(true, 567, FrameY: 0));
GnomeValidationResult invalidGnome = GnomeValidationQuery.Evaluate(
  gnomeWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170);
if (invalidGnome.IsValid || !invalidGnome.ShouldKill)
{
  throw new InvalidOperationException("Gnome invalid-frame contract diverged.");
}

Console.WriteLine("CHECK: gnome footprint and ground contract remains deterministic");

WorldGrid anchorWorld = new(replayWidth, replayHeight);
_ = anchorWorld.TrySetTile(190, 171, new WorldTile(true, 2));
_ = anchorWorld.TrySetTile(189, 170, new WorldTile(true, 2));
AnchorOrientationValidationResult anchor = AnchorOrientationValidationQuery.Evaluate(
  anchorWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  style: 0,
  wallType: 0,
  switchToWallIfInvalid: false);
if (!anchor.IsValid || anchor.ShouldKill || anchor.SuggestedStyle != 0)
{
  throw new InvalidOperationException("Anchor bottom orientation contract diverged.");
}

AnchorOrientationValidationResult wallAnchor = AnchorOrientationValidationQuery.Evaluate(
  new WorldGrid(replayWidth, replayHeight).CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  style: 9,
  wallType: 1,
  switchToWallIfInvalid: true);
if (!wallAnchor.IsValid || !wallAnchor.UsedWallFallback || wallAnchor.SuggestedStyle != 4)
{
  throw new InvalidOperationException("Anchor wall fallback contract diverged.");
}

AnchorOrientationValidationResult repeatedAnchor = AnchorOrientationValidationQuery.Evaluate(
  anchorWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  style: 0,
  wallType: 0,
  switchToWallIfInvalid: false);
if (repeatedAnchor != anchor)
{
  throw new InvalidOperationException("Anchor orientation evaluation was not deterministic.");
}

Console.WriteLine("CHECK: anchor orientation contract remains deterministic");

StinkbugBlockerValidationResult stinkbug = StinkbugBlockerValidationQuery.Evaluate(
  anchorWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  style: 2,
  wallType: 0);
if (!stinkbug.IsValid || stinkbug.ShouldKill || stinkbug.SuggestedStyle != 2 ||
    !stinkbug.SwappedHorizontalStyle)
{
  throw new InvalidOperationException("Stinkbug blocker horizontal style contract diverged.");
}

StinkbugBlockerValidationResult invalidStinkbug = StinkbugBlockerValidationQuery.Evaluate(
  new WorldGrid(replayWidth, replayHeight).CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  style: 0,
  wallType: 0);
if (invalidStinkbug.IsValid || !invalidStinkbug.ShouldKill)
{
  throw new InvalidOperationException("Stinkbug blocker invalid-anchor contract diverged.");
}

Console.WriteLine("CHECK: stinkbug blocker orientation contract remains deterministic");

WorldGrid chandelierWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 3; offsetX++)
{
  for (int offsetY = 0; offsetY < 3; offsetY++)
  {
    _ = chandelierWorld.TrySetTile(
      190 + offsetX,
      170 + offsetY,
      new WorldTile(true, 15));
  }
}

_ = chandelierWorld.TrySetTile(191, 169, new WorldTile(true, 2));
ChandelierValidationResult chandelier = ChandelierValidationQuery.Evaluate(
  chandelierWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  type: 15);
if (!chandelier.IsValid || chandelier.ShouldKill || chandelier.Width != 3 ||
    !chandelier.HasSolidSupport)
{
  throw new InvalidOperationException("Chandelier footprint and support contract diverged.");
}

_ = chandelierWorld.TrySetTile(192, 172, new WorldTile(false, 0));
ChandelierValidationResult invalidChandelier = ChandelierValidationQuery.Evaluate(
  chandelierWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  type: 15);
if (invalidChandelier.IsValid || !invalidChandelier.ShouldKill)
{
  throw new InvalidOperationException("Chandelier invalid-footprint contract diverged.");
}

Console.WriteLine("CHECK: chandelier footprint and support contract remains deterministic");

WorldGrid potWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 2; offsetX++)
{
  for (int offsetY = 0; offsetY < 2; offsetY++)
  {
    _ = potWorld.TrySetTile(
      190 + offsetX,
      170 + offsetY,
      new WorldTile(
        true,
        28,
        FrameX: (short)(offsetX * 18),
        FrameY: (short)(offsetY * 18)));
  }

  _ = potWorld.TrySetTile(190 + offsetX, 172, new WorldTile(true, 2));
}

PotValidationResult pot = PotValidationQuery.Evaluate(
  potWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170);
if (!pot.IsValid || pot.ShouldKill || pot.StyleBand != 0)
{
  throw new InvalidOperationException("Pot footprint and support contract diverged.");
}

PotValidationResult type653Pot = PotValidationQuery.Evaluate(
  potWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  190,
  170,
  type: 653);
if (type653Pot.IsValid || !type653Pot.ShouldKill || !type653Pot.UsedType653BottomSlope)
{
  throw new InvalidOperationException("Pot type-653 support contract diverged.");
}

Console.WriteLine("CHECK: pot footprint and support contract remains deterministic");

WorldGrid palmWorld = new(replayWidth, replayHeight);
_ = palmWorld.TrySetTile(190, 170, new WorldTile(true, 53, FrameX: 66));
_ = palmWorld.TrySetTile(190, 169, new WorldTile(true, 234));
PalmTreeValidationResult palm = PalmTreeValidationQuery.Evaluate(
  palmWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170);
if (!palm.IsSupported || palm.ShouldKill || palm.NormalizedAboveType != 53 ||
    palm.SuggestedFrameX != 220)
{
  throw new InvalidOperationException("Palm tree support and frame contract diverged.");
}

_ = palmWorld.TrySetTile(190, 169, new WorldTile(true, 1));
PalmTreeValidationResult invalidPalm = PalmTreeValidationQuery.Evaluate(
  palmWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170);
if (invalidPalm.IsSupported || !invalidPalm.ShouldKill)
{
  throw new InvalidOperationException("Palm tree invalid-ground contract diverged.");
}

Console.WriteLine("CHECK: palm tree support and frame contract remains deterministic");

WorldGrid configuredTreeFrameWorld = new(replayWidth, replayHeight);
_ = configuredTreeFrameWorld.TrySetTile(
  190,
  170,
  new WorldTile(true, 5, FrameX: 66, FrameY: 70));
_ = configuredTreeFrameWorld.TrySetTile(189, 170, new WorldTile(true, 5));
_ = configuredTreeFrameWorld.TrySetTile(191, 170, new WorldTile(true, 5));
_ = configuredTreeFrameWorld.TrySetTile(190, 171, new WorldTile(true, 2));
TreeFrameValidationResult treeFrame = TreeFrameValidationQuery.Evaluate(
  configuredTreeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170,
  treeType: 5);
if (!treeFrame.IsSupported || treeFrame.ShouldKill || !treeFrame.HasLeftTree ||
    !treeFrame.HasRightTree || treeFrame.SuggestedFrameX != 110)
{
  throw new InvalidOperationException("Tree frame support contract diverged.");
}

_ = configuredTreeFrameWorld.TrySetTile(190, 171, new WorldTile(true, 1));
TreeFrameValidationResult invalidTreeFrame = TreeFrameValidationQuery.Evaluate(
  configuredTreeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170,
  treeType: 5);
if (invalidTreeFrame.IsSupported || !invalidTreeFrame.ShouldKill)
{
  throw new InvalidOperationException("Tree frame invalid-ground contract diverged.");
}

Console.WriteLine("CHECK: tree frame support and branch contract remains deterministic");

_ = configuredTreeFrameWorld.TrySetTile(190, 171, new WorldTile(true, 2));
TreeSettingsValidationResult configuredTree = TreeSettingsValidationQuery.Evaluate(
  configuredTreeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170,
  treeType: 5,
  isGroundValid: groundType => groundType == 2);
if (!configuredTree.IsSupported || configuredTree.ShouldKill ||
    !configuredTree.GroundValid || !configuredTree.HasLeftTree ||
    !configuredTree.HasRightTree)
{
  throw new InvalidOperationException("Configured tree ground contract diverged.");
}

TreeSettingsValidationResult invalidConfiguredTree = TreeSettingsValidationQuery.Evaluate(
  configuredTreeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  190,
  170,
  treeType: 5,
  isGroundValid: groundType => groundType == 53);
if (invalidConfiguredTree.IsSupported || !invalidConfiguredTree.ShouldKill)
{
  throw new InvalidOperationException("Configured tree invalid-ground contract diverged.");
}

Console.WriteLine("CHECK: configured tree ground contract remains deterministic");

SpecialTownNpcSpawningResult ordinaryTownNpc = SpecialTownNpcSpawningQuery.Evaluate(
  npcType: 17,
  truffleUnlocked: false,
  roomAboveWorldSurface: true,
  noFunctionalSurface: false,
  mushroomTileCount: 0,
  mushroomTileThreshold: 100);
if (!ordinaryTownNpc.IsAllowed || ordinaryTownNpc.UsedTruffleRule)
{
  throw new InvalidOperationException("Ordinary special-town NPC rule diverged.");
}

SpecialTownNpcSpawningResult truffleAllowed = SpecialTownNpcSpawningQuery.Evaluate(
  npcType: 160,
  truffleUnlocked: true,
  roomAboveWorldSurface: true,
  noFunctionalSurface: false,
  mushroomTileCount: 100,
  mushroomTileThreshold: 100);
if (!truffleAllowed.IsAllowed || !truffleAllowed.UsedTruffleRule)
{
  throw new InvalidOperationException("Truffle spawning rule did not allow a valid room.");
}

SpecialTownNpcSpawningResult truffleRejected = SpecialTownNpcSpawningQuery.Evaluate(
  npcType: 160,
  truffleUnlocked: false,
  roomAboveWorldSurface: true,
  noFunctionalSurface: false,
  mushroomTileCount: 99,
  mushroomTileThreshold: 100);
if (truffleRejected.IsAllowed)
{
  throw new InvalidOperationException("Truffle spawning rule accepted insufficient inputs.");
}

Console.WriteLine("CHECK: special-town NPC spawning predicate remains deterministic");

List<int> achievementNpcTypes = new()
{
  38, 17, 107, 19, 22, 124, 228, 178, 18, 229, 209, 54, 108, 160, 20, 369, 207, 227,
  208, 441, 353, 550, 588, 633, 663, 670, 678, 679, 680, 681, 682, 683, 684
};
TownAchievementEligibilityResult completeAchievements = TownAchievementEligibilityQuery.Evaluate(
  achievementNpcTypes);
if (!completeAchievements.RealEstateComplete || !completeAchievements.TownSlimesComplete ||
    completeAchievements.RealEstateMissingCount != 0 ||
    completeAchievements.TownSlimesMissingCount != 0)
{
  throw new InvalidOperationException("Town achievement eligibility contract diverged.");
}

achievementNpcTypes.Remove(670);
TownAchievementEligibilityResult incompleteAchievements = TownAchievementEligibilityQuery.Evaluate(
  achievementNpcTypes);
if (!incompleteAchievements.RealEstateComplete || incompleteAchievements.TownSlimesComplete ||
    incompleteAchievements.TownSlimesMissingCount != 1)
{
  throw new InvalidOperationException("Town slime achievement eligibility contract diverged.");
}

Console.WriteLine("CHECK: town achievement eligibility predicate remains deterministic");

IReadOnlyList<int> realEstateDefaults = TownAchievementEligibilityQuery.RegisterRealEstateDefaults();
IReadOnlyList<int> townSlimeDefaults = TownAchievementEligibilityQuery.RegisterTownSlimeDefaults();
if (realEstateDefaults.Count != 25 || townSlimeDefaults.Count != 8 ||
    realEstateDefaults[0] != 38 || townSlimeDefaults[^1] != 684 ||
    !realEstateDefaults.SequenceEqual(TownAchievementEligibilityQuery.RegisterRealEstateDefaults()) ||
    !townSlimeDefaults.SequenceEqual(TownAchievementEligibilityQuery.RegisterTownSlimeDefaults()) ||
    !((IList<int>)realEstateDefaults).IsReadOnly ||
    !((IList<int>)townSlimeDefaults).IsReadOnly)
{
  throw new InvalidOperationException("Town achievement defaults were not stable or immutable.");
}

WorldGrid undergroundWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 120; offsetX++)
{
  for (int offsetY = 0; offsetY < 3; offsetY++)
  {
    _ = undergroundWorld.TrySetTile(140 + offsetX, 80 + offsetY, new WorldTile(true, 2));
  }
}

UndergroundClassificationResult denseUnderground = UndergroundClassificationQuery.Evaluate(
  undergroundWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  200,
  160,
  worldSurface: 100,
  currentTileHasWall: false);
if (!denseUnderground.IsUnderground || denseUnderground.SolidTileCount == 0)
{
  throw new InvalidOperationException("Underground dense-window contract diverged.");
}

UndergroundClassificationResult deepUnderground = UndergroundClassificationQuery.Evaluate(
  undergroundWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  200,
  200,
  worldSurface: 100,
  currentTileHasWall: false);
if (!deepUnderground.IsUnderground || !deepUnderground.UsedDeepShortcut ||
    deepUnderground.SolidTileCount != 0 || deepUnderground.ScannedTileCount != 0)
{
  throw new InvalidOperationException("Underground deep shortcut contract diverged.");
}

UndergroundClassificationResult shallowSurface = UndergroundClassificationQuery.Evaluate(
  undergroundWorld.CreateSnapshot(replayRequest.Metadata),
  sunflowerDefinitions,
  200,
  40,
  worldSurface: 100,
  currentTileHasWall: false);
if (shallowSurface.IsUnderground || !shallowSurface.UsedShallowShortcut)
{
  throw new InvalidOperationException("Underground shallow shortcut contract diverged.");
}

Console.WriteLine("CHECK: underground classification contract remains deterministic");

RoomBoundaryValidationResult validRoomBoundary = RoomBoundaryValidationQuery.Evaluate(
  x: 100,
  y: 100,
  worldWidth: 400,
  worldHeight: 300,
  roomTileCount: 20,
  roomMinX: 90,
  roomMaxX: 110,
  roomMinY: 90,
  roomMaxY: 110,
  maxRoomTiles: 200,
  maxRoomSize: 100,
  stopOnFail: true,
  roomTilesContainsPoint: true);
if (!validRoomBoundary.IsAllowed || validRoomBoundary.ShouldStop)
{
  throw new InvalidOperationException("Room boundary contract diverged for valid input.");
}

RoomBoundaryValidationResult edgeRoomBoundary = RoomBoundaryValidationQuery.Evaluate(
  x: 5,
  y: 100,
  worldWidth: 400,
  worldHeight: 300,
  roomTileCount: 20,
  roomMinX: 0,
  roomMaxX: 10,
  roomMinY: 90,
  roomMaxY: 110,
  maxRoomTiles: 200,
  maxRoomSize: 100,
  stopOnFail: true,
  roomTilesContainsPoint: true);
if (edgeRoomBoundary.IsAllowed || !edgeRoomBoundary.TooCloseToWorldEdge)
{
  throw new InvalidOperationException("Room boundary edge contract diverged.");
}

Console.WriteLine("CHECK: room boundary contract remains deterministic");

SecretSeedInputResult normalizedSecretSeed = SecretSeedInputQuery.Evaluate(
  "  My-Seed!! ",
  new List<(string Plaintext, string Code)> { ("myseed", "unused-code") });
if (!normalizedSecretSeed.HasNormalizedInput || !normalizedSecretSeed.IsMatch ||
    normalizedSecretSeed.NormalizedInput != "myseed" ||
    normalizedSecretSeed.DisplayInput != "  MySeed ")
{
  throw new InvalidOperationException("Secret-seed input normalization contract diverged.");
}

SecretSeedInputResult invalidSecretSeed = SecretSeedInputQuery.Evaluate(
  "---",
  Array.Empty<(string Plaintext, string Code)>());
if (invalidSecretSeed.HasNormalizedInput || invalidSecretSeed.IsMatch)
{
  throw new InvalidOperationException("Secret-seed empty-normalization contract diverged.");
}

Console.WriteLine("CHECK: secret-seed input normalization contract remains deterministic");

IReadOnlyList<SecretSeedDefinition> secretSeedDefinitions = SecretSeedDefinitionRegistry.RegisterDefaults();
if (secretSeedDefinitions.Count != 35 ||
    secretSeedDefinitions.Any(definition =>
      definition.Status != SecretSeedBehaviorStatus.UnsupportedWithEvidence ||
      string.IsNullOrWhiteSpace(definition.SourceAnchor)) ||
    !secretSeedDefinitions.Any(definition => definition.Variant == "no-surface") ||
    !secretSeedDefinitions.Any(definition => definition.Variant == "world-is-infected"))
{
  throw new InvalidOperationException(
    "Version4 secret-seed registry did not preserve explicit unsupported definitions.");
}

Console.WriteLine("PASS: secret-seed registry preserves explicit unsupported Version4 variants");

if (!BackgroundEquivalenceQuery.AreEquivalent(3, 31) ||
    !BackgroundEquivalenceQuery.AreEquivalent(7, 73) ||
    BackgroundEquivalenceQuery.AreEquivalent(3, 5) ||
    !BackgroundEquivalenceQuery.AreEquivalent(12, 12) ||
    BackgroundEquivalenceQuery.AreEquivalent(12, 13))
{
  throw new InvalidOperationException("Background equivalence contract diverged.");
}

Console.WriteLine("CHECK: background equivalence contract remains deterministic");

JungleChestItemSelectionResult jungleItem = JungleChestItemSelectionQuery.Evaluate(6);
if (jungleItem.BaseItemType != 213 || jungleItem.NextJungleItemCount != 7 ||
    !jungleItem.RandomOverrideDeferred || !jungleItem.CounterMutationDeferred)
{
  throw new InvalidOperationException("Jungle chest item rotation contract diverged.");
}

Console.WriteLine("CHECK: jungle chest item rotation contract remains deterministic");

if (!SecretSeedCodeCheckQuery.Matches("  My-Code! ", "mycode") ||
    SecretSeedCodeCheckQuery.Matches("wrong", "mycode") ||
    SecretSeedCodeCheckQuery.Matches("---", "mycode"))
{
  throw new InvalidOperationException("Secret-seed code check contract diverged.");
}

Console.WriteLine("CHECK: secret-seed code check contract remains deterministic");

TileSolidityOverrideProjection solidityOverrides = TileSolidityOverrideQuery.Evaluate(solid: true);
if (!solidityOverrides.Solid || solidityOverrides.BoulderTileTypes.Count != 9 ||
    solidityOverrides.CrackedBrickTileTypes.Count != 3 ||
    !solidityOverrides.BoulderTileTypes.Contains((ushort)138) ||
    solidityOverrides.BoulderTileTypes.Contains((ushort)665) ||
    !solidityOverrides.CrackedBrickTileTypes.Contains((ushort)483))
{
  throw new InvalidOperationException("Tile solidity override contract diverged.");
}

Console.WriteLine("CHECK: tile solidity override projection remains deterministic");

if (!TileSolidityOverrideQuery.RegisterBoulderDefaults().SequenceEqual(
      TileSolidityOverrideQuery.RegisterBoulderDefaults()) ||
    !TileSolidityOverrideQuery.RegisterCrackedBrickDefaults().SequenceEqual(
      TileSolidityOverrideQuery.RegisterCrackedBrickDefaults()) ||
    !((IList<ushort>)TileSolidityOverrideQuery.RegisterBoulderDefaults()).IsReadOnly ||
    !((IList<ushort>)TileSolidityOverrideQuery.RegisterCrackedBrickDefaults()).IsReadOnly)
{
  throw new InvalidOperationException("Tile solidity override defaults were mutable or unstable.");
}

if (!AlchemyPlantHarvestabilityQuery.IsHarvestable(
      style: 0,
      y: 100,
      dayTime: true,
      bloodMoon: false,
      raining: false,
      cloudAlpha: 0,
      time: 0,
      worldSurface: 200,
      remixWorld: false,
      maxTilesY: 300,
      moonPhase: 2) ||
    AlchemyPlantHarvestabilityQuery.IsHarvestable(
      style: 1,
      y: 100,
      dayTime: true,
      bloodMoon: false,
      raining: false,
      cloudAlpha: 0,
      time: 0,
      worldSurface: 200,
      remixWorld: false,
      maxTilesY: 300,
      moonPhase: 2) ||
    !HarvestableHerbQuery.IsHarvestableWithSeed(84, 0, 100, alchemyPlantHarvestable: false) ||
    HarvestableHerbQuery.IsHarvestableWithSeed(82, 0, 100, alchemyPlantHarvestable: true))
{
  throw new InvalidOperationException("Alchemy herb harvestability contract diverged.");
}

Console.WriteLine("CHECK: alchemy herb harvestability contract remains deterministic");

if (!ChestRiggingQuery.IsRigged(new WorldTile(true, 467, FrameX: 144)) ||
    ChestRiggingQuery.IsRigged(new WorldTile(true, 467, FrameX: 108)) ||
    ChestRiggingQuery.IsRigged(new WorldTile(true, 21, FrameX: 144)))
{
  throw new InvalidOperationException("Chest rigging contract diverged.");
}

Console.WriteLine("CHECK: chest rigging contract remains deterministic");

List<TownNpcSpawnCandidate> townNpcCandidates = new()
{
  new TownNpcSpawnCandidate(17, true, false, true, false, false, true),
  new TownNpcSpawnCandidate(18, true, false, true, true, false, false),
  new TownNpcSpawnCandidate(19, true, true, true, true, false, false)
};
int selectedOccupant = TownNpcSpawnSelectorQuery.Select([17, 18], townNpcCandidates);
if (selectedOccupant != 17)
{
  throw new InvalidOperationException("Town NPC occupant priority contract diverged.");
}

townNpcCandidates[0] = townNpcCandidates[0] with { AlreadyPresent = true };
int selectedRoom = TownNpcSpawnSelectorQuery.Select([17], townNpcCandidates);
if (selectedRoom != 18)
{
  throw new InvalidOperationException("Town NPC room fallback contract diverged.");
}

Console.WriteLine("CHECK: town NPC spawn selector contract remains deterministic");

UndergroundTreeHeightResult undergroundTreeHeight = UndergroundTreeHeightPolicy.Next(
  new GenerationRandomState(1456),
  treeHeightAddon: 2);
if (undergroundTreeHeight.Height < 7 || undergroundTreeHeight.Height > 16)
{
  throw new InvalidOperationException("Underground tree height was not range bounded.");
}

WorldGenerationStateComponent undergroundTreeTrunkState = new(49);
List<TileChangeCommand> undergroundTreeTrunkCommands = new();
WorldGrid undergroundTreeTrunkWorld = new(replayWidth, replayHeight);
_ = undergroundTreeTrunkWorld.TrySetTile(190, 180, new WorldTile(true, 60));
_ = undergroundTreeTrunkWorld.TrySetTile(189, 180, new WorldTile(true, 60));
if (!new UndergroundTreeTrunkCommandSystem().TryAppendCommands(
      undergroundTreeTrunkWorld.CreateSnapshot(replayRequest.Metadata),
      originX: 190,
      groundY: 180,
      height: 5,
      new TileProtectionComponent(190, 180, 0, 0),
      ref undergroundTreeTrunkState,
      undergroundTreeTrunkCommands) ||
    undergroundTreeTrunkCommands.Count != 5 ||
    undergroundTreeTrunkCommands[0].TileType != 5 ||
    undergroundTreeTrunkWorld.GetTile(190, 179).IsActive)
{
  throw new InvalidOperationException("Underground tree trunk commands were not command-only.");
}

Console.WriteLine("PASS: underground tree height and trunk commands remain bounded");

TreeLeafPassStyleResult gemLeafPassStyle = TreeLeafPassStyleQuery.Evaluate(
  x: 14,
  new WorldTile(true, 583, FrameX: 22, FrameY: 242),
  new WorldTile(true, 2),
  treeHeight: 7,
  hollowTreeFoliageStyle: 20);
if (gemLeafPassStyle.TreeFrame != 2 || gemLeafPassStyle.PassStyle != 1249 ||
    gemLeafPassStyle.TreeHeight != 7)
{
  throw new InvalidOperationException("Gem tree leaf pass style diverged from legacy.");
}

TreeLeafPassStyleResult hollowLeafPassStyle = TreeLeafPassStyleQuery.Evaluate(
  x: 1,
  new WorldTile(true, 5, FrameX: 22, FrameY: 220),
  new WorldTile(true, 109),
  treeHeight: 7,
  hollowTreeFoliageStyle: 20);
if (hollowLeafPassStyle.TreeFrame != 4 || hollowLeafPassStyle.PassStyle != 1115 ||
    hollowLeafPassStyle.TreeHeight != 12)
{
  throw new InvalidOperationException("Hollow tree leaf pass style diverged from legacy.");
}

Console.WriteLine("PASS: tree leaf pass styles preserve bounded legacy mapping");

WorldGrid treeLeafScanWorld = new(replayWidth, replayHeight);
_ = treeLeafScanWorld.TrySetTile(200, 199, new WorldTile(true, 5, FrameX: 22, FrameY: 220));
_ = treeLeafScanWorld.TrySetTile(200, 201, new WorldTile(true, 2));
TreeLeafScanResult treeLeafScan = TreeLeafScanQuery.Scan(
  treeLeafScanWorld.CreateSnapshot(replayRequest.Metadata),
  200,
  200,
  hollowTreeFoliageStyle: 20);
if (!treeLeafScan.FoundTopTile || treeLeafScan.TreeHeight != 2 ||
    treeLeafScan.PassStyle != 910 || treeLeafScan.TreeFrame != 1)
{
  throw new InvalidOperationException("Tree leaf snapshot scan diverged from legacy traversal.");
}

Console.WriteLine("PASS: tree leaf snapshot scan preserves bounded legacy traversal");

IReadOnlySet<ushort> treeLeafCheckedDefaults = TreeLeafCheckedTypeRegistry.RegisterDefaults();
if (treeLeafCheckedDefaults.Count != 13 ||
    !treeLeafCheckedDefaults.Contains(5) ||
    !treeLeafCheckedDefaults.Contains(323) ||
    !treeLeafCheckedDefaults.Contains(634) ||
    !TreeLeafFrameQuery.IsLeafyTreeTop(new WorldTile(true, 5, FrameX: 22, FrameY: 220)))
{
  throw new InvalidOperationException("Tree-leaf checked-type registry diverged from legacy TileID sets.");
}

Console.WriteLine("PASS: tree-leaf checked-type registry preserves shared legacy TileID sets");

Dictionary<ushort, TileWiringClassificationDefinition> wiringDefinitions = new()
{
  [500] = new TileWiringClassificationDefinition(
    500,
    IsMechanism: true,
    IgnoreWhenValidatingTraps: false,
    IsTrigger: false),
  [501] = new TileWiringClassificationDefinition(
    501,
    IsMechanism: true,
    IgnoreWhenValidatingTraps: true,
    IsTrigger: true)
};
WorldTile actuatorTrapTile = new(
  IsActive: true,
  Type: 500,
  IsActuated: true);
WorldTile ignoredMechanismTile = new(IsActive: true, Type: 501);
if (!TileWiringQuery.IsItATrap(actuatorTrapTile, wiringDefinitions) ||
    TileWiringQuery.IsItATrap(ignoredMechanismTile, wiringDefinitions) ||
    !TileWiringQuery.IsItATrigger(ignoredMechanismTile, wiringDefinitions) ||
    !TileWiringQuery.IsItATrigger(
      new WorldTile(IsActive: true, Type: 467, FrameX: 144),
      wiringDefinitions) ||
    !TileWiringQuery.IsItATrigger(
      new WorldTile(IsActive: true, Type: 314),
      wiringDefinitions,
      isPressurePlate: true))
{
  throw new InvalidOperationException(
    "Explicit tile wiring classifications diverged from bounded legacy predicates.");
}

Console.WriteLine("PASS: tile trap and trigger predicates preserve bounded legacy classifications");

if (!DungeonPlatformQuery.IsPlatformOrShelf(
      new WorldTile(IsActive: true, Type: 19, FrameY: 18 * 6)) ||
    !DungeonPlatformQuery.IsPlatformOrShelf(
      new WorldTile(IsActive: true, Type: 19, FrameY: 18 * 12)) ||
    DungeonPlatformQuery.IsPlatformOrShelf(
      new WorldTile(IsActive: true, Type: 19, FrameY: 18 * 13)) ||
    DungeonPlatformQuery.IsPlatformOrShelf(
      new WorldTile(IsActive: true, Type: 20, FrameY: 18 * 6)) ||
    DungeonPlatformQuery.IsPlatformOrShelf(default))
{
  throw new InvalidOperationException(
    "Dungeon platform and shelf classification diverged from legacy frame rules.");
}

Console.WriteLine("PASS: dungeon platform and shelf query preserves bounded frame rules");

AtmosphericSurfaceProfile normalAtmosphericProfile = new(
  replayHeight,
  replaySurfaceY,
  replayRequest.RockLayerY,
  IsRemixWorld: false);
AtmosphericSurfaceProfile remixAtmosphericProfile = normalAtmosphericProfile with
{
  WorldHeight = 1200,
  IsRemixWorld = true
};
if (!AtmosphericSurfaceQuery.IsSurfaceForAtmospherics(
      y: replaySurfaceY,
      profile: normalAtmosphericProfile) ||
    AtmosphericSurfaceQuery.IsSurfaceForAtmospherics(
      y: replaySurfaceY + 1,
      profile: normalAtmosphericProfile) ||
    !AtmosphericSurfaceQuery.IsSurfaceForAtmospherics(
      y: replayRequest.RockLayerY + 1,
      profile: remixAtmosphericProfile) ||
    AtmosphericSurfaceQuery.IsSurfaceForAtmospherics(
      y: replayRequest.RockLayerY,
      profile: remixAtmosphericProfile) ||
    AtmosphericSurfaceQuery.IsSurfaceForAtmospherics(
      y: replayHeight - 350,
      profile: remixAtmosphericProfile))
{
  throw new InvalidOperationException(
    "Atmospheric surface query diverged from bounded legacy remix rules.");
}

Console.WriteLine("PASS: atmospheric surface query preserves bounded legacy surface rules");

WorldGrid pressurePlateWorld = new(replayWidth, replayHeight);
_ = pressurePlateWorld.TrySetTile(25, 26, new WorldTile(IsActive: true, Type: 1));
PressurePlatePlacementDefinition pressurePlateDefinition = new(
  ForbiddenWallType: 350,
  IsBoulder: false);
if (!PressurePlatePlacementQuery.CanGenerateAt(
      pressurePlateWorld.CreateSnapshot(replayRequest.Metadata),
      tileDefinitions,
      x: 25,
      y: 25,
      pressurePlateDefinition))
{
  throw new InvalidOperationException(
    "Pressure plate placement did not accept a supported legacy tile configuration.");
}

_ = pressurePlateWorld.TrySetTile(25, 26, new WorldTile(IsActive: true, Type: 1, WallType: 350));
if (PressurePlatePlacementQuery.CanGenerateAt(
      pressurePlateWorld.CreateSnapshot(replayRequest.Metadata),
      tileDefinitions,
      x: 25,
      y: 25,
      pressurePlateDefinition))
{
  throw new InvalidOperationException("Pressure plate placement did not reject the forbidden wall.");
}

Console.WriteLine("PASS: pressure plate placement preserves bounded legacy support rules");

if (StatueStyleItemQuery.ToItem(0) != 360 ||
    StatueStyleItemQuery.ToItem(1) != 52 ||
    StatueStyleItemQuery.ToItem(43) != 1152 ||
    StatueStyleItemQuery.ToItem(51) != 3651 ||
    StatueStyleItemQuery.ToItem(63) != 3708 ||
    StatueStyleItemQuery.ToItem(76) != 4397 ||
    StatueStyleItemQuery.ToItem(82) != 5319 ||
    StatueStyleItemQuery.ToItem(10) != 446)
{
  throw new InvalidOperationException("Statue style item mapping diverged from legacy switch rules.");
}

Console.WriteLine("PASS: statue style item mapping preserves bounded legacy switch rules");

if (!PlatformSupportQuery.IsBelowANonHammeredPlatform(
      new WorldTile(IsActive: true, Type: 19, IsHalfBrick: false, Slope: 0),
      platformTypes: new ushort[] { 19 }) ||
    PlatformSupportQuery.IsBelowANonHammeredPlatform(
      new WorldTile(IsActive: true, Type: 19, IsHalfBrick: true, Slope: 0),
      platformTypes: new ushort[] { 19 }) ||
    PlatformSupportQuery.IsBelowANonHammeredPlatform(
      new WorldTile(IsActive: true, Type: 19, IsHalfBrick: false, Slope: 1),
      platformTypes: new ushort[] { 19 }) ||
    PlatformSupportQuery.IsBelowANonHammeredPlatform(
      new WorldTile(IsActive: true, Type: 20),
      platformTypes: new ushort[] { 19 }))
{
  throw new InvalidOperationException(
    "Non-hammered platform query diverged from legacy active/platform/shape rules.");
}

Console.WriteLine("PASS: non-hammered platform query preserves bounded legacy shape rules");

if (CandleItemDropQuery.ToItem(-1) != 105 ||
    CandleItemDropQuery.ToItem(0) != 105 ||
    CandleItemDropQuery.ToItem(1) != 1405 ||
    CandleItemDropQuery.ToItem(4) != 2045 ||
    CandleItemDropQuery.ToItem(13) != 2054 ||
    CandleItemDropQuery.ToItem(14) != 2153 ||
    CandleItemDropQuery.ToItem(16) != 2155 ||
    CandleItemDropQuery.ToItem(17) != 2236 ||
    CandleItemDropQuery.ToItem(30) != 3890 ||
    CandleItemDropQuery.ToItem(43) != 5606 ||
    CandleItemDropQuery.ToItem(63) != 6115 ||
    CandleItemDropQuery.ToItem(64) != 105)
{
  throw new InvalidOperationException(
    "Candle item drop mapping diverged from legacy style rules.");
}

Console.WriteLine("PASS: candle item drop mapping preserves bounded legacy switch rules");

if (PicnicTableItemDropQuery.ToItem(-1) != 4064 ||
    PicnicTableItemDropQuery.ToItem(0) != 4064 ||
    PicnicTableItemDropQuery.ToItem(1) != 4065 ||
    PicnicTableItemDropQuery.ToItem(2) != 4064)
{
  throw new InvalidOperationException(
    "Picnic table item drop mapping diverged from legacy style rules.");
}

Console.WriteLine("PASS: picnic table item drop mapping preserves bounded legacy style rules");

if (BottleItemDropQuery.ToItem(-1) != 31 ||
    BottleItemDropQuery.ToItem(0) != 31 ||
    BottleItemDropQuery.ToItem(1) != 28 ||
    BottleItemDropQuery.ToItem(2) != 110 ||
    BottleItemDropQuery.ToItem(3) != 350 ||
    BottleItemDropQuery.ToItem(4) != 351 ||
    BottleItemDropQuery.ToItem(5) != 2234 ||
    BottleItemDropQuery.ToItem(6) != 2244 ||
    BottleItemDropQuery.ToItem(7) != 2257 ||
    BottleItemDropQuery.ToItem(8) != 2258 ||
    BottleItemDropQuery.ToItem(9) != 31)
{
  throw new InvalidOperationException(
    "Bottle item drop mapping diverged from legacy style rules.");
}

Console.WriteLine("PASS: bottle item drop mapping preserves bounded legacy switch rules");

int[] benchItems =
{
  335, 2397, 2398, 2399, 2400, 2401, 2402, 2403, 2404, 2405,
  2406, 2407, 2408, 2409, 2410, 2411, 2412, 2413, 2414, 2415,
  2416, 2521, 2527, 2539, 858, 2582, 2634, 2635, 2636, 2823,
  3150, 3152, 3151, 3918, 3919, 3947, 3973, 4161, 4182, 4203,
  4224, 4313, 4582, 4993, 5164, 5185, 5206, 5564, 5617, 5705,
  5728, 5753, 5772, 5793, 5814, 5835, 5854, 5874, 5893, 5914,
  5948, 5970, 5991, 6014, 6037, 6060, 6083, 6105, 6127
};
if (BenchItemDropQuery.ToItem(-1) != 335 || BenchItemDropQuery.ToItem(0) != 335)
{
  throw new InvalidOperationException("Bench default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 68; style++)
{
  if (BenchItemDropQuery.ToItem(style) != benchItems[style])
  {
    throw new InvalidOperationException(
      $"Bench item mapping diverged for style {style}.");
  }
}

if (BenchItemDropQuery.ToItem(69) != 335)
{
  throw new InvalidOperationException("Bench out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: bench item drop mapping preserves bounded legacy switch rules");

int[] clockStyleItems =
{
  2809, 3126, 3128, 3127, 3898, 3899, 3900, 3901, 3902, 3940,
  3966, 4154, 4175, 4196, 4217, 4306, 4575, 5157, 5178, 5199,
  5557, 5610, 5698, 5721, 5746, 5764, 5785, 5806, 5827, 5847,
  5866, 5887, 5906, 5940, 5963, 5983, 6006, 6029, 6052, 6075,
  6097, 6119
};
if (ClockItemDropQuery.ToItem(-1) != 359 || ClockItemDropQuery.ToItem(0) != 359)
{
  throw new InvalidOperationException("Clock default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 5; style++)
{
  if (ClockItemDropQuery.ToItem(style) != 2237 + style - 1)
  {
    throw new InvalidOperationException($"Clock first range diverged for style {style}.");
  }
}

if (ClockItemDropQuery.ToItem(6) != 2560 || ClockItemDropQuery.ToItem(7) != 2575)
{
  throw new InvalidOperationException("Clock special style mappings diverged from legacy rules.");
}

for (int style = 8; style <= 23; style++)
{
  if (ClockItemDropQuery.ToItem(style) != 2591 + style - 8)
  {
    throw new InvalidOperationException($"Clock second range diverged for style {style}.");
  }
}

for (int style = 24; style <= 65; style++)
{
  if (ClockItemDropQuery.ToItem(style) != clockStyleItems[style - 24])
  {
    throw new InvalidOperationException($"Clock item mapping diverged for style {style}.");
  }
}

if (ClockItemDropQuery.ToItem(66) != 359)
{
  throw new InvalidOperationException("Clock out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: clock item drop mapping preserves bounded legacy switch rules");

int[] bedStyleItems =
{
  2139, 2140, 2231, 2520, 2538, 2553, 2568, 2669, 2811, 3162,
  3164, 3163, 3897, 3932, 3959, 4146, 4167, 4188, 4209, 4299,
  4567, 5149, 5170, 5191, 5549, 5602, 5690, 5713, 5740, 5757,
  5778, 5799, 5820, 5841, 5859, 5880, 5899, 5933, 5956, 5976,
  5999, 6022, 6045, 6068, 6091, 6112
};
if (BedItemDropQuery.ToItem(-1) != 224 || BedItemDropQuery.ToItem(0) != 224)
{
  throw new InvalidOperationException("Bed default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 3; style++)
{
  if (BedItemDropQuery.ToItem(style) != style + 643)
  {
    throw new InvalidOperationException($"Bed first range diverged for style {style}.");
  }
}

if (BedItemDropQuery.ToItem(4) != 920)
{
  throw new InvalidOperationException("Bed special style mapping diverged from legacy rules.");
}

for (int style = 5; style <= 8; style++)
{
  if (BedItemDropQuery.ToItem(style) != 1465 + style)
  {
    throw new InvalidOperationException($"Bed second range diverged for style {style}.");
  }
}

for (int style = 9; style <= 12; style++)
{
  if (BedItemDropQuery.ToItem(style) != 1710 + style)
  {
    throw new InvalidOperationException($"Bed third range diverged for style {style}.");
  }
}

for (int style = 13; style <= 18; style++)
{
  if (BedItemDropQuery.ToItem(style) != 2066 + style - 13)
  {
    throw new InvalidOperationException($"Bed fourth range diverged for style {style}.");
  }
}

for (int style = 19; style <= 64; style++)
{
  if (BedItemDropQuery.ToItem(style) != bedStyleItems[style - 19])
  {
    throw new InvalidOperationException($"Bed item mapping diverged for style {style}.");
  }
}

if (BedItemDropQuery.ToItem(65) != 224)
{
  throw new InvalidOperationException("Bed out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: bed item drop mapping preserves bounded legacy switch rules");

int[] candelabraStyleItems =
{
  2227, 2522, 2541, 2555, 2570, 2664, 2665, 2666, 2667, 2668,
  2825, 3168, 3170, 3169, 3893, 3935, 3961, 4149, 4170, 4191,
  4212, 4302, 4570, 5152, 5173, 5194, 5552, 5605, 5693, 5716,
  5742, 5759, 5780, 5801, 5822, 5843, 5861, 5882, 5901, 5935,
  5958, 5978, 6001, 6024, 6047, 6070, 6093, 6114
};
if (CandelabraItemDropQuery.ToItem(-1) != 349 ||
    CandelabraItemDropQuery.ToItem(0) != 349)
{
  throw new InvalidOperationException(
    "Candelabra default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 12; style++)
{
  if (CandelabraItemDropQuery.ToItem(style) != 2092 + style - 1)
  {
    throw new InvalidOperationException($"Candelabra first range diverged for style {style}.");
  }
}

for (int style = 13; style <= 16; style++)
{
  if (CandelabraItemDropQuery.ToItem(style) != 2149 + style - 13)
  {
    throw new InvalidOperationException($"Candelabra second range diverged for style {style}.");
  }
}

for (int style = 17; style <= 64; style++)
{
  if (CandelabraItemDropQuery.ToItem(style) != candelabraStyleItems[style - 17])
  {
    throw new InvalidOperationException($"Candelabra item mapping diverged for style {style}.");
  }
}

if (CandelabraItemDropQuery.ToItem(65) != 349)
{
  throw new InvalidOperationException(
    "Candelabra out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: candelabra item drop mapping preserves bounded legacy switch rules");

int[] bookcaseInitialItems =
{
  1414, 1415, 1416, 1463, 1512, 2020, 2021, 2022, 2023,
  2024, 2025, 2026, 2027, 2028, 2029, 2030, 2031
};
int[] bookcaseStyleItems =
{
  2233, 2536, 2540, 2554, 2569, 2670, 2817, 3165, 3167, 3166,
  3917, 3933, 3960, 4147, 4168, 4189, 4210, 4300, 4568, 5150,
  5171, 5192, 5550, 5603, 5691, 5714, 5758, 5779, 5800, 5821,
  5842, 5860, 5881, 5900, 5934, 5957, 5977, 6000, 6023, 6046,
  6069, 6092, 6113
};
if (BookcaseItemDropQuery.ToItem(-1) != 354 || BookcaseItemDropQuery.ToItem(0) != 354)
{
  throw new InvalidOperationException("Bookcase default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 17; style++)
{
  if (BookcaseItemDropQuery.ToItem(style) != bookcaseInitialItems[style - 1])
  {
    throw new InvalidOperationException($"Bookcase initial mapping diverged for style {style}.");
  }
}

for (int style = 18; style <= 21; style++)
{
  if (BookcaseItemDropQuery.ToItem(style) != 2135 + style - 18)
  {
    throw new InvalidOperationException($"Bookcase range diverged for style {style}.");
  }
}

for (int style = 22; style <= 64; style++)
{
  if (BookcaseItemDropQuery.ToItem(style) != bookcaseStyleItems[style - 22])
  {
    throw new InvalidOperationException($"Bookcase item mapping diverged for style {style}.");
  }
}

if (BookcaseItemDropQuery.ToItem(65) != 354)
{
  throw new InvalidOperationException("Bookcase out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: bookcase item drop mapping preserves bounded legacy switch rules");

int[] chandelierInitialItems = { 107, 108, 710, 711, 712, 1812 };
int[] chandelierStyleItems =
{
  2224, 2525, 2543, 2558, 2573, 2652, 2653, 2654, 2655, 2656,
  2657, 2813, 3177, 3179, 3178, 3894, 3938, 3964, 4152, 4173,
  4194, 4215, 4305, 4573, 5155, 5176, 5197, 5555, 5608, 5696,
  5719, 5744, 5762, 5783, 5804, 5825, 5845, 5864, 5885, 5904,
  5938, 5961, 5981, 6004, 6027, 6050, 6073, 6096, 6117
};
if (ChandelierItemDropQuery.ToItem(-1) != 106 ||
    ChandelierItemDropQuery.ToItem(0) != 106)
{
  throw new InvalidOperationException(
    "Chandelier default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 6; style++)
{
  if (ChandelierItemDropQuery.ToItem(style) != chandelierInitialItems[style - 1])
  {
    throw new InvalidOperationException($"Chandelier initial mapping diverged for style {style}.");
  }
}

for (int style = 7; style <= 17; style++)
{
  if (ChandelierItemDropQuery.ToItem(style) != 2055 + style - 7)
  {
    throw new InvalidOperationException($"Chandelier first range diverged for style {style}.");
  }
}

for (int style = 18; style <= 21; style++)
{
  if (ChandelierItemDropQuery.ToItem(style) != 2141 + style - 18)
  {
    throw new InvalidOperationException($"Chandelier second range diverged for style {style}.");
  }
}

for (int style = 22; style <= 70; style++)
{
  if (ChandelierItemDropQuery.ToItem(style) != chandelierStyleItems[style - 22])
  {
    throw new InvalidOperationException($"Chandelier item mapping diverged for style {style}.");
  }
}

if (ChandelierItemDropQuery.ToItem(71) != 106)
{
  throw new InvalidOperationException(
    "Chandelier out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: chandelier item drop mapping preserves bounded legacy switch rules");

int[] lanternStyleItems =
{
  2226, 2530, 2546, 2564, 2579, 2641, 2642, 2820, 3138, 3140,
  3139, 3891, 3943, 3970, 4157, 4178, 4199, 4220, 4309, 4578,
  5160, 5181, 5202, 5560, 5613, 5701, 5724, 5749, 5768, 5789,
  5810, 5831, 5850, 5870, 5890, 5910, 5944, 5967, 5987, 6010,
  6033, 6056, 6079, 6101, 6123
};
if (LanternItemDropQuery.ToItem(-1) != 1388 || LanternItemDropQuery.ToItem(0) != 136)
{
  throw new InvalidOperationException("Lantern low-style mapping diverged from legacy rules.");
}

for (int style = 1; style <= 6; style++)
{
  if (LanternItemDropQuery.ToItem(style) != 1389 + style)
  {
    throw new InvalidOperationException($"Lantern first range diverged for style {style}.");
  }
}

if (LanternItemDropQuery.ToItem(7) != 1431 || LanternItemDropQuery.ToItem(8) != 1808 ||
    LanternItemDropQuery.ToItem(9) != 1859)
{
  throw new InvalidOperationException("Lantern special style mappings diverged from legacy rules.");
}

for (int style = 10; style <= 21; style++)
{
  if (LanternItemDropQuery.ToItem(style) != 2032 + style - 10)
  {
    throw new InvalidOperationException($"Lantern second range diverged for style {style}.");
  }
}

for (int style = 22; style <= 25; style++)
{
  if (LanternItemDropQuery.ToItem(style) != 2145 + style - 22)
  {
    throw new InvalidOperationException($"Lantern third range diverged for style {style}.");
  }
}

for (int style = 26; style <= 70; style++)
{
  if (LanternItemDropQuery.ToItem(style) != lanternStyleItems[style - 26])
  {
    throw new InvalidOperationException($"Lantern item mapping diverged for style {style}.");
  }
}

if (LanternItemDropQuery.ToItem(71) != 136)
{
  throw new InvalidOperationException("Lantern out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: lantern item drop mapping preserves bounded legacy switch rules");

int[] lampStyleItems =
{
  2225, 2533, 2547, 2563, 2578, 2643, 2644, 2645, 2646, 2647,
  2819, 3135, 3137, 3136, 3892, 3942, 3969, 4156, 4177, 4198,
  4219, 4308, 4577, 5159, 5180, 5201, 5559, 5612, 5700, 5723,
  5748, 5767, 5788, 5809, 5830, 5849, 5869, 5889, 5909, 5943,
  5966, 5986, 6009, 6032, 6055, 6078, 6100, 6122
};
if (LampItemDropQuery.ToItem(-1) != 342 || LampItemDropQuery.ToItem(0) != 342)
{
  throw new InvalidOperationException("Lamp default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 10; style++)
{
  if (LampItemDropQuery.ToItem(style) != 2082 + style - 1)
  {
    throw new InvalidOperationException($"Lamp first range diverged for style {style}.");
  }
}

for (int style = 11; style <= 16; style++)
{
  if (LampItemDropQuery.ToItem(style) != 2129 + style - 11)
  {
    throw new InvalidOperationException($"Lamp second range diverged for style {style}.");
  }
}

for (int style = 17; style <= 64; style++)
{
  if (LampItemDropQuery.ToItem(style) != lampStyleItems[style - 17])
  {
    throw new InvalidOperationException($"Lamp item mapping diverged for style {style}.");
  }
}

if (LampItemDropQuery.ToItem(65) != 342)
{
  throw new InvalidOperationException("Lamp out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: lamp item drop mapping preserves bounded legacy switch rules");

int[] pianoStyleItems =
{
  2531, 2548, 2565, 2580, 2671, 2821, 3141, 3143, 3142, 3915,
  3916, 3944, 3971, 4158, 4179, 4200, 4221, 4310, 4579, 5161,
  5182, 5203, 5561, 5614, 5702, 5725, 5750, 5769, 5790, 5811,
  5832, 5851, 5871, 5891, 5911, 5945, 5968, 5988, 6011, 6034,
  6057, 6080, 6102, 6124
};
if (PianoItemDropQuery.ToItem(-1) != 333 || PianoItemDropQuery.ToItem(0) != 333)
{
  throw new InvalidOperationException("Piano default item mapping diverged from legacy rules.");
}

for (int style = 1; style <= 3; style++)
{
  if (PianoItemDropQuery.ToItem(style) != 640 + style)
  {
    throw new InvalidOperationException($"Piano first range diverged for style {style}.");
  }
}

if (PianoItemDropQuery.ToItem(4) != 919)
{
  throw new InvalidOperationException("Piano special style mapping diverged from legacy rules.");
}

for (int style = 5; style <= 7; style++)
{
  if (PianoItemDropQuery.ToItem(style) != 2245 + style - 5)
  {
    throw new InvalidOperationException($"Piano second range diverged for style {style}.");
  }
}

for (int style = 8; style <= 10; style++)
{
  if (PianoItemDropQuery.ToItem(style) != 2254 + style - 8)
  {
    throw new InvalidOperationException($"Piano third range diverged for style {style}.");
  }
}

for (int style = 11; style <= 20; style++)
{
  if (PianoItemDropQuery.ToItem(style) != 2376 + style - 11)
  {
    throw new InvalidOperationException($"Piano fourth range diverged for style {style}.");
  }
}

for (int style = 21; style <= 64; style++)
{
  if (PianoItemDropQuery.ToItem(style) != pianoStyleItems[style - 21])
  {
    throw new InvalidOperationException($"Piano item mapping diverged for style {style}.");
  }
}

if (PianoItemDropQuery.ToItem(65) != 333)
{
  throw new InvalidOperationException("Piano out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: piano item drop mapping preserves bounded legacy switch rules");

int[] sinkStyleItems =
{
  3147, 3149, 3148, 3896, 3946, 3972, 4160, 4181, 4202, 4223,
  4312, 4581, 5163, 5184, 5205, 5563, 5616, 5704, 5727, 5752,
  5771, 5792, 5813, 5834, 5853, 5873, 5892, 5913, 5947, 5969,
  5990, 6013, 6036, 6059, 6082, 6104, 6126
};
if (SinkItemDropQuery.ToItem(-1) != 2827)
{
  throw new InvalidOperationException("Sink negative-style mapping diverged from legacy rules.");
}

for (int style = 0; style <= 28; style++)
{
  if (SinkItemDropQuery.ToItem(style) != 2827 + style)
  {
    throw new InvalidOperationException($"Sink range diverged for style {style}.");
  }
}

for (int style = 29; style <= 65; style++)
{
  if (SinkItemDropQuery.ToItem(style) != sinkStyleItems[style - 29])
  {
    throw new InvalidOperationException($"Sink item mapping diverged for style {style}.");
  }
}

if (SinkItemDropQuery.ToItem(66) != 2827)
{
  throw new InvalidOperationException("Sink out-of-range item mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: sink item drop mapping preserves bounded legacy switch rules");

int[] secondTableItems =
{
  3920, 3948, 3974, 4162, 4183, 4204, 4225, 4314, 4583, 5165,
  5186, 5207, 5565, 5618, 5706, 5729, 5773, 5794, 5815, 5836,
  5875, 5894, 5915, 5949, 5971, 5992, 6015, 6038, 6061, 6084,
  6106, 6128
};
if (TableItemDropQuery.ToItem(-1, false) != 32 ||
    TableItemDropQuery.ToItem(0, true) != 3920)
{
  throw new InvalidOperationException("Table default mappings diverged from legacy rules.");
}

for (int style = 1; style <= 31; style++)
{
  if (TableItemDropQuery.ToItem(style, true) != secondTableItems[style])
  {
    throw new InvalidOperationException($"Second table mapping diverged for style {style}.");
  }
}

for (int style = 1; style <= 3; style++)
{
  if (TableItemDropQuery.ToItem(style, false) != 637 + style)
  {
    throw new InvalidOperationException($"Table first range diverged for style {style}.");
  }
}

for (int style = 4; style <= 7; style++)
{
  if (TableItemDropQuery.ToItem(style, false) != 823 + style)
  {
    throw new InvalidOperationException($"Table second range diverged for style {style}.");
  }
}

for (int style = 15; style <= 20; style++)
{
  if (TableItemDropQuery.ToItem(style, false) != 1698 + style)
  {
    throw new InvalidOperationException($"Table third range diverged for style {style}.");
  }
}

foreach ((int style, int expected) in new[]
{
  (8, 917), (9, 1144), (10, 1397), (11, 1400), (12, 1403), (13, 1460),
  (14, 1510), (21, 1794), (22, 1816), (23, 1926), (24, 2248), (25, 2259),
  (26, 2532), (27, 2550), (28, 677), (29, 2583), (30, 2743), (31, 2824),
  (32, 3153), (33, 3155), (34, 3154)
})
{
  if (TableItemDropQuery.ToItem(style, false) != expected)
  {
    throw new InvalidOperationException($"Table item mapping diverged for style {style}.");
  }
}

if (TableItemDropQuery.ToItem(35, false) != 32 ||
    TableItemDropQuery.ToItem(32, true) != 3920)
{
  throw new InvalidOperationException("Table out-of-range mappings diverged from legacy rules.");
}

Console.WriteLine("PASS: table item drop mapping preserves bounded legacy switch rules");

int[] bathtubItems =
{
  336, 2072, 2073, 2074, 2075, 2076, 2077, 2078, 2079, 2080,
  2081, 2124, 2125, 2126, 2127, 2128, 2232, 2519, 2537, 2552,
  2567, 2658, 2659, 2660, 2661, 2662, 2663, 2810, 3159, 3161,
  3160, 3895, 3931, 3958, 4145, 4166, 4187, 4208, 4298, 4566,
  5148, 5169, 5190, 5548, 5601, 5689, 5712, 5739, 5756, 5777,
  5798, 5819, 5840, 5858, 5879, 5898, 5932, 5955, 5975, 5998,
  6021, 6044, 6067, 6090, 6111
};
if (BathtubItemDropQuery.ToItem(-1) != 336 ||
    BathtubItemDropQuery.ToItem(65) != 336)
{
  throw new InvalidOperationException("Bathtub default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 64; style++)
{
  if (BathtubItemDropQuery.ToItem(style) != bathtubItems[style])
  {
    throw new InvalidOperationException($"Bathtub mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: bathtub item drop mapping preserves bounded legacy switch rules");

int[] workbenchItems =
{
  36, 635, 636, 637, 811, 812, 813, 814, 815, 916,
  1145, 1398, 1401, 1404, 1461, 1511, 1795, 1817, 2229, 2251,
  2252, 2253, 2534, 673, 2631, 2632, 2633, 2826, 3156, 3158,
  3157, 3909, 3910, 3949, 3975, 4163, 4184, 4205, 4226, 4315,
  4584, 5166, 5187, 5208, 5566, 5619, 5707, 5730, 5775, 5796,
  5817, 5838, 5856, 5877, 5896, 5917, 5951, 5973, 5994, 6017,
  6040, 6063, 6086, 6108, 6130
};
if (WorkbenchItemDropQuery.ToItem(-1) != 36 ||
    WorkbenchItemDropQuery.ToItem(65) != 36)
{
  throw new InvalidOperationException("Workbench default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 64; style++)
{
  if (WorkbenchItemDropQuery.ToItem(style) != workbenchItems[style])
  {
    throw new InvalidOperationException($"Workbench mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: workbench item drop mapping preserves bounded legacy switch rules");

int[] chairItems =
{
  34, 358, 628, 629, 630, 806, 807, 808, 809, 810,
  826, 915, 1143, 1396, 1399, 1402, 1459, 1509, 1703, 1704,
  1705, 1706, 1707, 1708, 1792, 1814, 1925, 2228, 2288, 2524,
  2557, 2572, 2812, 3174, 3176, 3175, 3889, 3937, 3963, 4151,
  4172, 4193, 4214, 4304, 4572, 5154, 5175, 5196, 5554, 5607,
  5695, 5718, 5761, 5782, 5803, 5824, 5863, 5884, 5903, 5937,
  5960, 5980, 6003, 6026, 6049, 6072, 6095, 6116
};
if (ChairItemDropQuery.ToItem(-1) != 34 ||
    ChairItemDropQuery.ToItem(68) != 34)
{
  throw new InvalidOperationException("Chair default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 67; style++)
{
  if (ChairItemDropQuery.ToItem(style) != chairItems[style])
  {
    throw new InvalidOperationException($"Chair mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: chair item drop mapping preserves bounded legacy switch rules");

if (ToiletItemDropQuery.ToItem(-1) != 4096 ||
    ToiletItemDropQuery.ToItem(65) != 4096)
{
  throw new InvalidOperationException("Toilet default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 31; style++)
{
  if (ToiletItemDropQuery.ToItem(style) != 4096 + style)
  {
    throw new InvalidOperationException($"Toilet range mapping diverged for style {style}.");
  }
}

foreach ((int style, int expected) in new[]
{
  (32, 4141), (33, 4165), (34, 4186), (35, 4207), (36, 4228), (37, 4316),
  (38, 4586), (39, 4731), (40, 5168), (41, 5189), (42, 5210), (43, 5568),
  (44, 5621), (45, 5709), (46, 5732), (47, 5755), (48, 5774), (49, 5795),
  (50, 5816), (51, 5837), (52, 5855), (53, 5876), (54, 5895), (55, 5916),
  (56, 5950), (57, 5972), (58, 5993), (59, 6016), (60, 6039), (61, 6062),
  (62, 6085), (63, 6107), (64, 6129)
})
{
  if (ToiletItemDropQuery.ToItem(style) != expected)
  {
    throw new InvalidOperationException($"Toilet mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: toilet item drop mapping preserves bounded legacy switch rules");

int[] platformItems =
{
  94, 631, 632, 633, 634, 913, 1384, 1385, 1386, 1387,
  1388, 1389, 1418, 1457, 1702, 1796, 1818, 2518, 2549, 2566,
  2581, 2627, 2628, 2629, 2630, 2744, 2822, 3144, 3146, 3145,
  3903, 3904, 3905, 3906, 3907, 3908, 3945, 3957, 4159, 4180,
  4201, 4222, 4311, 4416, 4580, 5162, 5183, 5204, 5292, 5544,
  5562, 5615, 5703, 5726, 5751, 5770, 5791, 5812, 5833, 5852,
  5872, 5912, 5946, 5989, 6012, 6035, 6058, 6081, 6103, 6125
};
if (PlatformItemDropQuery.ToItem(-1) != 94 ||
    PlatformItemDropQuery.ToItem(70) != 94)
{
  throw new InvalidOperationException("Platform default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 69; style++)
{
  if (PlatformItemDropQuery.ToItem(style) != platformItems[style])
  {
    throw new InvalidOperationException($"Platform mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: platform item drop mapping preserves bounded legacy switch rules");

if (MusicBoxItemDropQuery.ToItem(-100) != 462 ||
    MusicBoxItemDropQuery.ToItem(-1) != 561 ||
    MusicBoxItemDropQuery.ToItem(101) != 576)
{
  throw new InvalidOperationException("Music box default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 12; style++)
{
  if (MusicBoxItemDropQuery.ToItem(style) != 562 + style)
  {
    throw new InvalidOperationException($"Music box first range diverged for style {style}.");
  }
}

for (int style = 13; style <= 27; style++)
{
  if (MusicBoxItemDropQuery.ToItem(style) != 1583 + style)
  {
    throw new InvalidOperationException($"Music box second range diverged for style {style}.");
  }
}

foreach ((int style, int expected) in new[]
{
  (28, 1963), (29, 1964), (30, 1965), (31, 2742), (32, 3044), (33, 3235),
  (34, 3236), (35, 3237), (36, 3370), (37, 3371), (38, 3796), (39, 3869),
  (40, 4082), (41, 4078), (42, 4079), (43, 4077), (44, 4080), (45, 4081),
  (46, 4237), (47, 4356), (48, 4357), (49, 4358), (50, 4421), (51, 4606),
  (52, 4979), (53, 4985), (54, 4990), (55, 4991), (56, 4992), (57, 5006),
  (58, 5014), (59, 5015), (60, 5016), (61, 5017), (62, 5018), (63, 5019),
  (64, 5020), (65, 5021), (66, 5022), (67, 5023), (68, 5024), (69, 5025),
  (70, 5026), (71, 5027), (72, 5028), (73, 5029), (74, 5030), (75, 5031),
  (76, 5032), (77, 5033), (78, 5034), (79, 5035), (80, 5036), (81, 5037),
  (82, 5038), (83, 5039), (84, 5040), (85, 5044), (86, 5112), (87, 5362),
  (88, 5578), (89, 5538), (90, 5579), (91, 5580), (92, 5539), (93, 5581),
  (94, 5582), (95, 5637), (96, 5638), (97, 5639), (98, 6144), (99, 6145),
  (100, 6146)
})
{
  if (MusicBoxItemDropQuery.ToItem(style) != expected)
  {
    throw new InvalidOperationException($"Music box mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: music box item drop mapping preserves bounded legacy switch rules");

int[] dresserItems =
{
  334, 647, 648, 649, 918, 2386, 2387, 2388, 2389, 2390,
  2391, 2392, 2393, 2394, 2395, 2396, 2529, 2545, 2562, 2577,
  2637, 2638, 2639, 2640, 2816, 3132, 3134, 3133, 3911, 3912,
  3913, 3914, 3934, 3968, 4148, 4169, 4190, 4211, 4301, 4569,
  5151, 5172, 5193, 5551, 5604, 5692, 5715, 5741, 5766, 5787,
  5808, 5829, 5848, 5868, 5888, 5908, 5942, 5965, 5985, 6008,
  6031, 6054, 6077, 6099, 6121
};
if (DresserItemDropQuery.ToItem(-1) != 334 ||
    DresserItemDropQuery.ToItem(65) != 334)
{
  throw new InvalidOperationException("Dresser default mappings diverged from legacy rules.");
}

for (int style = 0; style <= 64; style++)
{
  if (DresserItemDropQuery.ToItem(style) != dresserItems[style])
  {
    throw new InvalidOperationException($"Dresser mapping diverged for style {style}.");
  }
}

Console.WriteLine("PASS: dresser item drop mapping preserves bounded legacy switch rules");

int[] secondChestItems =
{
  3884, 3885, 3939, 3965, 3988, 4153, 4174, 4195, 4216, 4265,
  4267, 4574, 4712, 4712, 5156, 5177, 5198, 5556, 5609, 5697,
  5720, 5745, 5763, 5784, 5805, 5826, 5846, 5865, 5886, 5905,
  5939, 5962, 5982, 6005, 6028, 6051, 6074, 6118
};
int[] firstChestItems =
{
  48, 306, 306, 328, 328, 343, 348, 625, 626, 627,
  680, 681, 831, 838, 914, 952, 1142, 1298, 1528, 1529,
  1530, 1531, 1532, 1528, 1529, 1530, 1531, 1532, 2230, 2249,
  2250, 2526, 2544, 2559, 2574, 2612, 2612, 2613, 2613, 2614,
  2614, 2615, 2616, 2617, 2618, 2619, 2620, 2748, 2814, 3180,
  3125, 3181
};
for (int style = 0; style <= 37; style++)
{
  if (ChestItemDropQuery.ToItem(style, true) != secondChestItems[style])
  {
    throw new InvalidOperationException($"Second chest mapping diverged for style {style}.");
  }
}

for (int style = 0; style <= 51; style++)
{
  if (ChestItemDropQuery.ToItem(style, false) != firstChestItems[style])
  {
    throw new InvalidOperationException($"First chest mapping diverged for style {style}.");
  }
}

if (ChestItemDropQuery.ToItem(-1, true) != 3884 ||
    ChestItemDropQuery.ToItem(38, true) != 3884 ||
    ChestItemDropQuery.ToItem(-1, false) != 48 ||
    ChestItemDropQuery.ToItem(52, false) != 48)
{
  throw new InvalidOperationException("Chest default mappings diverged from legacy rules.");
}

Console.WriteLine("PASS: chest item drop mapping preserves dual legacy style tables");

foreach ((int style, int expected) in new[]
{
  (0, 3886), (1, 3887), (2, 3950), (3, 3976), (4, -1), (5, 4164),
  (6, 4185), (7, 4206), (8, 4227), (9, 4266), (10, 4268), (11, 4585),
  (12, 4713), (13, -1), (14, 5167), (15, 5188), (16, 5209), (17, 5567),
  (18, 5620), (19, 5708), (20, 5731), (21, 5754), (22, 5776), (23, 5797),
  (24, 5818), (25, 5839), (26, 5857), (27, 5878), (28, 5897), (29, 5918),
  (30, 5952), (31, 5974), (32, 5995), (33, 6018), (34, 6041), (35, 6064),
  (36, 6087), (37, 6131)
})
{
  if (FakeChestItemDropQuery.ToItem(style, true) != expected)
  {
    throw new InvalidOperationException($"Second fake chest mapping diverged for style {style}.");
  }
}

foreach ((int style, int expected) in new[]
{
  (0, 3665), (1, 3666), (2, 3665), (3, 3667), (4, 3665), (5, 3665),
  (6, 3665), (7, 3668), (8, 3669), (9, 3670), (10, 3671), (11, 3672),
  (12, 3673), (13, 3674), (14, 3675), (15, 3676), (16, 3677), (17, 3678),
  (18, 3679), (19, 3680), (20, 3681), (21, 3682), (22, 3683), (23, 3665),
  (24, 3665), (25, 3665), (26, 3665), (27, 3665), (28, 3684), (29, 3685),
  (30, 3686), (31, 3687), (32, 3688), (33, 3689), (34, 3690), (35, 3691),
  (36, 3665), (37, 3692), (38, 3665), (39, 3693), (40, 3665), (41, 3694),
  (42, 3695), (43, 3696), (44, 3697), (45, 3698), (46, 3699), (47, 3700),
  (48, 3701), (49, 3702), (50, 3703), (51, 3704)
})
{
  if (FakeChestItemDropQuery.ToItem(style, false) != expected)
  {
    throw new InvalidOperationException($"First fake chest mapping diverged for style {style}.");
  }
}

if (FakeChestItemDropQuery.ToItem(-1, true) != 3886 ||
    FakeChestItemDropQuery.ToItem(38, true) != 3886 ||
    FakeChestItemDropQuery.ToItem(-1, false) != 3665 ||
    FakeChestItemDropQuery.ToItem(52, false) != 3665)
{
  throw new InvalidOperationException("Fake chest default mappings diverged from legacy rules.");
}

Console.WriteLine("PASS: fake chest item drop mapping preserves dual legacy style tables");

int[] campfireItems =
{
  966, 3046, 3047, 3048, 3049, 3050, 3723, 3724,
  4689, 4690, 4691, 4692, 4693, 4694, 5299, 5357
};
for (int style = 0; style <= 15; style++)
{
  if (CampfireItemDropQuery.ToItem(style) != campfireItems[style])
  {
    throw new InvalidOperationException($"Campfire mapping diverged for style {style}.");
  }
}

if (CampfireItemDropQuery.ToItem(-1) != 966 ||
    CampfireItemDropQuery.ToItem(16) != 966)
{
  throw new InvalidOperationException("Campfire default mappings diverged from legacy rules.");
}

Console.WriteLine("PASS: campfire item drop mapping preserves bounded legacy switch rules");

if (RainbowPaintQuery.ToPaintId(0, 0, false) != 13 ||
    RainbowPaintQuery.ToPaintId(43, 43, false) != 22 ||
    RainbowPaintQuery.ToPaintId(44, 0, false) != 13 ||
    RainbowPaintQuery.ToPaintId(10, 10, false) != 22)
{
  throw new InvalidOperationException("Rainbow paint mapping diverged for direct coordinates.");
}

if (RainbowPaintQuery.ToPaintId(0, 0, true) != 8 ||
    RainbowPaintQuery.ToPaintId(25, 25, true) != 22 ||
    RainbowPaintQuery.ToPaintId(49, 0, true) != 13)
{
  throw new InvalidOperationException("Rainbow paint mapping diverged for wiggly coordinates.");
}

if (RainbowPaintQuery.ToPaintId(-1, 0, false) != 12 ||
    RainbowPaintQuery.ToPaintId(-50, -10, true) != 4)
{
  throw new InvalidOperationException("Rainbow paint mapping normalized legacy negative coordinates.");
}

Console.WriteLine("PASS: rainbow paint mapping preserves direct, wiggly, and negative coordinates");

if (!DungeonChestQuery.IsLockedBiomeChest(21, 23) ||
    !DungeonChestQuery.IsLockedBiomeChest(21, 27) ||
    !DungeonChestQuery.IsLockedBiomeChest(467, 13) ||
    DungeonChestQuery.IsLockedBiomeChest(21, 22) ||
    DungeonChestQuery.IsLockedBiomeChest(21, 28) ||
    DungeonChestQuery.IsLockedBiomeChest(467, 12) ||
    DungeonChestQuery.IsLockedBiomeChest(467, 14) ||
    DungeonChestQuery.IsLockedBiomeChest(0, 23))
{
  throw new InvalidOperationException(
    "Locked dungeon biome chest mapping diverged from legacy rules.");
}

Console.WriteLine("PASS: locked dungeon biome chest mapping preserves bounded legacy styles");

if (PileGenerationAttemptPolicy.GetAttempts(4200, false) != 2100 ||
    PileGenerationAttemptPolicy.GetAttempts(4200, true) != 210 ||
    PileGenerationAttemptPolicy.GetAttempts(101, false) != 50 ||
    PileGenerationAttemptPolicy.GetAttempts(101, true) != 5)
{
  throw new InvalidOperationException(
    "Pile generation attempt policy diverged from legacy division rules.");
}

Console.WriteLine("PASS: pile generation attempt policy preserves explicit world-rule inputs");

if (!PlantTypeConversionQuery.IsBadTypeMatch(23, 3))
{
  throw new InvalidOperationException("Plant grass mismatch was accepted.");
}

if (PlantTypeConversionQuery.IsBadTypeMatch(2, 3) ||
    PlantTypeConversionQuery.IsBadTypeMatch(477, 73) ||
    PlantTypeConversionQuery.IsBadTypeMatch(23, 24) ||
    PlantTypeConversionQuery.IsBadTypeMatch(60, 61) ||
    PlantTypeConversionQuery.IsBadTypeMatch(70, 71) ||
    PlantTypeConversionQuery.IsBadTypeMatch(109, 110) ||
    PlantTypeConversionQuery.IsBadTypeMatch(199, 201) ||
    PlantTypeConversionQuery.IsBadTypeMatch(633, 637))
{
  throw new InvalidOperationException("Plant compatible support was rejected.");
}

if (!PlantTypeConversionQuery.IsBadTypeMatch(2, 637))
{
  throw new InvalidOperationException("Plant ash mismatch was accepted for non-ash grass.");
}

PlantTypeConversionResult grassConversion = PlantTypeConversionQuery.Evaluate(113, 144, 2);
if (grassConversion.TileType != 73 || grassConversion.FrameX != 144 ||
    grassConversion.IsMushroom)
{
  throw new InvalidOperationException("Plant grass conversion diverged from legacy rules.");
}

PlantTypeConversionResult corruptConversion = PlantTypeConversionQuery.Evaluate(3, 180, 23);
if (corruptConversion.TileType != 24 || corruptConversion.FrameX != 126 ||
    corruptConversion.IsMushroom)
{
  throw new InvalidOperationException("Plant corrupt-grass conversion diverged from legacy rules.");
}

PlantTypeConversionResult mushroomConversion = PlantTypeConversionQuery.Evaluate(3, 144, 60);
if (mushroomConversion.TileType != 61 || mushroomConversion.FrameX != 18 ||
    !mushroomConversion.IsMushroom)
{
  throw new InvalidOperationException("Plant mushroom conversion diverged from legacy rules.");
}

PlantTypeConversionResult crimsonMushroom = PlantTypeConversionQuery.Evaluate(201, 270, 199);
if (crimsonMushroom.TileType != 201 || crimsonMushroom.FrameX != 270 ||
    !crimsonMushroom.IsMushroom)
{
  throw new InvalidOperationException(
    "Plant crimson mushroom handling diverged from legacy rules.");
}

Console.WriteLine("PASS: plant type conversion preserves bounded legacy compatibility and frames");

WorldGrid plantWorld = new(replayWidth, replayHeight);
WorldMetadata plantMetadata = new(
  "worldgen-plant-check-query",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
_ = plantWorld.TrySetTile(
  40,
  40,
  new WorldTile(
    IsActive: true,
    Type: 3,
    LiquidAmount: 90,
    LiquidType: 1,
    FrameX: 144,
    FrameY: 36,
    WallType: 6,
    HasWire: true));
_ = plantWorld.TrySetTile(40, 41, new WorldTile(IsActive: true, Type: 23));
WorldGridSnapshot corruptPlantSnapshot = plantWorld.CreateSnapshot(plantMetadata);
if (!PlantPlacementQuery.CanPlace(
      corruptPlantSnapshot,
      tileDefinitions,
      40,
      40,
      24) ||
    PlantPlacementQuery.CanPlace(
      corruptPlantSnapshot,
      tileDefinitions,
      40,
      40,
      3))
{
  throw new InvalidOperationException(
    "Plant placement support classification diverged from legacy rules.");
}

PlantCheckResult corruptPlantCheck = PlantCheckQuery.Evaluate(
  corruptPlantSnapshot,
  tileDefinitions,
  40,
  40);
if (corruptPlantCheck.ShouldDestroy || !corruptPlantCheck.ShouldConvert ||
    corruptPlantCheck.Conversion.TileType != 24 ||
    corruptPlantCheck.Conversion.FrameX != 144 || !corruptPlantCheck.Conversion.IsMushroom)
{
  throw new InvalidOperationException("Plant check conversion intent diverged from legacy rules.");
}

if (!PlantCheckCommandSystem.TryCreateCommand(
      corruptPlantSnapshot,
      tileDefinitions,
      40,
      40,
      18,
      out TileChangeCommand plantConversionCommand) ||
    plantConversionCommand.Kind != TileChangeKind.UpdateTileType ||
    plantConversionCommand.TileType != 24 || plantConversionCommand.FrameX != 144 ||
    plantConversionCommand.FrameY != 36)
{
  throw new InvalidOperationException(
    "Plant check conversion command did not preserve legacy type and frame semantics.");
}

WorldGrid convertedPlantWorld = WorldGrid.FromSnapshot(corruptPlantSnapshot);
if (!new TileChangeCommitSystem().TryCommit(
      convertedPlantWorld,
      new[] { plantConversionCommand },
      out TileChangeCommitResult plantConversionCommit) ||
    plantConversionCommit.AppliedCount != 1)
{
  throw new InvalidOperationException("Plant conversion command did not commit.");
}

WorldTile convertedPlantTile = convertedPlantWorld.GetTile(40, 40);
if (convertedPlantTile.Type != 24 || convertedPlantTile.FrameX != 144 ||
    convertedPlantTile.FrameY != 36 || convertedPlantTile.LiquidAmount != 90 ||
    convertedPlantTile.LiquidType != 1 || convertedPlantTile.WallType != 6 ||
    !convertedPlantTile.HasWire)
{
  throw new InvalidOperationException(
    "Plant conversion command did not preserve unrelated tile state.");
}

_ = plantWorld.TrySetTile(40, 41, new WorldTile(IsActive: true, Type: 1));
WorldGridSnapshot unsupportedPlantSnapshot = plantWorld.CreateSnapshot(plantMetadata);
if (!PlantCheckQuery.Evaluate(unsupportedPlantSnapshot, tileDefinitions, 40, 40).ShouldDestroy)
{
  throw new InvalidOperationException("Unsupported plants were not marked for destruction.");
}

if (!PlantCheckCommandSystem.TryCreateCommand(
      unsupportedPlantSnapshot,
      tileDefinitions,
      40,
      40,
      19,
      out TileChangeCommand plantDestroyCommand) ||
    plantDestroyCommand.Kind != TileChangeKind.Kill || plantDestroyCommand.TileType != 0)
{
  throw new InvalidOperationException("Plant check destruction did not produce a kill command.");
}

_ = plantWorld.TrySetTile(40, 40, new WorldTile(IsActive: true, Type: 703));
_ = plantWorld.TrySetTile(40, 41, new WorldTile(IsActive: true, Type: 1, Slope: 3));
WorldGridSnapshot slopedSaplingSnapshot = plantWorld.CreateSnapshot(plantMetadata);
if (!PlantPlacementQuery.CanPlace(
      slopedSaplingSnapshot,
      tileDefinitions,
      40,
      40,
      703) ||
    PlantCheckQuery.Evaluate(slopedSaplingSnapshot, tileDefinitions, 40, 40).ShouldDestroy)
{
  throw new InvalidOperationException("Plant 703 bottom-slope support diverged from legacy rules.");
}

Console.WriteLine("PASS: plant placement and check queries preserve bounded support decisions");

WorldGrid foodPlatterWorld = new(replayWidth, replayHeight);
_ = foodPlatterWorld.TrySetTile(60, 60, new WorldTile(IsActive: true, Type: 520));
_ = foodPlatterWorld.TrySetTile(60, 61, new WorldTile(IsActive: true, Type: 1));
FoodPlatterSnapshot foodPlatter = new(
  EntityId: 700,
  TileX: 60,
  TileY: 60,
  Exists: true,
  StoredItem: new ItemStack(123, 2));
WorldGridSnapshot supportedFoodPlatterSnapshot = foodPlatterWorld.CreateSnapshot(plantMetadata);
if (FoodPlatterDestructionQuery.Evaluate(
      supportedFoodPlatterSnapshot,
      tileDefinitions,
      foodPlatter).ShouldDestroy)
{
  throw new InvalidOperationException("Supported Food Platter was incorrectly marked for destruction.");
}

_ = foodPlatterWorld.TrySetTile(60, 61, default);
WorldGridSnapshot unsupportedFoodPlatterSnapshot = foodPlatterWorld.CreateSnapshot(plantMetadata);
if (!FoodPlatterDestructionCommandSystem.TryCreateBatch(
      unsupportedFoodPlatterSnapshot,
      tileDefinitions,
      foodPlatter,
      sequence: 94,
      out FoodPlatterDestructionBatch foodPlatterBatch) ||
    !foodPlatterBatch.RemoveTileEntity || foodPlatterBatch.DroppedItem != new ItemStack(123, 2))
{
  throw new InvalidOperationException("Food Platter destruction batch did not preserve entity and drop intent.");
}

Dictionary<int, TileEntityPersistentState> foodPlatterEntities = new()
{
  [foodPlatter.EntityId] = new TileEntityPersistentState(
    id: foodPlatter.EntityId,
    type: 7,
    tileX: foodPlatter.TileX,
    tileY: foodPlatter.TileY,
    payload: Array.Empty<byte>(),
    isOpaque: true)
};
List<ItemStack> foodPlatterDrops = new();
if (!new FoodPlatterDestructionCommitSystem().TryCommit(
      foodPlatterWorld,
      foodPlatterEntities,
      foodPlatterBatch,
      foodPlatterDrops,
      out FoodPlatterDestructionCommitResult foodPlatterCommit) ||
    !foodPlatterCommit.TileEntityRemoved || !foodPlatterCommit.ItemDropped ||
    foodPlatterWorld.GetTile(60, 60).IsActive || foodPlatterEntities.Count != 0 ||
    !foodPlatterDrops.SequenceEqual(new[] { new ItemStack(123, 2) }))
{
  throw new InvalidOperationException("Food Platter destruction did not commit atomically.");
}

WorldGrid vineWorld = new(replayWidth, replayHeight);
_ = vineWorld.TrySetTile(40, 40, new WorldTile(IsActive: true, Type: 52, FrameX: 18));
_ = vineWorld.TrySetTile(40, 39, new WorldTile(IsActive: true, Type: 60));
WorldGridSnapshot vineSnapshot = vineWorld.CreateSnapshot(plantMetadata);
VineFrameResult vineConversion = VineFrameQuery.Evaluate(vineSnapshot, 40, 40);
if (vineConversion.ShouldKeep || vineConversion.ShouldKill ||
    vineConversion.ReplacementTileType != 62 ||
    !VineFrameCommandSystem.TryCreateCommand(
      vineSnapshot,
      40,
      40,
      sequence: 90,
      out TileChangeCommand vineCommand) ||
    vineCommand.Kind != TileChangeKind.UpdateTileType || vineCommand.TileType != 62 ||
    vineCommand.FrameX != 18)
{
  throw new InvalidOperationException("Vine conversion did not preserve the legacy mutation intent.");
}

_ = vineWorld.TrySetTile(40, 39, new WorldTile(IsActive: true, Type: 2));
VineFrameResult supportedVine = VineFrameQuery.Evaluate(
  vineWorld.CreateSnapshot(plantMetadata),
  40,
  40);
if (!supportedVine.ShouldKeep || supportedVine.ShouldKill ||
    supportedVine.ReplacementTileType is not null)
{
  throw new InvalidOperationException("Supported vines were not retained.");
}

_ = vineWorld.TrySetTile(40, 39, new WorldTile(IsActive: true, Type: 2, Slope: 3));
WorldGridSnapshot unsupportedVineSnapshot = vineWorld.CreateSnapshot(plantMetadata);
VineFrameResult unsupportedVine = VineFrameQuery.Evaluate(unsupportedVineSnapshot, 40, 40);
if (!unsupportedVine.ShouldKill ||
    !VineFrameCommandSystem.TryCreateCommand(
      unsupportedVineSnapshot,
      40,
      40,
      sequence: 91,
      out TileChangeCommand killVineCommand) ||
    killVineCommand.Kind != TileChangeKind.Kill)
{
  throw new InvalidOperationException("Unsupported vines were not converted to kill commands.");
}

WorldGrid cactusWorld = new(replayWidth, replayHeight);
_ = cactusWorld.TrySetTile(50, 50, new WorldTile(IsActive: true, Type: 80));
_ = cactusWorld.TrySetTile(50, 51, new WorldTile(IsActive: true, Type: 53));
WorldGridSnapshot cactusSnapshot = cactusWorld.CreateSnapshot(plantMetadata);
CactusFrameResult groundedCactus = CactusFrameQuery.Evaluate(cactusSnapshot, 50, 50);
if (groundedCactus.ShouldKill || groundedCactus.SupportX != 50 ||
    groundedCactus.SupportY != 51 ||
    CactusFrameCommandSystem.TryCreateCommand(
      cactusSnapshot,
      50,
      50,
      sequence: 92,
      out TileChangeCommand groundedCactusCommand) ||
    groundedCactusCommand != default)
{
  throw new InvalidOperationException("Grounded cactus was not preserved without a command.");
}

_ = cactusWorld.TrySetTile(50, 51, new WorldTile(IsActive: true, Type: 1));
WorldGridSnapshot unsupportedCactusSnapshot = cactusWorld.CreateSnapshot(plantMetadata);
CactusFrameResult unsupportedCactus = CactusFrameQuery.Evaluate(
  unsupportedCactusSnapshot,
  50,
  50);
if (!unsupportedCactus.ShouldKill ||
    !CactusFrameCommandSystem.TryCreateCommand(
      unsupportedCactusSnapshot,
      50,
      50,
      sequence: 93,
      out TileChangeCommand killCactusCommand) ||
    killCactusCommand.Kind != TileChangeKind.Kill)
{
  throw new InvalidOperationException("Unsupported cactus was not converted to a kill command.");
}

if (!TileSlopingQuery.ForbidsSloping(21) || !TileSlopingQuery.ForbidsSloping(597) ||
    TileSlopingQuery.ForbidsSloping(1))
{
  throw new InvalidOperationException("Tile sloping protection table diverged from legacy rules.");
}

WorldGrid poundingWorld = new(replayWidth, replayHeight);
WorldMetadata poundingMetadata = new(
  "worldgen-tile-pounding-query",
  new WorldSeed(1456),
  replayWidth,
  replayHeight);
_ = poundingWorld.TrySetTile(50, 50, new WorldTile(IsActive: true, Type: 1));
WorldGridSnapshot poundableSnapshot = poundingWorld.CreateSnapshot(poundingMetadata);
HashSet<ushort> boulderTileTypes = [138, 484, 664, 665, 711, 712, 713, 714, 715, 716];
if (!TilePoundingEligibilityQuery.CanPound(
      poundableSnapshot,
      50,
      50,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => true) ||
    TilePoundingEligibilityQuery.CanPound(
      poundableSnapshot,
      50,
      50,
      boulderTileTypes,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => false))
{
  throw new InvalidOperationException(
    "Tile pounding did not preserve the final CanKillTile guard.");
}

_ = poundingWorld.TrySetTile(50, 50, new WorldTile(IsActive: true, Type: 138));
WorldGridSnapshot boulderPoundingSnapshot = poundingWorld.CreateSnapshot(poundingMetadata);
if (TilePoundingEligibilityQuery.CanPound(
      boulderPoundingSnapshot,
      50,
      50,
      boulderTileTypes,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => true))
{
  throw new InvalidOperationException("Boulder tiles were accepted for pounding.");
}

_ = poundingWorld.TrySetTile(50, 50, new WorldTile(IsActive: true, Type: 190));
WorldGridSnapshot generatingPoundingSnapshot = poundingWorld.CreateSnapshot(poundingMetadata);
if (TilePoundingEligibilityQuery.CanPound(
      generatingPoundingSnapshot,
      50,
      50,
      boulderTileTypes,
      isGeneratingOrLoadingWorld: true,
      canKillTile: static (_, _) => true))
{
  throw new InvalidOperationException("Generating-world pounding exception was not preserved.");
}

_ = poundingWorld.TrySetTile(50, 50, new WorldTile(IsActive: true, Type: 1));
_ = poundingWorld.TrySetTile(50, 49, new WorldTile(IsActive: true, Type: 21));
WorldGridSnapshot protectedAbovePoundingSnapshot = poundingWorld.CreateSnapshot(poundingMetadata);
if (TilePoundingEligibilityQuery.CanPound(
      protectedAbovePoundingSnapshot,
      50,
      50,
      boulderTileTypes,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => true))
{
  throw new InvalidOperationException("Protected above-tile relation was accepted for pounding.");
}

Console.WriteLine("PASS: tile pounding queries preserve bounded legacy eligibility");

_ = poundingWorld.TrySetTile(
  50,
  50,
  new WorldTile(
    IsActive: true,
    Type: 1,
    LiquidAmount: 120,
    LiquidType: 2,
    FrameX: 36,
    FrameY: 54,
    WallType: 7,
    HasWire: true,
    IsHalfBrick: true));
_ = poundingWorld.TrySetTile(50, 49, default);
WorldGridSnapshot shapedPoundingSnapshot = poundingWorld.CreateSnapshot(poundingMetadata);
if (!TilePoundingCommandSystem.TryCreateSlopeCommand(
      shapedPoundingSnapshot,
      50,
      50,
      slope: 3,
      sequence: 20,
      boulderTileTypes: boulderTileTypes,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => true,
      out TileChangeCommand slopeCommand) ||
    slopeCommand.Kind != TileChangeKind.UpdateTileShape || slopeCommand.IsHalfBrick != false ||
    slopeCommand.Slope != 3)
{
  throw new InvalidOperationException(
    "Tile slope command did not preserve the legacy shape intent.");
}

WorldGrid shapedPoundingWorld = WorldGrid.FromSnapshot(shapedPoundingSnapshot);
if (!new TileChangeCommitSystem().TryCommit(
      shapedPoundingWorld,
      new[] { slopeCommand },
      out TileChangeCommitResult slopeCommit) || slopeCommit.AppliedCount != 1)
{
  throw new InvalidOperationException("Tile slope command did not commit.");
}

WorldTile slopedTile = shapedPoundingWorld.GetTile(50, 50);
if (slopedTile.IsHalfBrick || slopedTile.Slope != 3 || slopedTile.Type != 1 ||
    slopedTile.LiquidAmount != 120 || slopedTile.LiquidType != 2 || slopedTile.FrameX != 36 ||
    slopedTile.FrameY != 54 || slopedTile.WallType != 7 || !slopedTile.HasWire)
{
  throw new InvalidOperationException("Tile slope command did not preserve unrelated tile state.");
}

WorldGridSnapshot poundableShapeSnapshot = shapedPoundingWorld.CreateSnapshot(poundingMetadata);
if (!TilePoundingCommandSystem.TryCreatePoundCommand(
      poundableShapeSnapshot,
      50,
      50,
      sequence: 21,
      boulderTileTypes: boulderTileTypes,
      isGeneratingOrLoadingWorld: false,
      canKillTile: static (_, _) => true,
      out TileChangeCommand poundCommand) || poundCommand.IsHalfBrick != true ||
    poundCommand.Slope is not null)
{
  throw new InvalidOperationException("Tile pound command did not toggle half-brick state.");
}

Console.WriteLine("PASS: tile slope and pound commands preserve bounded shape updates");

TileMergeNeighbors mergeNeighbors = new(
  Up: 2,
  Down: 8,
  Left: 2,
  Right: 7,
  UpLeft: 2,
  UpRight: 9,
  DownLeft: 2,
  DownRight: 10);
TileMergeFrametestResult frametest = TileMergeQuery.ApplyFrametest(
  tileType: 1,
  lookForTileType: 2,
  neighbors: mergeNeighbors,
  mergeUp: false,
  mergeDown: true,
  mergeLeft: false,
  mergeRight: false);
if (frametest.Neighbors.Up != 1 || frametest.Neighbors.Down != 8 ||
    frametest.Neighbors.Left != 2 || frametest.Neighbors.UpLeft != 1 ||
    frametest.Neighbors.DownLeft != 1 || !frametest.FrameUp || frametest.FrameDown ||
    !frametest.FrameLeft || frametest.FrameRight)
{
  throw new InvalidOperationException("Tile merge frametest semantics diverged from legacy rules.");
}

WorldGrid mergeFrameWorld = new(replayWidth, replayHeight);
_ = mergeFrameWorld.TrySetTile(10, 9, new WorldTile(IsActive: true, Type: 1));
IReadOnlyList<TileFrameRequest> mergeFrameRequests = frametest.CreateFrameRequests(
  10,
  10,
  mergeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  Array.Empty<TileChangeCommand>());
if (mergeFrameRequests.Count != 2 || mergeFrameRequests[0].X != 10 ||
    mergeFrameRequests[0].Y != 9 ||
    mergeFrameRequests[0].MutationKind != TileFrameMutationKind.TileMergeFrametest ||
    mergeFrameRequests[1].X != 9 || mergeFrameRequests[1].Y != 10 ||
    mergeFrameRequests[1].MutationKind != TileFrameMutationKind.TileMergeFrametest)
{
  throw new InvalidOperationException("Tile merge frametest did not create the requested frame work.");
}

HashSet<int> mergeLookForTileTypes = [2, 7];
TileMergeNeighbors cardinalMerge = TileMergeQuery.ReplaceCardinal(
  1,
  mergeLookForTileTypes,
  mergeNeighbors);
if (cardinalMerge.Up != 1 || cardinalMerge.Down != 8 || cardinalMerge.Left != 1 ||
    cardinalMerge.Right != 1 || cardinalMerge.UpLeft != 2 || cardinalMerge.DownLeft != 2)
{
  throw new InvalidOperationException("Cardinal tile merge altered the wrong neighbor set.");
}

TileMergeNeighbors allMerge = TileMergeQuery.ReplaceAllExcept(
  1,
  mergeLookForTileTypes,
  excludedTileType: 7,
  neighbors: mergeNeighbors);
if (allMerge.Up != 1 || allMerge.Left != 1 || allMerge.UpLeft != 1 ||
    allMerge.DownLeft != 1 || allMerge.Right != 7)
{
  throw new InvalidOperationException("Excluded tile merge semantics diverged from legacy rules.");
}

HashSet<int> excludedTileTypes = [2, 10];
TileMergeNeighbors weirdMerge = TileMergeQuery.ReplaceDifferentExcept(
  tileType: 1,
  replacementTileType: 99,
  excludedTileTypes: excludedTileTypes,
  neighbors: mergeNeighbors);
if (weirdMerge.Up != 2 || weirdMerge.Down != 99 || weirdMerge.Left != 2 ||
    weirdMerge.Right != 99 || weirdMerge.UpRight != 99 || weirdMerge.DownRight != 10)
{
  throw new InvalidOperationException("Weird tile merge semantics diverged from legacy rules.");
}

Console.WriteLine("PASS: tile merge queries preserve bounded neighbor rewrite semantics");

WorldTile visibleTile = new(IsActive: true, Type: 1);
WorldTile invisibleTile = visibleTile with { IsInvisibleBlock = true };
TileMergeCullMask mergeCullMask = TileMergeCullingQuery.Evaluate(
  invisibleTile,
  visibleTile,
  invisibleTile,
  null,
  visibleTile,
  visibleTile,
  invisibleTile,
  invisibleTile,
  visibleTile,
  showInvisibleBlocks: false);
TileMergeCullMask visibleMergeCullMask = TileMergeCullingQuery.Evaluate(
  invisibleTile,
  visibleTile,
  invisibleTile,
  null,
  visibleTile,
  visibleTile,
  invisibleTile,
  invisibleTile,
  visibleTile,
  showInvisibleBlocks: true);
if (!mergeCullMask.CullUp || mergeCullMask.CullDown || mergeCullMask.CullLeft ||
    !mergeCullMask.CullRight || !mergeCullMask.CullUpLeft ||
    mergeCullMask.CullUpRight || mergeCullMask.CullDownLeft ||
    !mergeCullMask.CullDownRight || visibleMergeCullMask != default)
{
  throw new InvalidOperationException(
    "Tile merge culling did not preserve the explicit invisible-block visibility policy.");
}

if (MossColorQuery.GetColor(179) != 0 || MossColorQuery.GetColor(512) != 0 ||
    MossColorQuery.GetColor(381) != 5 || MossColorQuery.GetColor(540) != 8 ||
    MossColorQuery.GetColor(625) != 9 || MossColorQuery.GetColor(628) != 10 ||
    MossColorQuery.GetColor(1) != -1)
{
  throw new InvalidOperationException("Moss color mapping diverged from legacy tile IDs.");
}

IReadOnlyDictionary<int, int> mossDefaults = MossColorQuery.RegisterDefaults();
if (mossDefaults.Count != 22 ||
    !mossDefaults.SequenceEqual(MossColorQuery.RegisterDefaults()) ||
    mossDefaults is not System.Collections.Frozen.FrozenDictionary<int, int>)
{
  throw new InvalidOperationException("Moss color defaults were not stable and frozen.");
}

Console.WriteLine("PASS: moss color query preserves bounded legacy tile mappings");

Dictionary<ushort, OrePatchTileDefinition> orePatchDefinitions = new()
{
  [1] = new OrePatchTileDefinition(1, IsSolid: true, IsGrass: true, false, false, false)
};
WorldGrid orePatchWorld = new(replayWidth, replayHeight);
for (int x = 210; x <= 230; x++)
{
  for (int y = 217; y <= 240; y++)
  {
    _ = orePatchWorld.TrySetTile(x, y, new WorldTile(true, 1, WallType: 1));
  }
}

_ = orePatchWorld.TrySetTile(220, 210, new WorldTile(true, 1));
_ = orePatchWorld.TrySetTile(219, 210, new WorldTile(true, 1));
_ = orePatchWorld.TrySetTile(221, 210, new WorldTile(true, 1));
OrePatchEligibilityResult orePatchEligibility = OrePatchEligibilityQuery.Evaluate(
  orePatchWorld.CreateSnapshot(replayRequest.Metadata),
  220,
  205,
  worldSurfaceY: 220,
  orePatchDefinitions);
if (!orePatchEligibility.IsEligible || orePatchEligibility.GroundY != 210)
{
  throw new InvalidOperationException("Ore patch eligibility diverged from legacy support checks.");
}

_ = orePatchWorld.TrySetTile(220, 220, new WorldTile(true, 1));
orePatchEligibility = OrePatchEligibilityQuery.Evaluate(
  orePatchWorld.CreateSnapshot(replayRequest.Metadata),
  220,
  205,
  worldSurfaceY: 220,
  orePatchDefinitions);
if (orePatchEligibility.Reason != OrePatchEligibilityReason.SupportWallMissing)
{
  throw new InvalidOperationException("Ore patch eligibility did not reject missing support walls.");
}

_ = orePatchWorld.TrySetTile(220, 220, new WorldTile(true, 1, WallType: 1));
Console.WriteLine("PASS: ore patch eligibility preserves bounded legacy support checks");

WorldSizeProfile smallWorldSize = WorldSizeProfile.FromLegacyIndex(0);
WorldSizeProfile mediumWorldSize = WorldSizeProfile.FromLegacyIndex(1);
WorldSizeProfile largeWorldSize = WorldSizeProfile.FromLegacyIndex(-1);
if (smallWorldSize.Width != 4200 || smallWorldSize.Height != 1200 ||
    mediumWorldSize.Width != 6400 || mediumWorldSize.Height != 1800 ||
    largeWorldSize.Width != 8400 || largeWorldSize.Height != 2400 ||
    WorldSizeProfile.GetLegacyIndexForWidth(4200) != 0 ||
    WorldSizeProfile.GetLegacyIndexForWidth(4201) != 1 ||
    WorldSizeProfile.GetLegacyIndexForWidth(6400) != 1 ||
    WorldSizeProfile.GetLegacyIndexForWidth(6401) != 2)
{
  throw new InvalidOperationException(
    "World size profiles did not preserve legacy dimensions and width classification.");
}

WorldBoundsComponent largeWorldBounds = new(largeWorldSize.Width, largeWorldSize.Height);
if (largeWorldBounds.PixelWidth != 134400 ||
    largeWorldBounds.PixelHeight != 38400 ||
    largeWorldBounds.SectionColumnCount != 42 ||
    largeWorldBounds.SectionRowCount != 16)
{
  throw new InvalidOperationException(
    "World bounds did not derive legacy pixel and section dimensions.");
}

string evidenceDirectory = Path.Combine(repositoryRoot, "docs", "worldgen");
Directory.CreateDirectory(evidenceDirectory);
WorldgenInventory inventory = new(
  "WorldGen.cs",
  sourcePath,
  new FileEvidence(
    new FileInfo(sourcePath).Length,
    File.ReadLines(sourcePath).Count(),
    Convert.ToHexString(SHA256.HashData(sourceBytes)),
    typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown"),
  methods,
  fields,
  references,
  new ReplayEvidence(
    replayRequest.Metadata.Width,
    replayRequest.Metadata.Height,
    replayRequest.Metadata.Seed.Value,
    replayRequest.SpawnX,
    replayRequest.SurfaceY,
    firstFingerprint,
    GetSectionVersions(firstSnapshot)));
JsonSerializerOptions options = new() { WriteIndented = true };
File.WriteAllText(
  Path.Combine(evidenceDirectory, "worldgen-source-inventory.json"),
  JsonSerializer.Serialize(inventory, options));
File.WriteAllText(
  Path.Combine(evidenceDirectory, "worldgen-method-map.md"),
  CreateMethodMap(inventory));
Console.WriteLine(
  $"PASS: stage 0 inventory contains {methods.Count} methods and {fields.Count} fields");
Console.WriteLine("PASS: legacy GenerateWorld has a bounded partial mapping with exclusions");
Console.WriteLine($"PASS: deterministic replay fingerprint {firstFingerprint}");

if (typeof(WorldGenerationRequest).GetProperty("RockLayerY") is null)
{
  throw new InvalidOperationException(
    "WorldGenerationRequest must expose the frozen rock-layer input.");
}

WorldGenerationRequest enrichedRequest = new(
  replayRequest.Metadata,
  replayRequest.SpawnX,
  replayRequest.SurfaceY,
  seedVariant: "default",
    randomStreamVersion: 1,
    generationId: 42);
if (enrichedRequest.GenerationId != 42 ||
    enrichedRequest.SeedVariant != "default" ||
    enrichedRequest.RandomStreamVersion != 1 ||
    enrichedRequest.RockLayerY != 153)
{
  throw new InvalidOperationException("WorldGenerationRequest did not freeze generation inputs.");
}

LegacyTerrainRuntimeProfile runtimeTerrainProfile = new(
  WorldSurface: 100.5,
  RockLayer: 160.25,
  WorldSurfaceLow: 90,
  WorldSurfaceHigh: 110,
  RockLayerLow: 150,
  RockLayerHigh: 170,
  LeftBeachEnd: 20,
  RightBeachStart: replayWidth - 20,
  WaterLine: 200,
  LavaLine: 250);
if (LegacyMainWorldSurfacePolicy.Resolve(runtimeTerrainProfile, replayRequest.Metadata) != 135)
{
  throw new InvalidOperationException(
    "Legacy Main.worldSurface did not project from the terrain high surface plus padding.");
}
Console.WriteLine("PASS: legacy Main.worldSurface projection preserves TerrainPass padding");
WorldGenerationRequest profiledRequest = new(
  replayRequest.Metadata,
  replayRequest.SpawnX,
  replayRequest.SurfaceY,
  terrainProfile: runtimeTerrainProfile);
if (profiledRequest.TerrainProfile != runtimeTerrainProfile)
{
  throw new InvalidOperationException("WorldGenerationRequest did not freeze the terrain runtime profile.");
}

WorldGenerationRequest skyblockRequest = new(
  replayRequest.Metadata,
  replayRequest.SpawnX,
  replayRequest.SurfaceY,
  rockLayerY: replayRequest.RockLayerY,
  terrainProfile: runtimeTerrainProfile,
  isSkyblockWorld: true);
if (!skyblockRequest.IsSkyblockWorld)
{
  throw new InvalidOperationException("WorldGenerationRequest did not freeze the Skyblock guard.");
}

WorldGenerationTrace profiledTrace = new WorldGenerationPipeline().GenerateWithTrace(profiledRequest);
if (!profiledTrace.Stages.Any(stage => stage.Stage == WorldGenerationStage.Cave) ||
    !ContainsMaximumLiquid(profiledTrace.FinalSnapshot))
{
  throw new InvalidOperationException(
    "Profile-enabled world generation did not commit the SmallHoles liquid pass.");
}

Console.WriteLine("PASS: profile-enabled Cave stage commits SmallHoles tile and liquid commands");

AssertThrows<ArgumentException>(() =>
  new WorldGenerationRequest(
    replayRequest.Metadata,
    replayRequest.SpawnX,
    replayRequest.SurfaceY,
    terrainProfile: runtimeTerrainProfile with { RightBeachStart = 10 }));
Console.WriteLine("PASS: legacy terrain runtime profile is validated and optional");

WorldGenerationRequest unsupportedRuleRequest = new(
  new WorldMetadata(
    replayRequest.Metadata.Name,
    replayRequest.Metadata.Seed,
    replayRequest.Metadata.Width,
    replayRequest.Metadata.Height,
    worldId: replayRequest.Metadata.WorldId,
    spawnX: replayRequest.Metadata.SpawnX,
    spawnY: replayRequest.Metadata.SpawnY,
    seedVariant: "for-the-worthy",
    randomStreamVersion: replayRequest.Metadata.RandomStreamVersion),
  replayRequest.SpawnX,
  replayRequest.SurfaceY,
  seedVariant: "for-the-worthy",
  rules: new WorldRuleSnapshotComponent(0, "for-the-worthy", false));
try
{
  _ = new WorldGenerationPipeline().Generate(unsupportedRuleRequest);
  throw new InvalidOperationException(
    "Unsupported secret-seed generation did not fail before world generation.");
}
catch (UnsupportedWorldGenerationRulesException unsupportedRules)
{
  if (unsupportedRules.SeedVariant != "for-the-worthy" ||
      unsupportedRules.Reasons.Count != 1 ||
      !unsupportedRules.Reasons[0].Contains("secret-seed", StringComparison.Ordinal))
  {
    throw new InvalidOperationException(
      "Unsupported secret-seed rejection did not preserve structured deferred reasons.");
  }
}

Console.WriteLine("PASS: unsupported legacy world rules fail before generation");

WorldGenerationBootstrap bootstrap = new WorldGenerationStageSystem().Initialize(enrichedRequest);
if (bootstrap.State.GenerationId != enrichedRequest.GenerationId ||
    bootstrap.Seed.Seed != enrichedRequest.Metadata.Seed.Value ||
    bootstrap.Bounds.Width != replayWidth ||
    bootstrap.Rules.SecretSeedVariant != "default" ||
    bootstrap.Cursor.Stage != WorldGenerationStage.Created ||
    bootstrap.Runtime.GenerationId != enrichedRequest.GenerationId ||
    bootstrap.Runtime.Stage != WorldGenerationStage.Created ||
    bootstrap.Runtime.NextSequence != bootstrap.State.NextSequence ||
    bootstrap.Runtime.Random.State != bootstrap.Cursor.RandomState ||
    bootstrap.Runtime.Random.StreamVersion != enrichedRequest.RandomStreamVersion)
{
  throw new InvalidOperationException(
    "World generation stage bootstrap did not freeze components.");
}

Console.WriteLine("PASS: world-generation runtime and random snapshots are frozen at bootstrap");

WorldGenerationStateComponent state = new(42);
WorldGenerationStateComponent exhaustedSequenceState = new(42, long.MaxValue);
bool rejectedExhaustedSequence = false;
try
{
  _ = exhaustedSequenceState.ReserveSequence();
}
catch (InvalidOperationException exception) when (exception.Message.Contains("exhausting"))
{
  rejectedExhaustedSequence = true;
}

if (!rejectedExhaustedSequence || exhaustedSequenceState.NextSequence != long.MaxValue)
{
  throw new InvalidOperationException(
    "World generation sequence exhaustion was not rejected without mutation.");
}

Console.WriteLine("PASS: world generation sequence exhaustion fails closed");
if (!state.TryAdvance(WorldGenerationStage.Terrain) ||
    state.TryAdvance(WorldGenerationStage.Created) ||
    state.Stage != WorldGenerationStage.Terrain)
{
  throw new InvalidOperationException("World generation stages did not advance monotonically.");
}

WorldGrid commitWorld = new(replayWidth, replayHeight);
WorldSectionCoordinates committedSection = new(0, 0);
IReadOnlyList<TileChangeCommand> commands = new[]
{
  new TileChangeCommand(2, 11, 10, TileChangeKind.Place, 2),
  new TileChangeCommand(1, 10, 10, TileChangeKind.Place, 1)
};
TileChangeCommitSystem commitSystem = new();
if (!Enum.TryParse("SetWall", out TileChangeKind _) ||
    typeof(TileChangeCommand).GetProperty("WallType") is null)
{
  throw new InvalidOperationException(
    "Tile changes must represent a wall-only mutation without replacing the block.");
}

if (!commitSystem.TryCommit(commitWorld, commands, out TileChangeCommitResult commitResult) ||
    commitResult.AppliedCount != 2 ||
    commitWorld.GetTile(10, 10).Type != 1 ||
    commitWorld.GetTile(11, 10).Type != 2 ||
    commitWorld.GetSectionVersion(committedSection) != 2)
{
  throw new InvalidOperationException("Tile changes did not commit in stable sequence order.");
}

WorldTile terminalTileBefore = commitWorld.GetTile(30, 30);
if (commitSystem.TryCommit(
      commitWorld,
      new[] { new TileChangeCommand(long.MaxValue - 1, 30, 30, TileChangeKind.Place, 7) },
      out _) ||
    commitWorld.GetTile(30, 30) != terminalTileBefore)
{
  throw new InvalidOperationException(
    "Tile commit accepted a terminal sequence or mutated before rejecting it.");
}

WorldTile terminalFrameBefore = commitWorld.GetTile(31, 30);
if (commitSystem.TryCommit(
      commitWorld,
      new[] { new TileFrameCommand(long.MaxValue - 1, 31, 30, 1, 2) },
      out _) ||
    commitWorld.GetTile(31, 30) != terminalFrameBefore)
{
  throw new InvalidOperationException(
    "Tile frame commit accepted a terminal sequence or mutated before rejecting it.");
}

Console.WriteLine("PASS: tile and frame commits reject terminal successor sequences atomically");

WorldMetadata sectionVersionMetadata = new("section-version-boundary", new WorldSeed(77), 400, 300);
WorldTile[,] sectionVersionTiles = new WorldTile[400, 300];
long[,] sectionVersions = new long[2, 2];
sectionVersions[0, 0] = long.MaxValue;
WorldGrid sectionVersionWorld = WorldGrid.FromSnapshot(new WorldGridSnapshot(
  sectionVersionMetadata,
  sectionVersionTiles,
  sectionVersions));
if (sectionVersionWorld.TrySetTile(10, 10, new WorldTile(true, 1)) ||
    sectionVersionWorld.TrySetLiquid(10, 10, 1, 1) ||
    sectionVersionWorld.GetTile(10, 10).IsActive ||
    sectionVersionWorld.GetSectionVersion(new WorldSectionCoordinates(0, 0)) != long.MaxValue)
{
  throw new InvalidOperationException(
    "World section version overflow was not rejected atomically.");
}

Console.WriteLine("PASS: world section version exhaustion rejects tile and liquid mutation");

long[,] negativeSectionVersions = new long[2, 2];
negativeSectionVersions[0, 0] = -1;
bool rejectedNegativeSectionVersion = false;
try
{
  _ = new WorldGridSnapshot(sectionVersionMetadata, sectionVersionTiles, negativeSectionVersions);
}
catch (ArgumentOutOfRangeException)
{
  rejectedNegativeSectionVersion = true;
}

if (!rejectedNegativeSectionVersion)
{
  throw new InvalidOperationException("World snapshot accepted a negative section version.");
}

Console.WriteLine("PASS: world snapshot rejects negative section versions");

WorldTile wallSourceTile = new(
  IsActive: true,
  Type: 12,
  LiquidAmount: 91,
  LiquidType: 3,
  FrameX: 144,
  FrameY: 216,
  WallType: 2,
  HasWire: true,
  Slope: 3);
if (!commitWorld.TrySetTile(20, 20, wallSourceTile) ||
    !commitSystem.TryCommit(
      commitWorld,
      new[] { new TileChangeCommand(3, 20, 20, TileChangeKind.SetWall, 0, 9) },
      out TileChangeCommitResult wallCommitResult) ||
    wallCommitResult.AppliedCount != 1)
{
  throw new InvalidOperationException("Wall-only tile commands could not be committed.");
}

WorldTile wallUpdatedTile = commitWorld.GetTile(20, 20);
if (wallUpdatedTile != wallSourceTile with { WallType = 9 })
{
  throw new InvalidOperationException(
    "Wall-only tile commands did not preserve the existing tile state.");
}

WorldTile paintSourceTile = wallSourceTile with
{
  TileColor = 4,
  WallColor = 9
};
if (!commitWorld.TrySetTile(21, 20, paintSourceTile) ||
    !commitSystem.TryCommit(
      commitWorld,
      new[]
      {
        new TileChangeCommand(
          4,
          21,
          20,
          TileChangeKind.SetPaint,
          0,
          TileColor: 27)
      },
      out TileChangeCommitResult paintCommitResult) ||
    paintCommitResult.AppliedCount != 1 ||
    commitWorld.GetTile(21, 20) != paintSourceTile with { TileColor = 27 })
{
  throw new InvalidOperationException(
    "Paint tile commands did not preserve unmodified tile state and wall color.");
}

Console.WriteLine("PASS: paint commands update an explicit color channel through tile commit");

WorldTile killedTile = TileMutationProjection.Apply(
  wallSourceTile,
  new TileChangeCommand(4, 20, 20, TileChangeKind.Kill, 0));
WorldTile preservedLiquidTile = TileMutationProjection.Apply(
  wallSourceTile,
  new TileChangeCommand(5, 20, 20, TileChangeKind.Kill, 0, PreserveLiquid: true));
if (killedTile != default ||
    preservedLiquidTile != new WorldTile(
      IsActive: false,
      Type: 0,
      LiquidAmount: wallSourceTile.LiquidAmount,
      LiquidType: wallSourceTile.LiquidType))
{
  throw new InvalidOperationException(
    "Tile kill projection did not preserve the explicit liquid contract.");
}

long versionBeforeRejectedBatch = commitWorld.GetSectionVersion(committedSection);
IReadOnlyList<TileChangeCommand> rejectedCommands = new[]
{
  new TileChangeCommand(3, 12, 10, TileChangeKind.Place, 3),
  new TileChangeCommand(3, 13, 10, TileChangeKind.Place, 4)
};
if (commitSystem.TryCommit(
      commitWorld,
      rejectedCommands,
      out TileChangeCommitResult rejectedResult) ||
    rejectedResult.FailureReason is null ||
    commitWorld.GetSectionVersion(committedSection) != versionBeforeRejectedBatch ||
    commitWorld.GetTile(12, 10) != default)
{
  throw new InvalidOperationException("Invalid tile batches were not rejected atomically.");
}

if (commitSystem.TryCommit(
      commitWorld,
      new[] { new TileChangeCommand(long.MaxValue, 10, 10, TileChangeKind.Place, 3) },
      out _))
{
  throw new InvalidOperationException("Tile commit accepted a sequence that would overflow next sequence.");
}

Console.WriteLine("PASS: frozen generation inputs, monotonic stages, and atomic tile commit");

if (Type.GetType(
      "Terraria.Dome.Simulation.WorldGeneration.Systems.DirtWallBackgroundSystem, " +
      "Terraria.Dome.Simulation") is null)
{
  throw new InvalidOperationException(
    "The legacy DirtWallBackgrounds source slice must have an ECS system.");
}

WorldGrid wallBackgroundWorld = new(replayWidth, replayHeight);
foreach ((int x, int y) in new[]
{
  (9, 5), (10, 5), (11, 5), (9, 6), (10, 6), (11, 6)
})
{
  _ = wallBackgroundWorld.TrySetTile(x, y, new WorldTile(true, 1));
}

WorldGenerationStateComponent wallBackgroundState = new(42);
_ = wallBackgroundState.TryAdvance(WorldGenerationStage.Terrain);
_ = wallBackgroundState.TryAdvance(WorldGenerationStage.Cave);
_ = wallBackgroundState.TryAdvance(WorldGenerationStage.Biome);
List<TileChangeCommand> wallBackgroundCommands = new();
new DirtWallBackgroundSystem().AppendCommands(
  wallBackgroundWorld.CreateSnapshot(replayRequest.Metadata),
  worldSurfaceY: 6,
  Enumerable.Repeat(0, replayWidth - 2).ToArray(),
  ref wallBackgroundState,
  wallBackgroundCommands);
if (!wallBackgroundCommands.Any(command =>
      command.X == 10 && command.Y == 6 && command.Kind == TileChangeKind.SetWall) ||
    !commitSystem.TryCommit(
      wallBackgroundWorld,
      wallBackgroundCommands,
      out TileChangeCommitResult wallBackgroundResult) ||
    !wallBackgroundResult.Succeeded ||
    wallBackgroundWorld.GetTile(10, 6) != new WorldTile(true, 1, WallType: 2))
{
  throw new InvalidOperationException(
    "Dirt wall backgrounds did not preserve the enclosed tile while writing a dirt wall.");
}

WorldGenerationRequest capturedWallRequest = new(
  replayRequest.Metadata,
  replayRequest.SpawnX,
  replayRequest.SurfaceY,
  seedVariant: replayRequest.SeedVariant,
  randomStreamVersion: replayRequest.RandomStreamVersion,
  generationId: replayRequest.GenerationId,
  rules: replayRequest.Rules,
  rockLayerY: replayRequest.RockLayerY,
  dirtWallSurfaceOffsetChanges: Enumerable.Repeat(0, replayWidth - 2).ToArray());
WorldGrid defaultWallPipelineWorld = new WorldGenerationPipeline().Generate(replayRequest);
WorldGrid capturedWallPipelineWorld = new WorldGenerationPipeline().Generate(capturedWallRequest);
int defaultDirtWallCount = CountWalls(defaultWallPipelineWorld, wallType: 2);
int capturedDirtWallCount = CountWalls(capturedWallPipelineWorld, wallType: 2);
if (capturedDirtWallCount <= defaultDirtWallCount)
{
  throw new InvalidOperationException(
    "An explicit DirtWallBackgrounds oracle did not add wall-only pipeline output.");
}

IReadOnlyList<int> parsedDirtWallOffsets = ParseDirtWallOffsetChanges(
  new[] { "1,-1", "2,0", "3,1" },
  expectedWorldWidth: 5);
if (!parsedDirtWallOffsets.SequenceEqual(new[] { -1, 0, 1 }))
{
  throw new InvalidOperationException(
    "Dirt wall offset artifacts were not parsed in their recorded column order.");
}

static bool ContainsMaximumLiquid(WorldGridSnapshot snapshot)
{
  for (int x = 0; x < snapshot.Metadata.Width; x++)
  {
    for (int y = 0; y < snapshot.Metadata.Height; y++)
    {
      if (snapshot.GetTile(x, y).LiquidAmount == byte.MaxValue)
      {
        return true;
      }
    }
  }

  return false;
}

static int CountWalls(WorldGrid world, ushort wallType)
{
  int count = 0;
  for (int y = 0; y < world.Height; y++)
  {
    for (int x = 0; x < world.Width; x++)
    {
      if (world.GetTile(x, y).WallType == wallType)
      {
        count++;
      }
    }
  }

  return count;
}

static int CountWallTiles(WorldGridSnapshot snapshot)
{
  int count = 0;
  for (int y = 0; y < snapshot.Metadata.Height; y++)
  {
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      if (snapshot.GetTile(x, y).WallType != 0)
      {
        count++;
      }
    }
  }

  return count;
}

static IReadOnlyList<int> ParseDirtWallOffsetChanges(
  IReadOnlyList<string> rows,
  int expectedWorldWidth)
{
  ArgumentNullException.ThrowIfNull(rows);
  if (expectedWorldWidth < 3)
  {
    throw new ArgumentOutOfRangeException(nameof(expectedWorldWidth));
  }

  int expectedCount = expectedWorldWidth - 2;
  if (rows.Count != expectedCount)
  {
    throw new ArgumentException(
      "A dirt wall offset row is required for every non-edge column.",
      nameof(rows));
  }

  int[] changes = new int[expectedCount];
  for (int index = 0; index < rows.Count; index++)
  {
    string[] values = rows[index].Split(',', StringSplitOptions.TrimEntries);
    if (values.Length != 2 ||
        !int.TryParse(values[0], out int column) ||
        !int.TryParse(values[1], out int delta))
    {
      throw new ArgumentException("A dirt wall offset row is malformed.", nameof(rows));
    }

    if (column != index + 1 || delta < -1 || delta > 1)
    {
      throw new ArgumentException("A dirt wall offset row is outside the legacy range.", nameof(rows));
    }

    changes[index] = delta;
  }

  return Array.AsReadOnly(changes);
}

static LegacyTerrainRuntimeProfile ReadLegacyTerrainRuntimeProfile(
  string path,
  WorldMetadata metadata)
{
  string? firstLine = File.ReadLines(path)
    .FirstOrDefault(line => !String.IsNullOrWhiteSpace(line));
  if (firstLine is null)
  {
    throw new InvalidDataException("The legacy terrain profile artifact is empty.");
  }

  using JsonDocument document = JsonDocument.Parse(firstLine);
  JsonElement root = document.RootElement;
  LegacyTerrainRuntimeProfile profile = new(
    root.GetProperty("worldSurface").GetDouble(),
    root.GetProperty("rockLayer").GetDouble(),
    root.GetProperty("worldSurfaceLow").GetDouble(),
    root.GetProperty("worldSurfaceHigh").GetDouble(),
    root.GetProperty("rockLayerLow").GetDouble(),
    root.GetProperty("rockLayerHigh").GetDouble(),
    root.GetProperty("leftBeachEnd").GetInt32(),
    root.GetProperty("rightBeachStart").GetInt32(),
    root.GetProperty("waterLine").GetInt32(),
    root.GetProperty("lavaLine").GetInt32());
  string checkpointPath = Path.Combine(
    Path.GetDirectoryName(path) ?? String.Empty,
    "legacy-terrain-checkpoints.jsonl");
  if (File.Exists(checkpointPath))
  {
    string? initialLine = File.ReadLines(checkpointPath)
      .FirstOrDefault(line => line.Contains("\"phase\":\"initial\"", StringComparison.Ordinal));
    if (initialLine is not null)
    {
      using JsonDocument checkpoint = JsonDocument.Parse(initialLine);
      profile = profile with
      {
        InitialWorldSurface = checkpoint.RootElement.GetProperty("surface").GetDouble(),
        InitialRockLayer = checkpoint.RootElement.GetProperty("rockLayer").GetDouble()
      };
    }
  }

  profile.Validate(metadata);
  return profile;
}

TerrainProfileComponent terrainProfile = new(
  surfaceY: replaySurfaceY,
  rockLayerY: 150,
  underworldY: 270);
if (terrainProfile.SurfaceY >= terrainProfile.RockLayerY ||
    terrainProfile.RockLayerY >= terrainProfile.UnderworldY ||
    terrainProfile.UnderworldY >= replayHeight)
{
  throw new InvalidOperationException("Terrain profile height bands were not ordered.");
}

WorldGrid caveWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent caveState = new(42);
List<TileChangeCommand> terrainCommands = new();
GenerationRandomState terrainRandomState = new(unchecked((uint)replayRequest.Metadata.Seed.Value));
new TerrainBaseSystem().AppendCommands(
  caveWorld.CreateSnapshot(replayRequest.Metadata),
  replayRequest,
  terrainProfile,
  ref caveState,
  ref terrainRandomState,
  terrainCommands);
if (!new TileChangeCommitSystem().TryCommit(caveWorld, terrainCommands, out _))
{
  throw new InvalidOperationException("Terrain base commands could not be committed.");
}

WorldGridSnapshot caveSnapshot = caveWorld.CreateSnapshot(replayRequest.Metadata);
GenerationCursorComponent terrainCursor = new GenerationCursorComponent(
  WorldGenerationStage.Created,
  0,
  0,
  0).Advance(WorldGenerationStage.Terrain, 0, 1, terrainRandomState.Value);
WorldGenerationCheckpoint terrainCheckpoint = new(
  caveSnapshot,
    caveState,
    terrainCursor,
    WorldGenerationRuntimeState.Create(
      caveState,
      terrainCursor,
      replayRequest.RandomStreamVersion));
bool rejectedUndefinedRuntimeStage = false;
try
{
  _ = new WorldGenerationRuntimeState(
    caveState.GenerationId,
    (WorldGenerationStage)int.MaxValue,
    caveState.NextSequence,
    new WorldGenerationRandomSnapshot(
      terrainCursor.RandomState,
      replayRequest.RandomStreamVersion));
}
catch (ArgumentOutOfRangeException)
{
  rejectedUndefinedRuntimeStage = true;
}

if (!rejectedUndefinedRuntimeStage)
{
  throw new InvalidOperationException(
    "World generation accepted an undefined runtime stage.");
}

Console.WriteLine("PASS: generation runtime rejects undefined stages");
bool rejectedMismatchedRuntime = false;
try
{
  _ = new WorldGenerationCheckpoint(
    caveSnapshot,
    caveState,
    terrainCursor,
    new WorldGenerationRuntimeState(
      caveState.GenerationId,
      caveState.Stage,
      caveState.NextSequence + 1,
      new WorldGenerationRandomSnapshot(
        terrainCursor.RandomState,
        replayRequest.RandomStreamVersion)));
}
catch (ArgumentException)
{
  rejectedMismatchedRuntime = true;
}

if (!rejectedMismatchedRuntime)
{
  throw new InvalidOperationException(
    "A generation checkpoint accepted a runtime state with a mismatched sequence.");
}

Console.WriteLine("PASS: generation checkpoints reject mismatched runtime state");
WorldGrid uninterruptedCaveWorld = terrainCheckpoint.RestoreWorld();
WorldGrid restartedCaveWorld = terrainCheckpoint.RestoreWorld();
WorldGenerationStateComponent uninterruptedCaveState = terrainCheckpoint.State;
WorldGenerationStateComponent restartedCaveState = terrainCheckpoint.State;
List<TileChangeCommand> firstCaveCommands = new();
List<TileChangeCommand> secondCaveCommands = new();
CaveCarvingComponent caveProfile = new("single-tunnel", radius: 1, density: 4);
new CaveCarvingSystem().AppendCommands(
  caveSnapshot,
  replayRequest,
  caveProfile,
  ref caveState,
  firstCaveCommands);
WorldGenerationStateComponent secondCaveState = new(42);
List<TileChangeCommand> ignoredTerrainCommands = new();
new TerrainBaseSystem().AppendCommands(
  caveWorld.CreateSnapshot(replayRequest.Metadata),
  replayRequest,
  terrainProfile,
  ref secondCaveState,
  ignoredTerrainCommands);
new CaveCarvingSystem().AppendCommands(
  caveSnapshot,
  replayRequest,
  caveProfile,
  ref secondCaveState,
  secondCaveCommands);
List<TileChangeCommand> uninterruptedCommands = new();
List<TileChangeCommand> restartedCommands = new();
new CaveCarvingSystem().AppendCommands(
  uninterruptedCaveWorld.CreateSnapshot(replayRequest.Metadata),
  replayRequest,
  caveProfile,
  ref uninterruptedCaveState,
  uninterruptedCommands);
new CaveCarvingSystem().AppendCommands(
  restartedCaveWorld.CreateSnapshot(replayRequest.Metadata),
  replayRequest,
  caveProfile,
  ref restartedCaveState,
  restartedCommands);
bool uninterruptedCommitted = new TileChangeCommitSystem().TryCommit(
  uninterruptedCaveWorld,
  uninterruptedCommands,
  out TileChangeCommitResult uninterruptedCommit);
bool restartedCommitted = new TileChangeCommitSystem().TryCommit(
  restartedCaveWorld,
  restartedCommands,
  out TileChangeCommitResult restartedCommit);
string uninterruptedCaveFingerprint = CreateSnapshotFingerprint(
  uninterruptedCaveWorld.CreateSnapshot(replayRequest.Metadata));
string restartedCaveFingerprint = CreateSnapshotFingerprint(
  restartedCaveWorld.CreateSnapshot(replayRequest.Metadata));
if (!uninterruptedCommitted ||
    !restartedCommitted ||
     uninterruptedCommit.AppliedCount != restartedCommit.AppliedCount ||
    !StringComparer.Ordinal.Equals(uninterruptedCaveFingerprint, restartedCaveFingerprint) ||
    uninterruptedCaveState.Stage != restartedCaveState.Stage ||
    uninterruptedCaveState.NextSequence != restartedCaveState.NextSequence ||
    terrainCheckpoint.Cursor != new GenerationCursorComponent(
      WorldGenerationStage.Terrain,
      0,
      1,
      terrainRandomState.Value))
{
  throw new InvalidOperationException("Cursor checkpoint restart replay was not deterministic.");
}
if (!firstCaveCommands.SequenceEqual(secondCaveCommands) ||
    firstCaveCommands.Any(command =>
      Math.Abs(command.X - replayRequest.SpawnX) <= 4 &&
      command.Y >= replayRequest.SurfaceY &&
      command.Y <= replayRequest.SurfaceY + 7))
{
  throw new InvalidOperationException(
    "Cave carving was not deterministic or crossed spawn protection.");
}

CursorRestartEvidence cursorEvidence = new(
  "Terrain",
  "Cave",
  terrainCheckpoint.Cursor.SectionX,
  terrainCheckpoint.Cursor.SectionY,
  terrainCheckpoint.Cursor.RandomState,
  restartedCaveFingerprint,
  uninterruptedCaveState.NextSequence,
  restartedCaveState.NextSequence);
File.WriteAllText(
  Path.Combine(evidenceDirectory, "cursor-restart-replay.json"),
  JsonSerializer.Serialize(cursorEvidence, options));
Console.WriteLine(
  $"PASS: cursor checkpoint restart replay resumes deterministically at cave stage " +
  $"fingerprint {restartedCaveFingerprint}");

BiomeSurfaceResult unsupportedBiome = new BiomeSurfaceSystem().AppendCommands(
  caveSnapshot,
  new BiomeSurfaceComponent("unsupported-biome"),
  ref caveState,
  new List<TileChangeCommand>());
if (unsupportedBiome.Supported || unsupportedBiome.FailureReason is null)
{
  throw new InvalidOperationException("Unsupported biome rules were not explicit.");
}

if (!WorldGenerationSystemOrder.Systems.SequenceEqual(new[]
    {
      WorldGenerationSystemId.TerrainBase,
      WorldGenerationSystemId.CaveCarving,
      WorldGenerationSystemId.BiomeSurface,
      WorldGenerationSystemId.OrePlacement,
      WorldGenerationSystemId.StructurePlacement,
      WorldGenerationSystemId.TreePlacement,
      WorldGenerationSystemId.LiquidSource,
      WorldGenerationSystemId.LiquidPropagation,
      WorldGenerationSystemId.TileFrame,
      WorldGenerationSystemId.TileChangeCommit,
    WorldGenerationSystemId.Validation
  }) ||
  !WorldGenerationSystemOrder.Systems.SequenceEqual(WorldGenerationSystemOrder.RegisterDefaults()))
{
  throw new InvalidOperationException("World generation system order was not explicit and stable.");
}

try
{
  ((IList<WorldGenerationSystemId>)WorldGenerationSystemOrder.Systems)[0] =
    WorldGenerationSystemOrder.Systems[0];
  throw new InvalidOperationException("World generation system order projection was mutable.");
}
catch (NotSupportedException)
{
}

Console.WriteLine(
  "PASS: terrain profile, protected deterministic cave, biome failure, and system order");

WorldGrid objectWorld = new(replayWidth, replayHeight);
WorldGridSnapshot objectSnapshot = objectWorld.CreateSnapshot(replayRequest.Metadata);
TileProtectionComponent protection = new(
  replayRequest.SpawnX,
  replayRequest.SurfaceY,
  HalfWidth: 4,
  Height: 7);
OreDefinition copper = new(
  "copper",
  tileType: 7,
  minDepth: replaySurfaceY + 10,
  maxDepth: replaySurfaceY + 80,
  veinRadius: 1,
  priority: 10);
WorldGenerationStateComponent oreState = new(42);
List<TileChangeCommand> oreCommands = new();
new OrePlacementSystem().AppendCommands(
  objectSnapshot,
  replayRequest,
  copper,
  protection,
  ref oreState,
  oreCommands);
if (oreCommands.Count == 0 ||
    oreCommands.Any(command => protection.IsProtected(command.X, command.Y)))
{
  throw new InvalidOperationException(
    "Ore placement did not produce a protected deterministic vein.");
}

OrePlacementTransactionSystem oreTransactionSystem = new();
WorldGrid oreTransactionWorld = new(replayWidth, replayHeight);
OrePlacementPreparationResult orePreparation = oreTransactionSystem.TryPrepare(
  oreTransactionWorld.CreateSnapshot(replayRequest.Metadata),
  replayRequest,
  copper,
  protection,
  out OrePlacementPreparationResult orePreparationResult)
  ? orePreparationResult
  : throw new InvalidOperationException("Ore transaction did not prepare a free footprint.");
WorldGenerationStateComponent oreTransactionState = new(43);
List<TileChangeCommand> oreTransactionCommands = new();
if (!oreTransactionSystem.TryAppendCommands(
      oreTransactionWorld.CreateSnapshot(replayRequest.Metadata),
      orePreparation,
      protection,
      ref oreTransactionState,
      oreTransactionCommands) ||
    oreTransactionCommands.Count == 0 ||
    oreTransactionCommands.Any(command => protection.IsProtected(command.X, command.Y)))
{
  throw new InvalidOperationException("Ore transaction did not append prepared commands.");
}

WorldGrid blockedOreTransactionWorld = new(replayWidth, replayHeight);
_ = blockedOreTransactionWorld.TrySetTile(
  orePreparation.Cells[0].X,
  orePreparation.Cells[0].Y,
  new WorldTile(true, 1));
if (oreTransactionSystem.TryPrepare(
      blockedOreTransactionWorld.CreateSnapshot(replayRequest.Metadata),
      replayRequest,
      copper,
      protection,
      out _) )
{
  throw new InvalidOperationException("Ore transaction did not reject an occupied footprint.");
}

OreDefinition orePatchDefinition = new(
  "ore-patch",
  tileType: 7,
  minDepth: replaySurfaceY + 10,
  maxDepth: replaySurfaceY + 80,
  veinRadius: 0,
  priority: 10);
OrePatchPlacementSystem orePatchPlacementSystem = new();
OrePatchPlacementPreparation orePatchPreparation = orePatchPlacementSystem.TryPrepare(
  orePatchWorld.CreateSnapshot(replayRequest.Metadata),
  originX: 220,
  originY: 205,
  worldSurfaceY: 220,
  orePatchDefinitions,
  orePatchDefinition,
  protection,
  out OrePatchPlacementPreparation orePatchPreparationResult)
  ? orePatchPreparationResult
  : throw new InvalidOperationException("Eligible ore patch did not prepare a transaction.");
WorldGenerationStateComponent orePatchState = new(44);
List<TileChangeCommand> orePatchCommands = new();
_ = orePatchWorld.TrySetTile(
  orePatchPreparation.Transaction.Cells[0].X,
  orePatchPreparation.Transaction.Cells[0].Y,
  new WorldTile(true, 1));
if (orePatchPlacementSystem.TryAppendCommands(
      orePatchWorld.CreateSnapshot(replayRequest.Metadata),
      orePatchPreparation,
      protection,
      ref orePatchState,
      orePatchCommands) ||
    orePatchCommands.Count != 0 ||
    orePatchState.Stage >= WorldGenerationStage.Ore)
{
  throw new InvalidOperationException(
    "Ore patch placement did not reject a post-prepare footprint conflict atomically.");
}

_ = orePatchWorld.TrySetTile(
  orePatchPreparation.Transaction.Cells[0].X,
  orePatchPreparation.Transaction.Cells[0].Y,
  default);
if (orePatchPreparation.GroundY != 210 ||
    !orePatchPlacementSystem.TryAppendCommands(
      orePatchWorld.CreateSnapshot(replayRequest.Metadata),
      orePatchPreparation,
      protection,
      ref orePatchState,
      orePatchCommands) ||
    orePatchCommands.Count == 0)
{
  throw new InvalidOperationException(
    "Ore patch placement did not compose eligibility and a prepared transaction.");
}

LegacyPassRandomState legacyOrePatchRandom = new(1456);
OrePatchPlacementPreparation legacyOrePatchPreparation = orePatchPlacementSystem.TryPrepare(
  orePatchWorld.CreateSnapshot(replayRequest.Metadata),
  originX: 220,
  originY: 205,
  worldSurfaceY: 220,
  orePatchDefinitions,
  orePatchDefinition,
  copperTileType: 7,
  ironTileType: 6,
  legacyOrePatchRandom,
  protection,
  out OrePatchPlacementPreparation legacyOrePatchPreparationResult)
  ? legacyOrePatchPreparationResult
  : throw new InvalidOperationException("Legacy ore patch did not prepare a transaction.");
WorldGenerationStateComponent legacyOrePatchState = new(44);
List<TileChangeCommand> legacyOrePatchCommands = new();
if (!orePatchPlacementSystem.TryAppendLegacyTrailAndBlobCommands(
      orePatchWorld.CreateSnapshot(replayRequest.Metadata),
      legacyOrePatchPreparation,
      legacyOrePatchRandom,
      ref legacyOrePatchState,
      legacyOrePatchCommands) ||
    !legacyOrePatchCommands.Any(command => command.Source == "worldgen.ore.OrePatch.trail") ||
    !legacyOrePatchCommands.Any(command => command.Source == "worldgen.ore.OrePatch.blob"))
{
  throw new InvalidOperationException("Ore patch did not compose source-backed trail and blob commands.");
}

Console.WriteLine(
  "PASS: ore placement prepares atomically and composes source-backed trail/blob commands");

TreeDefinition ordinaryTree = new(
  "ordinary",
  trunkTileType: 3,
  leafTileType: 4,
  minimumHeight: 3,
  maximumHeight: 5,
  canopyRadius: 2);
TreePlacementComponent treePlacement = new(ordinaryTree.Id, 100, 100);
WorldGenerationStateComponent treeState = new(42);
List<TileChangeCommand> treeCommands = new();
if (!new TreePlacementSystem().TryAppendCommands(
      objectSnapshot,
      replayRequest,
      ordinaryTree,
      treePlacement,
      protection,
      ref treeState,
      treeCommands) ||
    treeCommands.Count == 0 ||
    treeCommands.Any(command => protection.IsProtected(command.X, command.Y)))
{
  throw new InvalidOperationException("Tree placement did not respect definition or protection.");
}

StructureDefinition house = new(
  "starter-house",
  width: 4,
  height: 3,
  tileType: 5,
  wallType: 1,
  allowReplaceExisting: false);
StructurePlacementSystem structureSystem = new();
if (!structureSystem.TryPrepare(
      objectSnapshot,
      house,
      originX: 20,
      originY: 20,
      protection,
      out StructurePlacementComponent placement,
      out string? structureFailure))
{
  throw new InvalidOperationException($"Starter structure did not prepare: {structureFailure}");
}

_ = objectWorld.TrySetTile(20, 20, new WorldTile(true, 1));
WorldGenerationStateComponent structureState = new(42);
List<TileChangeCommand> structureCommands = new();
if (structureSystem.AppendCommands(
      objectWorld.CreateSnapshot(replayRequest.Metadata),
      house,
      placement,
      protection,
      ref structureState,
      structureCommands) ||
    structureCommands.Count != 0 ||
    structureState.Stage >= WorldGenerationStage.Structure)
{
  throw new InvalidOperationException(
    "Structure commit did not reject a post-prepare footprint conflict atomically.");
}

_ = objectWorld.TrySetTile(20, 20, default);
if (!structureSystem.AppendCommands(
      objectSnapshot,
      house,
      placement,
      protection,
    ref structureState,
    structureCommands) ||
    structureCommands.Count != house.Width * house.Height * 2 ||
    !new TileChangeCommitSystem().TryCommit(objectWorld, structureCommands, out _))
{
  throw new InvalidOperationException("Starter structure did not commit Tile and Wall transactionally.");
}

if (objectWorld.GetTile(20, 20).WallType != house.WallType)
{
  throw new InvalidOperationException("Starter structure did not commit its wall footprint.");
}

WorldGridSnapshot occupiedSnapshot = objectWorld.CreateSnapshot(replayRequest.Metadata);
if (structureSystem.TryPrepare(
      occupiedSnapshot,
      house,
      originX: 20,
      originY: 20,
      protection,
      out _,
      out _))
{
  throw new InvalidOperationException(
    "Structure conflict was not rejected before command creation.");
}

Console.WriteLine(
  "PASS: ore, tree, and transactional structure placement honor definitions and protection");

StructurePlacementTransactionSystem structureTransactionSystem = new();
WorldGrid transactionStructureWorld = new(replayWidth, replayHeight);
WorldGridSnapshot transactionStructureSnapshot =
  transactionStructureWorld.CreateSnapshot(replayRequest.Metadata);
if (!structureTransactionSystem.TryPrepare(
      transactionStructureSnapshot,
      house,
      originX: 40,
      originY: 40,
      protection,
      out StructurePlacementComponent transactionPlacement,
      out string? transactionFailure))
{
  throw new InvalidOperationException(
    $"Independent structure transaction did not prepare: {transactionFailure}");
}

_ = transactionStructureWorld.TrySetTile(
  transactionPlacement.OriginX,
  transactionPlacement.OriginY,
  new WorldTile(true, 1));
WorldGenerationStateComponent blockedTransactionState = new(45);
List<TileChangeCommand> blockedTransactionCommands = new();
if (structureTransactionSystem.TryAppendCommands(
      transactionStructureWorld.CreateSnapshot(replayRequest.Metadata),
      house,
      transactionPlacement,
      protection,
      ref blockedTransactionState,
      blockedTransactionCommands) ||
    blockedTransactionCommands.Count != 0 ||
    blockedTransactionState.Stage >= WorldGenerationStage.Structure)
{
  throw new InvalidOperationException(
    "Independent structure transaction did not reject a post-prepare conflict atomically.");
}

_ = transactionStructureWorld.TrySetTile(
  transactionPlacement.OriginX,
  transactionPlacement.OriginY,
  default);
WorldGenerationStateComponent transactionStructureState = new(45);
List<TileChangeCommand> transactionStructureCommands = new();
if (!structureTransactionSystem.TryAppendCommands(
      transactionStructureSnapshot,
      house,
      transactionPlacement,
      protection,
      ref transactionStructureState,
      transactionStructureCommands) ||
    transactionStructureCommands.Count != house.Width * house.Height * 2 ||
    !new TileChangeCommitSystem().TryCommit(
      transactionStructureWorld,
      transactionStructureCommands,
      out _)
    || transactionStructureWorld.GetTile(40, 40).WallType != house.WallType)
{
  throw new InvalidOperationException(
    "Independent structure transaction did not commit tile and wall commands.");
}

WorldGrid replayStructureWorld = new(replayWidth, replayHeight);
StructurePlacementComponent replayPlacement = structureTransactionSystem.TryPrepare(
  replayStructureWorld.CreateSnapshot(replayRequest.Metadata),
  house,
  originX: 40,
  originY: 40,
  protection,
  out StructurePlacementComponent replayPlacementResult,
  out string? replayStructureFailure)
  ? replayPlacementResult
  : throw new InvalidOperationException(
    $"Structure transaction replay did not prepare: {replayStructureFailure}");
WorldGenerationStateComponent replayStructureState = new(45);
List<TileChangeCommand> replayStructureCommands = new();
if (!structureTransactionSystem.TryAppendCommands(
      replayStructureWorld.CreateSnapshot(replayRequest.Metadata),
      house,
      replayPlacement,
      protection,
      ref replayStructureState,
      replayStructureCommands) ||
    !transactionStructureCommands.SequenceEqual(replayStructureCommands))
{
  throw new InvalidOperationException(
    "Independent structure transaction command replay was not deterministic.");
}

Console.WriteLine(
  "PASS: independent structure transaction commits tile and wall commands atomically");

SimpleStructurePattern pattern = SimpleStructurePattern.Parse(new[] { "0x", "10" });
SimpleStructurePattern mirroredPattern = pattern.Mirror(horizontalMirror: true, verticalMirror: false);
IReadOnlyList<StructureDefinition> simpleActions = new[]
{
  new StructureDefinition("pattern-tile", 1, 1, 5, 2, allowReplaceExisting: false),
  new StructureDefinition("pattern-wall", 1, 1, 6, 0, allowReplaceExisting: false)
};
WorldGrid simpleStructureWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent simpleStructureState = new(50);
List<TileChangeCommand> simpleStructureCommands = new();
if (!new SimpleStructurePlacementSystem().TryAppendCommands(
      simpleStructureWorld.CreateSnapshot(replayRequest.Metadata),
      mirroredPattern,
      simpleActions,
      originX: 50,
      originY: 50,
      protection,
      ref simpleStructureState,
      simpleStructureCommands) ||
    simpleStructureCommands.Count != 5 ||
    simpleStructureCommands.Any(command => command.X is not (49 or 50)) ||
    simpleStructureCommands.Any(command => command.Y is not (50 or 51)) ||
    simpleStructureState.Stage < WorldGenerationStage.Structure)
{
  throw new InvalidOperationException(
    "SimpleStructure pattern did not produce deterministic mirrored tile commands.");
}

if (SimpleStructurePattern.Parse(new[] { "0" }).GetActionIndex(0, 0) != 0 ||
    SimpleStructurePattern.Parse(new[] { "x" }).GetActionIndex(0, 0) != -1)
{
  throw new InvalidOperationException("SimpleStructure pattern digit and hole parsing is incorrect.");
}

if (new SimpleStructurePlacementSystem().TryAppendCommands(
      simpleStructureWorld.CreateSnapshot(replayRequest.Metadata),
      SimpleStructurePattern.Parse(new[] { "0" }),
      new[] { new StructureDefinition("wide", 2, 1, 5, 0, allowReplaceExisting: false) },
      originX: 60,
      originY: 60,
      protection,
      ref simpleStructureState,
      new List<TileChangeCommand>()))
{
  throw new InvalidOperationException(
    "SimpleStructure accepted a non-1x1 action that has no typed cell contract.");
}

Console.WriteLine(
  "PASS: SimpleStructure pattern parsing, mirroring, fail-closed actions, and commands");

WorldGrid liquidWorld = new(replayWidth, replayHeight);
WorldMetadata liquidMetadata = replayRequest.Metadata;
WorldGridSnapshot liquidSnapshot = liquidWorld.CreateSnapshot(liquidMetadata);
WorldBoundsComponent liquidBounds = new(replayWidth, replayHeight);
LiquidDefinition water = new("water", type: 0, maxAmount: byte.MaxValue);
LiquidDefinition lava = new("lava", type: 1, maxAmount: byte.MaxValue);
LiquidDefinition obsidian = new("obsidian", type: 2, maxAmount: byte.MaxValue);
LiquidSourceComponent liquidSource = new(50, 100, water.Type, byte.MaxValue, "worldgen");
WorldGenerationStateComponent liquidState = new(42);
List<LiquidWorkItemComponent> workItems = new();
new LiquidSourceSystem().AppendWorkItems(
  new[] { liquidSource },
  liquidBounds,
  ref liquidState,
  workItems);
if (workItems.Count != 1 || workItems[0].LiquidType != water.Type)
{
  throw new InvalidOperationException("Liquid sources did not become bounded work items.");
}

List<LiquidChangeCommand> firstLiquidCommands = new();
LiquidPropagationSystem propagationSystem = new();
LiquidPropagationResult firstPropagation = propagationSystem.TryAppendCommands(
  liquidSnapshot,
  new[] { water, lava, obsidian },
  new[] { new LiquidMergeComponent(water.Type, lava.Type, obsidian.Type) },
  workItems,
  budget: 4,
  ref liquidState,
  firstLiquidCommands);
if (!firstPropagation.Succeeded ||
    firstPropagation.ConsumedWorkItems > 4 ||
    firstPropagation.RemainingWorkItems < 0 ||
    firstLiquidCommands.Count > 4)
{
  throw new InvalidOperationException("Liquid propagation exceeded its deterministic budget.");
}

WorldGrid mergeWorld = new(replayWidth, replayHeight);
if (!mergeWorld.TrySetLiquid(liquidSource.X, liquidSource.Y, amount: 100, lava.Type))
{
  throw new InvalidOperationException("Liquid merge fixture could not seed lava.");
}
WorldGridSnapshot mergeSnapshot = mergeWorld.CreateSnapshot(liquidMetadata);
WorldGenerationStateComponent mergeState = new(42);
List<LiquidWorkItemComponent> mergeWorkItems = new();
new LiquidSourceSystem().AppendWorkItems(
  new[] { liquidSource },
  liquidBounds,
  ref mergeState,
  mergeWorkItems);
List<LiquidChangeCommand> mergeCommands = new();
LiquidPropagationResult mergeResult = propagationSystem.TryAppendCommands(
  mergeSnapshot,
  new[] { water, lava, obsidian },
  new[] { new LiquidMergeComponent(water.Type, lava.Type, obsidian.Type) },
  mergeWorkItems,
  budget: 1,
  ref mergeState,
  mergeCommands);
if (!mergeResult.Succeeded || mergeCommands.Count != 1 ||
    mergeCommands[0].Type != obsidian.Type)
{
  throw new InvalidOperationException("Liquid merge did not use the explicit merge rule.");
}

long liquidVersion = mergeWorld.GetSectionVersion(
  mergeWorld.GetSectionCoordinates(liquidSource.X, liquidSource.Y));
if (!new LiquidChangeCommitSystem().TryCommit(
      mergeWorld,
      mergeCommands,
      new[] { water, lava, obsidian },
      out LiquidChangeCommitResult liquidCommit) ||
    liquidCommit.AppliedCount != 1 ||
    mergeWorld.GetTile(liquidSource.X, liquidSource.Y).LiquidType != obsidian.Type ||
    mergeWorld.GetSectionVersion(
      mergeWorld.GetSectionCoordinates(liquidSource.X, liquidSource.Y)) <= liquidVersion)
{
  throw new InvalidOperationException(
    "Liquid commands did not commit with a section version change.");
}

if (new LiquidChangeCommitSystem().TryCommit(
      mergeWorld,
      new[] { new LiquidChangeCommand(99, 1, 1, byte.MaxValue, 9) },
      new[] { water },
      out LiquidChangeCommitResult invalidLiquidCommit) ||
    invalidLiquidCommit.FailureReason is null)
{
  throw new InvalidOperationException("Invalid liquid commands were not rejected.");
}

if (LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Lava) !=
      LiquidInteractionKind.LavaWater ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Honey,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water) !=
      LiquidInteractionKind.HoneyWater ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Lava,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Honey) !=
      LiquidInteractionKind.HoneyLava ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Shimmer,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water) !=
      LiquidInteractionKind.ShimmerWater ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Lava,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Shimmer) !=
      LiquidInteractionKind.ShimmerLava ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Honey,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Shimmer) !=
      LiquidInteractionKind.ShimmerHoney ||
    LiquidInteractionClassifier.GetKind(
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water) !=
      LiquidInteractionKind.None ||
    LiquidInteractionClassifier.GetKind(
      (Terraria.Dome.Simulation.Liquid.Components.LiquidType)9,
      Terraria.Dome.Simulation.Liquid.Components.LiquidType.Water) !=
      LiquidInteractionKind.None)
{
  throw new InvalidOperationException(
    "Liquid interaction classification did not preserve the legacy pair table.");
}

WorldGrid sessionWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent sessionState = new(44);
List<LiquidWorkItemComponent> sessionWorkItems = new();
new LiquidSourceSystem().AppendWorkItems(
  new[] { liquidSource },
  liquidBounds,
  ref sessionState,
  sessionWorkItems);
LiquidPropagationSession session = new(
  sessionWorld,
  liquidMetadata,
  new[] { water, lava, obsidian },
  new[] { new LiquidMergeComponent(water.Type, lava.Type, obsidian.Type) },
  sessionState,
  sessionWorkItems);
LiquidPropagationAdvanceResult firstSessionAdvance = session.Advance(budget: 1);
if (!firstSessionAdvance.Succeeded ||
    firstSessionAdvance.ConsumedWorkItems != 1 ||
    firstSessionAdvance.PendingWorkItems == 0 ||
    session.PendingWorkItemCount != firstSessionAdvance.PendingWorkItems)
{
  throw new InvalidOperationException(
    "Liquid propagation session did not retain bounded pending work.");
}

LiquidPropagationCheckpoint sessionCheckpoint = session.CreateCheckpoint();
LiquidPropagationAdvanceResult uninterruptedSessionAdvance = session.Advance(budget: 3);
LiquidPropagationSession restartedSession = LiquidPropagationSession.Restore(
  sessionCheckpoint,
  new[] { water, lava, obsidian },
  new[] { new LiquidMergeComponent(water.Type, lava.Type, obsidian.Type) });
LiquidPropagationAdvanceResult restartedSessionAdvance = restartedSession.Advance(budget: 3);
if (!uninterruptedSessionAdvance.Succeeded ||
    !restartedSessionAdvance.Succeeded ||
    uninterruptedSessionAdvance.ConsumedWorkItems != restartedSessionAdvance.ConsumedWorkItems ||
    uninterruptedSessionAdvance.PendingWorkItems != restartedSessionAdvance.PendingWorkItems ||
    CreateSnapshotFingerprint(session.CreateSnapshot()) !=
      CreateSnapshotFingerprint(restartedSession.CreateSnapshot()))
{
  throw new InvalidOperationException(
    "Liquid propagation checkpoint restart was not deterministic.");
}

WorldGrid deduplicatedSessionWorld = new(replayWidth, replayHeight);
WorldGenerationStateComponent deduplicatedSessionState = new(46);
List<LiquidWorkItemComponent> deduplicatedWorkItems = new();
new LiquidSourceSystem().AppendWorkItems(
  new[] { liquidSource },
  liquidBounds,
  ref deduplicatedSessionState,
  deduplicatedWorkItems);
LiquidPropagationSession deduplicatedSession = new(
  deduplicatedSessionWorld,
  liquidMetadata,
  new[] { water },
  Array.Empty<LiquidMergeComponent>(),
  deduplicatedSessionState,
  deduplicatedWorkItems);
_ = deduplicatedSession.Advance(budget: 1);
_ = deduplicatedSession.Advance(budget: 8);
if (deduplicatedSession.CreateSnapshot().GetTile(liquidSource.X, liquidSource.Y).LiquidAmount !=
    byte.MaxValue)
{
  throw new InvalidOperationException(
    "Liquid propagation session revisited a committed source across budget ticks.");
}

WorldGrid rollbackWorld = new(replayWidth, replayHeight);
_ = rollbackWorld.TrySetLiquid(liquidSource.X, liquidSource.Y, amount: 100, lava.Type);
WorldGenerationStateComponent rollbackState = new(45);
List<LiquidWorkItemComponent> rollbackWorkItems = new();
new LiquidSourceSystem().AppendWorkItems(
  new[] { liquidSource },
  liquidBounds,
  ref rollbackState,
  rollbackWorkItems);
LiquidPropagationSession rollbackSession = new(
  rollbackWorld,
  liquidMetadata,
  new[] { water, lava },
  Array.Empty<LiquidMergeComponent>(),
  rollbackState,
  rollbackWorkItems);
string rollbackFingerprint = CreateSnapshotFingerprint(rollbackSession.CreateSnapshot());
long rollbackSequence = rollbackSession.State.NextSequence;
LiquidPropagationAdvanceResult rollbackResult = rollbackSession.Advance(budget: 1);
if (rollbackResult.Succeeded ||
    rollbackSession.PendingWorkItemCount != rollbackWorkItems.Count ||
    rollbackSession.State.NextSequence != rollbackSequence ||
    CreateSnapshotFingerprint(rollbackSession.CreateSnapshot()) != rollbackFingerprint)
{
  throw new InvalidOperationException(
    "Liquid propagation failure did not restore its checkpoint state.");
}

Console.WriteLine("PASS: bounded liquid propagation, explicit merge, and versioned liquid commit");

WorldGrid frameWorld = new(replayWidth, replayHeight);
_ = frameWorld.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 1));
WorldGridSnapshot frameSnapshot = frameWorld.CreateSnapshot(replayRequest.Metadata);
List<TileChangeCommand> pendingFrameChanges = new()
{
  new TileChangeCommand(0, 11, 10, TileChangeKind.Place, 1)
};
WorldGenerationStateComponent frameState = new(42);
List<TileFrameCommand> frameCommands = new();
IReadOnlyList<TileFrameRequest> frameRequests = TileFrameEvaluationQuery.CreateRequests(
  frameSnapshot,
  pendingFrameChanges,
  TileFrameMutationKind.TileChange);
if (frameRequests.Count == 0 || frameRequests[0].Source != "worldgen.frame")
{
  throw new InvalidOperationException(
    "Frame requests did not use the stable fallback provenance source.");
}
TileFrameEvaluationResult frameEvaluation = TileFrameEvaluationQuery.Evaluate(
  frameRequests.Single(request => request.X == 10 && request.Y == 10));
TileFrameEvaluationResult reverseOrderFrameEvaluation = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[]
    {
      new TileChangeCommand(1, 11, 10, TileChangeKind.Kill, 0),
      new TileChangeCommand(0, 11, 10, TileChangeKind.Place, 1)
    }));
TileFrameEvaluationResult forwardOrderFrameEvaluation = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[]
    {
      new TileChangeCommand(0, 11, 10, TileChangeKind.Place, 1),
      new TileChangeCommand(1, 11, 10, TileChangeKind.Kill, 0)
    }));
if (reverseOrderFrameEvaluation.IsSupported != forwardOrderFrameEvaluation.IsSupported ||
    reverseOrderFrameEvaluation.FrameX != forwardOrderFrameEvaluation.FrameX ||
    reverseOrderFrameEvaluation.FrameY != forwardOrderFrameEvaluation.FrameY ||
    reverseOrderFrameEvaluation.IsHalfBrick != forwardOrderFrameEvaluation.IsHalfBrick ||
    reverseOrderFrameEvaluation.Slope != forwardOrderFrameEvaluation.Slope ||
    reverseOrderFrameEvaluation.ShouldKill != forwardOrderFrameEvaluation.ShouldKill ||
    reverseOrderFrameEvaluation.Classification != forwardOrderFrameEvaluation.Classification ||
    !reverseOrderFrameEvaluation.AffectedCoordinates.SequenceEqual(
      forwardOrderFrameEvaluation.AffectedCoordinates))
{
  throw new InvalidOperationException(
    "Pending tile framing changed when mutation enumeration order changed.");
}
if (TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 1)) !=
      TileFrameClassificationKind.OrdinarySolid ||
    TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 520)) !=
      TileFrameClassificationKind.FoodPlatter ||
    TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 80)) !=
      TileFrameClassificationKind.Cactus ||
    TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 21)) !=
      TileFrameClassificationKind.MultiTileUnsupported ||
    TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 385)) !=
      TileFrameClassificationKind.CosmeticNoFrameChange ||
    TileFrameClassificationQuery.Classify(new WorldTile(IsActive: true, Type: 3)) !=
      TileFrameClassificationKind.FrameImportantUnsupported)
{
  throw new InvalidOperationException(
    "Tile frame classification did not preserve the migrated special-tile boundary.");
}

TileFrameEvaluationResult unsupportedSpecialFrame = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[] { new TileChangeCommand(0, 10, 10, TileChangeKind.UpdateTileType, 520) }));
if (unsupportedSpecialFrame.IsSupported)
{
  throw new InvalidOperationException(
    "Frame-important special tiles received generic frame evaluation.");
}

if (unsupportedSpecialFrame.Classification != TileFrameClassificationKind.FoodPlatter)
{
  throw new InvalidOperationException(
    "Tile frame evaluation did not preserve the special-tile classification.");
}

TileFrameEvaluationResult noOpCosmeticFrame = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[] { new TileChangeCommand(0, 10, 10, TileChangeKind.UpdateTileType, 385) }));
if (noOpCosmeticFrame.IsSupported ||
    noOpCosmeticFrame.Classification != TileFrameClassificationKind.CosmeticNoFrameChange)
{
  throw new InvalidOperationException(
    "Cosmetic no-op tile was incorrectly routed to generic framing.");
}

WorldGrid cosmeticNeighborWorld = new(replayWidth, replayHeight);
_ = cosmeticNeighborWorld.TrySetTile(30, 30, new WorldTile(IsActive: true, Type: 1));
_ = cosmeticNeighborWorld.TrySetTile(29, 30, new WorldTile(IsActive: true, Type: 5));
_ = cosmeticNeighborWorld.TrySetTile(31, 30, new WorldTile(IsActive: true, Type: 6, Slope: 2));
_ = cosmeticNeighborWorld.TrySetTile(30, 29, new WorldTile(IsActive: true, Type: 7));
TileCosmeticNeighborResult cosmeticNeighbors = TileCosmeticNeighborQuery.Evaluate(
  cosmeticNeighborWorld.CreateSnapshot(replayRequest.Metadata),
  30,
  30,
  new HashSet<ushort> { 5 });
if (cosmeticNeighbors.CanonicalTileType != 1 || cosmeticNeighbors.Left != 1 ||
    cosmeticNeighbors.Right != -1 ||
    cosmeticNeighbors.Up != 7 || cosmeticNeighbors.Down != -1)
{
  throw new InvalidOperationException(
    "Cosmetic neighbor projection did not preserve canonical types and slope boundaries.");
}

_ = cosmeticNeighborWorld.TrySetTile(30, 30, new WorldTile(IsActive: true, Type: 668));
TileCosmeticNeighborResult dirtAliasNeighbors = TileCosmeticNeighborQuery.Evaluate(
  cosmeticNeighborWorld.CreateSnapshot(replayRequest.Metadata),
  30,
  30,
  new HashSet<ushort>());
if (dirtAliasNeighbors.CanonicalTileType != 0)
{
  throw new InvalidOperationException("Tile 668 cosmetic alias did not normalize to dirt.");
}

_ = cosmeticNeighborWorld.TrySetTile(30, 30, new WorldTile(IsActive: true, Type: 697));
TileCosmeticNeighborResult snowAliasNeighbors = TileCosmeticNeighborQuery.Evaluate(
  cosmeticNeighborWorld.CreateSnapshot(replayRequest.Metadata),
  30,
  30,
  new HashSet<ushort>());
if (snowAliasNeighbors.CanonicalTileType != 51)
{
  throw new InvalidOperationException("Tile 697 cosmetic alias did not normalize to tile 51.");
}

_ = cosmeticNeighborWorld.TrySetTile(30, 30, new WorldTile(IsActive: true, Type: 5));
TileCosmeticNeighborResult stoneSourceNeighbors = TileCosmeticNeighborQuery.Evaluate(
  cosmeticNeighborWorld.CreateSnapshot(replayRequest.Metadata),
  30,
  30,
  new HashSet<ushort> { 5 });
if (stoneSourceNeighbors.CanonicalTileType != 1)
{
  throw new InvalidOperationException("Cosmetic stone source type did not normalize to tile 1.");
}

IReadOnlySet<ushort> cosmeticStoneTypes = new HashSet<ushort> { 5 };
TileCosmeticNeighborResult pendingCosmeticNeighbors = TileCosmeticNeighborQuery.Evaluate(
  cosmeticNeighborWorld.CreateSnapshot(replayRequest.Metadata),
  30,
  30,
  new[] { new TileChangeCommand(0, 31, 30, TileChangeKind.Place, 5) },
  cosmeticStoneTypes);
if (pendingCosmeticNeighbors.Right != 1)
{
  throw new InvalidOperationException(
    "Cosmetic neighbor projection did not apply pending tile mutations.");
}

TileFrameEvaluationResult wallOverlayFrame = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[] { new TileChangeCommand(0, 10, 10, TileChangeKind.SetWall, 0, WallType: 7) }));
if (wallOverlayFrame.Classification != TileFrameClassificationKind.OrdinarySolid)
{
  throw new InvalidOperationException("SetWall pending overlay changed tile classification unexpectedly.");
}

TileFrameEvaluationResult inactiveOverlayFrame = TileFrameEvaluationQuery.Evaluate(
  new TileFrameRequest(
    10,
    10,
    TileFrameMutationKind.TileChange,
    frameSnapshot,
    new[] { new TileChangeCommand(0, 10, 10, TileChangeKind.SetInactive, 0, IsInactive: true) }));
if (inactiveOverlayFrame.Classification != TileFrameClassificationKind.InactiveOrUnsupported)
{
  throw new InvalidOperationException(
    "SetInactive pending overlay changed tile classification unexpectedly.");
}

WorldGrid frameImportantWorld = new(replayWidth, replayHeight);
_ = frameImportantWorld.TrySetTile(90, 90, new WorldTile(IsActive: true, Type: 136));
_ = frameImportantWorld.TrySetTile(90, 91, new WorldTile(IsActive: true, Type: 1));
TileFrameImportant136Result frameImportantDown = TileFrameImportant136Query.Evaluate(
  frameImportantWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  90,
  90);
if (!frameImportantDown.IsSupported || frameImportantDown.FrameX != 0 ||
    frameImportantDown.ShouldKill)
{
  throw new InvalidOperationException("Tile 136 down-support framing diverged from legacy rules.");
}

_ = frameImportantWorld.TrySetTile(90, 91, default);
_ = frameImportantWorld.TrySetTile(89, 90, new WorldTile(IsActive: true, Type: 1));
TileFrameImportant136Result frameImportantLeft = TileFrameImportant136Query.Evaluate(
  frameImportantWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  90,
  90);
if (!frameImportantLeft.IsSupported || frameImportantLeft.FrameX != 18)
{
  throw new InvalidOperationException("Tile 136 left-support framing diverged from legacy rules.");
}

_ = frameImportantWorld.TrySetTile(89, 90, default);
TileFrameImportant136Result frameImportantKilled = TileFrameImportant136Query.Evaluate(
  frameImportantWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  90,
  90);
if (!frameImportantKilled.ShouldKill || frameImportantKilled.IsSupported)
{
  throw new InvalidOperationException("Tile 136 unsupported framing did not preserve kill intent.");
}

IReadOnlySet<ushort> beamDefaults = BeamTileRegistry.RegisterDefaults();
IReadOnlySet<ushort> treeTrunkDefaults = TreeTrunkTileRegistry.RegisterDefaults();
if (beamDefaults.Count != 7 || !beamDefaults.Contains(124) || !beamDefaults.Contains(578) ||
    treeTrunkDefaults.Count != 12 || !treeTrunkDefaults.Contains(5) ||
    !treeTrunkDefaults.Contains(634))
{
  throw new InvalidOperationException("Tile 136 support registries diverged from legacy TileID sets.");
}

Console.WriteLine("PASS: beam and tree-trunk support registries preserve shared legacy TileID sets");

WorldGrid mossFrameWorld = new(replayWidth, replayHeight);
_ = mossFrameWorld.TrySetTile(100, 100, new WorldTile(IsActive: true, Type: 184));
_ = mossFrameWorld.TrySetTile(100, 101, new WorldTile(IsActive: true, Type: 179));
TileFrameImportant184Result mossFrameDown = TileFrameImportant184Query.Evaluate(
  mossFrameWorld.CreateSnapshot(replayRequest.Metadata),
  100,
  100,
  currentFrameY: -1,
  randomState: new GenerationRandomState(11));
if (!mossFrameDown.IsSupported || mossFrameDown.FrameX != 0 ||
    mossFrameDown.FrameY is < 0 or > 36 || mossFrameDown.ShouldKill)
{
  throw new InvalidOperationException("Tile 184 down moss framing diverged from legacy rules.");
}

_ = mossFrameWorld.TrySetTile(100, 101, default);
_ = mossFrameWorld.TrySetTile(99, 100, new WorldTile(IsActive: true, Type: 180));
TileFrameImportant184Result mossFrameLeft = TileFrameImportant184Query.Evaluate(
  mossFrameWorld.CreateSnapshot(replayRequest.Metadata),
  100,
  100,
  currentFrameY: 120,
  randomState: new GenerationRandomState(11));
if (!mossFrameLeft.IsSupported || mossFrameLeft.FrameX != 22 ||
    mossFrameLeft.FrameY != 120)
{
  throw new InvalidOperationException("Tile 184 left moss framing did not preserve frame Y.");
}

_ = mossFrameWorld.TrySetTile(99, 100, default);
TileFrameImportant184Result mossFrameKilled = TileFrameImportant184Query.Evaluate(
  mossFrameWorld.CreateSnapshot(replayRequest.Metadata),
  100,
  100,
  currentFrameY: -1,
  randomState: new GenerationRandomState(11));
if (!mossFrameKilled.ShouldKill || mossFrameKilled.IsSupported)
{
  throw new InvalidOperationException("Tile 184 unsupported framing did not preserve kill intent.");
}

WorldGrid sandFrameWorld = new(replayWidth, replayHeight);
_ = sandFrameWorld.TrySetTile(110, 110, new WorldTile(IsActive: true, Type: 529));
_ = sandFrameWorld.TrySetTile(110, 111, new WorldTile(IsActive: true, Type: 53));
TileFrameImportant529Result sandFrameSupported = TileFrameImportant529Query.Evaluate(
  sandFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  110,
  110);
if (!sandFrameSupported.IsSupported || sandFrameSupported.ShouldKill)
{
  throw new InvalidOperationException("Tile 529 conversion-sand support diverged from legacy rules.");
}

_ = sandFrameWorld.TrySetTile(110, 111, new WorldTile(IsActive: true, Type: 1));
TileFrameImportant529Result sandFrameKilled = TileFrameImportant529Query.Evaluate(
  sandFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  110,
  110);
if (!sandFrameKilled.ShouldKill || sandFrameKilled.IsSupported)
{
  throw new InvalidOperationException("Tile 529 non-sand support did not preserve kill intent.");
}

IReadOnlySet<ushort> conversionSandDefaults = ConversionSandTileRegistry.RegisterDefaults();
if (conversionSandDefaults.Count != 4 ||
    !conversionSandDefaults.Contains(53) ||
    !conversionSandDefaults.Contains(112) ||
    !conversionSandDefaults.Contains(116) ||
    !conversionSandDefaults.Contains(234))
{
  throw new InvalidOperationException("Conversion-sand registry defaults diverged from legacy TileID sets.");
}

Console.WriteLine("PASS: conversion-sand registry preserves shared legacy TileID sets");

WorldGrid pileFrameWorld = new(replayWidth, replayHeight);
_ = pileFrameWorld.TrySetTile(120, 120, new WorldTile(IsActive: true, Type: 324));
_ = pileFrameWorld.TrySetTile(120, 121, new WorldTile(IsActive: true, Type: 1));
TileFrameImportant324Result pileFrameSupported = TileFrameImportant324Query.Evaluate(
  pileFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  120,
  120);
if (!pileFrameSupported.IsSupported || pileFrameSupported.ShouldKill)
{
  throw new InvalidOperationException("Tile 324 support framing diverged from legacy rules.");
}

_ = pileFrameWorld.TrySetTile(120, 121, new WorldTile(IsActive: true, Type: 665));
TileFrameImportant324Result pileFrameKilled = TileFrameImportant324Query.Evaluate(
  pileFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  120,
  120);
if (!pileFrameKilled.ShouldKill || pileFrameKilled.IsSupported)
{
  throw new InvalidOperationException("Tile 324 boulder support did not preserve kill intent.");
}

WorldGrid threeByOneWorld = new(replayWidth, replayHeight);
for (int offset = 0; offset < 3; offset++)
{
  _ = threeByOneWorld.TrySetTile(
    130 + offset,
    130,
    new WorldTile(
      IsActive: true,
      Type: 235,
      FrameX: checked((short)(offset * 18)),
      FrameY: 0));
  _ = threeByOneWorld.TrySetTile(130 + offset, 131, new WorldTile(IsActive: true, Type: 1));
}

Tile3x1ValidationResult threeByOneValid = Tile3x1ValidationQuery.Evaluate(
  threeByOneWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  131,
  130,
  235);
if (!threeByOneValid.IsValid || threeByOneValid.RequiresBreakabilityCheck ||
    threeByOneValid.OriginX != 130 || threeByOneValid.OriginY != 130)
{
  throw new InvalidOperationException("3x1 footprint validation did not preserve legacy origin rules.");
}

_ = threeByOneWorld.TrySetTile(
  131,
  130,
  new WorldTile(IsActive: true, Type: 235, FrameX: 18, FrameY: 18));
Tile3x1ValidationResult threeByOneInvalid = Tile3x1ValidationQuery.Evaluate(
  threeByOneWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  131,
  130,
  235);
if (threeByOneInvalid.IsValid || !threeByOneInvalid.RequiresBreakabilityCheck)
{
  throw new InvalidOperationException("3x1 invalid frame did not preserve deferred breakability.");
}

WorldGrid pileValidationWorld = new(replayWidth, replayHeight);
_ = pileValidationWorld.TrySetTile(
  140,
  140,
  new WorldTile(IsActive: true, Type: 185, FrameX: 36 * 18));
_ = pileValidationWorld.TrySetTile(140, 141, new WorldTile(IsActive: true, Type: 147));
TilePileValidationResult snowPile = TilePileValidationQuery.Evaluate(
  pileValidationWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  140,
  140,
  new HashSet<ushort> { 147 },
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>());
if (!snowPile.IsSupported || snowPile.ShouldKill || snowPile.RequiresTwoByOneCheck)
{
  throw new InvalidOperationException("Snow pile support validation diverged from legacy rules.");
}

_ = pileValidationWorld.TrySetTile(140, 141, new WorldTile(IsActive: true, Type: 1));
TilePileValidationResult invalidPile = TilePileValidationQuery.Evaluate(
  pileValidationWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  140,
  140,
  new HashSet<ushort> { 147 },
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>());
if (!invalidPile.ShouldKill || invalidPile.IsSupported)
{
  throw new InvalidOperationException("Invalid snow pile support did not preserve kill intent.");
}

_ = pileValidationWorld.TrySetTile(
  140,
  140,
  new WorldTile(IsActive: true, Type: 185, FrameX: 0, FrameY: 18));
TilePileValidationResult deferredPile = TilePileValidationQuery.Evaluate(
  pileValidationWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  140,
  140,
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>(),
  new HashSet<ushort>());
if (!deferredPile.RequiresTwoByOneCheck || deferredPile.ShouldKill)
{
  throw new InvalidOperationException("2x1 pile branch did not remain explicitly deferred.");
}

WorldGrid orbValidationWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 2; offsetX++)
{
  for (int offsetY = 0; offsetY < 2; offsetY++)
  {
    _ = orbValidationWorld.TrySetTile(
      170 + offsetX,
      170 + offsetY,
      new WorldTile(
        IsActive: true,
        Type: 12,
        FrameX: checked((short)(offsetX == 0 ? 0 : 36)),
        FrameY: checked((short)(offsetY == 0 ? 0 : 18))));
  }

  _ = orbValidationWorld.TrySetTile(170 + offsetX, 172, new WorldTile(IsActive: true, Type: 1));
}

TileOrbValidationResult orbValid = TileOrbValidationQuery.Evaluate(
  orbValidationWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  171,
  171,
  12);
if (!orbValid.IsValid || orbValid.ShouldKill || orbValid.OriginX != 170 || orbValid.OriginY != 170)
{
  throw new InvalidOperationException("2x2 orb footprint validation diverged from legacy rules.");
}

_ = orbValidationWorld.TrySetTile(171, 170, default);
TileOrbValidationResult orbInvalid = TileOrbValidationQuery.Evaluate(
  orbValidationWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  171,
  171,
  12);
if (orbInvalid.IsValid || !orbInvalid.ShouldKill)
{
  throw new InvalidOperationException("Invalid 2x2 orb footprint did not preserve kill intent.");
}

WorldGrid styledObjectWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 2; offsetX++)
{
  for (int offsetY = 0; offsetY < 2; offsetY++)
  {
    _ = styledObjectWorld.TrySetTile(
      180 + offsetX,
      180 + offsetY,
      new WorldTile(
        IsActive: true,
        Type: 254,
        FrameX: checked((short)(offsetX * 18 + 72)),
        FrameY: checked((short)(offsetY * 18))));
  }

  _ = styledObjectWorld.TrySetTile(180 + offsetX, 182, new WorldTile(IsActive: true, Type: 2));
}

Tile2x2StyleValidationResult styledObjectValid = Tile2x2StyleValidationQuery.Evaluate(
  styledObjectWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  181,
  181,
  254);
if (!styledObjectValid.IsValid || styledObjectValid.ShouldKill ||
    styledObjectValid.OriginX != 180 || styledObjectValid.OriginY != 180 ||
    styledObjectValid.StyleBand != 2)
{
  throw new InvalidOperationException(
    "2x2 style footprint validation diverged from legacy rules.");
}

_ = styledObjectWorld.TrySetTile(181, 182, new WorldTile(IsActive: true, Type: 1));
Tile2x2StyleValidationResult styledObjectInvalid = Tile2x2StyleValidationQuery.Evaluate(
  styledObjectWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  181,
  181,
  254);
if (styledObjectInvalid.IsValid || !styledObjectInvalid.ShouldKill)
{
  throw new InvalidOperationException(
    "Invalid 2x2 style support did not preserve kill intent.");
}

WorldGrid twoByOneWorld = new(replayWidth, replayHeight);
_ = twoByOneWorld.TrySetTile(
  190,
  190,
  new WorldTile(IsActive: true, Type: 16, FrameX: 36, FrameY: 18));
_ = twoByOneWorld.TrySetTile(
  191,
  190,
  new WorldTile(IsActive: true, Type: 16, FrameX: 54, FrameY: 18));
_ = twoByOneWorld.TrySetTile(190, 191, new WorldTile(IsActive: true, Type: 1));
_ = twoByOneWorld.TrySetTile(191, 191, new WorldTile(IsActive: true, Type: 1));
Tile2x1ValidationResult twoByOneValid = Tile2x1ValidationQuery.Evaluate(
  twoByOneWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  191,
  190,
  16,
  new HashSet<ushort>());
if (!twoByOneValid.IsValid || twoByOneValid.ShouldKill ||
    twoByOneValid.OriginX != 190 || twoByOneValid.OriginY != 190 ||
    twoByOneValid.StyleBand != 1)
{
  throw new InvalidOperationException("2x1 footprint validation diverged from legacy rules.");
}

Tile2x1ValidationResult twoByOnePile = Tile2x1ValidationQuery.Evaluate(
  twoByOneWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  191,
  190,
  185);
if (twoByOnePile.IsValid || twoByOnePile.ShouldKill ||
    !twoByOnePile.RequiresPileValidation)
{
  throw new InvalidOperationException(
    "2x1 pile validation did not preserve the deferred legacy branch.");
}

WorldGrid dyeFrameWorld = new(replayWidth, replayHeight);
_ = dyeFrameWorld.TrySetTile(150, 150, new WorldTile(IsActive: true, Type: 227));
_ = dyeFrameWorld.TrySetTile(150, 151, new WorldTile(IsActive: true, Type: 1));
TileDyeFrameResult dyeFrameDefault = TileDyeFrameQuery.Evaluate(
  dyeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  150,
  150,
  frameX: 0);
if (!dyeFrameDefault.IsSupported || dyeFrameDefault.ShouldKill)
{
  throw new InvalidOperationException("Default dye frame support diverged from legacy rules.");
}

_ = dyeFrameWorld.TrySetTile(150, 151, new WorldTile(IsActive: true, Type: 80));
TileDyeFrameResult dyeFrameCactus = TileDyeFrameQuery.Evaluate(
  dyeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  150,
  150,
  frameX: 204);
if (!dyeFrameCactus.IsSupported || dyeFrameCactus.ShouldKill)
{
  throw new InvalidOperationException("Cactus dye frame support diverged from legacy rules.");
}

_ = dyeFrameWorld.TrySetTile(150, 151, new WorldTile(IsActive: true, Type: 1));
TileDyeFrameResult dyeFrameKilled = TileDyeFrameQuery.Evaluate(
  dyeFrameWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  150,
  150,
  frameX: 204);
if (!dyeFrameKilled.ShouldKill || dyeFrameKilled.IsSupported)
{
  throw new InvalidOperationException("Invalid cactus dye frame did not preserve kill intent.");
}

WorldGrid rockGolemWorld = new(replayWidth, replayHeight);
_ = rockGolemWorld.TrySetTile(160, 160, new WorldTile(IsActive: true, Type: 579));
_ = rockGolemWorld.TrySetTile(160, 161, new WorldTile(IsActive: true, Type: 1));
RockGolemHeadFrameResult rockGolemSupported = RockGolemHeadFrameQuery.Evaluate(
  rockGolemWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  160,
  160);
if (!rockGolemSupported.IsSupported || rockGolemSupported.ShouldKill)
{
  throw new InvalidOperationException("Rock Golem head support diverged from legacy rules.");
}

_ = rockGolemWorld.TrySetTile(160, 161, default);
RockGolemHeadFrameResult rockGolemKilled = RockGolemHeadFrameQuery.Evaluate(
  rockGolemWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  160,
  160);
if (!rockGolemKilled.ShouldKill || rockGolemKilled.IsSupported)
{
  throw new InvalidOperationException("Unsupported Rock Golem head did not preserve kill intent.");
}

if (!frameEvaluation.IsSupported || frameEvaluation.FrameX != 18 ||
    frameEvaluation.FrameY != 0 || frameEvaluation.AffectedCoordinates.Count != 5)
{
  throw new InvalidOperationException(
    "Tile frame evaluation did not use the pending mutation overlay.");
}

if (!new TileFrameSystem().TryAppendCommands(
      frameSnapshot,
      pendingFrameChanges,
      ref frameState,
      frameCommands) ||
    frameCommands.Count == 0 ||
    frameSnapshot.GetTile(10, 10) != new WorldTile(IsActive: true, Type: 1) ||
    frameWorld.GetTile(11, 10) != default)
{
  throw new InvalidOperationException(
    "Tile framing changed state instead of producing frame commands.");
}

long frameSectionVersion = frameWorld.GetSectionVersion(
  frameWorld.GetSectionCoordinates(10, 10));
if (!new TileChangeCommitSystem().TryCommit(
      frameWorld,
      frameCommands,
      out TileFrameCommitResult frameCommit) ||
    frameCommit.AppliedCount != frameCommands.Count ||
    frameWorld.GetTile(10, 10).FrameX != 18 ||
    frameWorld.GetSectionVersion(frameWorld.GetSectionCoordinates(10, 10)) <=
      frameSectionVersion)
{
  throw new InvalidOperationException(
    "Tile frame commands did not commit through the tile change boundary.");
}

WorldGrid boundaryFrameWorld = new(replayWidth, replayHeight);
_ = boundaryFrameWorld.TrySetTile(0, 0, new WorldTile(IsActive: true, Type: 1));
List<TileFrameCommand> boundaryFrameCommands = new();
WorldGenerationStateComponent boundaryFrameState = new(43);
if (!new TileFrameSystem().TryAppendCommands(
      boundaryFrameWorld.CreateSnapshot(replayRequest.Metadata),
      new[] { new TileChangeCommand(0, 0, 1, TileChangeKind.Place, 1) },
      ref boundaryFrameState,
      boundaryFrameCommands) ||
    boundaryFrameCommands.Any(command => !boundaryFrameWorld.Contains(command.X, command.Y)) ||
    !new TileChangeCommitSystem().TryCommit(
      boundaryFrameWorld,
      boundaryFrameCommands,
      out TileFrameCommitResult boundaryFrameCommit) ||
    boundaryFrameCommit.AppliedCount != boundaryFrameCommands.Count)
{
  throw new InvalidOperationException(
    "Tile framing did not discard out-of-world neighbor commands.");
}

WorldGrid fourByTwoWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 4; offsetX++)
{
  for (int offsetY = 0; offsetY < 2; offsetY++)
  {
    _ = fourByTwoWorld.TrySetTile(
      30 + offsetX,
      30 + offsetY,
      new WorldTile(
        IsActive: true,
        Type: 79,
        FrameX: checked((short)(offsetX * 18)),
        FrameY: checked((short)(offsetY * 18))));
  }

  _ = fourByTwoWorld.TrySetTile(30 + offsetX, 32, new WorldTile(IsActive: true, Type: 1));
}

Tile4x2ValidationResult fourByTwoValid = Tile4x2ValidationQuery.Evaluate(
  fourByTwoWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  30,
  30,
  79);
if (!fourByTwoValid.IsValid || fourByTwoValid.ShouldKill ||
    fourByTwoValid.OriginX != 30 || fourByTwoValid.OriginY != 30)
{
  throw new InvalidOperationException("Valid 4x2 tile footprint was rejected.");
}

_ = fourByTwoWorld.TrySetTile(32, 32, default);
Tile4x2ValidationResult fourByTwoInvalid = Tile4x2ValidationQuery.Evaluate(
  fourByTwoWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  30,
  30,
  79);
if (fourByTwoInvalid.IsValid || !fourByTwoInvalid.ShouldKill)
{
  throw new InvalidOperationException("Invalid 4x2 tile support did not preserve kill intent.");
}

WorldGrid threeByFourWorld = new(replayWidth, replayHeight);
for (int offsetX = 0; offsetX < 3; offsetX++)
{
  for (int offsetY = 0; offsetY < 4; offsetY++)
  {
    _ = threeByFourWorld.TrySetTile(
      40 + offsetX,
      30 + offsetY,
      new WorldTile(
        IsActive: true,
        Type: 101,
        FrameX: checked((short)(offsetX * 18)),
        FrameY: checked((short)(offsetY * 18))));
  }

  _ = threeByFourWorld.TrySetTile(40 + offsetX, 34, new WorldTile(IsActive: true, Type: 1));
}

Tile3x4ValidationResult threeByFourValid = Tile3x4ValidationQuery.Evaluate(
  threeByFourWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  40,
  30,
  101);
if (!threeByFourValid.IsValid || threeByFourValid.ShouldKill ||
    threeByFourValid.OriginX != 40 || threeByFourValid.OriginY != 30)
{
  throw new InvalidOperationException("Valid 3x4 tile footprint was rejected.");
}

_ = threeByFourWorld.TrySetTile(41, 34, default);
Tile3x4ValidationResult threeByFourInvalid = Tile3x4ValidationQuery.Evaluate(
  threeByFourWorld.CreateSnapshot(replayRequest.Metadata),
  tileDefinitions,
  40,
  30,
  101);
if (threeByFourInvalid.IsValid || !threeByFourInvalid.ShouldKill)
{
  throw new InvalidOperationException("Invalid 3x4 tile support did not preserve kill intent.");
}

WorldGenerationStateComponent validationState = new(42);
_ = validationState.TryAdvance(WorldGenerationStage.Committed);
WorldGenerationValidationResult validationResult = new WorldGenerationValidationSystem().Validate(
  frameSnapshot,
  ref validationState,
  Array.Empty<LiquidWorkItemComponent>(),
  new[]
  {
    new StructurePlacementComponent(
      "starter-house",
      20,
      20,
      new StructureFootprintComponent(4, 3))
  });
if (!validationResult.Succeeded || !validationState.IsComplete)
{
  throw new InvalidOperationException("Valid world generation state did not reach validation.");
}

WorldGenerationStateComponent invalidValidationState = new(42);
WorldGenerationValidationResult invalidValidation = new WorldGenerationValidationSystem().Validate(
  frameSnapshot,
  ref invalidValidationState,
  new[] { new LiquidWorkItemComponent(-1, 0, 0, 1, 0) },
  Array.Empty<StructurePlacementComponent>());
if (invalidValidation.Succeeded || invalidValidation.FailureReason is null)
{
  throw new InvalidOperationException("Invalid world generation work was not rejected.");
}

WorldGenerationStateComponent unsettledValidationState = new(42);
_ = unsettledValidationState.TryAdvance(WorldGenerationStage.Committed);
WorldGenerationValidationResult unsettledValidation =
  new WorldGenerationValidationSystem().Validate(
    frameSnapshot,
    ref unsettledValidationState,
    new[] { new LiquidWorkItemComponent(20, 20, 0, 1, 1, "worldgen.liquid") },
    Array.Empty<StructurePlacementComponent>());
if (unsettledValidation.Succeeded ||
    unsettledValidation.FailureReason is null ||
    !unsettledValidation.FailureReason.Contains("pending work", StringComparison.Ordinal))
{
  throw new InvalidOperationException(
    "World generation validation accepted unsettled liquid work.");
}

Console.WriteLine(
  "PASS: frame commands, immutable snapshots, and generation validation boundaries");

WorldGrid runtimeWorld = new(4200, 1200);
DefaultWorldEnvironmentConvergence convergence =
  DefaultWorldEnvironmentConvergence.Create(runtimeWorld);
WorldEnvironmentTickSystem environmentTickSystem = new();
WorldRuleSnapshot ruleSnapshot = new(0, 0, true);
IReadOnlyList<WorldEnvironmentChange> runtimeChanges = Array.Empty<WorldEnvironmentChange>();
for (int tick = 0; tick < 5; tick++)
{
  runtimeChanges = environmentTickSystem.Advance(runtimeWorld, ruleSnapshot, convergence);
}

if (runtimeChanges.Count == 0 || runtimeWorld.GetTile(100, 400).LiquidAmount == 0)
{
  throw new InvalidOperationException(
    "Runtime environment systems did not continue from a world snapshot.");
}

Console.WriteLine("PASS: runtime environment tick consumes rule snapshot and bounded changes");

LegacyErrorWorldTileDefinitionRegistry errorWorldRegistry =
  LegacyErrorWorldTileDefinitionRegistry.RegisterDefaults();
if (errorWorldRegistry.Definitions.Count != TileDefinitionRegistry.Version4TileCount ||
    errorWorldRegistry.ByType.Count != TileDefinitionRegistry.Version4TileCount ||
    errorWorldRegistry.ByType[3].IsFrameImportant !=
      TileFrameImportantRegistry.Contains(3) ||
    errorWorldRegistry.ByType[41].IsDungeon != true ||
    errorWorldRegistry.ByType[1].IsSolid != true)
{
  throw new InvalidOperationException(
    "ErrorWorld TileID projection did not preserve the Version4 registry boundary.");
}

Console.WriteLine(
  "PASS: ErrorWorld TileID projection preserves the complete Version4 registry boundary");

WorldRuleState runtimeRules = new();
WorldRuntimeSnapshot incompleteRuntime = new(
  new WorldClockSnapshot(0, 0, true, false, 1),
  runtimeRules,
  generationCompleted: false,
  generationCompletionTick: -1);
WorldRuntimeTickSystem runtimeTickSystem = new();
if (runtimeTickSystem.Advance(incompleteRuntime, 4) != incompleteRuntime)
{
  throw new InvalidOperationException(
    "World runtime advanced before one-time generation completion.");
}

WorldRuntimeSnapshot completedRuntime = incompleteRuntime.CompleteGeneration();
WorldRuntimeSnapshot advancedRuntime = runtimeTickSystem.Advance(completedRuntime, 4);
if (!advancedRuntime.GenerationCompleted || advancedRuntime.Clock.TickNumber != 4 ||
    advancedRuntime.GenerationCompletionTick != 0)
{
  throw new InvalidOperationException(
    "World runtime did not advance deterministically after generation completion.");
}

WorldRuntimeSnapshot replayedRuntime = runtimeTickSystem.Advance(completedRuntime, 4);
if (advancedRuntime != replayedRuntime)
{
  throw new InvalidOperationException(
    "World runtime snapshot replay did not produce the same tick state.");
}

Console.WriteLine(
  "PASS: world runtime snapshot separates generation completion from Main Tick advancement");

static void VerifyTorchDefinitions()
{
  IReadOnlyList<TorchDefinition> first = TorchDefinitionRegistry.RegisterDefaults();
  IReadOnlyList<TorchDefinition> second = TorchDefinitionRegistry.RegisterDefaults();
  if (first.Count != 24 ||
      second.Count != first.Count ||
      !first[0].Equals(second[0]) ||
      !first[^1].Equals(second[^1]))
  {
    throw new InvalidOperationException("Torch definition count does not match the source registry.");
  }

  try
  {
    ((IList<TorchDefinition>)first)[0] = first[0];
    throw new InvalidOperationException("Torch definition projection was mutable.");
  }
  catch (NotSupportedException)
  {
  }

  if (!TorchDefinitionRegistry.TryGet(0, out TorchDefinition ordinary) ||
      ordinary.DustType != 6 || !ordinary.IsBiomeTorch)
  {
    throw new InvalidOperationException("The ordinary torch definition is incorrect.");
  }

  if (!TorchDefinitionRegistry.TryGet(23, out TorchDefinition shimmer) ||
      shimmer.DustType != 310 || !shimmer.IsBiomeTorch)
  {
    throw new InvalidOperationException("The shimmer torch definition is incorrect.");
  }

  if (TorchDefinitionRegistry.TryGet(-1, out _) ||
      TorchDefinitionRegistry.TryGet(24, out _))
  {
    throw new InvalidOperationException("Out-of-range torch IDs were accepted.");
  }

  Console.WriteLine("PASS: fixed TorchID definitions preserve source dust and biome facts");
}

static void VerifyTileFrameImportantRegistry()
{
  IReadOnlyList<ushort> first = TileFrameImportantRegistry.RegisterDefaults();
  IReadOnlyList<ushort> second = TileFrameImportantRegistry.RegisterDefaults();
  if (first.Count < 300 || !first.SequenceEqual(second) ||
      !TileFrameImportantRegistry.Contains(3) ||
      TileFrameImportantRegistry.Contains(1))
  {
    throw new InvalidOperationException("Tile framing defaults were not stable or bounded.");
  }

  try
  {
    ((IList<ushort>)first)[0] = first[0];
    throw new InvalidOperationException("Tile framing definition projection was mutable.");
  }
  catch (NotSupportedException)
  {
  }

  Console.WriteLine("PASS: tile framing defaults preserve ordered immutable registry semantics");
}

static void VerifyTileEntityDefinitions()
{
  string[] expectedNames =
  [
    "TrainingDummy",
    "ItemFrame",
    "LogicSensor",
    "DisplayDoll",
    "WeaponsRack",
    "HatRack",
    "FoodPlatter",
    "TeleportationPylon",
    "DeadCellsDisplayJar",
    "KiteAnchor",
    "CritterAnchor"
  ];
  if (TileEntityDefinitionRegistry.Definitions.Count != expectedNames.Length)
  {
    throw new InvalidOperationException("Tile-entity definition count does not match the source.");
  }

  try
  {
    ((IList<TileEntityDefinition>)TileEntityDefinitionRegistry.Definitions).Clear();
    throw new InvalidOperationException("Tile-entity definitions projection was mutable.");
  }
  catch (NotSupportedException)
  {
  }

  for (byte type = 0; type < expectedNames.Length; type++)
  {
    if (!TileEntityDefinitionRegistry.TryGet(type, out TileEntityDefinition definition) ||
        definition.Type != type || definition.Name != expectedNames[type])
    {
      throw new InvalidOperationException($"Tile-entity type {type} does not match source order.");
    }
  }

  if (TileEntityDefinitionRegistry.TryGet(11, out _))
  {
    throw new InvalidOperationException("An unregistered tile-entity type was accepted.");
  }

  Console.WriteLine("PASS: TileEntity.InitializeAll definitions preserve source registration order");
}

static MethodInventory CreateMethodInventory(MethodDeclarationSyntax method)
{
  string body = method.ToFullString();
  int line = method.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
  SourceMapping? mapping = CreateMethodMapping(method.Identifier.ValueText, line);
  return new MethodInventory(
    method.Identifier.ValueText,
    method.Modifiers.Any(SyntaxKind.PublicKeyword)
      ? "Public"
      : method.Modifiers.Any(SyntaxKind.InternalKeyword) ? "Internal" : "Private",
    line,
    Classify(body),
    mapping?.Status ?? "Unmapped",
    CreateReferences(body),
    mapping);
}

static SourceMapping? CreateMethodMapping(string name, int line)
{
  if (name == "IsItATrap" && line == 22015)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileWiringQuery.cs:IsItATrap"
      },
      "Classifies active actuated and mechanism tiles with explicit wiring definitions.",
      new[]
      {
        "Legacy TileID.Sets wiring ownership is supplied by the caller."
      },
      "Terraria.Dome.WorldGeneration.Verification tile wiring predicate checks.");
  }

  if (name == "IsItATrigger" && line == 22034)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileWiringQuery.cs:IsItATrigger"
      },
      "Classifies explicit wiring triggers and legacy pressure-plate frame rules.",
      new[]
      {
        "Legacy Minecart.IsPressurePlate behavior is supplied as explicit input."
      },
      "Terraria.Dome.WorldGeneration.Verification tile wiring predicate checks.");
  }

  if (name == "IsDungeonPlatformOrShelf" && line == 10533)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/DungeonPlatformQuery.cs:" +
        "IsPlatformOrShelf"
      },
      "Classifies dungeon platform and shelf frame columns from an immutable tile value.",
      new[]
      {
        "Legacy Tile reference ownership is represented by WorldTile value input."
      },
      "Terraria.Dome.WorldGeneration.Verification dungeon platform frame checks.");
  }

  if (name == "GenerateWorld_SetupDungeonGenVars" && line == 10096)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/DungeonGenerationState.cs:SetUp"
      },
      "Preserves explicit current-dungeon replacement and immutable context append/reset semantics.",
      new[]
      {
        "Dungeon pass scheduling and per-context generation fields remain deferred."
      },
      "Terraria.Dome.DungeonBounds.Verification immutable dungeon setup checks.");
  }

  if (name == "InitializeSecretSeeds" && line == 556)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "SecretSeedRuntimeProjection.cs:Create"
      },
      "Projects the six source-side secret-seed runtime flags from explicit enabled variants.",
      new[]
      {
        "WorldGenerationPipeline does not yet execute non-default secret-seed variants."
      },
      "Terraria.Dome.DungeonBounds.Verification secret-seed runtime projection checks.");
  }

  if (name == "FinalizeSecretSeeds" && line == 586)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacySecretSeedFinalizePolicy.cs:CreateActions"
      },
      "Builds source-ordered secret-seed finalization actions from explicit enabled variants.",
      new[]
      {
        "Action execution remains split across world-generation, liquid, and world-object owners."
      },
      "Terraria.Dome.DungeonBounds.Verification secret-seed finalization policy checks.");
  }

  if (name == "DoPaintEverythingGray" && line == 693)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyPaintEverythingGray.cs:AppendCommands"
      },
      "Emits source-ordered paint commands from immutable tiles, explicit treasure sets, " +
      "and pass-scoped random state.",
      new[]
      {
        "Secret-seed action scheduling and the legacy TileID ore/gem registry remain explicit " +
        "caller inputs."
      },
      "Terraria.Dome.WorldGeneration.Verification gray paint command checks.");
  }

  if (name == "DoPaintEverythingNegative" && line == 729)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyPaintEverythingNegative.cs:AppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyPaintEverythingNegativeProfile.cs:LegacyPaintEverythingNegativeProfile"
      },
      "Emits bounded selective or full negative-paint commands from immutable tiles, explicit " +
      "classification sets, and pass-scoped random state.",
      new[]
      {
        "Secret-seed action scheduling and legacy TileID registry ownership remain explicit " +
        "caller inputs."
      },
      "Terraria.Dome.WorldGeneration.Verification negative paint command checks.");
  }

  if (name == "DoCoatEverythingEcho" && line == 803)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyCoatEverythingEcho.cs:AppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyCoatEverythingEchoProfile.cs:LegacyCoatEverythingEchoProfile"
      },
      "Emits immutable-snapshot Echo coating and error-world cleanup commands through the " +
      "typed tile commit boundary.",
      new[]
      {
        "Legacy chest inventory injection remains a world-object responsibility and secret-seed " +
        "action scheduling remains deferred."
      },
      "Terraria.Dome.WorldGeneration.Verification Echo coating command checks.");
  }

  if (name == "DoCoatEverythingIlluminant" && line == 893)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyCoatEverythingIlluminant.cs:AppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyCoatEverythingIlluminantProfile.cs:LegacyCoatEverythingIlluminantProfile"
      },
      "Emits source-ordered selective, random, or full Illuminant coating commands from " +
      "immutable snapshots and a pass-scoped random state.",
      new[]
      {
        "Secret-seed action scheduling and legacy TileID registry ownership remain explicit " +
        "caller inputs."
      },
      "Terraria.Dome.WorldGeneration.Verification Illuminant coating command checks.");
  }

  if (name == "DoNoSurface" && line == 929)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/LegacyNoSurfacePolicy.cs:Create"
      },
      "Builds source-ordered underground meteor and spawn-randomization actions from explicit " +
      "world-rule inputs.",
      new[]
      {
        "Meteor, spawn, and torch action execution remain owned by their authoritative systems."
      },
      "Terraria.Dome.WorldGeneration.Verification no-surface action policy checks.");
  }

  if (name == "DoErrorWorldGetRandomBlock" && line == 980)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldRandomBlock.cs:TrySelect"
      },
      "Selects an eligible ErrorWorld tile from explicit immutable tile definitions using " +
      "pass-scoped rejection sampling.",
      new[]
      {
        "The full TileID registry and the consuming ErrorWorld block-shuffle pass remain " +
        "deferred."
      },
      "Terraria.Dome.WorldGeneration.Verification ErrorWorld random-block source checks.");
  }

  if (name == "DoErrorWorldShuffleBlocks" && line == 992)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldTileSwap.cs:AppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldTileRectangleSwap.cs:AppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldCandidateQuery.cs:IsSwapEligible",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldCandidateSelector.cs:TrySelect",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldSpawnExclusionPolicy.cs:IsSingleTileExcluded",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldSpawnExclusionPolicy.cs:IsRectangleExcluded",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldSwapOperation.cs:TryAppendSingleTileSwap",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldSwapOperation.cs:TryAppendRectangleSwap",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldAxisTrail.cs:AppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldAxisTrailPolicy.cs:Create",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldPassCountPolicy.cs:Create",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldRandomBlockRewrite.cs:TryAppendCommand",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldShufflePass.cs:AppendCommands"
      },
      "Selects source-eligible candidates, applies spawn exclusions and pass counts, and emits " +
      "bounded rewrite, single-tile, non-overlapping rectangle, and axial trail commands.",
      new[]
      {
        "Full TileID registry projection, overlapping rectangle ordering, and pipeline scheduling " +
        "remain deferred."
      },
      "Terraria.Dome.WorldGeneration.Verification ErrorWorld single-tile swap source checks.");
  }

  if (name == "DoErrorWorldFinish" && line == 1201)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldFinishCleanup.cs:AppendCommands"
      },
      "Emits bounded immutable-snapshot cleanup commands for cracked bricks, moss, walls, " +
      "crystal blocks, and unwired statue frames.",
      new[]
      {
        "Chest swaps, wire routing, liquid pool placement, killability rules, NPC replacement, " +
        "and full random-consumption parity remain deferred."
      },
      "Terraria.Dome.WorldGeneration.Verification ErrorWorld finish cleanup checks.");
  }

  if (name == "DoErrorWorldFindChestItem" && line == 1463)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyErrorWorldChestItemPolicy.cs:SelectItemType"
      },
      "Selects the source-defined ErrorWorld chest item with a single pass-scoped random draw.",
      new[]
      {
        "Chest placement and the complete ErrorWorld chest-swap transaction remain deferred."
      },
      "Terraria.Dome.WorldGeneration.Verification ErrorWorld chest-item policy checks.");
  }

  if (name == "DoExtraLiquidAddLiquid" && line == 1508)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyExtraLiquidAddLiquid.cs:AppendCommands"
      },
      "Emits source-ordered bounded liquid commands from an immutable world snapshot.",
      new[]
      {
        "Bubble-block placement, QuickWater scheduling, and the full ExtraLiquid finish pass " +
        "remain deferred."
      },
      "Terraria.Dome.WorldGeneration.Verification ExtraLiquid command checks.");
  }

  if (name == "DoExtraLiquidAddBubbleBlocks" && line == 1558)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyExtraLiquidAddBubbleBlocks.cs:AppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyExtraLiquidPassExecution.cs:TryExecuteBubbleBlocks"
      },
      "Emits and commits deterministic bounded bubble-square, liquid-clear, and fullbright commands.",
      new[]
      {
        "QuickWater scheduling, landmass persistence, and complete source random-consumption parity " +
        "remain deferred."
      },
      "Terraria.Dome.WorldGeneration.Verification ExtraLiquid bubble-block checks.");
  }

  if (name == "DoExtraLiquidFinish" && line == 1744)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyExtraLiquidFinish.cs:AppendCommands"
      },
      "Emits bounded fullbright cleanup, underworld lava, and tile-removal commands.",
      new[]
      {
        "QuickWater scheduling and tile 518 frame refresh remain deferred to liquid and frame " +
        "systems."
      },
      "Terraria.Dome.WorldGeneration.Verification ExtraLiquid finish command checks.");
  }

  if (name == "DoRainsForAYear" && line == 1788)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/LegacyRainsForAYearPolicy.cs:Apply"
      },
      "Projects the source rain duration, raining state, and cloud target into a runtime result.",
      new[]
      {
        "Runtime command scheduling and cloud-system ownership remain deferred."
      },
      "Terraria.Dome.WorldGeneration.Verification rains-for-a-year runtime policy checks.");
  }

  if (name == "DoRandomSpawn" && line == 1798)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/LegacyRandomSpawnPolicy.cs:Create"
      },
      "Builds the source one-time random-spawn and torch action plan.",
      new[]
      {
        "Spawn randomization and torch placement command execution remain deferred."
      },
      "Terraria.Dome.WorldGeneration.Verification random-spawn policy checks.");
  }

  if (name == "DoAddTeleporters" && line == 1809)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyTeleporterPlacementQuery.cs:CanPlace",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyTeleporterClearArea.cs:TryAppendCommands"
      },
      "Evaluates source candidate gates and emits bounded clear-area tile commands.",
      new[]
      {
        "Pair selection, tile placement, wiring, and source retry scheduling remain deferred."
      },
      "Terraria.Dome.WorldGeneration.Verification teleporter candidate checks.");
  }

  if (name == "DoStartInHardmode" && line == 1997)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyStartInHardmodePolicy.cs:Apply"
      },
      "Projects source hardmode enablement and initialization intent into frozen generation rules.",
      new[]
      {
        "WorldProgression hardmode initialization and its gameplay side effects remain deferred."
      },
      "Terraria.Dome.WorldGeneration.Verification start-in-hardmode policy checks.");
  }

  if (name == "DoNoInfection" && line == 2005)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyNoInfectionPolicy.cs:GetConversion" },
      "Classifies source tile and wall conversion exclusions from immutable tile state.",
      new[] { "World conversion command execution remains deferred." },
      "Terraria.Dome.WorldGeneration.Verification no-infection policy checks.");
  }

  if (name == "DoHallowOnSurface" && line == 2052)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyHallowOnSurface.cs:AppendCommands" },
      "Emits bounded surface hallow tile and wall conversion commands.",
      new[] { "Pipeline scheduling and full conversion parity remain deferred." },
      "Terraria.Dome.WorldGeneration.Verification hallow-on-surface checks.");
  }

  if (name == "DoWorldIsInfected" && line == 2121)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyWorldInfectionPolicy.cs:CreateColumn" },
      "Builds deterministic source-style infection-column inputs and conversion exclusions.",
      new[] { "World conversion command execution remains deferred." },
      "Terraria.Dome.WorldGeneration.Verification infection policy checks.");
  }

  if (name == "DoSurfaceIsInSpace" && line == 2177)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacySurfaceIsInSpace.cs:AppendCommands" },
      "Emits bounded surface cloud-wall cleanup commands.",
      new[] { "Pipeline scheduling remains deferred." },
      "Terraria.Dome.WorldGeneration.Verification surface-in-space checks.");
  }

  if (name == "DoSurfaceIsMushrooms" && line == 2197)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacySurfaceIsMushrooms.cs:AppendNormalModeCommands" },
      "Emits bounded normal-mode surface mushroom conversion commands.",
      new[] { "Secret-seed mode variants and pipeline scheduling remain deferred." },
      "Terraria.Dome.WorldGeneration.Verification surface-mushroom checks.");
  }

  if (name == "DoWorldIsFrozenFinish" && line == 2287)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyWorldIsFrozenFinishPolicy.cs:CreateNpcReplacementIntents" },
      "Creates bounded frozen-world chest and NPC replacement intents.",
      new[] { "Chest and NPC command execution remains deferred." },
      "Terraria.Dome.WorldGeneration.Verification frozen-finish intent checks.");
  }

  if (name == "DoWorldIsFrozen" && line == 2330)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyWorldIsFrozen.cs:AppendCommands" },
      "Emits bounded frozen-world tile and wall conversion commands.",
      new[] { "Pipeline scheduling and complete secret-seed interaction parity remain deferred." },
      "Terraria.Dome.WorldGeneration.Verification world-is-frozen checks.");
  }

  if (name == "DoSurfaceIsDesertNoSurfaceCleanup" && line == 2709)
  {
    return new SourceMapping("Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacySurfaceIsDesertNoSurfaceCleanup.cs:AppendCommands" },
      "Emits bounded surface-desert no-surface cleanup commands.",
      new[] { "Pipeline scheduling remains deferred." },
      "Terraria.Dome.WorldGeneration.Verification desert cleanup checks.");
  }

  if (name == "DoNoSpiderCavesILiedMoreSpiderCaves" && line == 2736)
  {
    return new SourceMapping("Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyNoSpiderCavesCleanup.cs:AppendCommands" },
      "Emits bounded spider-wall cleanup commands.",
      new[] { "Pipeline scheduling remains deferred." },
      "Terraria.Dome.WorldGeneration.Verification spider cleanup checks.");
  }

  if (name == "DoActuallyNoTraps" && line == 2752)
  {
    return new SourceMapping("Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyActuallyNoTraps.cs:AppendHardModeCommands" },
      "Emits bounded hardmode trap-removal commands.",
      new[] { "Pipeline scheduling remains deferred." },
      "Terraria.Dome.WorldGeneration.Verification actually-no-traps checks.");
  }

  if (name == "DoRainbowStuff" && line == 2817)
  {
    return new SourceMapping("Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyRainbowStaticRewrite.cs:AppendCommands" },
      "Emits bounded static rainbow tile and wall rewrite commands.",
      new[] { "Dynamic rainbow and pipeline scheduling remain deferred." },
      "Terraria.Dome.WorldGeneration.Verification rainbow static rewrite checks.");
  }

  if (name == "DoDigExtraHoles" && line == 2957)
  {
    return new SourceMapping("Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyDigExtraHoles.cs:CreateInvocations" },
      "Builds deterministic source TileRunner invocation inputs.",
      new[] { "Pipeline scheduling remains deferred." },
      "Terraria.Dome.WorldGeneration.Verification dig-extra-holes checks.");
  }

  if (name == "DoRoundLandMasses" && line == 2976)
  {
    return new SourceMapping("Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyRoundLandmassSeedDefinitions.cs:Create" },
      "Builds deterministic source landmass seed definitions.",
      new[] { "Landmass command execution remains deferred." },
      "Terraria.Dome.WorldGeneration.Verification round-landmass checks.");
  }

  if (name == "DoPooEverywhere" && line == 3083)
  {
    return new SourceMapping("Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyPooEverywherePolicy.cs:GetAttemptCount" },
      "Calculates the bounded source placement attempt count.",
      new[] { "Placement execution remains deferred." },
      "Terraria.Dome.WorldGeneration.Verification poo policy checks.");
  }

  if (name == "DoPortalGunInChests" && line == 3114)
  {
    return new SourceMapping("Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/LegacyPortalGunChestPolicy.cs:TryCreateIntent" },
      "Creates bounded portal-gun chest item intents.",
      new[] { "Chest command execution remains deferred." },
      "Terraria.Dome.WorldGeneration.Verification portal-gun chest checks.");
  }

  if (name == "DisablePassesForSpecialSeeds" && line == 21674)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyDualDungeonPassPolicy.cs:ShouldDisable"
      },
      "Classifies the source-defined pass names disabled by the explicit dual-dungeons rule.",
      new[]
      {
        "WorldGenerationPipeline does not yet schedule non-default secret-seed pass definitions."
      },
      "Terraria.Dome.DungeonBounds.Verification dual-dungeon pass policy checks.");
  }

  if (name == "IsSurfaceForAtmospherics" && line == 10033)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/AtmosphericSurfaceQuery.cs:" +
        "IsSurfaceForAtmospherics"
      },
      "Classifies normal and remix atmospheric surface heights from an explicit profile.",
      new[]
      {
        "Legacy Main remixWorld, worldSurface, rockLayer and maxTilesY fields become " +
        "explicit profile input."
      },
      "Terraria.Dome.WorldGeneration.Verification atmospheric surface checks.");
  }

  if (name == "CanGeneratePressurePlateAt" && line == 10072)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PressurePlatePlacementQuery.cs:" +
        "CanGenerateAt"
      },
      "Checks bounded placement support, boulder classification, and forbidden-wall rules.",
      new[]
      {
        "Legacy TileID.Sets.Boulders ownership is supplied as an explicit definition input."
      },
      "Terraria.Dome.WorldGeneration.Verification pressure plate placement checks.");
  }

  if (name == "IsBelowANonHammeredPlatform" && line == 31812)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlatformSupportQuery.cs:" +
        "IsBelowANonHammeredPlatform"
      },
      "Classifies active platform support without half-brick or slope state.",
      new[]
      {
        "Legacy TileID.Sets.Platforms ownership is supplied as an explicit collection."
      },
      "Terraria.Dome.WorldGeneration.Verification non-hammered platform checks.");
  }

  if (name == "GetItemDrop_Candles" && line == 32845)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/CandleItemDropQuery.cs:ToItem"
      },
      "Maps candle tile styles to item identifiers using the legacy deterministic switch.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification candle item-drop mapping checks.");
  }

  if (name == "GetItemDrop_PicnicTables" && line == 33374)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PicnicTableItemDropQuery.cs:ToItem"
      },
      "Maps picnic-table style one to its item identifier and all other styles to default.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification picnic-table item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Bottles" && line == 34364)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/BottleItemDropQuery.cs:ToItem"
      },
      "Maps bottle tile styles to item identifiers using the legacy deterministic switch.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification bottle item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Benches" && line == 33296)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/BenchItemDropQuery.cs:ToItem"
      },
      "Maps bench tile styles to item identifiers using the legacy deterministic switch.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification bench item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Clocks" && line == 33214)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/ClockItemDropQuery.cs:ToItem"
      },
      "Maps clock tile styles to item identifiers using legacy ranges and switch branches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification clock item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Beds" && line == 33028)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/BedItemDropQuery.cs:ToItem"
      },
      "Maps bed tile styles to item identifiers using legacy ranges and switch branches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification bed item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Candelabras" && line == 33385)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/CandelabraItemDropQuery.cs:ToItem"
      },
      "Maps candelabra tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification candelabra item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Bookcases" && line == 33567)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/BookcaseItemDropQuery.cs:ToItem"
      },
      "Maps bookcase tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification bookcase item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Chandeliers" && line == 33769)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/ChandelierItemDropQuery.cs:ToItem"
      },
      "Maps chandelier tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification chandelier item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Lanterns" && line == 33968)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/LanternItemDropQuery.cs:ToItem"
      },
      "Maps lantern tile styles to item identifiers, including legacy negative-style behavior.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification lantern item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Lamps" && line == 34184)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/LampItemDropQuery.cs:ToItem"
      },
      "Maps lamp tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification lamp item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Pianos" && line == 34399)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PianoItemDropQuery.cs:ToItem"
      },
      "Maps piano tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification piano item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Sinks" && line == 34572)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/SinkItemDropQuery.cs:ToItem"
      },
      "Maps sink tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification sink item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Tables" && line == 35240)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TableItemDropQuery.cs:ToItem"
      },
      "Maps first and second table tile styles using explicit legacy input branches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification table item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Bathtubs" && line == 35433)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/BathtubItemDropQuery.cs:ToItem"
      },
      "Maps bathtub tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification bathtub item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Workbenches" && line == 35602)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/WorkbenchItemDropQuery.cs:ToItem"
      },
      "Maps workbench tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification workbench item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Chair" && line == 35792)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/ChairItemDropQuery.cs:ToItem"
      },
      "Maps chair tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification chair item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Toilet" && line == 35932)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/ToiletItemDropQuery.cs:ToItem"
      },
      "Maps toilet tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification toilet item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Platforms" && line == 36046)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlatformItemDropQuery.cs:ToItem"
      },
      "Maps platform tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification platform item-drop mapping checks.");
  }

  if (name == "GetItemDrop_MusicBoxes" && line == 36259)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/MusicBoxItemDropQuery.cs:ToItem"
      },
      "Maps music-box tile styles to item identifiers, including negative-style behavior.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification music-box item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Dressers" && line == 42757)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/DresserItemDropQuery.cs:ToItem"
      },
      "Maps dresser tile styles to item identifiers using legacy ranges and switches.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification dresser item-drop mapping checks.");
  }

  if (name == "GetItemDrop_Chests" && line == 34701)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/ChestItemDropQuery.cs:ToItem"
      },
      "Maps first and second chest tile styles using explicit legacy input branches.",
      new[]
      {
        "The private coordinate wrapper reads Main.tile and remains unmapped.",
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification chest item-drop mapping checks.");
  }

  if (name == "GetItemDrop_FakeChests" && line == 34990)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/FakeChestItemDropQuery.cs:ToItem"
      },
      "Maps first and second fake-chest tile styles, including explicit no-drop values.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification fake-chest item-drop mapping checks.");
  }

  if (name == "GetCampfireItemDrop" && line == 42943)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/CampfireItemDropQuery.cs:ToItem"
      },
      "Maps campfire tile styles to item identifiers using the bounded legacy switch rules.",
      new[]
      {
        "Legacy item registry and tile-frame ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification campfire item-drop mapping checks.");
  }

  if (name == "GetRainbowPaintIDForPosition" && line == 21860)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/RainbowPaintQuery.cs:ToPaintId"
      },
      "Maps direct and wiggly coordinate paint identifiers using the legacy arithmetic.",
      new[]
      {
        "Legacy tile and wall mutation remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification rainbow paint mapping checks.");
  }

  if (name == "IsLockedDungeonBiomeChest" && line == 29381)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/DungeonChestQuery.cs:" +
        "IsLockedBiomeChest"
      },
      "Maps locked dungeon biome chest type and style classification.",
      new[]
      {
        "Legacy chest storage and tile ownership remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification locked dungeon chest mapping checks.");
  }

  if (name == "GetPileGenerationAttempts" && line == 21715)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PileGenerationAttemptPolicy.cs:" +
        "GetAttempts"
      },
      "Maps pile attempts from explicit world width and skyblock rule inputs.",
      new[]
      {
        "Legacy Main.maxTilesX and skyblockWorldGen reads remain outside this pure mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification pile generation attempt policy checks.");
  }

  if (name == "PlantCheck_TryGetNewType" && line == 67430)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlantTypeConversionQuery.cs:Evaluate"
      },
      "Maps plant tile type, frame, and mushroom conversion from explicit tile inputs.",
      new[]
      {
        "Legacy PlantCheck neighborhood scans, slope rules, tile mutation, and destruction remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification plant type conversion checks.");
  }

  if (name == "PlantCheck_CanPlaceHook" && line == 67332)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlantPlacementQuery.cs:CanPlace"
      },
      "Maps plant support eligibility from an immutable snapshot and tile definitions.",
      new[]
      {
        "Legacy mutable Tile access and placement-hook integration remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification plant placement checks.");
  }

  if (name == "PlantCheck" && line == 67360)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlantCheckQuery.cs:Evaluate",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/PlantCheckCommandSystem.cs:" +
        "TryCreateCommand",
        "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
      },
      "Maps clamped plant support, conversion, destruction, and atomic type/frame command intent.",
      new[]
      {
        "Legacy nullable 3x3 tile scan and KillTile side effects remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification plant check command checks.");
  }

  if (name == "PlantCheck_IsBadTypeMatch" && line == 67434)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/PlantTypeConversionQuery.cs:" +
        "IsBadTypeMatch"
      },
      "Maps plant and support tile compatibility classification.",
      new[]
      {
        "Legacy PlantCheck neighborhood scans, slope rules, tile mutation, and destruction remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification plant type conversion checks.");
  }

  if (name == "CanPoundTile" && line == 67449)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TilePoundingEligibilityQuery.cs:" +
        "CanPound"
      },
      "Maps local pound eligibility from a snapshot, boulder definitions, and CanKillTile input.",
      new[]
      {
        "Legacy mutable Tile initialization and full CanKillTile ownership remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification tile pounding eligibility checks.");
  }

  if (name == "ForbidsSloping" && line == 67501)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileSlopingQuery.cs:ForbidsSloping"
      },
      "Maps the bounded legacy tile-type table that forbids sloping below active tiles.",
      new[]
      {
        "Legacy mutable Tile coordinate access is represented by an explicit tile-type input."
      },
      "Terraria.Dome.WorldGeneration.Verification tile sloping checks.");
  }

  if (name == "SlopeTile" && line == 67526)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/TilePoundingCommandSystem.cs:" +
        "TryCreateSlopeCommand",
        "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
      },
      "Maps accepted slope changes to atomic shape commands.",
      new[]
      {
        "Legacy effects, player movement, framing, and network broadcast remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification tile slope command checks.");
  }

  if (name == "PoundTile" && line == 67565)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/TilePoundingCommandSystem.cs:" +
        "TryCreatePoundCommand",
        "src/Terraria.Dome.Simulation/World/Systems/TileChangeCommitSystem.cs:TryCommit"
      },
      "Maps accepted half-brick toggles to atomic shape commands.",
      new[]
      {
        "Legacy effects, player movement, framing, and network broadcast remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification tile pound command checks.");
  }

  if (name == "TileMergeAttemptFrametest" && line is 67602 or 67656)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ApplyFrametest"
      },
      "Maps neighbor rewrite and cardinal frame-work decisions from explicit values.",
      new[]
      {
        "Legacy TileFrame side effects and raw bool-array ownership remain outside this mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification tile merge query checks.");
  }

  if (name == "TileMergeAttempt" && line is 67710 or 67732)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceCardinal"
      },
      "Maps four-neighbor replacement from explicit type or type-set inputs.",
      new[]
      {
        "Legacy raw bool-array ownership becomes an explicit set."
      },
      "Terraria.Dome.WorldGeneration.Verification tile merge query checks.");
  }

  if (name == "TileMergeAttempt" && line is 67754 or 67792)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAll"
      },
      "Maps eight-neighbor replacement from explicit type or type-set inputs.",
      new[]
      {
        "Legacy raw bool-array ownership becomes an explicit set."
      },
      "Terraria.Dome.WorldGeneration.Verification tile merge query checks.");
  }

  if (name == "TileMergeAttempt" && line is 67830 or 67868)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:ReplaceAllExcept"
      },
      "Maps eight-neighbor replacement with explicit excluded type inputs.",
      new[]
      {
        "Legacy raw bool-array ownership becomes explicit sets."
      },
      "Terraria.Dome.WorldGeneration.Verification tile merge query checks.");
  }

  if (name == "TileMergeAttemptWeird" && line == 67906)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileMergeQuery.cs:" +
        "ReplaceDifferentExcept"
      },
      "Maps eight-neighbor replacement for non-excluded types distinct from the source type.",
      new[]
      {
        "Legacy raw bool-array ownership becomes an explicit set."
      },
      "Terraria.Dome.WorldGeneration.Verification tile merge query checks.");
  }

  if (name == "GetTileMossColor" && line == 67944)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/MossColorQuery.cs:GetColor"
      },
      "Maps the bounded legacy moss tile-type table to moss color identifiers.",
      Array.Empty<string>(),
      "Terraria.Dome.WorldGeneration.Verification moss color checks.");
  }

  if (name == "StatueStyleToItem" && line == 31437)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/StatueStyleItemQuery.cs:ToItem"
      },
      "Maps statue styles to item IDs with the bounded legacy switch table.",
      new[]
      {
        "Legacy item registry ownership and statue placement side effects remain excluded."
      },
      "Terraria.Dome.WorldGeneration.Verification statue style mapping checks.");
  }

  if (name == "OreHelper" && line == 9263)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/LegacyOreHelper.cs:AppendCommands"
      },
      "Converts the bounded legacy three-by-three type 1 and 40 cleanup into " +
      "snapshot-derived tile commands.",
      new[]
      {
        "Legacy caller scheduling, immediate mutable tile writes, and frame side effects."
      },
      "Terraria.Dome.WorldGeneration.Verification OreHelper source checks.");
  }

  if (name == "OrePatch" && line == 9624)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "OrePatchEligibilityQuery.cs:Evaluate",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrePatchPlacementSystem.cs:TryPrepare",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrePatchPlacementSystem.cs:TryAppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrePatchPlacementSystem.cs:TryAppendLegacyTrailAndBlobCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyOrePatchTypePolicy.cs:SelectTileType",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyOreTierSelectionPolicy.cs:Select",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyOrePatchTrail.cs:TryAppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LegacyOrePatchBlob.cs:TryAppendCommands"
      },
      "Composes bounded OrePatch eligibility, tier selection, trail/blob traversal, and a " +
      "conflict-safe prepare/command transaction from immutable snapshots.",
      new[]
      {
        "World-rule tier selection projection into generation scheduling.",
        "SquareTileFrame and mutable Tile ownership."
      },
      "Terraria.Dome.WorldGeneration.Verification ore patch, tier, trail/blob, and command checks.");
  }

  if (name == "OreRunner" && line == 41739)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/LegacyOreRunner.cs:AppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/MossTileTypeRegistry.cs:TileTypes"
      },
      "Renders bounded deterministic ore tile and wall commands from immutable snapshots, " +
      "including source-backed moss and special replacement filters.",
      new[]
      {
        "Paint, framing, network side effects, and legacy caller scheduling."
      },
      "Terraria.Dome.WorldGeneration.Verification OreRunner source checks.");
  }

  if (name == "TreeGrowFXCheck" && line == 23974)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeLeafScanQuery.cs:Scan"
      },
      "Scans immutable snapshot tree tiles and computes leaf pass-style inputs without " +
      "network dispatch.",
      new[]
      {
        "Legacy Main.tile ownership and TileID.Sets.GetsCheckedForLeaves lookup.",
        "NetMessage.SendData tree-growth FX dispatch."
      },
      "Terraria.Dome.WorldGeneration.Verification tree leaf snapshot scan.");
  }

  if (name == "GetTreeLeaf" && line == 24008)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeLeafPassStyleQuery.cs:Evaluate"
      },
      "Computes tree frame, foliage pass style, and hollow-tree height increment from " +
      "explicit Tile values and foliage-style input.",
      new[]
      {
        "Legacy GetHollowTreeFoliageStyle global lookup.",
        "TreeGrowFXCheck network dispatch and mutable Tile ownership."
      },
      "Terraria.Dome.WorldGeneration.Verification tree leaf pass-style checks.");
  }

  if (name == "GrowUndergroundTree" && line == 25416)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "UndergroundTreeGrowthEligibilityQuery.cs:Evaluate",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "UndergroundTreeHeightPolicy.cs:Next",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "UndergroundTreeTrunkCommandSystem.cs:TryAppendCommands"
      },
      "Checks bounded underground-tree eligibility, selects deterministic height, and emits " +
      "command-only trunk placement from an immutable snapshot.",
      new[]
      {
        "Legacy genRand stream parity and random branch selection.",
        "Branch mutation, frame updates, RangeFrame, and network side effects."
      },
      "Terraria.Dome.WorldGeneration.Verification underground tree eligibility checks.");
  }

  if (name == "IsTileALeafyTreeTop" && line == 24227)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeLeafFrameQuery.cs:IsLeafyTreeTop"
      },
      "Classifies active leafy-tree top frames against an explicit immutable type set.",
      new[]
      {
        "Legacy TileID.Sets.GetsCheckedForLeaves ownership and CallTracker instrumentation."
      },
      "Terraria.Dome.WorldGeneration.Verification leafy tree top frame checks.");
  }

  if (name == "IsTileATreeBranch" && line == 24269)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeTrunkFrameQuery.cs:TryGetBranchOffset"
      },
      "Classifies legacy tree branch frames against an explicit immutable trunk-type set.",
      new[]
      {
        "Legacy TileID.Sets.IsATreeTrunk ownership and CallTracker instrumentation."
      },
      "Terraria.Dome.WorldGeneration.Verification tree branch frame checks.");
  }

  if (name == "IsTileATreeRoot" && line == 24296)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeTrunkFrameQuery.cs:TryGetRootOffset"
      },
      "Classifies legacy tree root frames against an explicit immutable trunk-type set.",
      new[]
      {
        "Legacy TileID.Sets.IsATreeTrunk ownership and CallTracker instrumentation."
      },
      "Terraria.Dome.WorldGeneration.Verification tree root frame checks.");
  }

  if (name == "GrowTree" && line == 24323)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "OrdinaryTreeGrowthEligibilityQuery.cs:Evaluate",
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "OrdinaryTreeHeightPolicy.cs:Next",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrdinaryTreeTrunkCommandSystem.cs:TryAppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrdinaryTreePlacementSystem.cs:TryPrepare",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrdinaryTreePlacementSystem.cs:TryPrepareWithHeightSelection",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "OrdinaryTreePlacementSystem.cs:TryAppendCommands"
      },
      "Separates immutable-snapshot ordinary-tree preparation, bounded deterministic height " +
      "selection, and command-only trunk placement.",
      new[]
      {
        "Legacy genRand stream parity and errorWorld/extraLivingTrees height branches.",
        "World-rule branches for remix and notTheBees.",
        "Branch/foliage mutation, framing, colors, and network side effects."
      },
      "Terraria.Dome.WorldGeneration.Verification ordinary tree eligibility and trunk checks.");
  }

  if (name == "IsTileTypeFitForTree" && line == 24245)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "OrdinaryTreeGroundQuery.cs:IsSuitable"
      },
      "Classifies explicit ordinary-tree ground Tile IDs without reading legacy global sets.",
      new[]
      {
        "Legacy TileID sets and CallTracker instrumentation.",
        "GrowTree eligibility, random height, tree mutation, framing, and network side effects."
      },
      "Terraria.Dome.WorldGeneration.Verification ordinary tree ground checks.");
  }

  if (name == "EmptyTileCheck" && line == 25960)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeCanopyClearanceQuery.cs:IsClear"
      },
      "Checks an explicit immutable snapshot rectangle for legacy tree canopy clearance.",
      new[]
      {
        "Legacy TileID.Sets.CommonSapling ownership and mutable Main.tile state.",
        "CallTracker instrumentation and callers outside tree canopy preparation."
      },
      "Terraria.Dome.WorldGeneration.Verification tree canopy clearance checks.");
  }

  if (name == "GrowTreeWithSettings" && line == 24955)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
      "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeProfileGrowthEligibilityQuery.cs:Evaluate",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "TreeProfileTrunkCommandSystem.cs:TryAppendCommands"
      },
      "Validates an explicit profile tree root and emits protected trunk commands from an immutable snapshot.",
      new[]
      {
        "Legacy genRand height selection, error-world variations, and empty-canopy clearance.",
        "Branch and foliage layout, framing, color changes, and NetMessage effects."
      },
      "Terraria.Dome.WorldGeneration.Verification profile tree eligibility checks.");
  }

  if (name == "TryGrowingTreeByType" && line == 24908)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeGrowthDispatchQuery.cs:TryGet"
      },
      "Selects an explicit ordinary, palm, or profile tree handler without executing growth.",
      new[]
      {
        "Legacy grow handler execution, Main.tile mutation, and genRand stream ownership.",
        "Tree framing, color changes, NetMessage side effects, and unsupported handler behavior."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded tree-dispatch checks.");
  }

  if (name is "DefaultTreeWallTest" or "GemTreeWallTest" && line is 24815 or 24826)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeWallSuitabilityQuery.cs:IsSuitable"
      },
      "Classifies an explicit wall definition as suitable for a legacy tree profile.",
      new[]
      {
        "Legacy WallID.Sets ownership and wall-definition initialization.",
        "Tree placement, CallTracker instrumentation, and mutable timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tree wall suitability checks.");
  }

  if (name is "GemTreeGroundTest" or "VanityTreeGroundTest" or "AshTreeGroundTest" &&
      line is 24863 or 24878 or 24893)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "TreeGroundSuitabilityQuery.cs:IsSuitable"
      },
      "Classifies an explicit tile definition as suitable ground for a legacy tree profile.",
      new[]
      {
        "Legacy TileID.Sets ownership and tile-definition initialization.",
        "Tree placement, wall suitability, CallTracker instrumentation, and mutable timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tree ground suitability checks.");
  }

  if (name == "TryGetFromTreeId" && line == 3951)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Definitions/" +
        "LegacyTreeProfileRegistry.cs:TryGet"
      },
      "Looks up immutable tree profile metadata by legacy tree tile ID.",
      new[]
      {
        "Legacy ground and wall suitability delegates.",
        "Tree placement, foliage, CallTracker instrumentation, and mutable generator timing."
      },
      "Terraria.Dome.WorldGeneration.Verification immutable tree-profile lookup checks.");
  }

  if (name == "randMoss" && line == 9028)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/MossSelectionPolicy.cs:Next" },
      "Selects legacy neon moss and distinct ordinary moss types through explicit state.",
      new[]
      {
        "Legacy neonMossType and mossType global ownership and genRand stream ownership.",
        "CallTracker instrumentation and mutable generator timing."
      },
      "Terraria.Dome.WorldGeneration.Verification deterministic moss-selection checks.");
  }

  if (name == "randGem" && line == 8997)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/GemTileRandomPolicy.cs:NextGemIndex" },
      "Draws legacy gem indexes through explicit deterministic state and enabled-gem flags.",
      new[]
      {
        "Legacy gem global array ownership and genRand stream ownership.",
        "CallTracker instrumentation and mutable generator timing."
      },
      "Terraria.Dome.WorldGeneration.Verification deterministic gem-selection checks.");
  }

  if (name == "randGemTile" && line == 9009)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/WorldGeneration/GemTileRandomPolicy.cs:NextTile" },
      "Applies the legacy one-in-twenty gem tile draw and tile ID mapping through explicit state.",
      new[]
      {
        "Legacy gem global array ownership and genRand stream ownership.",
        "CallTracker instrumentation and mutable generator timing."
      },
      "Terraria.Dome.WorldGeneration.Verification deterministic gem-selection checks.");
  }

  if (name == "SquareTileFrame" && line == 67181)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/SquareTileFrameRequestQuery.cs:10"
      },
      "Emits the source-compatible 3x3 row-major framing request topology " +
      "with immutable bounds checks.",
      new[]
      {
        "TileFrame frame-value semantics, mutable Tile ownership, map updates, " +
        "and runtime notifications."
      },
      "Terraria.Dome.WorldGeneration.Verification square tile framing topology check.");
  }

  if (name == "SquareWallFrame" && line == 67196)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/SquareWallFrameRequestQuery.cs:8"
      },
      "Emits the source-compatible bounded 3x3 wall-frame coordinate topology.",
      new[]
      {
        "Framing.WallFrame value semantics, center reset behavior, mutable wall " +
        "ownership, and notifications."
      },
      "Terraria.Dome.WorldGeneration.Verification square wall framing topology check.");
  }

  if (name == "RangeFrame" && line == 67211)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/RangeFrameCoordinateQuery.cs:7"
      },
      "Emits the bounded expanded rectangle in deterministic X-major/Y-minor order.",
      new[]
      {
        "TileFrame and Framing.WallFrame values, MapUpdateQueue side effects, " +
        "mutable ownership, and notifications."
      },
      "Terraria.Dome.WorldGeneration.Verification range framing topology check.");
  }

  if (name == "TileFrameImportant" && line == 71665)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/TileFrameImportant136Query.cs:8",
        "src/Terraria.Dome.Simulation/WorldGeneration/TileFrameImportant184Query.cs:6",
        "src/Terraria.Dome.Simulation/WorldGeneration/TileFrameImportant324Query.cs:8",
        "src/Terraria.Dome.Simulation/WorldGeneration/TileFrameImportant529Query.cs:8"
      },
      "Covers source branches for tile types 136, 184, 324, and 529 with typed " +
      "support, frame, random-state, and kill decisions.",
      new[]
      {
        "Other TileFrameImportant branches, mutable Tile ownership, KillTile " +
        "side effects, and notifications."
      },
      "Terraria.Dome.WorldGeneration.Verification typed TileFrameImportant branch checks.");
  }

  if (name == "RandomWorldPoint" && line == 22195)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/RandomWorldPointPolicy.cs:Next"
      },
      "Generates an inset world point through explicit deterministic generation state.",
      new[]
      {
        "Legacy genRand global ownership and shared stream ordering outside the explicit state.",
        "Legacy Main world dimensions, Point type, CallTracker instrumentation, " +
        "and generator timing."
      },
      "Terraria.Dome.WorldGeneration.Verification deterministic world-point checks.");
  }

  if (name == "RandomRectanglePoint" && line is 22181 or 22188)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "RandomRectanglePointPolicy.cs:Next"
      },
      "Generates a bounded rectangle point through explicit deterministic generation state.",
      new[]
      {
        "Legacy genRand global ownership and shared stream ordering outside the explicit state.",
        "Legacy Point/Rectangle types, CallTracker instrumentation, and mutable generator timing.",
        "Invalid or empty rectangles are rejected by the explicit simulation contract."
      },
      "Terraria.Dome.WorldGeneration.Verification deterministic rectangle-point checks.");
  }

  if (name == "errorWorldAdjustment" && line == 328)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "SecretSeedAdjustmentPolicy.cs:Adjust"
      },
      "Applies the legacy active-secret-seed default and integer scaling to an explicit value.",
      new[]
      {
        "Legacy activeSecretSeedCount global ownership and secret-seed registration lifecycle.",
        "Legacy CallTracker instrumentation and mutable secret-seed state."
      },
      "Terraria.Dome.WorldGeneration.Verification secret-seed adjustment checks.");
  }

  if (name == "IsSafeFromRain" && line == 60739)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileLineTraceQuery.cs:IsSafeFromRainPath" },
      "Traces an explicit rain trajectory through an immutable snapshot and stops at solid tiles.",
      new[]
      {
        "Legacy Rain static velocity provider and Main.windSpeedCurrent ownership.",
        "Legacy CallTracker instrumentation and DelegateMethods mutable CheckResultOut state.",
        "The compatibility wrapper's implicit pixel/vector inputs are replaced by explicit parameters."
      },
      "Terraria.Dome.WorldGeneration.Verification snapshot rain-trace checks.");
  }

  if (name is "TopEdgeCanBeAttachedTo" or "RightEdgeCanBeAttachedTo" or
      "LeftEdgeCanBeAttachedTo" or "BottomEdgeCanBeAttachedTo" &&
      line is 58950 or 58972 or 58994 or 59016)
  {
    string target = line switch
    {
      58950 => "CanAttachToTop",
      58972 => "CanAttachToRight",
      58994 => "CanAttachToLeft",
      _ => "CanAttachToBottom"
    };
    return new SourceMapping("Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:" + target },
      "Classifies a snapshot tile as an attachment edge using immutable Version4 tile definitions.",
      new[] { "Legacy nullable Tile slots and exception swallowing.", "CallTracker and mutable Main.tile timing." },
      "Terraria.Dome.WorldGeneration.Verification tile-attachment query checks.");
  }

  if (name == "GetWorldUpdateRate" && line == 59850)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/WorldUpdateRatePolicy.cs:GetRate" },
      "Caps an explicit desired tile-update rate at 24 and returns zero while time is frozen.",
      new[]
      {
        "Legacy Main.desiredWorldTilesUpdateRate global ownership and scheduler integration.",
        "CreativePowerManager state lookup and CallTracker instrumentation.",
        "Negative desired rates are rejected by the explicit simulation contract."
      },
      "Terraria.Dome.WorldGeneration.Verification world update-rate policy checks.");
  }

  if (name == "CountNearBlocksTypes" && line == 58214)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:CountNearbyTileTypes" },
      "Counts active matching snapshot tiles in a clamped square with the legacy cap cutoff.",
      new[]
      {
        "Legacy Main dimensions, direct mutable Main.tile access, and nullable Tile behavior.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "Negative radius and negative tile types are rejected by the explicit snapshot contract."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-neighborhood counting checks.");
  }

  if (name == "setWorldSize" && line == 6221)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:PixelWidth",
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:PixelHeight",
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:SectionColumnCount",
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:SectionRowCount"
      },
      "Derives immutable pixel and section dimensions from world bounds.",
      new[]
      {
        "Legacy Main.bottomWorld, rightWorld, maxSectionsX, and maxSectionsY mutation.",
        "Global world initialization order and any caller-visible static side effects."
      },
      "Terraria.Dome.WorldGeneration.Verification world-size derived bounds checks.");
  }

  if (name == "GetWorldSize" && line == 6231)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/WorldSizeProfile.cs:" +
        "GetLegacyIndexForWidth"
      },
      "Maps the legacy width thresholds to a pure world size index classifier.",
      new[]
      {
        "Legacy Main.maxTilesX static read and CallTracker instrumentation.",
        "World creation, persistence, protocol, and global state interactions."
      },
      "Terraria.Dome.WorldGeneration.Verification world-size width classification checks.");
  }

  if (name == "SetWorldSize" && line == 6246)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/WorldSizeProfile.cs:FromLegacyIndex"
      },
      "Maps legacy size indices to immutable fixed tile dimension profiles.",
      new[]
      {
        "Legacy Main.maxTilesX/maxTilesY mutation and global lifecycle ordering.",
        "World allocation, save state, network state, and protocol reconfiguration."
      },
      "Terraria.Dome.WorldGeneration.Verification fixed world-size profile checks.");
  }

  if (name == "InWorld" && line == 8944)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:Contains"
      },
      "Maps the Point overload to the immutable coordinate bounds contract.",
      new[]
      {
        "Legacy Terraria.Point adapter and Main.maxTilesX/maxTilesY static state.",
        "Legacy CallTracker instrumentation and caller-specific Tile assumptions."
      },
      "Terraria.Dome.WorldGeneration.Verification bounds overload checks.");
  }

  if (name == "InWorld" && line == 8951)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:Contains"
      },
      "Maps coordinate bounds with a non-negative symmetric fluff margin.",
      new[]
      {
        "Legacy Main.maxTilesX/maxTilesY static dimensions and CallTracker behavior.",
        "Any Tile existence, Tile initialization, or caller-side world state assumptions."
      },
      "Terraria.Dome.WorldGeneration.Verification coordinate and fluff checks.");
  }

  if (name == "InWorld" && line == 8962)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Components/" +
        "WorldBoundsComponent.cs:ContainsRectangle"
      },
      "Maps rectangular bounds with explicit width, height, and fluff validation.",
      new[]
      {
        "Legacy Terraria.Rectangle adapter and Main.maxTilesX/maxTilesY static state.",
        "Legacy integer overflow behavior, CallTracker instrumentation, and Tile side effects."
      },
      "Terraria.Dome.WorldGeneration.Verification rectangle bounds checks.");
  }

  if (name == "AreAnyTilesInSetNearby" && line == 8153)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:" +
        "AreAnyTilesInSetNearby"
      },
      "Queries an immutable snapshot over the inclusive square neighborhood for active tiles.",
      new[]
      {
        "Legacy Main.tile reads, nullable Tile slots, and Main-based InWorld checks.",
        "Legacy CallTracker instrumentation and any mutable-world timing assumptions.",
        "Legacy null or short bool[] exception behavior; short read-only sets are non-matches."
      },
      "Terraria.Dome.WorldGeneration.Verification snapshot neighborhood query checks.");
  }

  if (name == "IsTileNearby" && line == 8183)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileNeighborhoodQuery.cs:IsTileNearby"
      },
      "Queries an immutable snapshot over the inclusive neighborhood with tile-235 X stride.",
      new[]
      {
        "Legacy Main.tile reads, nullable Tile slots, and Main-based InWorld checks.",
        "Legacy CallTracker instrumentation and any mutable-world timing assumptions.",
        "Legacy negative tile-type behavior; the snapshot query rejects negative type values."
      },
      "Terraria.Dome.WorldGeneration.Verification snapshot neighborhood query checks.");
  }

  if (name == "countTiles" && line == 8799)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileRegionProbe.cs:CountOpenTiles"
      },
      "Returns a bounded snapshot flood-fill result with legacy category counts.",
      new[]
      {
        "Legacy static counter fields, CountedTiles dictionary, and CallTracker instrumentation.",
        "Legacy nullable Tile access and direct Main dimensions/global Tile state.",
        "Unsupported definition registries and callers that read legacy counter fields after return."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded region-probe checks.");
  }

  if (name == "nextCount" && line == 8814)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileRegionProbe.cs:CountOpenTiles"
      },
      "Performs the bounded left-right-up-down snapshot traversal for CountOpenTiles.",
      new[]
      {
        "Legacy recursive call stack, static mutable counters, and CountedTiles dictionary identity.",
        "Legacy nullable Tile access, Main dimensions, CallTracker instrumentation, and global Tile state.",
        "The separate countDirtTiles and nextDirtCount traversal rules."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded region-probe checks.");
  }

  if (name == "countDirtTiles" && line == 8894)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileDirtRegionProbe.cs:CountTiles"
      },
      "Returns a bounded snapshot dirt-wall region count without shared mutable counters.",
      new[]
      {
        "Legacy static numTileCount and CountedTiles state, plus CallTracker instrumentation.",
        "Legacy nullable Tile access and direct Main dimensions/global Tile state.",
        "Legacy callers that read shared counters or use the region result to run placement logic."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded dirt-region probe checks.");
  }

  if (name == "nextDirtCount" && line == 8904)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileDirtRegionProbe.cs:CountTiles"
      },
      "Performs bounded snapshot traversal through dirt or jungle walls with legacy neighbors.",
      new[]
      {
        "Legacy recursive call stack, static mutable counters, and CountedTiles dictionary identity.",
        "Legacy nullable Tile access, Main dimensions, CallTracker instrumentation, and global Tile state.",
        "The separate countTiles and nextCount lava, shimmer, and category-count rules."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded dirt-region probe checks.");
  }

  if (name == "SolidTile" && line == 58615)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolid" },
      "Classifies a supplied immutable tile from explicit solid and platform definitions.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid and Main.tileSolidTop static arrays and CallTracker instrumentation.",
        "Legacy Tile reference identity and mutation timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "TileEmpty" && line == 58636)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsEmpty" },
      "Classifies snapshot cells as empty when inactive or marked inactive.",
      new[]
      {
        "Legacy nullable Tile slots and direct Main.tile array access.",
        "Legacy CallTracker instrumentation and out-of-bounds exception behavior."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidOrSlopedTile" && line == 58647)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidOrSloped" },
      "Classifies supplied active non-platform solid tiles without excluding slopes or half bricks.",
      new[]
      {
        "Legacy nullable Tile behavior and Main.tileSolid/Main.tileSolidTop static arrays.",
        "Legacy CallTracker instrumentation and Tile reference identity."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "TileType" && line == 58658)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:GetActiveTileType" },
      "Returns the active snapshot tile type or negative one when inactive.",
      new[]
      {
        "Legacy nullable Tile slots, direct Main.tile array access, and CallTracker instrumentation.",
        "Legacy out-of-bounds exception behavior and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidOrSlopedTile" && line == 58669)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidOrSloped" },
      "Classifies a snapshot coordinate through the immutable solid-or-sloped predicate.",
      new[]
      {
        "Legacy nullable Tile slots, direct Main.tile array access, and CallTracker instrumentation.",
        "Legacy out-of-bounds exception behavior and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTile" && line == 58770)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolid" },
      "Classifies a snapshot coordinate with the legacy no-doors solid-tile option.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid/Main.tileSolidTop static arrays and CallTracker instrumentation.",
        "The old Point overload and direct mutable Main.tile access."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTile2" && line == 58795)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatformTop" },
      "Classifies supplied immutable tiles with the SolidTile2 platform top-slope rule.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid static array and CallTracker instrumentation.",
        "Legacy Tile reference identity and mutation timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "PlatformProperTopFrame" && line == 58816)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsPlatformProperTopFrame" },
      "Classifies platform frame columns using the fixed legacy 18-pixel frame width.",
      new[]
      {
        "Legacy TileObjectData runtime lookup for platform coordinate width.",
        "Legacy CallTracker instrumentation and any modded platform frame contract."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTileAllowBottomSlope" && line == 58832)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingBottomSlope" },
      "Classifies snapshot cells with bottom slopes and valid platform top-frame exceptions.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid/Main.tileSolidTop and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTile2" && line == 59064)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatformTop" },
      "Classifies snapshot coordinates with the SolidTile2 platform top-slope rule.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tile array access, Main.tileSolid static array, and CallTracker instrumentation.",
        "Legacy out-of-bounds exception behavior and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTileNoPlatforms" && line == 58858)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidWithoutPlatforms" },
      "Classifies snapshot cells as solid while excluding platform definitions.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid/Main.tileSolidTop and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTileAllowTopSlope" && line == 58884)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingTopSlope" },
      "Classifies snapshot cells that allow top-facing slope support and platform half bricks.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTileAllowLeftSlope" && line == 58906)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingLeftSlope" },
      "Classifies snapshot cells that allow left-facing slope support.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTileAllowRightSlope" && line == 58928)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:IsSolidAllowingRightSlope" },
      "Classifies snapshot cells that allow right-facing slope support.",
      new[]
      {
        "Legacy nullable Tile is treated as solid and all exceptions are swallowed.",
        "Legacy Main.tileSolid and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTile3" && line == 59038)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:" +
        "IsSolidWithLegacyTile3Semantics"
      },
      "Classifies snapshot coordinates through the legacy one-tile-fluff SolidTile3 boundary.",
      new[]
      {
        "Legacy Main.tile array and Main.tileSolid/Main.tileSolidTop static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "Legacy nullable Tile behavior is excluded because snapshot cells are non-null values."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "SolidTile3" && line == 59049)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/TileStateQuery.cs:" +
        "IsSolidWithLegacyTile3Semantics"
      },
      "Classifies supplied immutable tiles as active non-platform solid tiles.",
      new[]
      {
        "Legacy nullable Tile behavior and Main.tileSolid/Main.tileSolidTop static arrays.",
        "Legacy CallTracker instrumentation and Tile reference identity."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-state query checks.");
  }

  if (name == "HasAnyWireNearby" && line == 60717)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileWireQuery.cs:HasAnyWireNearby" },
      "Scans a clamped immutable snapshot rectangle for any of the four wire channels.",
      new[]
      {
        "Legacy nullable Tile slots, Main dimensions, and direct mutable Main.tile access.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "Negative boxSpread is rejected by the explicit snapshot contract."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-wire query checks.");
  }

  if (name == "GetRopeEnds" && line == 58676)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:FindEnds" },
      "Finds bounded rope endpoints in an immutable snapshot using the Version4 rope set.",
      new[]
      {
        "Legacy nullable Tile slots, Main dimensions, and direct mutable Main.tile access.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "Negative rangeToCheck is rejected by the explicit snapshot contract."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-rope query checks.");
  }

  if (name == "IsRope" && line == 58736)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:IsRope" },
      "Classifies active Version4 rope tiles and supported platform bridges from a snapshot.",
      new[]
      {
        "Legacy nullable Tile slots, Main.tileRope and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "The overload that exposes legacy out parameters remains unmapped."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-rope query checks.");
  }

  if (name == "IsRope" && line == 58727)
  {
    return new SourceMapping(
      "Partial",
      new[] { "src/Terraria.Dome.Simulation/World/TileRopeQuery.cs:IsRope" },
      "Classifies a snapshot coordinate without exposing the intermediate rope endpoints.",
      new[]
      {
        "Legacy nullable Tile slots, Main.tileRope and TileID.Sets.Platforms static arrays.",
        "Legacy CallTracker instrumentation and mutable Tile timing.",
        "Legacy endpoint out parameters are intentionally not exposed by this convenience mapping."
      },
      "Terraria.Dome.WorldGeneration.Verification tile-rope query checks.");
  }

  if (name == "GetLiquidChangeType" && line == 4527)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/Liquid/Definitions/" +
        "LiquidInteractionClassifier.cs:GetKind"
      },
      "Maps the four liquid pair table to a protocol-independent interaction fact.",
      new[]
      {
        "Legacy TileChangeType protocol enum and NetMessage.SendTileSquare projection.",
        "Tile placement, liquid mutation, framing, and any runtime side effects.",
        "Compatibility packet encoding and caller-specific notification routing."
      },
      "Terraria.Dome.WorldGeneration.Verification liquid interaction classification checks.");
  }

  if (name == "EmptyLiquid" && line == 4454)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/World/Systems/" +
        "LiquidChangeCommitSystem.cs:TryCommit"
      },
      "Maps validated liquid state clearing to the immutable command commit boundary.",
      new[]
      {
        "Legacy Main.tile access, solid-tile predicates, and direct liquid clearing.",
        "SquareTileFrame, NetMessage.sendWater, and runtime notification side effects.",
        "Legacy global Liquid queue and all liquid-type-specific compatibility behavior."
      },
      "Terraria.Dome.WorldGeneration.Verification liquid command validation and commit checks.");
  }

  if (name == "PlaceLiquid" && line == 4478)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/" +
        "LiquidPropagationSession.cs:Advance",
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "LiquidPropagationSystem.cs:TryAppendCommands",
        "src/Terraria.Dome.Simulation/World/Systems/" +
        "LiquidChangeCommitSystem.cs:TryCommit"
      },
      "Maps bounded liquid work-item propagation, amount validation, and typed command commit.",
      new[]
      {
        "Legacy Main.tile solid predicates and direct liquid mutation/clamping.",
        "SquareTileFrame, NetMessage, and runtime notification side effects.",
        "Legacy Liquid.GetLiquidMergeTypes tile placement and global queue semantics.",
        "Uncaptured liquid definitions, secret seeds, and unsupported world-generation rules."
      },
      "Terraria.Dome.WorldGeneration.Verification bounded propagation, merge, and commit checks.");
  }

  if (name == "AddPasses" && line == 10553)
  {
    return new SourceMapping(
      "Partial",
      new[]
      {
        "src/Terraria.Dome.Simulation/WorldGeneration/Systems/" +
        "DirtWallBackgroundSystem.cs:AppendCommands",
        "src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs:Generate",
        "src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationRequest.cs:" +
        "WorldGenerationRequest"
      },
      "Models the DirtWallBackgrounds predicate and conditionally commits oracle wall commands.",
      new[]
      {
        "Legacy pass registration order, GenVars, and all other generation passes.",
        "UnifiedRandom state except for the archived default seed-1456 offset artifact.",
        "Secret-seed, difficulty, hardmode, and all unmatched world sizes/options."
      },
      "Fixture plus source-built default seed-1456 oracle differential.");
  }

  if (name != "GenerateWorld" || line != 10108)
  {
    return null;
  }

  return new SourceMapping(
    "Partial",
    new[] { "src/Terraria.Dome.Simulation/WorldGeneration/WorldGenerationPipeline.cs:Generate" },
    "Creates a deterministic request-to-snapshot stage pipeline through command commits.",
    new[]
    {
      "Legacy Main state, configuration hooks, and complete generator pass registration.",
      "Secret-seed, difficulty, evil, and world-option input capture.",
      "clearWorld, Reset, Finish, save, audio, callback, and legacy temporary-state behavior."
    },
    "Terraria.Dome.WorldGeneration.Verification deterministic replay and stage assertions.");
}

static IEnumerable<FieldInventory> CreateFieldInventory(FieldDeclarationSyntax field)
{
  string body = field.ToFullString();
  string visibility = field.Modifiers.Any(SyntaxKind.PublicKeyword)
    ? "Public"
    : field.Modifiers.Any(SyntaxKind.InternalKeyword) ? "Internal" : "Private";
  foreach (VariableDeclaratorSyntax variable in field.Declaration.Variables)
  {
    yield return new FieldInventory(
      variable.Identifier.ValueText,
      visibility,
      field.GetLocation().GetLineSpan().StartLinePosition.Line + 1,
      field.Modifiers.Any(SyntaxKind.StaticKeyword),
      Classify(body),
      "Unmapped",
      CreateReferences(body));
  }
}

static List<ReferenceInventory> CreateReferenceInventory(CompilationUnitSyntax root)
{
  string source = root.ToFullString();
  return new[] { "Main", "Tile", "Liquid", "NetMessage" }
    .Select(reference => new ReferenceInventory(
      reference,
      CountToken(source, reference),
      source.Contains(reference, StringComparison.Ordinal) ? "Observed" : "Absent"))
    .ToList();
}

static List<string> CreateReferences(string body)
{
  return new[] { "Main", "Tile", "Liquid", "NetMessage" }
    .Where(reference => body.Contains(reference, StringComparison.Ordinal))
    .ToList();
}

static string Classify(string body)
{
  if (body.Contains("Liquid", StringComparison.Ordinal))
  {
    return "Liquid";
  }

  if (body.Contains("Tile", StringComparison.Ordinal))
  {
    return "Tile";
  }

  if (body.Contains("Structure", StringComparison.Ordinal) ||
      body.Contains("Dungeon", StringComparison.Ordinal))
  {
    return "Structure";
  }

  if (body.Contains("Tree", StringComparison.Ordinal) ||
      body.Contains("Grow", StringComparison.Ordinal))
  {
    return "Tree";
  }

  if (body.Contains("Ore", StringComparison.Ordinal) ||
      body.Contains("Gem", StringComparison.Ordinal))
  {
    return "Ore";
  }

  if (body.Contains("Biome", StringComparison.Ordinal) ||
      body.Contains("Surface", StringComparison.Ordinal))
  {
    return "Biome";
  }

  return "WorldRuntime";
}

static int CountToken(string source, string token)
{
  return source.Split(token, StringSplitOptions.None).Length - 1;
}

static string CreateSnapshotFingerprint(WorldGridSnapshot snapshot)
{
  StringBuilder builder = new();
  for (int y = 0; y < snapshot.Metadata.Height; y++)
  {
    for (int x = 0; x < snapshot.Metadata.Width; x++)
    {
      builder.Append(snapshot.GetTile(x, y));
    }
  }

  for (int y = 0; y < snapshot.Metadata.Height / WorldGrid.SectionHeight; y++)
  {
    for (int x = 0; x < snapshot.Metadata.Width / WorldGrid.SectionWidth; x++)
    {
      builder.Append(snapshot.GetSectionVersion(new WorldSectionCoordinates(x, y)));
    }
  }

  return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
}

static string CreateLegacyOracleFingerprint(LegacyWorldDocument document)
{
  using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
  AppendText(hash, document.Version.ToString());
  AppendText(hash, document.Metadata.Name);
  AppendInt32(hash, document.Metadata.WorldId);
  AppendInt32(hash, document.Metadata.Width);
  AppendInt32(hash, document.Metadata.Height);
  AppendInt32(hash, document.Metadata.SpawnX);
  AppendInt32(hash, document.Metadata.SpawnY);
  Span<byte> tileBuffer = stackalloc byte[32];
  foreach (LegacyTile tile in document.Tiles)
  {
    int offset = 0;
    tileBuffer[offset++] = tile.IsActive ? (byte)1 : (byte)0;
    BinaryPrimitives.WriteUInt16LittleEndian(tileBuffer[offset..], tile.TileType);
    offset += sizeof(ushort);
    BinaryPrimitives.WriteUInt16LittleEndian(tileBuffer[offset..], tile.WallType);
    offset += sizeof(ushort);
    tileBuffer[offset++] = tile.LiquidAmount;
    tileBuffer[offset++] = tile.LiquidKind;
    tileBuffer[offset++] = tile.HasWire ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.HasWire2 ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.HasWire3 ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.HasWire4 ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.IsHalfBrick ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.Slope;
    tileBuffer[offset++] = tile.IsActuated ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.IsInactive ? (byte)1 : (byte)0;
    BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameX);
    offset += sizeof(short);
    BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameY);
    offset += sizeof(short);
    tileBuffer[offset++] = tile.TileColor;
    tileBuffer[offset++] = tile.WallColor;
    tileBuffer[offset++] = tile.IsInvisibleBlock ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.IsInvisibleWall ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.IsFullbrightBlock ? (byte)1 : (byte)0;
    tileBuffer[offset++] = tile.IsFullbrightWall ? (byte)1 : (byte)0;
    hash.AppendData(tileBuffer[..offset]);
  }

  foreach (LegacyChest chest in document.Chests)
  {
    AppendText(hash, chest);
  }

  foreach (LegacySign sign in document.Signs)
  {
    AppendText(hash, sign);
  }

  foreach (LegacyNpc npc in document.Npcs)
  {
    AppendText(hash, npc);
  }

  foreach (LegacyTileEntity entity in document.TileEntities)
  {
    AppendText(hash, entity);
  }

  return Convert.ToHexString(hash.GetHashAndReset());
}

static string CreateLegacyProjectedFingerprint(LegacyWorldDocument document)
{
  using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
  AppendInt32(hash, document.Metadata.Width);
  AppendInt32(hash, document.Metadata.Height);
  Span<byte> tileBuffer = stackalloc byte[9];
  foreach (LegacyTile tile in document.Tiles)
  {
    int offset = 0;
    tileBuffer[offset++] = tile.IsActive ? (byte)1 : (byte)0;
    BinaryPrimitives.WriteUInt16LittleEndian(tileBuffer[offset..], tile.TileType);
    offset += sizeof(ushort);
    tileBuffer[offset++] = tile.LiquidAmount;
    tileBuffer[offset++] = tile.LiquidKind;
    BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameX);
    offset += sizeof(short);
    BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameY);
    offset += sizeof(short);
    hash.AppendData(tileBuffer[..offset]);
  }

  return Convert.ToHexString(hash.GetHashAndReset());
}

static void RunLegacyDifferential(
  string legacyPath,
  string repositoryRoot,
  bool useLegacyMetadataSpawn,
  bool useLegacyMetadataSurfaceY,
  int? requestedSurfaceY,
  string? dirtWallOffsetsPath,
  string? terrainProfilePath)
{
  using FileStream oracleStream = new(
    legacyPath,
    FileMode.Open,
    FileAccess.Read,
    FileShare.Read);
  LegacyWorldDocument legacy = WldWorldReader.Read(oracleStream);
  int spawnX = useLegacyMetadataSpawn
    ? legacy.Metadata.SpawnX
    : legacy.Metadata.Width / 2;
  if (useLegacyMetadataSurfaceY && !double.IsFinite(legacy.Metadata.WorldSurface))
  {
    throw new ArgumentOutOfRangeException(
      nameof(legacy.Metadata.WorldSurface),
      "The legacy WLD world surface must be finite.");
  }

  int surfaceY = useLegacyMetadataSurfaceY
    ? (int)legacy.Metadata.WorldSurface
    : requestedSurfaceY ?? legacy.Metadata.Height / 4;
  int legacyWorldSurfaceY = GetLegacyLayerY(
    legacy.Metadata.WorldSurface,
    legacy.Metadata.Height,
    nameof(legacy.Metadata.WorldSurface));
  int legacyRockLayerY = GetLegacyLayerY(
    legacy.Metadata.RockLayer,
    legacy.Metadata.Height,
    nameof(legacy.Metadata.RockLayer));
  if (spawnX < 0 || spawnX >= legacy.Metadata.Width)
  {
    throw new ArgumentOutOfRangeException(
      nameof(spawnX),
      "The selected legacy differential spawn X is outside the world bounds.");
  }

  if (surfaceY < 0 || surfaceY >= legacy.Metadata.Height)
  {
    throw new ArgumentOutOfRangeException(
      nameof(requestedSurfaceY),
      "The selected legacy differential surface Y is outside the world bounds.");
  }

  if (legacyWorldSurfaceY > legacyRockLayerY)
  {
    throw new ArgumentOutOfRangeException(
      nameof(legacy.Metadata.RockLayer),
      "The legacy WLD rock layer must not be above the world surface.");
  }

  string spawnSource = useLegacyMetadataSpawn ? "legacy-metadata" : "world-center";
  string surfaceYSource = useLegacyMetadataSurfaceY
    ? "legacy-world-surface"
    : requestedSurfaceY.HasValue ? "explicit-argument" : "world-quarter";
  WorldMetadata metadata = new(
    legacy.Metadata.Name,
    new WorldSeed(1456),
    legacy.Metadata.Width,
    legacy.Metadata.Height);
  IReadOnlyList<int>? dirtWallSurfaceOffsetChanges = dirtWallOffsetsPath is null
    ? null
    : ParseDirtWallOffsetChanges(
      File.ReadAllLines(dirtWallOffsetsPath),
      metadata.Width);
  LegacyTerrainRuntimeProfile? terrainProfile = terrainProfilePath is null
    ? null
    : ReadLegacyTerrainRuntimeProfile(terrainProfilePath, metadata);
  WorldGenerationRequest generationRequest = new(
    metadata,
    spawnX,
    surfaceY,
    rockLayerY: legacyRockLayerY,
    dirtWallSurfaceOffsetChanges: dirtWallSurfaceOffsetChanges,
    terrainProfile: terrainProfile);
  WorldGrid generated = new WorldGenerationPipeline().Generate(generationRequest);
  WorldGenerationTrace fullProfileTrace =
    new WorldGenerationPipeline().GenerateWithTrace(generationRequest);
  WriteStageTraceArtifact(repositoryRoot, fullProfileTrace);
  int sectionColumns = metadata.Width / WorldGrid.SectionWidth;
  int sectionRows = metadata.Height / WorldGrid.SectionHeight;
  int[,] mismatchesBySection = new int[sectionColumns, sectionRows];
  int[,] extendedStateMismatchesBySection = new int[sectionColumns, sectionRows];
  Dictionary<string, int>[,] mismatchFieldCountsBySection =
    new Dictionary<string, int>[sectionColumns, sectionRows];
  Dictionary<string, int> mismatchFieldCounts = CreateMismatchFieldCounts();
  Dictionary<string, int> extendedStateMismatchFieldCounts =
    CreateExtendedStateMismatchFieldCounts();
  List<LegacySpatialRegionAccumulator> spatialRegions = CreateSpatialRegions(
    legacyWorldSurfaceY,
    legacyRockLayerY,
    metadata.Height);
  int comparedTiles = 0;
  int mismatchTiles = 0;
  int extendedStateMismatchTiles = 0;
  int activeLegacyTiles = 0;
  int activeGeneratedTiles = 0;
  for (int index = 0; index < legacy.Tiles.Count; index++)
  {
    int x = index / metadata.Height;
    int y = index % metadata.Height;
    LegacyTile expected = legacy.Tiles[index];
    WorldTile actual = generated.GetTile(x, y);
    WorldSectionCoordinates section = new(
      x / WorldGrid.SectionWidth,
      y / WorldGrid.SectionHeight);
    Dictionary<string, int>? sectionMismatchFieldCounts =
      mismatchFieldCountsBySection[section.X, section.Y];
    if (sectionMismatchFieldCounts is null)
    {
      sectionMismatchFieldCounts = CreateMismatchFieldCounts();
      mismatchFieldCountsBySection[section.X, section.Y] = sectionMismatchFieldCounts;
    }

    LegacySpatialRegionAccumulator spatialRegion = spatialRegions[
      GetSpatialRegionIndex(y, legacyWorldSurfaceY, legacyRockLayerY)];
    bool hasMismatch = CountTileStateMismatches(
      expected,
      actual,
      mismatchFieldCounts,
      sectionMismatchFieldCounts,
      spatialRegion.MismatchFieldCounts);
    if (expected.IsActive)
    {
      activeLegacyTiles++;
    }

    if (actual.IsActive)
    {
      activeGeneratedTiles++;
    }

    comparedTiles++;
    spatialRegion.ComparedTiles++;
    if (hasMismatch)
    {
      mismatchTiles++;
      mismatchesBySection[section.X, section.Y]++;
      spatialRegion.MismatchTiles++;
    }

    if (CountExtendedStateMismatches(
          expected,
          actual,
          extendedStateMismatchFieldCounts))
    {
      extendedStateMismatchTiles++;
      extendedStateMismatchesBySection[section.X, section.Y]++;
    }
  }

  List<LegacySpatialRegionDifference> regions = spatialRegions
    .Select(region => region.ToDifference())
    .ToList();
  List<LegacySectionDifference> sections = new(sectionColumns * sectionRows);
  for (int y = 0; y < sectionRows; y++)
  {
    for (int x = 0; x < sectionColumns; x++)
    {
      sections.Add(new LegacySectionDifference(
        x,
        y,
        mismatchesBySection[x, y],
        extendedStateMismatchesBySection[x, y],
        mismatchFieldCountsBySection[x, y] ?? CreateMismatchFieldCounts()));
    }
  }

  LegacyDifferentialEvidence evidence = new(
    legacyPath,
    metadata.Width,
    metadata.Height,
    spawnX,
    surfaceY,
    spawnSource,
    surfaceYSource,
    legacy.Metadata.SpawnX,
    legacy.Metadata.SpawnY,
    legacyWorldSurfaceY,
    legacyRockLayerY,
    dirtWallOffsetsPath,
    dirtWallOffsetsPath is null
      ? null
      : Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(dirtWallOffsetsPath))),
    dirtWallSurfaceOffsetChanges?.Count,
    generationRequest.RockLayerY,
    comparedTiles,
    mismatchTiles,
    mismatchFieldCounts,
    extendedStateMismatchTiles,
    extendedStateMismatchFieldCounts,
    activeLegacyTiles,
    activeGeneratedTiles,
    CreateLegacyOracleFingerprint(legacy),
    CreateLegacyProjectedFingerprint(legacy),
    CreateWorldGridFingerprint(generated),
    sections,
    regions);
  string evidenceDirectory = Path.Combine(repositoryRoot, "docs", "worldgen");
  Directory.CreateDirectory(evidenceDirectory);
  File.WriteAllText(
    Path.Combine(evidenceDirectory, "legacy-worldgen-differential.json"),
    JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
  Console.WriteLine(
    $"DIFF: legacy oracle compared {comparedTiles} tiles, mismatches {mismatchTiles}, " +
    $"extended-state mismatches {extendedStateMismatchTiles}");
}

static void WriteStageTraceArtifact(string repositoryRoot, WorldGenerationTrace trace)
{
  string stageTraceDirectory = Path.Combine(
    repositoryRoot,
    "Build",
    "diagnostics",
    "server-ecs-convergence",
    "P9-worldgen",
    "current-stage-trace");
  Directory.CreateDirectory(stageTraceDirectory);
  var stages = trace.Stages.Select(stage => new
  {
    stage = stage.Stage.ToString(),
    fingerprint = CreateSnapshotWorldGridFingerprint(stage.Snapshot),
    wallTileCount = CountWallTiles(stage.Snapshot),
    nextSequence = stage.NextSequence,
    oracleParity = "not-compared"
  });
  File.WriteAllText(
    Path.Combine(stageTraceDirectory, "stage-fingerprints-full-profile.json"),
    JsonSerializer.Serialize(stages, new JsonSerializerOptions { WriteIndented = true }));
}

static int GetLegacyLayerY(double layer, int worldHeight, string parameterName)
{
  if (!double.IsFinite(layer))
  {
    throw new ArgumentOutOfRangeException(
      parameterName,
      "The legacy WLD layer value must be finite.");
  }

  int layerY = (int)layer;
  if (layerY < 0 || layerY > worldHeight)
  {
    throw new ArgumentOutOfRangeException(
      parameterName,
      "The legacy WLD layer value is outside the world bounds.");
  }

  return layerY;
}

static List<LegacySpatialRegionAccumulator> CreateSpatialRegions(
  int worldSurfaceY,
  int rockLayerY,
  int worldHeight)
{
  return new List<LegacySpatialRegionAccumulator>
  {
    new("AboveWorldSurface", 0, worldSurfaceY, CreateMismatchFieldCounts()),
    new("SurfaceToRockLayer", worldSurfaceY, rockLayerY, CreateMismatchFieldCounts()),
    new("BelowRockLayer", rockLayerY, worldHeight, CreateMismatchFieldCounts())
  };
}

static int GetSpatialRegionIndex(int y, int worldSurfaceY, int rockLayerY)
{
  if (y < worldSurfaceY)
  {
    return 0;
  }

  if (y < rockLayerY)
  {
    return 1;
  }

  return 2;
}

static bool CountTileStateMismatches(
  LegacyTile expected,
  WorldTile actual,
  Dictionary<string, int> mismatchFieldCounts,
  Dictionary<string, int> sectionMismatchFieldCounts,
  Dictionary<string, int> regionMismatchFieldCounts)
{
  bool hasMismatch = false;
  IncrementMismatch(expected.IsActive != actual.IsActive, "IsActive", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.TileType != actual.Type, "TileType", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.LiquidAmount != actual.LiquidAmount, "LiquidAmount", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.LiquidKind != actual.LiquidType, "LiquidKind", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.FrameX != actual.FrameX, "FrameX", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.FrameY != actual.FrameY, "FrameY", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.WallType != actual.WallType, "WallType", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.HasWire != actual.HasWire, "Wire", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.HasWire2 != actual.HasWire2, "Wire2", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.HasWire3 != actual.HasWire3, "Wire3", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.HasWire4 != actual.HasWire4, "Wire4", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsHalfBrick != actual.IsHalfBrick, "HalfBrick", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.Slope != actual.Slope, "Slope", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsActuated != actual.IsActuated, "Actuated", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsInactive != actual.IsInactive, "Inactive", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.TileColor != actual.TileColor, "TileColor", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.WallColor != actual.WallColor, "WallColor", mismatchFieldCounts,
    sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsInvisibleBlock != actual.IsInvisibleBlock, "InvisibleBlock",
    mismatchFieldCounts, sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsInvisibleWall != actual.IsInvisibleWall, "InvisibleWall",
    mismatchFieldCounts, sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsFullbrightBlock != actual.IsFullbrightBlock, "FullbrightBlock",
    mismatchFieldCounts, sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  IncrementMismatch(expected.IsFullbrightWall != actual.IsFullbrightWall, "FullbrightWall",
    mismatchFieldCounts, sectionMismatchFieldCounts, regionMismatchFieldCounts, ref hasMismatch);
  return hasMismatch;
}

static Dictionary<string, int> CreateMismatchFieldCounts()
{
  return new Dictionary<string, int>(StringComparer.Ordinal)
  {
    ["Actuated"] = 0,
    ["FrameX"] = 0,
    ["FrameY"] = 0,
    ["FullbrightBlock"] = 0,
    ["FullbrightWall"] = 0,
    ["HalfBrick"] = 0,
    ["Inactive"] = 0,
    ["InvisibleBlock"] = 0,
    ["InvisibleWall"] = 0,
    ["IsActive"] = 0,
    ["LiquidAmount"] = 0,
    ["LiquidKind"] = 0,
    ["Slope"] = 0,
    ["TileColor"] = 0,
    ["TileType"] = 0,
    ["WallColor"] = 0,
    ["WallType"] = 0,
    ["Wire"] = 0,
    ["Wire2"] = 0,
    ["Wire3"] = 0,
    ["Wire4"] = 0
  };
}

static void IncrementMismatch(
  bool isMismatch,
  string fieldName,
  Dictionary<string, int> mismatchFieldCounts,
  Dictionary<string, int> sectionMismatchFieldCounts,
  Dictionary<string, int> regionMismatchFieldCounts,
  ref bool hasMismatch)
{
  if (!isMismatch)
  {
    return;
  }

  mismatchFieldCounts[fieldName]++;
  sectionMismatchFieldCounts[fieldName]++;
  regionMismatchFieldCounts[fieldName]++;
  hasMismatch = true;
}

static bool CountExtendedStateMismatches(
  LegacyTile expected,
  WorldTile actual,
  Dictionary<string, int> extendedStateMismatchFieldCounts)
{
  bool hasMismatch = false;
  IncrementExtendedStateMismatch(expected.WallType != actual.WallType, "WallType",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.TileColor != actual.TileColor, "TileColor",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.WallColor != actual.WallColor, "WallColor",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsInvisibleBlock != actual.IsInvisibleBlock,
    "InvisibleBlock", extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsInvisibleWall != actual.IsInvisibleWall,
    "InvisibleWall", extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsFullbrightBlock != actual.IsFullbrightBlock,
    "FullbrightBlock", extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsFullbrightWall != actual.IsFullbrightWall,
    "FullbrightWall", extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.HasWire != actual.HasWire, "Wire",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.HasWire2 != actual.HasWire2, "Wire2",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.HasWire3 != actual.HasWire3, "Wire3",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.HasWire4 != actual.HasWire4, "Wire4",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsHalfBrick != actual.IsHalfBrick, "HalfBrick",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.Slope != actual.Slope, "Slope",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsActuated != actual.IsActuated, "Actuated",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  IncrementExtendedStateMismatch(expected.IsInactive != actual.IsInactive, "Inactive",
    extendedStateMismatchFieldCounts, ref hasMismatch);
  return hasMismatch;
}

static void LegacyBeachBoundsDefinitionSourceCheck()
{
  LegacyBeachBounds normal = LegacyBeachBoundsDefinition.Calculate(
    new LegacyPassRandomState(1456),
    4200,
    LegacyDungeonSide.Right,
    tenthAnniversaryWorld: false,
    remixWorld: false);
  if (normal.BeachSandRandomCenter != 320 ||
      normal.BeachSandRandomWidthRange != 20 ||
      normal.BeachSandDungeonExtraWidth != 40 ||
      normal.BeachSandJungleExtraWidth != 20 ||
      normal.LeftBeachEnd < 340 || normal.LeftBeachEnd >= 380 ||
      normal.RightBeachStart <= 3840 || normal.RightBeachStart > 3880)
  {
    throw new InvalidOperationException(
      $"Legacy beach bounds diverged from source constants: " +
      $"left={normal.LeftBeachEnd}, right={normal.RightBeachStart}.");
  }

  LegacyBeachBounds anniversary = LegacyBeachBoundsDefinition.Calculate(
    new LegacyPassRandomState(1456),
    4200,
    LegacyDungeonSide.Left,
    tenthAnniversaryWorld: true,
    remixWorld: false);
  if (anniversary.LeftBeachEnd != 360 || anniversary.RightBeachStart != 3820)
  {
    throw new InvalidOperationException(
      $"Legacy anniversary beach bounds diverged: " +
      $"left={anniversary.LeftBeachEnd}, right={anniversary.RightBeachStart}.");
  }

  LegacyBeachBounds remix = LegacyBeachBoundsDefinition.Calculate(
    new LegacyPassRandomState(1456),
    4200,
    LegacyDungeonSide.Left,
    tenthAnniversaryWorld: true,
    remixWorld: true);
  if (remix.LeftBeachEnd == 360 || remix.RightBeachStart == 3820)
  {
    throw new InvalidOperationException(
      "Remix worlds incorrectly used anniversary fixed beach widths.");
  }

  Console.WriteLine("PASS: WorldGen beach bounds preserve source constants and seed branches");
}

static void LegacyTerrainPassContractSourceCheck()
{
  LegacyTerrainPassContract contract =
    LegacyTerrainPassContractDefinition.CreateDefault();
  if (contract.PassName != "Terrain" ||
      Math.Abs(contract.Weight - 449.3721923828125) > 0.0000001 ||
      contract.ConfigurationSection != "Terrain" ||
      contract.FlatBeachPadding != 5 ||
      !contract.ResetsRandomFromWorldSeed)
  {
    throw new InvalidOperationException(
      "Terrain pass contract diverged from WorldGenerator and configuration source facts.");
  }

  Console.WriteLine(
    "PASS: Terrain pass scheduling and pass-scoped random reset preserve source contract");
}

static void LegacyTerrainFeatureRunSourceCheck()
{
  WorldMetadata metadata = new(
    "terrain-feature-run",
    new WorldSeed(1456),
    width: 200,
    height: 150);
  LegacyTerrainRuntimeProfile runtimeProfile = new(
    WorldSurface: 45,
    RockLayer: 80,
    WorldSurfaceLow: 30,
    WorldSurfaceHigh: 60,
    RockLayerLow: 70,
    RockLayerHigh: 90,
    LeftBeachEnd: 0,
    RightBeachStart: 100,
    WaterLine: 100,
    LavaLine: 120)
  {
    InitialWorldSurface = 45,
    InitialRockLayer = 80
  };
  WorldGenerationRequest request = new(
    metadata,
    spawnX: 100,
    surfaceY: 45,
    rockLayerY: 80,
    terrainProfile: runtimeProfile);
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  WorldGenerationStateComponent state = new(metadata.WorldId);
  List<TileChangeCommand> commands = new();
  new TerrainBaseSystem().AppendLegacyCommands(
    world.CreateSnapshot(metadata),
    request,
    new TerrainProfileComponent(45, 80, metadata.Height - 1),
    ref state,
    commands);
  TileChangeCommand columnTwo = commands.First(command => command.X == 2);
  if (columnTwo.Y != 33 || columnTwo.TileType != 0 || columnTwo.FrameX != -1 ||
      columnTwo.FrameY != -1)
  {
    throw new InvalidOperationException(
      $"Terrain feature-run padding diverged at x=2: y={columnTwo.Y}, " +
      $"type={columnTwo.TileType}, frameX={columnTwo.FrameX}, frameY={columnTwo.FrameY}.");
  }

  Console.WriteLine(
    "PASS: Terrain feature-run initialization preserves the source beach-padding boundary");
}

static void LegacyTerrainCentralFeatureGuardSourceCheck()
{
  WorldMetadata metadata = new(
    "terrain-central-feature-guard",
    new WorldSeed(1456),
    width: 200,
    height: 150);
  LegacyTerrainRuntimeProfile runtimeProfile = new(
    WorldSurface: 45,
    RockLayer: 80,
    WorldSurfaceLow: 30,
    WorldSurfaceHigh: 60,
    RockLayerLow: 70,
    RockLayerHigh: 90,
    LeftBeachEnd: 0,
    RightBeachStart: 100,
    WaterLine: 100,
    LavaLine: 120)
  {
    InitialWorldSurface = 45,
    InitialRockLayer = 80
  };
  WorldGenerationRequest request = new(
    metadata,
    spawnX: 100,
    surfaceY: 45,
    rockLayerY: 80,
    terrainProfile: runtimeProfile);
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  WorldGenerationStateComponent state = new(metadata.WorldId);
  List<TileChangeCommand> commands = new();
  new TerrainBaseSystem().AppendLegacyCommands(
    world.CreateSnapshot(metadata),
    request,
    new TerrainProfileComponent(45, 80, metadata.Height - 1),
    ref state,
    commands);
  TileChangeCommand columnOneHundredFour = commands.First(command => command.X == 104);
  if (columnOneHundredFour.Y != 31 || columnOneHundredFour.TileType != 0 ||
      columnOneHundredFour.FrameX != -1 || columnOneHundredFour.FrameY != -1)
  {
    throw new InvalidOperationException(
      $"Terrain central feature guard diverged at x=104: y={columnOneHundredFour.Y}, " +
      $"type={columnOneHundredFour.TileType}, frameX={columnOneHundredFour.FrameX}, " +
      $"frameY={columnOneHundredFour.FrameY}.");
  }

  Console.WriteLine(
    "PASS: Terrain central feature guard preserves source random re-selection");
}

static void LegacyTerrainWorldSizeClampSourceCheck()
{
  WorldMetadata metadata = new(
    "terrain-world-size-clamp",
    new WorldSeed(1456),
    width: 4200,
    height: 1200);
  LegacyTerrainRuntimeProfile runtimeProfile = new(
    WorldSurface: 229,
    RockLayer: 396.14,
    WorldSurfaceLow: 174.6,
    WorldSurfaceHigh: 300,
    RockLayerLow: 320.14,
    RockLayerHigh: 397.14,
    LeftBeachEnd: 0,
    RightBeachStart: 4199,
    WaterLine: 765,
    LavaLine: 835)
  {
    InitialWorldSurface = 174.6,
    InitialRockLayer = 373.14
  };
  WorldGenerationRequest request = new(
    metadata,
    spawnX: 2100,
    surfaceY: 229,
    rockLayerY: 397,
    terrainProfile: runtimeProfile);
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  WorldGenerationStateComponent state = new(metadata.WorldId);
  List<TileChangeCommand> commands = new();
  new TerrainBaseSystem().AppendLegacyCommands(
    world.CreateSnapshot(metadata),
    request,
    new TerrainProfileComponent(229, 397, metadata.Height - 1),
    ref state,
    commands);
  TileChangeCommand firstColumn = commands.First(command => command.X == 0);
  TileChangeCommand firstGround = commands.First(
    command => command.X == 0 && command.TileType == 1);
  if (firstColumn.Y != 228 || firstColumn.TileType != 0 ||
      firstColumn.FrameX != -1 || firstColumn.FrameY != -1 ||
      firstGround.Y != 374)
  {
    throw new InvalidOperationException(
      $"Terrain small-world lower clamp diverged at x=0: y={firstColumn.Y}, " +
      $"type={firstColumn.TileType}, firstGroundY={firstGround.Y}, " +
      $"frameX={firstColumn.FrameX}, frameY={firstColumn.FrameY}.");
  }

  Console.WriteLine(
    $"PASS: Terrain small-world lower clamp preserves GetWorldSize source branch " +
    $"(firstGroundY={firstGround.Y})");
}

static void LegacyTerrainRightBeachFeatureResetSourceCheck()
{
  WorldMetadata metadata = new(
    "terrain-right-beach-feature-reset",
    new WorldSeed(1456),
    width: 200,
    height: 150);
  LegacyTerrainRuntimeProfile runtimeProfile = new(
    WorldSurface: 30,
    RockLayer: 80,
    WorldSurfaceLow: 20,
    WorldSurfaceHigh: 40,
    RockLayerLow: 70,
    RockLayerHigh: 90,
    LeftBeachEnd: 0,
    RightBeachStart: 100,
    WaterLine: 100,
    LavaLine: 120)
  {
    InitialWorldSurface = 30,
    InitialRockLayer = 80
  };
  WorldGenerationRequest request = new(
    metadata,
    spawnX: 100,
    surfaceY: 30,
    rockLayerY: 80,
    terrainProfile: runtimeProfile);
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  WorldGenerationStateComponent state = new(metadata.WorldId);
  List<TileChangeCommand> commands = new();
  new TerrainBaseSystem().AppendLegacyCommands(
    world.CreateSnapshot(metadata),
    request,
    new TerrainProfileComponent(30, 80, metadata.Height - 1),
    ref state,
    commands);
  TileChangeCommand postBoundaryColumn = commands.First(command =>
    command.X == 104);
  if (postBoundaryColumn.Y != 28 || postBoundaryColumn.TileType != 0 ||
      postBoundaryColumn.FrameX != -1 || postBoundaryColumn.FrameY != -1)
  {
    throw new InvalidOperationException(
      $"Terrain right-beach feature reset diverged at x=104: y={postBoundaryColumn.Y}, " +
      $"type={postBoundaryColumn.TileType}, frameX={postBoundaryColumn.FrameX}, " +
      $"frameY={postBoundaryColumn.FrameY}.");
  }

  Console.WriteLine(
    "PASS: Terrain right-beach boundary unconditionally resets the source feature run");
}

static void LegacyTerrainSurfaceClampSourceCheck()
{
  LegacyTerrainSurfaceClampResult beach = LegacyTerrainSurfaceClampPolicy.Apply(
    surface: 400,
    columnX: 359,
    leftBeachEnd: 360,
    rightBeachStart: 3840,
    flatBeachPadding: 5,
    lowerSurface: 228,
    upperSurface: 312,
    beachSurfaceCap: 276);
  if (!beach.IsBeachColumn || beach.Surface != 276 || beach.ResetFeatureRun)
  {
    throw new InvalidOperationException("Terrain beach clamp diverged from TerrainPass predicate.");
  }

  LegacyTerrainSurfaceClampResult lower = LegacyTerrainSurfaceClampPolicy.Apply(
    surface: 200,
    columnX: 1000,
    leftBeachEnd: 360,
    rightBeachStart: 3840,
    flatBeachPadding: 5,
    lowerSurface: 228,
    upperSurface: 312,
    beachSurfaceCap: 276);
  LegacyTerrainSurfaceClampResult upper = LegacyTerrainSurfaceClampPolicy.Apply(
    surface: 400,
    columnX: 1000,
    leftBeachEnd: 360,
    rightBeachStart: 3840,
    flatBeachPadding: 5,
    lowerSurface: 228,
    upperSurface: 312,
    beachSurfaceCap: 276);
  LegacyTerrainSurfaceClampResult unchanged = LegacyTerrainSurfaceClampPolicy.Apply(
    surface: 260,
    columnX: 1000,
    leftBeachEnd: 360,
    rightBeachStart: 3840,
    flatBeachPadding: 5,
    lowerSurface: 228,
    upperSurface: 312,
    beachSurfaceCap: 276);
  if (lower.Surface != 228 || !lower.ResetFeatureRun ||
      upper.Surface != 312 || !upper.ResetFeatureRun ||
      unchanged.Surface != 260 || unchanged.ResetFeatureRun || unchanged.IsBeachColumn)
  {
    throw new InvalidOperationException(
      "Terrain interior clamp and feature-run reset diverged from TerrainPass predicate.");
  }

  Console.WriteLine(
    "PASS: Terrain surface clamp preserves beach and interior mutation boundaries");
}

static void LegacyTerrainColumnContractSourceCheck()
{
  IReadOnlyList<LegacyTerrainColumnMutation> mutations =
    LegacyTerrainColumnContract.PrepareFillColumn(
      worldHeight: 10,
      worldSurface: 3.5,
      rockLayer: 7.5);
  if (mutations.Count != 11 ||
      mutations[0] != new LegacyTerrainColumnMutation(0, false, null, -1, -1) ||
      mutations[2] != new LegacyTerrainColumnMutation(2, false, null, -1, -1) ||
      mutations[3] != new LegacyTerrainColumnMutation(3, false, null, -1, -1) ||
      mutations[4] != new LegacyTerrainColumnMutation(3, true, 0, -1, -1) ||
      mutations[8] != new LegacyTerrainColumnMutation(7, true, 0, -1, -1) ||
      mutations[9] != new LegacyTerrainColumnMutation(8, true, 1, -1, -1) ||
      mutations[10] != new LegacyTerrainColumnMutation(9, true, 1, -1, -1))
  {
    throw new InvalidOperationException(
      "Terrain FillColumn mutation order or inactive frame sentinels diverged from oracle.");
  }

  Console.WriteLine(
    "PASS: Terrain FillColumn preserves layer ordering and frame sentinels");

  IReadOnlyList<LegacyTerrainColumnTile> existingTiles = new[]
  {
    new LegacyTerrainColumnTile(true, 1),
    new LegacyTerrainColumnTile(true, 1),
    new LegacyTerrainColumnTile(true, 1),
    new LegacyTerrainColumnTile(false, 1),
    new LegacyTerrainColumnTile(true, 0),
    new LegacyTerrainColumnTile(true, 1),
    new LegacyTerrainColumnTile(false, 0),
    new LegacyTerrainColumnTile(true, 1)
  };
  IReadOnlyList<LegacyTerrainColumnMutation> retargeted =
    LegacyTerrainColumnContract.PrepareRetargetColumn(8, 2.5, existingTiles);
  if (retargeted.Count != 6 ||
      retargeted[0] != new LegacyTerrainColumnMutation(0, false, null, -1, -1) ||
      retargeted[2] != new LegacyTerrainColumnMutation(2, false, null, -1, -1) ||
      retargeted[3] != new LegacyTerrainColumnMutation(3, true, 0, -1, -1) ||
      retargeted[4] != new LegacyTerrainColumnMutation(4, true, 0, -1, -1) ||
      retargeted[5] != new LegacyTerrainColumnMutation(6, true, 0, -1, -1))
  {
    throw new InvalidOperationException(
      "Terrain RetargetColumn state-dependent mutation filtering diverged from oracle.");
  }

  Console.WriteLine(
    "PASS: Terrain RetargetColumn preserves state-dependent dirt conversion");
}

static void LegacyCavePassContractSourceCheck()
{
  IReadOnlyList<LegacyCavePassDefinition> schedule =
    LegacyCavePassContractDefinition.CreateDefaultSchedule();
  if (!schedule.SequenceEqual(LegacyCavePassContractDefinition.CreateDefaultSchedule()))
  {
    throw new InvalidOperationException("Cave pass defaults were not stable across registration.");
  }

  try
  {
    ((IList<LegacyCavePassDefinition>)schedule)[0] = schedule[0];
    throw new InvalidOperationException("Cave pass schedule projection was mutable.");
  }
  catch (NotSupportedException)
  {
  }

  string[] expectedNames =
  {
    "MountainCaves",
    "DirtLayerCaves",
    "RockLayerCaves",
    "SurfaceCaves",
    "WavyCaves"
  };
  if (schedule.Count != expectedNames.Length ||
      schedule.Select(pass => pass.Name).SequenceEqual(expectedNames) == false ||
      schedule.Any(pass => !pass.ResetsRandomFromWorldSeed ||
        !pass.UsesTileRunner || !pass.MutatesTiles))
  {
    throw new InvalidOperationException(
      "Cave pass schedule diverged from source pass order or mutation boundaries.");
  }

  LegacyCavePassExecutionCoordinator coordinator =
    new(schedule, worldSeed: 42);
  for (int index = 0; index < expectedNames.Length; index++)
  {
    LegacyCavePassResetCheckpoint checkpoint =
      coordinator.BeginNextPass(out LegacyPassRandomState random);
    if (checkpoint.PassName != expectedNames[index] || checkpoint.PassIndex != index ||
        checkpoint.WorldSeed != 42 || checkpoint.ResetOrdinal != index + 1 ||
        random.Next(1000) != new LegacyPassRandomState(42).Next(1000))
    {
      throw new InvalidOperationException(
        "Cave pass coordinator did not reset and order passes from the world seed.");
    }
  }

  coordinator.Complete();

  LegacyTileRunnerRequest request = new(
    x: 100,
    y: 200,
    strength: 3.0,
    steps: 25,
    tileType: -1,
    addTile: false,
    speedX: 0.5,
    speedY: 1.0,
    noYChange: false,
    overwrite: true,
    ignoreTileType: -1);
  if (request.TileType != -1 || request.AddTile || request.Steps != 25 ||
      request.Overwrite != true)
  {
    throw new InvalidOperationException(
      "Cave TileRunner request did not preserve source parameter shape.");
  }

  Console.WriteLine(
    "PASS: Cave pass schedule and TileRunner request preserve source boundaries");
}

static void LegacyCavePassVerticalRangeSourceCheck()
{
  WorldMetadata metadata = new(
    "cave-vertical-range",
    new WorldSeed(1456),
    width: 200,
    height: 150);
  LegacyTerrainRuntimeProfile runtimeProfile = new(
    WorldSurface: 30,
    RockLayer: 80,
    WorldSurfaceLow: 20,
    WorldSurfaceHigh: 40,
    RockLayerLow: 70,
    RockLayerHigh: 90,
    LeftBeachEnd: 0,
    RightBeachStart: 100,
    WaterLine: 100,
    LavaLine: 120)
  {
    InitialWorldSurface = 30,
    InitialRockLayer = 80
  };
  WorldGenerationRequest request = new(
    metadata,
    spawnX: 100,
    surfaceY: 30,
    rockLayerY: 80,
    terrainProfile: runtimeProfile);
  System.Reflection.MethodInfo resolver = typeof(LegacyCavePassSystem).GetMethod(
    "ResolveVerticalRange",
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static) ??
    throw new InvalidOperationException("Legacy cave vertical-range resolver was not found.");
  IReadOnlyList<LegacyTileRunnerPassInput> recipes =
    LegacyTileRunnerPassInputDefinition.CreateDefaultRecipes();
  (string RecipeName, int MinimumY, int MaximumY)[] expectedRanges =
  [
    ("surface-dirt", 0, 21),
    ("surface-high-dirt", 20, 41),
    ("rock-high-dirt", 40, 91),
    ("rock-layer-stone", 70, 150)
  ];
  foreach ((string recipeName, int minimumY, int maximumY) in expectedRanges)
  {
    LegacyTileRunnerPassInput recipe = recipes.First(input =>
      input.RecipeName == recipeName);
    ValueTuple<int, int> actual = (ValueTuple<int, int>)(resolver.Invoke(
      null,
      [recipe, request, metadata.Height]) ??
      throw new InvalidOperationException(
        $"Legacy cave vertical range resolver returned no value for {recipeName}."));
    if (actual != (minimumY, maximumY))
    {
      throw new InvalidOperationException(
        $"Legacy cave vertical range diverged for {recipeName}: " +
        $"actual=({actual.Item1},{actual.Item2}), expected=({minimumY},{maximumY}).");
    }
  }

  Console.WriteLine(
    "PASS: cave TileRunner ranges preserve runtime world-surface and rock-layer bounds");
}

static void LegacyTileRunnerMutationSourceCheck()
{
  LegacyTileRunnerMutationProfile carve =
    LegacyTileRunnerMutationPolicy.Classify(-1, addTile: false, noYChange: false);
  LegacyTileRunnerMutationProfile place =
    LegacyTileRunnerMutationPolicy.Classify(1, addTile: true, noYChange: true);
  LegacyTileRunnerMutationProfile liquidSensitive =
    LegacyTileRunnerMutationPolicy.Classify(59, addTile: true, noYChange: true);
  if (!carve.RemovesTile || carve.WritesTileType || carve.ActivatesTile ||
      carve.ClearsLiquid || carve.WritesWall ||
      !place.WritesTileType || !place.ActivatesTile || !place.ClearsLiquid ||
      !place.WritesWall || !liquidSensitive.WritesTileType ||
      !liquidSensitive.ActivatesTile || !liquidSensitive.ClearsLiquid ||
      liquidSensitive.WritesWall)
  {
    throw new InvalidOperationException(
      "TileRunner mutation classification diverged from source type/addTile/noYChange branches.");
  }

  Console.WriteLine(
    "PASS: TileRunner mutation classification preserves source type branches");
}

static void LegacyTileRunnerEnvelopeSourceCheck()
{
  LegacyTileRunnerEnvelope envelope = LegacyTileRunnerEnvelopePolicy.Advance(
    centerX: 0.0,
    centerY: 5.0,
    initialStrength: 10.0,
    totalSteps: 4,
    remainingSteps: 4.0,
    speedX: 1.5,
    speedY: -0.5,
    worldWidth: 20,
    worldHeight: 20);
  if (envelope.Strength != 10.0 || envelope.MinX != 1 || envelope.MaxXExclusive != 5 ||
      envelope.MinY != 1 || envelope.MaxYExclusive != 10 ||
      envelope.NextCenterX != 1.5 || envelope.NextCenterY != 4.5 ||
      envelope.RemainingSteps != 3.0)
  {
    throw new InvalidOperationException(
      "TileRunner envelope diverged from source strength scaling or boundary clipping.");
  }

  Console.WriteLine(
    "PASS: TileRunner envelope preserves strength scaling and interior clipping");
}

static void LegacyTileRunnerDistanceSourceCheck()
{
  if (!LegacyTileRunnerDistancePolicy.IsWithinManhattanEnvelope(
        4, 5, 5.0, 5.0, 10.0, 0) ||
      LegacyTileRunnerDistancePolicy.IsWithinManhattanEnvelope(
        10, 5, 5.0, 5.0, 10.0, 0) ||
      !LegacyTileRunnerDistancePolicy.IsWithinManhattanEnvelope(
        10, 5, 5.0, 5.0, 10.0, 10) ||
      LegacyTileRunnerDistancePolicy.IsWithinManhattanEnvelope(
        10, 5, 5.0, 5.0, 10.0, -10))
  {
    throw new InvalidOperationException(
      "TileRunner Manhattan distance predicate diverged from strict source threshold.");
  }

  Console.WriteLine(
    "PASS: TileRunner Manhattan distance predicate preserves strict random threshold");
}

static void LegacyTileRunnerTargetRegistrySourceCheck()
{
  if (LegacyTileRunnerTargetRegistry.IsStone(1) ||
      !LegacyTileRunnerTargetRegistry.IsStone(63) ||
      !LegacyTileRunnerTargetRegistry.IsStone(566) ||
      LegacyTileRunnerTargetRegistry.OreCount != 19 ||
      !LegacyTileRunnerTargetRegistry.IsOre(7) ||
      !LegacyTileRunnerTargetRegistry.IsOre(223) ||
      LegacyTileRunnerTargetRegistry.IsOre(1) ||
      LegacyTileRunnerTargetRegistry.IsOre(397))
  {
    throw new InvalidOperationException(
      "TileRunner target registries diverged from legacy Main.tileStone or TileID.Sets.Ore.");
  }

  Console.WriteLine(
    "PASS: TileRunner target registries preserve legacy Main.tileStone and TileID.Sets.Ore facts");
}

static void LegacyTileRunnerCandidateRegistrySourceCheck()
{
  if (LegacyTileRunnerCandidateRegistry.FrameImportantCount != 397 ||
      LegacyTileRunnerCandidateRegistry.TileCutCount != 41 ||
      !LegacyTileRunnerCandidateRegistry.IsFrameImportant(3) ||
      LegacyTileRunnerCandidateRegistry.IsTileCut(4) ||
      !LegacyTileRunnerCandidateRegistry.IsTileCut(3))
  {
    throw new InvalidOperationException(
      "TileRunner candidate registries diverged from legacy Main tile arrays.");
  }

  Console.WriteLine("PASS: TileRunner candidate registries preserve legacy Main tile array facts");
}

static void LegacyTileRunnerNoYChangeWallSourceCheck()
{
  WorldMetadata metadata = new("TileRunnerNoYChange", new WorldSeed(42), 200, 150);
  LegacyTileRunnerPassInput recipe = new(
    "TileRunner", "no-y-change", 1, false, 10, 11, 1, 2, "test", false, false, true);
  LegacyTileRunnerRequest preserveRequest = new(20, 10, 10.0, 1, 1, false, 0.0, 0.0, true,
    true, -1);
  LegacyTileRunnerPassInvocation preserveInvocation = new(
    recipe, preserveRequest, 20, 10, 10, 1, 0);
  WorldGrid preserveWorld = new(200, 150);
  for (int x = 0; x < preserveWorld.Width; x++)
  {
    for (int y = 0; y < preserveWorld.Height; y++)
    {
      _ = preserveWorld.TrySetTile(x, y, new WorldTile(IsActive: true, Type: 396));
    }
  }

  WorldGenerationStateComponent preserveState = new(1);
  List<TileChangeCommand> preserveCommands = new();
  LegacyTileRunnerTraversal.AppendCommands(
    preserveWorld.CreateSnapshot(metadata),
    preserveInvocation,
    new LegacyPassRandomState(42),
    worldSurfaceY: 20,
    rockLayerY: 30,
    ref preserveState,
    preserveCommands);
  TileChangeCommand wallCommand = preserveCommands.First(command =>
    command.Kind == TileChangeKind.SetWall);
  if (preserveCommands.Any(command => command.Kind == TileChangeKind.UpdateTileType) ||
      !new TileChangeCommitSystem().TryCommit(preserveWorld, preserveCommands, out _) ||
      preserveWorld.GetTile(wallCommand.X, wallCommand.Y).Type != 396 ||
      preserveWorld.GetTile(wallCommand.X, wallCommand.Y).WallType != 2)
  {
    throw new InvalidOperationException(
      "TileRunner noYChange did not write a dirt wall independently of tile preservation.");
  }

  LegacyTileRunnerRequest type59Request = new(20, 10, 10.0, 1, 59, false, 0.0, 0.0, true,
    true, -1);
  LegacyTileRunnerPassInvocation type59Invocation = new(
    recipe with { TileType = 59 }, type59Request, 20, 10, 10, 1, 0);
  WorldGenerationStateComponent type59State = new(2);
  List<TileChangeCommand> type59Commands = new();
  LegacyTileRunnerTraversal.AppendCommands(
    new WorldGrid(200, 150).CreateSnapshot(metadata),
    type59Invocation,
    new LegacyPassRandomState(42),
    worldSurfaceY: 20,
    rockLayerY: 30,
    ref type59State,
    type59Commands);
  if (type59Commands.Any(command => command.Kind == TileChangeKind.SetWall))
  {
    throw new InvalidOperationException(
      "TileRunner noYChange wrote a dirt wall for the type-59 exclusion.");
  }

  Console.WriteLine("PASS: TileRunner noYChange preserves independent dirt-wall semantics");
}

static void LegacySmallHolesPassSourceCheck()
{
  LegacySmallHolesPassDefinition definition = LegacySmallHolesPassDefinitionFactory.CreateDefault();
  if (definition.CalculateIterationCount(200, 150) != 45 ||
      definition.LiquidChanceDenominator != 5)
  {
    throw new InvalidOperationException("SmallHoles pass density or liquid chance diverged from source.");
  }

  LegacySmallHolesInvocationPair first = LegacySmallHolesInvocationFactory.Create(
    definition, new LegacyPassRandomState(42), 200, 150, 40);
  LegacySmallHolesInvocationPair second = LegacySmallHolesInvocationFactory.Create(
    definition, new LegacyPassRandomState(42), 200, 150, 40);
  if (first != second || (first.TileType != -1 && first.TileType != -2) ||
      first.First.TileType != first.TileType || first.Second.TileType != first.TileType ||
      first.First.Strength < 2 || first.First.Strength >= 5 ||
      first.First.Steps < 2 || first.First.Steps >= 20 ||
      first.Second.Strength < 8 || first.Second.Strength >= 15 ||
      first.Second.Steps < 7 || first.Second.Steps >= 30)
  {
    throw new InvalidOperationException(
      "SmallHoles invocation pair did not preserve source type and random ranges.");
  }

  Console.WriteLine("PASS: SmallHoles preserves deterministic type and TileRunner range inputs");
}

static void LegacySmallHolesBatchSourceCheck()
{
  WorldMetadata metadata = new("SmallHoles", new WorldSeed(42), 200, 150);
  WorldGrid world = new(200, 150);
  for (int x = 0; x < world.Width; x++)
  {
    for (int y = 0; y < world.Height; y++)
    {
      _ = world.TrySetTile(x, y, new WorldTile(IsActive: true, Type: 1));
    }
  }

  LegacyTileRunnerSnapshotExecutionContext context = new(
    WaterLine: 20,
    LavaLine: 100,
    WorldSurface: 40,
    LiquidType: 0,
    RemixWorld: false,
    RockLayer: 80,
    MaxTilesY: 150,
    IsOceanDepth: false,
    SourceLine: 11080);
  LegacySmallHolesPassDefinition definition = LegacySmallHolesPassDefinitionFactory.CreateDefault();
  LegacyTileRunnerSnapshotBatchExecutionResult first = LegacySmallHolesPassExecution.Execute(
    world.CreateSnapshot(metadata), definition, new LegacyPassRandomState(42), context, 10, 1);
  LegacyTileRunnerSnapshotBatchExecutionResult second = LegacySmallHolesPassExecution.Execute(
    world.CreateSnapshot(metadata), definition, new LegacyPassRandomState(42), context, 10, 1);
  bool matchingTileCommands = first.Commands.TileCommands.SequenceEqual(second.Commands.TileCommands);
  bool matchingLiquidCommands = first.Commands.LiquidCommands.SequenceEqual(
    second.Commands.LiquidCommands);
  if (first.Commands.PassName != "SmallHoles" || first.Provenance.Count != 2 ||
      first.Commands.NextSequence != 13 || !matchingTileCommands || !matchingLiquidCommands)
  {
    throw new InvalidOperationException(
      $"SmallHoles batch: pass={first.Commands.PassName}, provenance={first.Provenance.Count}, " +
      $"next={first.Commands.NextSequence}, tileMatch={matchingTileCommands}, " +
      $"liquidMatch={matchingLiquidCommands}.");
  }

  WorldGenerationStateComponent skyblockState = new(42);
  LegacyPassRandomState skyblockRandom = new(42);
  List<TileChangeCommand> skyblockTileCommands = new();
  List<LiquidChangeCommand> skyblockLiquidCommands = new();
  LegacySmallHolesPassExecution.AppendEnvelopeCommands(
    world.CreateSnapshot(metadata),
    definition,
    skyblockRandom,
    new LegacyTileRunnerLiquidContext(
      WaterLine: 20,
      LavaLine: 100,
      LiquidType: 0,
      RemixWorld: false,
      RockLayerY: 80,
      WorldHeight: 150,
      IsOceanDepth: false),
    worldSurfaceY: 40,
    rockLayerY: 80,
    iterationCount: 1,
    isSkyblockWorld: true,
    ref skyblockState,
    skyblockTileCommands,
    skyblockLiquidCommands);
  if (skyblockState.Stage != WorldGenerationStage.Created || skyblockState.NextSequence != 0 ||
      skyblockRandom.SampleCount != 0 || skyblockTileCommands.Count != 0 ||
      skyblockLiquidCommands.Count != 0)
  {
    throw new InvalidOperationException(
      "SmallHoles did not honor the Skyblock deny-generation guard.");
  }

  Console.WriteLine("PASS: SmallHoles emits deterministic typed command batches");
}

static void LegacyTileRunnerLiquidTraversalSourceCheck()
{
  WorldMetadata metadata = new("TileRunnerLiquid", new WorldSeed(42), 200, 150);
  WorldGrid world = new(200, 150);
  for (int x = 0; x < world.Width; x++)
  {
    for (int y = 0; y < world.Height; y++)
    {
      _ = world.TrySetTile(x, y, new WorldTile(IsActive: true, Type: 1));
    }
  }

  LegacyTileRunnerPassInput recipe = new(
    "SmallHoles", "liquid-envelope", -2, false, 10, 11, 1, 2, "test", false, false, true);
  LegacyTileRunnerPassInvocation invocation = new(
    recipe,
    new LegacyTileRunnerRequest(100, 120, 10.0, 1, -2, false, 0.0, 0.0, false, true, -1),
    100,
    120,
    10,
    1,
    0);
  WorldGenerationStateComponent state = new(1);
  List<TileChangeCommand> tileCommands = new();
  List<LiquidChangeCommand> liquidCommands = new();
  LegacyTileRunnerTraversal.AppendCommands(
    world.CreateSnapshot(metadata),
    invocation,
    new LegacyPassRandomState(42),
    worldSurfaceY: 40,
    rockLayerY: 80,
    ref state,
    tileCommands,
    new LegacyTileRunnerLiquidContext(20, 100, 0, false, 80, 150, false),
    liquidCommands);
  if (liquidCommands.Count == 0 || tileCommands.Count < liquidCommands.Count ||
      !tileCommands.All(command => command.Kind == TileChangeKind.Kill && command.PreserveLiquid))
  {
    throw new InvalidOperationException("TileRunner -2 did not emit paired liquid-preserving kills.");
  }

  LegacyTileRunnerPassCommandBatch batch = new(
    "SmallHoles", tileCommands.AsReadOnly(), liquidCommands.AsReadOnly(), state.NextSequence);
  if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
        world,
        batch,
        new[] { new LiquidDefinition("water", 0, byte.MaxValue), new LiquidDefinition("lava", 1, byte.MaxValue) },
        out _) ||
      !liquidCommands.All(command =>
        !world.GetTile(command.X, command.Y).IsActive &&
        world.GetTile(command.X, command.Y).LiquidAmount == byte.MaxValue))
  {
    throw new InvalidOperationException("TileRunner -2 batch did not commit inactive liquid cells.");
  }

  Console.WriteLine("PASS: TileRunner -2 envelope emits and commits typed liquid commands");
}

static void LegacyTileRunnerCandidateSourceCheck()
{
  LegacyTileRunnerTileState important = new(true, 100, true, false);
  LegacyTileRunnerTileState cut = new(true, 100, true, true);
  LegacyTileRunnerTileState ignored = new(true, 42, false, false);
  LegacyTileRunnerTileState empty = new(false, 0, false, false);
  if (LegacyTileRunnerCandidateQuery.ShouldMutate(
        important, 5, 5, 5.0, 5.0, 10.0, 0, -1) ||
      !LegacyTileRunnerCandidateQuery.ShouldMutate(
        cut, 5, 5, 5.0, 5.0, 10.0, 0, -1) ||
      LegacyTileRunnerCandidateQuery.ShouldMutate(
        ignored, 5, 5, 5.0, 5.0, 10.0, 0, 42) ||
      !LegacyTileRunnerCandidateQuery.ShouldMutate(
        empty, 5, 5, 5.0, 5.0, 10.0, 0, 42))
  {
    throw new InvalidOperationException(
      "TileRunner candidate query diverged from frame-important, cut, ignore, or distance gates.");
  }

  Console.WriteLine(
    "PASS: TileRunner candidate query preserves protected and ignore skip gates");
}

static void LegacyGenerationClearabilitySourceCheck()
{
  IReadOnlySet<int> excludedTileTypes = LegacyGenerationClearabilityPolicy.RegisterDefaults();
  if (excludedTileTypes.Count != 17 || !excludedTileTypes.Contains(41) ||
      excludedTileTypes.Contains(0) ||
      !excludedTileTypes.SetEquals(LegacyGenerationClearabilityPolicy.RegisterDefaults()))
  {
    throw new InvalidOperationException("Generation clearability defaults were not stable or bounded.");
  }

  if (!LegacyGenerationClearabilityPolicy.CanClear(0, false) ||
      LegacyGenerationClearabilityPolicy.CanClear(41, false) ||
      LegacyGenerationClearabilityPolicy.CanClear(483, false) ||
      LegacyGenerationClearabilityPolicy.CanClear(0, true) ||
      LegacyGenerationClearabilityPolicy.CanClear(-1, false))
  {
    throw new InvalidOperationException(
      "Generation clearability policy diverged from TileID set and dungeon veto semantics.");
  }

  Console.WriteLine(
    "PASS: generation clearability preserves static tile exclusions and dynamic veto");
}

static void LegacyTileRunnerOverrideSourceCheck()
{
  LegacyTileRunnerOverrideContext stoneOnDirt = new(
    0, 1, true, true, false, true, false, 100, 200, 0, true);
  LegacyTileRunnerOverrideContext oreOnOre = new(
    396, 1, true, false, true, true, false, 200, 100, 0, true);
  LegacyTileRunnerOverrideContext oreOnNonOre = new(
    396, 1, true, false, false, true, false, 200, 100, 0, true);
  LegacyTileRunnerOverrideContext desertSand = new(
    53, 59, true, false, false, true, true, 500, 400, 0, true);
  LegacyTileRunnerOverrideContext surfaceSand = new(
    53, 1, true, false, false, true, false, 100, 200, 0, true);
  if (!LegacyTileRunnerOverridePolicy.MustPreserveExistingTile(stoneOnDirt) ||
      LegacyTileRunnerOverridePolicy.MustPreserveExistingTile(oreOnOre) ||
      !LegacyTileRunnerOverridePolicy.MustPreserveExistingTile(oreOnNonOre) ||
      !LegacyTileRunnerOverridePolicy.MustPreserveExistingTile(desertSand) ||
      !LegacyTileRunnerOverridePolicy.MustPreserveExistingTile(surfaceSand))
  {
    throw new InvalidOperationException(
      "TileRunner override policy diverged from source type-specific preservation branches.");
  }

  Console.WriteLine(
    "PASS: TileRunner override policy preserves source type-specific branches");
}

static void LegacyTileRunnerSideEffectSourceCheck()
{
  LegacyTileRunnerSideEffectProfile liquid =
    LegacyTileRunnerSideEffectPolicy.Classify(-2, false, false, true, 10, 20, 80, 50);
  LegacyTileRunnerSideEffectProfile positive =
    LegacyTileRunnerSideEffectPolicy.Classify(1, true, true, false, 40, 20, 80, 50);
  LegacyTileRunnerSideEffectProfile aboveSurface =
    LegacyTileRunnerSideEffectPolicy.Classify(1, true, true, false, 60, 20, 80, 50);
  LegacyTileRunnerSideEffectProfile special =
    LegacyTileRunnerSideEffectPolicy.Classify(59, true, true, false, 40, 20, 80, 50);
  LegacyTileRunnerSideEffectProfile type59LiquidCleanup =
    LegacyTileRunnerSideEffectPolicy.Classify(
      59, false, false, true, 90, 20, 80, 50, 0, false, 200, 1000, false,
      currentLiquidAmount: 1);
  LegacyTileRunnerSideEffectProfile remixOcean =
    LegacyTileRunnerSideEffectPolicy.Classify(
      -2, false, false, true, 900, 20, 120, 50, 3, true, 200, 1200, true);
  LegacyTileRunnerSideEffectProfile lavaInjection =
    LegacyTileRunnerSideEffectPolicy.Classify(
      -2, false, false, true, 130, 20, 120, 50, 0, false, 200, 1000, false);
  LegacyTileRunnerCommandBatch emitted = LegacyTileRunnerCommandEmitter.Create(
    7, 130, 11, 12, -2, false, false, true, 20, 120, 50, 0, false, 200, 1000, false);
  LegacyTileRunnerCommandBatch placed = LegacyTileRunnerCommandEmitter.Create(
    8, 40, 21, 22, 1, true, true, false, 20, 120, 50, 0, false, 200, 1000, false);
  LegacyTileRunnerCommandBatch type59Cleanup = LegacyTileRunnerCommandEmitter.Create(
    8, 90, 25, 26, 59, false, false, true, 20, 80, 50, 0, false, 200, 1000, false,
    currentLiquidAmount: 1);
  LegacyTileRunnerLiquidProjection waterProjection =
    LegacyTileRunnerLiquidProjectionPolicy.Resolve(0, false);
  LegacyTileRunnerLiquidProjection lavaProjection =
    LegacyTileRunnerLiquidProjectionPolicy.Resolve(0, true);
  IReadOnlyList<LegacyTileRunnerPassInput> passRecipes =
    LegacyTileRunnerPassInputDefinition.CreateDefaultRecipes();
  if (!passRecipes.SequenceEqual(LegacyTileRunnerPassInputDefinition.CreateDefaultRecipes()))
  {
    throw new InvalidOperationException("TileRunner pass defaults were not stable across registration.");
  }

  try
  {
    ((IList<LegacyTileRunnerPassInput>)passRecipes)[0] = passRecipes[0];
    throw new InvalidOperationException("TileRunner pass projection was mutable.");
  }
  catch (NotSupportedException)
  {
  }

  LegacyTileRunnerPassInputDefinition.Validate(passRecipes);
  LegacyTileRunnerPassInput surfaceDirtRecipe = passRecipes.Single(recipe =>
    recipe.RecipeName == "surface-dirt");
  LegacyTileRunnerPassInput rockRecipe = passRecipes.Single(recipe =>
    recipe.RecipeName == "rock-layer-stone");
  if (passRecipes.Count != 5 || surfaceDirtRecipe.TileType != 1 ||
      surfaceDirtRecipe.MaximumStrengthExclusive != 15 ||
      surfaceDirtRecipe.MaximumStepsExclusive != 40 ||
      rockRecipe.VerticalRange != "rockLayerLow..maxTilesY" ||
      !passRecipes.All(recipe => recipe.ResetsRandomFromWorldSeed))
  {
    throw new InvalidOperationException(
      "TileRunner pass input recipes did not preserve source call ranges.");
  }
  LegacyTileRunnerPassInvocation firstInvocation =
    LegacyTileRunnerPassInvocationFactory.Create(
      surfaceDirtRecipe,
      new LegacyPassRandomState(42),
      0,
      8400,
      0,
      1200);
  LegacyTileRunnerPassInvocation secondInvocation =
    LegacyTileRunnerPassInvocationFactory.Create(
      surfaceDirtRecipe,
      new LegacyPassRandomState(42),
      0,
      8400,
      0,
      1200);
  if (firstInvocation.RandomDrawCount != 4 ||
      firstInvocation.Request.X != firstInvocation.XDraw ||
      firstInvocation.Request.Strength != firstInvocation.StrengthDraw ||
      firstInvocation.Request.Steps != firstInvocation.StepsDraw ||
      firstInvocation.Request.X != secondInvocation.Request.X ||
      firstInvocation.Request.Y != secondInvocation.Request.Y ||
      firstInvocation.Request.Strength != secondInvocation.Request.Strength ||
      firstInvocation.Request.Steps != secondInvocation.Request.Steps)
  {
    throw new InvalidOperationException(
      "TileRunner invocation did not preserve deterministic pass-scoped draw order.");
  }
  LegacyTileRunnerCommandBatch invocationCommands =
    LegacyTileRunnerCommandEmitter.Create(
      firstInvocation.Request.X,
      firstInvocation.Request.Y,
      31,
      32,
      firstInvocation.Request.TileType,
      firstInvocation.Request.AddTile,
      firstInvocation.Request.NoYChange,
      true,
      20,
      120,
      50,
      0,
      false,
      200,
      1000,
      false);
  LegacyTileRunnerInvocationProvenance provenance =
    LegacyTileRunnerInvocationProvenanceFactory.Create(
      firstInvocation,
      invocationCommands,
      invocationIndex: 0,
      sourceLine: 12242);
  if (provenance.PassName != "DirtLayerCaves" ||
      provenance.RecipeName != "surface-dirt" || provenance.InvocationIndex != 0 ||
      provenance.SourceLine != 12242 ||
      provenance.Request.X != firstInvocation.Request.X)
  {
    throw new InvalidOperationException(
      "TileRunner command provenance did not preserve pass and source ownership.");
  }
  LegacyTileRunnerCommandBatch secondInvocationCommands =
    LegacyTileRunnerCommandEmitter.Create(
      secondInvocation.Request.X,
      secondInvocation.Request.Y,
      33,
      34,
      secondInvocation.Request.TileType,
      secondInvocation.Request.AddTile,
      secondInvocation.Request.NoYChange,
      true,
      20,
      120,
      50,
      0,
      false,
      200,
      1000,
      false);
  LegacyTileRunnerInvocationProvenance secondProvenance =
    LegacyTileRunnerInvocationProvenanceFactory.Create(
      secondInvocation,
      secondInvocationCommands,
      invocationIndex: 1,
      sourceLine: 12242);
  LegacyTileRunnerPassProvenance passProvenance =
    LegacyTileRunnerPassProvenanceFactory.Create(
      "DirtLayerCaves",
      new[] { secondProvenance, provenance });
  if (passProvenance.Invocations.Count != 2 ||
      passProvenance.TileCommandCount != 2 || passProvenance.LiquidCommandCount != 0 ||
      passProvenance.Invocations[0].InvocationIndex != 0)
  {
    throw new InvalidOperationException(
      "TileRunner pass provenance did not aggregate invocation ownership.");
  }
  LegacyTileRunnerPassExecutionLedger executionLedger =
    new("DirtLayerCaves", "surface-dirt", expectedInvocationCount: 2);
  executionLedger.Append(provenance);
  executionLedger.Append(secondProvenance);
  if (executionLedger.RecordedInvocationCount != 2 ||
      executionLedger.Complete().Count != 2)
  {
    throw new InvalidOperationException(
      "TileRunner execution ledger did not complete its bounded invocation count.");
  }
  LegacyTileRunnerPassInput highDirtRecipe = passRecipes.Single(recipe =>
    recipe.RecipeName == "surface-high-dirt");
  LegacyTileRunnerPassInvocation highDirtInvocation =
    LegacyTileRunnerPassInvocationFactory.Create(
      highDirtRecipe,
      new LegacyPassRandomState(42),
      0,
      8400,
      0,
      1200);
  LegacyTileRunnerInvocationProvenance highDirtProvenance =
    LegacyTileRunnerInvocationProvenanceFactory.Create(
      highDirtInvocation,
      invocationCommands,
      invocationIndex: 0,
      sourceLine: 12260);
  LegacyTileRunnerPassExecutionLedger highDirtLedger =
    new("DirtLayerCaves", "surface-high-dirt", expectedInvocationCount: 1);
  highDirtLedger.Append(highDirtProvenance);
  LegacyTileRunnerPassExecutionSetResult executionSet =
    LegacyTileRunnerPassExecutionSetFactory.Complete(
      "DirtLayerCaves",
      new[] { executionLedger, highDirtLedger });
  if (executionSet.Recipes.Count != 2 || executionSet.InvocationCount != 3 ||
      executionSet.TileCommandCount != 3)
  {
    throw new InvalidOperationException(
      "TileRunner multi-recipe execution set did not complete pass handoff.");
  }
  WorldGridSnapshot executionSnapshot =
    new WorldGrid(8400, 1200, initializeLegacyEmptyFrames: true)
      .CreateSnapshot(new WorldMetadata("worldgen", new WorldSeed(42), 8400, 1200));
  LegacyTileRunnerInvocationProvenance snapshotProvenance =
    LegacyTileRunnerSnapshotExecution.Execute(
      executionSnapshot,
      firstInvocation,
      new LegacyTileRunnerSnapshotExecutionContext(
        20,
        120,
        50,
        0,
        false,
        200,
        1000,
        false,
        12242),
      41,
      42,
      0);
  if (snapshotProvenance.SourceLine != 12242 ||
      snapshotProvenance.Request.X != firstInvocation.Request.X ||
      snapshotProvenance.Commands.TileCommand?.Sequence != 41)
  {
    throw new InvalidOperationException(
      "TileRunner snapshot execution did not preserve request-to-command provenance.");
  }
  LegacyTileRunnerSnapshotBatchExecutionResult snapshotBatch =
    LegacyTileRunnerSnapshotBatchExecution.Execute(
      executionSnapshot,
      "DirtLayerCaves",
      new[] { firstInvocation, secondInvocation },
      new LegacyTileRunnerSnapshotExecutionContext(
        20,
        120,
        50,
        0,
        false,
        200,
        1000,
        false,
        12242),
      startingSequence: 51);
  if (snapshotBatch.Provenance.Count != 2 ||
      snapshotBatch.Commands.TileCommands.Count != 2 ||
      snapshotBatch.Commands.NextSequence != 54 ||
      snapshotBatch.Provenance[1].InvocationIndex != 1)
  {
    throw new InvalidOperationException(
      "TileRunner snapshot batch did not preserve bounded sequence aggregation.");
  }
  WorldGrid snapshotCommitWorld = WorldGrid.FromSnapshot(executionSnapshot);
  if (!LegacyTileRunnerSnapshotCommitHandoff.TryCommit(
        snapshotCommitWorld,
        snapshotBatch,
        new[]
        {
          new LiquidDefinition("water", 0, byte.MaxValue),
          new LiquidDefinition("lava", 1, byte.MaxValue)
        },
        out LegacyTileRunnerCommandCommitResult snapshotCommitResult) ||
      !snapshotCommitResult.Succeeded || snapshotCommitResult.AppliedTileCount != 2 ||
      !snapshotCommitWorld.GetTile(firstInvocation.Request.X, firstInvocation.Request.Y).IsActive ||
      !snapshotCommitWorld.GetTile(secondInvocation.Request.X, secondInvocation.Request.Y).IsActive)
  {
    throw new InvalidOperationException(
      "TileRunner snapshot commit handoff did not commit the provenance-matched batch.");
  }
  LegacyTileRunnerSnapshotFingerprintResult generatedFingerprint =
    LegacyTileRunnerSnapshotFingerprint.Create(executionSnapshot);
  LegacyTileRunnerSnapshotFingerprintResult comparedFingerprint =
    LegacyTileRunnerSnapshotFingerprint.Create(
      executionSnapshot,
      generatedFingerprint.GeneratedFingerprint);
  LegacyTileRunnerSnapshotFingerprintResult mismatchFingerprint =
    LegacyTileRunnerSnapshotFingerprint.Create(executionSnapshot, "oracle-not-compared");
  if (generatedFingerprint.Compared || comparedFingerprint.Matches != true ||
      mismatchFingerprint.Compared != true || mismatchFingerprint.Matches ||
      generatedFingerprint.GeneratedFingerprint.Length != 64)
  {
    throw new InvalidOperationException(
      "TileRunner snapshot fingerprint comparison did not preserve explicit parity status.");
  }
  Dictionary<string, string> generatedStages = new()
  {
    ["Terrain"] = generatedFingerprint.GeneratedFingerprint,
    ["Cave"] = comparedFingerprint.GeneratedFingerprint
  };
  LegacyStageFingerprintComparisonResult missingOracleStages =
    LegacyStageFingerprintComparison.Compare(generatedStages, oracle: null);
  LegacyStageFingerprintComparisonResult comparedStages =
    LegacyStageFingerprintComparison.Compare(
      generatedStages,
      new Dictionary<string, string>
      {
        ["Terrain"] = generatedFingerprint.GeneratedFingerprint,
        ["Cave"] = "oracle-mismatch"
      });
  if (missingOracleStages.Compared || missingOracleStages.MissingOracleStageCount != 2 ||
      comparedStages.ComparedStageCount != 2 || comparedStages.MismatchCount != 1 ||
      comparedStages.Matches)
  {
    throw new InvalidOperationException(
      "Stage fingerprint comparison did not account for missing and mismatched oracle stages.");
  }
  LegacyTileRunnerPassExecutionLedger undercountedLedger =
    new("DirtLayerCaves", "surface-dirt", expectedInvocationCount: 2);
  undercountedLedger.Append(provenance);
  try
  {
    _ = undercountedLedger.Complete();
    throw new InvalidOperationException(
      "TileRunner execution ledger accepted an undercounted pass.");
  }
  catch (InvalidOperationException exception) when (exception.Message.Contains("reach"))
  {
  }
  IReadOnlyList<LegacyTileRunnerPassLoopDefinition> loopDefinitions =
    LegacyTileRunnerPassLoopContractDefinition.CreateDefault();
  if (!loopDefinitions.SequenceEqual(LegacyTileRunnerPassLoopContractDefinition.CreateDefault()))
  {
    throw new InvalidOperationException("TileRunner loop defaults were not stable across registration.");
  }

  try
  {
    ((IList<LegacyTileRunnerPassLoopDefinition>)loopDefinitions)[0] = loopDefinitions[0];
    throw new InvalidOperationException("TileRunner loop projection was mutable.");
  }
  catch (NotSupportedException)
  {
  }

  LegacyTileRunnerPassLoopDefinition surfaceLoop = loopDefinitions.Single(definition =>
    definition.RecipeName == "surface-dirt");
  LegacyTileRunnerPassLoopDefinition rockLoop = loopDefinitions.Single(definition =>
    definition.RecipeName == "rock-high-dirt");
  LegacyTileRunnerPassLoopDefinition rockLayerLoop = loopDefinitions.Single(definition =>
    definition.RecipeName == "rock-layer-stone");
  LegacyTileRunnerPassLoopDefinition surfaceCaveLoop = loopDefinitions.Single(definition =>
    definition.RecipeName == "surface-desert");
  if (surfaceLoop.CalculateInvocationCount(200, 150, remixWorld: false) != 5 ||
      rockLoop.CalculateInvocationCount(200, 150, remixWorld: false) != 135 ||
      rockLayerLoop.CalculateInvocationCount(200, 150, remixWorld: false) != 150 ||
      surfaceCaveLoop.CalculateInvocationCount(200, 150, remixWorld: false) != 1 ||
      loopDefinitions.Count != 5)
  {
    throw new InvalidOperationException(
      "TileRunner pass loop contract did not preserve source density cardinality.");
  }
  WorldGrid commitWorld = new(200, 150, initializeLegacyEmptyFrames: true);
  if (!commitWorld.TrySetTile(
        7,
        130,
        new WorldTile(IsActive: true, Type: 1, FrameX: 0, FrameY: 0)))
  {
    throw new InvalidOperationException("TileRunner commit fixture could not seed its tile.");
  }

  if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
        commitWorld,
        emitted,
        new[]
        {
          new LiquidDefinition("water", 0, byte.MaxValue),
          new LiquidDefinition("lava", 1, byte.MaxValue)
        },
        out LegacyTileRunnerCommandCommitResult commitResult) ||
      !commitResult.Succeeded || commitResult.AppliedTileCount != 1 ||
      commitResult.AppliedLiquidCount != 1 ||
      commitWorld.GetTile(7, 130).IsActive ||
      commitWorld.GetTile(7, 130).LiquidAmount != byte.MaxValue ||
      commitWorld.GetTile(7, 130).LiquidType != 1)
  {
    throw new InvalidOperationException(
      "TileRunner command commit boundary did not apply ordered tile and lava commands.");
  }

  LegacyTileRunnerCommandBatch invalidBatch = new(
    emitted.TileCommand,
    new LiquidChangeCommand(12, 7, 130, byte.MaxValue, 9),
    true);
  if (LegacyTileRunnerCommandCommitBoundary.TryCommit(
        commitWorld,
        invalidBatch,
        new[] { new LiquidDefinition("water", 0, byte.MaxValue) },
        out LegacyTileRunnerCommandCommitResult invalidCommit) ||
      invalidCommit.FailureReason is null || commitWorld.GetTile(7, 130).IsActive)
  {
    throw new InvalidOperationException(
      "TileRunner command commit boundary did not reject invalid liquid before mutation.");
  }

  LegacyTileRunnerCommandBatch secondBatch = LegacyTileRunnerCommandEmitter.Create(
    8, 40, 23, 24, 1, true, true, false, 20, 120, 50, 0, false, 200, 1000, false);
  LegacyTileRunnerPassCommandBatch passBatch =
    LegacyTileRunnerPassCommandBatchFactory.Create(
      "SurfaceCaves",
      new[] { secondBatch, emitted },
      startingSequence: 11);
  if (passBatch.PassName != "SurfaceCaves" ||
      passBatch.TileCommands.Count != 2 || passBatch.LiquidCommands.Count != 2 ||
      passBatch.TileCommands[0].Sequence != 11 ||
      passBatch.LiquidCommands[0].Sequence != 12 || passBatch.NextSequence != 25)
  {
    throw new InvalidOperationException(
      "TileRunner pass batch did not preserve global command ordering and ownership.");
  }

  WorldGrid passCommitWorld = new(200, 150, initializeLegacyEmptyFrames: true);
  if (!passCommitWorld.TrySetTile(
        7,
        130,
        new WorldTile(IsActive: true, Type: 1, FrameX: 0, FrameY: 0)) ||
      !passCommitWorld.TrySetTile(
        8,
        40,
        new WorldTile(IsActive: true, Type: 2, FrameX: 0, FrameY: 0)))
  {
    throw new InvalidOperationException("TileRunner pass commit fixture could not seed tiles.");
  }

  if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
        passCommitWorld,
        passBatch,
        new[]
        {
          new LiquidDefinition("water", 0, byte.MaxValue),
          new LiquidDefinition("lava", 1, byte.MaxValue)
        },
        out LegacyTileRunnerCommandCommitResult passCommitResult) ||
      !passCommitResult.Succeeded || passCommitResult.AppliedTileCount != 2 ||
      passCommitResult.AppliedLiquidCount != 2 ||
      passCommitWorld.GetTile(7, 130).IsActive ||
      !passCommitWorld.GetTile(8, 40).IsActive ||
      passCommitWorld.GetTile(8, 40).Type != 1 ||
      passCommitWorld.GetTile(8, 40).LiquidAmount != 0)
  {
    throw new InvalidOperationException(
      "TileRunner pass commit boundary did not commit the flattened command batch.");
  }

  LegacyTileRunnerCommandBatch duplicateSequence = new(
    new TileChangeCommand(11, 9, 40, TileChangeKind.Place, 1),
    null,
    false);
  try
  {
    _ = LegacyTileRunnerPassCommandBatchFactory.Create(
      "SurfaceCaves",
      new[] { emitted, duplicateSequence },
      startingSequence: 11);
    throw new InvalidOperationException(
      "TileRunner pass batch accepted a repeated cross-kind sequence.");
  }
  catch (InvalidOperationException exception) when (exception.Message.Contains("repeated"))
  {
  }
  if (!liquid.InjectsLiquidBeforeClear || !liquid.ClearsActiveTile ||
      liquid.InjectedLiquidType != 0 || liquid.SuppressedByRemixOceanDepth ||
      liquid.ClearsLiquidOnActivation || !positive.ClearsLiquidOnActivation ||
      !positive.ClearsLavaOnActivation || !positive.WritesSurfaceWall ||
      !aboveSurface.ClearsLiquidOnActivation || aboveSurface.WritesSurfaceWall ||
      special.WritesSurfaceWall || !type59LiquidCleanup.ClearsLiquidForType59 ||
      !remixOcean.InjectsLiquidBeforeClear ||
      remixOcean.SetsLava || !remixOcean.SuppressedByRemixOceanDepth ||
      !lavaInjection.InjectsLiquidBeforeClear || lavaInjection.InjectedLiquidType != 0 ||
      !lavaInjection.SetsLava)
  {
    throw new InvalidOperationException(
      "TileRunner liquid/wall side-effect profile diverged from source branches.");
  }

  if (emitted.TileCommand?.Kind != TileChangeKind.Kill ||
      emitted.TileCommand?.PreserveLiquid != true ||
      emitted.LiquidCommand?.Type != 1 || emitted.LiquidCommand?.Amount != byte.MaxValue ||
      !emitted.SetsLava ||
      waterProjection.CommandLiquidType != 0 || lavaProjection.CommandLiquidType != 1 ||
      placed.TileCommand?.Kind != TileChangeKind.Place ||
      placed.TileCommand?.WallType != 1 || placed.LiquidCommand?.Amount != 0 ||
      type59Cleanup.TileCommand?.Kind != TileChangeKind.Place ||
      type59Cleanup.LiquidCommand?.Amount != 0)
  {
    throw new InvalidOperationException(
      "TileRunner command emission did not preserve typed liquid and tile ordering contracts.");
  }

  Console.WriteLine(
    "PASS: TileRunner liquid and wall side effects preserve source branches");
}

static void LegacyTileRunnerRandomAdjustmentSourceCheck()
{
  LegacyTileRunnerRandomAdjustment drunk =
    LegacyTileRunnerRandomAdjustmentPolicy.Apply(
      10.0, 100, 1, false, true, false, false, 20, -10, 0);
  LegacyTileRunnerRandomAdjustment remix =
    LegacyTileRunnerRandomAdjustmentPolicy.Apply(
      10.0, 100, 1, false, false, true, false, -20, 0, 0);
  LegacyTileRunnerRandomAdjustment good =
    LegacyTileRunnerRandomAdjustmentPolicy.Apply(
      10.0, 100, 1, false, false, false, true, 20, 0, 2);
  LegacyTileRunnerRandomAdjustment excluded =
    LegacyTileRunnerRandomAdjustmentPolicy.Apply(
      10.0, 100, 57, false, false, false, true, 20, 0, 2);
  if (drunk.Strength != 12.0 || drunk.Steps != 90 ||
      remix.Strength != 8.0 || remix.Steps != 100 ||
      good.Strength != 13.0 || good.Steps != 102 ||
      excluded.Strength != 10.0 || excluded.Steps != 100)
  {
    throw new InvalidOperationException(
      "TileRunner random adjustment policy diverged from world-variant branches.");
  }

  Console.WriteLine(
    "PASS: TileRunner random adjustment preserves world-variant consumption branches");
}

static void LegacyTileRunnerInitializationSourceCheck()
{
  LegacyTileRunnerInitialization randomDirection =
    LegacyTileRunnerInitializationPolicy.Create(
      0.0, 0.0, -5, 7, 1, 1, false, false, false, false, false, false);
  LegacyTileRunnerInitialization explicitDirection =
    LegacyTileRunnerInitializationPolicy.Create(
      2.0, -1.0, -5, 7, 1, 1, false, false, false, false, false, false);
  LegacyTileRunnerInitialization bees =
    LegacyTileRunnerInitializationPolicy.Create(
      0.0, 0.0, 0, 0, 0, 1, true, true, false, false, false, false);
  LegacyTileRunnerInitialization good =
    LegacyTileRunnerInitializationPolicy.Create(
      0.0, 0.0, 0, 0, 1, 0, false, false, false, false, false, true);
  LegacyTileRunnerInitialization special =
    LegacyTileRunnerInitializationPolicy.Create(
      0.0, 0.0, 0, 0, 1, 0, false, true, true, true, true, false);
  if (Math.Abs(randomDirection.DirectionX + 0.5) > 0.0001 ||
      Math.Abs(randomDirection.DirectionY - 0.7) > 0.0001 ||
      !randomDirection.ConsumedDirectionRandom || randomDirection.LiquidType != 0 ||
      explicitDirection.DirectionX != 2.0 || explicitDirection.DirectionY != -1.0 ||
      explicitDirection.ConsumedDirectionRandom || bees.LiquidType != 2 ||
      !bees.ConsumedLiquidTypeRandom || good.LiquidType != 1 ||
      special.LiquidType != 3)
  {
    throw new InvalidOperationException(
      $"TileRunner initialization policy diverged: random=({randomDirection.DirectionX}," +
      $"{randomDirection.DirectionY},{randomDirection.LiquidType}), " +
      $"explicit=({explicitDirection.DirectionX},{explicitDirection.DirectionY}), " +
      $"bees={bees.LiquidType}, good={good.LiquidType}, special={special.LiquidType}.");
  }

  Console.WriteLine(
    "PASS: TileRunner initialization preserves direction and liquid-type random branches");
}

static void LegacyTileRunnerPerturbationSourceCheck()
{
  LegacyTileRunnerPerturbation jitter =
    LegacyTileRunnerPerturbationPolicy.ApplyDrunkJitter(10.0, 20.0, true, 0, -20, 40);
  LegacyTileRunnerPerturbation skipped =
    LegacyTileRunnerPerturbationPolicy.ApplyDrunkJitter(10.0, 20.0, true, 1, -20, 40);
  LegacyTileRunnerPerturbation normal =
    LegacyTileRunnerPerturbationPolicy.ApplyDrunkJitter(10.0, 20.0, false, 0, -20, 40);
  if (jitter.CenterX != 9.0 || jitter.CenterY != 22.0 || !jitter.AppliedDrunkJitter ||
      skipped.CenterX != 10.0 || skipped.CenterY != 20.0 || skipped.AppliedDrunkJitter ||
      normal.CenterX != 10.0 || normal.CenterY != 20.0 || normal.AppliedDrunkJitter)
  {
    throw new InvalidOperationException(
      "TileRunner drunk center perturbation diverged from per-step random gate.");
  }

  Console.WriteLine(
    "PASS: TileRunner drunk center perturbation preserves per-step random gate");
}

static void LegacyTileRunnerDriftSourceCheck()
{
  LegacyTileRunnerDrift applied = LegacyTileRunnerDriftPolicy.ApplyExtraDrift(
    10.0, 20.0, 1.0, -2.0, 60.0, 10.0, false, 0, 4, -6);
  LegacyTileRunnerDrift skippedByStrength = LegacyTileRunnerDriftPolicy.ApplyExtraDrift(
    10.0, 20.0, 1.0, -2.0, 50.0, 10.0, false, 0, 4, -6);
  LegacyTileRunnerDrift skippedByDrunkGate = LegacyTileRunnerDriftPolicy.ApplyExtraDrift(
    10.0, 20.0, 1.0, -2.0, 60.0, 10.0, true, 0, 4, -6);
  if (applied.CenterX != 11.0 || applied.CenterY != 18.0 ||
      applied.DirectionX != 1.2 || applied.DirectionY != -2.3 ||
      applied.RemainingSteps != 9.0 || !applied.AppliedExtraDrift ||
      skippedByStrength.AppliedExtraDrift || skippedByDrunkGate.AppliedExtraDrift)
  {
    throw new InvalidOperationException(
      "TileRunner strength-driven drift diverged from threshold and drunk gate semantics.");
  }

  Console.WriteLine(
    "PASS: TileRunner strength-driven drift preserves threshold and random gate");
}

static void LegacyTileRunnerDriftBatchSourceCheck()
{
  IReadOnlyList<int> thresholds = LegacyTileRunnerDriftBatchPolicy.RegisterDefaults();
  if (thresholds.Count != 12 || thresholds[0] != 50 || thresholds[^1] != 900 ||
      !thresholds.SequenceEqual(LegacyTileRunnerDriftBatchPolicy.RegisterDefaults()))
  {
    throw new InvalidOperationException("TileRunner drift thresholds were not stable or ordered.");
  }

  try
  {
    ((IList<int>)thresholds)[0] = thresholds[0];
    throw new InvalidOperationException("TileRunner drift threshold projection was mutable.");
  }
  catch (NotSupportedException)
  {
  }

  int[] rolls = new int[12];
  LegacyTileRunnerDriftBatch batch = LegacyTileRunnerDriftBatchPolicy.Apply(
    0.0,
    0.0,
    1.0,
    0.0,
    260.0,
    20.0,
    false,
    0,
    rolls,
    rolls);
  if (batch.AppliedDriftCount != 5 || batch.CenterX != 5.0 || batch.CenterY != 0.0 ||
      batch.DirectionX != 1.0 || batch.RemainingSteps != 15.0)
  {
    throw new InvalidOperationException(
      "TileRunner repeated strength-band drift diverged from source thresholds.");
  }

  Console.WriteLine(
    "PASS: TileRunner repeated strength-band drift preserves source thresholds");
}

static void LegacyTileRunnerDirectionSourceCheck()
{
  LegacyTileRunnerDirection clamped = LegacyTileRunnerDirectionPolicy.Finalize(
    0.9, 0.4, 1, true, false, 10.0, 500.0, 400.0, 1200, 10, 10, 10);
  LegacyTileRunnerDirection type59 = LegacyTileRunnerDirectionPolicy.Finalize(
    0.0, 0.9, 59, false, false, 10.0, 1000.0, 400.0, 1200, 0, 0, 10);
  LegacyTileRunnerDirection shallowType59 = LegacyTileRunnerDirectionPolicy.Finalize(
    0.0, -0.9, 59, false, false, 10.0, 450.0, 400.0, 1200, 0, 0, 0);
  LegacyTileRunnerDirection noYChange = LegacyTileRunnerDirectionPolicy.Finalize(
    0.0, 2.0, 1, false, true, 2.0, 500.0, 400.0, 1200, 0, 0, 10);
  if (clamped.X != 1.0 || Math.Abs(clamped.Y - 0.9) > 0.0001 ||
      type59.Y != -1.0 || !type59.AppliedType59Clamp ||
      shallowType59.Y != 1.0 || noYChange.Y != 1.0)
  {
    throw new InvalidOperationException(
      $"TileRunner final direction diverged: clamp=({clamped.X},{clamped.Y}), " +
      $"type59={type59.Y}, shallow={shallowType59.Y}, noY={noYChange.Y}.");
  }

  Console.WriteLine(
    "PASS: TileRunner final direction clamp preserves noYChange and type-59 branches");
}

static void LegacyCavePassPipelineSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyCavePassPipeline",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 17,
    seedVariant: "default");
  WorldGenerationRequest request = new(metadata, spawnX: 100, surfaceY: 40, rockLayerY: 80);
  WorldGrid firstWorld = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  WorldGenerationStateComponent firstState = new(request.GenerationId);
  List<TileChangeCommand> terrainCommands = new();
  TerrainProfileComponent terrainProfile = new(request.SurfaceY, request.RockLayerY, metadata.Height - 1);
  new TerrainBaseSystem().AppendCommands(
    firstWorld.CreateSnapshot(metadata), request, terrainProfile, ref firstState, terrainCommands);
  if (!new TileChangeCommitSystem().TryCommit(firstWorld, terrainCommands, out _))
  {
    throw new InvalidOperationException("Legacy cave pass fixture terrain could not be committed.");
  }
  List<TileChangeCommand> firstCommands = new();
  new LegacyCavePassSystem().AppendCommands(
    firstWorld.CreateSnapshot(metadata), request, ref firstState, firstCommands);
  const int ExpectedInvocationCount = 297;
  if (firstCommands.Count <= ExpectedInvocationCount || firstCommands.Any(command =>
      !command.Source.StartsWith("worldgen.cave.", StringComparison.Ordinal)) ||
      !firstCommands.Any(command => command.Source.StartsWith(
        "worldgen.cave.DirtLayerCaves.", StringComparison.Ordinal)) ||
      !firstCommands.Any(command => command.Source.StartsWith(
        "worldgen.cave.RockLayerCaves.", StringComparison.Ordinal)) ||
      !firstCommands.Any(command => command.Source.StartsWith(
        "worldgen.cave.SurfaceCaves.", StringComparison.Ordinal)))
  {
    throw new InvalidOperationException("Legacy cave pass pipeline did not emit attributed commands.");
  }

  WorldGrid secondWorld = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  WorldGenerationStateComponent secondState = new(request.GenerationId);
  List<TileChangeCommand> secondTerrainCommands = new();
  new TerrainBaseSystem().AppendCommands(
    secondWorld.CreateSnapshot(metadata), request, terrainProfile, ref secondState, secondTerrainCommands);
  if (!new TileChangeCommitSystem().TryCommit(secondWorld, secondTerrainCommands, out _))
  {
    throw new InvalidOperationException("Legacy cave pass replay terrain could not be committed.");
  }
  List<TileChangeCommand> secondCommands = new();
  new LegacyCavePassSystem().AppendCommands(
    secondWorld.CreateSnapshot(metadata), request, ref secondState, secondCommands);
  if (!firstCommands.SequenceEqual(secondCommands) || firstState.NextSequence != secondState.NextSequence)
  {
    throw new InvalidOperationException("Legacy cave pass pipeline was not deterministic.");
  }

  Console.WriteLine("PASS: legacy Dirt/Rock/Surface cave pass pipeline is deterministic and attributed");
}

static void LegacyWavyCavererSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyWavyCaverer",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 18,
    seedVariant: "default");
  LegacyWavyCavererInvocation carve = new(30, 50, 14.0, 0.5, 60, -1);
  LegacyWavyCavererInvocation place = carve with { TileType = 1 };
  WorldGenerationStateComponent firstState = new(18);
  WorldGenerationStateComponent secondState = new(18);
  List<TileChangeCommand> firstCommands = new();
  List<TileChangeCommand> secondCommands = new();
  WorldGridSnapshot snapshot = new WorldGrid(
    metadata.Width,
    metadata.Height,
    initializeLegacyEmptyFrames: true).CreateSnapshot(metadata);

  LegacyWavyCaverer.AppendCommands(
    snapshot,
    carve,
    new LegacyPassRandomState(1456),
    ref firstState,
    firstCommands);
  LegacyWavyCaverer.AppendCommands(
    snapshot,
    carve,
    new LegacyPassRandomState(1456),
    ref secondState,
    secondCommands);
  if (firstCommands.Count == 0 || !firstCommands.SequenceEqual(secondCommands) ||
      firstCommands.Any(command =>
        command.Kind != TileChangeKind.Kill || command.X < 20 ||
        command.X >= metadata.Width - 20 || command.Y < 20 ||
        command.Y >= metadata.Height - 20) ||
      firstCommands.Any(command => command.Source != "worldgen.cave.WavyCaverer"))
  {
    throw new InvalidOperationException(
      "WavyCaverer did not preserve deterministic kill commands and the source fluff boundary.");
  }

  WorldGenerationStateComponent placementState = new(18);
  List<TileChangeCommand> placementCommands = new();
  LegacyWavyCaverer.AppendCommands(
    snapshot,
    place,
    new LegacyPassRandomState(1456),
    ref placementState,
    placementCommands);
  if (placementCommands.Count == 0 || placementCommands.Any(command =>
        command.Kind != TileChangeKind.Place || command.TileType != 1))
  {
    throw new InvalidOperationException("WavyCaverer did not preserve positive-type placement.");
  }

  Console.WriteLine("PASS: WavyCaverer preserves deterministic typed commands and fluff bounds");
}

static void LegacyCaveTunnelSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyCaveTunnel",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 19,
    seedVariant: "default");
  WorldGridSnapshot snapshot = new WorldGrid(
    metadata.Width,
    metadata.Height,
    initializeLegacyEmptyFrames: true).CreateSnapshot(metadata);
  LegacyCaveTunnelInvocation dry = new(50.0, 70.0, 0.6, 0.8, 12, 4, false);
  WorldGenerationStateComponent firstState = new(19);
  WorldGenerationStateComponent secondState = new(19);
  List<TileChangeCommand> firstTiles = new();
  List<TileChangeCommand> secondTiles = new();
  List<LiquidChangeCommand> firstLiquids = new();
  List<LiquidChangeCommand> secondLiquids = new();

  LegacyCaveTunnelResult first = LegacyCaveTunnel.AppendCommands(
    snapshot,
    dry,
    new LegacyPassRandomState(1456),
    ref firstState,
    firstTiles,
    firstLiquids);
  LegacyCaveTunnelResult second = LegacyCaveTunnel.AppendCommands(
    snapshot,
    dry,
    new LegacyPassRandomState(1456),
    ref secondState,
    secondTiles,
    secondLiquids);
  if (first != second || firstTiles.Count == 0 || firstLiquids.Count != 0 ||
      !firstTiles.SequenceEqual(secondTiles) || !secondLiquids.SequenceEqual(firstLiquids) ||
      firstTiles.Any(command => command.Kind != TileChangeKind.Kill ||
        command.Source != "worldgen.cave.digTunnel" ||
        command.X < 0 || command.X >= metadata.Width ||
        command.Y < 0 || command.Y >= metadata.Height))
  {
    throw new InvalidOperationException(
      "digTunnel did not preserve deterministic, bounded dry carve commands.");
  }

  WorldGenerationStateComponent wetState = new(19);
  List<TileChangeCommand> wetTiles = new();
  List<LiquidChangeCommand> wetLiquids = new();
  LegacyCaveTunnel.AppendCommands(
    snapshot,
    dry with { IsWet = true },
    new LegacyPassRandomState(1456),
    ref wetState,
    wetTiles,
    wetLiquids);
  if (wetTiles.Count == 0 || wetLiquids.Count != wetTiles.Count ||
      wetLiquids.Any(command => command.Amount != byte.MaxValue || command.Type != 0 ||
        command.Source != "worldgen.cave.digTunnel") ||
      wetTiles.Select(command => (command.X, command.Y)).SequenceEqual(
        wetLiquids.Select(command => (command.X, command.Y))) == false)
  {
    throw new InvalidOperationException(
      "digTunnel did not pair each wet carve with a typed water command.");
  }

  Console.WriteLine("PASS: digTunnel preserves deterministic dry and wet typed command output");
}

static void LegacyCavererSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyCaverer",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 20,
    seedVariant: "default");
  WorldGridSnapshot snapshot = new WorldGrid(
    metadata.Width,
    metadata.Height,
    initializeLegacyEmptyFrames: true).CreateSnapshot(metadata);
  WorldGenerationStateComponent firstState = new(20);
  WorldGenerationStateComponent secondState = new(20);
  List<TileChangeCommand> firstTiles = new();
  List<TileChangeCommand> secondTiles = new();
  List<LiquidChangeCommand> firstLiquids = new();
  List<LiquidChangeCommand> secondLiquids = new();

  LegacyCaverer.AppendCommands(
    snapshot,
    startX: 80,
    startY: 100,
    new LegacyPassRandomState(1456),
    worldSurfaceY: 40,
    rockLayerY: 80,
    ref firstState,
    firstTiles,
    firstLiquids);
  LegacyCaverer.AppendCommands(
    snapshot,
    startX: 80,
    startY: 100,
    new LegacyPassRandomState(1456),
    worldSurfaceY: 40,
    rockLayerY: 80,
    ref secondState,
    secondTiles,
    secondLiquids);
  if (firstTiles.Count == 0 || !firstTiles.SequenceEqual(secondTiles) ||
      !firstLiquids.SequenceEqual(secondLiquids) ||
      firstTiles.Any(command => !command.Source.StartsWith(
        "worldgen.cave.", StringComparison.Ordinal)) ||
      !firstTiles.Any(command => command.Source == "worldgen.cave.digTunnel") ||
      firstLiquids.Any(command => command.Amount != byte.MaxValue || command.Type != 0 ||
        command.Source != "worldgen.cave.digTunnel"))
  {
    throw new InvalidOperationException(
      "Caverer did not preserve deterministic tunnel and TileRunner command output.");
  }

  Console.WriteLine("PASS: Caverer preserves deterministic typed tunnel output");
}

static void LegacySurfaceCavesCavererPassSourceCheck()
{
  if (LegacySurfaceCavesCavererPass.SurfaceCavesBeachAvoidance != 340 ||
      LegacySurfaceCavesCavererPass.CalculateInvocationCount(4199) != 4 ||
      LegacySurfaceCavesCavererPass.CalculateInvocationCount(4200) != 5)
  {
    throw new InvalidOperationException(
      "SurfaceCaves Caverer density or beach avoidance diverged from source.");
  }

  WorldMetadata metadata = new(
    "LegacySurfaceCavesCavererPass",
    new WorldSeed(1456),
    width: 200,
    height: 600,
    worldId: 21,
    seedVariant: "default");
  WorldGridSnapshot snapshot = new WorldGrid(
    metadata.Width,
    metadata.Height,
    initializeLegacyEmptyFrames: true).CreateSnapshot(metadata);
  WorldGenerationStateComponent state = new(21);
  List<TileChangeCommand> tileCommands = new();
  List<LiquidChangeCommand> liquidCommands = new();
  LegacySurfaceCavesCavererPass.AppendCommands(
    snapshot,
    new LegacyTerrainRuntimeProfile(
      WorldSurface: 150,
      RockLayer: 250,
      WorldSurfaceLow: 140,
      WorldSurfaceHigh: 160,
      RockLayerLow: 240,
      RockLayerHigh: 260,
      LeftBeachEnd: 20,
      RightBeachStart: 180,
      WaterLine: 300,
      LavaLine: 500),
    new LegacyPassRandomState(1456),
    worldSurfaceY: 150,
    rockLayerY: 250,
    ref state,
    tileCommands,
    liquidCommands);
  if (tileCommands.Count != 0 || liquidCommands.Count != 0)
  {
    throw new InvalidOperationException(
      "SurfaceCaves Caverer pass did not skip an invalid legacy random range.");
  }

  Console.WriteLine("PASS: SurfaceCaves Caverer pass preserves density and invalid-range skip");
}

static void LegacyMountinaterSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyMountinater",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 22,
    seedVariant: "default");
  WorldGridSnapshot snapshot = new WorldGrid(
    metadata.Width,
    metadata.Height,
    initializeLegacyEmptyFrames: true).CreateSnapshot(metadata);
  WorldGenerationStateComponent firstState = new(22);
  WorldGenerationStateComponent secondState = new(22);
  List<TileChangeCommand> firstCommands = new();
  List<TileChangeCommand> secondCommands = new();

  LegacyMountinater.AppendCommands(
    snapshot,
    startX: 100,
    startY: 70,
    new LegacyPassRandomState(1456),
    ref firstState,
    firstCommands);
  LegacyMountinater.AppendCommands(
    snapshot,
    startX: 100,
    startY: 70,
    new LegacyPassRandomState(1456),
    ref secondState,
    secondCommands);
  if (firstCommands.Count == 0 || !firstCommands.SequenceEqual(secondCommands) ||
      firstCommands.Any(command => command.Kind != TileChangeKind.Place ||
        command.TileType != 0 || command.Source != "worldgen.cave.Mountinater" ||
        command.X < 0 || command.X >= metadata.Width ||
        command.Y < 0 || command.Y >= metadata.Height))
  {
    throw new InvalidOperationException(
      "Mountinater did not preserve deterministic, bounded empty-tile placement commands.");
  }

  Console.WriteLine("PASS: Mountinater preserves deterministic bounded placement output");
}

static void LegacyMountainCavesPassSourceCheck()
{
  if (LegacyMountainCavesPass.CalculateInvocationCount(4999) != 4 ||
      LegacyMountainCavesPass.CalculateInvocationCount(5000) != 5 ||
      LegacyMountainCavesPass.IsCandidateXAccepted(2100, 4200, Array.Empty<int>()) ||
      LegacyMountainCavesPass.IsCandidateXAccepted(1200, 4200, new[] { 1299 }) ||
      !LegacyMountainCavesPass.IsCandidateXAccepted(1200, 4200, new[] { 1300 }))
  {
    throw new InvalidOperationException(
      "MountainCaves density, spawn-center exclusion, or history spacing diverged from source.");
  }

  Console.WriteLine("PASS: MountainCaves preserves source density and candidate X vetoes");
}

static void LegacySurfaceCavesVerticalPassSourceCheck()
{
  IReadOnlyList<LegacySurfaceCavesVerticalRecipe> recipes =
    LegacySurfaceCavesVerticalPass.CreateDefaultRecipes();
  if (recipes.Count != 4 ||
      LegacySurfaceCavesVerticalPass.CalculateInvocationCount(
        LegacySurfaceCavesVerticalFamily.Narrow, 4200) != 8 ||
      LegacySurfaceCavesVerticalPass.CalculateInvocationCount(
        LegacySurfaceCavesVerticalFamily.Medium, 4200) != 2 ||
      LegacySurfaceCavesVerticalPass.CalculateInvocationCount(
        LegacySurfaceCavesVerticalFamily.Deep, 4200) != 1 ||
      LegacySurfaceCavesVerticalPass.CalculateInvocationCount(
        LegacySurfaceCavesVerticalFamily.Horizontal, 4200) != 1 ||
      recipes.Any(recipe => recipe.TileType != -1 || recipe.AddTile))
  {
    throw new InvalidOperationException(
      "SurfaceCaves vertical recipe densities or TileRunner mutation mode diverged from source.");
  }

  LegacySurfaceCavesVerticalRecipe horizontalRecipe = recipes.Single(recipe =>
    recipe.Family == LegacySurfaceCavesVerticalFamily.Horizontal);
  LegacyPassRandomState horizontalRandom = new(1456);
  double horizontalSpeedX = LegacySurfaceCavesVerticalPass.DrawSpeedX(
    horizontalRecipe,
    horizontalRandom);
  if (horizontalSpeedX != 0.0 || horizontalRandom.SampleCount != 0)
  {
    throw new InvalidOperationException(
      "SurfaceCaves horizontal runner consumed a random speed-X sample.");
  }

  Console.WriteLine("PASS: SurfaceCaves vertical recipes preserve source densities and carve mode");
}

static void LegacySurfaceCavesVerticalTraversalSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacySurfaceCavesVertical",
    new WorldSeed(1456),
    width: 1000,
    height: 150,
    worldId: 23,
    seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  for (int x = 0; x < metadata.Width; x++)
  {
    _ = world.TrySetTile(x, 40, new WorldTile(IsActive: true, Type: 1));
  }

  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 50,
    RockLayer: 90,
    WorldSurfaceLow: 40,
    WorldSurfaceHigh: 60,
    RockLayerLow: 80,
    RockLayerHigh: 100,
    LeftBeachEnd: 10,
    RightBeachStart: 990,
    WaterLine: 100,
    LavaLine: 125);
  WorldGenerationStateComponent firstState = new(23);
  WorldGenerationStateComponent secondState = new(23);
  List<TileChangeCommand> firstCommands = new();
  List<TileChangeCommand> secondCommands = new();
  LegacySurfaceCavesVerticalPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(1456),
    ref firstState,
    firstCommands);
  LegacySurfaceCavesVerticalPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(1456),
    ref secondState,
    secondCommands);
  if (firstCommands.Count == 0 || !firstCommands.SequenceEqual(secondCommands) ||
      firstCommands.Any(command => command.Kind != TileChangeKind.Kill ||
        !command.Source.StartsWith("worldgen.cave.SurfaceCaves.vertical", StringComparison.Ordinal)))
  {
    throw new InvalidOperationException(
      "SurfaceCaves vertical traversal did not preserve deterministic typed carve commands.");
  }

  Console.WriteLine("PASS: SurfaceCaves vertical traversal preserves deterministic typed carve output");
}

static void LegacyEvilReplacementQuerySourceCheck()
{
  LegacyEvilReplacementDefinitions defaults = LegacyEvilReplacementDefinitions.CreateDefault();
  if (!defaults.DungeonTileTypes.SetEquals(new ushort[] { 41, 43, 44, 677, 678, 679 }) ||
      !defaults.CrackedBrickTileTypes.SetEquals(new ushort[] { 481, 482, 483 }) ||
      !defaults.DungeonWallTypes.SetEquals(new ushort[] { 7, 8, 9, 94, 95, 96, 97, 98, 99 }))
  {
    throw new InvalidOperationException(
      "CanEvilReplace definitions did not preserve legacy dungeon and cracked-brick sets.");
  }

  LegacyEvilReplacementDefinitions definitions = new(
    new HashSet<ushort> { 41 },
    new HashSet<ushort> { 43 },
    new HashSet<ushort> { 44 });
  if (LegacyEvilReplacementQuery.CanReplace(new WorldTile(IsActive: true, Type: 41), definitions) ||
      LegacyEvilReplacementQuery.CanReplace(new WorldTile(IsActive: true, Type: 43), definitions) ||
      !LegacyEvilReplacementQuery.CanReplace(new WorldTile(IsActive: false, Type: 41), definitions) ||
      LegacyEvilReplacementQuery.CanReplace(new WorldTile(IsActive: true, Type: 1, WallType: 44), definitions) ||
      !LegacyEvilReplacementQuery.CanReplace(new WorldTile(IsActive: true, Type: 1, WallType: 2), definitions))
  {
    throw new InvalidOperationException(
      "CanEvilReplace did not preserve active tile and wall exclusion semantics.");
  }

  Console.WriteLine("PASS: CanEvilReplace preserves frozen dungeon and cracked-brick exclusions");
}

static void LegacyChasmRunnerSidewaysSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyChasmRunnerSideways",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 24,
    seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  for (int x = 0; x < metadata.Width; x++)
  {
    for (int y = 60; y < metadata.Height; y++)
    {
      _ = world.TrySetTile(x, y, new WorldTile(IsActive: true, Type: 1, WallType: 2));
    }
  }

  _ = world.TrySetTile(110, 60, new WorldTile(IsActive: true, Type: 41, WallType: 2));
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  WorldGenerationStateComponent firstState = new(24);
  WorldGenerationStateComponent secondState = new(24);
  List<TileChangeCommand> firstCommands = new();
  List<TileChangeCommand> secondCommands = new();
  LegacyChasmRunnerSideways.AppendCommands(
    snapshot,
    new LegacyChasmRunnerSidewaysInvocation(100, 60, 1, 20, 0, 0, 1, false),
    LegacyEvilReplacementDefinitions.CreateDefault(),
    new LegacyPassRandomState(1456),
    ref firstState,
    firstCommands);
  LegacyChasmRunnerSideways.AppendCommands(
    snapshot,
    new LegacyChasmRunnerSidewaysInvocation(100, 60, 1, 20, 0, 0, 1, false),
    LegacyEvilReplacementDefinitions.CreateDefault(),
    new LegacyPassRandomState(1456),
    ref secondState,
    secondCommands);
  bool commandsMatch = firstCommands.SequenceEqual(secondCommands);
  bool sourcesMatch = firstCommands.All(
    command => command.Source == "worldgen.cave.ChasmRunnerSideways");
  bool protectsDungeon = !firstCommands.Any(command => command.X == 110 && command.Y == 60);
  bool writesEvilTile = firstCommands.Any(command =>
    command.Kind == TileChangeKind.UpdateTileType && command.TileType == 0);
  bool writesEvilWall = firstCommands.Any(command =>
    command.Kind == TileChangeKind.SetWall && command.WallType == 1);
  if (firstCommands.Count == 0 || !commandsMatch || !sourcesMatch || !protectsDungeon ||
      !writesEvilTile || !writesEvilWall)
  {
    throw new InvalidOperationException(
      $"ChasmRunnerSideways diverged: count={firstCommands.Count}, " +
      $"same={commandsMatch}, source={sourcesMatch}, protected={protectsDungeon}, " +
      $"tile={writesEvilTile}, wall={writesEvilWall}.");
  }

  Console.WriteLine("PASS: ChasmRunnerSideways preserves deterministic protected command output");
}

static void LegacyShadowOrbPlacementSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyShadowOrb",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 25,
    seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  WorldGenerationStateComponent state = new(25);
  List<StructurePlacementCommand> commands = new();
  if (!LegacyShadowOrbPlacement.TryAppendIntent(
        snapshot,
        40,
        30,
        crimsonHeart: true,
        ref state,
        commands) ||
      commands.Count != 1 ||
      commands[0] != new StructurePlacementCommand(
        0,
        "worldgen.structure.ShadowOrb.CrimsonHeart",
        39,
        29))
  {
    throw new InvalidOperationException(
      "AddShadowOrb did not emit the source-backed Crimson Heart placement intent.");
  }

  _ = world.TrySetTile(39, 29, new WorldTile(IsActive: true, Type: 31));
  WorldGenerationStateComponent rejectedState = new(25);
  List<StructurePlacementCommand> rejectedCommands = new();
  if (LegacyShadowOrbPlacement.TryAppendIntent(
        world.CreateSnapshot(metadata),
        40,
        30,
        crimsonHeart: false,
        ref rejectedState,
        rejectedCommands) ||
      rejectedCommands.Count != 0)
  {
    throw new InvalidOperationException(
      "AddShadowOrb did not reject an existing active shadow orb footprint.");
  }

  Console.WriteLine("PASS: AddShadowOrb emits typed 2x2 structure intent with source guards");
}

static void LegacyChasmRunnerSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyChasmRunner",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 26,
    seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  for (int x = 0; x < metadata.Width; x++)
  {
    for (int y = 40; y < metadata.Height; y++)
    {
      _ = world.TrySetTile(x, y, new WorldTile(IsActive: true, Type: 1, WallType: 2));
    }
  }

  for (int x = 60; x <= 140; x++)
  {
    for (int y = 5; y <= 100; y++)
    {
      WorldTile tile = y % 3 == 0
        ? new WorldTile(IsActive: true, Type: 1, WallType: 2)
        : default;
      _ = world.TrySetTile(x, y, tile);
    }
  }

  WorldGenerationStateComponent state = new(26);
  List<TileChangeCommand> commands = new();
  List<StructurePlacementCommand> structures = new();
  LegacyChasmRunner.AppendCommands(
    world.CreateSnapshot(metadata),
    new LegacyChasmRunnerInvocation(
      100,
      80,
      1,
      MakeOrb: true,
      WorldSurface: 10,
      RockLayer: 20,
      EvilTileType: 25,
      EvilWallType: 3),
    LegacyEvilReplacementDefinitions.CreateDefault(),
    TileDefinitionRegistry.CreateVersion4Base(),
    new LegacyPassRandomState(1456),
    ref state,
    commands,
    structures);
  if (commands.Count == 0 || structures.Count != 1 ||
      structures[0].DefinitionId != "worldgen.structure.ShadowOrb" ||
      !commands.Any(command => command.TileType == 26 &&
        command.Source == "worldgen.structure.Place3x2") ||
      !commands.Any(command => command.Kind == TileChangeKind.UpdateTileType &&
        command.TileType == 25 && command.IsActive == true) ||
      !commands.Any(command => command.Kind == TileChangeKind.SetWall && command.WallType == 3) ||
      !commands.All(command => command.Source.StartsWith("worldgen.cave.ChasmRunner", StringComparison.Ordinal) ||
        command.Source == "worldgen.structure.Place3x2"))
  {
    throw new InvalidOperationException(
      "ChasmRunner did not preserve carve, conversion, orb, and 3x2 pot output.");
  }

  Console.WriteLine("PASS: ChasmRunner emits bounded carve, conversion, orb, and 3x2 pot output");
}

static void LegacyPlace3x2SourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyPlace3x2",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 27,
    seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  for (int x = 59; x <= 61; x++)
  {
    _ = world.TrySetTile(x, 71, new WorldTile(IsActive: true, Type: 1));
  }

  WorldGenerationStateComponent state = new(27);
  List<TileChangeCommand> commands = new();
  if (!LegacyPlace3x2.TryAppendCommands(
        world.CreateSnapshot(metadata),
        TileDefinitionRegistry.CreateVersion4Base(),
        60,
        70,
        26,
        2,
        ref state,
        commands) ||
      commands.Count != 6 ||
      commands[0] != new TileChangeCommand(
        0, 59, 69, TileChangeKind.Place, 26, FrameX: 108, FrameY: 0,
        Source: "worldgen.structure.Place3x2") ||
      commands[5] != new TileChangeCommand(
        5, 61, 70, TileChangeKind.Place, 26, FrameX: 144, FrameY: 18,
        Source: "worldgen.structure.Place3x2"))
  {
    throw new InvalidOperationException(
      "Place3x2 did not emit the source-backed 3x2 style frame commands.");
  }

  _ = world.TrySetTile(60, 69, new WorldTile(IsActive: true, Type: 1));
  WorldGenerationStateComponent rejectedState = new(27);
  List<TileChangeCommand> rejectedCommands = new();
  if (LegacyPlace3x2.TryAppendCommands(
        world.CreateSnapshot(metadata),
        TileDefinitionRegistry.CreateVersion4Base(),
        60,
        70,
        26,
        0,
        ref rejectedState,
        rejectedCommands) ||
      rejectedCommands.Count != 0)
  {
    throw new InvalidOperationException("Place3x2 did not reject an active footprint tile.");
  }

  Console.WriteLine("PASS: Place3x2 emits framed 3x2 commands and rejects invalid footprint");
}

static void LegacyNoSurfaceTopFillSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyNoSurfaceTopFill",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 28,
    seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(10, 10, new WorldTile(IsActive: false, Type: 1));
  _ = world.TrySetTile(11, 10, new WorldTile(IsActive: false, Type: 60));
  _ = world.TrySetTile(12, 10, new WorldTile(IsActive: false, Type: 1, WallType: 7));
  _ = world.TrySetTile(13, 10, new WorldTile(IsActive: false, Type: 41));
  WorldGenerationStateComponent state = new(28);
  List<TileChangeCommand> commands = new();
  LegacyNoSurfaceTopFill.AppendCommands(
    world.CreateSnapshot(metadata),
    skyblockWorld: false,
    LegacyEvilReplacementDefinitions.CreateDefault(),
    new LegacyPassRandomState(1456),
    ref state,
    commands);
  TileChangeCommand? ordinary = commands.SingleOrDefault(command => command.X == 10 && command.Y == 10);
  TileChangeCommand? converted = commands.SingleOrDefault(command => command.X == 11 && command.Y == 10);
  if (ordinary is not { TileType: 1, IsActive: true } ||
      converted is not { TileType: 59, IsActive: true } ||
      commands.Any(command => command.X is 12 or 13 && command.Y == 10))
  {
    throw new InvalidOperationException(
      "DoNoSurfaceFillTheTop did not preserve dungeon and type conversion guards.");
  }

  WorldGenerationStateComponent skyblockState = new(28);
  List<TileChangeCommand> skyblockCommands = new();
  LegacyNoSurfaceTopFill.AppendCommands(
    world.CreateSnapshot(metadata),
    skyblockWorld: true,
    LegacyEvilReplacementDefinitions.CreateDefault(),
    new LegacyPassRandomState(1456),
    ref skyblockState,
    skyblockCommands);
  if (skyblockCommands.Count != 0)
  {
    throw new InvalidOperationException("DoNoSurfaceFillTheTop did not skip skyblock worlds.");
  }

  Console.WriteLine("PASS: DoNoSurfaceFillTheTop preserves bounded dungeon and conversion guards");
}

static void LegacySurfaceIsInSpaceSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacySurfaceIsInSpace",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 29,
    seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(10, 10, new WorldTile(IsActive: false, Type: 1, WallType: 73));
  _ = world.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 53, WallType: 73));
  _ = world.TrySetTile(12, 10, new WorldTile(IsActive: true, Type: 1, WallType: 73));
  _ = world.TrySetTile(13, 30, new WorldTile(IsActive: false, Type: 1, WallType: 73));
  WorldGenerationStateComponent state = new(29);
  List<TileChangeCommand> commands = new();
  LegacySurfaceIsInSpace.AppendCommands(
    world.CreateSnapshot(metadata),
    worldSurfaceY: 30,
    skyblockWorld: false,
    ref state,
    commands);
  if (commands.Count != 2 ||
      !commands.Any(command => command.X == 10 && command.Y == 10 && command.WallType == 0) ||
      !commands.Any(command => command.X == 11 && command.Y == 10 && command.WallType == 0) ||
      commands.Any(command => command.X is 12 or 13))
  {
    throw new InvalidOperationException(
      "DoSurfaceIsInSpace did not preserve wall, tile, or world-surface guards.");
  }

  WorldGenerationStateComponent skyblockState = new(29);
  List<TileChangeCommand> skyblockCommands = new();
  LegacySurfaceIsInSpace.AppendCommands(
    world.CreateSnapshot(metadata),
    worldSurfaceY: 30,
    skyblockWorld: true,
    ref skyblockState,
    skyblockCommands);
  if (skyblockCommands.Count != 0)
  {
    throw new InvalidOperationException("DoSurfaceIsInSpace did not skip skyblock worlds.");
  }

  Console.WriteLine("PASS: DoSurfaceIsInSpace preserves bounded wall-clear guards");
}

static void LegacySurfaceIsMushroomsSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacySurfaceIsMushrooms",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 30,
    seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 60, WallType: 15));
  _ = world.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 0, WallType: 63));
  _ = world.TrySetTile(12, 10, new WorldTile(IsActive: true, Type: 2, WallType: 205));
  _ = world.TrySetTile(13, 10, new WorldTile(IsActive: true, Type: 1, WallType: 64));
  _ = world.TrySetTile(14, 10, new WorldTile(IsActive: true, Type: 1, WallType: 1));
  _ = world.TrySetTile(15, 35, new WorldTile(IsActive: true, Type: 60, WallType: 15));
  WorldGenerationStateComponent state = new(30);
  List<TileChangeCommand> commands = new();
  LegacySurfaceIsMushrooms.AppendNormalModeCommands(
    world.CreateSnapshot(metadata),
    worldSurfaceY: 30,
    skyblockWorld: false,
    new LegacyPassRandomState(1456),
    ref state,
    commands);
  if (!commands.Any(command => command.X == 10 && command.Y == 10 &&
                               command.TileType == 70 &&
                               command.Kind == TileChangeKind.UpdateTileType) ||
      !commands.Any(command => command.X == 10 && command.Y == 10 &&
                               command.WallType == 80 &&
                               command.Kind == TileChangeKind.SetWall) ||
      !commands.Any(command => command.X == 11 && command.Y == 10 &&
                               command.TileType == 59) ||
      !commands.Any(command => command.X == 11 && command.Y == 10 &&
                               command.WallType == 80) ||
      !commands.Any(command => command.X == 12 && command.Y == 10 &&
                               command.TileType == 60) ||
      !commands.Any(command => command.X == 12 && command.Y == 10 &&
                               command.WallType == 80) ||
      !commands.Any(command => command.X == 13 && command.Y == 10 &&
                               command.WallType == 80) ||
      commands.Any(command => command.X == 14 && command.Y == 10) ||
      commands.Any(command => command.X == 15 && command.Y == 35))
  {
    throw new InvalidOperationException(
      "DoSurfaceIsMushrooms did not preserve normal-mode conversion and bounds guards.");
  }

  WorldGenerationStateComponent skyblockState = new(30);
  List<TileChangeCommand> skyblockCommands = new();
  LegacySurfaceIsMushrooms.AppendNormalModeCommands(
    world.CreateSnapshot(metadata),
    worldSurfaceY: 30,
    skyblockWorld: true,
    new LegacyPassRandomState(1456),
    ref skyblockState,
    skyblockCommands);
  if (skyblockCommands.Count != 0)
  {
    throw new InvalidOperationException("DoSurfaceIsMushrooms did not skip skyblock worlds.");
  }

  Console.WriteLine("PASS: DoSurfaceIsMushrooms preserves bounded normal-mode conversions");
}

static void LegacyWorldIsFrozenSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyWorldIsFrozen",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 31,
    seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(10, 10, new WorldTile(IsActive: true, Type: 1, WallType: 2));
  _ = world.TrySetTile(11, 10, new WorldTile(IsActive: true, Type: 25));
  _ = world.TrySetTile(12, 10, new WorldTile(IsActive: true, Type: 203));
  _ = world.TrySetTile(13, 10, new WorldTile(IsActive: true, Type: 117));
  _ = world.TrySetTile(14, 10, new WorldTile(IsActive: true, Type: 0));
  _ = world.TrySetTile(15, 10, new WorldTile(IsActive: true, Type: 2));
  _ = world.TrySetTile(16, 10, new WorldTile(IsActive: true, Type: 23));
  _ = world.TrySetTile(17, 10, new WorldTile(IsActive: true, Type: 199));
  _ = world.TrySetTile(18, 10, new WorldTile(IsActive: true, Type: 109));
  _ = world.TrySetTile(19, 10, new WorldTile(IsActive: true, Type: 123));
  _ = world.TrySetTile(20, 10, new WorldTile(IsActive: true, Type: 196, WallType: 59));
  _ = world.TrySetTile(21, 10, new WorldTile(IsActive: true, Type: 999, WallType: 1));
  _ = world.TrySetTile(22, 70, new WorldTile(IsActive: true, Type: 1, WallType: 2));
  WorldGenerationStateComponent state = new(31);
  List<TileChangeCommand> commands = new();
  LegacyWorldIsFrozen.AppendCommands(
    world.CreateSnapshot(metadata),
    underworldLayerY: 80,
    rockLayerY: 30,
    activeSecretSeedCount: 0,
    skyblockWorld: false,
    new LegacyPassRandomState(1456),
    ref state,
    commands);
  ushort[] expectedTileTypes = [161, 163, 200, 164, 147, 147, 147, 147, 147, 224, 460];
  for (int index = 0; index < expectedTileTypes.Length; index++)
  {
    int x = 10 + index;
    if (!commands.Any(command => command.X == x && command.Y == 10 &&
                                 command.Kind == TileChangeKind.UpdateTileType &&
                                 command.TileType == expectedTileTypes[index]))
    {
      throw new InvalidOperationException("DoWorldIsFrozen tile conversion mapping diverged.");
    }
  }

  if (!commands.Any(command => command.X == 10 && command.Y == 10 && command.WallType == 40) ||
      !commands.Any(command => command.X == 20 && command.Y == 10 && command.WallType == 40) ||
      commands.Any(command => command.X == 21 && command.Y == 10) ||
      commands.Any(command => command.X == 22 && command.Y == 70))
  {
    throw new InvalidOperationException(
      "DoWorldIsFrozen did not preserve wall or upper-bound conversion guards.");
  }

  WorldGenerationStateComponent retargetedState = new(31);
  List<TileChangeCommand> retargetedCommands = new();
  LegacyWorldIsFrozen.AppendCommands(
    world.CreateSnapshot(metadata),
    underworldLayerY: 80,
    rockLayerY: 30,
    activeSecretSeedCount: 6,
    skyblockWorld: false,
    new LegacyPassRandomState(1456),
    ref retargetedState,
    retargetedCommands);
  if (retargetedCommands.Any(command => command.X == 22 && command.Y == 70))
  {
    throw new InvalidOperationException("DoWorldIsFrozen did not retarget to the rock layer.");
  }

  WorldGenerationStateComponent skyblockState = new(31);
  List<TileChangeCommand> skyblockCommands = new();
  LegacyWorldIsFrozen.AppendCommands(
    world.CreateSnapshot(metadata),
    underworldLayerY: 80,
    rockLayerY: 30,
    activeSecretSeedCount: 0,
    skyblockWorld: true,
    new LegacyPassRandomState(1456),
    ref skyblockState,
    skyblockCommands);
  if (skyblockCommands.Count != 0)
  {
    throw new InvalidOperationException("DoWorldIsFrozen did not skip skyblock worlds.");
  }

  Console.WriteLine("PASS: DoWorldIsFrozen preserves bounded layer and conversion rules");
}

static void LegacyHallowOnSurfaceSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyHallowOnSurface", new WorldSeed(1456), width: 200, height: 150,
    worldId: 32, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  ushort[] sourceTypes = [2, 161, 53, 396, 397];
  ushort[] targetTypes = [109, 164, 116, 403, 402];
  for (int index = 0; index < sourceTypes.Length; index++)
  {
    _ = world.TrySetTile(10 + index, 10, new WorldTile(true, sourceTypes[index]));
  }

  _ = world.TrySetTile(20, 10, new WorldTile(true, 1, WallType: 63));
  _ = world.TrySetTile(21, 10, new WorldTile(true, 1, WallType: 187));
  _ = world.TrySetTile(22, 10, new WorldTile(true, 1, WallType: 216));
  _ = world.TrySetTile(23, 50, new WorldTile(true, 2));
  WorldGenerationStateComponent state = new(32);
  List<TileChangeCommand> commands = new();
  LegacyHallowOnSurface.AppendCommands(
    world.CreateSnapshot(metadata), 30, 80, false, false, false, false,
    new LegacyPassRandomState(1456), ref state, commands);
  for (int index = 0; index < targetTypes.Length; index++)
  {
    if (!commands.Any(command => command.X == 10 + index && command.Y == 10 &&
                                 command.TileType == targetTypes[index]))
    {
      throw new InvalidOperationException("DoHallowOnSurface tile mapping diverged.");
    }
  }

  if (!commands.Any(command => command.X == 20 && command.WallType == 70) ||
      !commands.Any(command => command.X == 21 && command.WallType == 222) ||
      !commands.Any(command => command.X == 22 && command.WallType == 219) ||
      commands.Any(command => command.X == 23 && command.Y == 50))
  {
    throw new InvalidOperationException("DoHallowOnSurface wall or bound rules diverged.");
  }

  WorldGenerationStateComponent noSurfaceState = new(32);
  List<TileChangeCommand> noSurfaceCommands = new();
  LegacyHallowOnSurface.AppendCommands(
    world.CreateSnapshot(metadata), 30, 80, true, true, true, false,
    new LegacyPassRandomState(1456), ref noSurfaceState, noSurfaceCommands);
  if (!noSurfaceCommands.Any(command => command.X == 20 && command.TileType == 117))
  {
    throw new InvalidOperationException("DoHallowOnSurface no-surface mapping diverged.");
  }

  Console.WriteLine("PASS: DoHallowOnSurface preserves bounded hallow conversions");
}

static void LegacyWorldInfectionPolicySourceCheck()
{
  LegacyWorldInfectionColumn normal = LegacyWorldInfectionPolicy.CreateColumn(
    10, 200, 30, 80, 130, false, false, false, false, false, false,
    new LegacyPassRandomState(1456));
  LegacyWorldInfectionColumn noInfection = LegacyWorldInfectionPolicy.CreateColumn(
    10, 200, 30, 80, 130, true, false, false, false, false, false,
    new LegacyPassRandomState(1456));
  LegacyWorldInfectionColumn combinedRules = LegacyWorldInfectionPolicy.CreateColumn(
    10, 200, 30, 80, 130, true, true, false, false, false, false,
    new LegacyPassRandomState(1456));
  LegacyWorldInfectionColumn drunkCrimson = LegacyWorldInfectionPolicy.CreateColumn(
    10, 200, 30, 80, 130, false, false, false, true, false, true,
    new LegacyPassRandomState(1456));
  if (normal.StartY is < 0 or > 2 || normal.ConversionType != 1 ||
      noInfection.StartY is < 40 or > 42 ||
      combinedRules.StartY is < 105 or > 107 ||
      drunkCrimson.ConversionType != 4 ||
      !LegacyWorldInfectionPolicy.ShouldConvertTiles(new WorldTile(true, 1)) ||
      LegacyWorldInfectionPolicy.ShouldConvertTiles(new WorldTile(true, 60)) ||
      !LegacyWorldInfectionPolicy.ShouldConvertWalls(new WorldTile(true, 1, WallType: 1)) ||
      LegacyWorldInfectionPolicy.ShouldConvertWalls(new WorldTile(true, 1, WallType: 70)))
  {
    throw new InvalidOperationException("DoWorldIsInfected policy diverged from source rules.");
  }

  Console.WriteLine("PASS: DoWorldIsInfected preserves column and conversion exclusion rules");
}

static void LegacyWorldInfectionConversionCommandSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyWorldInfectionConversion",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 37,
    seedVariant: "default");
  LegacyWorldInfectionConversionInput input = new(
    WorldSurfaceY: 30,
    RockLayerY: 80,
    UnderworldLayerY: 130,
    NoInfection: false,
    NoSurface: false,
    HallowOnSurface: false,
    DrunkWorld: false,
    Crimson: false,
    CrimsonLeft: false,
    SkyblockWorld: false);
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  LegacyWorldInfectionColumn[] columns = new LegacyWorldInfectionColumn[3];
  LegacyPassRandomState expectedRandom = new(1456);
  for (int x = 0; x < columns.Length; x++)
  {
    columns[x] = LegacyWorldInfectionPolicy.CreateColumn(
      x,
      metadata.Width,
      input.WorldSurfaceY,
      input.RockLayerY,
      input.UnderworldLayerY,
      input.NoInfection,
      input.NoSurface,
      input.HallowOnSurface,
      input.DrunkWorld,
      input.Crimson,
      input.CrimsonLeft,
      expectedRandom);
  }

  _ = world.TrySetTile(
    0,
    columns[0].StartY,
    new WorldTile(IsActive: true, Type: 60, WallType: 70));
  _ = world.TrySetTile(
    1,
    columns[1].StartY,
    new WorldTile(IsActive: true, Type: 60, WallType: 1));
  _ = world.TrySetTile(
    2,
    columns[2].StartY,
    new WorldTile(IsActive: true, Type: 1));

  WorldGenerationStateComponent state = new(37, nextSequence: 100);
  List<LegacyWorldInfectionConversionCommand> commands = new();
  LegacyWorldInfectionConversionCommandEmitter.AppendCommands(
    world.CreateSnapshot(metadata),
    input,
    new LegacyPassRandomState(1456),
    ref state,
    commands);
  if (commands.Count != 2 ||
      !commands.Any(command =>
        command.X == 1 && command.Y == columns[1].StartY &&
        command.ConversionType == 1 && !command.ConvertTiles && command.ConvertWalls) ||
      !commands.Any(command =>
        command.X == 2 && command.Y == columns[2].StartY &&
        command.ConversionType == 1 && command.ConvertTiles && !command.ConvertWalls) ||
      commands.Any(command => command.X == 0) ||
      commands[0].Sequence != 100 || commands[1].Sequence != 101 ||
      commands.Any(command => !command.IsValid(world.CreateSnapshot(metadata))))
  {
    throw new InvalidOperationException(
      "DoWorldIsInfected did not emit bounded typed conversion commands.");
  }

  WorldGrid drunkWorld = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  LegacyPassRandomState drunkExpectedRandom = new(1456);
  LegacyWorldInfectionColumn firstDrunkColumn = default;
  LegacyWorldInfectionColumn lastDrunkColumn = default;
  for (int x = 0; x < metadata.Width; x++)
  {
    LegacyWorldInfectionColumn column = LegacyWorldInfectionPolicy.CreateColumn(
      x,
      metadata.Width,
      input.WorldSurfaceY,
      input.RockLayerY,
      input.UnderworldLayerY,
      input.NoInfection,
      input.NoSurface,
      input.HallowOnSurface,
      drunkWorld: true,
      crimson: false,
      crimsonLeft: true,
      drunkExpectedRandom);
    if (x == 0)
    {
      firstDrunkColumn = column;
    }

    if (x == metadata.Width - 1)
    {
      lastDrunkColumn = column;
    }
  }

  _ = drunkWorld.TrySetTile(0, firstDrunkColumn.StartY, new WorldTile(true, 1));
  _ = drunkWorld.TrySetTile(
    metadata.Width - 1,
    lastDrunkColumn.StartY,
    new WorldTile(true, 1));
  LegacyWorldInfectionConversionInput drunkInput = input with
  {
    DrunkWorld = true,
    CrimsonLeft = true
  };
  List<LegacyWorldInfectionConversionCommand> drunkCommands = new();
  WorldGenerationStateComponent drunkState = new(37);
  LegacyWorldInfectionConversionCommandEmitter.AppendCommands(
    drunkWorld.CreateSnapshot(metadata),
    drunkInput,
    new LegacyPassRandomState(1456),
    ref drunkState,
    drunkCommands);
  if (!drunkCommands.Any(command =>
        command.X == 0 && command.ConversionType == 4) ||
      !drunkCommands.Any(command =>
        command.X == metadata.Width - 1 && command.ConversionType == 1))
  {
    throw new InvalidOperationException(
      "DoWorldIsInfected did not preserve drunk-world conversion-side selection.");
  }

  WorldGrid noInfectionWorld = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  LegacyWorldInfectionConversionInput noInfectionInput = input with
  {
    NoInfection = true,
    NoSurface = true
  };
  LegacyWorldInfectionColumn noInfectionColumn = LegacyWorldInfectionPolicy.CreateColumn(
    0,
    metadata.Width,
    noInfectionInput.WorldSurfaceY,
    noInfectionInput.RockLayerY,
    noInfectionInput.UnderworldLayerY,
    noInfectionInput.NoInfection,
    noInfectionInput.NoSurface,
    noInfectionInput.HallowOnSurface,
    noInfectionInput.DrunkWorld,
    noInfectionInput.Crimson,
    noInfectionInput.CrimsonLeft,
    new LegacyPassRandomState(1456));
  _ = noInfectionWorld.TrySetTile(
    0,
    noInfectionColumn.StartY - 1,
    new WorldTile(true, 1));
  _ = noInfectionWorld.TrySetTile(
    0,
    noInfectionColumn.StartY,
    new WorldTile(true, 1));
  List<LegacyWorldInfectionConversionCommand> noInfectionCommands = new();
  WorldGenerationStateComponent noInfectionState = new(37);
  LegacyWorldInfectionConversionCommandEmitter.AppendCommands(
    noInfectionWorld.CreateSnapshot(metadata),
    noInfectionInput,
    new LegacyPassRandomState(1456),
    ref noInfectionState,
    noInfectionCommands);
  if (!noInfectionCommands.Any(command =>
        command.X == 0 && command.Y == noInfectionColumn.StartY) ||
      noInfectionCommands.Any(command =>
        command.X == 0 && command.Y == noInfectionColumn.StartY - 1))
  {
    throw new InvalidOperationException(
      "DoWorldIsInfected did not preserve no-infection/no-surface start bounds.");
  }

  LegacyWorldInfectionConversionInput skyblockInput = input with { SkyblockWorld = true };
  List<LegacyWorldInfectionConversionCommand> skyblockCommands = new();
  WorldGenerationStateComponent skyblockState = new(37);
  LegacyWorldInfectionConversionCommandEmitter.AppendCommands(
    world.CreateSnapshot(metadata),
    skyblockInput,
    new LegacyPassRandomState(1456),
    ref skyblockState,
    skyblockCommands);
  if (skyblockCommands.Count != 0)
  {
    throw new InvalidOperationException(
      "DoWorldIsInfected did not skip skyblock worlds before emitting commands.");
  }

  Console.WriteLine(
    "PASS: DoWorldIsInfected emits bounded typed conversion commands with source gates");
}

static void LegacyWorldInfectionConversionRegistryAndCommitSourceCheck()
{
  IReadOnlyList<LegacyWorldInfectionConversionRule> rules =
    LegacyWorldInfectionConversionRegistry.RegisterDefaults();
  if (rules.Count != 82 || rules.Any(rule => rule.SourceTypes is not FrozenSet<ushort>))
  {
    throw new InvalidOperationException(
      "World infection conversion definitions were not frozen or complete.");
  }

  LegacyWorldInfectionConversionRule[] type1Tiles = rules
    .Where(rule => rule.ConversionType == 1 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Tile)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type4Tiles = rules
    .Where(rule => rule.ConversionType == 4 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Tile)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type1Walls = rules
    .Where(rule => rule.ConversionType == 1 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Wall)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type4Walls = rules
    .Where(rule => rule.ConversionType == 4 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Wall)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type2Tiles = rules
    .Where(rule => rule.ConversionType == 2 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Tile)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type2Walls = rules
    .Where(rule => rule.ConversionType == 2 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Wall)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type3Tiles = rules
    .Where(rule => rule.ConversionType == 3 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Tile)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type3Walls = rules
    .Where(rule => rule.ConversionType == 3 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Wall)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type8Tiles = rules
    .Where(rule => rule.ConversionType == 8 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Tile)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type9Tiles = rules
    .Where(rule => rule.ConversionType == 9 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Tile)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type10Tiles = rules
    .Where(rule => rule.ConversionType == 10 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Tile)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type5Tiles = rules
    .Where(rule => rule.ConversionType == 5 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Tile)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type5Walls = rules
    .Where(rule => rule.ConversionType == 5 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Wall)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type6Tiles = rules
    .Where(rule => rule.ConversionType == 6 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Tile)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  LegacyWorldInfectionConversionRule[] type6Walls = rules
    .Where(rule => rule.ConversionType == 6 &&
                   rule.Channel == LegacyWorldInfectionConversionChannel.Wall)
    .OrderBy(rule => rule.Priority)
    .ToArray();
  if (rules.Count != 82 ||
      !LegacyWorldInfectionConversionRegistry.TryGetTileRule(5, 2, out _) ||
      !LegacyWorldInfectionConversionRegistry.TryGetWallRule(6, 1, out _) ||
      !type5Tiles.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 0, 53, 397, 396, 69 }) ||
      !type5Walls.Select(rule => rule.TargetType).SequenceEqual(new ushort[] { 187, 216 }) ||
      !type6Tiles.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 0, 147, 161, 69 }) ||
      !type6Walls.Select(rule => rule.TargetType).SequenceEqual(new ushort[] { 71, 40 }) ||
      type5Tiles.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 5)) == false ||
      type5Walls.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 2)) == false ||
      type6Tiles.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 4)) == false ||
      type6Walls.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 2)) == false)
  {
    throw new InvalidOperationException(
      "Type-5/6 conversion rules were not admitted to the immutable registry.");
  }
  if (!type1Tiles.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 0, 25, 661, 23, 163, 112, 398, 400, 32 }) ||
      !type4Tiles.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 0, 203, 662, 199, 200, 234, 399, 401, 352 }) ||
      !type1Walls.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 69, 3, 217, 220, 188, 189, 190, 191 }) ||
      !type4Walls.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 81, 83, 218, 221, 192, 193, 194, 195 }) ||
      !type2Tiles.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 0, 117, 492, 109, 164, 116, 402, 403, 0 }) ||
      !type2Walls.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 70, 28, 219, 222, 200, 201, 202, 203 }) ||
      !type3Tiles.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 0, 70, 0 }) ||
      !type3Walls.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 80 }) ||
      !type8Tiles.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 211 }) ||
      !type9Tiles.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 60, 59, 1, 53, 397, 396, 0 }) ||
      !type10Tiles.Select(rule => rule.TargetType).SequenceEqual(
        new ushort[] { 60, 1, 53, 397, 396, 0 }) ||
      type1Tiles.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 9)) == false ||
      type4Tiles.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 9)) == false ||
      type1Walls.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 8)) == false ||
      type4Walls.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 8)) == false ||
      type2Tiles.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 9)) == false ||
      type2Walls.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 8)) == false ||
      type3Tiles.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 3)) == false ||
      type3Walls.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 1)) == false ||
      type8Tiles.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 1)) == false ||
      type9Tiles.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 7)) == false ||
      type10Tiles.Select(rule => rule.Priority).SequenceEqual(Enumerable.Range(0, 6)) == false)
  {
    throw new InvalidOperationException(
      "World infection conversion target tables or priority order diverged from Version4.");
  }

  static void AssertSourceTypes(
    LegacyWorldInfectionConversionChannel channel,
    LegacyWorldInfectionConversionCategory category,
    params ushort[] expected)
  {
    IReadOnlySet<ushort> actual = LegacyWorldInfectionConversionRegistry.GetSourceTypes(
      channel,
      category);
    if (!actual.SetEquals(expected) || actual is not FrozenSet<ushort>)
    {
      throw new InvalidOperationException(
        $"World infection source set drifted for {channel}/{category}.");
    }
  }

  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.Moss,
    179, 180, 181, 182, 183, 381, 534, 536, 539, 625, 627);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.Stone,
    1, 25, 117, 203);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.MossOrStone,
    1, 25, 117, 179, 180, 181, 182, 183, 203, 381, 534, 536, 539, 625, 627);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.JungleGrass,
    60, 661, 662);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.GolfGrass,
    477, 492);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.Grass,
    2, 23, 109, 199, 477, 492);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.Ice,
    161, 163, 164, 200);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.Sand,
    53, 112, 116, 234);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.HardenedSand,
    397, 398, 399, 402);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.Sandstone,
    396, 400, 401, 403);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.Thorn,
    32, 69, 352, 655);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.Dirt,
    0);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.Snow,
    147);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.MushroomGrass,
    60);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.ChlorophyteMushroomSurface,
    59, 60);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.ChlorophyteGrass,
    2, 23, 109, 199, 477, 492, 661, 662);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.ChlorophyteSecondaryGrass,
    23, 199, 661, 662);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.ChlorophyteStone,
    25, 203);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.ChlorophyteSand,
    112, 234);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.ChlorophyteHardenedSand,
    398, 399);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.ChlorophyteSandstone,
    400, 401);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.ChlorophyteKill,
    24, 32, 201, 205, 352, 636);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.Grass,
    63, 64, 65, 66, 67, 68, 69, 70, 81, 264, 265, 268);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.Stone,
    1, 3, 28, 61, 83, 185, 246, 248, 262, 269, 274, 349);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.Dirt,
    2, 16);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.Snow,
    40, 249);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.GlowingMushroomWall,
    15, 64, 67, 247);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.Ice,
    71, 266);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.HardenedSand,
    216, 217, 218, 219, 304, 305, 306, 307);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.Sandstone,
    187, 220, 221, 222, 275, 308, 309, 310);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.NewWall1,
    188, 192, 200, 204, 212, 276, 280, 288, 292, 300);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.NewWall2,
    189, 193, 201, 205, 213, 277, 281, 289, 293, 301);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.NewWall3,
    190, 194, 202, 206, 214, 278, 282, 290, 294, 302);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.NewWall4,
    191, 195, 203, 207, 215, 279, 283, 291, 295, 303);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.StoneFamilyWall,
    1, 3, 28, 61, 71, 83, 185, 187, 188, 189, 190, 191, 192, 193, 194, 195, 200, 201,
    202, 203, 204, 205, 206, 207, 212, 213, 214, 215, 220, 221, 222, 246, 248, 262,
    266, 269, 274, 275, 276, 277, 278, 279, 280, 281, 282, 283, 288, 289, 290, 291,
    292, 293, 294, 295, 300, 301, 302, 303, 308, 309, 310, 349);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Wall,
    LegacyWorldInfectionConversionCategory.HardenedSandDirtSnowWall,
    2, 16, 40, 216, 217, 218, 219, 249, 304, 305, 306, 307);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.GrassSandSnowDirt,
    0, 2, 23, 53, 109, 112, 116, 147, 199, 234, 477, 492);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.GrassSandHardenedSandSnowDirt,
    0, 2, 23, 53, 109, 112, 116, 147, 199, 234, 397, 398, 399, 402, 477, 492);
  AssertSourceTypes(
    LegacyWorldInfectionConversionChannel.Tile,
    LegacyWorldInfectionConversionCategory.MossStoneIceSandstone,
    1, 25, 117, 161, 163, 164, 179, 180, 181, 182, 183, 200, 203, 381, 396, 400, 401,
    403, 534, 536, 539, 625, 627);

  if (LegacyWorldInfectionConversionRegistry.TryGetTileRule(3, 1, out _) ||
      LegacyWorldInfectionConversionRegistry.TryGetWallRule(3, 63, out _) ||
      !LegacyWorldInfectionConversionRegistry.TryGetTileRule(2, 477, out
        LegacyWorldInfectionConversionRule golfRule) ||
      golfRule.Category != LegacyWorldInfectionConversionCategory.GolfGrass ||
      golfRule.TargetType != 492 ||
      !LegacyWorldInfectionConversionRegistry.IsTileTarget(3, 70) ||
      !LegacyWorldInfectionConversionRegistry.IsWallTarget(3, 80) ||
      !LegacyWorldInfectionConversionRegistry.TryGetTileRule(
        8,
        59,
        out LegacyWorldInfectionConversionRule type8Rule) ||
      type8Rule.TargetType != 211 ||
      !LegacyWorldInfectionConversionRegistry.TryGetTileRule(
        9,
        2,
        out LegacyWorldInfectionConversionRule type9GrassRule) ||
      type9GrassRule.TargetType != 60 ||
      !LegacyWorldInfectionConversionRegistry.TryGetTileRule(
        10,
        23,
        out LegacyWorldInfectionConversionRule type10GrassRule) ||
      type10GrassRule.TargetType != 60 ||
      LegacyWorldInfectionConversionRegistry.TryGetTileRule(10, 2, out _) ||
      !LegacyWorldInfectionConversionRegistry.TryGetTileRule(
        9,
        24,
        out LegacyWorldInfectionConversionRule type9KillRule) ||
      !type9KillRule.IsDeferred ||
      !LegacyWorldInfectionConversionRegistry.TryGetTileRule(
        10,
        636,
        out LegacyWorldInfectionConversionRule type10KillRule) ||
      !type10KillRule.IsDeferred ||
      LegacyWorldInfectionConversionRegistry.IsTileTarget(9, 0) ||
      LegacyWorldInfectionConversionRegistry.IsTileTarget(10, 0))
  {
    throw new InvalidOperationException(
      "Conversion registry resolved an unsupported type or lost type-2 GolfGrass priority.");
  }

  WorldMetadata metadata = new(
    "LegacyWorldInfectionConversionRegistry",
    new WorldSeed(1456),
    width: 200,
    height: 150,
    worldId: 137,
    seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  List<LegacyWorldInfectionConversionCommand> intents = new();
  Dictionary<(int X, int Y), WorldTile> expectedTiles = new();
  int coordinateIndex = 0;
  long intentSequence = 1000;
  LegacyWorldInfectionConversionRule[] regularRules = rules
    .Where(rule => !rule.IsDeferred && (rule.ConversionType is 1 or 4))
    .OrderBy(rule => rule.ConversionType)
    .ThenBy(rule => rule.Channel)
    .ThenBy(rule => rule.Priority)
    .ToArray();
  foreach (LegacyWorldInfectionConversionRule rule in regularRules)
  {
    ushort sourceType = rule.SourceTypes
      .Where(source => source != rule.TargetType)
      .Order()
      .First();
    int x = 10 + coordinateIndex++;
    int y = rule.Channel == LegacyWorldInfectionConversionChannel.Tile ? 10 : 20;
    WorldTile original = rule.Channel == LegacyWorldInfectionConversionChannel.Tile
      ? new WorldTile(
        true,
        sourceType,
        LiquidAmount: 123,
        LiquidType: 2,
        FrameX: 44,
        FrameY: 66,
        HasWire: true,
        HasWire2: true,
        HasWire3: true,
        HasWire4: true,
        IsHalfBrick: true,
        Slope: 2,
        IsActuated: true,
        TileColor: 7,
        WallColor: 8,
        IsInvisibleBlock: true,
        IsInvisibleWall: true,
        IsFullbrightBlock: true,
        IsFullbrightWall: true)
      : new WorldTile(
        true,
        1,
        LiquidAmount: 124,
        LiquidType: 1,
        FrameX: 45,
        FrameY: 67,
        WallType: sourceType,
        HasWire: true,
        IsHalfBrick: true,
        Slope: 1,
        IsActuated: true,
        TileColor: 9,
        WallColor: 10,
        IsInvisibleBlock: true,
        IsInvisibleWall: true,
        IsFullbrightBlock: true,
        IsFullbrightWall: true);
    _ = world.TrySetTile(x, y, original);
    expectedTiles[(x, y)] = rule.Channel == LegacyWorldInfectionConversionChannel.Tile
      ? original with { Type = rule.TargetType }
      : original with { WallType = rule.TargetType };
    intents.Add(new LegacyWorldInfectionConversionCommand(
      intentSequence++,
      x,
      y,
      rule.ConversionType,
      ConvertTiles: rule.Channel == LegacyWorldInfectionConversionChannel.Tile,
      ConvertWalls: rule.Channel == LegacyWorldInfectionConversionChannel.Wall));
  }

  int noOpTileX = 60;
  int noOpWallX = 61;
  _ = world.TrySetTile(noOpTileX, 30, new WorldTile(true, 203, FrameX: 11, FrameY: 12));
  _ = world.TrySetTile(noOpWallX, 30, new WorldTile(true, 1, WallType: 81, FrameX: 13, FrameY: 14));
  intents.Add(new LegacyWorldInfectionConversionCommand(
    intentSequence++, noOpTileX, 30, 4, ConvertTiles: true, ConvertWalls: false));
  intents.Add(new LegacyWorldInfectionConversionCommand(
    intentSequence++, noOpWallX, 30, 4, ConvertTiles: false, ConvertWalls: true));

  int torchX = 62;
  int thornX = 63;
  int unknownTileX = 64;
  int inactiveTileX = 65;
  int unknownWallX = 66;
  int emptyWallX = 67;
  _ = world.TrySetTile(torchX, 30, new WorldTile(true, 4, FrameY: 44));
  _ = world.TrySetTile(thornX, 30, new WorldTile(true, 69));
  _ = world.TrySetTile(unknownTileX, 30, new WorldTile(true, 999));
  _ = world.TrySetTile(inactiveTileX, 30, new WorldTile(false, 1));
  _ = world.TrySetTile(unknownWallX, 30, new WorldTile(true, 1, WallType: 999));
  _ = world.TrySetTile(emptyWallX, 30, new WorldTile(true, 1));
  intents.Add(new LegacyWorldInfectionConversionCommand(
    intentSequence++, torchX, 30, 1, ConvertTiles: true, ConvertWalls: false));
  intents.Add(new LegacyWorldInfectionConversionCommand(
    intentSequence++, thornX, 30, 1, ConvertTiles: true, ConvertWalls: false));
  intents.Add(new LegacyWorldInfectionConversionCommand(
    intentSequence++, unknownTileX, 30, 1, ConvertTiles: true, ConvertWalls: false));
  intents.Add(new LegacyWorldInfectionConversionCommand(
    intentSequence++, inactiveTileX, 30, 1, ConvertTiles: true, ConvertWalls: false));
  intents.Add(new LegacyWorldInfectionConversionCommand(
    intentSequence++, unknownWallX, 30, 1, ConvertTiles: false, ConvertWalls: true));
  intents.Add(new LegacyWorldInfectionConversionCommand(
    intentSequence++, emptyWallX, 30, 1, ConvertTiles: false, ConvertWalls: true));

  WorldGridSnapshot sourceSnapshot = world.CreateSnapshot(metadata);
  WorldGrid replayWorld = WorldGrid.FromSnapshot(sourceSnapshot);
  WorldGenerationStateComponent state = new(137, nextSequence: 5000);
  WorldGenerationStateComponent replayState = new(137, nextSequence: 5000);
  LegacyWorldInfectionConversionCommitResult reverseResult;
  bool reverseCommitted = LegacyWorldInfectionConversionCommitBoundary.TryCommit(
    world,
    sourceSnapshot,
    intents.OrderByDescending(command => command.Sequence).ToArray(),
    ref state,
    out reverseResult);
  LegacyWorldInfectionConversionCommitResult forwardResult;
  bool forwardCommitted = LegacyWorldInfectionConversionCommitBoundary.TryCommit(
    replayWorld,
    sourceSnapshot,
    intents,
    ref replayState,
    out forwardResult);
  if (!reverseCommitted || !forwardCommitted ||
      reverseResult.AppliedTileCount != 14 ||
      reverseResult.AppliedWallCount != 16 ||
      reverseResult.NoOpCount != 2 ||
      reverseResult.DeferredCount != 6 ||
      state.NextSequence != 5030 ||
      replayState.NextSequence != 5030 ||
      !reverseResult.Deferred.SequenceEqual(forwardResult.Deferred))
  {
    throw new InvalidOperationException(
      "World infection conversion commit was not deterministic or fully reported.");
  }

  foreach (KeyValuePair<(int X, int Y), WorldTile> expected in expectedTiles)
  {
    if (world.GetTile(expected.Key.X, expected.Key.Y) != expected.Value ||
        replayWorld.GetTile(expected.Key.X, expected.Key.Y) != expected.Value)
    {
      throw new InvalidOperationException(
        "World infection conversion did not preserve supported channel state.");
    }
  }

  if (world.GetTile(torchX, 30).Type != 4 ||
      world.GetTile(thornX, 30).Type != 69 ||
      world.GetTile(unknownTileX, 30).Type != 999 ||
      world.GetTile(inactiveTileX, 30).Type != 1 ||
      world.GetTile(unknownWallX, 30).WallType != 999 ||
      world.GetTile(emptyWallX, 30).WallType != 0 ||
      world.GetTile(noOpTileX, 30).Type != 203 ||
      world.GetTile(noOpWallX, 30).WallType != 81)
  {
    throw new InvalidOperationException(
      "Deferred or already-targeted conversion channels were mutated unexpectedly.");
  }

  if (reverseResult.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.TorchFrameMutation) != 1 ||
      reverseResult.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.ThornKillMutation) != 1 ||
      reverseResult.Deferred.Count(deferred =>
        deferred.Reason ==
          LegacyWorldInfectionConversionDeferredReason.TileCategoryNotRegistered) != 1 ||
      reverseResult.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.InactiveTile) != 1 ||
      reverseResult.Deferred.Count(deferred =>
        deferred.Reason ==
          LegacyWorldInfectionConversionDeferredReason.WallCategoryNotRegistered) != 1 ||
      reverseResult.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.EmptyWall) != 1)
  {
    throw new InvalidOperationException(
      "Unsupported conversion channels did not retain structured Deferred reasons.");
  }

  WorldGrid staleWorld = WorldGrid.FromSnapshot(sourceSnapshot);
  _ = staleWorld.TrySetTile(10, 10, staleWorld.GetTile(10, 10) with { TileColor = 99 });
  WorldGenerationStateComponent staleState = new(137, nextSequence: 6000);
  LegacyWorldInfectionConversionCommitResult staleResult;
  if (LegacyWorldInfectionConversionCommitBoundary.TryCommit(
        staleWorld,
        sourceSnapshot,
        new[] { intents[0] },
        ref staleState,
        out staleResult) ||
      staleResult.Succeeded ||
      staleState.NextSequence != 6000)
  {
    throw new InvalidOperationException(
      "Stale world infection conversion snapshots were not rejected atomically.");
  }

  LegacyWorldInfectionConversionCommitResult invalidResult;
  WorldGenerationStateComponent invalidState = new(137, nextSequence: 6100);
  if (LegacyWorldInfectionConversionCommitBoundary.TryCommit(
        replayWorld,
        sourceSnapshot,
        [new LegacyWorldInfectionConversionCommand(1, 1, 1, 7, true, false)],
        ref invalidState,
        out invalidResult) ||
      invalidState.NextSequence != 6100)
  {
    throw new InvalidOperationException(
      "Unsupported conversion type was not rejected before projection.");
  }

  WorldGrid type2World = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  LegacyWorldInfectionConversionRule[] type2RegularRules = rules
    .Where(rule => rule.ConversionType == 2 && !rule.IsDeferred)
    .OrderBy(rule => rule.Channel)
    .ThenBy(rule => rule.Priority)
    .ToArray();
  List<LegacyWorldInfectionConversionCommand> type2Intents = new();
  Dictionary<(int X, int Y), WorldTile> type2ExpectedTiles = new();
  int type2Index = 0;
  long type2IntentSequence = 700;
  foreach (LegacyWorldInfectionConversionRule rule in type2RegularRules)
  {
    ushort sourceType = rule.SourceTypes
      .Where(source => source != rule.TargetType)
      .Order()
      .First();
    int x = 10 + type2Index++;
    int y = rule.Channel == LegacyWorldInfectionConversionChannel.Tile ? 10 : 20;
    WorldTile original = rule.Channel == LegacyWorldInfectionConversionChannel.Tile
      ? new WorldTile(
        true,
        sourceType,
        LiquidAmount: 91,
        LiquidType: 2,
        FrameX: 31,
        FrameY: 41,
        HasWire: true,
        IsHalfBrick: true,
        Slope: 3,
        IsActuated: true,
        TileColor: 12,
        WallColor: 13,
        IsInvisibleBlock: true,
        IsInvisibleWall: true,
        IsFullbrightBlock: true,
        IsFullbrightWall: true)
      : new WorldTile(
        true,
        1,
        LiquidAmount: 92,
        LiquidType: 1,
        FrameX: 32,
        FrameY: 42,
        WallType: sourceType,
        HasWire: true,
        IsHalfBrick: true,
        Slope: 2,
        IsActuated: true,
        TileColor: 14,
        WallColor: 15,
        IsInvisibleBlock: true,
        IsInvisibleWall: true,
        IsFullbrightBlock: true,
        IsFullbrightWall: true);
    _ = type2World.TrySetTile(x, y, original);
    type2ExpectedTiles[(x, y)] = rule.Channel == LegacyWorldInfectionConversionChannel.Tile
      ? original with { Type = rule.TargetType }
      : original with { WallType = rule.TargetType };
    type2Intents.Add(new LegacyWorldInfectionConversionCommand(
      type2IntentSequence++,
      x,
      y,
      2,
      ConvertTiles: rule.Channel == LegacyWorldInfectionConversionChannel.Tile,
      ConvertWalls: rule.Channel == LegacyWorldInfectionConversionChannel.Wall));
  }

  const int type2SelfTileX = 50;
  const int type2SelfWallX = 51;
  _ = type2World.TrySetTile(type2SelfTileX, 30, new WorldTile(true, 492));
  _ = type2World.TrySetTile(
    type2SelfWallX,
    30,
    new WorldTile(true, 1, WallType: 70));
  type2Intents.Add(new LegacyWorldInfectionConversionCommand(
    type2IntentSequence++, type2SelfTileX, 30, 2, true, false));
  type2Intents.Add(new LegacyWorldInfectionConversionCommand(
    type2IntentSequence++, type2SelfWallX, 30, 2, false, true));

  const int type2TorchX = 52;
  const int type2ThornX = 53;
  const int type2ContextualX = 54;
  const int type2NoContextX = 57;
  _ = type2World.TrySetTile(type2TorchX, 30, new WorldTile(true, 4, FrameY: 66));
  _ = type2World.TrySetTile(type2ThornX, 30, new WorldTile(true, 69));
  _ = type2World.TrySetTile(type2ContextualX, 30, new WorldTile(true, 59));
  _ = type2World.TrySetTile(type2ContextualX + 1, 30, new WorldTile(true, 109));
  _ = type2World.TrySetTile(type2NoContextX, 30, new WorldTile(true, 59));
  type2Intents.Add(new LegacyWorldInfectionConversionCommand(
    type2IntentSequence++, type2TorchX, 30, 2, true, false));
  type2Intents.Add(new LegacyWorldInfectionConversionCommand(
    type2IntentSequence++, type2ThornX, 30, 2, true, false));
  type2Intents.Add(new LegacyWorldInfectionConversionCommand(
    type2IntentSequence++, type2ContextualX, 30, 2, true, false));
  type2Intents.Add(new LegacyWorldInfectionConversionCommand(
    type2IntentSequence++, type2NoContextX, 30, 2, true, false));

  WorldGridSnapshot type2Snapshot = type2World.CreateSnapshot(metadata);
  WorldGenerationStateComponent type2State = new(137, nextSequence: 8000);
  LegacyWorldInfectionConversionCommitResult type2Result;
  if (!LegacyWorldInfectionConversionCommitBoundary.TryCommit(
        type2World,
        type2Snapshot,
        type2Intents,
        ref type2State,
        out type2Result) ||
      type2Result.AppliedTileCount != 7 ||
      type2Result.AppliedWallCount != 8 ||
      type2Result.NoOpCount != 3 ||
      type2Result.DeferredCount != 3 ||
      type2State.NextSequence != 8015 ||
      type2Result.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.TorchFrameMutation) != 1 ||
      type2Result.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.ThornKillMutation) != 1 ||
      type2Result.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.AdjacentGrassCleanup) != 1)
  {
    throw new InvalidOperationException(
      "Type-2 conversion did not preserve regular mappings or Deferred side effects.");
  }

  foreach (KeyValuePair<(int X, int Y), WorldTile> expected in type2ExpectedTiles)
  {
    if (type2World.GetTile(expected.Key.X, expected.Key.Y) != expected.Value)
    {
      throw new InvalidOperationException(
        "Type-2 conversion did not preserve unrelated tile state.");
    }
  }

  if (type2World.GetTile(type2SelfTileX, 30).Type != 492 ||
      type2World.GetTile(type2SelfWallX, 30).WallType != 70 ||
      type2World.GetTile(type2TorchX, 30).FrameY != 66 ||
      type2World.GetTile(type2ThornX, 30).Type != 69 ||
      type2World.GetTile(type2ContextualX, 30).Type != 59 ||
      type2World.GetTile(type2NoContextX, 30).Type != 59)
  {
    throw new InvalidOperationException(
      "Type-2 no-op or Deferred conversion paths mutated the world unexpectedly.");
  }

  WorldGrid type3World = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  ushort[] type3WallSources = [15, 64, 67, 247];
  for (int index = 0; index < type3WallSources.Length; index++)
  {
    _ = type3World.TrySetTile(
      10 + index,
      20,
      new WorldTile(
        true,
        1,
        LiquidAmount: 75,
        LiquidType: 1,
        FrameX: 17,
        FrameY: 27,
        WallType: type3WallSources[index],
        HasWire: true,
        IsHalfBrick: true,
        Slope: 1,
        IsActuated: true,
        TileColor: 16,
        WallColor: 17,
        IsInvisibleBlock: true,
        IsInvisibleWall: true,
        IsFullbrightBlock: true,
        IsFullbrightWall: true));
  }

  _ = type3World.TrySetTile(
    20,
    20,
    new WorldTile(
      true,
      60,
      LiquidAmount: 76,
      LiquidType: 2,
      FrameX: 18,
      FrameY: 28,
      HasWire: true,
      IsHalfBrick: true,
      Slope: 2,
      IsActuated: true,
      TileColor: 18,
      WallColor: 19,
      IsInvisibleBlock: true,
      IsInvisibleWall: true,
      IsFullbrightBlock: true,
      IsFullbrightWall: true));
  const int type3SelfTileX = 21;
  const int type3SelfWallX = 22;
  const int type3TorchX = 23;
  const int type3ThornX = 24;
  _ = type3World.TrySetTile(type3SelfTileX, 20, new WorldTile(true, 70));
  _ = type3World.TrySetTile(type3SelfWallX, 20, new WorldTile(true, 1, WallType: 80));
  _ = type3World.TrySetTile(type3TorchX, 20, new WorldTile(true, 4, FrameY: 88));
  _ = type3World.TrySetTile(type3ThornX, 20, new WorldTile(true, 69));

  List<LegacyWorldInfectionConversionCommand> type3Intents = new();
  long type3Sequence = 100;
  for (int index = 0; index < type3WallSources.Length; index++)
  {
    type3Intents.Add(new LegacyWorldInfectionConversionCommand(
      type3Sequence++,
      10 + index,
      20,
      3,
      ConvertTiles: false,
      ConvertWalls: true));
  }

  type3Intents.Add(new LegacyWorldInfectionConversionCommand(
    type3Sequence++, 20, 20, 3, ConvertTiles: true, ConvertWalls: false));
  type3Intents.Add(new LegacyWorldInfectionConversionCommand(
    type3Sequence++, type3SelfTileX, 20, 3, ConvertTiles: true, ConvertWalls: false));
  type3Intents.Add(new LegacyWorldInfectionConversionCommand(
    type3Sequence++, type3SelfWallX, 20, 3, ConvertTiles: false, ConvertWalls: true));
  type3Intents.Add(new LegacyWorldInfectionConversionCommand(
    type3Sequence++, type3TorchX, 20, 3, ConvertTiles: true, ConvertWalls: false));
  type3Intents.Add(new LegacyWorldInfectionConversionCommand(
    type3Sequence++, type3ThornX, 20, 3, ConvertTiles: true, ConvertWalls: false));
  WorldGridSnapshot type3Snapshot = type3World.CreateSnapshot(metadata);
  WorldGenerationStateComponent type3State = new(137, nextSequence: 9000);
  LegacyWorldInfectionConversionCommitResult type3Result;
  if (!LegacyWorldInfectionConversionCommitBoundary.TryCommit(
        type3World,
        type3Snapshot,
        type3Intents,
        ref type3State,
        out type3Result) ||
      type3Result.AppliedTileCount != 1 ||
      type3Result.AppliedWallCount != 4 ||
      type3Result.NoOpCount != 2 ||
      type3Result.DeferredCount != 2 ||
      type3State.NextSequence != 9005 ||
      type3Result.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.TorchFrameMutation) != 1 ||
      type3Result.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.ThornKillMutation) != 1)
  {
    throw new InvalidOperationException(
      "Type-3 conversion did not preserve regular mappings or Deferred side effects.");
  }

  for (int index = 0; index < type3WallSources.Length; index++)
  {
    if (type3World.GetTile(10 + index, 20).WallType != 80 ||
        type3World.GetTile(10 + index, 20).LiquidAmount != 75 ||
        type3World.GetTile(10 + index, 20).IsActuated != true)
    {
      throw new InvalidOperationException(
        "Type-3 wall conversion did not preserve unrelated tile state.");
    }
  }

  if (type3World.GetTile(20, 20).Type != 70 ||
      type3World.GetTile(type3SelfTileX, 20).Type != 70 ||
      type3World.GetTile(type3SelfWallX, 20).WallType != 80 ||
      type3World.GetTile(type3TorchX, 20).FrameY != 88 ||
      type3World.GetTile(type3ThornX, 20).Type != 69)
  {
    throw new InvalidOperationException(
      "Type-3 no-op or Deferred conversion paths mutated the world unexpectedly.");
  }

  LegacyWorldInfectionConversionRule[] type8To10RegularRules = rules
    .Where(rule => rule.ConversionType is 8 or 9 or 10 && !rule.IsDeferred)
    .OrderBy(rule => rule.ConversionType)
    .ThenBy(rule => rule.Priority)
    .ToArray();
  if (type8To10RegularRules.Length != 12)
  {
    throw new InvalidOperationException(
      "Type-8/9/10 regular conversion rule count diverged from Version4.");
  }

  WorldGrid type8To10World = new(
    metadata.Width,
    metadata.Height,
    initializeLegacyEmptyFrames: true);
  List<LegacyWorldInfectionConversionCommand> type8To10Intents = new();
  Dictionary<(int X, int Y), WorldTile> type8To10ExpectedTiles = new();
  int type8To10Index = 0;
  long type8To10Sequence = 1200;
  foreach (LegacyWorldInfectionConversionRule rule in type8To10RegularRules)
  {
    ushort sourceType = rule.SourceTypes
      .Where(source => source != rule.TargetType)
      .Order()
      .First();
    int x = 10 + type8To10Index++;
    const int y = 50;
    WorldTile original = new(
      true,
      sourceType,
      LiquidAmount: 91,
      LiquidType: 2,
      FrameX: 31,
      FrameY: 41,
      WallType: (ushort)(100 + type8To10Index),
      HasWire: true,
      HasWire2: true,
      IsHalfBrick: true,
      Slope: 3,
      IsActuated: true,
      TileColor: 12,
      WallColor: 13,
      IsInvisibleBlock: true,
      IsInvisibleWall: true,
      IsFullbrightBlock: true,
      IsFullbrightWall: true);
    _ = type8To10World.TrySetTile(x, y, original);
    type8To10ExpectedTiles[(x, y)] = original with { Type = rule.TargetType };
    type8To10Intents.Add(new LegacyWorldInfectionConversionCommand(
      type8To10Sequence++,
      x,
      y,
      rule.ConversionType,
      ConvertTiles: true,
      ConvertWalls: false));
  }

  const int type8To10Self8X = 40;
  const int type8To10Self9X = 41;
  const int type8To10Self10X = 42;
  const int type8To10Kill9X = 43;
  const int type8To10Kill10X = 44;
  const int type8To10WallOnlyX = 45;
  _ = type8To10World.TrySetTile(type8To10Self8X, 50, new WorldTile(true, 211));
  _ = type8To10World.TrySetTile(type8To10Self9X, 50, new WorldTile(true, 60));
  _ = type8To10World.TrySetTile(type8To10Self10X, 50, new WorldTile(true, 1));
  _ = type8To10World.TrySetTile(type8To10Kill9X, 50, new WorldTile(true, 24));
  _ = type8To10World.TrySetTile(type8To10Kill10X, 50, new WorldTile(true, 636));
  _ = type8To10World.TrySetTile(
    type8To10WallOnlyX,
    50,
    new WorldTile(true, 1, WallType: 77, FrameX: 51, FrameY: 61));
  type8To10Intents.Add(new LegacyWorldInfectionConversionCommand(
    type8To10Sequence++, type8To10Self8X, 50, 8, true, false));
  type8To10Intents.Add(new LegacyWorldInfectionConversionCommand(
    type8To10Sequence++, type8To10Self9X, 50, 9, true, false));
  type8To10Intents.Add(new LegacyWorldInfectionConversionCommand(
    type8To10Sequence++, type8To10Self10X, 50, 10, true, false));
  type8To10Intents.Add(new LegacyWorldInfectionConversionCommand(
    type8To10Sequence++, type8To10Kill9X, 50, 9, true, false));
  type8To10Intents.Add(new LegacyWorldInfectionConversionCommand(
    type8To10Sequence++, type8To10Kill10X, 50, 10, true, false));
  type8To10Intents.Add(new LegacyWorldInfectionConversionCommand(
    type8To10Sequence++, type8To10WallOnlyX, 50, 9, false, true));

  WorldGridSnapshot type8To10Snapshot = type8To10World.CreateSnapshot(metadata);
  WorldGrid type8To10ReplayWorld = WorldGrid.FromSnapshot(type8To10Snapshot);
  WorldGenerationStateComponent type8To10State = new(137, nextSequence: 12000);
  WorldGenerationStateComponent type8To10ReplayState =
    new(137, nextSequence: 12000);
  LegacyWorldInfectionConversionCommitResult type8To10ReverseResult;
  bool type8To10ReverseCommitted = LegacyWorldInfectionConversionCommitBoundary.TryCommit(
    type8To10World,
    type8To10Snapshot,
    type8To10Intents.OrderByDescending(command => command.Sequence).ToArray(),
    ref type8To10State,
    out type8To10ReverseResult);
  LegacyWorldInfectionConversionCommitResult type8To10ForwardResult;
  bool type8To10ForwardCommitted = LegacyWorldInfectionConversionCommitBoundary.TryCommit(
    type8To10ReplayWorld,
    type8To10Snapshot,
    type8To10Intents,
    ref type8To10ReplayState,
    out type8To10ForwardResult);
  if (!type8To10ReverseCommitted || !type8To10ForwardCommitted ||
      type8To10ReverseResult.AppliedTileCount != 12 ||
      type8To10ReverseResult.AppliedWallCount != 0 ||
      type8To10ReverseResult.NoOpCount != 3 ||
      type8To10ReverseResult.DeferredCount != 3 ||
      type8To10State.NextSequence != 12012 ||
      type8To10ReplayState.NextSequence != 12012 ||
      !type8To10ReverseResult.Deferred.SequenceEqual(type8To10ForwardResult.Deferred) ||
      type8To10ReverseResult.Deferred.Count(deferred =>
        deferred.Reason ==
          LegacyWorldInfectionConversionDeferredReason.ChlorophyteKillMutation) != 2 ||
      type8To10ReverseResult.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.UnsupportedChannel) != 1)
  {
    throw new InvalidOperationException(
      "Type-8/9/10 conversion did not preserve deterministic regular and Deferred paths.");
  }

  foreach (KeyValuePair<(int X, int Y), WorldTile> expected in type8To10ExpectedTiles)
  {
    if (type8To10World.GetTile(expected.Key.X, expected.Key.Y) != expected.Value ||
        type8To10ReplayWorld.GetTile(expected.Key.X, expected.Key.Y) != expected.Value)
    {
      throw new InvalidOperationException(
        "Type-8/9/10 conversion did not preserve unrelated tile state.");
    }
  }

  if (type8To10World.GetTile(type8To10Self8X, 50).Type != 211 ||
      type8To10World.GetTile(type8To10Self9X, 50).Type != 60 ||
      type8To10World.GetTile(type8To10Self10X, 50).Type != 1 ||
      type8To10World.GetTile(type8To10Kill9X, 50).Type != 24 ||
      type8To10World.GetTile(type8To10Kill10X, 50).Type != 636 ||
      type8To10World.GetTile(type8To10WallOnlyX, 50).Type != 1 ||
      type8To10World.GetTile(type8To10WallOnlyX, 50).WallType != 77)
  {
    throw new InvalidOperationException(
        "Type-8/9/10 no-op, KillTile, or wall-channel paths mutated unexpectedly.");
  }

  const int type5OpenX = 10;
  const int type5SolidX = 11;
  const int type5WallX = 12;
  const int type5SelfX = 13;
  const int type6GrassX = 14;
  const int type6StoneX = 15;
  const int type6WallX = 16;
  const int type6WallSecondX = 17;
  const int type5TorchX = 18;
  const int type5ThornX = 19;
  const int type6TorchX = 20;
  const int type6ThornX = 21;
  WorldGrid type5And6World = new(
    metadata.Width,
    metadata.Height,
    initializeLegacyEmptyFrames: true);
  WorldTile preservedState = new(
    true,
    2,
    LiquidAmount: 73,
    LiquidType: 2,
    FrameX: 31,
    FrameY: 41,
    WallType: 91,
    HasWire: true,
    IsHalfBrick: true,
    Slope: 3,
    IsActuated: true,
    TileColor: 12,
    WallColor: 13,
    IsInvisibleBlock: true,
    IsInvisibleWall: true,
    IsFullbrightBlock: true,
    IsFullbrightWall: true);
  _ = type5And6World.TrySetTile(type5OpenX, 50, preservedState);
  _ = type5And6World.TrySetTile(type5SolidX, 50, preservedState with { WallType = 0 });
  _ = type5And6World.TrySetTile(type5SolidX, 51, new WorldTile(true, 1));
  _ = type5And6World.TrySetTile(type5WallX, 50, preservedState with { WallType = 1 });
  _ = type5And6World.TrySetTile(type5SelfX, 50, preservedState with { Type = 53 });
  _ = type5And6World.TrySetTile(type6GrassX, 50, preservedState with { WallType = 0 });
  _ = type5And6World.TrySetTile(type6StoneX, 50, preservedState with { Type = 1, WallType = 0 });
  _ = type5And6World.TrySetTile(type6WallX, 50, preservedState with { WallType = 1 });
  _ = type5And6World.TrySetTile(type6WallSecondX, 50, preservedState with { WallType = 216 });
  _ = type5And6World.TrySetTile(type5TorchX, 50, preservedState with { Type = 4 });
  _ = type5And6World.TrySetTile(type5ThornX, 50, preservedState with { Type = 32 });
  _ = type5And6World.TrySetTile(type6TorchX, 50, preservedState with { Type = 4 });
  _ = type5And6World.TrySetTile(type6ThornX, 50, preservedState with { Type = 32 });
  WorldGridSnapshot type5And6Snapshot = type5And6World.CreateSnapshot(metadata);
  TileDefinitionRegistry type5And6Definitions = TileDefinitionRegistry.RegisterDefaults();
  if (!SandFallEligibilityQuery.BlockBelowMakesSandConvertIntoHardenedSand(
        type5And6Snapshot,
        type5And6Definitions,
        type5OpenX,
        50) ||
      SandFallEligibilityQuery.BlockBelowMakesSandConvertIntoHardenedSand(
        type5And6Snapshot,
        type5And6Definitions,
        type5SolidX,
        50))
  {
    throw new InvalidOperationException(
      "Type-5 sand conversion did not preserve the below-tile support query.");
  }

  List<LegacyWorldInfectionConversionCommand> type5And6Intents =
  [
    new(2000, type5OpenX, 50, 5, ConvertTiles: true, ConvertWalls: false),
    new(2001, type5SolidX, 50, 5, ConvertTiles: true, ConvertWalls: false),
    new(2002, type5WallX, 50, 5, ConvertTiles: false, ConvertWalls: true),
    new(2003, type5SelfX, 50, 5, ConvertTiles: true, ConvertWalls: false),
    new(2004, type6GrassX, 50, 6, ConvertTiles: true, ConvertWalls: false),
    new(2005, type6StoneX, 50, 6, ConvertTiles: true, ConvertWalls: false),
    new(2006, type6WallX, 50, 6, ConvertTiles: false, ConvertWalls: true),
    new(2007, type6WallSecondX, 50, 6, ConvertTiles: false, ConvertWalls: true),
    new(2008, type5TorchX, 50, 5, ConvertTiles: true, ConvertWalls: false),
    new(2009, type5ThornX, 50, 5, ConvertTiles: true, ConvertWalls: false),
    new(2010, type6TorchX, 50, 6, ConvertTiles: true, ConvertWalls: false),
    new(2011, type6ThornX, 50, 6, ConvertTiles: true, ConvertWalls: false)
  ];
  WorldGrid type5And6ReplayWorld = WorldGrid.FromSnapshot(type5And6Snapshot);
  WorldGenerationStateComponent type5And6State = new(137, nextSequence: 12000);
  WorldGenerationStateComponent type5And6ReplayState =
    new(137, nextSequence: 12000);
  LegacyWorldInfectionConversionCommitResult type5And6ReverseResult;
  bool type5And6ReverseCommitted = LegacyWorldInfectionConversionCommitBoundary.TryCommit(
    type5And6World,
    type5And6Snapshot,
    type5And6Intents.OrderByDescending(command => command.Sequence).ToArray(),
    ref type5And6State,
    out type5And6ReverseResult);
  LegacyWorldInfectionConversionCommitResult type5And6ForwardResult;
  bool type5And6ForwardCommitted = LegacyWorldInfectionConversionCommitBoundary.TryCommit(
    type5And6ReplayWorld,
    type5And6Snapshot,
    type5And6Intents,
    ref type5And6ReplayState,
    out type5And6ForwardResult);
  if (!type5And6ReverseCommitted || !type5And6ForwardCommitted ||
      type5And6ReverseResult.AppliedTileCount != 4 ||
      type5And6ReverseResult.AppliedWallCount != 3 ||
      type5And6ReverseResult.NoOpCount != 1 ||
      type5And6ReverseResult.DeferredCount != 4 ||
      type5And6State.NextSequence != 12007 ||
      type5And6ReplayState.NextSequence != 12007 ||
      !type5And6ReverseResult.Deferred.SequenceEqual(type5And6ForwardResult.Deferred) ||
      type5And6ReverseResult.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.TorchFrameMutation) != 2 ||
      type5And6ReverseResult.Deferred.Count(deferred =>
        deferred.Reason == LegacyWorldInfectionConversionDeferredReason.ThornKillMutation) != 2)
  {
    throw new InvalidOperationException(
      "Type-5/6 conversion did not preserve deterministic regular and Deferred paths.");
  }

  if (type5And6World.GetTile(type5OpenX, 50).Type != 397 ||
      type5And6World.GetTile(type5SolidX, 50).Type != 53 ||
      type5And6World.GetTile(type5WallX, 50).WallType != 187 ||
      type5And6World.GetTile(type5SelfX, 50).Type != 53 ||
      type5And6World.GetTile(type6GrassX, 50).Type != 147 ||
      type5And6World.GetTile(type6StoneX, 50).Type != 161 ||
      type5And6World.GetTile(type6WallX, 50).WallType != 71 ||
      type5And6World.GetTile(type6WallSecondX, 50).WallType != 40 ||
      type5And6World.GetTile(type5TorchX, 50).Type != 4 ||
      type5And6World.GetTile(type5ThornX, 50).Type != 32 ||
      type5And6World.GetTile(type6TorchX, 50).Type != 4 ||
      type5And6World.GetTile(type6ThornX, 50).Type != 32)
  {
    throw new InvalidOperationException(
      "Type-5/6 no-op or Deferred paths mutated the world unexpectedly.");
  }

  if (type5And6World.GetTile(type5OpenX, 50).LiquidAmount != preservedState.LiquidAmount ||
      type5And6World.GetTile(type5OpenX, 50).FrameY != preservedState.FrameY ||
      type5And6World.GetTile(type5OpenX, 50).HasWire != preservedState.HasWire ||
      type5And6World.GetTile(type5WallX, 50).TileColor != preservedState.TileColor ||
      type5And6World.GetTile(type6WallX, 50).IsFullbrightWall != preservedState.IsFullbrightWall)
  {
    throw new InvalidOperationException(
      "Type-5/6 conversion did not preserve unrelated tile state.");
  }

  Console.WriteLine(
    "PASS: World infection conversion registry and deterministic commit preserve type-5/6 " +
    "mappings and Deferred paths");
}

static void LegacyNoInfectionPolicySourceCheck()
{
  if (LegacyNoInfectionPolicy.GetConversion(new WorldTile(true, 70, WallType: 80), false).HasWork ||
      LegacyNoInfectionPolicy.GetConversion(new WorldTile(true, 203, WallType: 83), true).HasWork ||
      LegacyNoInfectionPolicy.GetConversion(new WorldTile(true, 25, WallType: 3), true).HasWork ||
      LegacyNoInfectionPolicy.GetConversion(new WorldTile(true, 1, WallType: 1), true) !=
        new LegacyNoInfectionConversion(true, true))
  {
    throw new InvalidOperationException("DoNoInfection conversion flags diverged from source.");
  }

  Console.WriteLine("PASS: DoNoInfection preserves source conversion exclusion flags");
}

static void LegacyWorldIsFrozenFinishPolicySourceCheck()
{
  LegacyFrozenChestItemIntent chestIntent = default;
  bool createdChestIntent = false;
  for (int seed = 1; seed <= 32 && !createdChestIntent; seed++)
  {
    createdChestIntent = LegacyWorldIsFrozenFinishPolicy.TryCreateChestItemIntent(
      [4, 2, 0, 0], new LegacyPassRandomState(seed), out chestIntent);
  }

  IReadOnlyList<LegacyFrozenNpcReplacementIntent> replacements =
    LegacyWorldIsFrozenFinishPolicy.CreateNpcReplacementIntents(
      [1, 22, 44, 22], 123, 456, false, true);
  if (!createdChestIntent || chestIntent != new LegacyFrozenChestItemIntent(2, 1869) ||
      !replacements.SequenceEqual(
        [new LegacyFrozenNpcReplacementIntent(1, 142, 123, 456),
         new LegacyFrozenNpcReplacementIntent(3, 142, 123, 456)]) ||
      LegacyWorldIsFrozenFinishPolicy.CreateNpcReplacementIntents(
        [22], 1, 2, true, true).Count != 0 ||
      LegacyWorldIsFrozenFinishPolicy.CreateNpcReplacementIntents(
        [22], 1, 2, false, false).Count != 0)
  {
    throw new InvalidOperationException("DoWorldIsFrozenFinish intents diverged from source.");
  }

  Console.WriteLine("PASS: DoWorldIsFrozenFinish preserves chest and NPC replacement intents");
}

static void LegacyBiomeCleanupSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyBiomeCleanup", new WorldSeed(1456), width: 200, height: 150,
    worldId: 33, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(10, 10, new WorldTile(true, 147, WallType: 187));
  _ = world.TrySetTile(11, 10, new WorldTile(true, 161, WallType: 216));
  _ = world.TrySetTile(12, 10, new WorldTile(true, 1, WallType: 204));
  _ = world.TrySetTile(4, 10, new WorldTile(true, 147, WallType: 187));
  WorldGenerationStateComponent state = new(33);
  List<TileChangeCommand> desertCommands = new();
  LegacySurfaceIsDesertNoSurfaceCleanup.AppendCommands(
    world.CreateSnapshot(metadata), false, ref state, desertCommands);
  List<TileChangeCommand> spiderCommands = new();
  LegacyNoSpiderCavesCleanup.AppendCommands(world.CreateSnapshot(metadata), ref state, spiderCommands);
  if (!desertCommands.Any(command => command.X == 10 && command.TileType == 397) ||
      !desertCommands.Any(command => command.X == 11 && command.TileType == 396) ||
      desertCommands.Any(command => command.X == 4) ||
      spiderCommands.Count != 1 || spiderCommands[0].X != 12 || spiderCommands[0].WallType != 62)
  {
    throw new InvalidOperationException("Biome cleanup command boundaries diverged from source.");
  }

  Console.WriteLine("PASS: desert and spider cleanup preserve bounded commands");
}

static void LegacyActuallyNoTrapsSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyActuallyNoTraps", new WorldSeed(1456), width: 200, height: 150,
    worldId: 34, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(10, 10, new WorldTile(true, 48));
  _ = world.TrySetTile(11, 10, new WorldTile(true, 232));
  _ = world.TrySetTile(12, 10, new WorldTile(true, 1));
  WorldGenerationStateComponent state = new(34);
  List<TileChangeCommand> commands = new();
  LegacyActuallyNoTraps.AppendHardModeCommands(
    world.CreateSnapshot(metadata), true, ref state, commands);
  if (commands.Count != 2 || commands.Any(command => command.Kind != TileChangeKind.Kill) ||
      !commands.Select(command => command.X).Order().SequenceEqual([10, 11]))
  {
    throw new InvalidOperationException("DoActuallyNoTraps hard mode diverged from source.");
  }

  WorldGenerationStateComponent disabledState = new(34);
  List<TileChangeCommand> disabledCommands = new();
  LegacyActuallyNoTraps.AppendHardModeCommands(
    world.CreateSnapshot(metadata), false, ref disabledState, disabledCommands);
  if (disabledCommands.Count != 0)
  {
    throw new InvalidOperationException("DoActuallyNoTraps did not preserve hard-mode gate.");
  }

  Console.WriteLine("PASS: DoActuallyNoTraps preserves bounded hard-mode removals");
}

static void LegacyRainbowStaticRewriteSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyRainbowStaticRewrite", new WorldSeed(1456), width: 200, height: 150,
    worldId: 35, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(10, 10, new WorldTile(true, 189));
  _ = world.TrySetTile(11, 10, new WorldTile(true, 202));
  _ = world.TrySetTile(12, 10, new WorldTile(true, 1, WallType: 82));
  _ = world.TrySetTile(13, 10, new WorldTile(true, 1, WallType: 1));
  WorldGenerationStateComponent state = new(35);
  List<TileChangeCommand> commands = new();
  LegacyRainbowStaticRewrite.AppendCommands(world.CreateSnapshot(metadata), ref state, commands);
  if (!commands.Any(command => command.X == 10 && command.TileType == 719) ||
      !commands.Any(command => command.X == 11 && command.TileType == 692) ||
      !commands.Any(command => command.X == 12 && command.WallType == 346) ||
      commands.Any(command => command.X == 13))
  {
    throw new InvalidOperationException("DoRainbowStuff static rewrite diverged from source.");
  }

  Console.WriteLine("PASS: DoRainbowStuff preserves static tile and wall rewrites");
}

static void LegacyPaintEverythingGraySourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyPaintEverythingGray", new WorldSeed(1456), width: 200, height: 150,
    worldId: 36, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(0, 0, new WorldTile(true, 7, TileColor: 1, WallColor: 2));
  _ = world.TrySetTile(1, 0, new WorldTile(true, 63, TileColor: 3, WallColor: 4));
  _ = world.TrySetTile(2, 0, new WorldTile(true, 178, TileColor: 5, WallColor: 6));
  _ = world.TrySetTile(3, 0, new WorldTile(true, 1, TileColor: 7, WallColor: 8));
  _ = world.TrySetTile(0, 6, new WorldTile(true, 7, TileColor: 9, WallColor: 10));
  WorldGenerationStateComponent treasureState = new(36);
  List<TileChangeCommand> treasureCommands = new();
  LegacyPaintEverythingGray.AppendCommands(
    world.CreateSnapshot(metadata),
    worldSurfaceY: 3,
    useWhitePaint: false,
    justTheSurface: true,
    justTreasure: true,
    oreTileTypes: new HashSet<ushort> { 7 },
    gemTileTypes: new HashSet<ushort> { 63 },
    new LegacyPassRandomState(1456),
    ref treasureState,
    treasureCommands);
  if (treasureCommands.Count != 3 ||
      treasureCommands.Any(command => command.Kind != TileChangeKind.SetPaint ||
                                      command.TileColor != 27 ||
                                      command.WallColor is not null) ||
      !treasureCommands.Select(command => command.X).Order().SequenceEqual([0, 1, 2]) ||
      !new TileChangeCommitSystem().TryCommit(world, treasureCommands, out _) ||
      world.GetTile(0, 0).TileColor != 27 || world.GetTile(0, 0).WallColor != 2 ||
      world.GetTile(1, 0).TileColor != 27 || world.GetTile(1, 0).WallColor != 4 ||
      world.GetTile(2, 0).TileColor != 27 || world.GetTile(2, 0).WallColor != 6 ||
      world.GetTile(3, 0).TileColor != 7 || world.GetTile(0, 6).TileColor != 9)
  {
    throw new InvalidOperationException(
      "DoPaintEverythingGray treasure and surface boundaries diverged from source.");
  }

  WorldGenerationStateComponent fullState = new(37);
  List<TileChangeCommand> fullCommands = new();
  LegacyPaintEverythingGray.AppendCommands(
    world.CreateSnapshot(metadata),
    worldSurfaceY: 3,
    useWhitePaint: true,
    justTheSurface: false,
    justTreasure: false,
    oreTileTypes: new HashSet<ushort>(),
    gemTileTypes: new HashSet<ushort>(),
    new LegacyPassRandomState(1456),
    ref fullState,
    fullCommands);
  if (fullCommands.Count != metadata.Width * metadata.Height ||
      fullCommands.Any(command => command.TileColor != 26 || command.WallColor != 26))
  {
    throw new InvalidOperationException(
      "DoPaintEverythingGray full white paint commands diverged from source.");
  }

  Console.WriteLine("PASS: DoPaintEverythingGray preserves bounded paint command semantics");
}

static void LegacyPaintEverythingNegativeSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyPaintEverythingNegative", new WorldSeed(1456), width: 200, height: 150,
    worldId: 38, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(10, 50, new WorldTile(true, 100, TileColor: 1));
  _ = world.TrySetTile(10, 51, new WorldTile(true, 200, TileColor: 2));
  _ = world.TrySetTile(10, 52, new WorldTile(true, 19, WallType: 300, TileColor: 3));
  _ = world.TrySetTile(10, 53, new WorldTile(true, 400, TileColor: 4));
  _ = world.TrySetTile(10, 54, new WorldTile(true, 1, WallType: 73, WallColor: 5));
  _ = world.TrySetTile(10, 55, new WorldTile(true, 192, TileColor: 6));
  _ = world.TrySetTile(10, 56, new WorldTile(true, 52, TileColor: 7));
  _ = world.TrySetTile(10, 57, new WorldTile(true, 382, TileColor: 8));
  _ = world.TrySetTile(10, 58, new WorldTile(false, 187, TileColor: 9));
  _ = world.TrySetTile(10, 59, new WorldTile(true, 186, TileColor: 10));
  _ = world.TrySetTile(10, 60, new WorldTile(true, 384, TileColor: 11));
  _ = world.TrySetTile(10, 61, new WorldTile(true, 1, WallType: 60, WallColor: 12));
  _ = world.TrySetTile(10, 70, new WorldTile(true, 1, TileColor: 13, WallColor: 14));
  LegacyPaintEverythingNegativeProfile profile = new(
    new HashSet<ushort> { 100 },
    new HashSet<ushort> { 200 },
    new HashSet<ushort> { 300 },
    new HashSet<ushort> { 400 },
    new HashSet<ushort> { 52, 382, 62 },
    new HashSet<ushort> { 186, 187 });
  WorldGenerationStateComponent selectedState = new(38);
  List<TileChangeCommand> selectedCommands = new();
  LegacyPaintEverythingNegative.AppendCommands(
    world.CreateSnapshot(metadata),
    worldSurfaceY: 40,
    justUnderground: false,
    justSomeThings: true,
    profile,
    new LegacyPassRandomState(1456),
    ref selectedState,
    selectedCommands);
  if (!new TileChangeCommitSystem().TryCommit(world, selectedCommands, out _) ||
      world.GetTile(10, 50).TileColor != 30 ||
      world.GetTile(10, 51).TileColor != 30 ||
      world.GetTile(10, 52).TileColor != 30 ||
      world.GetTile(10, 52).WallColor != 30 ||
      world.GetTile(10, 53).TileColor != 30 ||
      world.GetTile(10, 54).WallColor != 30 ||
      world.GetTile(10, 55).TileColor != 30 ||
      world.GetTile(10, 56).TileColor != 30 ||
      world.GetTile(10, 57).TileColor != 30 ||
      world.GetTile(10, 58).TileColor != 30 ||
      world.GetTile(10, 59).TileColor != 30 ||
      world.GetTile(10, 60).TileColor != 30 ||
      world.GetTile(10, 61).WallColor != 30 ||
      world.GetTile(10, 70).TileColor != 13 ||
      world.GetTile(10, 70).WallColor != 14)
  {
    throw new InvalidOperationException(
      "DoPaintEverythingNegative selective type and neighbor paint behavior diverged.");
  }

  int expectedFirstY = 40 - new LegacyPassRandomState(1456).Next(3);
  WorldGenerationStateComponent fullState = new(39);
  List<TileChangeCommand> fullCommands = new();
  LegacyPaintEverythingNegative.AppendCommands(
    world.CreateSnapshot(metadata),
    worldSurfaceY: 40,
    justUnderground: true,
    justSomeThings: false,
    profile,
    new LegacyPassRandomState(1456),
    ref fullState,
    fullCommands);
  if (fullCommands.Count == 0 || fullCommands[0].X != 0 ||
      fullCommands[0].Y != expectedFirstY ||
      fullCommands.Any(command => command.TileColor != 30 || command.WallColor != 30))
  {
    throw new InvalidOperationException(
      "DoPaintEverythingNegative full underground paint behavior diverged.");
  }

  Console.WriteLine("PASS: DoPaintEverythingNegative preserves bounded paint command semantics");
}

static void LegacyCoatEverythingEchoSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyCoatEverythingEcho", new WorldSeed(1456), width: 200, height: 150,
    worldId: 40, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(10, 10, new WorldTile(true, 48));
  _ = world.TrySetTile(11, 10, new WorldTile(true, 1, WallType: 2));
  _ = world.TrySetTile(12, 10, new WorldTile(true, 1, WallType: 2));
  IReadOnlySet<ushort> solidTypes = new HashSet<ushort> { 1 };
  LegacyCoatEverythingEchoProfile profile = new(
    new HashSet<ushort> { 48 },
    new HashSet<ushort>(),
    solidTypes);
  WorldGenerationStateComponent state = new(40);
  List<TileChangeCommand> commands = new();
  LegacyCoatEverythingEcho.AppendCommands(
    world.CreateSnapshot(metadata), false, false, false, profile, ref state, commands);
  if (!commands.Any(command => command.X == 10 && command.IsInvisibleBlock == true) ||
      !commands.Any(command => command.X == 11 && command.IsInvisibleWall == true) ||
      !new TileChangeCommitSystem().TryCommit(world, commands, out _) ||
      !world.GetTile(10, 10).IsInvisibleBlock || !world.GetTile(11, 10).IsInvisibleWall)
  {
    throw new InvalidOperationException("DoCoatEverythingEcho selective coating diverged from source.");
  }

  Console.WriteLine("PASS: DoCoatEverythingEcho preserves bounded coating command semantics");
}

static void LegacyCoatEverythingIlluminantSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyCoatEverythingIlluminant", new WorldSeed(1456), width: 200, height: 150,
    worldId: 41, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(10, 10, new WorldTile(true, 12));
  _ = world.TrySetTile(11, 10, new WorldTile(true, 1, IsInvisibleBlock: true, IsInvisibleWall: true));
  LegacyCoatEverythingIlluminantProfile profile = new(new HashSet<ushort> { 12 });
  WorldGenerationStateComponent state = new(41);
  List<TileChangeCommand> commands = new();
  LegacyCoatEverythingIlluminant.AppendCommands(
    world.CreateSnapshot(metadata), true, false, profile, new LegacyPassRandomState(1456),
    ref state, commands);
  if (!new TileChangeCommitSystem().TryCommit(world, commands, out _) ||
      !world.GetTile(10, 10).IsFullbrightBlock || world.GetTile(10, 10).IsFullbrightWall)
  {
    throw new InvalidOperationException(
      "DoCoatEverythingIlluminant selective coating diverged from source.");
  }

  LegacyPassRandomState expectedRandom = new(1456);
  bool expectedRandomSpot = false;
  for (int x = 0; x < metadata.Width; x++)
  {
    for (int y = 0; y < metadata.Height; y++)
    {
      bool selected = expectedRandom.Next(2) == 0;
      if (x == 11 && y == 10)
      {
        expectedRandomSpot = selected;
      }
    }
  }

  WorldGenerationStateComponent randomState = new(42);
  List<TileChangeCommand> randomCommands = new();
  LegacyCoatEverythingIlluminant.AppendCommands(
    world.CreateSnapshot(metadata), false, true, profile, new LegacyPassRandomState(1456),
    ref randomState, randomCommands);
  if (!new TileChangeCommitSystem().TryCommit(world, randomCommands, out _) ||
      world.GetTile(11, 10).IsFullbrightBlock != expectedRandomSpot ||
      world.GetTile(11, 10).IsFullbrightWall != expectedRandomSpot ||
      world.GetTile(11, 10).IsInvisibleBlock == expectedRandomSpot ||
      world.GetTile(11, 10).IsInvisibleWall == expectedRandomSpot)
  {
    throw new InvalidOperationException(
      "DoCoatEverythingIlluminant random coating diverged from source RNG replay.");
  }

  Console.WriteLine("PASS: DoCoatEverythingIlluminant preserves bounded coating command semantics");
}

static void LegacyNoSurfacePolicySourceCheck()
{
  LegacyNoSurfaceActionPlan noAction = LegacyNoSurfacePolicy.Create(false, true, false, 8400);
  LegacyNoSurfaceActionPlan spawnAction = LegacyNoSurfacePolicy.Create(false, false, false, 8400);
  LegacyNoSurfaceActionPlan alreadyRandomized = LegacyNoSurfacePolicy.Create(false, false, true, 8400);
  LegacyNoSurfaceActionPlan skyblock = LegacyNoSurfacePolicy.Create(true, false, false, 8400);
  if (noAction.UndergroundMeteorAttempts != 8 || noAction.RandomizeSpawn ||
      noAction.PlaceTorchesAroundSpawn || spawnAction.UndergroundMeteorAttempts != 8 ||
      !spawnAction.RandomizeSpawn || !spawnAction.PlaceTorchesAroundSpawn ||
      alreadyRandomized.RandomizeSpawn || alreadyRandomized.PlaceTorchesAroundSpawn ||
      skyblock.UndergroundMeteorAttempts != 0 || skyblock.RandomizeSpawn)
  {
    throw new InvalidOperationException("DoNoSurface action policy diverged from source.");
  }

  Console.WriteLine("PASS: DoNoSurface preserves bounded meteor and spawn action policy");
}

static void LegacyErrorWorldRandomBlockSourceCheck()
{
  IReadOnlyList<LegacyErrorWorldTileDefinition> definitions = new[]
  {
    new LegacyErrorWorldTileDefinition(0, true, false, false, false),
    new LegacyErrorWorldTileDefinition(58, true, false, false, false),
    new LegacyErrorWorldTileDefinition(1, false, false, false, false)
  };
  if (!LegacyErrorWorldRandomBlock.TrySelect(
        definitions,
        new LegacyPassRandomState(1456),
        out ushort selectedTileType) ||
      selectedTileType != 0 ||
      LegacyErrorWorldRandomBlock.TrySelect(
        new[] { new LegacyErrorWorldTileDefinition(58, true, false, false, false) },
        new LegacyPassRandomState(1456),
        out _))
  {
    throw new InvalidOperationException("DoErrorWorldGetRandomBlock selection diverged from source.");
  }

  Console.WriteLine("PASS: DoErrorWorldGetRandomBlock preserves explicit candidate selection");
}

static void LegacyErrorWorldSingleTileSwapSourceCheck()
{
  WorldMetadata metadata = new(
    "ErrorWorldSwap", new WorldSeed(1456), width: 200, height: 150,
    worldId: 61, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  WorldTile first = new(
    true, 1, LiquidAmount: 15, LiquidType: 1, FrameX: 18, FrameY: 36, WallType: 4,
    Slope: 2, TileColor: 5, IsFullbrightBlock: true);
  WorldTile second = new(
    false, 2, LiquidAmount: 20, LiquidType: 2, FrameX: 72, FrameY: 54, WallType: 5,
    Slope: 3, TileColor: 8, IsInvisibleBlock: true);
  _ = world.TrySetTile(10, 10, first);
  _ = world.TrySetTile(20, 20, second);
  WorldGenerationStateComponent state = new(61);
  List<TileChangeCommand> commands = new();
  LegacyErrorWorldTileSwap.AppendCommands(10, 10, 20, 20, first, second, ref state, commands);
  if (commands.Count != 6 || commands[0].Kind != TileChangeKind.UpdateTileType ||
      commands[1].Kind != TileChangeKind.SetPaint || commands[2].Kind != TileChangeKind.SetCoating ||
      commands[3].Kind != TileChangeKind.UpdateTileType ||
      !new TileChangeCommitSystem().TryCommit(world, commands, out _))
  {
    throw new InvalidOperationException("ErrorWorld single-tile swap command shape diverged.");
  }

  WorldTile firstAfter = world.GetTile(10, 10);
  WorldTile secondAfter = world.GetTile(20, 20);
  if (firstAfter.IsActive != second.IsActive || firstAfter.Type != second.Type ||
      firstAfter.TileColor != second.TileColor ||
      firstAfter.IsFullbrightBlock != second.IsFullbrightBlock ||
      firstAfter.IsInvisibleBlock != second.IsInvisibleBlock || firstAfter.LiquidAmount != 15 ||
      firstAfter.FrameX != 18 || firstAfter.WallType != 4 || firstAfter.Slope != 2 ||
      secondAfter.IsActive != first.IsActive || secondAfter.Type != first.Type ||
      secondAfter.TileColor != first.TileColor ||
      secondAfter.IsFullbrightBlock != first.IsFullbrightBlock ||
      secondAfter.IsInvisibleBlock != first.IsInvisibleBlock || secondAfter.LiquidAmount != 20 ||
      secondAfter.FrameX != 72 || secondAfter.WallType != 5 || secondAfter.Slope != 3)
  {
    throw new InvalidOperationException("ErrorWorld single-tile swap fields diverged from source.");
  }

  Console.WriteLine("PASS: DoErrorWorldShuffleBlocks preserves bounded single-tile swaps");
}

static void LegacyErrorWorldRectangleSwapSourceCheck()
{
  WorldMetadata metadata = new(
    "ErrorWorldRectangleSwap", new WorldSeed(1456), width: 200, height: 150,
    worldId: 62, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  for (int offsetX = 0; offsetX < 2; offsetX++)
  {
    for (int offsetY = 0; offsetY < 3; offsetY++)
    {
      _ = world.TrySetTile(
        10 + offsetX,
        10 + offsetY,
        new WorldTile(true, (ushort)(10 + offsetX + offsetY), TileColor: 1));
      _ = world.TrySetTile(
        30 + offsetX,
        30 + offsetY,
        new WorldTile(false, (ushort)(30 + offsetX + offsetY), TileColor: 2));
    }
  }

  WorldGenerationStateComponent state = new(62);
  List<TileChangeCommand> commands = new();
  LegacyErrorWorldTileRectangleSwap.AppendCommands(
    world.CreateSnapshot(metadata), 10, 10, 30, 30, 2, 3, ref state, commands);
  if (commands.Count != 36 || !new TileChangeCommitSystem().TryCommit(world, commands, out _) ||
      world.GetTile(10, 10).Type != 30 || world.GetTile(10, 10).TileColor != 2 ||
      world.GetTile(31, 32).Type != 13 || world.GetTile(31, 32).TileColor != 1)
  {
    throw new InvalidOperationException("ErrorWorld rectangle swap commands diverged from source.");
  }

  Console.WriteLine("PASS: DoErrorWorldShuffleBlocks preserves bounded rectangle swaps");
}

static void LegacyErrorWorldCandidateSourceCheck()
{
  WorldMetadata metadata = new(
    "ErrorWorldCandidate", new WorldSeed(1456), width: 200, height: 150,
    worldId: 63, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(10, 9, new WorldTile(true, 1));
  _ = world.TrySetTile(10, 10, new WorldTile(true, 1));
  _ = world.TrySetTile(10, 11, new WorldTile(true, 1));
  Dictionary<ushort, LegacyErrorWorldTileDefinition> definitions = new()
  {
    [1] = new LegacyErrorWorldTileDefinition(1, true, false, false, false),
    [2] = new LegacyErrorWorldTileDefinition(2, true, false, true, false)
  };
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  if (!LegacyErrorWorldCandidateQuery.IsSwapEligible(snapshot, definitions, 10, 10))
  {
    throw new InvalidOperationException("ErrorWorld solid candidate was rejected.");
  }

  _ = world.TrySetTile(10, 10, new WorldTile(true, 1, LiquidAmount: 1, LiquidType: 3));
  _ = world.TrySetTile(10, 11, new WorldTile(true, 2));
  snapshot = world.CreateSnapshot(metadata);
  if (LegacyErrorWorldCandidateQuery.IsSwapEligible(snapshot, definitions, 10, 10))
  {
    throw new InvalidOperationException("ErrorWorld shimmer or neighboring frame rejection diverged.");
  }

  Console.WriteLine("PASS: DoErrorWorldShuffleBlocks preserves bounded candidate eligibility");
}

static void LegacyErrorWorldCandidateSelectionSourceCheck()
{
  WorldMetadata metadata = new(
    "ErrorWorldSelection", new WorldSeed(1456), width: 200, height: 150,
    worldId: 64, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  for (int x = 30; x < 40; x++)
  {
    for (int y = 30; y < 40; y++)
    {
      _ = world.TrySetTile(x, y, new WorldTile(true, 1));
    }
  }

  Dictionary<ushort, LegacyErrorWorldTileDefinition> definitions = new()
  {
    [0] = new LegacyErrorWorldTileDefinition(0, false, false, false, false),
    [1] = new LegacyErrorWorldTileDefinition(1, true, false, false, false)
  };
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  bool firstSelected = LegacyErrorWorldCandidateSelector.TrySelect(
    snapshot, definitions, 30, 40, 30, 40, 2, 2, new LegacyPassRandomState(1456),
    out LegacyErrorWorldCandidate first);
  bool secondSelected = LegacyErrorWorldCandidateSelector.TrySelect(
    snapshot, definitions, 30, 40, 30, 40, 2, 2, new LegacyPassRandomState(1456),
    out LegacyErrorWorldCandidate second);
  if (!firstSelected || !secondSelected || first != second ||
      !LegacyErrorWorldCandidateQuery.IsSwapEligible(snapshot, definitions, first.X, first.Y) ||
      LegacyErrorWorldCandidateSelector.TrySelect(
        snapshot, definitions, 0, 20, 20, 30, 1, 1, new LegacyPassRandomState(1456), out _))
  {
    throw new InvalidOperationException("ErrorWorld candidate selection diverged from source bounds.");
  }

  Console.WriteLine("PASS: DoErrorWorldShuffleBlocks preserves bounded candidate selection");
}

static void LegacyErrorWorldSwapOperationSourceCheck()
{
  WorldMetadata metadata = new(
    "ErrorWorldOperation", new WorldSeed(1456), width: 200, height: 150,
    worldId: 65, seedVariant: "default");
  Dictionary<ushort, LegacyErrorWorldTileDefinition> definitions = new()
  {
    [0] = new LegacyErrorWorldTileDefinition(0, false, false, false, false),
    [1] = new LegacyErrorWorldTileDefinition(1, true, false, false, false),
    [2] = new LegacyErrorWorldTileDefinition(2, true, false, false, false)
  };
  LegacyErrorWorldSpawnExclusionProfile normalProfile = new(200, 20.0, 130, false);
  bool appended = false;
  for (int seed = 1; seed <= 128 && !appended; seed++)
  {
    WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
    for (int x = 30; x < 34; x++)
    {
      for (int y = 30; y < 34; y++)
      {
        _ = world.TrySetTile(x, y, new WorldTile(true, 1));
        _ = world.TrySetTile(x + 40, y, new WorldTile(true, 2));
      }
    }

    WorldGenerationStateComponent state = new(65);
    List<TileChangeCommand> commands = new();
    appended = LegacyErrorWorldSwapOperation.TryAppendSingleTileSwap(
      world.CreateSnapshot(metadata), definitions, normalProfile, 30, 74, 30, 34,
      new LegacyPassRandomState(seed), ref state, commands);
    if (appended && (!new TileChangeCommitSystem().TryCommit(world, commands, out _) ||
                     commands.Count != 6 || !commands.Any(command => command.TileType == 1) ||
                     !commands.Any(command => command.TileType == 2)))
    {
      throw new InvalidOperationException("ErrorWorld single-tile swap operation diverged.");
    }
  }

  if (!appended || !LegacyErrorWorldSpawnExclusionPolicy.IsSingleTileExcluded(
        normalProfile, 100, 19))
  {
    throw new InvalidOperationException("ErrorWorld swap operation or spawn exclusion diverged.");
  }

  Console.WriteLine("PASS: DoErrorWorldShuffleBlocks preserves bounded swap operations");
}

static void LegacyErrorWorldAxisTrailSourceCheck()
{
  WorldMetadata metadata = new(
    "ErrorWorldAxisTrail", new WorldSeed(1456), width: 200, height: 150,
    worldId: 66, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  WorldTile source = new(true, 7, FrameX: 12, FrameY: 18, WallType: 4, IsHalfBrick: true,
    Slope: 2, TileColor: 9, IsInvisibleBlock: true, IsFullbrightBlock: true);
  _ = world.TrySetTile(100, 75, source);
  _ = world.TrySetTile(101, 75, new WorldTile(false, 0));
  _ = world.TrySetTile(102, 75, new WorldTile(true, 8));
  WorldGenerationStateComponent state = new(66);
  List<TileChangeCommand> commands = new();
  int copied = LegacyErrorWorldAxisTrail.AppendCommands(
    world.CreateSnapshot(metadata), 100, 75, new LegacyErrorWorldAxisDirection(1, 0), 10,
    ref state, commands);
  if (copied != 1 || commands.Count != 4 || !new TileChangeCommitSystem().TryCommit(world, commands, out _) ||
      world.GetTile(101, 75).Type != 7 || !world.GetTile(101, 75).IsInvisibleBlock ||
      world.GetTile(100, 75).IsHalfBrick || world.GetTile(100, 75).Slope != 0 ||
      world.GetTile(102, 75).Type != 8)
  {
    throw new InvalidOperationException("ErrorWorld axis trail diverged from source traversal.");
  }

  Console.WriteLine("PASS: DoErrorWorldShuffleBlocks preserves bounded axis trail");
}

static void LegacyErrorWorldAxisTrailPolicySourceCheck()
{
  bool sawHorizontal = false;
  bool sawVertical = false;
  for (int seed = 1; seed <= 64; seed++)
  {
    LegacyErrorWorldAxisTrailPlan first = LegacyErrorWorldAxisTrailPolicy.Create(
      new LegacyPassRandomState(seed));
    LegacyErrorWorldAxisTrailPlan second = LegacyErrorWorldAxisTrailPolicy.Create(
      new LegacyPassRandomState(seed));
    if (first != second || !first.Direction.IsValid || first.Length is < 5 or > 20)
    {
      throw new InvalidOperationException("ErrorWorld axis trail random policy diverged.");
    }

    sawHorizontal |= first.Direction.X != 0;
    sawVertical |= first.Direction.Y != 0;
  }

  if (!sawHorizontal || !sawVertical)
  {
    throw new InvalidOperationException("ErrorWorld axis trail random policy did not cover both axes.");
  }

  Console.WriteLine("PASS: DoErrorWorldShuffleBlocks preserves axis trail random policy");
}

static void LegacyErrorWorldPassCountSourceCheck()
{
  LegacyErrorWorldPassCounts normal = LegacyErrorWorldPassCountPolicy.Create(4200, 1, false);
  LegacyErrorWorldPassCounts skyblock = LegacyErrorWorldPassCountPolicy.Create(4200, 1, true);
  LegacyErrorWorldPassCounts adjusted = LegacyErrorWorldPassCountPolicy.Create(4200, 4, false);
  if (normal != new LegacyErrorWorldPassCounts(42000, 2100, 2100, 42000) ||
      skyblock != new LegacyErrorWorldPassCounts(21000, 2100, 2100, 42000) ||
      adjusted != new LegacyErrorWorldPassCounts(10500, 525, 525, 10500))
  {
    throw new InvalidOperationException("ErrorWorld pass counts diverged from source arithmetic.");
  }

  Console.WriteLine("PASS: DoErrorWorldShuffleBlocks preserves source pass counts");
}

static void LegacyErrorWorldRandomBlockRewriteSourceCheck()
{
  WorldMetadata metadata = new(
    "ErrorWorldRewrite", new WorldSeed(1456), width: 200, height: 150,
    worldId: 67, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(40, 40, new WorldTile(false, 1));
  Dictionary<ushort, LegacyErrorWorldTileDefinition> definitions = new()
  {
    [0] = new LegacyErrorWorldTileDefinition(0, false, false, false, false),
    [1] = new LegacyErrorWorldTileDefinition(1, true, false, false, false),
    [2] = new LegacyErrorWorldTileDefinition(2, true, false, false, false)
  };
  WorldGenerationStateComponent state = new(67);
  List<TileChangeCommand> commands = new();
  if (!LegacyErrorWorldRandomBlockRewrite.TryAppendCommand(
        world.CreateSnapshot(metadata), definitions, new[] { definitions[2] }, 40, 41, 40, 41,
        isSkyblockWorld: true, new LegacyPassRandomState(1456), ref state, commands) ||
      commands.Count != 1 || commands[0].TileType != 2 || commands[0].IsActive != true ||
      !new TileChangeCommitSystem().TryCommit(world, commands, out _) ||
      world.GetTile(40, 40).Type != 2 || !world.GetTile(40, 40).IsActive)
  {
    throw new InvalidOperationException("ErrorWorld random block rewrite diverged from source.");
  }

  Console.WriteLine("PASS: DoErrorWorldShuffleBlocks preserves bounded random block rewrites");
}

static void LegacyErrorWorldShufflePassSourceCheck()
{
  WorldMetadata metadata = new(
    "ErrorWorldPass", new WorldSeed(1456), width: 200, height: 150,
    worldId: 68, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  for (int x = 40; x < 80; x++)
  {
    for (int y = 80; y < 120; y++)
    {
      _ = world.TrySetTile(x, y, new WorldTile(true, 1));
    }
  }

  LegacyErrorWorldTileDefinitionRegistry registry =
    LegacyErrorWorldTileDefinitionRegistry.RegisterDefaults();
  LegacyErrorWorldPassCounts counts = new(1, 0, 0, 0);
  WorldGenerationStateComponent state = new(68);
  List<TileChangeCommand> commands = new();
  LegacyErrorWorldShuffleResult result = LegacyErrorWorldShufflePass.AppendCommands(
    world.CreateSnapshot(metadata),
    registry,
    new LegacyErrorWorldSpawnExclusionProfile(200, 20.0, 130, false),
    counts,
    isSkyblockWorld: false,
    new LegacyPassRandomState(1456),
    ref state,
    commands);
  if (result.RandomBlockRewrites != 1 || !result.CompletedWithoutRejection || commands.Count != 1 ||
      !new TileChangeCommitSystem().TryCommit(world, commands, out _) ||
      !registry.ByType.TryGetValue(world.GetTile(commands[0].X, commands[0].Y).Type,
        out LegacyErrorWorldTileDefinition rewrittenDefinition) ||
      !rewrittenDefinition.IsSolid || rewrittenDefinition.IsSolidTop ||
      rewrittenDefinition.IsFrameImportant || rewrittenDefinition.IsDungeon)
  {
    throw new InvalidOperationException("ErrorWorld pass orchestration diverged from source.");
  }

  Console.WriteLine("PASS: DoErrorWorldShuffleBlocks preserves bounded pass orchestration");
}

static void LegacyErrorWorldFinishCleanupSourceCheck()
{
  WorldMetadata metadata = new("error-world-finish", new WorldSeed(8), 200, 150);
  WorldGrid world = new(200, 150);
  _ = world.TrySetTile(25, 25, new WorldTile(true, 481));
  _ = world.TrySetTile(26, 25, new WorldTile(true, 501));
  _ = world.TrySetTile(27, 25, new WorldTile(true, 137));
  _ = world.TrySetTile(28, 25, new WorldTile(true, 1, WallType: 238));
  _ = world.TrySetTile(29, 35, new WorldTile(true, 1, WallType: 73));
  WorldGenerationStateComponent state = new(88);
  List<TileChangeCommand> commands = new();
  LegacyErrorWorldFinishCleanup.AppendCommands(
    world.CreateSnapshot(metadata),
    worldSurfaceY: 30,
    randomAdjustment: 1,
    new HashSet<ushort>(),
    new LegacyPassRandomState(9),
    ref state,
    commands);
  if (!new TileChangeCommitSystem().TryCommit(world, commands, out _) ||
      !world.GetTile(25, 25).IsInvisibleBlock || !world.GetTile(26, 25).IsInvisibleBlock ||
      world.GetTile(28, 25).WallType != 0 || world.GetTile(29, 35).WallType != 0 ||
      world.GetTile(27, 25).FrameX is < 0 or > 90 || world.GetTile(27, 25).FrameX % 18 != 0)
  {
    throw new InvalidOperationException("ErrorWorld finish cleanup diverged from bounded source rules.");
  }

  Console.WriteLine("PASS: DoErrorWorldFinish preserves bounded tile cleanup commands");
}

static void LegacyErrorWorldChestItemPolicySourceCheck()
{
  LegacyPassRandomState baseline = new(264);
  LegacyPassRandomState balanced = new(264);
  if (LegacyErrorWorldChestItemPolicy.SelectItemType(true, balanced) != -1 ||
      baseline.Next(32) != balanced.Next(32))
  {
    throw new InvalidOperationException(
      "Balanced ErrorWorld chest selection did not preserve the random stream.");
  }

  HashSet<int> allowedItems =
  [
    4008, 238, 2275, 3352, 3262, 3334, 4818, 1325, 4144, 3350, 4347, 1309, 1863,
    485, 748, 1825, 1321, 5451, 3385, 3386, 3387, 3388, 4951, 3043, 2341, 2342,
    2800, 3623, 4980, 4273, 4711, 4420
  ];
  LegacyPassRandomState first = new(264);
  LegacyPassRandomState second = new(264);
  for (int index = 0; index < 64; index++)
  {
    int firstItemType = LegacyErrorWorldChestItemPolicy.SelectItemType(false, first);
    int secondItemType = LegacyErrorWorldChestItemPolicy.SelectItemType(false, second);
    if (firstItemType != secondItemType || !allowedItems.Contains(firstItemType))
    {
      throw new InvalidOperationException("ErrorWorld chest item selection was not deterministic.");
    }
  }

  Console.WriteLine("PASS: DoErrorWorldFindChestItem preserves balanced and random item policy");
}

static void LegacyExtraLiquidAddLiquidSourceCheck()
{
  WorldMetadata metadata = new("extra-liquid", new WorldSeed(12), 200, 150);
  WorldGrid world = new(200, 150);
  _ = world.TrySetTile(50, 70, new WorldTile(false, 0, WallType: 86));
  _ = world.TrySetTile(51, 70, new WorldTile(false, 0, WallType: 187));
  _ = world.TrySetTile(52, 120, new WorldTile(false, 0));
  WorldGenerationStateComponent firstState = new(90);
  WorldGenerationStateComponent secondState = new(90);
  List<LiquidChangeCommand> first = new();
  List<LiquidChangeCommand> second = new();
  LegacyExtraLiquidAddLiquid.AppendCommands(
    world.CreateSnapshot(metadata), 80, 110, isRemixWorld: false, isSkyblockWorld: false,
    new LegacyPassRandomState(64), ref firstState, first);
  LegacyExtraLiquidAddLiquid.AppendCommands(
    world.CreateSnapshot(metadata), 80, 110, isRemixWorld: false, isSkyblockWorld: false,
    new LegacyPassRandomState(64), ref secondState, second);
  if (!first.SequenceEqual(second) ||
      !new LiquidChangeCommitSystem().TryCommit(
        world,
        first,
        new[]
        {
          new LiquidDefinition("water", 0, byte.MaxValue),
          new LiquidDefinition("honey", 2, byte.MaxValue)
        },
        out _) ||
      first.Any(command => command.X is < 40 or >= 160 || command.Y is < 40 or >= 110))
  {
    throw new InvalidOperationException("ExtraLiquid command emission was not bounded and deterministic.");
  }

  LegacyPassRandomState baseline = new(64);
  LegacyPassRandomState skyblock = new(64);
  List<LiquidChangeCommand> skyblockCommands = new();
  WorldGenerationStateComponent skyblockState = new(90);
  LegacyExtraLiquidAddLiquid.AppendCommands(
    world.CreateSnapshot(metadata), 80, 110, isRemixWorld: false, isSkyblockWorld: true,
    skyblock, ref skyblockState, skyblockCommands);
  if (skyblockCommands.Count != 0 || baseline.Next(100) != skyblock.Next(100))
  {
    throw new InvalidOperationException("Skyblock ExtraLiquid branch did not remain inert.");
  }

  Console.WriteLine("PASS: DoExtraLiquidAddLiquid preserves bounded liquid commands");
}

static void LegacyExtraLiquidAddBubbleBlocksSourceCheck()
{
  WorldMetadata metadata = new("extra-liquid-bubbles", new WorldSeed(64), 200, 300);
  WorldGrid world = new(200, 300);
  for (int x = 40; x < 160; x++)
  {
    for (int y = 40; y < 260; y++)
    {
      _ = world.TrySetTile(x, y, new WorldTile(
        IsActive: false,
        Type: 0,
        LiquidAmount: byte.MaxValue,
        WallType: 1));
    }
  }

  List<TileChangeCommand> firstTiles = new();
  List<LiquidChangeCommand> firstLiquids = new();
  List<TileChangeCommand> secondTiles = new();
  List<LiquidChangeCommand> secondLiquids = new();
  WorldGenerationStateComponent firstState = new(100);
  WorldGenerationStateComponent secondState = new(100);
  IReadOnlyList<LegacyExtraLiquidBubbleSquare> first =
    LegacyExtraLiquidAddBubbleBlocks.AppendCommands(
      world.CreateSnapshot(metadata), 60, 120, isRemixWorld: false, isSkyblockWorld: false,
      new LegacyPassRandomState(64), ref firstState, firstTiles, firstLiquids);
  IReadOnlyList<LegacyExtraLiquidBubbleSquare> second =
    LegacyExtraLiquidAddBubbleBlocks.AppendCommands(
      world.CreateSnapshot(metadata), 60, 120, isRemixWorld: false, isSkyblockWorld: false,
      new LegacyPassRandomState(64), ref secondState, secondTiles, secondLiquids);
  if (!first.SequenceEqual(second) ||
      firstTiles.Count != secondTiles.Count ||
      firstLiquids.Count != secondLiquids.Count ||
      firstTiles.Count == 0 ||
      firstTiles.Any(command => !command.IsFullbrightBlock.GetValueOrDefault()) ||
      firstLiquids.Any(command => command.Amount != 0) ||
      LegacyExtraLiquidAddBubbleBlocks.AppendCommands(
        world.CreateSnapshot(metadata), 60, 120, isRemixWorld: false, isSkyblockWorld: true,
        new LegacyPassRandomState(64), ref secondState, secondTiles, secondLiquids).Count != 0)
  {
    throw new InvalidOperationException(
      "ExtraLiquid bubble-block placement was not deterministic or bounded.");
  }

  Console.WriteLine("PASS: DoExtraLiquidAddBubbleBlocks preserves bounded bubble placement");
}

static void LegacyExtraLiquidBubbleExecutionSourceCheck()
{
  WorldMetadata metadata = new("extra-liquid-bubble-execution", new WorldSeed(64), 200, 300);
  WorldGrid world = new(200, 300);
  for (int x = 40; x < 160; x++)
  {
    for (int y = 40; y < 260; y++)
    {
      _ = world.TrySetTile(x, y, new WorldTile(
        IsActive: false,
        Type: 0,
        LiquidAmount: byte.MaxValue,
        WallType: 1));
    }
  }

  WorldGenerationStateComponent state = new(120);
  LegacyExtraLiquidExecutionResult result = LegacyExtraLiquidPassExecution.TryExecuteBubbleBlocks(
    world, metadata, worldSurfaceY: 60, underworldLayerY: 120, isRemixWorld: false,
    isSkyblockWorld: false, new LegacyPassRandomState(64), ref state);
  if (!result.Succeeded || result.BubbleCount == 0 || result.AppliedTileCount == 0 ||
      result.AppliedLiquidCount == 0 || state.NextSequence != result.NextSequence ||
      !world.GetTile(result.Bubbles[0].X - result.Bubbles[0].Radius, result.Bubbles[0].Y).IsActive ||
      world.GetTile(result.Bubbles[0].X, result.Bubbles[0].Y).LiquidAmount != 0)
  {
    throw new InvalidOperationException(
      "ExtraLiquid bubble execution did not atomically commit its typed commands.");
  }

  Console.WriteLine("PASS: DoExtraLiquidAddBubbleBlocks commits typed bubble commands");
}

static void LegacyExtraLiquidFinishSourceCheck()
{
  WorldMetadata metadata = new("extra-liquid-finish", new WorldSeed(12), 200, 150);
  WorldGrid world = new(200, 150);
  _ = world.TrySetTile(50, 100, new WorldTile(
    true, 375, LiquidAmount: byte.MaxValue, LiquidType: 0, IsFullbrightBlock: true));
  _ = world.TrySetTile(51, 110, new WorldTile(true, 1, LiquidAmount: byte.MaxValue));
  _ = world.TrySetTile(52, 110, new WorldTile(true, 56));
  WorldGenerationStateComponent state = new(94);
  List<TileChangeCommand> tileCommands = new();
  List<LiquidChangeCommand> liquidCommands = new();
  LegacyExtraLiquidFinish.AppendCommands(
    world.CreateSnapshot(metadata), 100, isSkyblockWorld: false, ref state, tileCommands,
    liquidCommands);
  IReadOnlyCollection<LiquidDefinition> definitions =
  [new LiquidDefinition("water", 0, byte.MaxValue), new LiquidDefinition("lava", 1, byte.MaxValue)];
  if (!new TileChangeCommitSystem().TryCommit(world, tileCommands, out _) ||
      !new LiquidChangeCommitSystem().TryCommit(world, liquidCommands, definitions, out _) ||
      world.GetTile(50, 100).IsActive || world.GetTile(51, 110).LiquidType != 1 ||
      world.GetTile(52, 110).IsActive)
  {
    throw new InvalidOperationException("ExtraLiquid finish did not commit source-backed cleanup.");
  }

  List<TileChangeCommand> skyblockTiles = new();
  List<LiquidChangeCommand> skyblockLiquids = new();
  WorldGenerationStateComponent skyblockState = new(94);
  LegacyExtraLiquidFinish.AppendCommands(
    world.CreateSnapshot(metadata), 100, isSkyblockWorld: true, ref skyblockState, skyblockTiles,
    skyblockLiquids);
  if (skyblockTiles.Count != 0 || skyblockLiquids.Count != 0)
  {
    throw new InvalidOperationException("Skyblock ExtraLiquid finish branch did not remain inert.");
  }

  Console.WriteLine("PASS: DoExtraLiquidFinish preserves bounded tile and liquid commands");
}

static void LegacyRainsForAYearSourceCheck()
{
  LegacyRainsForAYearResult result = LegacyRainsForAYearPolicy.Apply(new WorldRuleState());
  if (!result.Rules.IsRaining || result.Rules.RainTimeTicks != 1892160000 ||
      result.Rules.RainStrength != 1.0f || result.Rules.MaximumRainStrength != 1.0f ||
      result.CloudCount != 200)
  {
    throw new InvalidOperationException("Rains-for-a-year runtime policy diverged from source.");
  }

  Console.WriteLine("PASS: DoRainsForAYear preserves bounded runtime weather policy");
}

static void LegacyRandomSpawnPolicySourceCheck()
{
  LegacyNoSurfaceActionPlan pending = LegacyRandomSpawnPolicy.Create(false);
  LegacyNoSurfaceActionPlan completed = LegacyRandomSpawnPolicy.Create(true);
  if (!pending.RandomizeSpawn || !pending.PlaceTorchesAroundSpawn ||
      pending.UndergroundMeteorAttempts != 0 || completed != default)
  {
    throw new InvalidOperationException("Random-spawn policy diverged from source guard.");
  }

  Console.WriteLine("PASS: DoRandomSpawn preserves one-time spawn action policy");
}

static void LegacyTeleporterPlacementQuerySourceCheck()
{
  WorldMetadata metadata = new("teleporter", new WorldSeed(12), 200, 150);
  WorldGrid world = new(200, 150);
  for (int x = 99; x <= 101; x++)
  {
    _ = world.TrySetTile(x, 100, new WorldTile(true, 1));
  }

  TileDefinitionRegistry definitions = TileDefinitionRegistry.RegisterDefaults();
  if (!LegacyTeleporterPlacementQuery.CanPlace(
        world.CreateSnapshot(metadata), definitions, new HashSet<ushort>(), new HashSet<ushort>(),
        100, 100, 20, 20, moreForcefulPlacement: false))
  {
    throw new InvalidOperationException("Teleporter placement rejected a valid source footprint.");
  }

  _ = world.TrySetTile(120, 99, new WorldTile(true, 235));
  if (LegacyTeleporterPlacementQuery.CanPlace(
        world.CreateSnapshot(metadata), definitions, new HashSet<ushort>(), new HashSet<ushort>(),
        100, 100, 20, 20, moreForcefulPlacement: false))
  {
    throw new InvalidOperationException("Teleporter placement ignored the source proximity guard.");
  }

  Console.WriteLine("PASS: DoAddTeleporters preserves bounded candidate eligibility");
}

static void LegacyTeleporterClearAreaSourceCheck()
{
  WorldMetadata metadata = new("teleporter-clear", new WorldSeed(12), 200, 150);
  WorldGrid world = new(200, 150);
  _ = world.TrySetTile(99, 100, new WorldTile(true, 1));
  _ = world.TrySetTile(100, 100, new WorldTile(true, 2));
  _ = world.TrySetTile(101, 100, new WorldTile(true, 3));
  _ = world.TrySetTile(99, 101, new WorldTile(false, 0));
  _ = world.TrySetTile(100, 101, new WorldTile(true, 4, IsHalfBrick: true, Slope: 1));
  _ = world.TrySetTile(101, 101, new WorldTile(true, 4));
  WorldGenerationStateComponent state = new(96);
  List<TileChangeCommand> commands = new();
  if (!LegacyTeleporterClearArea.TryAppendCommands(
        world.CreateSnapshot(metadata), 100, 100, new HashSet<ushort> { 1, 2 }, ref state,
        commands) ||
      !new TileChangeCommitSystem().TryCommit(world, commands, out _) ||
      world.GetTile(99, 100).IsActive || world.GetTile(100, 100).IsActive ||
      !world.GetTile(101, 100).IsActive || !world.GetTile(99, 101).IsActive ||
      world.GetTile(99, 101).Type != 4 || world.GetTile(100, 101).Slope != 0 ||
      world.GetTile(100, 101).IsHalfBrick)
  {
    throw new InvalidOperationException("Teleporter clear-area commands diverged from source.");
  }

  Console.WriteLine("PASS: DoAddTeleporters preserves bounded clear-area commands");
}

static void LegacyStartInHardmodePolicySourceCheck()
{
  LegacyStartInHardmodeResult result = LegacyStartInHardmodePolicy.Apply(
    new WorldRuleSnapshotComponent(0, "start-in-hardmode", false));
  if (!result.Rules.IsHardmode || result.Rules.Difficulty != 0 ||
      result.Rules.SecretSeedVariant != "start-in-hardmode" || !result.InitializeHardmode)
  {
    throw new InvalidOperationException("Start-in-hardmode policy diverged from source.");
  }

  Console.WriteLine("PASS: DoStartInHardmode preserves bounded bootstrap policy");
}

static void LegacyDigExtraHolesSourceCheck()
{
  IReadOnlyList<LegacyTileRunnerPassInvocation> first = LegacyDigExtraHoles.CreateInvocations(
    200, 150, new LegacyPassRandomState(1456));
  IReadOnlyList<LegacyTileRunnerPassInvocation> second = LegacyDigExtraHoles.CreateInvocations(
    200, 150, new LegacyPassRandomState(1456));
  if (first.Count != 20 || !first.SequenceEqual(second) ||
      first.Any(invocation => invocation.Request.TileType != -1 ||
        invocation.Request.AddTile || !invocation.Request.NoYChange ||
        invocation.Request.X is < 50 or >= 150 || invocation.Request.Y is < 50 or >= 100 ||
        invocation.Request.Strength is < 5 or >= 30 ||
        invocation.Request.Steps is < 30 or >= 201 ||
        invocation.RandomDrawCount is < 7 or > 8))
  {
    throw new InvalidOperationException("DoDigExtraHoles invocation batch diverged from source.");
  }

  Console.WriteLine("PASS: DoDigExtraHoles preserves deterministic TileRunner invocations");
}

static void LegacyRoundLandmassSeedDefinitionsSourceCheck()
{
  IReadOnlyList<LegacyRoundLandmassSeedDefinition> first =
    LegacyRoundLandmassSeedDefinitions.Create(800, 300, new LegacyPassRandomState(1456));
  IReadOnlyList<LegacyRoundLandmassSeedDefinition> second =
    LegacyRoundLandmassSeedDefinitions.Create(800, 300, new LegacyPassRandomState(1456));
  if (first.Count != 3 || !first.SequenceEqual(second) ||
      first[0].X is < 115 or >= 135 || first[1].X is < 665 or >= 685 ||
      first[2].X is < 350 or > 450 || first[2].Radius is < 100 or >= 201 ||
      first.Take(2).Any(definition => definition.Radius is < 235 or >= 266))
  {
    throw new InvalidOperationException("DoRoundLandMasses seed definitions diverged from source.");
  }

  Console.WriteLine("PASS: DoRoundLandMasses preserves deterministic seed definitions");
}

static void LegacyPortalGunAndPooPolicySourceCheck()
{
  LegacyFrozenChestItemIntent intent = default;
  bool created = false;
  for (int seed = 1; seed <= 64 && !created; seed++)
  {
    created = LegacyPortalGunChestPolicy.TryCreateIntent(
      [4, 2, 0, 0], new LegacyPassRandomState(seed), out intent);
  }

  if (!created || intent != new LegacyFrozenChestItemIntent(2, 3384) ||
      LegacyPooEverywherePolicy.GetAttemptCount(1000, 500, 0) != 100 ||
      LegacyPooEverywherePolicy.GetAttemptCount(1000, 500, 4) != 50 ||
      LegacyPooEverywherePolicy.GetAttemptCount(1000, 500, 7) != 33)
  {
    throw new InvalidOperationException("Portal-gun chest or poo attempt policy diverged from source.");
  }

  Console.WriteLine("PASS: portal-gun chest and poo attempt policies preserve source rules");
}

static void LegacyOreRunnerSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyOreRunner", new WorldSeed(1456), width: 200, height: 150,
    worldId: 36, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  for (int x = 90; x < 110; x++)
  {
    for (int y = 65; y < 85; y++)
    {
      _ = world.TrySetTile(x, y, new WorldTile(true, 1, WallType: 2));
    }
  }

  _ = world.TrySetTile(100, 75, new WorldTile(true, 225, WallType: 108));
  _ = world.TrySetTile(101, 75, new WorldTile(true, 179, WallType: 2));
  _ = world.TrySetTile(102, 75, new WorldTile(true, 184, WallType: 2));
  LegacyOreRunnerRequest request = new(
    100, 75, 8, 3, TileType: 666, WallType: 3,
    StayInArea: new LegacyOreRunnerArea(90, 65, 20, 20), OnlyReplaceTileType: 1,
    OnlyReplaceWallType: 2);
  WorldGenerationStateComponent firstState = new(36);
  List<TileChangeCommand> first = new();
  LegacyOreRunner.AppendCommands(
    world.CreateSnapshot(metadata), request, new LegacyPassRandomState(1456), ref firstState, first);
  WorldGenerationStateComponent secondState = new(36);
  List<TileChangeCommand> second = new();
  LegacyOreRunner.AppendCommands(
    world.CreateSnapshot(metadata), request, new LegacyPassRandomState(1456), ref secondState, second);
  if (first.Count == 0 || !first.SequenceEqual(second) ||
      first.Any(command => command.X is < 90 or >= 110 || command.Y is < 65 or >= 85) ||
      first.Any(command => command.X == 100 && command.Y == 75) ||
      !first.Any(command => command.Kind == TileChangeKind.UpdateTileType && command.TileType == 666) ||
      !first.Any(command => command.Kind == TileChangeKind.SetWall && command.WallType == 3))
  {
    throw new InvalidOperationException("OreRunner command rendering diverged from source bounds.");
  }

  WorldGenerationStateComponent mossState = new(36);
  List<TileChangeCommand> mossCommands = new();
  LegacyOreRunner.AppendCommands(
    world.CreateSnapshot(metadata),
    new LegacyOreRunnerRequest(101, 75, 8, 1, TileType: 666),
    new LegacyPassRandomState(1456),
    ref mossState,
    mossCommands);
  if (!mossCommands.Any(command => command.X == 101 && command.Y == 75 &&
        command.Kind == TileChangeKind.UpdateTileType && command.TileType == 666) ||
      mossCommands.Any(command => command.X == 102 && command.Y == 75 &&
        command.Kind == TileChangeKind.UpdateTileType))
  {
    throw new InvalidOperationException("OreRunner moss registry diverged from source entries.");
  }

  WorldGridSnapshot topSnapshot = world.CreateSnapshot(metadata);
  LegacyOreRunnerRequest surfaceRequest = new(
    100, 0, 8, 3, TileType: 59, WallType: -1);
  WorldGenerationStateComponent surfaceState = new(36);
  List<TileChangeCommand> surfaceCommands = new();
  LegacyOreRunner.AppendCommands(
    topSnapshot, surfaceRequest, new LegacyPassRandomState(1456), ref surfaceState, surfaceCommands);
  if (surfaceCommands.Count != 0)
  {
    throw new InvalidOperationException("OreRunner did not preserve the type-59 top early-stop.");
  }

  Console.WriteLine("PASS: OreRunner preserves deterministic bounded ore commands");
}

static void LegacyOreHelperSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyOreHelper", new WorldSeed(1456), width: 200, height: 150,
    worldId: 37, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(4, 4, new WorldTile(true, 1));
  _ = world.TrySetTile(3, 5, new WorldTile(true, 40));
  _ = world.TrySetTile(5, 3, new WorldTile(true, 2));
  WorldGenerationStateComponent state = new(37);
  List<TileChangeCommand> commands = new();
  LegacyOreHelper.AppendCommands(
    world.CreateSnapshot(metadata), 4, 4, ref state, commands);

  if (commands.Count != 2 ||
      !commands.Any(command => command.X == 4 && command.Y == 4 &&
        command.Kind == TileChangeKind.UpdateTileType && command.TileType == 0 &&
        command.Source == "worldgen.ore.OreHelper") ||
      !commands.Any(command => command.X == 3 && command.Y == 5 &&
        command.Kind == TileChangeKind.UpdateTileType && command.TileType == 0) ||
      commands.Any(command => command.X == 5 && command.Y == 3) ||
      !commands.Select(command => command.Sequence).SequenceEqual(new long[] { 0, 1 }))
  {
    throw new InvalidOperationException("OreHelper command rendering diverged from source.");
  }

  AssertThrows<ArgumentOutOfRangeException>(() =>
  {
    WorldGenerationStateComponent edgeState = new(37);
    LegacyOreHelper.AppendCommands(
      world.CreateSnapshot(metadata), 0, 4, ref edgeState, new List<TileChangeCommand>());
  });

  Console.WriteLine("PASS: OreHelper preserves bounded three-by-three ore cleanup");
}

static void LegacyOrePatchTypePolicySourceCheck()
{
  bool selectedIron = false;
  bool selectedCopper = false;
  for (int seed = 1; seed <= 32; seed++)
  {
    ushort selected = LegacyOrePatchTypePolicy.SelectTileType(
      copperTileType: 7,
      ironTileType: 6,
      new LegacyPassRandomState(seed));
    selectedIron |= selected == 6;
    selectedCopper |= selected == 7;
  }

  if (!selectedIron || !selectedCopper)
  {
    throw new InvalidOperationException("OrePatch SavedOreTiers selection did not preserve source branches.");
  }

  AssertThrows<ArgumentNullException>(() =>
    LegacyOrePatchTypePolicy.SelectTileType(7, 6, null!));

  WorldMetadata metadata = new(
    "LegacyOrePatchPolicy", new WorldSeed(1456), width: 200, height: 150,
    worldId: 38, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  OrePatchPlacementSystem system = new();
  OreDefinition definition = new("surface-ore", 999, 10, 40, 0, 3);
  IReadOnlyDictionary<ushort, OrePatchTileDefinition> definitions =
    new Dictionary<ushort, OrePatchTileDefinition>
    {
      [1] = new OrePatchTileDefinition(1, true, true, false, false, false)
    };
  OrePatchPlacementPreparation preparation;
  bool prepared = system.TryPrepare(
    world.CreateSnapshot(metadata), 100, 20, 80, definitions, definition,
    copperTileType: 7, ironTileType: 6, new LegacyPassRandomState(1),
    new TileProtectionComponent(0, 0, -1, -1), out preparation);
  if (prepared && preparation.Transaction.TileType is not 6 and not 7)
  {
    throw new InvalidOperationException("OrePatch policy integration selected an invalid tile type.");
  }

  Console.WriteLine("PASS: OrePatch preserves SavedOreTiers copper and iron selection");
}

static void LegacyOreTierSelectionPolicySourceCheck()
{
  bool foundAlternativeCopper = false;
  bool foundAlternativeIron = false;
  bool foundAlternativeSilver = false;
  bool foundAlternativeGold = false;
  for (int seed = 1; seed <= 128; seed++)
  {
    LegacyOreTierSelection first = LegacyOreTierSelectionPolicy.Select(
      false, false, new LegacyPassRandomState(seed));
    LegacyOreTierSelection second = LegacyOreTierSelectionPolicy.Select(
      false, false, new LegacyPassRandomState(seed));
    LegacyOreTierSelection dontStarve = LegacyOreTierSelectionPolicy.Select(
      true, false, new LegacyPassRandomState(seed));
    if (first != second || dontStarve.IronTileType != 6 || dontStarve.GoldTileType != 8)
    {
      throw new InvalidOperationException("SavedOreTiers random gating diverged from source.");
    }

    foundAlternativeCopper |= first.CopperTileType == 166;
    foundAlternativeIron |= first.IronTileType == 167;
    foundAlternativeSilver |= first.SilverTileType == 168;
    foundAlternativeGold |= first.GoldTileType == 169;
  }

  LegacyOreTierSelection drunkDontStarve = LegacyOreTierSelectionPolicy.Select(
    true, true, new LegacyPassRandomState(4));
  if (!foundAlternativeCopper || !foundAlternativeIron || !foundAlternativeSilver ||
      !foundAlternativeGold ||
      drunkDontStarve.IronTileType is not 6 and not 167 ||
      drunkDontStarve.GoldTileType is not 8 and not 169)
  {
    throw new InvalidOperationException("SavedOreTiers did not preserve tier selection values.");
  }

  AssertThrows<ArgumentNullException>(() =>
    LegacyOreTierSelectionPolicy.Select(false, false, null!));
  Console.WriteLine("PASS: SavedOreTiers preserves deterministic world-rule tier selection");
}

static void LegacyOrePatchTrailSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyOrePatchTrail", new WorldSeed(1456), width: 200, height: 150,
    worldId: 39, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  for (int x = 80; x < 120; x++)
  {
    for (int y = 60; y < 120; y++)
    {
      _ = world.TrySetTile(x, y, new WorldTile(true, 1));
    }
  }

  WorldGenerationStateComponent firstState = new(39);
  List<TileChangeCommand> first = new();
  bool firstAppended = LegacyOrePatchTrail.TryAppendCommands(
    world.CreateSnapshot(metadata), 100, 75, 7, new LegacyPassRandomState(1456),
    ref firstState, first);
  WorldGenerationStateComponent secondState = new(39);
  List<TileChangeCommand> second = new();
  bool secondAppended = LegacyOrePatchTrail.TryAppendCommands(
    world.CreateSnapshot(metadata), 100, 75, 7, new LegacyPassRandomState(1456),
    ref secondState, second);
  if (!firstAppended || !secondAppended || !first.SequenceEqual(second) ||
      first.Count == 0 || !first.Any(command => command.Source == "worldgen.ore.OrePatch.trail" &&
        command.TileType == 7) ||
      first.Where(command => command.Source == "worldgen.ore.OrePatch.trail").Any(command =>
        command.X is < 80 or >= 120 || command.Y is < 60 or >= 120))
  {
    throw new InvalidOperationException("OrePatch initial trail diverged from source traversal.");
  }

  if (!new TileChangeCommitSystem().TryCommit(world, first, out _) ||
      first.Where(command => command.Source == "worldgen.ore.OrePatch.trail").Any(command =>
        world.GetTile(command.X, command.Y).Type != 7))
  {
    throw new InvalidOperationException("OrePatch trail cleanup overwrote a placed ore tile.");
  }

  WorldGenerationStateComponent edgeState = new(39);
  List<TileChangeCommand> edgeCommands = new();
  if (LegacyOrePatchTrail.TryAppendCommands(
        world.CreateSnapshot(metadata), 0, 0, 7, new LegacyPassRandomState(1456),
        ref edgeState, edgeCommands) || edgeCommands.Count != 0)
  {
    throw new InvalidOperationException("OrePatch trail did not reject an out-of-bounds batch.");
  }

  Console.WriteLine("PASS: OrePatch preserves deterministic initial ore trail commands");
}

static void LegacyOrePatchBlobSourceCheck()
{
  WorldMetadata metadata = new(
    "LegacyOrePatchBlob", new WorldSeed(1456), width: 200, height: 150,
    worldId: 40, seedVariant: "default");
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  for (int x = 40; x < 160; x++)
  {
    for (int y = 30; y < 130; y++)
    {
      _ = world.TrySetTile(x, y, new WorldTile(true, 1));
    }
  }

  WorldGenerationStateComponent firstState = new(40);
  List<TileChangeCommand> first = new();
  bool firstAppended = LegacyOrePatchBlob.TryAppendCommands(
    world.CreateSnapshot(metadata), 100, 75, 7, new LegacyPassRandomState(1456),
    ref firstState, first);
  WorldGenerationStateComponent secondState = new(40);
  List<TileChangeCommand> second = new();
  bool secondAppended = LegacyOrePatchBlob.TryAppendCommands(
    world.CreateSnapshot(metadata), 100, 75, 7, new LegacyPassRandomState(1456),
    ref secondState, second);
  if (!firstAppended || !secondAppended || first.Count == 0 || !first.SequenceEqual(second) ||
      first.Any(command => command.Source != "worldgen.ore.OrePatch.blob") ||
      !first.Any(command => command.TileType == 7) ||
      !first.Any(command => command.IsActive == false))
  {
    throw new InvalidOperationException("OrePatch blob commands diverged from source traversal.");
  }

  if (!new TileChangeCommitSystem().TryCommit(world, first, out _) ||
      !first.Where(command => command.TileType == 7).Any(command =>
        world.GetTile(command.X, command.Y).Type == 7))
  {
    throw new InvalidOperationException("OrePatch blob commands did not commit ore mutations.");
  }

  WorldGenerationStateComponent edgeState = new(40);
  List<TileChangeCommand> edgeCommands = new();
  if (LegacyOrePatchBlob.TryAppendCommands(
        world.CreateSnapshot(metadata), 1, 1, 7, new LegacyPassRandomState(1456),
        ref edgeState, edgeCommands) || edgeCommands.Count != 0)
  {
    throw new InvalidOperationException("OrePatch blob did not reject an out-of-bounds batch.");
  }

  Console.WriteLine("PASS: OrePatch preserves deterministic blob and inactive carve commands");
}

static Dictionary<string, int> CreateExtendedStateMismatchFieldCounts()
{
  return new Dictionary<string, int>(StringComparer.Ordinal)
  {
    ["Actuated"] = 0,
    ["FullbrightBlock"] = 0,
    ["FullbrightWall"] = 0,
    ["HalfBrick"] = 0,
    ["Inactive"] = 0,
    ["InvisibleBlock"] = 0,
    ["InvisibleWall"] = 0,
    ["Slope"] = 0,
    ["TileColor"] = 0,
    ["WallColor"] = 0,
    ["WallType"] = 0,
    ["Wire"] = 0,
    ["Wire2"] = 0,
    ["Wire3"] = 0,
    ["Wire4"] = 0
  };
}

static void IncrementExtendedStateMismatch(
  bool isMismatch,
  string fieldName,
  Dictionary<string, int> extendedStateMismatchFieldCounts,
  ref bool hasMismatch)
{
  if (!isMismatch)
  {
    return;
  }

  extendedStateMismatchFieldCounts[fieldName]++;
  hasMismatch = true;
}

static string CreateWorldGridFingerprint(WorldGrid world)
{
  using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
  AppendInt32(hash, world.Width);
  AppendInt32(hash, world.Height);
  Span<byte> tileBuffer = stackalloc byte[16];
  for (int x = 0; x < world.Width; x++)
  {
    for (int y = 0; y < world.Height; y++)
    {
      WorldTile tile = world.GetTile(x, y);
      int offset = 0;
      tileBuffer[offset++] = tile.IsActive ? (byte)1 : (byte)0;
      BinaryPrimitives.WriteUInt16LittleEndian(tileBuffer[offset..], tile.Type);
      offset += sizeof(ushort);
      tileBuffer[offset++] = tile.LiquidAmount;
      tileBuffer[offset++] = tile.LiquidType;
      BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameX);
      offset += sizeof(short);
      BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameY);
      offset += sizeof(short);
      hash.AppendData(tileBuffer[..offset]);
    }
  }

  return Convert.ToHexString(hash.GetHashAndReset());
}

static string CreateSnapshotWorldGridFingerprint(WorldGridSnapshot snapshot)
{
  using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
  AppendInt32(hash, snapshot.Metadata.Width);
  AppendInt32(hash, snapshot.Metadata.Height);
  Span<byte> tileBuffer = stackalloc byte[9];
  for (int x = 0; x < snapshot.Metadata.Width; x++)
  {
    for (int y = 0; y < snapshot.Metadata.Height; y++)
    {
      WorldTile tile = snapshot.GetTile(x, y);
      int offset = 0;
      tileBuffer[offset++] = tile.IsActive ? (byte)1 : (byte)0;
      BinaryPrimitives.WriteUInt16LittleEndian(tileBuffer[offset..], tile.Type);
      offset += sizeof(ushort);
      tileBuffer[offset++] = tile.LiquidAmount;
      tileBuffer[offset++] = tile.LiquidType;
      BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameX);
      offset += sizeof(short);
      BinaryPrimitives.WriteInt16LittleEndian(tileBuffer[offset..], tile.FrameY);
      offset += sizeof(short);
      hash.AppendData(tileBuffer[..offset]);
    }
  }

  return Convert.ToHexString(hash.GetHashAndReset());
}

static void AppendInt32(IncrementalHash hash, int value)
{
  Span<byte> buffer = stackalloc byte[sizeof(int)];
  BinaryPrimitives.WriteInt32LittleEndian(buffer, value);
  hash.AppendData(buffer);
}

static void AppendText(IncrementalHash hash, object? value)
{
  byte[] bytes = Encoding.UTF8.GetBytes(value?.ToString() ?? string.Empty);
  hash.AppendData(bytes);
}

static Dictionary<string, long> GetSectionVersions(WorldGridSnapshot snapshot)
{
  Dictionary<string, long> versions = new(StringComparer.Ordinal);
  for (int y = 0; y < snapshot.Metadata.Height / WorldGrid.SectionHeight; y++)
  {
    for (int x = 0; x < snapshot.Metadata.Width / WorldGrid.SectionWidth; x++)
    {
      WorldSectionCoordinates coordinates = new(x, y);
      versions[$"{x},{y}"] = snapshot.GetSectionVersion(coordinates);
    }
  }

  return versions;
}

static string CreateMethodMap(WorldgenInventory inventory)
{
  StringBuilder builder = new();
  builder.AppendLine("# WorldGen method map");
  builder.AppendLine();
  builder.AppendLine($"- Source: `{inventory.SourcePath}`");
  builder.AppendLine($"- SHA-256: `{inventory.Source.Sha256}`");
  builder.AppendLine();
  builder.AppendLine(
    "Entries remain `Unmapped` unless a source-line mapping records target scope and exclusions.");
  builder.AppendLine();
  builder.AppendLine("| Line | Visibility | Name | Candidate domain | Status | References |");
  builder.AppendLine("| ---: | --- | --- | --- | --- | --- |");
  foreach (MethodInventory method in inventory.Methods)
  {
    builder.AppendLine(
      $"| {method.Line} | {method.Visibility} | `{method.Name}` | {method.Domain} | " +
      $"{method.Status} | {string.Join(", ", method.References)} |");
  }

  return builder.ToString();
}

static void AssertThrows<TException>(Action action)
  where TException : Exception
{
  try
  {
    action();
  }
  catch (TException)
  {
    return;
  }

  throw new InvalidOperationException(
    $"Expected {typeof(TException).Name} was not thrown.");
}

static void RunRocksInDirtFocusedVerification()
{
  const int width = 200;
  const int height = 150;
  const int seed = 1456;
  IReadOnlyList<LegacyTileRunnerPassInput> recipes =
    LegacyRocksInDirtPassDefinition.CreateDefaultRecipes();
  IReadOnlyList<LegacyTileRunnerPassLoopDefinition> loops =
    LegacyRocksInDirtPassDefinition.CreateDefaultLoops();
  if (recipes.Count != 3 || loops.Count != 3 ||
      recipes[0] != new LegacyTileRunnerPassInput(
        "RocksInDirt", "surface-dirt", 1, false, 4, 15, 5, 40,
        "0..worldSurfaceLow", true, true, true) ||
      recipes[1] != new LegacyTileRunnerPassInput(
        "RocksInDirt", "surface-high-dirt", 1, false, 4, 10, 5, 30,
        "worldSurfaceLow..worldSurfaceHigh", true, true, true) ||
      recipes[2] != new LegacyTileRunnerPassInput(
        "RocksInDirt", "rock-high-dirt", 1, false, 2, 7, 2, 23,
        "worldSurfaceHigh..rockLayerHigh", true, true, true) ||
      loops[0].TileDensity != 0.00015 || loops[1].TileDensity != 0.0002 ||
      loops[2].TileDensity != 0.0045 ||
      loops[0].CalculateInvocationCount(width, height, remixWorld: false) != 5 ||
      loops[1].CalculateInvocationCount(width, height, remixWorld: false) != 6 ||
      loops[2].CalculateInvocationCount(width, height, remixWorld: false) != 135 ||
      LegacyRocksInDirtPassDefinition.CalculateInvocationCount(4200, 1200, 0.00015) != 756)
  {
    throw new InvalidOperationException(
      "RocksInDirt recipes or density cardinality drifted from the source pass.");
  }

  try
  {
    ((IList<LegacyTileRunnerPassInput>)recipes)[0] = recipes[0];
    throw new InvalidOperationException("RocksInDirt recipes projection was mutable.");
  }
  catch (NotSupportedException)
  {
  }

  WorldMetadata metadata = new(
    "rocks-in-dirt-focused",
    new WorldSeed(seed),
    width,
    height,
    isRemixWorld: false);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  string inputFingerprint = CreateSnapshotWorldGridFingerprint(snapshot);
  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 40.0,
    RockLayer: 80.0,
    WorldSurfaceLow: 20.0,
    WorldSurfaceHigh: 40.0,
    RockLayerLow: 70.0,
    RockLayerHigh: 90.0,
    LeftBeachEnd: 20,
    RightBeachStart: 180,
    WaterLine: 100,
    LavaLine: 120);

  LegacyTileRunnerPassInput secondRecipe = recipes[1];
  MethodInfo createInvocation = typeof(LegacyRocksInDirtPass).GetMethod(
    "CreateInvocation",
    BindingFlags.NonPublic | BindingFlags.Static) ??
    throw new InvalidOperationException("RocksInDirt invocation owner was not found.");
  LegacyPassRandomState invocationRandom = new(seed);
  LegacyTileRunnerPassInvocation invocation =
    (LegacyTileRunnerPassInvocation)(createInvocation.Invoke(
      null,
      [snapshot, secondRecipe, invocationRandom, 20, 41, true,
        new Dictionary<(int X, int Y), WorldTile>()]) ??
      throw new InvalidOperationException("RocksInDirt invocation was not created."));
  LegacyPassRandomState expectedRandom = new(seed);
  int expectedX = expectedRandom.Next(0, width);
  int initialY = expectedRandom.Next(20, 41);
  int expectedY = initialY;
  if (!snapshot.GetTile(expectedX, initialY - 10).IsActive)
  {
    expectedY = expectedRandom.Next(20, 41);
  }

  int expectedStrength = expectedRandom.Next(4, 10);
  int expectedSteps = expectedRandom.Next(5, 30);
  if (invocation.XDraw != expectedX || invocation.YDraw != expectedY ||
      invocation.StrengthDraw != expectedStrength || invocation.StepsDraw != expectedSteps ||
      invocation.RandomDrawCount != 5)
  {
    throw new InvalidOperationException(
      "RocksInDirt second-family inactive-offset reroll did not preserve draw order.");
  }

  LegacyTileRunnerPassInvocation activeOffsetInvocation =
    (LegacyTileRunnerPassInvocation)(createInvocation.Invoke(
      null,
      [CreateActiveOffsetSnapshot(metadata, expectedX, initialY - 10), secondRecipe,
        new LegacyPassRandomState(seed), 20, 41, true,
        new Dictionary<(int X, int Y), WorldTile>()]) ??
      throw new InvalidOperationException("RocksInDirt active-offset invocation was not created."));
  if (activeOffsetInvocation.YDraw != initialY || activeOffsetInvocation.RandomDrawCount != 4)
  {
    throw new InvalidOperationException(
      "RocksInDirt second-family active-offset branch consumed an unexpected reroll.");
  }

  LegacyTileRunnerPassInvocation firstInvocation =
    (LegacyTileRunnerPassInvocation)(createInvocation.Invoke(
      null,
      [snapshot, recipes[0], new LegacyPassRandomState(seed), 0, 21, false,
        new Dictionary<(int X, int Y), WorldTile>()]) ??
      throw new InvalidOperationException("RocksInDirt first-family invocation was not created."));
  if (firstInvocation.RandomDrawCount != 4 || firstInvocation.YDraw is < 0 or >= 21)
  {
    throw new InvalidOperationException(
      "RocksInDirt first-family range or draw count drifted from source.");
  }

  LegacyTileRunnerPassInvocation thirdInvocation =
    (LegacyTileRunnerPassInvocation)(createInvocation.Invoke(
      null,
      [snapshot, recipes[2], new LegacyPassRandomState(seed), 40, 91, false,
        new Dictionary<(int X, int Y), WorldTile>()]) ??
      throw new InvalidOperationException("RocksInDirt third-family invocation was not created."));
  if (thirdInvocation.RandomDrawCount != 4 || thirdInvocation.YDraw is < 40 or >= 91)
  {
    throw new InvalidOperationException(
      "RocksInDirt third-family range or draw count drifted from source.");
  }

  WorldGenerationStateComponent firstState = new(7);
  List<TileChangeCommand> firstCommands = new();
  LegacyRocksInDirtPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isSkyblockWorld: false,
    ref firstState,
    firstCommands);
  if (firstCommands.Count == 0 || firstState.Stage != WorldGenerationStage.Cave ||
      !StringComparer.Ordinal.Equals(
        inputFingerprint,
        CreateSnapshotWorldGridFingerprint(snapshot)) ||
      firstCommands.Any(command =>
        !metadata.IsInside(command.X, command.Y) || command.Sequence < 0 ||
        command.Source is not (
          "worldgen.cave.RocksInDirt.surface-dirt" or
          "worldgen.cave.RocksInDirt.surface-high-dirt" or
          "worldgen.cave.RocksInDirt.rock-high-dirt")))
  {
    throw new InvalidOperationException(
      "RocksInDirt did not emit bounded source-attributed commands from its snapshot.");
  }

  WorldGenerationStateComponent secondState = new(7);
  List<TileChangeCommand> secondCommands = new();
  LegacyRocksInDirtPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isSkyblockWorld: false,
    ref secondState,
    secondCommands);
  if (!firstCommands.SequenceEqual(secondCommands) ||
      firstState.Stage != secondState.Stage || firstState.NextSequence != secondState.NextSequence)
  {
    throw new InvalidOperationException(
      "RocksInDirt command emission was not deterministic from an immutable snapshot.");
  }

  WorldGrid commitWorld = WorldGrid.FromSnapshot(snapshot);
  if (!new TileChangeCommitSystem().TryCommit(
        commitWorld,
        firstCommands,
        out TileChangeCommitResult commitResult) ||
      commitResult.AppliedCount != firstCommands.Count ||
      StringComparer.Ordinal.Equals(
        inputFingerprint,
        CreateSnapshotWorldGridFingerprint(commitWorld.CreateSnapshot(metadata))))
  {
    throw new InvalidOperationException(
      "RocksInDirt commands did not remain isolated until deterministic commit.");
  }

  LegacyTileRunnerPassCommandBatch batch = new(
    "RocksInDirt",
    firstCommands,
    Array.Empty<LiquidChangeCommand>(),
    firstState.NextSequence);
  if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
        WorldGrid.FromSnapshot(snapshot),
        batch,
        Array.Empty<LiquidDefinition>(),
        out LegacyTileRunnerCommandCommitResult batchResult) ||
      !batchResult.Succeeded || batchResult.AppliedTileCount != firstCommands.Count)
  {
    throw new InvalidOperationException(
      "RocksInDirt typed command batch did not commit atomically.");
  }

  WorldGenerationStateComponent skyblockState = new(7);
  List<TileChangeCommand> skyblockCommands = new();
  LegacyRocksInDirtPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isSkyblockWorld: true,
    ref skyblockState,
    skyblockCommands);
  if (skyblockCommands.Count != 0 || skyblockState.Stage != WorldGenerationStage.Created ||
      skyblockState.NextSequence != 0)
  {
    throw new InvalidOperationException(
      "RocksInDirt did not honor the Skyblock deny-generation guard.");
  }

  Console.WriteLine(
    $"PASS: RocksInDirt source recipes, reroll, deterministic commands, and atomic commit " +
    $"({firstCommands.Count} commands; source line {LegacyRocksInDirtPass.SourceLine})");
  Console.WriteLine(
    "SUMMARY: RocksInDirt focused verification completed; TileRunner traversal, aggregate cave " +
    "ordering, full WLD differential, and legacy deletion remain deferred");
}

static void RunClayFocusedVerification()
{
  const int seed = 1456;
  const int width = 200;
  const int height = 300;
  LegacyClayPassDefinition.Validate();
  IReadOnlyList<LegacyTileRunnerPassInput> recipes =
    LegacyClayPassDefinition.CreateDefaultRecipes();
  IReadOnlyList<LegacyTileRunnerPassLoopDefinition> loops =
    LegacyClayPassDefinition.CreateDefaultLoops();
  if (recipes.Count != 4 || loops.Count != 4 ||
      recipes[0] != new LegacyTileRunnerPassInput(
        "Clay", "surface-low-clay", 40, false, 4, 14, 10, 50,
        "0..worldSurfaceLow", true, true, true) ||
      recipes[1] != new LegacyTileRunnerPassInput(
        "Clay", "remix-clay", 40, false, 8, 15, 5, 50,
        "rockLayer-25..maxTilesY-350", true, true, true) ||
      recipes[2] != new LegacyTileRunnerPassInput(
        "Clay", "surface-high-clay", 40, false, 8, 14, 15, 45,
        "worldSurfaceLow..worldSurfaceHigh+1", true, true, true) ||
      recipes[3] != new LegacyTileRunnerPassInput(
        "Clay", "rock-high-clay", 40, false, 8, 15, 5, 50,
        "worldSurfaceHigh..rockLayerHigh+1", true, true, true) ||
      loops[0] != new LegacyTileRunnerPassLoopDefinition(
        "Clay", "surface-low-clay", 2E-05, 1.0) ||
      loops[1] != new LegacyTileRunnerPassLoopDefinition(
        "Clay", "remix-clay", 7E-05, 1.0) ||
      loops[2] != new LegacyTileRunnerPassLoopDefinition(
        "Clay", "surface-high-clay", 5E-05, 1.0) ||
      loops[3] != new LegacyTileRunnerPassLoopDefinition(
        "Clay", "rock-high-clay", 2E-05, 1.0) ||
      LegacyClayPassDefinition.CalculateInvocationCount(4200, 1200, 2E-05) != 100 ||
      LegacyClayPassDefinition.CalculateInvocationCount(4200, 1200, 7E-05) != 352 ||
      LegacyClayPassDefinition.CalculateInvocationCount(4200, 1200, 5E-05) != 252 ||
      LegacyClayPassDefinition.CalculateInvocationCount(4200, 1200, 2E-05) != 100 ||
      LegacyClayPassDefinition.CalculateInvocationCount(200, 150, 2E-05) != 0)
  {
    throw new InvalidOperationException(
      "Clay recipes, loop densities, or floor-based cardinality drifted from the source.");
  }

  WorldMetadata metadata = new(
    "clay-focused",
    new WorldSeed(seed),
    width,
    height,
    worldId: seed,
    spawnX: width / 2,
    spawnY: 20,
    worldSurface: 65,
    rockLayer: 120,
    isRemixWorld: false);
  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 65.5,
    RockLayer: 120.5,
    WorldSurfaceLow: 30.75,
    WorldSurfaceHigh: 80.25,
    RockLayerLow: 100.5,
    RockLayerHigh: 140.75,
    LeftBeachEnd: 20,
    RightBeachStart: 180,
    WaterLine: 200,
    LavaLine: 240);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  if (!world.TrySetTile(100, 20, new WorldTile(true, 1)) ||
      !world.TrySetTile(100, 21, new WorldTile(true, 40)) ||
      !world.TrySetTile(100, 22, new WorldTile(true, 40)) ||
      !world.TrySetTile(100, 23, new WorldTile(true, 40)) ||
      !world.TrySetTile(100, 24, new WorldTile(true, 40)) ||
      !world.TrySetTile(100, 25, new WorldTile(true, 40)) ||
      !world.TrySetTile(101, 10, new WorldTile(true, 1)))
  {
    throw new InvalidOperationException("Clay cleanup fixture could not seed active tiles.");
  }

  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  string inputFingerprint = CreateSnapshotWorldGridFingerprint(snapshot);
  WorldGenerationStateComponent firstState = new(seed);
  List<TileChangeCommand> firstCommands = new();
  LegacyPassRandomState firstRandom = new(seed);
  LegacyClayPass.AppendCommands(
    snapshot,
    profile,
    firstRandom,
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref firstState,
    firstCommands);
  if (firstState.Stage != WorldGenerationStage.Cave || firstCommands.Count == 0 ||
      !StringComparer.Ordinal.Equals(
        inputFingerprint,
        CreateSnapshotWorldGridFingerprint(snapshot)) ||
      firstCommands.Any(command =>
        !metadata.IsInside(command.X, command.Y) || command.Sequence < 0 ||
        (command.Source != "worldgen.cave.Clay.cleanup" && command.TileType != 40) ||
        (command.Source == "worldgen.cave.Clay.cleanup" &&
          (command.TileType != 0 || command.Priority <= 0))))
  {
    throw new InvalidOperationException(
      "Clay did not emit bounded source-attributed commands from its immutable snapshot.");
  }

  string[] expectedFamilies =
  [
    "worldgen.cave.Clay.surface-low-clay",
    "worldgen.cave.Clay.surface-high-clay",
    "worldgen.cave.Clay.rock-high-clay"
  ];
  foreach (string familySource in expectedFamilies)
  {
    if (!firstCommands.Any(command => command.Source == familySource))
    {
      throw new InvalidOperationException(
        $"Clay source family did not emit a command: {familySource}.");
    }
  }

  List<TileChangeCommand> cleanupCommands = firstCommands
    .Where(command => command.Source == "worldgen.cave.Clay.cleanup")
    .ToList();
  if (!cleanupCommands.Any(command => command.X == 100 && command.Y is >= 21 and <= 25) ||
      cleanupCommands.Any(command => command.X is < 5 or >= width - 5) ||
      cleanupCommands.Any(command => command.Y < 20 || command.Y > 24 + 1) ||
      cleanupCommands.Max(command => command.Sequence) <= firstCommands
        .Where(command => command.Source != "worldgen.cave.Clay.cleanup")
        .Max(command => command.Sequence))
  {
    throw new InvalidOperationException(
      "Clay cleanup did not preserve first-active and five-row source ordering.");
  }

  LegacyPassRandomState expectedRandom = new(seed);
  for (int recipeIndex = 0; recipeIndex < recipes.Count; recipeIndex++)
  {
    LegacyTileRunnerPassInput recipe = recipes[recipeIndex];
    (int minimumY, int maximumYExclusive) = recipeIndex switch
    {
      0 => (0, (int)profile.WorldSurfaceLow),
      1 => ((int)profile.RockLayer - 25, height - 350),
      2 => ((int)profile.WorldSurfaceLow, (int)profile.WorldSurfaceHigh + 1),
      3 => ((int)profile.WorldSurfaceHigh, (int)profile.RockLayerHigh + 1),
      _ => throw new InvalidOperationException("Clay recipe index was invalid.")
    };
    if (recipeIndex == 1)
    {
      continue;
    }

    LegacyTileRunnerPassInvocation invocation =
      LegacyTileRunnerPassInvocationFactory.Create(
        recipe,
        expectedRandom,
        minimumXInclusive: 0,
        maximumXExclusive: width,
        minimumYInclusive: minimumY,
        maximumYExclusive: maximumYExclusive);
    if (invocation.XDraw is < 0 or >= width ||
        invocation.YDraw < minimumY || invocation.YDraw >= maximumYExclusive ||
        invocation.StrengthDraw < recipe.MinimumStrength ||
        invocation.StrengthDraw >= recipe.MaximumStrengthExclusive ||
        invocation.StepsDraw < recipe.MinimumSteps ||
        invocation.StepsDraw >= recipe.MaximumStepsExclusive ||
        invocation.RandomDrawCount != 4)
    {
      throw new InvalidOperationException(
        $"Clay invocation range or draw order drifted for {recipe.RecipeName}.");
    }
  }

  WorldGenerationStateComponent secondState = new(seed);
  List<TileChangeCommand> secondCommands = new();
  LegacyPassRandomState secondRandom = new(seed);
  LegacyClayPass.AppendCommands(
    snapshot,
    profile,
    secondRandom,
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref secondState,
    secondCommands);
  if (!firstCommands.SequenceEqual(secondCommands) ||
      firstState.NextSequence != secondState.NextSequence ||
      firstRandom.SampleCount != secondRandom.SampleCount)
  {
    throw new InvalidOperationException(
      "Clay command emission was not deterministic from an immutable snapshot.");
  }

  WorldGrid commitWorld = WorldGrid.FromSnapshot(snapshot);
  LegacyTileRunnerPassCommandBatch batch = new(
    "Clay",
    firstCommands.AsReadOnly(),
    Array.Empty<LiquidChangeCommand>(),
    firstState.NextSequence);
  if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
        commitWorld,
        batch,
        Array.Empty<LiquidDefinition>(),
        out LegacyTileRunnerCommandCommitResult commitResult) ||
      !commitResult.Succeeded ||
      commitResult.AppliedTileCount != firstCommands.Count ||
      StringComparer.Ordinal.Equals(
        inputFingerprint,
        CreateSnapshotWorldGridFingerprint(commitWorld.CreateSnapshot(metadata))) ||
      commitWorld.GetTile(100, 21).Type != 0)
  {
    throw new InvalidOperationException("Clay typed command batch did not commit atomically.");
  }

  WorldGenerationStateComponent skyblockState = new(seed);
  List<TileChangeCommand> skyblockCommands = new();
  LegacyPassRandomState skyblockRandom = new(seed);
  LegacyClayPass.AppendCommands(
    snapshot,
    profile,
    skyblockRandom,
    isRemixWorld: false,
    isSkyblockWorld: true,
    ref skyblockState,
    skyblockCommands);
  LegacyPassRandomState skyblockExpectedRandom = new(seed);
  if (skyblockCommands.Count != 0 || skyblockState.Stage != WorldGenerationStage.Created ||
      skyblockState.NextSequence != 0 || skyblockRandom.SampleCount != 0 ||
      skyblockExpectedRandom.Next(100) != skyblockRandom.Next(100))
  {
    throw new InvalidOperationException("Clay did not honor the Skyblock deny-generation guard.");
  }

  const int remixHeight = 600;
  WorldMetadata remixMetadata = new(
    "clay-remix-focused",
    new WorldSeed(seed),
    width,
    remixHeight,
    worldId: seed,
    spawnX: width / 2,
    spawnY: 20,
    worldSurface: 65,
    rockLayer: 220,
    isRemixWorld: true);
  LegacyTerrainRuntimeProfile remixProfile = profile with
  {
    RockLayer = 220.5,
    RockLayerLow = 190.5,
    RockLayerHigh = 240.75
  };
  WorldGrid remixWorld = new(width, remixHeight, initializeLegacyEmptyFrames: true);
  if (!remixWorld.TrySetTile(100, 20, new WorldTile(true, 1)))
  {
    throw new InvalidOperationException("Clay Remix fixture could not seed an active tile.");
  }

  WorldGenerationStateComponent remixState = new(seed);
  List<TileChangeCommand> remixCommands = new();
  LegacyPassRandomState remixRandom = new(seed);
  LegacyClayPass.AppendCommands(
    remixWorld.CreateSnapshot(remixMetadata),
    remixProfile,
    remixRandom,
    isRemixWorld: true,
    isSkyblockWorld: false,
    ref remixState,
    remixCommands);
  if (!remixCommands.Any(command => command.Source == "worldgen.cave.Clay.remix-clay") ||
      remixCommands.Any(command => command.Source is
        "worldgen.cave.Clay.surface-high-clay" or
        "worldgen.cave.Clay.rock-high-clay") ||
      remixRandom.SampleCount <= firstRandom.SampleCount)
  {
    throw new InvalidOperationException(
      "Clay Remix branch did not preserve its separate source loop contract.");
  }

  Console.WriteLine(
    $"PASS: Clay source recipes, floor counts, deterministic commands, cleanup, Remix guard, " +
    $"and atomic commit ({firstCommands.Count} commands; source line {LegacyClayPass.SourceLine})");
  Console.WriteLine(
    "SUMMARY: Clay focused verification completed; complete TileRunner traversal, Remix oracle " +
    "parity, aggregate cave ordering, full WLD differential, and legacy deletion remain deferred");
}

static void RunDirtInRocksFocusedVerification()
{
  const int width = 200;
  const int height = 150;
  const int seed = 1456;
  LegacyDirtInRocksPassDefinition.Validate();
  LegacyTileRunnerPassInput recipe =
    LegacyDirtInRocksPassDefinition.CreateDefaultRecipe();
  if (recipe != new LegacyTileRunnerPassInput(
        "DirtInRocks",
        "rock-layer-dirt",
        0,
        false,
        2,
        6,
        2,
        40,
        "rockLayerLow..maxTilesY",
        true,
        true,
        true) ||
      LegacyDirtInRocksPassDefinition.CalculateInvocationCount(width, height) != 150 ||
      LegacyDirtInRocksPassDefinition.CalculateInvocationCount(4200, 1200) != 25200)
  {
    throw new InvalidOperationException(
      "DirtInRocks definition drifted from the source loop contract.");
  }

  WorldMetadata metadata = new(
    "dirt-in-rocks-focused",
    new WorldSeed(seed),
    width,
    height,
    worldId: seed,
    spawnX: width / 2,
    spawnY: 40,
    worldSurface: 40,
    rockLayer: 80,
    isRemixWorld: false);
  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 40.0,
    RockLayer: 80.0,
    WorldSurfaceLow: 20.0,
    WorldSurfaceHigh: 40.0,
    RockLayerLow: 50.0,
    RockLayerHigh: 90.0,
    LeftBeachEnd: 20,
    RightBeachStart: 180,
    WaterLine: 100,
    LavaLine: 120);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  for (int x = 0; x < width; x++)
  {
    for (int y = 50; y < 65; y++)
    {
      if (!world.TrySetTile(x, y, new WorldTile(true, 53)))
      {
        throw new InvalidOperationException(
          "DirtInRocks type-53 preservation fixture could not seed a tile.");
      }
    }

    for (int y = 70; y < height; y++)
    {
      if ((x + y) % 3 == 0 && !world.TrySetTile(x, y, new WorldTile(true, 1)))
      {
        throw new InvalidOperationException(
          "DirtInRocks input fixture could not seed an active rock tile.");
      }
    }
  }

  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  string inputFingerprint = CreateSnapshotWorldGridFingerprint(snapshot);
  WorldGenerationStateComponent firstState = new(seed);
  List<TileChangeCommand> firstCommands = new();
  LegacyPassRandomState firstRandom = new(seed);
  LegacyDirtInRocksPass.AppendCommands(
    snapshot,
    profile,
    firstRandom,
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref firstState,
    firstCommands);
  if (firstState.Stage != WorldGenerationStage.Cave || firstCommands.Count == 0 ||
      !StringComparer.Ordinal.Equals(
        inputFingerprint,
        CreateSnapshotWorldGridFingerprint(snapshot)) ||
      firstCommands.Any(command =>
        !metadata.IsInside(command.X, command.Y) ||
        command.TileType != 0 ||
        !command.Source.StartsWith("worldgen.cave.DirtInRocks.", StringComparison.Ordinal)))
  {
    throw new InvalidOperationException(
      "DirtInRocks did not emit bounded source-attributed commands from its immutable snapshot.");
  }

  LegacyPassRandomState expectedRandom = new(seed);
  LegacyTileRunnerPassInvocation expectedInvocation =
    LegacyTileRunnerPassInvocationFactory.Create(
      recipe,
      expectedRandom,
      minimumXInclusive: 0,
      maximumXExclusive: width,
      minimumYInclusive: 50,
      maximumYExclusive: height);
  if (expectedInvocation.XDraw is < 0 or >= width ||
      expectedInvocation.YDraw is < 50 or >= height ||
      expectedInvocation.StrengthDraw is < 2 or >= 6 ||
      expectedInvocation.StepsDraw is < 2 or >= 40 ||
      expectedInvocation.RandomDrawCount != 4)
  {
    throw new InvalidOperationException(
      $"DirtInRocks invocation ranges or draw ordering drifted from the source: " +
      $"x={expectedInvocation.XDraw}, y={expectedInvocation.YDraw}, " +
      $"strength={expectedInvocation.StrengthDraw}, steps={expectedInvocation.StepsDraw}, " +
      $"draws={expectedInvocation.RandomDrawCount}.");
  }

  WorldGenerationStateComponent secondState = new(seed);
  List<TileChangeCommand> secondCommands = new();
  LegacyPassRandomState secondRandom = new(seed);
  LegacyDirtInRocksPass.AppendCommands(
    snapshot,
    profile,
    secondRandom,
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref secondState,
    secondCommands);
  if (!firstCommands.SequenceEqual(secondCommands) ||
      firstState.NextSequence != secondState.NextSequence ||
      firstRandom.SampleCount != secondRandom.SampleCount)
  {
    throw new InvalidOperationException(
      "DirtInRocks command emission was not deterministic from an immutable snapshot.");
  }

  if (LegacyMainWorldSurfacePolicy.Resolve(profile, metadata) != 65 ||
      firstCommands.Any(command =>
        command.Kind == TileChangeKind.UpdateTileType &&
        command.TileType == 0 &&
        command.Y >= 50 &&
        command.Y < 65 &&
        snapshot.GetTile(command.X, command.Y).Type == 53))
  {
    throw new InvalidOperationException(
      "DirtInRocks incorrectly overwrote type-53 tiles below Main.worldSurface.");
  }

  WorldGrid commitWorld = WorldGrid.FromSnapshot(snapshot);
  LegacyTileRunnerPassCommandBatch batch = new(
    "DirtInRocks",
    firstCommands.AsReadOnly(),
    Array.Empty<LiquidChangeCommand>(),
    firstState.NextSequence);
  if (!LegacyTileRunnerCommandCommitBoundary.TryCommit(
        commitWorld,
        batch,
        Array.Empty<LiquidDefinition>(),
        out LegacyTileRunnerCommandCommitResult commitResult) ||
      !commitResult.Succeeded ||
      commitResult.AppliedTileCount != firstCommands.Count ||
      StringComparer.Ordinal.Equals(
        inputFingerprint,
        CreateSnapshotWorldGridFingerprint(commitWorld.CreateSnapshot(metadata))))
  {
    throw new InvalidOperationException(
      "DirtInRocks typed command batch did not commit atomically.");
  }

  WorldGenerationStateComponent skyblockState = new(seed);
  List<TileChangeCommand> skyblockCommands = new();
  LegacyDirtInRocksPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isRemixWorld: false,
    isSkyblockWorld: true,
    ref skyblockState,
    skyblockCommands);
  if (skyblockCommands.Count != 0 || skyblockState.Stage != WorldGenerationStage.Created ||
      skyblockState.NextSequence != 0)
  {
    throw new InvalidOperationException(
      "DirtInRocks did not honor the Skyblock deny-generation guard.");
  }

  WorldMetadata remixMetadata = new(
    "dirt-in-rocks-remix-focused",
    new WorldSeed(seed),
    width,
    height,
    worldId: seed,
    spawnX: width / 2,
    spawnY: 40,
    worldSurface: 40,
    rockLayer: 80,
    isRemixWorld: true);
  LegacyTerrainRuntimeProfile remixProfile = profile with
  {
    RockLayer = 130.0,
    RockLayerLow = 120.0,
    RockLayerHigh = 140.0
  };
  WorldGrid remixWorld = new(width, height, initializeLegacyEmptyFrames: true);
  if (!remixWorld.TrySetTile(0, 70, new WorldTile(true, 0)) ||
      !remixWorld.TrySetTile(1, 70, new WorldTile(true, 1)))
  {
    throw new InvalidOperationException(
      "DirtInRocks Remix fixture could not seed toggle tiles.");
  }

  WorldGridSnapshot remixSnapshot = remixWorld.CreateSnapshot(remixMetadata);
  WorldGenerationStateComponent remixBaseState = new(seed);
  List<TileChangeCommand> remixBaseCommands = new();
  LegacyPassRandomState remixBaseRandom = new(seed);
  LegacyDirtInRocksPass.AppendCommands(
    remixSnapshot,
    remixProfile,
    remixBaseRandom,
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref remixBaseState,
    remixBaseCommands);
  WorldGenerationStateComponent remixState = new(seed);
  List<TileChangeCommand> remixCommands = new();
  LegacyPassRandomState remixRandom = new(seed);
  LegacyDirtInRocksPass.AppendCommands(
    remixSnapshot,
    remixProfile,
    remixRandom,
    isRemixWorld: true,
    isSkyblockWorld: false,
    ref remixState,
    remixCommands);
  if (remixCommands.Count != 2 ||
      remixCommands.Any(command =>
        command.Source != "worldgen.cave.DirtInRocks.remix" ||
        command.IsActive != true ||
        command.TileType is not (0 or 1)) ||
      remixRandom.SampleCount != remixBaseRandom.SampleCount + width)
  {
    throw new InvalidOperationException(
      "DirtInRocks Remix did not preserve the post-loop column toggle contract.");
  }

  WorldGrid remixCommitted = WorldGrid.FromSnapshot(remixSnapshot);
  if (!new TileChangeCommitSystem().TryCommit(
        remixCommitted,
        remixCommands,
        out TileChangeCommitResult remixCommitResult) ||
      remixCommitResult.AppliedCount != remixCommands.Count ||
      remixCommitted.GetTile(0, 70).Type != 1 ||
      remixCommitted.GetTile(1, 70).Type != 0)
  {
    throw new InvalidOperationException(
      "DirtInRocks Remix toggle commands did not commit atomically.");
  }

  Console.WriteLine(
    $"PASS: DirtInRocks source recipe, ranges, deterministic commands, Remix toggle, and " +
    $"atomic commit ({firstCommands.Count} base commands; source line " +
    $"{LegacyDirtInRocksPass.SourceLine})");
  Console.WriteLine(
    "SUMMARY: DirtInRocks focused verification completed; complete TileRunner traversal, " +
    "aggregate cave ordering, full WLD differential, and legacy deletion remain deferred");
}

static WorldGridSnapshot CreateActiveOffsetSnapshot(WorldMetadata metadata, int x, int y)
{
  WorldGrid world = new(metadata.Width, metadata.Height, initializeLegacyEmptyFrames: true);
  if (!world.TrySetTile(x, y, new WorldTile(IsActive: true, Type: 1)))
  {
    throw new InvalidOperationException("RocksInDirt active-offset fixture could not seed a tile.");
  }

  return world.CreateSnapshot(metadata);
}

static void RunDirtLayerCavesFocusedVerification()
{
  const int width = 800;
  const int height = 300;
  const int seed = 1456;
  LegacyDirtLayerCavesPassDefinition definition =
    LegacyDirtLayerCavesPassDefinitionFactory.CreateDefault();
  definition.Validate();
  if (definition.Density != 3E-05 ||
      definition.LiquidPreservingChanceDenominator != 6 ||
      definition.MinimumStrength != 5 ||
      definition.MaximumStrengthExclusive != 15 ||
      definition.MinimumSteps != 30 ||
      definition.MaximumStepsExclusive != 200 ||
      definition.SmallHolesBeachAvoidance != 340 ||
      definition.CalculateInvocationCount(4200, 1200, false) != 151 ||
      definition.CalculateInvocationCount(4200, 1200, true) != 302)
  {
    throw new InvalidOperationException(
      "DirtLayerCaves definition drifted from the source loop contract.");
  }

  FieldInfo? recipeField = typeof(LegacyDirtLayerCavesPass).GetField(
    "_recipe",
    BindingFlags.NonPublic | BindingFlags.Static);
  if (recipeField is null || recipeField.FieldType != typeof(LegacyTileRunnerPassInput))
  {
    throw new InvalidOperationException(
      "DirtLayerCaves owner did not expose the private typed recipe field.");
  }

  if (LegacyDirtLayerCavesPass.IsSupportedWorldWidth(680) ||
      !LegacyDirtLayerCavesPass.IsSupportedWorldWidth(681))
  {
    throw new InvalidOperationException(
      "DirtLayerCaves width support did not preserve the two-margin boundary.");
  }

  if (LegacyDirtLayerCavesPass.IsCandidateAccepted(
        100,
        99,
        width,
        100.0,
        80.0,
        definition.SmallHolesBeachAvoidance) ||
      !LegacyDirtLayerCavesPass.IsCandidateAccepted(
        100,
        100,
        width,
        100.0,
        80.0,
        definition.SmallHolesBeachAvoidance) ||
      LegacyDirtLayerCavesPass.IsCandidateAccepted(
        461,
        99,
        width,
        100.0,
        80.0,
        definition.SmallHolesBeachAvoidance) ||
      !LegacyDirtLayerCavesPass.IsCandidateAccepted(
        460,
        99,
        width,
        100.0,
        80.0,
        definition.SmallHolesBeachAvoidance) ||
      LegacyDirtLayerCavesPass.IsCandidateAccepted(
        360,
        79,
        width,
        100.0,
        80.0,
        definition.SmallHolesBeachAvoidance) ||
      !LegacyDirtLayerCavesPass.IsCandidateAccepted(
        360,
        80,
        width,
        100.0,
        80.0,
        definition.SmallHolesBeachAvoidance) ||
      LegacyDirtLayerCavesPass.IsCandidateAccepted(
        440,
        79,
        width,
        100.0,
        80.0,
        definition.SmallHolesBeachAvoidance) ||
      !LegacyDirtLayerCavesPass.IsCandidateAccepted(
        440,
        80,
        width,
        100.0,
        80.0,
        definition.SmallHolesBeachAvoidance))
  {
    throw new InvalidOperationException(
      "DirtLayerCaves candidate retry predicate did not preserve source boundaries.");
  }

  bool sawStandardTileType = false;
  bool sawLiquidPreservingTileType = false;
  for (int randomSeed = 0; randomSeed < 10000; randomSeed++)
  {
    int tileType = definition.SelectTileType(new LegacyPassRandomState(randomSeed));
    sawStandardTileType |= tileType == -1;
    sawLiquidPreservingTileType |= tileType == -2;
  }

  if (!sawStandardTileType || !sawLiquidPreservingTileType ||
      definition.ScaleStrength(10, false) != 10 ||
      definition.ScaleStrength(10, true) != 11 ||
      definition.ScaleSteps(100, false) != 100 ||
      definition.ScaleSteps(100, true) != 190)
  {
    throw new InvalidOperationException(
      "DirtLayerCaves type selection or remix scaling was not source-backed.");
  }

  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 80.0,
    RockLayer: 150.0,
    WorldSurfaceLow: 60.0,
    WorldSurfaceHigh: 100.0,
    RockLayerLow: 120.0,
    RockLayerHigh: 160.0,
    LeftBeachEnd: 100,
    RightBeachStart: 700,
    WaterLine: 200,
    LavaLine: 240);
  WorldMetadata metadata = new(
    "dirt-layer-caves-focused",
    new WorldSeed(seed),
    width,
    height,
    isRemixWorld: false);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  string inputFingerprint = CreateSnapshotWorldGridFingerprint(snapshot);
  MethodInfo createInvocation = typeof(LegacyDirtLayerCavesPass).GetMethod(
    "CreateInvocation",
    BindingFlags.NonPublic | BindingFlags.Static) ??
    throw new InvalidOperationException("DirtLayerCaves invocation owner was not found.");
  LegacyPassRandomState invocationRandom = new(seed);
  LegacyTileRunnerPassInvocation invocation =
    (LegacyTileRunnerPassInvocation)(createInvocation.Invoke(
      null,
      [snapshot, profile, definition, invocationRandom, false]) ??
      throw new InvalidOperationException("DirtLayerCaves invocation was not created."));
  LegacyPassRandomState expectedRandom = new(seed);
  int expectedTileType = definition.SelectTileType(expectedRandom);
  int expectedX = expectedRandom.Next(0, width);
  int expectedY = expectedRandom.Next((int)profile.WorldSurfaceLow, (int)profile.RockLayerHigh + 1);
  int retryCount = 0;
  while (!LegacyDirtLayerCavesPass.IsCandidateAccepted(
           expectedX,
           expectedY,
           width,
           profile.WorldSurfaceHigh,
           profile.WorldSurface,
           definition.SmallHolesBeachAvoidance))
  {
    expectedX = expectedRandom.Next(0, width);
    expectedY = expectedRandom.Next(
      (int)profile.WorldSurfaceLow,
      (int)profile.RockLayerHigh + 1);
    retryCount++;
  }

  int expectedStrength = expectedRandom.Next(
    definition.MinimumStrength,
    definition.MaximumStrengthExclusive);
  int expectedSteps = expectedRandom.Next(
    definition.MinimumSteps,
    definition.MaximumStepsExclusive);
  if (invocation.Request.TileType != expectedTileType ||
      invocation.Request.X != expectedX ||
      invocation.Request.Y != expectedY ||
      invocation.Request.Strength != expectedStrength ||
      invocation.Request.Steps != expectedSteps ||
      invocation.RandomDrawCount != 5 + retryCount * 2)
  {
    throw new InvalidOperationException(
      "DirtLayerCaves invocation did not preserve source random draw order.");
  }

  WorldGenerationStateComponent firstState = new(7);
  List<TileChangeCommand> firstCommands = new();
  LegacyDirtLayerCavesPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref firstState,
    firstCommands);
  if (firstCommands.Count == 0 ||
      firstState.Stage != WorldGenerationStage.Cave ||
      !StringComparer.Ordinal.Equals(
        inputFingerprint,
        CreateSnapshotWorldGridFingerprint(snapshot)) ||
      firstCommands.Any(command =>
        !metadata.IsInside(command.X, command.Y) ||
        command.Sequence < 0 ||
        command.Kind != TileChangeKind.Kill ||
        command.TileType != 0 ||
        !StringComparer.Ordinal.Equals(
          command.Source,
          "worldgen.cave.DirtLayerCaves.dirt-layer")))
  {
    throw new InvalidOperationException(
      "DirtLayerCaves did not emit bounded source-attributed kill commands from its snapshot.");
  }

  WorldGenerationStateComponent secondState = new(7);
  List<TileChangeCommand> secondCommands = new();
  LegacyDirtLayerCavesPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref secondState,
    secondCommands);
  if (!firstCommands.SequenceEqual(secondCommands) ||
      firstState.Stage != secondState.Stage ||
      firstState.NextSequence != secondState.NextSequence)
  {
    throw new InvalidOperationException(
      "DirtLayerCaves command emission was not deterministic from an immutable snapshot.");
  }

  WorldGrid commitWorld = WorldGrid.FromSnapshot(snapshot);
  TileChangeCommand firstCommand = firstCommands[0];
  if (!commitWorld.TrySetTile(firstCommand.X, firstCommand.Y, new WorldTile(true, 1)))
  {
    throw new InvalidOperationException(
      "DirtLayerCaves commit fixture could not seed an active tile.");
  }

  WorldGridSnapshot commitInput = commitWorld.CreateSnapshot(metadata);
  string commitInputFingerprint = CreateSnapshotWorldGridFingerprint(commitInput);
  if (!new TileChangeCommitSystem().TryCommit(
        commitWorld,
        firstCommands,
        out TileChangeCommitResult commitResult) ||
      commitResult.AppliedCount != firstCommands.Count ||
      StringComparer.Ordinal.Equals(
        commitInputFingerprint,
        CreateSnapshotWorldGridFingerprint(commitWorld.CreateSnapshot(metadata))))
  {
    throw new InvalidOperationException(
      "DirtLayerCaves commands did not remain isolated until deterministic commit.");
  }

  WorldGenerationStateComponent skyblockState = new(7);
  List<TileChangeCommand> skyblockCommands = new();
  LegacyDirtLayerCavesPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isRemixWorld: false,
    isSkyblockWorld: true,
    ref skyblockState,
    skyblockCommands);
  if (skyblockCommands.Count != 0 ||
      skyblockState.Stage != WorldGenerationStage.Created ||
      skyblockState.NextSequence != 0)
  {
    throw new InvalidOperationException(
      "DirtLayerCaves did not honor the Skyblock deny-generation guard.");
  }

  Console.WriteLine(
    $"PASS: DirtLayerCaves focused contract, deterministic commands, and commit isolation " +
    $"({firstCommands.Count} commands; source line {LegacyDirtLayerCavesPass.SourceAnchorLine})");
  Console.WriteLine(
    "SUMMARY: DirtLayerCaves focused verification completed; aggregate cave ordering, full " +
    "TileRunner parity, WLD differential, and legacy deletion remain deferred");
}

static void RunRockLayerCavesFocusedVerification()
{
  const int width = 800;
  const int height = 300;
  const int seed = 1456;
  LegacyRockLayerCavesPassDefinition definition =
    LegacyRockLayerCavesPassDefinitionFactory.CreateDefault();
  definition.Validate();
  if (definition.Density != 0.00013 ||
      definition.LiquidPreservingChanceDenominator != 10 ||
      definition.MinimumStrength != 6 ||
      definition.MaximumStrengthExclusive != 20 ||
      definition.MinimumSteps != 50 ||
      definition.MaximumStepsExclusive != 300 ||
      definition.CalculateInvocationCount(4200, 1200, false) != 655 ||
      definition.CalculateInvocationCount(4200, 1200, true) != 720 ||
      definition.CalculateInvocationCount(200, 150, false) != 3 ||
      definition.ScaleStrength(19, true) != 13 ||
      definition.ScaleSteps(299, true) != 209)
  {
    throw new InvalidOperationException(
      "RockLayerCaves definition or floor-based cardinality drifted from the source.");
  }

  bool sawStandardTileType = false;
  bool sawLiquidPreservingTileType = false;
  for (int randomSeed = 0; randomSeed < 10000; randomSeed++)
  {
    int tileType = definition.SelectTileType(new LegacyPassRandomState(randomSeed));
    sawStandardTileType |= tileType == -1;
    sawLiquidPreservingTileType |= tileType == -2;
  }

  if (!sawStandardTileType || !sawLiquidPreservingTileType)
  {
    throw new InvalidOperationException(
      "RockLayerCaves type selection did not preserve the -1/-2 source branches.");
  }

  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 80.0,
    RockLayer: 150.0,
    WorldSurfaceLow: 60.0,
    WorldSurfaceHigh: 100.0,
    RockLayerLow: 120.0,
    RockLayerHigh: 160.0,
    LeftBeachEnd: 100,
    RightBeachStart: 700,
    WaterLine: 200,
    LavaLine: 240);
  WorldMetadata metadata = new(
    "rock-layer-caves-focused",
    new WorldSeed(seed),
    width,
    height,
    isRemixWorld: false);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  LegacyPassRandomState expectedRandom = new(seed);
  int expectedTileType = definition.SelectTileType(expectedRandom);
  int expectedStrength = expectedRandom.Next(
    definition.MinimumStrength,
    definition.MaximumStrengthExclusive);
  int expectedSteps = expectedRandom.Next(
    definition.MinimumSteps,
    definition.MaximumStepsExclusive);
  int expectedX = expectedRandom.Next(0, width);
  int expectedY = expectedRandom.Next((int)profile.RockLayerHigh, height);
  if (!world.TrySetTile(expectedX, expectedY, new WorldTile(IsActive: true, Type: 1)))
  {
    throw new InvalidOperationException(
      "RockLayerCaves focused fixture could not seed an active tile.");
  }

  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  string inputFingerprint = CreateSnapshotWorldGridFingerprint(snapshot);
  FieldInfo? recipeField = typeof(LegacyRockLayerCavesPass).GetField(
    "_recipe",
    BindingFlags.NonPublic | BindingFlags.Static);
  if (recipeField is null || recipeField.FieldType != typeof(LegacyTileRunnerPassInput))
  {
    throw new InvalidOperationException(
      "RockLayerCaves owner did not expose the private typed recipe field.");
  }

  LegacyTileRunnerPassInput recipe =
    (LegacyTileRunnerPassInput)(recipeField.GetValue(null) ??
      throw new InvalidOperationException("RockLayerCaves recipe was null."));
  if (recipe.PassName != "RockLayerCaves" || recipe.RecipeName != "rock-layer" ||
      recipe.TileType != -1 || recipe.AddTile || recipe.MinimumStrength != 6 ||
      recipe.MaximumStrengthExclusive != 20 || recipe.MinimumSteps != 50 ||
      recipe.MaximumStepsExclusive != 300 || recipe.VerticalRange != "rockLayerHigh..maxTilesY" ||
      !recipe.UsesRandomX || !recipe.UsesRandomY || !recipe.ResetsRandomFromWorldSeed)
  {
    throw new InvalidOperationException(
      "RockLayerCaves typed recipe drifted from the source invocation contract.");
  }

  PropertyInfo preserveTileStateProperty = typeof(TileChangeCommand).GetProperty(
    "PreserveTileState",
    BindingFlags.Public | BindingFlags.Instance) ??
    throw new InvalidOperationException(
      "Tile change commands did not expose negative TileRunner state preservation.");

  MethodInfo createInvocation = typeof(LegacyRockLayerCavesPass).GetMethod(
    "CreateInvocation",
    BindingFlags.NonPublic | BindingFlags.Static) ??
    throw new InvalidOperationException("RockLayerCaves invocation owner was not found.");
  LegacyPassRandomState invocationRandom = new(seed);
  LegacyTileRunnerPassInvocation invocation =
    (LegacyTileRunnerPassInvocation)(createInvocation.Invoke(
      null,
      [snapshot, profile, definition, invocationRandom, false]) ??
      throw new InvalidOperationException("RockLayerCaves invocation was not created."));
  if (invocation.Request.TileType != expectedTileType ||
      invocation.Request.Strength != expectedStrength ||
      invocation.Request.Steps != expectedSteps ||
      invocation.Request.X != expectedX ||
      invocation.Request.Y != expectedY ||
      invocation.RandomDrawCount != 5 ||
      invocationRandom.SampleCount != 5 ||
      invocation.Request.AddTile || invocation.Request.NoYChange ||
      invocation.Request.SpeedX != 0.0 || invocation.Request.SpeedY != 0.0)
  {
    throw new InvalidOperationException(
      "RockLayerCaves invocation did not preserve source draw order or request flags.");
  }

  WorldGenerationStateComponent firstState = new(7);
  List<TileChangeCommand> firstCommands = new();
  LegacyPassRandomState firstRandom = new(seed);
  LegacyRockLayerCavesPass.AppendCommands(
    snapshot,
    profile,
    firstRandom,
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref firstState,
    firstCommands);
  if (firstCommands.Count == 0 ||
      firstState.Stage != WorldGenerationStage.Cave ||
      firstRandom.SampleCount <= 5 ||
      !StringComparer.Ordinal.Equals(
        inputFingerprint,
        CreateSnapshotWorldGridFingerprint(snapshot)) ||
      firstCommands.Any(command =>
        !metadata.IsInside(command.X, command.Y) ||
        command.Sequence < 0 ||
        command.Kind != TileChangeKind.Kill ||
        command.TileType != 0 ||
        (bool)(preserveTileStateProperty.GetValue(command) ?? false) != true ||
        !StringComparer.Ordinal.Equals(
          command.Source,
          "worldgen.cave.RockLayerCaves.rock-layer")))
  {
    throw new InvalidOperationException(
      "RockLayerCaves did not emit bounded source-attributed commands from its snapshot.");
  }

  WorldGenerationStateComponent secondState = new(7);
  List<TileChangeCommand> secondCommands = new();
  LegacyPassRandomState secondRandom = new(seed);
  LegacyRockLayerCavesPass.AppendCommands(
    snapshot,
    profile,
    secondRandom,
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref secondState,
    secondCommands);
  if (!firstCommands.SequenceEqual(secondCommands) ||
      firstState.Stage != secondState.Stage ||
      firstState.NextSequence != secondState.NextSequence ||
      firstRandom.SampleCount != secondRandom.SampleCount)
  {
    throw new InvalidOperationException(
      "RockLayerCaves command emission was not deterministic from an immutable snapshot.");
  }

  WorldGrid commitWorld = WorldGrid.FromSnapshot(snapshot);
  string commitInputFingerprint =
    CreateSnapshotWorldGridFingerprint(commitWorld.CreateSnapshot(metadata));
  WorldTile committedInputTile = commitWorld.GetTile(firstCommands[0].X, firstCommands[0].Y);
  if (!new TileChangeCommitSystem().TryCommit(
        commitWorld,
        firstCommands,
        out TileChangeCommitResult commitResult) ||
       !commitResult.Succeeded ||
       commitResult.AppliedCount != firstCommands.Count ||
       StringComparer.Ordinal.Equals(
         commitInputFingerprint,
         CreateSnapshotWorldGridFingerprint(commitWorld.CreateSnapshot(metadata))) ||
       commitWorld.GetTile(firstCommands[0].X, firstCommands[0].Y) !=
         committedInputTile with { IsActive = false })
  {
    throw new InvalidOperationException(
      "RockLayerCaves typed commands did not preserve negative TileRunner state at commit.");
  }

  WorldGenerationStateComponent skyblockState = new(7);
  List<TileChangeCommand> skyblockCommands = new();
  LegacyPassRandomState skyblockRandom = new(seed);
  LegacyRockLayerCavesPass.AppendCommands(
    snapshot,
    profile,
    skyblockRandom,
    isRemixWorld: false,
    isSkyblockWorld: true,
    ref skyblockState,
    skyblockCommands);
  if (skyblockCommands.Count != 0 || skyblockState.Stage != WorldGenerationStage.Created ||
      skyblockState.NextSequence != 0 || skyblockRandom.SampleCount != 0)
  {
    throw new InvalidOperationException(
      "RockLayerCaves did not honor the Skyblock deny-generation guard.");
  }

  LegacyTerrainRuntimeProfile invalidHighProfile = profile with
  {
    RockLayerHigh = height + 1.0
  };
  WorldGenerationStateComponent invalidHighState = new(7);
  List<TileChangeCommand> invalidHighCommands = new();
  LegacyPassRandomState invalidHighRandom = new(seed);
  LegacyRockLayerCavesPass.AppendCommands(
    snapshot,
    invalidHighProfile,
    invalidHighRandom,
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref invalidHighState,
    invalidHighCommands);
  if (invalidHighCommands.Count != 0 || invalidHighState.Stage != WorldGenerationStage.Created ||
      invalidHighState.NextSequence != 0 || invalidHighRandom.SampleCount != 0)
  {
    throw new InvalidOperationException(
      "RockLayerCaves did not no-op an invalid high-rock-layer range.");
  }

  WorldGenerationRequest request = new(
    metadata,
    spawnX: width / 2,
    surfaceY: 80,
    rockLayerY: 150,
    terrainProfile: profile);
  WorldGenerationStateComponent genericState = new(7);
  List<TileChangeCommand> genericCommands = new();
  new LegacyCavePassSystem().AppendCommands(
    snapshot,
    request,
    ref genericState,
    genericCommands);
  if (genericCommands.Any(command =>
        command.Source.StartsWith(
          "worldgen.cave.RockLayerCaves.",
          StringComparison.Ordinal)))
  {
    throw new InvalidOperationException(
      "Profile-enabled RockLayerCaves was emitted by the generic cave fallback.");
  }

  Console.WriteLine(
    $"PASS: RockLayerCaves base definition, draw order, immutable commands, guards, and " +
    $"generic-owner exclusion ({firstCommands.Count} commands; source line " +
    $"{LegacyRockLayerCavesPass.SourceLine})");
  Console.WriteLine(
    "SUMMARY: RockLayerCaves focused base-loop verification completed; Remix paired " +
    "no-Y-change calls, complete TileRunner/liquid parity, aggregate cave ordering, WLD " +
    "differential, and legacy deletion remain deferred");
}

static void RunMountainCavesFocusedVerification()
{
  const int width = 2000;
  const int height = 300;
  const int seed = 1456;
  WorldMetadata metadata = new(
    "mountain-caves-focused",
    new WorldSeed(seed),
    width,
    height,
    worldSurface: 50,
    rockLayer: 90,
    isRemixWorld: false);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  for (int x = 0; x < width; x++)
  {
    _ = world.TrySetTile(x, 40, new WorldTile(IsActive: true, Type: 1));
  }

  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 50,
    RockLayer: 90,
    WorldSurfaceLow: 40,
    WorldSurfaceHigh: 60,
    RockLayerLow: 80,
    RockLayerHigh: 100,
    LeftBeachEnd: 100,
    RightBeachStart: 1900,
    WaterLine: 180,
    LavaLine: 240);
  MethodInfo calculateInvocationCount = typeof(LegacyMountainCavesPass).GetMethod(
    "CalculateInvocationCount",
    BindingFlags.Public | BindingFlags.Static,
    binder: null,
    types: [typeof(int), typeof(bool)],
    modifiers: null) ??
    throw new InvalidOperationException(
      "MountainCaves invocation-count owner did not expose its Remix contract.");
  MethodInfo isCandidateXAccepted = typeof(LegacyMountainCavesPass).GetMethod(
    "IsCandidateXAccepted",
    BindingFlags.Public | BindingFlags.Static,
    binder: null,
    types: [typeof(int), typeof(int), typeof(IReadOnlyList<int>), typeof(bool)],
    modifiers: null) ??
    throw new InvalidOperationException(
      "MountainCaves candidate owner did not expose its Remix contract.");
  int nonRemixCount = (int)(calculateInvocationCount.Invoke(null, [width, false]) ??
    throw new InvalidOperationException("MountainCaves non-Remix count was not returned."));
  int remixCount = (int)(calculateInvocationCount.Invoke(null, [width, true]) ??
    throw new InvalidOperationException("MountainCaves Remix count was not returned."));
  bool nonRemixCenterAccepted = (bool)(isCandidateXAccepted.Invoke(
    null,
    [width / 2, width, Array.Empty<int>(), false]) ??
    throw new InvalidOperationException("MountainCaves non-Remix candidate result was not returned."));
  bool remixCenterAccepted = (bool)(isCandidateXAccepted.Invoke(
    null,
    [width / 2, width, Array.Empty<int>(), true]) ??
    throw new InvalidOperationException("MountainCaves Remix candidate result was not returned."));
  if (nonRemixCount != 2 || remixCount != 3 || nonRemixCenterAccepted || !remixCenterAccepted)
  {
    throw new InvalidOperationException(
      "MountainCaves Remix count or center-candidate rule diverged from source.");
  }

  MethodInfo appendCommands = typeof(LegacyMountainCavesPass).GetMethod(
    "AppendCommands",
    BindingFlags.Public | BindingFlags.Static,
    binder: null,
    types:
    [
      typeof(WorldGridSnapshot),
      typeof(LegacyTerrainRuntimeProfile),
      typeof(LegacyPassRandomState),
      typeof(WorldGenerationStateComponent).MakeByRefType(),
      typeof(List<TileChangeCommand>),
      typeof(bool),
      typeof(bool),
      typeof(bool),
      typeof(bool)
    ],
    modifiers: null) ??
    throw new InvalidOperationException(
      "MountainCaves owner did not expose explicit generation-rule guards.");
  foreach ((string guard, bool isSkyblockWorld, bool isNoSurfaceWorld,
    bool isSurfaceDesertWorld) in new[]
  {
    ("Skyblock", true, false, false),
    ("no-surface", false, true, false),
    ("surface-desert", false, false, true)
  })
  {
    WorldGenerationStateComponent state = new(seed);
    LegacyPassRandomState random = new(seed);
    List<TileChangeCommand> commands = new();
    object?[] invocationArguments =
    [
      snapshot,
      profile,
      random,
      state,
      commands,
      false,
      isSkyblockWorld,
      isNoSurfaceWorld,
      isSurfaceDesertWorld
    ];
    _ = appendCommands.Invoke(null, invocationArguments);
    state = (WorldGenerationStateComponent)invocationArguments[3]!;
    if (commands.Count != 0 || random.SampleCount != 0 ||
        state.Stage != WorldGenerationStage.Created || state.NextSequence != 0)
    {
      throw new InvalidOperationException(
        $"MountainCaves {guard} guard consumed state, commands, or random samples.");
    }
  }

  Console.WriteLine(
    "PASS: MountainCaves preserves guards and Remix count/center candidate semantics");
}

static void RunWavyCavesFocusedVerification()
{
  Type wavyPassType = typeof(LegacyWavyCaverer).Assembly.GetType(
    "Terraria.Dome.Simulation.WorldGeneration.LegacyWavyCavesPass") ??
    throw new InvalidOperationException("WavyCaves pass owner was not found.");
  MethodInfo calculateInvocationCount = wavyPassType.GetMethod(
    "CalculateInvocationCount",
    BindingFlags.Public | BindingFlags.Static,
    binder: null,
    types: [typeof(int), typeof(bool)],
    modifiers: null) ??
    throw new InvalidOperationException("WavyCaves owner did not expose its count contract.");
  int baseCount = (int)(calculateInvocationCount.Invoke(null, [4200, false]) ??
    throw new InvalidOperationException("WavyCaves base count was not returned."));
  int remixCount = (int)(calculateInvocationCount.Invoke(null, [4200, true]) ??
    throw new InvalidOperationException("WavyCaves Remix count was not returned."));
  if (baseCount != 35 || remixCount != 11)
  {
    throw new InvalidOperationException(
      $"WavyCaves density diverged from source: expected 35/11, got {baseCount}/{remixCount}.");
  }

  MethodInfo appendCommands = wavyPassType.GetMethod(
    "AppendCommands",
    BindingFlags.Public | BindingFlags.Static,
    binder: null,
    types:
    [
      typeof(WorldGridSnapshot),
      typeof(LegacyTerrainRuntimeProfile),
      typeof(LegacyPassRandomState),
      typeof(LegacyWavyCavesPassDefinition),
      typeof(bool),
      typeof(bool),
      typeof(bool),
      typeof(WorldGenerationStateComponent).MakeByRefType(),
      typeof(List<TileChangeCommand>)
    ],
    modifiers: null) ??
    throw new InvalidOperationException("WavyCaves owner did not expose explicit generation guards.");

  const int width = 1200;
  const int height = 600;
  const int seed = 1456;
  WorldMetadata metadata = new(
    "wavy-caves-focused",
    new WorldSeed(seed),
    width,
    height,
    worldSurface: 50,
    rockLayer: 90);
  WorldGridSnapshot snapshot = new WorldGrid(
    width,
    height,
    initializeLegacyEmptyFrames: true).CreateSnapshot(metadata);
  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 50,
    RockLayer: 90,
    WorldSurfaceLow: 40,
    WorldSurfaceHigh: 60,
    RockLayerLow: 80,
    RockLayerHigh: 100,
    LeftBeachEnd: 100,
    RightBeachStart: 1100,
    WaterLine: 300,
    LavaLine: 500);

  foreach ((string guard, bool isSkyblockWorld, bool isDontStarveWorld) in new[]
  {
    ("Skyblock", true, true),
    ("non-dont-starve", false, false)
  })
  {
    WorldGenerationStateComponent state = new(seed);
    LegacyPassRandomState random = new(seed);
    List<TileChangeCommand> commands = new();
    object?[] invocationArguments =
    [
      snapshot,
      profile,
      random,
      LegacyWavyCavesPassDefinition.CreateDefault(),
      false,
      isSkyblockWorld,
      isDontStarveWorld,
      state,
      commands
    ];
    _ = appendCommands.Invoke(null, invocationArguments);
    state = (WorldGenerationStateComponent)invocationArguments[7]!;
    if (commands.Count != 0 || random.SampleCount != 0 ||
        state.Stage != WorldGenerationStage.Created || state.NextSequence != 0)
    {
      throw new InvalidOperationException(
        $"WavyCaves {guard} guard consumed state, commands, or random samples.");
    }
  }

  WorldGenerationStateComponent firstState = new(seed);
  WorldGenerationStateComponent secondState = new(seed);
  LegacyPassRandomState firstRandom = new(seed);
  LegacyPassRandomState secondRandom = new(seed);
  List<TileChangeCommand> firstCommands = new();
  List<TileChangeCommand> secondCommands = new();
  object?[] firstArguments =
  [
    snapshot,
    profile,
    firstRandom,
    LegacyWavyCavesPassDefinition.CreateDefault(),
    false,
    false,
    true,
    firstState,
    firstCommands
  ];
  object?[] secondArguments =
  [
    snapshot,
    profile,
    secondRandom,
    LegacyWavyCavesPassDefinition.CreateDefault(),
    false,
    false,
    true,
    secondState,
    secondCommands
  ];
  _ = appendCommands.Invoke(null, firstArguments);
  _ = appendCommands.Invoke(null, secondArguments);
  firstState = (WorldGenerationStateComponent)firstArguments[7]!;
  secondState = (WorldGenerationStateComponent)secondArguments[7]!;
  if (firstCommands.Count == 0 || !firstCommands.SequenceEqual(secondCommands) ||
      firstRandom.SampleCount != secondRandom.SampleCount ||
      firstCommands.Any(command => command.Source != "worldgen.cave.WavyCaverer"))
  {
    throw new InvalidOperationException(
      "WavyCaves did not preserve deterministic guarded source-attributed output.");
  }

  RunWavyCavesUnderworldUpperBoundContract();

  Console.WriteLine(
    $"PASS: WavyCaves preserves guard, density, Remix division, and deterministic output " +
    $"({firstCommands.Count} commands; {firstRandom.SampleCount} random samples)");
}

static void RunTunnelsFocusedVerification()
{
  if (LegacyTunnelsPass.CalculateInvocationCount(4200) != 6 ||
      LegacyTunnelsPass.CalculateInvocationCount(4200, isRemixWorld: true) != 9 ||
      LegacyTunnelsPass.CalculateInvocationCount(4200, isRemixWorld: true,
        isTenthAnniversaryWorld: true) != 9)
  {
    throw new InvalidOperationException(
      "Tunnels density or Remix multiplier diverged from the legacy source.");
  }

  if (!LegacyTunnelsPass.IsCandidateXAccepted(1000, 4200, false, false) ||
      LegacyTunnelsPass.IsCandidateXAccepted(2100, 4200, false, false) ||
      !LegacyTunnelsPass.IsCandidateXAccepted(2100, 4200, true, false) ||
      !LegacyTunnelsPass.IsCandidateXAccepted(2100, 4200, false, true))
  {
    throw new InvalidOperationException(
      "Tunnels candidate interval or center exclusion diverged from the legacy source.");
  }

  WorldMetadata metadata = new(
    "TunnelsFocused",
    new WorldSeed(451),
    width: 1200,
    height: 600,
    worldId: 451,
    seedVariant: "default");
  WorldGrid tunnelWorld = new(
    metadata.Width,
    metadata.Height,
    initializeLegacyEmptyFrames: true);
  for (int x = 450; x < metadata.Width - 450; x++)
  {
    tunnelWorld.TrySetTile(x, 100, new WorldTile(true, 1));
  }
  WorldGridSnapshot snapshot = tunnelWorld.CreateSnapshot(metadata);
  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 120,
    WorldSurfaceLow: 110,
    WorldSurfaceHigh: 130,
    RockLayer: 210,
    RockLayerLow: 190,
    RockLayerHigh: 230,
    LeftBeachEnd: 100,
    RightBeachStart: 1100,
    WaterLine: 300,
    LavaLine: 500);
  WorldGenerationStateComponent firstState = new(451);
  WorldGenerationStateComponent secondState = new(451);
  List<TileChangeCommand> firstTiles = new();
  List<LiquidChangeCommand> firstLiquids = new();
  List<TileChangeCommand> secondTiles = new();
  List<LiquidChangeCommand> secondLiquids = new();
  LegacyTunnelsPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(451),
    isRemixWorld: false,
    isSkyblockWorld: false,
    isNoSurfaceWorld: false,
    isSurfaceDesertWorld: false,
    isTenthAnniversaryWorld: false,
    ref firstState,
    firstTiles,
    firstLiquids);
  LegacyTunnelsPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(451),
    isRemixWorld: false,
    isSkyblockWorld: false,
    isNoSurfaceWorld: false,
    isSurfaceDesertWorld: false,
    isTenthAnniversaryWorld: false,
    ref secondState,
    secondTiles,
    secondLiquids);
  if (!firstTiles.SequenceEqual(secondTiles) ||
      !firstLiquids.SequenceEqual(secondLiquids) ||
      firstTiles.Count == 0 ||
      firstLiquids.Count != 0 ||
      firstTiles.Any(command => command.Source != "worldgen.cave.Tunnels.surface-tunnel"))
  {
    throw new InvalidOperationException(
      "Tunnels did not emit deterministic, dry, source-attributed commands.");
  }

  WorldGenerationStateComponent deniedState = new(451);
  List<TileChangeCommand> deniedTiles = new();
  List<LiquidChangeCommand> deniedLiquids = new();
  LegacyTunnelsPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(451),
    isRemixWorld: false,
    isSkyblockWorld: true,
    isNoSurfaceWorld: false,
    isSurfaceDesertWorld: false,
    isTenthAnniversaryWorld: false,
    ref deniedState,
    deniedTiles,
    deniedLiquids);
  if (deniedTiles.Count != 0 || deniedLiquids.Count != 0 || deniedState.NextSequence != 0)
  {
    throw new InvalidOperationException("Tunnels Skyblock guard consumed state or commands.");
  }

  Console.WriteLine(
    $"PASS: Tunnels preserves density, center guard, deterministic dry commands " +
    $"({firstTiles.Count} commands)");
}

static void RunOceanSandFocusedVerification()
{
  const int width = 800;
  const int height = 300;
  const int seed = 1456;
  WorldMetadata metadata = new(
    "OceanSandFocused",
    new WorldSeed(seed),
    width,
    height,
    worldId: seed,
    seedVariant: "default");
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  for (int x = 0; x < width; x++)
  {
    _ = world.TrySetTile(x, 30, new WorldTile(IsActive: true, Type: 1));
  }

  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 50,
    RockLayer: 90,
    WorldSurfaceLow: 40,
    WorldSurfaceHigh: 60,
    RockLayerLow: 80,
    RockLayerHigh: 100,
    LeftBeachEnd: 100,
    RightBeachStart: 700,
    WaterLine: 180,
    LavaLine: 240);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  LegacyOceanSandPassResult first = LegacyOceanSandPass.CreatePlan(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isSkyblockWorld: false,
    isNoSurfaceWorld: false);
  LegacyOceanSandPassResult second = LegacyOceanSandPass.CreatePlan(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isSkyblockWorld: false,
    isNoSurfaceWorld: false);
  if (first.Skipped || first.Bands.Count != 3 ||
      !first.Bands.SequenceEqual(second.Bands) ||
      !first.Columns.SequenceEqual(second.Columns) ||
      !first.PyramidCandidates.SequenceEqual(second.PyramidCandidates) ||
      first.RandomSamplesConsumed != second.RandomSamplesConsumed)
  {
    throw new InvalidOperationException(
      "OceanSand did not preserve deterministic band, column, and pyramid scheduling.");
  }

  LegacyOceanSandBandPlan leftBand = first.Bands[0];
  LegacyOceanSandBandPlan centerBand = first.Bands[1];
  LegacyOceanSandBandPlan rightBand = first.Bands[2];
  if (leftBand.Left != 0 || leftBand.RightExclusive != profile.LeftBeachEnd ||
      rightBand.Left != profile.RightBeachStart || rightBand.RightExclusive != width ||
      !centerBand.IsSkipped ||
      IsOceanSandCentralCandidate(leftBand.CandidateX, width) ||
      IsOceanSandCentralCandidate(rightBand.CandidateX, width) ||
      first.Columns.Count != profile.LeftBeachEnd + (width - profile.RightBeachStart))
  {
    throw new InvalidOperationException(
      "OceanSand did not preserve forced beach ranges, center skip, or candidate exclusion.");
  }

  if (first.Columns.Any(column => column.FirstActiveY != 30 ||
      column.Depth is < 50 or > 200 ||
      column.CarveDepth < 0 || column.RandomDrawCount <= 0))
  {
    throw new InvalidOperationException(
      "OceanSand did not preserve first-active scans, depth clamping, or column random draws.");
  }

  if (first.TileWrites.Count == 0 || !first.TileWrites.SequenceEqual(second.TileWrites) ||
      first.TileWrites.Any(write =>
        write.X < 0 || write.X >= width || write.Y < 0 || write.Y >= height))
  {
    throw new InvalidOperationException(
      "OceanSand did not preserve deterministic in-bounds type-53 tile writes.");
  }

  if (first.PyramidCandidates.Any(candidate =>
      candidate.X is not 50 and not 750 || candidate.Y != 30))
  {
    throw new InvalidOperationException(
      "OceanSand pyramid candidates were not tied to a band midpoint and first active tile.");
  }

  WorldGenerationStateComponent projectionState = new(generationId: 2);
  List<TileChangeCommand> projectedCommands = new();
  LegacyOceanSandPassResult projected = LegacyOceanSandPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isSkyblockWorld: false,
    isNoSurfaceWorld: false,
    ref projectionState,
    projectedCommands);
  if (!projected.TileWrites.SequenceEqual(first.TileWrites) ||
      projectedCommands.Count != projected.TileWrites.Count ||
      projectedCommands.Any(command =>
        command.Kind != TileChangeKind.UpdateTileType ||
        command.TileType != 53 ||
        command.Source != "worldgen.ocean-sand.type-53"))
  {
    throw new InvalidOperationException(
      "OceanSand did not emit source-attributed type-53 tile commands.");
  }

  WorldGrid projectedWorld = WorldGrid.FromSnapshot(snapshot);
  TileChangeCommitSystem commitSystem = new();
  if (!commitSystem.TryCommit(
        projectedWorld,
        projectedCommands,
        out TileChangeCommitResult commitResult) ||
      commitResult.AppliedCount != projectedCommands.Count ||
      projected.TileWrites.Any(write => projectedWorld.GetTile(write.X, write.Y).Type != 53))
  {
    throw new InvalidOperationException(
      "OceanSand type-53 commands did not commit to the projected world grid.");
  }

  LegacyOceanSandPassResult skyblock = LegacyOceanSandPass.CreatePlan(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isSkyblockWorld: true,
    isNoSurfaceWorld: false);
  LegacyOceanSandPassResult noSurface = LegacyOceanSandPass.CreatePlan(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isSkyblockWorld: false,
    isNoSurfaceWorld: true);
  if (!skyblock.Skipped || !noSurface.Skipped ||
      skyblock.Bands.Count != 0 || skyblock.Columns.Count != 0 ||
      noSurface.Bands.Count != 0 || noSurface.Columns.Count != 0 ||
      skyblock.RandomSamplesConsumed != 0 || noSurface.RandomSamplesConsumed != 0)
  {
    throw new InvalidOperationException(
      "OceanSand guards consumed random state or produced scheduling output.");
  }

  Console.WriteLine(
    $"PASS: OceanSand preserves source guards, three-band scheduling, beach forcing, " +
    $"column depth state, and deterministic pyramid signals ({first.Columns.Count} columns; " +
    $"{first.RandomSamplesConsumed} random samples)");
  Console.WriteLine(
    "SUMMARY: OceanSand tile-type projection, TileRunner-equivalent mutation, pyramid structure " +
    "placement, aggregate pass ordering, global RNG/WLD parity, and legacy deletion remain deferred");
}

static bool IsOceanSandCentralCandidate(int candidateX, int width)
{
  return (double)candidateX > width * 0.4 && (double)candidateX < width * 0.6;
}

static void RunSandPatchesFocusedVerification()
{
  const int width = 1000;
  const int height = 900;
  const int seed = 122;
  WorldMetadata metadata = new(
    "sand-patches-focused",
    new WorldSeed(seed),
    width,
    height,
    worldId: seed,
    worldSurface: 120,
    rockLayer: 300,
    isRemixWorld: false);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 120,
    RockLayer: 300,
    WorldSurfaceLow: 80,
    WorldSurfaceHigh: 140,
    RockLayerLow: 260,
    RockLayerHigh: 340,
    LeftBeachEnd: 100,
    RightBeachStart: 900,
    WaterLine: 400,
    LavaLine: 700);
  LegacySandPatchesPassDefinition definition =
    LegacySandPatchesPassDefinition.CreateDefault();
  if (definition.Density != 0.013 || definition.RemixInvocationDivisor != 4 ||
      definition.MinimumStrength != 15 || definition.MaximumStrengthExclusive != 70 ||
      definition.MinimumSteps != 20 || definition.MaximumStepsExclusive != 130 ||
      definition.TileType != 53 ||
      definition.CalculateInvocationCount(width, isRemixWorld: false) != 13 ||
      definition.CalculateInvocationCount(width, isRemixWorld: true) != 3)
  {
    throw new InvalidOperationException(
      "SandPatches definition or source invocation cardinality drifted.");
  }

  if (definition.GetInitialYRange(height, profile, isRemixWorld: false) != (120, 300) ||
      definition.GetInitialYRange(height, profile, isRemixWorld: true) != (200, 550) ||
      definition.GetRetryYRange(profile) != (120, 300))
  {
    throw new InvalidOperationException(
      "SandPatches default or Remix Y ranges drifted from the source pass.");
  }

  LegacyPassRandomState defaultRandom = new(seed);
  IReadOnlyList<LegacyTileRunnerPassInvocation> defaultInvocations =
    LegacySandPatchesPass.CreateInvocations(
      snapshot,
      profile,
      defaultRandom,
      isRemixWorld: false,
      isSkyblockWorld: false);
  LegacyPassRandomState secondDefaultRandom = new(seed);
  IReadOnlyList<LegacyTileRunnerPassInvocation> secondDefaultInvocations =
    LegacySandPatchesPass.CreateInvocations(
      snapshot,
      profile,
      secondDefaultRandom,
      isRemixWorld: false,
      isSkyblockWorld: false);
  if (defaultInvocations.Count != 13 ||
      !defaultInvocations.SequenceEqual(secondDefaultInvocations) ||
      defaultRandom.SampleCount != secondDefaultRandom.SampleCount ||
      defaultRandom.SampleCount !=
        defaultInvocations.Sum(invocation => invocation.RandomDrawCount) ||
      defaultInvocations[0].XDraw != 44 || defaultInvocations[0].YDraw != 227 ||
      defaultInvocations[0].RandomDrawCount != 6 ||
      !defaultInvocations.Any(invocation => invocation.RandomDrawCount > 4))
  {
    throw new InvalidOperationException(
      "SandPatches default scheduling did not preserve deterministic retry draw accounting.");
  }

  LegacyTileRunnerPassInput expectedDefaultRecipe = new(
    "SandPatches",
    "sand-patch",
    53,
    false,
    15,
    70,
    20,
    130,
    "worldSurface..rockLayer",
    true,
    true,
    true);
  if (defaultInvocations.Any(invocation => invocation.Recipe != expectedDefaultRecipe))
  {
    throw new InvalidOperationException(
      "SandPatches default invocation recipe did not preserve the TileRunner contract.");
  }

  foreach (LegacyTileRunnerPassInvocation invocation in defaultInvocations)
  {
    LegacyTileRunnerRequest request = invocation.Request;
    if (!metadata.IsInside(request.X, request.Y) ||
        request.TileType != 53 || request.AddTile || request.SpeedX != 0.0 ||
        request.SpeedY != 0.0 || request.NoYChange || !request.Overwrite ||
        request.IgnoreTileType != -1 || request.Strength is < 15 or >= 70 ||
        request.Steps is < 20 or >= 130 || invocation.RandomDrawCount < 4)
    {
      throw new InvalidOperationException(
        "SandPatches default request fields or bounds diverged from the source call.");
    }
  }

  LegacyPassRandomState remixRandom = new(seed);
  IReadOnlyList<LegacyTileRunnerPassInvocation> remixInvocations =
    LegacySandPatchesPass.CreateInvocations(
      snapshot,
      profile,
      remixRandom,
      isRemixWorld: true,
      isSkyblockWorld: false);
  long remixExpectedSamples = remixInvocations.Sum(invocation => invocation.RandomDrawCount);
  if (remixInvocations.Count != 3 ||
      remixRandom.SampleCount != remixExpectedSamples ||
      remixInvocations[0].XDraw != 598 || remixInvocations[0].YDraw != 171 ||
      remixInvocations[0].RandomDrawCount != 7 ||
      remixInvocations[0].YDraw >= 200 ||
      remixInvocations[1].YDraw is < 200 or >= 550 ||
      remixInvocations[2].YDraw is < 200 or >= 550 ||
      remixInvocations.Any(invocation => invocation.Recipe != expectedDefaultRecipe with
      {
        VerticalRange = "rockLayer-100..maxTilesY-350"
      }))
  {
    throw new InvalidOperationException(
      $"SandPatches Remix scheduling drifted: count={remixInvocations.Count}, " +
      $"samples={remixRandom.SampleCount}, expectedSamples={remixExpectedSamples}, first=" +
      $"({remixInvocations[0].XDraw},{remixInvocations[0].YDraw}," +
      $"{remixInvocations[0].RandomDrawCount}), secondY={remixInvocations[1].YDraw}, " +
      $"thirdY={remixInvocations[2].YDraw}, recipe={remixInvocations[0].Recipe.VerticalRange}.");
  }

  WorldGenerationStateComponent firstState = new(seed);
  List<TileChangeCommand> firstCommands = new();
  LegacySandPatchesPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref firstState,
    firstCommands);
  if (firstCommands.Any(command =>
        !metadata.IsInside(command.X, command.Y) || command.TileType != 53 ||
        command.Source != "worldgen.cave.SandPatches.sand-patch"))
  {
    throw new InvalidOperationException(
      "SandPatches did not emit bounded source-attributed TileRunner commands.");
  }

  WorldGenerationStateComponent secondState = new(seed);
  List<TileChangeCommand> secondCommands = new();
  LegacySandPatchesPass.AppendCommands(
    snapshot,
    profile,
    new LegacyPassRandomState(seed),
    isRemixWorld: false,
    isSkyblockWorld: false,
    ref secondState,
    secondCommands);
  if (!firstCommands.SequenceEqual(secondCommands) ||
      firstState.Stage != WorldGenerationStage.Cave ||
      firstState.NextSequence != secondState.NextSequence)
  {
    throw new InvalidOperationException(
      "SandPatches command emission was not deterministic from the immutable snapshot.");
  }

  WorldGrid commitWorld = WorldGrid.FromSnapshot(snapshot);
  if (!new TileChangeCommitSystem().TryCommit(
        commitWorld,
        firstCommands,
        out TileChangeCommitResult commitResult) ||
      commitResult.AppliedCount != firstCommands.Count)
  {
    throw new InvalidOperationException(
      "SandPatches TileRunner commands did not commit through the typed tile boundary.");
  }

  LegacyPassRandomState deniedRandom = new(seed);
  WorldGenerationStateComponent deniedState = new(seed);
  List<TileChangeCommand> deniedCommands = new();
  LegacySandPatchesPass.AppendCommands(
    snapshot,
    profile,
    deniedRandom,
    isRemixWorld: false,
    isSkyblockWorld: true,
    ref deniedState,
    deniedCommands);
  if (deniedCommands.Count != 0 || deniedRandom.SampleCount != 0 ||
      deniedState.NextSequence != 0)
  {
    throw new InvalidOperationException(
      "SandPatches Skyblock guard consumed random state or emitted commands.");
  }

  Console.WriteLine(
    $"PASS: SandPatches preserves source guard, count, default/Remix Y ranges, central retry, " +
    $"TileRunner requests, and bounded commands ({defaultInvocations.Count} default invocations; " +
    $"{defaultRandom.SampleCount} random samples; {firstCommands.Count} commands)");
  Console.WriteLine(
    "SUMMARY: SandPatches full TileRunner traversal parity, aggregate ordering, global " +
    "RNG/WLD parity, and legacy deletion remain deferred");
}

static void RunUnderworldEvilBoundsFocusedVerification()
{
  AssertThrows<ArgumentOutOfRangeException>(() =>
    new LegacyTileRunnerRequest(-1, 10, 5.0, 1, -2, false, 0.0, 0.0, false, false, -1));
  LegacyUnderworldLiquidCaveOrigin origin = new(
    X: 0,
    Y: 200,
    SurfaceRunnersAllowed: true);
  IReadOnlyList<LegacyTileRunnerRequest> requests =
    LegacyUnderworldEvilRunnerFactory.Create(origin, new LegacyPassRandomState(1));
  if (requests.Count == 0 || requests[0].X >= 0 || requests[0].Y < 0)
  {
    throw new InvalidOperationException(
      "Underworld evil runner did not retain the source's out-of-world start coordinate.");
  }

  Console.WriteLine(
    $"PASS: Underworld evil runner retains source out-of-world starts without throwing " +
    $"({requests.Count} requests)");
}

static void RunWavyCavesUnderworldUpperBoundContract()
{
  const int width = 1200;
  const int height = 600;
  const int seed = 451;
  WorldMetadata metadata = new(
    "wavy-caves-underworld-bound",
    new WorldSeed(seed),
    width,
    height,
    worldSurface: 50,
    rockLayer: 90);
  WorldGridSnapshot snapshot = new WorldGrid(
    width,
    height,
    initializeLegacyEmptyFrames: true).CreateSnapshot(metadata);
  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 50,
    RockLayer: 90,
    WorldSurfaceLow: 40,
    WorldSurfaceHigh: 60,
    RockLayerLow: 80,
    RockLayerHigh: 100,
    LeftBeachEnd: 100,
    RightBeachStart: 1100,
    WaterLine: 300,
    LavaLine: 500);
  LegacyWavyCavesPassDefinition definition = LegacyWavyCavesPassDefinition.CreateDefault();
  WorldGenerationStateComponent expectedState = new(seed);
  LegacyPassRandomState expectedRandom = new(seed);
  List<TileChangeCommand> expectedCommands = new();
  AppendSourceWavyCavesCommands(
    snapshot,
    profile,
    expectedRandom,
    definition,
    ref expectedState,
    expectedCommands);
  WorldGenerationStateComponent actualState = new(seed);
  LegacyPassRandomState actualRandom = new(seed);
  List<TileChangeCommand> actualCommands = new();
  LegacyWavyCavesPass.AppendCommands(
    snapshot,
    profile,
    actualRandom,
    definition,
    isRemixWorld: false,
    isSkyblockWorld: false,
    isDontStarveWorld: true,
    ref actualState,
    actualCommands);
  if (!expectedCommands.SequenceEqual(actualCommands) ||
      expectedRandom.SampleCount != actualRandom.SampleCount)
  {
    throw new InvalidOperationException(
      "WavyCaves did not use Main.UnderworldLayer - 100 as its Y draw upper bound.");
  }

  Console.WriteLine("PASS: WavyCaves Y draws stop at legacy UnderworldLayer - 100");
}

static void AppendSourceWavyCavesCommands(
  WorldGridSnapshot snapshot,
  LegacyTerrainRuntimeProfile profile,
  LegacyPassRandomState random,
  LegacyWavyCavesPassDefinition definition,
  ref WorldGenerationStateComponent state,
  List<TileChangeCommand> commands)
{
  int invocationCount = definition.CalculateInvocationCount(snapshot.Metadata.Width, false);
  int underworldLayer = snapshot.Metadata.Height - 200;
  int minimumY = (int)profile.WorldSurface + definition.MinimumYInset;
  int maximumYExclusive = underworldLayer - definition.UnderworldYInset;
  int previousY = 0;
  for (int index = 0; index < invocationCount; index++)
  {
    double progress = index / (double)(invocationCount - 1);
    int startY = random.Next(minimumY, maximumYExclusive);
    int retries = 0;
    while (Math.Abs(startY - previousY) < definition.MinimumSpacing)
    {
      retries++;
      if (retries > 100)
      {
        break;
      }

      startY = random.Next(minimumY, maximumYExclusive);
    }

    previousY = startY;
    int startX = definition.MinimumStartXInset + (int)(
      (snapshot.Metadata.Width - definition.MinimumStartXInset * 2) * progress);
    LegacyWavyCaverer.AppendCommands(
      snapshot,
      new LegacyWavyCavererInvocation(
        startX,
        startY,
        12 + random.Next(3, 6),
        0.25 + random.NextDouble(),
        random.Next(300, 500),
        -1),
      random,
      ref state,
      commands);
  }
}

static void RunSurfaceCavesFocusedVerification()
{
  RunSurfaceCavesUnifiedOwnerContract();
  RunSurfaceCavesProjectedSurfaceScanContract();

  const int width = 800;
  const int height = 300;
  const int seed = 1456;
  WorldMetadata metadata = new(
    "surface-caves-focused",
    new WorldSeed(seed),
    width,
    height,
    isRemixWorld: false);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  for (int x = 0; x < width; x++)
  {
    for (int y = 0; y < 120; y++)
    {
      if (!world.TrySetTile(x, y, new WorldTile(IsActive: true, Type: 1)))
      {
        throw new InvalidOperationException(
          "SurfaceCaves focused fixture could not seed its active surface.");
      }
    }
  }

  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 50.0,
    RockLayer: 90.0,
    WorldSurfaceLow: 40.0,
    WorldSurfaceHigh: 60.0,
    RockLayerLow: 80.0,
    RockLayerHigh: 100.0,
    LeftBeachEnd: 100,
    RightBeachStart: 700,
    WaterLine: 180,
    LavaLine: 240);
  WorldGenerationRequest request = new(
    metadata,
    spawnX: width / 2,
    surfaceY: 50,
    rockLayerY: 90,
    terrainProfile: profile);
  WorldGenerationStateComponent state = new(seed);
  List<TileChangeCommand> commands = new();
  new LegacyCavePassSystem().AppendCommands(
    world.CreateSnapshot(metadata),
    request,
    ref state,
    commands);
  const string genericSource = "worldgen.cave.SurfaceCaves.surface-desert";
  if (commands.Any(command => StringComparer.Ordinal.Equals(command.Source, genericSource)))
  {
    throw new InvalidOperationException(
      "Profile-enabled SurfaceCaves emitted the stale generic surface-desert fallback.");
  }

  List<TileChangeCommand> dedicatedCommands = new();
  WorldGenerationStateComponent dedicatedState = new(seed);
  LegacySurfaceCavesVerticalPass.AppendCommands(
    world.CreateSnapshot(metadata),
    profile,
    new LegacyPassRandomState(seed),
    ref dedicatedState,
    dedicatedCommands);
  if (!dedicatedCommands.Any(command =>
        command.Source.StartsWith(
          "worldgen.cave.SurfaceCaves.vertical-",
          StringComparison.Ordinal)))
  {
    throw new InvalidOperationException(
      "SurfaceCaves dedicated vertical owner did not emit source-attributed commands.");
  }

  WorldGenerationRequest noProfileRequest = new(
    metadata,
    spawnX: width / 2,
    surfaceY: 50,
    rockLayerY: 90);
  List<TileChangeCommand> noProfileCommands = new();
  WorldGenerationStateComponent noProfileState = new(seed);
  new LegacyCavePassSystem().AppendCommands(
    world.CreateSnapshot(metadata),
    noProfileRequest,
    ref noProfileState,
    noProfileCommands);
  if (!noProfileCommands.Any(command =>
        StringComparer.Ordinal.Equals(command.Source, genericSource)))
  {
    throw new InvalidOperationException(
      "No-profile SurfaceCaves unexpectedly lost its generic compatibility fallback.");
  }

  Console.WriteLine(
    $"PASS: profile-enabled SurfaceCaves excludes generic fallback ({commands.Count} commands); " +
    $"no-profile fallback remains ({noProfileCommands.Count} commands)");
  Console.WriteLine(
    "SUMMARY: SurfaceCaves generic-owner exclusion remains pass-specific; dedicated vertical, " +
    "Caverer, Mountain, complete TileRunner parity, aggregate ordering, WLD differential, and " +
    "legacy deletion remain deferred");
}

static void RunSurfaceCavesProjectedSurfaceScanContract()
{
  const int width = 600;
  const int height = 150;
  const int surfaceX = 120;
  const int firstActiveY = 40;
  const int secondActiveY = 41;
  const int maximumYExclusive = 60;
  WorldMetadata metadata = new(
    "surface-caves-projected-scan",
    new WorldSeed(1456),
    width,
    height,
    worldSurface: 50,
    rockLayer: 90);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  _ = world.TrySetTile(surfaceX, firstActiveY, new WorldTile(IsActive: true, Type: 1));
  _ = world.TrySetTile(surfaceX, secondActiveY, new WorldTile(IsActive: true, Type: 1));
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  Dictionary<(int X, int Y), WorldTile> projectedTiles = new()
  {
    [(surfaceX, firstActiveY)] = new WorldTile(
      IsActive: false,
      Type: 0,
      FrameX: -1,
      FrameY: -1)
  };
  MethodInfo findSurfaceY = typeof(LegacySurfaceCavesVerticalPass).GetMethod(
    "FindSurfaceY",
    BindingFlags.NonPublic | BindingFlags.Static,
    binder: null,
    types:
    [
      typeof(WorldGridSnapshot),
      typeof(int),
      typeof(int),
      typeof(IDictionary<(int X, int Y), WorldTile>)
    ],
    modifiers: null) ??
    throw new InvalidOperationException(
      "SurfaceCaves FindSurfaceY did not expose the projected-tile scan boundary.");
  int projectedSurfaceY = (int)(findSurfaceY.Invoke(
    null,
    [snapshot, surfaceX, maximumYExclusive, projectedTiles]) ??
    throw new InvalidOperationException(
      "SurfaceCaves FindSurfaceY returned no projected scan result."));
  if (projectedSurfaceY != secondActiveY)
  {
    throw new InvalidOperationException(
      $"SurfaceCaves FindSurfaceY ignored projected tile state: expected {secondActiveY}, " +
      $"got {projectedSurfaceY}.");
  }

  Console.WriteLine(
    "PASS: SurfaceCaves FindSurfaceY observes projected tile state before scanning snapshot");
}

static void RunSurfaceCavesUnifiedOwnerContract()
{
  const int width = 4200;
  const int height = 1200;
  const int seed = 1456;
  WorldMetadata metadata = new(
    "surface-caves-unified-owner",
    new WorldSeed(seed),
    width,
    height,
    worldSurface: 50,
    rockLayer: 90,
    isRemixWorld: false);
  WorldGrid world = new(width, height, initializeLegacyEmptyFrames: true);
  for (int x = 0; x < width; x++)
  {
    _ = world.TrySetTile(x, 40, new WorldTile(IsActive: true, Type: 1));
  }

  LegacyTerrainRuntimeProfile profile = new(
    WorldSurface: 50,
    RockLayer: 90,
    WorldSurfaceLow: 40,
    WorldSurfaceHigh: 60,
    RockLayerLow: 80,
    RockLayerHigh: 100,
    LeftBeachEnd: 300,
    RightBeachStart: 3900,
    WaterLine: 700,
    LavaLine: 1000);
  WorldGridSnapshot snapshot = world.CreateSnapshot(metadata);
  string inputFingerprint = CreateSnapshotWorldGridFingerprint(snapshot);
  WorldGenerationStateComponent firstState = new(seed);
  WorldGenerationStateComponent secondState = new(seed);
  LegacyPassRandomState firstRandom = new(seed);
  LegacyPassRandomState secondRandom = new(seed);
  List<TileChangeCommand> firstCommands = new();
  List<TileChangeCommand> secondCommands = new();
  List<LiquidChangeCommand> firstLiquids = new();
  List<LiquidChangeCommand> secondLiquids = new();
  LegacySurfaceCavesPass.AppendCommands(
    snapshot,
    profile,
    firstRandom,
    isRemixWorld: false,
    isSkyblockWorld: false,
    isNoSurfaceWorld: false,
    ref firstState,
    firstCommands,
    firstLiquids);
  LegacySurfaceCavesPass.AppendCommands(
    snapshot,
    profile,
    secondRandom,
    isRemixWorld: false,
    isSkyblockWorld: false,
    isNoSurfaceWorld: false,
    ref secondState,
    secondCommands,
    secondLiquids);
  LegacySurfaceCavesPassDefinition definition =
    LegacySurfaceCavesPassDefinitionFactory.CreateDefault();
  bool commandCountValid = firstCommands.Count > 0;
  bool deterministicCommands = firstCommands.SequenceEqual(secondCommands);
  bool deterministicLiquids = firstLiquids.SequenceEqual(secondLiquids);
  bool deterministicRandom = firstRandom.SampleCount == secondRandom.SampleCount;
  bool sourcePrefixValid = firstCommands.All(command =>
    command.Source.StartsWith("worldgen.cave.SurfaceCaves.", StringComparison.Ordinal));
  bool hasNarrow = firstCommands.Any(command =>
    command.Source.StartsWith("worldgen.cave.SurfaceCaves.vertical-narrow", StringComparison.Ordinal));
  bool hasCaverer = firstCommands.Any(command =>
    command.Source.StartsWith("worldgen.cave.SurfaceCaves.Caverer", StringComparison.Ordinal));
  bool duplicateCavererSource = firstCommands.Any(command =>
    command.Source.Contains("Caverer.Caverer.", StringComparison.Ordinal));
  bool snapshotUnchanged = StringComparer.Ordinal.Equals(
    inputFingerprint,
    CreateSnapshotWorldGridFingerprint(snapshot));
  int narrowCount = definition.CalculateInvocationCount(
    LegacySurfaceCavesVerticalFamily.Narrow,
    4200,
    isRemixWorld: false);
  int remixNarrowCount = definition.CalculateInvocationCount(
    LegacySurfaceCavesVerticalFamily.Narrow,
    4200,
    isRemixWorld: true);
  int remixHorizontalCount = definition.CalculateInvocationCount(
    LegacySurfaceCavesVerticalFamily.Horizontal,
    4200,
    isRemixWorld: true);
  int cavererCount = definition.CalculateCavererInvocationCount(4200);
  if (!commandCountValid ||
      !deterministicCommands ||
      !deterministicLiquids ||
      !deterministicRandom ||
       !sourcePrefixValid ||
       !hasNarrow ||
       !hasCaverer ||
       duplicateCavererSource ||
       !snapshotUnchanged ||
      narrowCount != 8 ||
      remixNarrowCount != 24 ||
      remixHorizontalCount != 1 ||
      cavererCount != 5)
  {
    Console.WriteLine(
      $"DEBUG SurfaceCaves unified: commands={firstCommands.Count}/{secondCommands.Count}, " +
      $"liquids={firstLiquids.Count}/{secondLiquids.Count}, random=" +
      $"{firstRandom.SampleCount}/{secondRandom.SampleCount}, prefix={sourcePrefixValid}, " +
      $"narrow={hasNarrow}, caverer={hasCaverer}, snapshot={snapshotUnchanged}, " +
      $"duplicateCaverer={duplicateCavererSource}, " +
      $"counts={narrowCount}/{remixNarrowCount}/{remixHorizontalCount}/{cavererCount}");
    throw new InvalidOperationException(
      "SurfaceCaves unified owner did not preserve deterministic family order, counts, or snapshot input.");
  }

  WorldGenerationStateComponent skyblockState = new(seed);
  LegacyPassRandomState skyblockRandom = new(seed);
  List<TileChangeCommand> skyblockCommands = new();
  List<LiquidChangeCommand> skyblockLiquids = new();
  LegacySurfaceCavesPass.AppendCommands(
    snapshot,
    profile,
    skyblockRandom,
    isRemixWorld: false,
    isSkyblockWorld: true,
    isNoSurfaceWorld: false,
    ref skyblockState,
    skyblockCommands,
    skyblockLiquids);
  if (skyblockCommands.Count != 0 || skyblockLiquids.Count != 0 ||
      skyblockRandom.SampleCount != 0 || skyblockState.Stage != WorldGenerationStage.Created ||
      skyblockState.NextSequence != 0)
  {
    throw new InvalidOperationException(
      "SurfaceCaves Skyblock guard consumed state, commands, or random samples.");
  }

  WorldGenerationStateComponent noSurfaceState = new(seed);
  LegacyPassRandomState noSurfaceRandom = new(seed);
  List<TileChangeCommand> noSurfaceCommands = new();
  List<LiquidChangeCommand> noSurfaceLiquids = new();
  LegacySurfaceCavesPass.AppendCommands(
    snapshot,
    profile,
    noSurfaceRandom,
    isRemixWorld: false,
    isSkyblockWorld: false,
    isNoSurfaceWorld: true,
    ref noSurfaceState,
    noSurfaceCommands,
    noSurfaceLiquids);
  if (noSurfaceCommands.Count != 0 || noSurfaceLiquids.Count != 0 ||
      noSurfaceRandom.SampleCount != 0 || noSurfaceState.Stage != WorldGenerationStage.Created ||
      noSurfaceState.NextSequence != 0)
  {
    throw new InvalidOperationException(
      "SurfaceCaves no-surface guard consumed state, commands, or random samples.");
  }

  Console.WriteLine(
    $"PASS: SurfaceCaves unified owner preserves source order, guards, and deterministic replay " +
    $"({firstCommands.Count} commands; {firstLiquids.Count} liquids; " +
    $"{firstRandom.SampleCount} random samples)");
}

static string FindRepositoryRoot()
{
  DirectoryInfo? directory = new(AppContext.BaseDirectory);
  while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "AGENTS.md")))
  {
    directory = directory.Parent;
  }

  return directory?.FullName ?? throw new InvalidOperationException(
    "Repository root was not found.");
}

internal sealed record WorldgenInventory(
  string SourceName,
  string SourcePath,
  FileEvidence Source,
  IReadOnlyList<MethodInventory> Methods,
  IReadOnlyList<FieldInventory> Fields,
  IReadOnlyList<ReferenceInventory> References,
  ReplayEvidence Replay);

internal sealed record FileEvidence(long Bytes, int Lines, string Sha256, string ScannerVersion);

internal sealed record MethodInventory(
  string Name,
  string Visibility,
  int Line,
  string Domain,
  string Status,
  IReadOnlyList<string> References,
  SourceMapping? Mapping);

internal sealed record SourceMapping(
  string Status,
  IReadOnlyList<string> TargetMembers,
  string ImplementedScope,
  IReadOnlyList<string> ExcludedLegacyResponsibilities,
  string Verification);

internal sealed record FieldInventory(
  string Name,
  string Visibility,
  int Line,
  bool IsStatic,
  string Domain,
  string Status,
  IReadOnlyList<string> References);

internal sealed record ReferenceInventory(string Name, int Count, string Status);

internal sealed record ReplayEvidence(
  int Width,
  int Height,
  int Seed,
  int SpawnX,
  int SurfaceY,
  string SnapshotFingerprint,
  IReadOnlyDictionary<string, long> SectionVersions);

internal sealed record LegacyOracleEvidence(
  string Path,
  long Bytes,
  string Sha256,
  int Version,
  string FormatVersion,
  LegacyWorldMetadata Metadata,
  int TileCount,
  int LiquidTileCount,
  string Fingerprint);

internal sealed record CursorRestartEvidence(
  string CheckpointStage,
  string ResumedStage,
  int SectionX,
  int SectionY,
  uint RandomState,
  string SnapshotFingerprint,
  long UninterruptedNextSequence,
  long RestartedNextSequence);

internal sealed record LegacyDifferentialEvidence(
  string LegacyPath,
  int Width,
  int Height,
  int SpawnX,
  int SurfaceY,
  string SpawnSource,
  string SurfaceYSource,
  int LegacyMetadataSpawnX,
  int LegacyMetadataSpawnY,
  int LegacyWorldSurfaceY,
  int LegacyRockLayerY,
  string? DirtWallOffsetArtifactPath,
  string? DirtWallOffsetArtifactSha256,
  int? DirtWallOffsetCount,
  int EcsInputRockLayerY,
  int ComparedTiles,
  int MismatchTiles,
  IReadOnlyDictionary<string, int> MismatchFieldCounts,
  int ExtendedStateMismatchTiles,
  IReadOnlyDictionary<string, int> ExtendedStateMismatchFieldCounts,
  int ActiveLegacyTiles,
  int ActiveGeneratedTiles,
  string LegacyFingerprint,
  string LegacyProjectedFingerprint,
  string GeneratedFingerprint,
  IReadOnlyList<LegacySectionDifference> Sections,
  IReadOnlyList<LegacySpatialRegionDifference> Regions);

internal sealed record LegacySectionDifference(
  int SectionX,
  int SectionY,
  int MismatchTiles,
  int ExtendedStateMismatchTiles,
  IReadOnlyDictionary<string, int> MismatchFieldCounts);

internal sealed record LegacySpatialRegionDifference(
  string Name,
  int StartY,
  int EndExclusiveY,
  int ComparedTiles,
  int MismatchTiles,
  IReadOnlyDictionary<string, int> MismatchFieldCounts);

internal sealed class LegacySpatialRegionAccumulator
{
  public LegacySpatialRegionAccumulator(
    string name,
    int startY,
    int endExclusiveY,
    Dictionary<string, int> mismatchFieldCounts)
  {
    Name = name;
    StartY = startY;
    EndExclusiveY = endExclusiveY;
    MismatchFieldCounts = mismatchFieldCounts;
  }

  public int ComparedTiles { get; set; }

  public int EndExclusiveY { get; }

  public int MismatchTiles { get; set; }

  public Dictionary<string, int> MismatchFieldCounts { get; }

  public string Name { get; }

  public int StartY { get; }

  public LegacySpatialRegionDifference ToDifference()
  {
    return new LegacySpatialRegionDifference(
      Name,
      StartY,
      EndExclusiveY,
      ComparedTiles,
      MismatchTiles,
      MismatchFieldCounts);
  }
}
