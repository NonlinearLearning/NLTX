namespace Terraria.Items.InventoryContainers;

public sealed class TransferCompletionEffectPort
{
  private readonly HashSet<string> _completedOperations = new();
  private readonly Action<TransferCompletionEffect> _apply;

  public TransferCompletionEffectPort(Action<TransferCompletionEffect> apply)
  {
    _apply = apply ?? throw new ArgumentNullException(nameof(apply));
  }

  public bool ApplyOnce(TransferCompletionEffect effect)
  {
    ArgumentNullException.ThrowIfNull(effect);

    if (!_completedOperations.Add(effect.OperationKey))
    {
      return false;
    }

    _apply.Invoke(effect);
    return true;
  }
}
