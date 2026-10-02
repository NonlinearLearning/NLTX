using System;

namespace Terraria.WorldGeneration.Components;

public readonly record struct WorldGenerationRandomStateComponent
{
  public WorldGenerationRandomStateComponent(
    long generationId,
    uint state,
    int streamVersion,
    string? passId = null,
    long cursor = 0)
  {
    if (generationId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(generationId));
    }

    if (streamVersion <= 0)
    {
      throw new ArgumentOutOfRangeException(nameof(streamVersion));
    }

    if (cursor < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(cursor));
    }

    if (passId is not null)
    {
      ArgumentException.ThrowIfNullOrWhiteSpace(passId);
    }

    GenerationId = generationId;
    State = state;
    StreamVersion = streamVersion;
    PassId = passId;
    Cursor = cursor;
  }

  public long GenerationId { get; }

  public uint State { get; }

  public int StreamVersion { get; }

  public string? PassId { get; }

  public long Cursor { get; }
}
