using System.Collections.ObjectModel;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Modules.Creative;

public sealed class CreativePowerSnapshot
{
  private const int PlayerCount = 255;
  private static readonly ushort[] SharedTogglePowerIds = { 0, 9, 10, 13 };
  private static readonly ushort[] SharedSliderPowerIds = { 6, 7, 8, 12 };
  private static readonly ushort[] PlayerTogglePowerIds = { 5, 11 };
  private static readonly ushort[] PlayerSliderPowerIds = { 14 };
  private static readonly ushort[] AllPowerIds = Enumerable.Range(0, 15)
    .Select(powerId => (ushort)powerId)
    .ToArray();

  public CreativePowerSnapshot(
    CommitVersion version,
    IReadOnlyDictionary<ushort, bool> sharedToggles,
    IReadOnlyDictionary<ushort, float> sharedSliders,
    IReadOnlyDictionary<ushort, IReadOnlyList<bool>> playerToggles,
    IReadOnlyDictionary<ushort, IReadOnlyList<float>> playerSliders,
    IReadOnlyDictionary<ushort, CreativePowerPermissionLevel> permissions)
  {
    if (!version.IsValid)
    {
      throw new ArgumentException(
        "A Creative Power snapshot requires a valid commit version.",
        nameof(version));
    }

    ArgumentNullException.ThrowIfNull(sharedToggles);
    ArgumentNullException.ThrowIfNull(sharedSliders);
    ArgumentNullException.ThrowIfNull(playerToggles);
    ArgumentNullException.ThrowIfNull(playerSliders);
    ArgumentNullException.ThrowIfNull(permissions);

    Version = version;
    SharedToggles = CopyValues(sharedToggles, SharedTogglePowerIds);
    SharedSliders = CopySliderValues(sharedSliders, SharedSliderPowerIds);
    PlayerToggles = CopyPlayerToggles(playerToggles);
    PlayerSliders = CopyPlayerSliders(playerSliders);
    Permissions = CopyPermissions(permissions);
  }

  public CommitVersion Version { get; }
  public IReadOnlyDictionary<ushort, bool> SharedToggles { get; }
  public IReadOnlyDictionary<ushort, float> SharedSliders { get; }
  public IReadOnlyDictionary<ushort, IReadOnlyList<bool>> PlayerToggles { get; }
  public IReadOnlyDictionary<ushort, IReadOnlyList<float>> PlayerSliders { get; }
  public IReadOnlyDictionary<ushort, CreativePowerPermissionLevel> Permissions { get; }

  private static IReadOnlyDictionary<ushort, T> CopyValues<T>(
    IReadOnlyDictionary<ushort, T> values,
    IReadOnlyCollection<ushort> expectedPowerIds)
  {
    ValidatePowerIds(values.Keys, expectedPowerIds);
    Dictionary<ushort, T> copy = new();
    foreach (ushort powerId in expectedPowerIds.Order())
    {
      copy.Add(powerId, values[powerId]);
    }

    return new ReadOnlyDictionary<ushort, T>(copy);
  }

  private static IReadOnlyDictionary<ushort, float> CopySliderValues(
    IReadOnlyDictionary<ushort, float> values,
    IReadOnlyCollection<ushort> expectedPowerIds)
  {
    ValidatePowerIds(values.Keys, expectedPowerIds);
    Dictionary<ushort, float> copy = new();
    foreach (ushort powerId in expectedPowerIds.Order())
    {
      float value = values[powerId];
      ValidateSliderValue(value);
      copy.Add(powerId, value);
    }

    return new ReadOnlyDictionary<ushort, float>(copy);
  }

  private static IReadOnlyDictionary<ushort, IReadOnlyList<bool>> CopyPlayerToggles(
    IReadOnlyDictionary<ushort, IReadOnlyList<bool>> values)
  {
    ValidatePowerIds(values.Keys, PlayerTogglePowerIds);
    Dictionary<ushort, IReadOnlyList<bool>> copy = new();
    foreach (ushort powerId in PlayerTogglePowerIds.Order())
    {
      IReadOnlyList<bool> playerStates = values[powerId];
      ArgumentNullException.ThrowIfNull(playerStates);
      if (playerStates.Count != PlayerCount)
      {
        throw new ArgumentException(
          "Creative Power snapshots require 255 player-toggle states per power.");
      }

      copy.Add(powerId, Array.AsReadOnly(playerStates.ToArray()));
    }

    return new ReadOnlyDictionary<ushort, IReadOnlyList<bool>>(copy);
  }

  private static IReadOnlyDictionary<ushort, IReadOnlyList<float>> CopyPlayerSliders(
    IReadOnlyDictionary<ushort, IReadOnlyList<float>> values)
  {
    ValidatePowerIds(values.Keys, PlayerSliderPowerIds);
    Dictionary<ushort, IReadOnlyList<float>> copy = new();
    foreach (ushort powerId in PlayerSliderPowerIds.Order())
    {
      IReadOnlyList<float> playerValues = values[powerId];
      ArgumentNullException.ThrowIfNull(playerValues);
      if (playerValues.Count != PlayerCount)
      {
        throw new ArgumentException(
          "Creative Power snapshots require 255 player-slider values per power.");
      }

      float[] playerValuesCopy = playerValues.ToArray();
      foreach (float value in playerValuesCopy)
      {
        ValidateSliderValue(value);
      }

      copy.Add(powerId, Array.AsReadOnly(playerValuesCopy));
    }

    return new ReadOnlyDictionary<ushort, IReadOnlyList<float>>(copy);
  }

  private static IReadOnlyDictionary<ushort, CreativePowerPermissionLevel> CopyPermissions(
    IReadOnlyDictionary<ushort, CreativePowerPermissionLevel> values)
  {
    ValidatePowerIds(values.Keys, AllPowerIds);
    Dictionary<ushort, CreativePowerPermissionLevel> copy = new();
    foreach (ushort powerId in AllPowerIds)
    {
      CreativePowerPermissionLevel level = values[powerId];
      if (!Enum.IsDefined(level))
      {
        throw new ArgumentException(
          "A Creative Power snapshot contains an unknown permission level.");
      }

      copy.Add(powerId, level);
    }

    return new ReadOnlyDictionary<ushort, CreativePowerPermissionLevel>(copy);
  }

  private static void ValidatePowerIds(
    IEnumerable<ushort> actualPowerIds,
    IReadOnlyCollection<ushort> expectedPowerIds)
  {
    ushort[] actual = actualPowerIds.Order().ToArray();
    ushort[] expected = expectedPowerIds.Order().ToArray();
    if (!actual.SequenceEqual(expected))
    {
      throw new ArgumentException(
        "A Creative Power snapshot has an incomplete or unknown power table.");
    }
  }

  private static void ValidateSliderValue(float value)
  {
    if (!float.IsFinite(value) || value is < 0f or > 1f)
    {
      throw new ArgumentException(
        "A Creative Power snapshot contains a slider value outside the finite 0 to 1 range.");
    }
  }
}
