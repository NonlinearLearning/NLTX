namespace Terraria.Player;

public interface IPlayerInventoryCommitPort
{
  bool TryApply(in PlayerInventoryCommitPlan plan);
}
