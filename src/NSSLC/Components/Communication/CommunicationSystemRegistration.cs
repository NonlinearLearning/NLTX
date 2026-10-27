using System.Collections.Generic;
using ArchWorld = Arch.Core.World;
using Terraria.Combat;
using Terraria.Entity;
using Terraria.Relationships;

namespace Terraria.Communication;

/// <summary>Creates the ordered ECS registrations for the communication bridge.</summary>
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
      CombatAttackIntentBuffer attackIntents,
      CombatAttackDefinitionCatalog attackDefinitions,
      ICommunicationOutboundPort outbound,
      GameCommunicationOptions? options = null,
      TerrariaProtocolOptions? protocolOptions = null)
  {
    ArgumentNullException.ThrowIfNull(runtimeResolver);
    ArgumentNullException.ThrowIfNull(attackIntents);
    ArgumentNullException.ThrowIfNull(attackDefinitions);
    return Create(
        world,
        new TerrariaProtocolAdmissionHandler(
            new GameCommunicationAdapter(
                world,
                runtimeResolver,
                attackIntents,
                attackDefinitions,
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
