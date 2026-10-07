namespace Terraria.Player;

// Composes the legacy GetItem/FillAmmo/DoCoins behavior without owning Item payloads
// or executing sound, text, logging, achievement, or post-action effects.
public sealed class PlayerInventoryPickupSystem
{
  private readonly PlayerInventoryCommitSystem _inventorySystem;
  private readonly PlayerCoinMergeSystem _coinMergeSystem;
  private readonly IPlayerInventoryEffectPort _effectPort;

  public PlayerInventoryPickupSystem(
    PlayerInventoryCommitSystem inventorySystem,
    PlayerCoinMergeSystem coinMergeSystem,
    IPlayerInventoryEffectPort effectPort)
  {
    ArgumentNullException.ThrowIfNull(inventorySystem);
    ArgumentNullException.ThrowIfNull(coinMergeSystem);
    ArgumentNullException.ThrowIfNull(effectPort);

    _inventorySystem = inventorySystem;
    _coinMergeSystem = coinMergeSystem;
    _effectPort = effectPort;
  }

  public PlayerInventoryPickupResult Process(
    in PlayerInventoryPickupCommand command)
  {
    PlayerInventoryCommitResult inventoryResult =
      _inventorySystem.Commit(
        new PlayerInventoryCommitCommand(
          command.CommandId,
          command.Item,
          command.Candidate),
        command.Settings.CanGoIntoVoidVault);

    if (!inventoryResult.Applied)
    {
      return PlayerInventoryPickupResult.Rejected(
        command.Item.Stack,
        inventoryResult,
        command.CommandId == Guid.Empty
          ? PlayerInventoryPickupRejectionReason.EmptyCommand
          : PlayerInventoryPickupRejectionReason.InventoryCommitRejected);
    }

    if (!TryApplyIntent(
      command,
      PlayerInventoryEffectIntentKind.PickupSound,
      inventoryResult,
      amount: inventoryResult.AcceptedStack,
      enabled: !command.Settings.NoSound))
    {
      return EffectRejected(command, inventoryResult);
    }

    if (!TryApplyIntent(
      command,
      PlayerInventoryEffectIntentKind.PickupLog,
      inventoryResult,
      amount: inventoryResult.AcceptedStack,
      enabled: true))
    {
      return EffectRejected(command, inventoryResult);
    }

    if (!TryApplyIntent(
      command,
      PlayerInventoryEffectIntentKind.PickupText,
      inventoryResult,
      amount: inventoryResult.AcceptedStack,
      enabled: !command.Settings.NoText))
    {
      return EffectRejected(command, inventoryResult);
    }

    PlayerCoinMergeResult coinResult = default;
    bool coinMergeAttempted = false;
    if (command.Item.IsCoin &&
      !command.Settings.NoCoinMerge &&
      inventoryResult.Target.IsMainInventory)
    {
      coinMergeAttempted = true;
      coinResult = _coinMergeSystem.Merge(
        new PlayerCoinMergeCommand(
          command.CommandId,
          inventoryResult.Target.SlotIndex));
      if (!IsExpectedCoinNoOp(coinResult))
      {
        return new PlayerInventoryPickupResult(
          Applied: true,
          RemainingStack: inventoryResult.RemainingStack,
          Inventory: inventoryResult,
          CoinMergeAttempted: true,
          CoinMerge: coinResult,
          EffectsApplied: false,
          RejectionReason: PlayerInventoryPickupRejectionReason.EffectPortRejected);
      }
    }

    if (!TryApplyIntent(
      command,
      PlayerInventoryEffectIntentKind.Achievement,
      inventoryResult,
      amount: inventoryResult.AcceptedStack,
      enabled: true))
    {
      return new PlayerInventoryPickupResult(
        Applied: true,
        RemainingStack: inventoryResult.RemainingStack,
        Inventory: inventoryResult,
        CoinMergeAttempted: coinMergeAttempted,
        CoinMerge: coinResult,
        EffectsApplied: false,
        RejectionReason: PlayerInventoryPickupRejectionReason.EffectPortRejected);
    }

    if (!TryApplyIntent(
      command,
      PlayerInventoryEffectIntentKind.PostAction,
      inventoryResult,
      amount: inventoryResult.AcceptedStack,
      enabled: true))
    {
      return new PlayerInventoryPickupResult(
        Applied: true,
        RemainingStack: inventoryResult.RemainingStack,
        Inventory: inventoryResult,
        CoinMergeAttempted: coinMergeAttempted,
        CoinMerge: coinResult,
        EffectsApplied: false,
        RejectionReason: PlayerInventoryPickupRejectionReason.EffectPortRejected);
    }

    return new PlayerInventoryPickupResult(
      Applied: true,
      RemainingStack: inventoryResult.RemainingStack,
      Inventory: inventoryResult,
      CoinMergeAttempted: coinMergeAttempted,
      CoinMerge: coinResult,
      EffectsApplied: true,
      RejectionReason: PlayerInventoryPickupRejectionReason.None);
  }

  public void ResetForLifecycle()
  {
    _inventorySystem.ResetForLifecycle();
    _coinMergeSystem.ResetForLifecycle();
  }

  private bool TryApplyIntent(
    in PlayerInventoryPickupCommand command,
    PlayerInventoryEffectIntentKind kind,
    in PlayerInventoryCommitResult inventoryResult,
    int amount,
    bool enabled)
  {
    if (!enabled)
    {
      return true;
    }

    PlayerInventoryEffectIntent intent = new(
      kind,
      inventoryResult.Target,
      command.Item.Entity,
      amount,
      command.Item.IsCoin,
      command.Settings.LongText,
      command.Settings.MakeNewAndShiny);
    return _effectPort.TryApply(in intent);
  }

  private static bool IsExpectedCoinNoOp(
    PlayerCoinMergeResult coinResult)
  {
    return coinResult.Applied ||
      coinResult.RejectionReason ==
      PlayerCoinMergeRejectionReason.SourceIsNotUpgradeableCoin;
  }

  private static PlayerInventoryPickupResult EffectRejected(
    in PlayerInventoryPickupCommand command,
    in PlayerInventoryCommitResult inventoryResult)
  {
    return new PlayerInventoryPickupResult(
      Applied: true,
      RemainingStack: inventoryResult.RemainingStack,
      Inventory: inventoryResult,
      CoinMergeAttempted: false,
      CoinMerge: default,
      EffectsApplied: false,
      RejectionReason: PlayerInventoryPickupRejectionReason.EffectPortRejected);
  }
}
