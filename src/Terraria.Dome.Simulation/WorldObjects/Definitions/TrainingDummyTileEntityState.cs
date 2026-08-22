using System;
using System.Buffers.Binary;

namespace Terraria.Dome.Simulation.WorldObjects.Definitions;

public readonly record struct TrainingDummyTileEntityState(
  int EntityId,
  int TileX,
  int TileY,
  short NpcId)
{
  private const byte TrainingDummyEntityType = 0;
  private const int NpcPayloadLength = 2;

  public static bool TryRead(
    TileEntityPersistentState source,
    out TrainingDummyTileEntityState state)
  {
    if (source.IsOpaque ||
        source.Type != TrainingDummyEntityType ||
        source.Payload.Count != NpcPayloadLength)
    {
      state = default;
      return false;
    }

    short npcId = BinaryPrimitives.ReadInt16LittleEndian(
      new ReadOnlySpan<byte>(source.Payload is byte[] bytes ? bytes : [.. source.Payload]));
    state = new TrainingDummyTileEntityState(
      source.Id,
      source.TileX,
      source.TileY,
      npcId);
    return true;
  }
}
