namespace Terraria.Player;

public readonly record struct PlayerLoadoutPacket147ProcessResult(
  PlayerLoadoutPacket147ProcessStatus Status,
  PlayerLoadoutPacket147Header? Header,
  PlayerLoadoutSwitchResult Loadout,
  PlayerAccessoryVisibilityResult Visibility,
  bool VisibilityReadAfterLoadout)
{
  public bool Applied => Status == PlayerLoadoutPacket147ProcessStatus.Applied;
}
