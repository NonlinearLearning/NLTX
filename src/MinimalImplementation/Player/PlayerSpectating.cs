using MinimalImplementation.Server.Identity;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

public readonly record struct PlayerSpectating(
  EntityHandle? Target,
  PacketVector2? CameraPosition = null)
{
  public static PlayerSpectating None => new(null);

  public bool IsSpectating => Target is not null;
}
