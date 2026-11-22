using System;
using System.Collections.Generic;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Communication;
using Terraria.Entity;
using Terraria.Relationships;
using Terraria.World;

namespace Terraria.Server;

/// <summary>Creates the ordered ECS registrations for the communication bridge.</summary>
/// <remarks>
/// Assembly wiring belongs to the server host, not to the component library: the library publishes
/// systems, the host decides which of them run and in what order.
/// </remarks>
public static class CommunicationSystemRegistration
{
  /// <summary>
  /// Creates the communication registrations with the gameplay adapter as the packet owner.
  /// The adapter captures domain intents; callers register the corresponding commit systems in
  /// their normal phases.
  /// </summary>
  public static IReadOnlyList<EcsServerSystemRegistration> CreateGame(
      ArchWorld world,
      IEntityRuntimeResolver runtimeResolver,
      CombatAttackDefinitionCatalog attackDefinitions,
      ICommunicationOutboundPort outbound,
      IWorldItemQuery worldItems,
      GameCommunicationOptions? options = null,
      TerrariaProtocolOptions? protocolOptions = null,
      CommunicationSessionIngress? ingress = null,
      ICommunicationSessionPlayerLifecycle? playerLifecycle = null,
      CommunicationIngressPreparationSystem? preparation = null,
      IEnumerable<string>? outboundProjectionDependencies = null)
  {
    ArgumentNullException.ThrowIfNull(runtimeResolver);
    ArgumentNullException.ThrowIfNull(attackDefinitions);
    ArgumentNullException.ThrowIfNull(worldItems);
    return Create(
        world,
        new TerrariaProtocolAdmissionHandler(
            new GameCommunicationAdapter(
                world,
                runtimeResolver,
                attackDefinitions,
                worldItems,
                options),
            protocolOptions),
        outbound,
        ingress,
        playerLifecycle,
        preparation,
        outboundProjectionDependencies);
  }

  public static IReadOnlyList<EcsServerSystemRegistration> Create(
      ArchWorld world,
      ICommunicationPacketHandler packetHandler,
      ICommunicationOutboundPort outbound)
  {
    return Create(world, packetHandler, outbound, null, null);
  }

  public static IReadOnlyList<EcsServerSystemRegistration> Create(
      ArchWorld world,
      ICommunicationPacketHandler packetHandler,
      ICommunicationOutboundPort outbound,
      CommunicationSessionIngress? ingress,
      ICommunicationSessionPlayerLifecycle? playerLifecycle,
      CommunicationIngressPreparationSystem? preparation = null,
      IEnumerable<string>? outboundProjectionDependencies = null)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(packetHandler);
    ArgumentNullException.ThrowIfNull(outbound);
    List<EcsServerSystemRegistration> registrations = [];
    if (preparation is null && ingress is not null)
    {
      preparation = new CommunicationIngressPreparationSystem(
          world,
          ingress,
          playerLifecycle);
    }

    if (preparation is not null)
    {
      registrations.Add(new EcsServerSystemRegistration(
          EcsServerPhase.Preparation,
          EcsServerRegistrationKeys.CommunicationIngressPreparation,
          preparation));
    }

    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Preparation,
        EcsServerRegistrationKeys.CommunicationSessionTick,
        new CommunicationSessionTickSystem(world, packetHandler),
        after: preparation is null
            ? null
            : [EcsServerRegistrationKeys.CommunicationIngressPreparation],
        reason: preparation is null
            ? null
            : "Ingress facts must be captured before session packet handling reads them."));
    if (preparation is not null)
    {
      registrations.Add(new EcsServerSystemRegistration(
          EcsServerPhase.Commit,
          EcsServerRegistrationKeys.CommunicationSessionCommit,
          new CommunicationSessionCommitSystem(world, preparation)));
    }

    registrations.Add(new EcsServerSystemRegistration(
        EcsServerPhase.Projection,
        EcsServerRegistrationKeys.CommunicationOutboundProjection,
        new CommunicationOutboundProjectionSystem(world, outbound),
        after: outboundProjectionDependencies,
        reason: outboundProjectionDependencies is null
            ? null
            : "All gameplay outbound projections must populate session buffers before delivery."));
    return registrations.AsReadOnly();
  }
}
