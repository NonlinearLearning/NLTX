using MinimalImplementation.Contracts;

namespace MinimalImplementation.Server.Player;

/// <summary>Revision tuple for every external source read by Player facts capture.</summary>
public sealed record PlayerSourceRevision(
  CommitVersion Inventory,
  CommitVersion EquipmentMetadata,
  CommitVersion World)
{
  public bool IsValid => Inventory.IsValid && EquipmentMetadata.IsValid && World.IsValid;
}
