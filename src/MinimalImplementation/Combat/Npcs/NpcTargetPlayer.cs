using System.Numerics;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>One immutable P02 player snapshot, with type-specific aggro facts.</summary>
public sealed record NpcTargetPlayer(
    byte Slot, Vector2 Position, int Width, int Height,
    bool Active, bool Dead, bool Ghost, int Aggro, bool NoAggro,
    bool IsAnimatingItem, bool InGraveyard, NpcTankTarget? TankPet = null);
