using System.Numerics;

namespace Terraria.Player.Luck;

// Scope this port to the player whose rescan and environment state is passed to the System.
public interface IPlayerWallRescanPort
{
  bool ScanInsideUnbreakableWalls(Vector2 playerCenter);

  void BroadcastChange();
}
