using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>
/// Captured visual geometry: Center is the player's actual collision center; CameraPosition is
/// Bottom + (0, gfxOffY - 21) + netOffset. Client-uploaded CameraTarget is a different fact.
/// </summary>
public readonly record struct PlayerSpectatingGeometry(
  PacketVector2 Center,
  PacketVector2 CameraPosition);
