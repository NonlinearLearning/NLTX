using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Player;

/// <summary>Persistent Player state used by the source DoUnbreakableWallScan gate.</summary>
public readonly record struct PlayerUnbreakableWallState(
  bool Inside,
  int Cooldown,
  PacketVector2 LastPosition) {
  public const int RescanPeriod = 20;
  public const int RescanDistance = 256;
}
