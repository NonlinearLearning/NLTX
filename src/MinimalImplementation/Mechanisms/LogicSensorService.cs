using MinimalImplementation.Server.World;
using MechanismTileCoordinate = Terraria.WorldStorage.TileCoordinate;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Facts needed to evaluate one Version4 logic sensor tick.</summary>
public interface ILogicSensorFactSource
{
  bool IsDay { get; }

  bool IsPlayerAbove(MechanismTileCoordinate anchor);

  WorldTile ReadTile(MechanismTileCoordinate anchor);
}

/// <summary>
/// Evaluates TileEntity type 2 without reading global time, players or tiles implicitly.
/// Tile framing and wire propagation remain explicit effects for the world/mechanism owners.
/// </summary>
public sealed class LogicSensorService
{
  private readonly TileEntityRegistry _registry;

  public LogicSensorService(TileEntityRegistry registry)
  {
    _registry = registry ?? throw new ArgumentNullException(nameof(registry));
  }

  public LogicSensorTickPreparation Prepare(
    ILogicSensorFactSource facts,
    long tick)
  {
    ArgumentNullException.ThrowIfNull(facts);
    ArgumentOutOfRangeException.ThrowIfNegative(tick);
    var changes = new List<LogicSensorCandidate>();
    foreach (TileEntityRuntimeState state in _registry.GetTickEntities())
    {
      if (state.Definition.Type != 2)
      {
        continue;
      }
      bool target = Evaluate(state.LogicCheck, state.Anchor, facts);
      if (target == state.LogicOn)
      {
        continue;
      }
      changes.Add(new LogicSensorCandidate(
        state.Handle,
        state.Revision,
        state.Anchor,
        state.LogicCheck,
        target,
        tick));
    }
    return new LogicSensorTickPreparation(_registry, tick, changes);
  }

  private static bool Evaluate(
    byte check,
    MechanismTileCoordinate anchor,
    ILogicSensorFactSource facts)
  {
    return check switch
    {
      0 => false,
      1 => facts.IsDay,
      2 => !facts.IsDay,
      3 => facts.IsPlayerAbove(anchor),
      4 => IsLiquid(facts.ReadTile(anchor), LiquidType.Water),
      5 => IsLiquid(facts.ReadTile(anchor), LiquidType.Lava),
      6 => IsLiquid(facts.ReadTile(anchor), LiquidType.Honey),
      7 => facts.ReadTile(anchor).LiquidAmount > 0,
      _ => throw new ArgumentOutOfRangeException(nameof(check))
    };
  }

  private static bool IsLiquid(WorldTile tile, LiquidType type)
  {
    return tile.LiquidAmount > 0 && tile.LiquidType == type;
  }
}

public sealed class LogicSensorTickPreparation
{
  private readonly TileEntityRegistry _registry;
  private readonly long _registryRevision;
  private bool _committed;

  internal LogicSensorTickPreparation(
    TileEntityRegistry registry,
    long tick,
    IReadOnlyList<LogicSensorCandidate> candidates)
  {
    _registry = registry;
    _registryRevision = registry.Revision;
    Tick = tick;
    Candidates = Array.AsReadOnly(candidates.ToArray());
  }

  public long Tick { get; }

  public IReadOnlyList<LogicSensorCandidate> Candidates { get; }

  public bool Validate()
  {
    return !_committed && _registry.Revision == _registryRevision &&
      Candidates.All(candidate =>
        _registry.TryGet(candidate.Handle, out TileEntityRuntimeState? state) &&
        state is not null && state.Revision == candidate.ExpectedRevision &&
        state.Definition.Type == 2);
  }

  public LogicSensorTickResult Commit()
  {
    if (!Validate())
    {
      throw new InvalidOperationException("The logic sensor preparation became stale.");
    }

    var applied = new List<LogicSensorChange>(Candidates.Count);
    foreach (LogicSensorCandidate candidate in Candidates)
    {
      if (!_registry.TryGet(candidate.Handle, out TileEntityRuntimeState? state) || state is null ||
          !_registry.TryUpdateState(
            candidate.Handle,
            candidate.ExpectedRevision,
            state.Items,
            state.NpcIndex,
            candidate.LogicCheck,
            candidate.On,
            state.Pose,
            state.AnchorItemType,
            out TileEntityRuntimeState? updated) || updated is null)
      {
        throw new InvalidOperationException("The logic sensor changed during commit.");
      }
      applied.Add(new LogicSensorChange(
        updated.Handle,
        updated.Anchor,
        candidate.LogicCheck,
        candidate.On,
        candidate.On && candidate.LogicCheck == 3));
    }
    _committed = true;
    return new LogicSensorTickResult(Tick, applied);
  }
}

public sealed record LogicSensorCandidate(
  MechanismEntityHandle Handle,
  long ExpectedRevision,
  MechanismTileCoordinate Anchor,
  byte LogicCheck,
  bool On,
  long Tick);

public sealed record LogicSensorChange(
  MechanismEntityHandle Handle,
  MechanismTileCoordinate Anchor,
  byte LogicCheck,
  bool On,
  bool BlocksPlayerTeleportation);

public sealed record LogicSensorTickResult(
  long Tick,
  IReadOnlyList<LogicSensorChange> Changes);
