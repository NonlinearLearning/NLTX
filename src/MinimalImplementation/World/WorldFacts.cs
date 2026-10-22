using System.Security.Cryptography;

namespace MinimalImplementation.Server.World;

/// <summary>
/// Immutable Version4 tile facts used by every world projection.
/// </summary>
public sealed class WorldFacts
{
  private static readonly int[] FrameImportantTileIds =
  [
    3, 4, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 24, 26, 27, 28,
    29, 31, 33, 34, 35, 36, 42, 49, 50, 55, 61, 71, 72, 73, 74, 77, 78, 79,
    81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98,
    99, 100, 101, 102, 103, 104, 105, 106, 110, 113, 114, 125, 126, 128, 129,
    132, 133, 134, 135, 136, 137, 138, 139, 141, 142, 143, 144, 149, 165, 171,
    172, 173, 174, 178, 184, 185, 186, 187, 201, 207, 209, 210, 212, 215, 216,
    217, 218, 219, 220, 227, 228, 231, 233, 235, 236, 237, 238, 239, 240, 241,
    242, 243, 244, 245, 246, 247, 254, 269, 270, 271, 275, 276, 277, 278, 279,
    280, 281, 282, 283, 285, 286, 287, 288, 289, 290, 291, 292, 293, 294, 295,
    296, 297, 298, 299, 300, 301, 302, 303, 304, 305, 306, 307, 308, 309, 310,
    314, 316, 317, 318, 319, 320, 323, 324, 334, 335, 337, 338, 339, 349, 354,
    355, 356, 358, 359, 360, 361, 362, 363, 364, 372, 373, 374, 375, 376, 377,
    378, 380, 386, 387, 388, 389, 390, 391, 392, 393, 394, 395, 405, 406, 410,
    411, 412, 413, 414, 419, 420, 423, 424, 425, 427, 428, 429, 435, 436, 437,
    438, 439, 440, 441, 442, 443, 444, 445, 452, 453, 454, 455, 456, 457, 461,
    462, 463, 464, 465, 466, 467, 468, 469, 470, 471, 475, 476, 480, 484, 485,
    486, 487, 488, 489, 490, 491, 493, 494, 497, 499, 505, 506, 509, 510, 511,
    518, 519, 520, 521, 522, 523, 524, 525, 526, 527, 529, 530, 531, 532, 533,
    538, 542, 543, 544, 545, 547, 548, 549, 550, 551, 552, 553, 554, 555, 556,
    558, 559, 560, 564, 565, 567, 568, 569, 570, 571, 572, 573, 579, 580, 581,
    582, 583, 584, 585, 586, 587, 588, 589, 590, 591, 592, 593, 594, 595, 596,
    597, 598, 599, 600, 601, 602, 603, 604, 605, 606, 607, 608, 609, 610, 611,
    612, 613, 614, 615, 616, 617, 619, 620, 621, 622, 623, 624, 629, 630, 631,
    632, 634, 637, 639, 640, 642, 643, 644, 645, 646, 647, 648, 649, 650, 651,
    652, 653, 654, 656, 657, 658, 660, 663, 664, 665, 693, 694, 695, 696, 698,
    699, 700, 701, 702, 703, 704, 705, 706, 707, 709, 710, 711, 712, 713, 714,
    715, 716, 720, 721, 723, 724, 725, 726, 733, 751, 752
  ];

  private static readonly int[] SolidTileIds =
  [
    0, 1, 2, 6, 7, 8, 9, 10, 19, 22, 23, 25, 30, 37, 38, 39, 40, 41, 43, 44,
    45, 46, 47, 48, 53, 54, 56, 57, 58, 59, 60, 63, 64, 65, 66, 67, 68, 70,
    75, 76, 107, 108, 109, 111, 112, 116, 117, 118, 119, 120, 121, 122, 123,
    127, 130, 137, 138, 140, 145, 146, 147, 148, 150, 151, 152, 153, 154, 155,
    156, 157, 158, 159, 160, 161, 162, 163, 164, 166, 167, 168, 169, 170, 175,
    176, 177, 179, 180, 181, 182, 183, 188, 189, 190, 191, 192, 193, 194, 195,
    196, 197, 198, 199, 200, 202, 203, 204, 206, 208, 211, 221, 222, 223, 224,
    225, 226, 229, 230, 232, 234, 235, 239, 248, 249, 250, 251, 252, 253, 255,
    256, 257, 258, 259, 260, 261, 262, 263, 264, 265, 266, 267, 268, 272, 273,
    274, 284, 311, 312, 313, 315, 321, 322, 325, 326, 327, 328, 329, 345, 346,
    347, 348, 350, 357, 367, 368, 369, 370, 371, 379, 380, 381, 383, 384, 385,
    387, 388, 396, 397, 398, 399, 400, 401, 402, 403, 404, 407, 408, 409, 415,
    416, 417, 418, 421, 422, 426, 427, 430, 431, 432, 433, 434, 435, 436, 437,
    438, 439, 446, 447, 448, 458, 459, 460, 472, 473, 474, 476, 477, 478, 479,
    481, 482, 483, 484, 492, 495, 496, 498, 500, 501, 502, 503, 507, 508, 512,
    513, 514, 515, 516, 517, 534, 535, 536, 537, 539, 540, 541, 546, 557, 562,
    563, 566, 618, 625, 626, 627, 628, 633, 635, 641, 659, 661, 662, 664, 666,
    667, 668, 669, 670, 671, 672, 673, 674, 675, 676, 677, 678, 679, 680, 681,
    682, 683, 684, 685, 686, 687, 688, 689, 690, 691, 692, 708, 711, 712, 713,
    714, 715, 716, 717, 718, 719, 722, 726, 727, 728, 729, 730, 731, 732, 734,
    735, 736, 737, 738, 739, 740, 741, 742, 743, 744, 745, 746, 747, 748, 749,
    750
  ];

  private static readonly int[] SolidTopTileIds =
  [
    14, 16, 18, 19, 87, 88, 101, 114, 134, 239, 275, 276, 277, 278, 279, 280,
    281, 285, 286, 296, 297, 298, 299, 309, 310, 339, 358, 359, 361, 362, 363,
    364, 376, 380, 391, 392, 393, 394, 405, 413, 414, 427, 435, 436, 437, 438,
    439, 469, 532, 533, 538, 542, 544, 550, 551, 553, 554, 555, 556, 558, 559,
    582, 599, 600, 601, 602, 603, 604, 605, 606, 607, 608, 609, 610, 611, 612,
    619, 629, 632, 640, 643, 644, 645, 710
  ];

  private static readonly int[] _nonSolidSaveSlopesTileIds =
  [
    131, 351, 336, 340, 342, 341, 343, 344
  ];

  // Main calls Initialize_TileAndNPCData1 before Data2. Data2 copies tileLavaDeath at
  // that point, then applies explicit ObsidianKill/echo-furniture exceptions. Later
  // tileLavaDeath writes do not change the copy used by LiquidCheck.
  private static readonly int[] LiquidMergeReplaceableTileIds =
  [
    3, 5, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 24, 27, 28, 29, 32, 33, 34,
    35, 36, 42, 49, 50, 51, 52, 55, 61, 62, 69, 71, 72, 73, 74, 77, 78, 79,
    80, 81, 82, 83, 84, 85, 86, 87, 89, 90, 91, 92, 93, 94, 95, 96, 97, 98,
    100, 101, 102, 103, 104, 105, 106, 110, 113, 115, 125, 126, 128, 129, 132,
    133, 134, 135, 136, 139, 149, 165, 172, 173, 174, 178, 184, 185, 186, 187,
    201, 205, 209, 210, 212, 213, 215, 216, 217, 218, 219, 220, 227, 228, 231,
    233, 236, 238, 240, 241, 242, 243, 244, 245, 246, 247, 254, 269, 270, 271,
    275, 276, 277, 278, 279, 280, 281, 282, 283, 285, 286, 287, 288, 289, 290,
    291, 292, 293, 294, 295, 296, 297, 298, 299, 300, 301, 302, 303, 304, 305,
    306, 307, 308, 309, 310, 314, 316, 317, 318, 319, 323, 324, 335, 337, 338,
    339, 349, 352, 353, 354, 355, 372, 382, 390, 413, 425, 427, 435, 436, 437,
    438, 439, 453, 456, 463, 464, 465, 469, 484, 485, 486, 487, 488, 489, 490,
    493, 497, 499, 506, 510, 511, 528,
    529, 530, 532, 533, 538, 544, 546, 547, 548, 550, 551, 552, 553, 554, 555,
    556, 558, 559, 560, 564, 565, 567, 568, 569, 570, 571, 572, 573, 579, 580,
    581, 582, 591, 599, 600, 601, 602, 603, 604, 605, 606, 607, 608, 609, 610,
    611, 612, 619, 620, 621, 622, 623, 624, 629, 630, 631, 632, 636, 639, 640,
    642, 643, 644, 645, 646, 647, 648, 649, 650, 651, 652, 654, 655, 656, 660,
    668, 693, 694, 697, 698, 699, 700, 701, 702, 703, 704, 705, 706, 707, 710
  ];

  private WorldFacts(
    bool[] frameImportant,
    bool[] allowsSaveCompressionBatching,
    bool[] solid,
    bool[] solidTop,
    bool[] liquidMergeReplaceable,
    bool[] saveSlopes)
  {
    FrameImportant = Array.AsReadOnly(frameImportant);
    AllowsSaveCompressionBatching = Array.AsReadOnly(allowsSaveCompressionBatching);
    Solid = Array.AsReadOnly(solid);
    SolidTop = Array.AsReadOnly(solidTop);
    LiquidMergeReplaceable = Array.AsReadOnly(liquidMergeReplaceable);
    SaveSlopes = Array.AsReadOnly(saveSlopes);
    Fingerprint = ComputeFingerprint(
      frameImportant,
      allowsSaveCompressionBatching,
      solid,
      solidTop,
      liquidMergeReplaceable,
      saveSlopes);
  }

  public const int TileCount = 753;

  public const int WallCount = 367;

  public const string SourceProfile = "Terraria319/v1.4.5.6";

  public const string Version4MainSha256 =
    "66DBE1D1E6FB89A24A512309BB039DFEC7C06D4678D2A22D7ADECBAD3AFD6520";

  public const string Version4TileIdSha256 =
    "60181CCC3AF26DDAEF55D14774163A124F3D7F494DD999E1BAB6D125AE6BB1A9";

  public IReadOnlyList<bool> FrameImportant { get; }

  public IReadOnlyList<bool> AllowsSaveCompressionBatching { get; }

  public IReadOnlyList<bool> Solid { get; }

  public IReadOnlyList<bool> SolidTop { get; }

  public IReadOnlyList<bool> LiquidMergeReplaceable { get; }

  public IReadOnlyList<bool> SaveSlopes { get; }

  public string Fingerprint { get; }

  public bool[] CopyFrameImportantForProtocolInputs() => FrameImportant.ToArray();

  public bool[] CopyCompressionBatchingForProtocolInputs() =>
    AllowsSaveCompressionBatching.ToArray();

  public static WorldFacts CreateVersion4()
  {
    var frameImportant = new bool[TileCount];
    foreach (int tileId in FrameImportantTileIds)
    {
      frameImportant[tileId] = true;
    }

    var compression = new bool[TileCount];
    Array.Fill(compression, true);
    compression[520] = false;
    compression[423] = false;
    compression[723] = false;
    compression[724] = false;
    var solid = new bool[TileCount];
    foreach (int tileId in SolidTileIds)
    {
      solid[tileId] = true;
    }

    solid[110] = false;
    solid[3] = false;
    solid[4] = false;
    solid[5] = false;
    solid[11] = false;
    solid[634] = false;
    // TileID.Sets.PostSetupContent uses the final tileSolid table plus these exceptions.
    var saveSlopes = (bool[])solid.Clone();
    foreach (int tileId in _nonSolidSaveSlopesTileIds) {
      saveSlopes[tileId] = true;
    }

    var solidTop = new bool[TileCount];
    foreach (int tileId in SolidTopTileIds)
    {
      solidTop[tileId] = true;
    }

    var liquidMergeReplaceable = new bool[TileCount];
    foreach (int tileId in LiquidMergeReplaceableTileIds)
    {
      liquidMergeReplaceable[tileId] = true;
    }

    // Main.Initialize_TileAndNPCData2 first copies tileLavaDeath, then explicitly clears
    // tile 88 after that copy. Keep that exception visible in the facts table.
    liquidMergeReplaceable[88] = false;

    return new WorldFacts(
      frameImportant,
      compression,
      solid,
      solidTop,
      liquidMergeReplaceable,
      saveSlopes);
  }

  public bool IsValidTileType(ushort tileType)
  {
    return tileType < TileCount;
  }

  public bool BlocksLiquid(WorldTile tile)
  {
    // Liquid.UpdateLiquid ignores boulder solidity and bubbles for its whole update pass.
    if (tile.Type is 138 or 484 or 546 or 664 or 711 or 712 or 713 or 714 or 715 or 716) {
      return false;
    }

    return tile.Active && !tile.Inactive && tile.Type < Solid.Count &&
      Solid[tile.Type] && !SolidTop[tile.Type];
  }

  public bool AllowsLiquidMerge(WorldTile tile)
  {
    return !tile.Active ||
      (tile.Type < LiquidMergeReplaceable.Count && LiquidMergeReplaceable[tile.Type]);
  }

  private static string ComputeFingerprint(
    bool[] frameImportant,
    bool[] compression,
    bool[] solid,
    bool[] solidTop,
    bool[] liquidMergeReplaceable,
    bool[] saveSlopes)
  {
    var bytes = new byte[frameImportant.Length * 6];
    for (int index = 0; index < frameImportant.Length; index++)
    {
      bytes[index] = frameImportant[index] ? (byte)1 : (byte)0;
      bytes[index + frameImportant.Length] = compression[index] ? (byte)1 : (byte)0;
      bytes[index + frameImportant.Length * 2] = solid[index] ? (byte)1 : (byte)0;
      bytes[index + frameImportant.Length * 3] = solidTop[index] ? (byte)1 : (byte)0;
      bytes[index + frameImportant.Length * 4] = liquidMergeReplaceable[index] ? (byte)1 : (byte)0;
      bytes[index + frameImportant.Length * 5] = saveSlopes[index] ? (byte)1 : (byte)0;
    }

    return Convert.ToHexString(SHA256.HashData(bytes));
  }
}
