namespace Terraria.Player;

public static class PlayerPacket13RouteQuery
{
  public static PlayerPacket13RouteResult Evaluate(in PlayerPacket13RouteInput input)
  {
    if (!input.IsServerSideCharacter && input.DeclaredPlayerSlot == input.LocalPlayerSlot)
    {
      return new PlayerPacket13RouteResult(
        PlayerPacket13RouteDecision.IgnoreSelfEcho,
        null);
    }

    return new PlayerPacket13RouteResult(
      PlayerPacket13RouteDecision.ApplyToSender,
      input.SenderPlayerSlot);
  }
}
