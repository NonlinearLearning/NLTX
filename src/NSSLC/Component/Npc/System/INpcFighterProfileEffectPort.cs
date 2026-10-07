namespace Terraria.Npc;

public interface INpcFighterProfileEffectPort
{
  void EncourageDespawn(int ticks);

  void RequestTargetReacquire();

  void RequestNetworkSync();

  void RequestOpenDoor(int tileX, int tileY, int direction);
}
