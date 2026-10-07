namespace Terraria.Player;

public interface IPlayerPacket13RouteQuery
{
  PlayerPacket13RouteResult Evaluate(in PlayerPacket13RouteInput input);
}
