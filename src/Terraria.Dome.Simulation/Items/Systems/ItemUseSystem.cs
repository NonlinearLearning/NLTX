using System;
using Terraria.Dome.Simulation.Items.Components;
using Terraria.Dome.Simulation.Items.Definitions;
using Terraria.Dome.Simulation.Items.Events;

namespace Terraria.Dome.Simulation.Items.Systems;

public readonly record struct ItemUseResult(
  bool IsAccepted,
  int Health,
  int Mana,
  int ConsumedQuantity,
  ItemCommandRejection Rejection,
  ItemUsedEvent Event,
  ushort BuffType = 0,
  int BuffDurationTicks = 0,
  ushort ProjectileType = 0,
  float ProjectileSpeed = 0);

public sealed class ItemUseSystem
{
  public ItemUseResult TryUse(
    PlayerHandle player,
    int slot,
    ItemStack stack,
    ItemDefinition definition,
    ref ItemUseStateComponent state,
    int health,
    int maximumHealth,
    int mana,
    int maximumMana,
    long sequence)
  {
    if (sequence < 0 || !state.CanUse || stack.IsEmpty || maximumHealth < 0 || maximumMana < 0 ||
        health < 0 || health > maximumHealth || mana < 0 || mana > maximumMana)
    {
      return Reject(health, mana, "Item use state or vitals are invalid.");
    }

    int healthRestore = definition.Use?.HealthRestore ?? 0;
    int manaRestore = definition.Use?.ManaRestore ?? 0;
    ushort buffType = 0;
    int buffDurationTicks = 0;
    if (definition.Recovery is ItemRecoveryDefinition recovery)
    {
      if (healthRestore == 0)
      {
        healthRestore = recovery.Health;
      }

      if (manaRestore == 0)
      {
        manaRestore = recovery.Mana;
      }

      buffType = recovery.BuffType;
      buffDurationTicks = recovery.BuffDurationTicks;
    }

    if ((buffType == 0) != (buffDurationTicks == 0))
    {
      return Reject(health, mana, "Buff recovery requires a type and duration together.");
    }

    if (healthRestore == 0 && manaRestore == 0 && definition.Use is null)
    {
      healthRestore = definition.HealthRestore;
    }

    int manaCost = definition.Use?.ManaCost ?? 0;
    bool consumable = definition.Use?.Consumable ??
      (definition.Recovery?.Consumable ?? healthRestore > 0);
    bool hasUseAction = definition.Use is ItemUseDefinition use && use.UseTime > 0;
    ushort projectileType = definition.Use is ItemUseDefinition useDefinition &&
      useDefinition.ShootType != 0
      ? useDefinition.ShootType
      : definition.Combat?.ProjectileType ?? 0;
    float projectileSpeed = definition.Use is ItemUseDefinition speedDefinition &&
      speedDefinition.ShootSpeed > 0
      ? speedDefinition.ShootSpeed
      : definition.Combat?.ProjectileSpeed ?? 0;
    bool hasAction = hasUseAction || projectileType != 0;
    bool hasRecoveryEffect = healthRestore > 0 || manaRestore > 0 || buffType != 0;
    if (!hasRecoveryEffect && !hasAction)
    {
      return Reject(health, mana, "The item has no accepted recovery effect.");
    }

    if (manaCost > mana)
    {
      return Reject(health, mana, "The player does not have enough mana for the item.");
    }

    int updatedHealth = Math.Min(maximumHealth, health + healthRestore);
    int manaAfterCost = mana - manaCost;
    int updatedMana = Math.Min(maximumMana, manaAfterCost + manaRestore);
    if (updatedHealth == health && updatedMana == mana && !hasAction && buffType == 0)
    {
      return Reject(health, mana, "The player is already at the relevant maximum.");
    }

    int cooldown = definition.Use?.CooldownTicks ?? definition.UseCooldownTicks;
    int animation = definition.Use?.UseAnimation ?? 0;
    state.CooldownTicks = cooldown;
    state.AnimationTicks = animation;
    state.IsChanneling = definition.Use?.Channel ?? false;
    ItemUsedEvent usedEvent = new(
      player,
      stack.ItemType,
      slot,
      updatedHealth - health,
      Math.Max(0, updatedMana - manaAfterCost),
      consumable,
      sequence,
      manaCost,
      buffType,
      buffDurationTicks);
    return new ItemUseResult(
      true,
      updatedHealth,
      updatedMana,
      consumable ? 1 : 0,
      default,
      usedEvent,
      buffType,
      buffDurationTicks,
      projectileType,
      projectileSpeed);
  }

  private static ItemUseResult Reject(int health, int mana, string reason)
  {
    return new ItemUseResult(
      false,
      health,
      mana,
      0,
      ItemCommandRejection.Invalid(reason),
      default);
  }
}
