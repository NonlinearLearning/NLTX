namespace Terraria.NonAuthoritative.ContentDefinitions;

public sealed class ItemStaticCapabilityCatalog
{
  private readonly int[] _headType;
  private readonly int[] _bodyType;
  private readonly int[] _legType;
  private readonly bool[] _staff;
  private readonly bool[] _claw;
  private readonly int[] _spawnCounts;
  private readonly object _cacheLock = new();
  private ColorValue[]? _phaseColors;

  public ItemStaticCapabilityCatalog(int itemTypeCount, int armorSlotCount)
  {
    if (itemTypeCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(itemTypeCount));
    }

    if (armorSlotCount <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(armorSlotCount));
    }

    _headType = CreateReverseIndex(armorSlotCount);
    _bodyType = CreateReverseIndex(armorSlotCount);
    _legType = CreateReverseIndex(armorSlotCount);
    _staff = new bool[itemTypeCount];
    _claw = new bool[itemTypeCount];
    _spawnCounts = new int[itemTypeCount];
  }

  public IReadOnlyList<int> HeadType => _headType;

  public IReadOnlyList<int> BodyType => _bodyType;

  public IReadOnlyList<int> LegType => _legType;

  public IReadOnlyList<bool> Staff => _staff;

  public IReadOnlyList<bool> Claw => _claw;

  public bool IsStaff(int itemType)
  {
    return _staff[ValidateItemType(itemType, _staff.Length)];
  }

  public bool IsClaw(int itemType)
  {
    return _claw[ValidateItemType(itemType, _claw.Length)];
  }

  public int GetHeadItem(int armorType)
  {
    return _headType[ValidateItemType(armorType, _headType.Length)];
  }

  public int GetBodyItem(int armorType)
  {
    return _bodyType[ValidateItemType(armorType, _bodyType.Length)];
  }

  public int GetLegItem(int armorType)
  {
    return _legType[ValidateItemType(armorType, _legType.Length)];
  }

  public int GetSpawnCount(int itemType)
  {
    return _spawnCounts[ValidateItemType(itemType, _spawnCounts.Length)];
  }

  public void InvalidateDerivedCaches()
  {
    lock (_cacheLock)
    {
      Array.Clear(_spawnCounts);
      _phaseColors = null;
    }
  }

  internal void Register(
    int itemType,
    int headType,
    int bodyType,
    int legType,
    bool isStaff,
    bool isClaw)
  {
    int itemIndex = ValidateItemType(itemType, _staff.Length);
    _staff[itemIndex] = isStaff;
    _claw[itemIndex] = isClaw;
    SetReverseIndex(_headType, headType, itemType);
    SetReverseIndex(_bodyType, bodyType, itemType);
    SetReverseIndex(_legType, legType, itemType);
  }

  internal void RecordSpawn(int itemType)
  {
    int index = ValidateItemType(itemType, _spawnCounts.Length);
    lock (_cacheLock)
    {
      _spawnCounts[index]++;
    }
  }

  internal ColorValue GetOrCreatePhaseColor(
    IReadOnlyList<ColorValue> palette,
    int phaseIndex)
  {
    ArgumentNullException.ThrowIfNull(palette);
    if ((uint)phaseIndex >= (uint)palette.Count)
    {
      throw new ArgumentOutOfRangeException(nameof(phaseIndex));
    }

    lock (_cacheLock)
    {
      _phaseColors ??= palette.ToArray();
      return _phaseColors[phaseIndex];
    }
  }

  private static int[] CreateReverseIndex(int count)
  {
    int[] values = new int[count];
    Array.Fill(values, -1);
    return values;
  }

  private static void SetReverseIndex(int[] values, int index, int itemType)
  {
    if (index == 0)
    {
      return;
    }

    values[ValidateItemType(index, values.Length)] = itemType;
  }

  private static int ValidateItemType(int index, int length)
  {
    if ((uint)index >= (uint)length)
    {
      throw new ArgumentOutOfRangeException(nameof(index));
    }

    return index;
  }
}

public static class ItemStaticCapabilityRegistrationSystem
{
  public static void Register(
    ItemStaticCapabilityCatalog catalog,
    int itemType,
    int headType,
    int bodyType,
    int legType,
    bool isStaff,
    bool isClaw)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    catalog.Register(itemType, headType, bodyType, legType, isStaff, isClaw);
  }
}

public static class ItemSpawnCacheAdapter
{
  public static void RecordSpawn(ItemStaticCapabilityCatalog catalog, int itemType)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    catalog.RecordSpawn(itemType);
  }
}

public static class PhaseColorProjection
{
  public static ColorValue GetPhaseColor(
    ItemStaticCapabilityCatalog catalog,
    IReadOnlyList<ColorValue> palette,
    int phaseIndex)
  {
    ArgumentNullException.ThrowIfNull(catalog);
    return catalog.GetOrCreatePhaseColor(palette, phaseIndex);
  }
}
