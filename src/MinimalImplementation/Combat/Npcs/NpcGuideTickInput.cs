using System.Numerics;
using MinimalImplementation.Combat.Identity;
using MinimalImplementation.Contracts;

namespace MinimalImplementation.Combat.Npcs;

/// <summary>Captured facts for the ordinary homeless Guide's idle/walk branch.</summary>
public sealed record NpcGuideTickInput(
    CombatEntityIdentity Entity,
    long SourceNpcRevision,
    ulong SourceWorldEpoch,
    CommitVersion SourceWorldCommitVersion,
    Vector2? ThreatPosition);
