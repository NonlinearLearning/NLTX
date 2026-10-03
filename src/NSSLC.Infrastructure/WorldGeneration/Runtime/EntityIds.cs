using NSSLC.WorldGeneration.DataStructures;
using NSSLC.WorldGeneration.ID;
namespace NSSLC.WorldGeneration;
public static class ItemID {
  public const int Count = 6147;
  public static class Sets {
    private static readonly SetFactory Factory = new SetFactory(Count);
    public static bool[] CanGetPrefixes = Factory.CreateBoolSet(true, 267, 1307, 562, 563, 564, 565, 566, 567, 568, 569, 570, 571, 572, 573, 574, 576, 1596, 1597, 1598, 1599, 1600, 1601, 1602, 1603, 1604, 1605, 1606, 1607, 1608, 1609, 1610, 1963, 1964, 1965, 2742, 3044, 3235, 3236, 3237, 3370, 3371, 3796, 3869, 4077, 4078, 4079, 4080, 4081, 4082, 4237, 4356, 4357, 4358, 4421, 4606, 4979, 4985, 4990, 4991, 4992, 5006, 5014, 5015, 5016, 5017, 5018, 5019, 5020, 5021, 5022, 5023, 5024, 5025, 5026, 5027, 5028, 5029, 5030, 5031, 5032, 5033, 5034, 5035, 5036, 5037, 5038, 5039, 5040, 5044, 5112, 5362, 5538, 5578, 5579, 5580, 5581, 5582, 5637, 5638, 5639, 6144, 6145, 6146);
    public static PlacementDetails[] DerivedPlacementDetails = new PlacementDetails[Count];
    public static bool[] ErrorWorldChestSwapImmunity = Factory.CreateBoolSet(0, 1156, 1571, 1569, 1260, 1572, 4607);
    public static int[] OverflowProtectionTimeOffset = Factory.CreateIntSet(0, 2, 200, 3, 150, 61, 150, 836, 150, 409, 150, 593, 200, 664, 100, 834, 100, 833, 100, 835, 100, 169, 100, 370, 100, 1246, 100, 408, 100, 3271, 150, 3277, 150, 3339, 150, 3276, 150, 3272, 150, 3274, 150, 3275, 150, 3338, 150, 176, 100, 172, 200, 424, 50, 1103, 50, 3087, 100, 3066, 100);
  }
}
public static class NPCID {
  public const int Count = 697;
  public static class Sets {
    private static readonly SetFactory Factory = new SetFactory(Count);
public static bool[] IsTownPet = Factory.CreateBoolSet(637, 638, 656, 670, 678, 679, 680, 681, 682, 683, 684);
  }
}
