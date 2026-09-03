using System;
using System.Collections.Generic;
using Terraria.Dome.Simulation.Items.Commands;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Events;
using Terraria.Dome.Simulation.Npc.Commands;
using Terraria.Dome.Simulation.Npc.Components;

namespace Terraria.Dome.Simulation.Items.Systems;

public readonly record struct ShopPurchaseResult(
  bool IsAccepted,
  string RejectionReason = "",
  ShopPurchaseReceipt? Receipt = null);

public sealed class ShopPurchaseState
{
  private readonly HashSet<int> _purchasedOffers = new();

  public bool IsPurchased(int offerId)
  {
    return _purchasedOffers.Contains(offerId);
  }

  internal void MarkPurchased(int offerId)
  {
    _purchasedOffers.Add(offerId);
  }
}

public sealed class ShopPurchaseSystem
{
  private const ushort CopperCoinType = 71;
  private const ushort SilverCoinType = 72;
  private const ushort GoldCoinType = 73;
  private const ushort PlatinumCoinType = 74;
  private readonly IReadOnlyDictionary<int, ShopOfferDefinition> _offers;
  private readonly CustomCurrencyDefinitionRegistry _currencies;

  public ShopPurchaseSystem(
    IReadOnlyDictionary<int, ShopOfferDefinition> offers,
    CustomCurrencyDefinitionRegistry? currencies = null)
  {
    ArgumentNullException.ThrowIfNull(offers);
    _offers = offers;
    _currencies = currencies ?? CustomCurrencyDefinitionRegistry.CreateDefault();
  }

  public ShopPurchaseResult TryPurchase(
    ShopPurchaseCommand command,
    NpcInteractionComponent interaction,
    ShopOfferDefinition offer,
    InventoryComponent inventory,
    ItemDefinitionRegistry definitions,
    ShopPurchaseState state)
  {
    ArgumentNullException.ThrowIfNull(inventory);
    ArgumentNullException.ThrowIfNull(definitions);
    ArgumentNullException.ThrowIfNull(state);
    try
    {
      offer.Validate();
    }
    catch (ArgumentOutOfRangeException)
    {
      return Reject("The shop offer is invalid.");
    }

    if (!_offers.TryGetValue(command.OfferId, out ShopOfferDefinition registeredOffer) ||
        registeredOffer != offer)
    {
      return Reject("The shop offer is not registered.");
    }

    if (!command.Player.IsValid || !command.Npc.IsValid || command.OfferId != offer.OfferId ||
        command.Sequence < 0 || string.IsNullOrWhiteSpace(command.SessionId) ||
        !interaction.HasActiveInteraction || interaction.Player != command.Player ||
        interaction.Npc != command.Npc ||
        interaction.Kind != NpcInteractionKind.Shop || interaction.SessionId != command.SessionId)
    {
      return Reject("The shop session is invalid.");
    }

    if (offer.BuyOnce && state.IsPurchased(offer.OfferId))
    {
      return Reject("The shop offer has already been purchased.");
    }

    if (!definitions.TryGet(offer.ItemType, out ItemDefinition item))
    {
      return Reject("The shop item is not registered.");
    }

    if (offer.Quantity > item.StackLimit)
    {
      return Reject("The inventory cannot accept the shop item.");
    }

    if (offer.UsesSpecialCurrency)
    {
      ShopPurchaseResult specialCurrencyResult = TryPurchaseWithSpecialCurrency(
        command,
        offer,
        inventory,
        definitions);
      if (specialCurrencyResult.IsAccepted && offer.BuyOnce)
      {
        state.MarkPurchased(offer.OfferId);
      }

      return specialCurrencyResult;
    }

    int effectivePriceCopper = offer.EffectivePriceCopper;
    List<int> coinSlots = new();
    List<int> emptySlots = new();
    long balance = 0;
    for (int slot = 0; slot < InventoryComponent.SlotCount; slot++)
    {
      ItemStack stack = inventory.GetSlot(slot);
      if (stack.IsEmpty)
      {
        emptySlots.Add(slot);
        continue;
      }

      long coinValue = GetCoinValue(stack.ItemType);
      if (coinValue == 0 || stack.Quantity <= 0)
      {
        continue;
      }

      int coinStackLimit = definitions.TryGet(stack.ItemType, out ItemDefinition coinDefinition)
        ? coinDefinition.StackLimit
        : ItemDefinition.CommonMaxStack;
      if (stack.Quantity > coinStackLimit)
      {
        return Reject("The inventory contains an over-limit currency stack.");
      }

      coinSlots.Add(slot);
      try
      {
        balance = checked(balance + coinValue * stack.Quantity);
      }
      catch (OverflowException)
      {
        return Reject("The ordinary currency balance is not representable.");
      }
    }

    if (balance < effectivePriceCopper)
    {
      return Reject("The player does not have enough ordinary currency.");
    }

    if (emptySlots.Count == 0)
    {
      return Reject("The inventory cannot accept the shop item.");
    }

    int currencySlotCapacity = coinSlots.Count + emptySlots.Count - 1;
    if (!TryBuildCurrencyChange(
          balance - effectivePriceCopper,
          definitions,
          currencySlotCapacity,
          out List<ItemStack> changeStacks))
    {
      return Reject("The inventory cannot represent the currency change.");
    }

    int itemSlot = emptySlots[0];
    List<int> currencySlots = new(currencySlotCapacity);
    currencySlots.AddRange(coinSlots);
    for (int index = 1; index < emptySlots.Count; index++)
    {
      currencySlots.Add(emptySlots[index]);
    }

    List<(int Slot, ItemStack Stack)> writes = new();
    for (int index = 0; index < currencySlots.Count; index++)
    {
      ItemStack replacement = index < changeStacks.Count
        ? changeStacks[index]
        : ItemStack.Empty;
      int slot = currencySlots[index];
      if (inventory.GetSlot(slot) != replacement)
      {
        writes.Add((slot, replacement));
      }
    }

    writes.Add((itemSlot, new ItemStack(offer.ItemType, offer.Quantity)));
    if (inventory.Revision < 0 || inventory.Revision > long.MaxValue - writes.Count)
    {
      return Reject("The inventory revision cannot advance for this purchase.");
    }

    foreach ((int slot, ItemStack stack) in writes)
    {
      inventory.SetSlot(slot, stack);
    }

    if (offer.BuyOnce)
    {
      state.MarkPurchased(offer.OfferId);
    }

    return new ShopPurchaseResult(true, Receipt: ShopPurchaseReceipt.Create(command, offer));
  }

  private ShopPurchaseResult TryPurchaseWithSpecialCurrency(
    ShopPurchaseCommand command,
    ShopOfferDefinition offer,
    InventoryComponent inventory,
    ItemDefinitionRegistry definitions)
  {
    if (!_currencies.TryGet(offer.SpecialCurrencyId, out CustomCurrencyDefinition currency))
    {
      return Reject("The shop currency is not registered.");
    }

    if (!definitions.TryGet(currency.ItemType, out ItemDefinition currencyItem))
    {
      return Reject("The shop currency item is not registered.");
    }

    List<int> currencySlots = new();
    List<int> emptySlots = new();
    long balance = 0;
    for (int slot = 0; slot < InventoryComponent.SlotCount; slot++)
    {
      ItemStack stack = inventory.GetSlot(slot);
      if (stack.IsEmpty)
      {
        emptySlots.Add(slot);
        continue;
      }

      if (stack.ItemType != currency.ItemType)
      {
        continue;
      }

      if (stack.Quantity > currencyItem.StackLimit)
      {
        return Reject("The inventory contains an over-limit special currency stack.");
      }

      currencySlots.Add(slot);
      long remainingCapacity = currency.CurrencyCap - balance;
      if (remainingCapacity > 0)
      {
        balance += Math.Min((long)stack.Quantity, remainingCapacity);
      }
    }

    int effectivePrice = offer.EffectivePrice;
    if (balance < effectivePrice)
    {
      return Reject("The player does not have enough special currency.");
    }

    long remainingPrice = effectivePrice;
    List<(int Slot, ItemStack Replacement)> paymentChanges = new();
    for (int index = 0; index < currencySlots.Count && remainingPrice > 0; index++)
    {
      int slot = currencySlots[index];
      ItemStack stack = inventory.GetSlot(slot);
      int consumed = (int)Math.Min((long)stack.Quantity, remainingPrice);
      remainingPrice -= consumed;
      ItemStack replacement = stack.WithQuantity(stack.Quantity - consumed);
      if (replacement != stack)
      {
        paymentChanges.Add((slot, replacement));
      }
    }

    if (remainingPrice != 0)
    {
      return Reject("The special currency balance cannot satisfy the offer price.");
    }

    int itemSlot = emptySlots.Count > 0
      ? emptySlots[0]
      : FindFreedCurrencySlot(paymentChanges);
    if (itemSlot < 0)
    {
      return Reject("The inventory cannot accept the shop item.");
    }

    List<(int Slot, ItemStack Stack)> writes = new();
    foreach ((int slot, ItemStack replacement) in paymentChanges)
    {
      if (slot == itemSlot && replacement.IsEmpty)
      {
        continue;
      }

      writes.Add((slot, replacement));
    }

    ItemStack purchasedItem = new(offer.ItemType, offer.Quantity);
    if (inventory.GetInstanceState(itemSlot) != default)
    {
      writes.Add((itemSlot, ItemStack.Empty));
    }

    writes.Add((itemSlot, purchasedItem));
    if (inventory.Revision < 0 || inventory.Revision > long.MaxValue - writes.Count)
    {
      return Reject("The inventory revision cannot advance for this purchase.");
    }

    foreach ((int slot, ItemStack stack) in writes)
    {
      inventory.SetSlot(slot, stack);
    }

    return new ShopPurchaseResult(true, Receipt: ShopPurchaseReceipt.Create(command, offer));
  }

  private static int FindFreedCurrencySlot(
    IReadOnlyList<(int Slot, ItemStack Replacement)> paymentChanges)
  {
    for (int index = 0; index < paymentChanges.Count; index++)
    {
      if (paymentChanges[index].Replacement.IsEmpty)
      {
        return paymentChanges[index].Slot;
      }
    }

    return -1;
  }

  private static bool TryBuildCurrencyChange(
    long change,
    ItemDefinitionRegistry definitions,
    int maxStackCount,
    out List<ItemStack> changeStacks)
  {
    changeStacks = new();
    if (change < 0 || maxStackCount < 0)
    {
      return false;
    }

    foreach (ushort coinType in new[]
    {
      PlatinumCoinType,
      GoldCoinType,
      SilverCoinType,
      CopperCoinType
    })
    {
      long coinValue = GetCoinValue(coinType);
      int stackLimit = definitions.TryGet(coinType, out ItemDefinition coinDefinition)
        ? coinDefinition.StackLimit
        : ItemDefinition.CommonMaxStack;
      if (stackLimit <= 0)
      {
        changeStacks.Clear();
        return false;
      }

      while (change >= coinValue)
      {
        if (changeStacks.Count >= maxStackCount)
        {
          changeStacks.Clear();
          return false;
        }

        long quantity = Math.Min(change / coinValue, stackLimit);
        changeStacks.Add(new ItemStack(coinType, (int)quantity));
        change -= quantity * coinValue;
      }
    }

    return change == 0;
  }

  private static long GetCoinValue(ushort itemType)
  {
    return itemType switch
    {
      CopperCoinType => 1,
      SilverCoinType => 100,
      GoldCoinType => 10000,
      PlatinumCoinType => 1000000,
      _ => 0
    };
  }

  private static ShopPurchaseResult Reject(string reason)
  {
    return new ShopPurchaseResult(false, reason);
  }
}
