namespace MinimalImplementation.Combat.Npcs;

/// <summary>One rolled item stack, awaiting the world-item owner's transaction.</summary>
public sealed record NpcDropIntent(int ItemType, int Stack, byte Prefix = 0);
