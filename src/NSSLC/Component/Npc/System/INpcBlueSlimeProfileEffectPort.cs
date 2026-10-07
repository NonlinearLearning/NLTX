namespace Terraria.Npc;

public interface INpcBlueSlimeProfileEffectPort
{
  NpcBlueSlimeTypeOneSelectionResult GenerateContainedItem(bool isBallooned);

  void RequestNetworkSync();

  void RequestTargetReacquire();
}
