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
      TerrariaProtocolOptions? protocolOptions = null)
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
        outbound);
  }

  public static IReadOnlyList<EcsServerSystemRegistration> Create(
      ArchWorld world,
      ICommunicationPacketHandler packetHandler,
      ICommunicationOutboundPort outbound)
  {
    ArgumentNullException.ThrowIfNull(world);
    ArgumentNullException.ThrowIfNull(packetHandler);
    ArgumentNullException.ThrowIfNull(outbound);
    return Array.AsReadOnly(new EcsServerSystemRegistration[]
    {
      new EcsServerSystemRegistration(
          EcsServerPhase.Preparation,
          new CommunicationSessionTickSystem(world, packetHandler)),
      new EcsServerSystemRegistration(
          EcsServerPhase.Projection,
          new CommunicationOutboundProjectionSystem(world, outbound))
    });
  }
}
