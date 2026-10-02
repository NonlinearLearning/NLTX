namespace Terraria.Player;

public interface IPlayerConnectionNetworkAdapter
{
  PlayerConnectionPacket14Result Apply(
    PlayerIdentityComponent identity,
    PlayerConnectionPacket14Input input);
}
