namespace Terraria.WorldGeneration.Components;

/// <summary>
/// Identifies one external dungeon-generation record without copying its unverified fields.
/// </summary>
public readonly record struct DungeonRecordSnapshot(int RecordId);
