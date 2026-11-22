using System.Numerics;
using Terraria.Session;

namespace Terraria.World;

/// <summary>Kind of world-item change another domain may request.</summary>
public enum WorldItemRequestKind : byte
{
  /// <summary>Occupy a free slot with a new item.</summary>
  Create,

  /// <summary>Rewrite an item the actor owns.</summary>
  Update,

  /// <summary>Drop the actor's claim on a slot without removing the item.</summary>
  Release,

  /// <summary>Remove an item the actor owns.</summary>
  Retire,

  /// <summary>Remove <see cref="WorldItemRequest.Stack"/> units the actor owns.</summary>
  /// <remarks>
  /// Pickup reporting uses this kind: the inventory domain states how many units it accepted and
  /// the World domain removes exactly that many, so a partial pickup can never duplicate items.
  /// </remarks>
  Consume
}

/// <summary>One world-item change requested by another domain.</summary>
/// <remarks>
/// A request states intent only. The World domain validates it against
/// <see cref="WorldItemRules"/> and the slot's current owner, so a requesting domain cannot write
/// authoritative item state even though it decides what to ask for.
/// </remarks>
public readonly record struct WorldItemRequest(
    WorldItemRequestKind Kind,
    short Slot,
    SessionEpoch Actor,
    int ItemType = 0,
    int Stack = 0,
    byte Prefix = 0,
    Vector2 Position = default,
    Vector2 Velocity = default,
    SessionEpoch? ReservedFor = null);
