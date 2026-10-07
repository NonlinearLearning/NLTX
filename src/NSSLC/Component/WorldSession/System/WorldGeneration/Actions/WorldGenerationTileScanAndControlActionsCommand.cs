using System;
using System.Collections.Generic;
using System.Collections.Frozen;

using Terraria.WorldGeneration.Components;

namespace Terraria.WorldGeneration.Actions;

public readonly record struct WorldGenerationTileScanAndControlActionsCommand
{
  public enum OperationKind : byte
  {
    Continue,
    Count,
    Scanner,
    TileScanner,
    Custom,
    UpdateBounds,
  }

  public interface IContinuation
  {
    bool Continue(TilePosition position);
  }

  public interface ICountResultSink
  {
    void Add(int value);
  }

  public interface IPerUnitAction
  {
    bool Apply(TilePosition position);
  }

  public interface IBoundsReference
  {
  }

  public sealed class TileCountAccumulator
  {
    private readonly ushort[] _tileIds;
    private readonly Dictionary<ushort, int> _counts;

    public TileCountAccumulator(IReadOnlyCollection<ushort> tileIds)
    {
      ArgumentNullException.ThrowIfNull(tileIds);

      HashSet<ushort> uniqueTileIds = new();
      List<ushort> orderedTileIds = new();
      foreach (ushort tileId in tileIds)
      {
        if (uniqueTileIds.Add(tileId))
        {
          orderedTileIds.Add(tileId);
        }
      }

      _tileIds = orderedTileIds.ToArray();
      _counts = new Dictionary<ushort, int>(_tileIds.Length);
      foreach (ushort tileId in _tileIds)
      {
        _counts.Add(tileId, 0);
      }

      TileIds = Array.AsReadOnly(_tileIds);
    }

    public IReadOnlyList<ushort> TileIds { get; }

    public void Record(ushort tileId)
    {
      TryRecord(tileId);
    }

    public bool TryRecord(ushort tileId)
    {
      if (!_counts.TryGetValue(tileId, out int count))
      {
        return false;
      }

      _counts[tileId] = count + 1;
      TotalMatches++;
      return true;
    }

    public int TotalMatches { get; private set; }

    public TileCountSnapshot CreateSnapshot()
    {
      return new TileCountSnapshot(_counts.ToFrozenDictionary(), TotalMatches);
    }

    public void Reset()
    {
      foreach (ushort tileId in _tileIds)
      {
        _counts[tileId] = 0;
      }

      TotalMatches = 0;
    }
  }

  public readonly record struct TileCountSnapshot(
    IReadOnlyDictionary<ushort, int> Counts,
    int TotalMatches);

  private WorldGenerationTileScanAndControlActionsCommand(
    OperationKind kind,
    IContinuation? continuation,
    ICountResultSink? countSink,
    TileCountAccumulator? tileAccumulator,
    IPerUnitAction? perUnitAction,
    IBoundsReference? bounds)
  {
    Kind = kind;
    Continuation = continuation;
    CountSink = countSink;
    TileAccumulator = tileAccumulator;
    PerUnitAction = perUnitAction;
    Bounds = bounds;
  }

  public OperationKind Kind { get; }

  public IContinuation? Continuation { get; }

  public ICountResultSink? CountSink { get; }

  public TileCountAccumulator? TileAccumulator { get; }

  public IPerUnitAction? PerUnitAction { get; }

  public IBoundsReference? Bounds { get; }

  public static WorldGenerationTileScanAndControlActionsCommand Continue(
    IContinuation continuation)
  {
    ArgumentNullException.ThrowIfNull(continuation);
    return new WorldGenerationTileScanAndControlActionsCommand(
      OperationKind.Continue,
      continuation,
      null,
      null,
      null,
      null);
  }

  public static WorldGenerationTileScanAndControlActionsCommand Count(
    ICountResultSink countSink)
  {
    ArgumentNullException.ThrowIfNull(countSink);
    return new WorldGenerationTileScanAndControlActionsCommand(
      OperationKind.Count,
      null,
      countSink,
      null,
      null,
      null);
  }

  public static WorldGenerationTileScanAndControlActionsCommand Scanner(
    ICountResultSink countSink)
  {
    ArgumentNullException.ThrowIfNull(countSink);
    return new WorldGenerationTileScanAndControlActionsCommand(
      OperationKind.Scanner,
      null,
      countSink,
      null,
      null,
      null);
  }

  public static WorldGenerationTileScanAndControlActionsCommand TileScanner(
    TileCountAccumulator tileAccumulator)
  {
    ArgumentNullException.ThrowIfNull(tileAccumulator);
    return new WorldGenerationTileScanAndControlActionsCommand(
      OperationKind.TileScanner,
      null,
      null,
      tileAccumulator,
      null,
      null);
  }

  public static WorldGenerationTileScanAndControlActionsCommand Custom(
    IPerUnitAction perUnitAction)
  {
    ArgumentNullException.ThrowIfNull(perUnitAction);
    return new WorldGenerationTileScanAndControlActionsCommand(
      OperationKind.Custom,
      null,
      null,
      null,
      perUnitAction,
      null);
  }

  public static WorldGenerationTileScanAndControlActionsCommand UpdateBounds(
    IBoundsReference bounds)
  {
    ArgumentNullException.ThrowIfNull(bounds);
    return new WorldGenerationTileScanAndControlActionsCommand(
      OperationKind.UpdateBounds,
      null,
      null,
      null,
      null,
      bounds);
  }

  public bool IsWellFormed =>
    Kind switch
    {
      OperationKind.Continue => Continuation is not null && HasNoOtherPayloads(),
      OperationKind.Count or OperationKind.Scanner =>
        CountSink is not null && HasNoOtherPayloads(),
      OperationKind.TileScanner => TileAccumulator is not null && HasNoOtherPayloads(),
      OperationKind.Custom => PerUnitAction is not null && HasNoOtherPayloads(),
      OperationKind.UpdateBounds => Bounds is not null && HasNoOtherPayloads(),
      _ => false,
    };

  public void Validate()
  {
    if (!IsWellFormed)
    {
      throw new ArgumentException(
        "The tile scan or control command contains an unsupported or incomplete operation.",
        nameof(Kind));
    }
  }

  private bool HasNoOtherPayloads()
  {
    int payloadCount = 0;
    payloadCount += Continuation is null ? 0 : 1;
    payloadCount += CountSink is null ? 0 : 1;
    payloadCount += TileAccumulator is null ? 0 : 1;
    payloadCount += PerUnitAction is null ? 0 : 1;
    payloadCount += Bounds is null ? 0 : 1;
    return payloadCount == 1;
  }
}
