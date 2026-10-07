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

  public byte LuckPotion { get; internal set; }

  public bool HasGardenGnomeNearby { get; internal set; }

  public bool BrokenMirrorBadLuck { get; internal set; }

  public float EquipmentBasedLuckBonus { get; internal set; }

  public bool LuckNeedsSync { get; internal set; }

  public bool ApplyNetworkFactors(
    int ladyBugLuckTime,
    float torchLuck,
    byte luckPotion,
    bool hasGardenGnomeNearby,
    bool brokenMirrorBadLuck,
    float equipmentBasedLuckBonus,
    float coinLuck,
    byte kiteLuckLevel,
    bool usedGalaxyPearl = false,
    bool lanternsUp = false,
    bool stinky = false)
  {
    if (!float.IsFinite(torchLuck) ||
      !float.IsFinite(equipmentBasedLuckBonus) ||
      !float.IsFinite(coinLuck))
    {
      return false;
    }

    LadyBugLuckTimeLeft = ladyBugLuckTime;
    TorchLuck = torchLuck;
    LuckPotion = luckPotion;
    HasGardenGnomeNearby = hasGardenGnomeNearby;
    BrokenMirrorBadLuck = brokenMirrorBadLuck;
    EquipmentBasedLuckBonus = equipmentBasedLuckBonus;
    CoinLuck = coinLuck;
    KiteLuckLevel = kiteLuckLevel;
    Luck = PlayerLuckSystem.Recalculate(
      new PlayerLuckCalculationInput(
        LadyBugLuckTimeLeft: ladyBugLuckTime,
        LadyBugGoodLuckTime: DefaultLadyBugGoodLuckTime,
        LadyBugBadLuckTime: DefaultLadyBugBadLuckTime,
        TorchLuck: torchLuck,
        LuckPotion: luckPotion,
        KiteLuckLevel: kiteLuckLevel,
        UsedGalaxyPearl: usedGalaxyPearl,
        LanternsUp: lanternsUp,
        HasGardenGnomeNearby: hasGardenGnomeNearby,
        Stinky: stinky,
        EquipmentBasedLuckBonus: equipmentBasedLuckBonus,
        CoinLuck: coinLuck,
        BrokenMirrorBadLuck: brokenMirrorBadLuck),
      this);
    LuckNeedsSync = true;
    return true;
  }

  // Terraria's network luck calculation uses NPC.ladyBugGoodLuckTime and
  // NPC.ladyBugBadLuckTime. These are fixed gameplay constants in the current
  // protocol generation; keep them at the calculation seam until the world
  // rules owner exposes a formal configuration query.
  private const int DefaultLadyBugGoodLuckTime = 43_200;
  private const int DefaultLadyBugBadLuckTime = -10_800;

  public int UnbreakableWallScanCooldown { get; internal set; }

  public Vector2 UnbreakableWallScanLastPosition { get; internal set; }
}
