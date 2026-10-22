using System;
using System.Collections.Generic;
using MinimalImplementation.Persistence;
using MinimalImplementation.Server.World;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Registers Time and World as one scheduler entry backed by one WorldTickCoordinator commit.
/// </summary>
/// <remarks>
/// The input factory is deliberately explicit. Until the host has an authoritative source for
/// the world epoch, elapsed game ticks, day rate and liquid budget, returning null keeps this
/// boundary NotReady instead of inventing defaults or publishing a successful no-op tick.
/// </remarks>
public sealed class WorldTickPhaseCompositeOwner : IServerTickPhaseCompositeOwner
{
  private readonly WorldTickCoordinator _coordinator;
  private readonly Func<long, WorldTickPhaseInput?> _inputFactory;
  private readonly WorldTickInvasionBridge? _invasionBridge;
  private readonly bool _applyInvasionBridge;
  private readonly IReadOnlyList<IServerTickPhaseParticipant> _participants;

  public WorldTickPhaseCompositeOwner(
    WorldTickCoordinator coordinator,
    Func<long, WorldTickPhaseInput?> inputFactory,
    WorldTickInvasionBridge? invasionBridge = null,
    bool applyInvasionBridge = true)
  {
    ArgumentNullException.ThrowIfNull(coordinator);
    ArgumentNullException.ThrowIfNull(inputFactory);
    _coordinator = coordinator;
    _inputFactory = inputFactory;
    _invasionBridge = invasionBridge;
    _applyInvasionBridge = applyInvasionBridge;
    _participants = Array.AsReadOnly(
      new IServerTickPhaseParticipant[]
    {
      new Participant(ServerTickPhase.Time, coordinator),
      new Participant(ServerTickPhase.World, coordinator)
    });
  }

  public ServerTickPhase Phase => ServerTickPhase.Time;

  public object TransactionIdentity => _coordinator;

  public IReadOnlyList<IServerTickPhaseParticipant> Participants => _participants;

  public WorldTickResult? LastResult { get; private set; }

  public void Advance(long gameTickIndex)
  {
    ArgumentOutOfRangeException.ThrowIfNegative(gameTickIndex);
    WorldTickPhaseInput? input = _inputFactory.Invoke(gameTickIndex);
    if (input is null)
    {
      LastResult = null;
      throw new InvalidOperationException(
        "The Time/World tick composite is NotReady: no authoritative tick input was supplied.");
    }

    WorldTickResult result = _coordinator.Advance(
      input.WorldEpoch,
      gameTickIndex,
      input.ElapsedGameTicks,
      input.DayRate,
      input.LiquidOperationBudget);
    LastResult = result;
    if (!result.Applied)
    {
      string detail = result.Rejection ?? "The world tick coordinator rejected the transaction.";
      throw new InvalidOperationException($"The Time/World tick composite was rejected: {detail}");
    }

    if (_applyInvasionBridge)
    {
      _invasionBridge?.Consume(result);
    }
  }

  private sealed class Participant : IServerTickPhaseParticipant
  {
    public Participant(ServerTickPhase phase, object transactionIdentity)
    {
      Phase = phase;
      TransactionIdentity = transactionIdentity;
    }

    public ServerTickPhase Phase { get; }

    public object TransactionIdentity { get; }
  }
}
