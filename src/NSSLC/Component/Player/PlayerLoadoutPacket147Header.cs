namespace Terraria.Player;

public readonly record struct PlayerLoadoutPacket147Header(
  Guid CommandId,
  int PacketPlayerIndex,
  int AuthorityPlayerIndex,
  int MainPlayerIndex,
  int TargetLoadoutIndex,
  bool UsingOrReusingItem,
  bool CCed,
  bool Dead)
{
  public PlayerLoadoutSwitchCommand ToSwitchCommand()
  {
    return new PlayerLoadoutSwitchCommand(
      CommandId,
      TargetLoadoutIndex,
      AuthorityPlayerIndex,
      MainPlayerIndex,
      UsingOrReusingItem,
      CCed,
      Dead);
  }
}
