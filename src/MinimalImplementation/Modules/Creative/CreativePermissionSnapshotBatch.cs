using MinimalImplementation.Contracts;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Modules.Creative;

/// <summary>
/// One complete module 9 permission projection captured for one target and owner version.
/// The root transport must submit one Packet 82 intent per entry before acknowledging this batch.
/// </summary>
public sealed class CreativePermissionSnapshotBatch
{
  private const int PermissionCount = 15;

  public CreativePermissionSnapshotBatch(
    SessionEpoch target,
    CommitVersion version,
    IReadOnlyList<Packet82PermissionData> entries)
  {
    if (!target.IsValid)
    {
      throw new ArgumentException(
        "A Creative permission projection target must be a valid epoch.",
        nameof(target));
    }

    if (!version.IsValid)
    {
      throw new ArgumentException(
        "A Creative permission projection requires a valid owner version.",
        nameof(version));
    }

    ArgumentNullException.ThrowIfNull(entries);
    if (entries.Count != PermissionCount)
    {
      throw new ArgumentException(
        "A Creative permission projection must contain all 15 power entries.",
        nameof(entries));
    }

    Packet82PermissionData[] copy = new Packet82PermissionData[PermissionCount];
    for (int index = 0; index < PermissionCount; index++)
    {
      Packet82PermissionData entry = entries[index]
        ?? throw new ArgumentException(
          "A Creative permission projection cannot contain a null entry.",
          nameof(entries));
      if (entry.Action != 0
          || entry.PowerId != index
          || !Enum.IsDefined((CreativePowerPermissionLevel)entry.Level))
      {
        throw new ArgumentException(
          "A Creative permission projection must be ordered action-0 data for powers 0-14.",
          nameof(entries));
      }

      copy[index] = entry;
    }

    Target = target;
    Version = version;
    Entries = Array.AsReadOnly(copy);
  }

  public SessionEpoch Target { get; }

  public CommitVersion Version { get; }

  public IReadOnlyList<Packet82PermissionData> Entries { get; }
}
