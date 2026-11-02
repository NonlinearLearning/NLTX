using System.Numerics;
using Terraria.Communication;

namespace Terraria.World;

/// <summary>One authoritative world item occupying a Version4 item slot.</summary>
public sealed record WorldItemState(
    short Slot,
    int ItemType,
    int Stack,
    byte Prefix,
    Vector2 Position,
    Vector2 Velocity,
    SessionEpoch? ReservedFor,
    long Revision);

/// <summary>Kind of externally visible change recorded for a world item.</summary>
public enum WorldItemChangeKind : byte
{
  Spawn,
  Update,
  Retire,
  Ownership
}

/// <summary>One committed world item change waiting for network projection.</summary>
public readonly record struct WorldItemMutation(
    WorldItemChangeKind Kind,
    WorldItemState Item,
    SessionEpoch? Origin = null);
