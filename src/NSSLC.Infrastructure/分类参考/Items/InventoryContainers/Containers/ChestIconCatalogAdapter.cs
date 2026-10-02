namespace Terraria.Items.InventoryContainers;

public sealed class ChestIconCatalogAdapter
{
  private readonly int[] _primaryChestIcons;
  private readonly int[] _secondaryChestIcons;
  private readonly int[] _dresserIcons;

  public ChestIconCatalogAdapter(
    IReadOnlyList<int> primaryChestIcons,
    IReadOnlyList<int> secondaryChestIcons,
    IReadOnlyList<int> dresserIcons)
  {
    ArgumentNullException.ThrowIfNull(primaryChestIcons);
    ArgumentNullException.ThrowIfNull(secondaryChestIcons);
    ArgumentNullException.ThrowIfNull(dresserIcons);
    if (primaryChestIcons.Count != ChestCapacityPolicy.MaxChestTypes)
    {
      throw new ArgumentException("The primary chest icon catalog has an invalid length.", nameof(primaryChestIcons));
    }

    if (secondaryChestIcons.Count != ChestCapacityPolicy.MaxChestTypes2)
    {
      throw new ArgumentException("The secondary chest icon catalog has an invalid length.", nameof(secondaryChestIcons));
    }

    if (dresserIcons.Count != ChestCapacityPolicy.MaxDresserTypes)
    {
      throw new ArgumentException("The dresser icon catalog has an invalid length.", nameof(dresserIcons));
    }

    _primaryChestIcons = primaryChestIcons.ToArray();
    _secondaryChestIcons = secondaryChestIcons.ToArray();
    _dresserIcons = dresserIcons.ToArray();
  }

  public bool TryGetChestIcon(int chestType, bool secondType, out int icon)
  {
    int[] icons = secondType ? _secondaryChestIcons : _primaryChestIcons;
    if ((uint)chestType >= (uint)icons.Length)
    {
      icon = 0;
      return false;
    }

    icon = icons[chestType];
    return true;
  }

  public bool TryGetDresserIcon(int dresserType, out int icon)
  {
    if ((uint)dresserType >= (uint)_dresserIcons.Length)
    {
      icon = 0;
      return false;
    }

    icon = _dresserIcons[dresserType];
    return true;
  }
}
