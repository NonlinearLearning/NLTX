using System.Numerics;

namespace Terraria.Player.Luck;

// status: implemented-partial
// componentId: PLAYER.COMP.LUCK_AND_RESCAN_STATE
// source-members: P08-1386..P08-1394, P08-1399..P08-1400
// excluded-members: P08-1390..P08-1391 (luck rules/config seam), P08-1395 (P09 capability owner),
// P08-1396 (P07/P03 cache owner), P08-1397..P08-1398 (static scan rules), P08-1401 (audio adapter)
// crossSubsystemOwner: integration-review
public sealed class PlayerLuckAndRescanStateComponent
{
  public float TorchLuck { get; internal set; }

  public bool HappyFunTorchTime { get; internal set; }

  public int LadyBugLuckTimeLeft { get; internal set; }

  public float Luck { get; internal set; }

  public float CoinLuck { get; internal set; }

  public byte KiteLuckLevel { get; internal set; }

  public bool LuckNeedsSync { get; internal set; }

  public int UnbreakableWallScanCooldown { get; internal set; }

  public Vector2 UnbreakableWallScanLastPosition { get; internal set; }
}
