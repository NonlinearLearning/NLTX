using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Terraria.Dome.Simulation.WorldModel.Definitions;

public sealed class TileDefinitionRegistry
{
  public const int Version4TileCount = 753;

  private readonly IReadOnlyDictionary<int, TileDefinition> _definitionsByType;

  private TileDefinitionRegistry(IReadOnlyList<TileDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    if (definitions.Count != Version4TileCount)
    {
      throw new ArgumentException(
        "A Version4 registry must contain exactly 753 Tile definitions.",
        nameof(definitions));
    }

    Dictionary<int, TileDefinition> indexed = new(Version4TileCount);
    for (int index = 0; index < definitions.Count; index++)
    {
      TileDefinition definition = definitions[index];
      if (definition.TileType != index || !indexed.TryAdd(definition.TileType, definition))
      {
        throw new ArgumentException(
          "A Version4 registry must contain each Tile ID exactly once.",
          nameof(definitions));
      }
    }

    Definitions = Array.AsReadOnly([.. definitions]);
    _definitionsByType = new ReadOnlyDictionary<int, TileDefinition>(indexed);
  }

  public IReadOnlyList<TileDefinition> Definitions { get; }

  public static TileDefinitionRegistry CreateVersion4Base()
  {
    HashSet<int> solidTypes =
    [
      0, 1, 2, 6, 7, 8, 9, 10, 19, 22, 23, 25, 30, 37, 38, 39, 40, 41, 43, 44, 45, 46,
      47, 48, 53, 54, 56, 57, 58, 59, 60, 63, 64, 65, 66, 67, 68, 70, 75, 76, 107, 108,
      109, 111, 112, 116, 117, 118, 119, 120, 121, 122, 123, 127, 130, 137, 138, 140,
      145, 146, 147, 148, 150, 151, 152, 153, 154, 155, 156, 157, 158, 159, 160, 161,
      162, 163, 164, 166, 167, 168, 169, 170, 175, 176, 177, 179, 180, 181, 182, 183,
      188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 202, 203, 204,
      206, 208, 211, 221, 222, 223, 224, 225, 226, 229, 230, 232, 234, 235, 239, 248,
      249, 250, 251, 252, 253, 255, 256, 257, 258, 259, 260, 261, 262, 263, 264, 265,
      266, 267, 268, 272, 273, 274, 284, 311, 312, 313, 315, 321, 322, 325, 326, 327,
      328, 329, 345, 346, 347, 348, 350, 357, 367, 368, 369, 370, 371, 379, 380, 381,
      383, 384, 385, 387, 388, 396, 397, 398, 399, 400, 401, 402, 403, 404, 407, 408,
      409, 415, 416, 417, 418, 421, 422, 426, 427, 430, 431, 432, 433, 434, 435, 436,
      437, 438, 439, 446, 447, 448, 458, 459, 460, 472, 473, 474, 476, 477, 478, 479,
      481, 482, 483, 484, 492, 495, 496, 498, 500, 501, 502, 503, 507, 508, 512, 513,
      514, 515, 516, 517, 534, 535, 536, 537, 539, 540, 541, 546, 557, 562, 563, 566,
      618, 625, 626, 627, 628, 633, 635, 641, 659, 661, 662, 664, 666, 667, 668, 669,
      670, 671, 672, 673, 674, 675, 676, 677, 678, 679, 680, 681, 682, 683, 684, 685,
      686, 687, 688, 689, 690, 691, 692, 708, 711, 712, 713, 714, 715, 716, 717, 718,
      719, 722, 726, 727, 728, 729, 730, 731, 732, 734, 735, 736, 737, 738, 739, 740,
      741, 742, 743, 744, 745, 746, 747, 748, 749, 750
    ];
    HashSet<int> platformTypes =
    [
      14, 16, 18, 19, 87, 88, 101, 114, 134, 239, 275, 276, 277, 278, 279, 280, 281,
      285, 286, 296, 297, 298, 299, 309, 310, 339, 358, 359, 361, 362, 363, 364, 376,
      380, 391, 392, 393, 394, 405, 413, 414, 427, 435, 436, 437, 438, 439, 469, 532,
      533, 538, 542, 544, 550, 551, 553, 554, 555, 556, 558, 559, 582, 599, 600, 601,
      602, 603, 604, 605, 606, 607, 608, 609, 610, 611, 612, 619, 629, 632, 640, 643,
      644, 645, 710
    ];
    List<TileDefinition> definitions = new(Version4TileCount);
    for (ushort tileType = 0; tileType < Version4TileCount; tileType++)
    {
      definitions.Add(new TileDefinition(
        tileType,
        solidTypes.Contains(tileType),
        platformTypes.Contains(tileType)));
    }

    return new TileDefinitionRegistry(definitions);
  }

  public bool TryGet(int tileType, out TileDefinition definition)
  {
    return _definitionsByType.TryGetValue(tileType, out definition);
  }
}
