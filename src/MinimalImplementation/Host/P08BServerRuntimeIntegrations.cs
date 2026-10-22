using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MinimalImplementation.Contracts;
using MinimalImplementation.Persistence;
using MinimalImplementation.Server.Protocol;

namespace MinimalImplementation.Server.Host;

/// <summary>
/// Supplies the real transport dispatcher and domain tick owners that are outside the P08-B host
/// boundary. The runtime refuses to synthesize either integration when this value is absent.
/// </summary>
public sealed class P08BServerRuntimeIntegrations
{
  public P08BServerRuntimeIntegrations(
    Func<SessionEpoch> allocateEpoch,
    Func<InboundRawFrame, InboundPacketEnvelope> createEnvelope,
    Func<InboundVersion4Packet, CancellationToken, ValueTask> onPacket,
    Func<ConnectionClosed, ValueTask> onClosed,
    Func<Exception, ValueTask> onConnectionFailure,
    IReadOnlyCollection<IServerTickPhaseOwner> tickPhaseOwners,
    Func<bool>? hasActiveClients = null,
    Func<SessionEpoch, ValueTask>? onConnected = null)
  {
    ArgumentNullException.ThrowIfNull(allocateEpoch);
    ArgumentNullException.ThrowIfNull(createEnvelope);
    ArgumentNullException.ThrowIfNull(onPacket);
    ArgumentNullException.ThrowIfNull(onClosed);
    ArgumentNullException.ThrowIfNull(onConnectionFailure);
    ArgumentNullException.ThrowIfNull(tickPhaseOwners);

    AllocateEpoch = allocateEpoch;
    CreateEnvelope = createEnvelope;
    OnPacket = onPacket;
    OnClosed = onClosed;
    OnConnectionFailure = onConnectionFailure;
    TickPhaseOwners = Array.AsReadOnly(new List<IServerTickPhaseOwner>(tickPhaseOwners).ToArray());
    HasActiveClients = hasActiveClients;
    OnConnected = onConnected;
  }

  public Func<SessionEpoch> AllocateEpoch { get; }

  public Func<InboundRawFrame, InboundPacketEnvelope> CreateEnvelope { get; }

  public Func<InboundVersion4Packet, CancellationToken, ValueTask> OnPacket { get; }

  public Func<ConnectionClosed, ValueTask> OnClosed { get; }

  public Func<Exception, ValueTask> OnConnectionFailure { get; }

  public IReadOnlyCollection<IServerTickPhaseOwner> TickPhaseOwners { get; }

  public Func<bool>? HasActiveClients { get; }

  public Func<SessionEpoch, ValueTask>? OnConnected { get; }
}
