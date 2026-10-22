using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;
using MinimalImplementation.Server.Player;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using PlayerEntity = MinimalImplementation.Server.Player.Player;

namespace MinimalImplementation.Server.Session;

/// <summary>Coordinates one idempotent session close on the server commit loop.</summary>
public sealed class SessionCloseCoordinator
{
  private readonly SessionAdmissionCoordinator _sessions;
  private readonly SessionBindingStore _bindings;
  private readonly PlayerOwner _players;
  private readonly PlayerSpectatingCoordinator? _spectating;
  private readonly Dictionary<SessionEpoch, SessionCloseResult> _completed = [];

  public SessionCloseCoordinator(
    SessionAdmissionCoordinator sessions,
    SessionBindingStore bindings,
    PlayerOwner players,
    PlayerSpectatingCoordinator? spectating = null)
  {
    ArgumentNullException.ThrowIfNull(sessions);
    ArgumentNullException.ThrowIfNull(bindings);
    ArgumentNullException.ThrowIfNull(players);
    _sessions = sessions;
    _bindings = bindings;
    _players = players;
    _spectating = spectating;
  }

  /// <summary>
  /// Runs dependent cleanup synchronously after entering Closing and before retiring the binding.
  /// Cleanup must allow retries: rejection keeps the player and binding in Closing, and a completed
  /// close replay returns the captured context without running cleanup or publishing again.
  /// Output preparation runs before dependency cleanup, observer/section writes and retirement.
  /// It must not mutate inputs or owners, perform I/O or reenter sessions. A rejection keeps
  /// Closing admission closed and exposes no lifecycle output; reserved versions may have gaps.
  /// </summary>
  public SessionCloseResult Close(
    ConnectionClosed closed,
    IReadOnlyCollection<SessionEpoch> activeRecipients,
    CommitVersion commitVersion,
    Func<SessionCloseContext, OwnerCommitResult>? releaseDependencies = null,
    Func<IReadOnlyList<SessionPacketProjection>, OwnerCommitResult>? prepareOutput = null)
  {
    ArgumentNullException.ThrowIfNull(closed);
    ArgumentNullException.ThrowIfNull(activeRecipients);
    if (!commitVersion.IsValid)
    {
      throw new ArgumentOutOfRangeException(nameof(commitVersion));
    }

    if (_completed.TryGetValue(closed.Epoch, out SessionCloseResult? prior))
    {
      // Replaying a prior 150 could overwrite an observer's newer spectating choice.
      return prior with { Projections = Array.Empty<SessionPacketProjection>(), IsReplay = true };
    }

    OwnerCommitResult begin = _sessions.BeginClosing(
      closed.Epoch,
      closed.LastReceivedSequence);
    if (begin.Status != OwnerCommitStatus.Applied
      && (!_sessions.TryGetStage(closed.Epoch, out SessionStage stage)
        || stage != SessionStage.Closing)) {
      return SessionCloseResult.Rejected(begin);
    }

    // BeginClosing reserves the session's authoritative close version. Use that version for
    // lifecycle projections; the caller's value is retained only for retries that observe an
    // already-Closing session and therefore have no new begin result.
    CommitVersion projectionVersion = begin.Version ?? commitVersion;

    _bindings.TryGetByEpoch(closed.Epoch, out SessionBinding? binding);
    var context = new SessionCloseContext(closed, binding);
    var projections = new List<SessionPacketProjection>();
    var spectatingRejections = new Dictionary<EntityHandle, OwnerCommitResult>();
    PlayerEntity? player = null;
    if (binding is not null) {
      _players.TryGetForBinding(binding, out player);
    }

    if (player is not null && binding is not null && activeRecipients.Count > 0) {
      projections.Add(new SessionPacketProjection(
        SessionAdmissionCoordinator.PlayerActiveMessageId,
        new Terraria.NetWork.Prototype.PacketDesignCompiler.Sample.PlayerActivePacket {
          Player = binding.PlayerSlot,
          ActiveState = 0
        },
        activeRecipients,
        closed.Epoch,
        projectionVersion));

      if (_sessions.HasConnectionAnnouncement(closed.Epoch)) {
        projections.Add(new SessionPacketProjection(
          SessionAdmissionCoordinator.NetModulesMessageId,
          new NetModulesPacket {
            ModuleId = (ushort)Packet82ModuleId.Text,
            Data = new Packet82TextData(byte.MaxValue,
              NetworkText.Key("LegacyMultiplayer.20",
                NetworkText.Literal(player.Appearance.Name)),
              255, 240, 20)
          },
          activeRecipients, closed.Epoch, projectionVersion));
      }
    }

    OwnerCommitResult prepared;
    if (player is not null && _spectating is not null) {
      prepared = _spectating.TryRepairAfterClose(player.Handle,
        repairs => {
          foreach (var repair in repairs) {
            if (repair.Value.Result.Status == OwnerCommitStatus.Applied) {
              projections.AddRange(repair.Value.Projections);
            } else {
              spectatingRejections.Add(repair.Key, repair.Value.Result);
            }
          }

          return PrepareOutputAndReleaseDependencies();
        }, out _);
    } else {
      prepared = PrepareOutputAndReleaseDependencies();
    }

    if (prepared.Status != OwnerCommitStatus.Applied) {
      return SessionCloseResult.Rejected(prepared, context);
    }

    if (player is not null && _spectating is null) {
      // The fallback clears stale handles; nearest-target repair needs authoritative adapters.
      _players.ClearSpectatingTarget(player.Handle);
    }

    if (binding is not null) {
      // World replacement may already have invalidated the registry handle. The epoch-owned
      // dictionaries still need retirement even when the binding no longer resolves a player.
      _players.TryRetire(closed.Epoch);
      _bindings.TryRemove(closed.Epoch, out _);
    }

    OwnerCommitResult completed = _sessions.CompleteClosing(closed.Epoch);
    SessionCloseResult result = completed.Status == OwnerCommitStatus.Applied
      ? (SessionCloseResult.Completed(completed, projections.AsReadOnly(), context) with {
        SpectatingRejections = new ReadOnlyDictionary<EntityHandle, OwnerCommitResult>(
          spectatingRejections)
      })
      : SessionCloseResult.Rejected(completed, context);
    if (result.IsCompleted) {
      _completed[closed.Epoch] = result;
    }
    return result;

    OwnerCommitResult PrepareOutputAndReleaseDependencies() {
      OwnerCommitResult? output = prepareOutput?.Invoke(projections.AsReadOnly());
      if (output is not null && output.Status != OwnerCommitStatus.Applied) {
        return output;
      }

      return releaseDependencies?.Invoke(context) ?? OwnerCommitResult.Applied(commitVersion);
    }
  }
}

public sealed record SessionCloseResult(
  OwnerCommitResult Result,
  IReadOnlyList<SessionPacketProjection> Projections,
  bool IsCompleted)
{
  public bool IsReplay { get; init; }

  public SessionCloseContext? Context { get; init; }

  public IReadOnlyDictionary<EntityHandle, OwnerCommitResult> SpectatingRejections { get; init; }
    = new ReadOnlyDictionary<EntityHandle, OwnerCommitResult>(
      new Dictionary<EntityHandle, OwnerCommitResult>());

  internal static SessionCloseResult Completed(
    OwnerCommitResult result,
    IReadOnlyList<SessionPacketProjection> projections,
    SessionCloseContext context)
  {
    return new SessionCloseResult(result, projections, true) { Context = context };
  }

  internal static SessionCloseResult Rejected(
      OwnerCommitResult result, SessionCloseContext? context = null)
  {
    return new SessionCloseResult(result, Array.Empty<SessionPacketProjection>(), false) {
      Context = context
    };
  }
}
