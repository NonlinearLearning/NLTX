namespace Terraria.Player;

public interface IPlayerVitalPacket16Adapter
{
  void Apply(
    PlayerIdentityComponent identity,
    PlayerVitalStateComponent vital,
    ref PlayerLifecycleComponent lifecycle,
    in PlayerVitalPacket16Input input);
}
