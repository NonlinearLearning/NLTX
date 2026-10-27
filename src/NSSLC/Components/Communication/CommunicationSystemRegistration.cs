using System.Collections.Generic;
using ArchWorld = Arch.Core.World;
using Terraria.Entity;

namespace Terraria.Communication;

/// <summary>Creates the ordered ECS registrations for the communication bridge.</summary>
public static class CommunicationSystemRegistration
{
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
