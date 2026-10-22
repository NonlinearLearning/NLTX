using MinimalImplementation.Server.Identity;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

public sealed record PlayerTargetRelation(
  PacketVector2? MinionRestTarget,
  EntityHandle? MinionAttackTarget)
{
  public static PlayerTargetRelation Empty => new(null, null);
}
