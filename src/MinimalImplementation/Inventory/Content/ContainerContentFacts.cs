namespace MinimalImplementation.Server.Inventory.Content;

/// <summary>Immutable Version4 container sizing facts for inventory initialization and bounds.</summary>
/// <remarks>
/// Version4 Terraria/Chest.cs:34 declares DefaultMaxItems=40 and line 36 declares
/// AbsoluteMaxItemsWeCanEverReachInAChestForNow=200. The same declarations and values occur in
/// the complete source tree at Terraria/Chest.cs:57 and :59. These constants record the source
/// bounds; Chest.cs:40 fixes MaxNameLength=20, and MessageBuffer.cs:3304 bounds packet chest ids
/// to 0..7999. Chest kind, tile validity, and dynamic-size migration remain inventory rules.
///
/// Version4 SHA-256: 14EAF2C87C3761E2585C71EA98D6DF2F653354207BFE726D771E5C353F2918B9.
/// Complete-source SHA-256: A19E6A6811315B4CD03E6C7EE72666E8D4356851EBAFE915220737B8870DC1C7.
/// </remarks>
public static class ContainerContentFacts
{
  public const int ChestIndexCount = 8000;
  public const int DefaultChestSlotCount = 40;
  public const int AbsoluteMaximumChestSlotCountForVersion4 = 200;
  public const int MaximumChestNameLength = 20;
}
