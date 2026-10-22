using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record ChestSnapshot
{
  public ChestId Id { get; }
  public WorldScopeId WorldScope { get; }
  public ChestGeneration Generation { get; }
  public WorldTilePoint Position { get; }
  public ushort KindId { get; }
  public string Name { get; }
  public CommitVersion Revision { get; }
  public IReadOnlyList<ChestSlotSnapshot> Slots { get; }

  public ChestSnapshot(
    ChestId id,
    WorldScopeId worldScope,
    ChestGeneration generation,
    WorldTilePoint position,
    ushort kindId,
    string name,
    CommitVersion revision,
    IReadOnlyCollection<ChestSlotSnapshot> slots)
  {
    if (!worldScope.IsValid)
    {
      throw new ArgumentException("A chest snapshot requires a valid world scope.", nameof(worldScope));
    }

    if (!generation.IsValid)
    {
      throw new ArgumentException("A chest snapshot requires a valid generation.", nameof(generation));
    }

    ArgumentNullException.ThrowIfNull(name);
    if (!revision.IsValid)
    {
      throw new ArgumentException("A chest snapshot requires a committed revision.", nameof(revision));
    }

    ArgumentNullException.ThrowIfNull(slots);
    Id = id;
    WorldScope = worldScope;
    Generation = generation;
    Position = position;
    KindId = kindId;
    Name = name;
    Revision = revision;
    Slots = Array.AsReadOnly(slots.ToArray());
  }
}
