namespace Terraria.Player;

public interface IPlayerRestPacket13Adapter
{
  PlayerRestPacket13Result Apply(
    PlayerRestComponent rest,
    PlayerPettingComponent petting,
    PlayerSleepingComponent sleeping,
    in PlayerRestPacket13Input input);
}
