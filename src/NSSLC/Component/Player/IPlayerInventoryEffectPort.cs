namespace Terraria.Player;

public interface IPlayerInventoryEffectPort
{
  bool TryApply(in PlayerInventoryEffectIntent intent);
}
