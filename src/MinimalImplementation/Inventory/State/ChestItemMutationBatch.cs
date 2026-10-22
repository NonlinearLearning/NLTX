using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Inventory.State;

public sealed record ChestItemMutationBatch
{
  public ChestId ChestId { get; }
  public WorldScopeId WorldScope { get; }
  public ChestGeneration Generation { get; }
  public CommitVersion ExpectedRevision { get; }
  public IReadOnlyList<ChestItemMutation> Mutations { get; }

  public ChestItemMutationBatch(
    ChestId chestId,
    WorldScopeId worldScope,
    ChestGeneration generation,
    CommitVersion expectedRevision,
    IReadOnlyCollection<ChestItemMutation> mutations)
  {
    ArgumentNullException.ThrowIfNull(mutations);
    ChestId = chestId;
    WorldScope = worldScope;
    Generation = generation;
    ExpectedRevision = expectedRevision;
    Mutations = Array.AsReadOnly(mutations.ToArray());
  }
}
