using System.Collections.Generic;

namespace Terraria.Share.Items;

/// <summary>
/// Narrow feed for the committed world-item changes that still need network publication.
/// </summary>
/// <remarks>
/// Consuming, not read-only: <see cref="DrainMutations"/> takes the changes and clears them.
/// Item creation is deliberately absent — a foreign domain states its intent through
/// <see cref="WorldItemSpawnRequestComponent"/> and the World domain performs the spawn, so the
/// authoritative item store never leaves its owner.
/// </remarks>
public interface IItemProjectionSource
{
  /// <summary>Takes the item changes committed since the previous drain and clears them.</summary>
  IReadOnlyList<ItemMutation> DrainMutations();
}
