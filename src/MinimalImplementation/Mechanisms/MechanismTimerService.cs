using MinimalImplementation.Contracts;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Prepares one UpdateMech step; the host owns admission and serialization.</summary>
public sealed class MechanismTimerService
{
  public sealed record TickResult(
    long Tick,
    CommitVersion CommitVersion,
    IReadOnlyList<TileCoordinate> VisitedTiles,
    IReadOnlyList<OutboundPacketIntent> OutboundIntents);

  public sealed class TickPreparation
  {
    private readonly MechanismTimerService _owner;
    private readonly long _baseTick;
    private readonly WorldTileMutationPreparation _world;
    private readonly MechanismCooldownService.Preparation _cooldowns;
    private bool _committed;

    internal TickPreparation(
      MechanismTimerService owner,
      long baseTick,
      WorldTileMutationPreparation world,
      MechanismCooldownService.Preparation cooldowns,
      TickResult result)
    {
      _owner = owner;
      _baseTick = baseTick;
      _world = world;
      _cooldowns = cooldowns;
      Result = result;
    }

    /// <summary>Tentative output: admit before Commit, then publish after Commit.</summary>
    public TickResult Result { get; }

    public bool Validate()
    {
      return !_committed && _owner._lastCommittedTick == _baseTick &&
        _cooldowns.Validate() && _world.Validate().IsValid;
    }

    /// <summary>The host retains its exclusive owner window through validation and commit.</summary>
    public TickResult Commit()
    {
      if (!Validate())
      {
        throw new InvalidOperationException("The prepared mechanism tick is stale.");
      }
      // No callbacks, encoding or external admission may occur between these participants.
      _world.Commit();
      _cooldowns.Commit();
      _owner._lastCommittedTick = Result.Tick;
      _committed = true;
      return Result;
    }
  }

  private readonly MechanismWorldPort _world;
  private readonly MechanismCooldownService _cooldowns;
  private readonly WiringPropagationService _wiring;
  private readonly MechanismPacketProducer _producer;
  private long _lastCommittedTick = -1;

  public MechanismTimerService(
    MechanismWorldPort world,
    MechanismCooldownService cooldowns,
    WiringPropagationService wiring,
    MechanismPacketProducer producer)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _cooldowns = cooldowns ?? throw new ArgumentNullException(nameof(cooldowns));
    _wiring = wiring ?? throw new ArgumentNullException(nameof(wiring));
    _producer = producer ?? throw new ArgumentNullException(nameof(producer));
  }

  public bool TryPrepareTick(
    long tick,
    IReadOnlyCollection<SessionEpoch> recipients,
    out TickPreparation? preparation,
    out string? rejection)
  {
    ArgumentNullException.ThrowIfNull(recipients);
    if (tick < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tick));
    }
    preparation = null;
    if (tick <= _lastCommittedTick)
    {
      rejection = "The mechanism tick was already committed or superseded.";
      return false;
    }
    WorldSnapshot before = _world.CaptureSnapshot();
    if (before.WorldEpoch != (ulong)_cooldowns.WorldScope)
    {
      rejection = "The timer service belongs to a different world.";
      return false;
    }
    long baseTick = _lastCommittedTick;
    MechanismCooldownService.Preparation cooldowns = _cooldowns.Prepare();
    WiringPropagationPlan plan = _wiring.PlanTimerTick(before, cooldowns);
    if (!plan.Accepted)
    {
      rejection = plan.Rejection;
      return false;
    }
    // Empty tile updates still bind P03's version, liquid queue and dirty state.
    if (!_world.TryPrepareTileDeltas(
      before.WorldEpoch,
      before.Version,
      plan.Deltas.OrderBy(pair => pair.Key.Y).ThenBy(pair => pair.Key.X).ToArray(),
      producesImmediateBroadcast: true,
      out WorldTileMutationPreparation? world,
      out _) || world is null || world.Result.Version is not CommitVersion version)
    {
      rejection = "The P03 world rejected the timer preparation.";
      return false;
    }
    IReadOnlyList<OutboundPacketIntent> intents = _producer.CreateWiringIntents(
      before, world, recipients.Distinct().ToArray(), plan.TileSquareEffects);
    var result = new TickResult(tick, version, plan.VisitedTiles, intents);
    preparation = new TickPreparation(this, baseTick, world, cooldowns, result);
    rejection = null;
    return true;
  }
}
