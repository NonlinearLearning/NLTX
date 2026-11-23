using ArchEntity = Arch.Core.Entity;

namespace Terraria.EntitySpawn;

/// <summary>Fact raised by the NPC domain after it created a creature that needs a network id.</summary>
/// <remarks>
/// The NPC domain creates the entity; the network slot and protocol type are the Network domain's
/// business. The fact carries both so the binding step needs no shared mutable state, and it stays
/// outside this assembly because <c>NpcNetworkIdentityComponent</c> lives in NSSLC.Network.
/// </remarks>
public readonly record struct EntitySpawnedFact(ArchEntity Entity, short NetworkType);
