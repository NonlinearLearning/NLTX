using Terraria.Dome.Simulation;

namespace Terraria.Dome.Protocol.V1456.Packets;

public readonly record struct ParticleOrchestraModulePacket(
  byte ParticleType,
  SimulationVector Position,
  SimulationVector Movement,
  int UniqueInfoPiece,
  byte InvokingPlayerSlot);
