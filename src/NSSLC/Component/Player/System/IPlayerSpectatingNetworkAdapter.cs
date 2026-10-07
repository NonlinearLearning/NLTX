namespace Terraria.Player;

public interface IPlayerSpectatingNetworkAdapter
{
  PlayerSpectatingPacket150Result Apply(
    ref PlayerLifecycleComponent lifecycle,
    PlayerSpectatingPacket150Input input);
}
