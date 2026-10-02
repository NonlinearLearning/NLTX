namespace Terraria.Player;

public interface IPlayerInventoryCoinMergePort
{
  bool TryApply(in PlayerCoinMergePlan plan);
}
