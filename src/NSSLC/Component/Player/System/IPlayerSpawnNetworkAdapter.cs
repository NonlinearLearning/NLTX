namespace Terraria.Player;

public interface IPlayerSpawnNetworkAdapter
{
  PlayerSpawnPacket12Result Apply(
    PlayerIdentityComponent identity,
    ref PlayerLifecycleComponent lifecycle,
    PlayerDeathRecordComponent deathRecord,
    PlayerRestComponent rest,
    PlayerSittingComponent sitting,
    PlayerSleepingComponent sleeping,
    PlayerSpawnPointComponent spawnPoint,
    PlayerSpawnPacket12Input input);
}
