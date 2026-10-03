using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

public sealed record PacketSnapshotCacheKey(string ProfileKey, Guid WorldKey,
    long WorldGeneration, SectionCoordinate Section, long SnapshotRevision, string Variant);
