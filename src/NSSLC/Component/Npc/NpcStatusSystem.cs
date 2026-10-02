using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Combat;

namespace Terraria.Npc;

public sealed class NpcStatusSystem
{
  private const int LifeRegenerationThreshold = 120;
  private const int ProjectileSlotCount = 1000;
  private const int JavelinProjectileType = 598;
  private const int TentacleSpikeProjectileType = 971;
  private const int BloodButcheredProjectileType = 975;
  private const int DaybreakProjectileType = 636;
  private const int CelledProjectileType = 614;
  private const int PoisonBuffType = 20;
  private const int TipsyBuffType = 25;
  private const int BleedingBuffType = 30;
  private const int HemorrhageBuffType = 375;
  private const int VenomBuffType = 70;
  private const int OnFireBuffType = 24;
  private const int MidasBuffType = 72;
  private const int IchorBuffType = 69;
  private const int BrokenArmorBuffType = 36;
  private const int ConfusedBuffType = 31;
  private const int OnFire2BuffType = 39;
  private const int OnFrostBurnBuffType = 44;
  private const int DrippingBuffType = 103;
  private const int DrippingSlimeBuffType = 137;
  private const int DrippingSparkleSlimeBuffType = 320;
  private const int LoveStruckBuffType = 119;
  private const int StinkyBuffType = 120;
  private const int SoulDrainBuffType = 151;
  private const int ShadowFlameBuffType = 153;
  private const int DryadWardBuffType = 165;
  private const int JavelinedBuffType = 169;
  private const int TentacleSpikedBuffType = 337;
  private const int BloodButcheredBuffType = 344;
  private const int CelledBuffType = 183;
  private const int DryadBaneBuffType = 186;
  private const int DaybreakBuffType = 189;
  private const int BetsysCurseBuffType = 203;
  private const int OiledBuffType = 204;
  private const int ScytheWhipBuffType = 310;
  private const int EelWhipBuffType = 362;
  private const int OnFire3BuffType = 323;
  private const int OnFrostBurn2BuffType = 324;
  private const int ShimmeringBuffType = 353;

  public NpcBuffStateUpdateResult ApplyBuffSlotPhase(
    NpcTypeId npcType,
    in NpcAiStateComponent aiState,
    in StatusEffectImmunityComponent immunity,
    ref StatusEffectSlotsComponent slots,
    ref NpcStatusFlagsComponent status,
    bool lowerBuffTime = true)
  {
    StatusEffectSlot[] originalSlots = slots.Slots.ToArray();
    StatusEffectSlot[] nextSlots = originalSlots.ToArray();
    ReadOnlySpan<bool> immuneByDefinition = immunity.ImmunityByDefinitionId.Span;
    if (HasActiveBuff(nextSlots, ShimmeringBuffType) &&
        immuneByDefinition.Length <= ShimmeringBuffType)
    {
      return new NpcBuffStateUpdateResult(
        applied: false,
        requiredInputMissing: true,
        buffSlotsChanged: false,
        Array.Empty<NpcBuffStateUpdateResult.EffectIntent>());
    }

    NpcStatusFlagsComponent nextStatus = default;
    nextStatus.LifeRegenerationExpectedLossPerSecond = -1;
    var effects = new List<NpcBuffStateUpdateResult.EffectIntent>();

    for (int index = 0; index < nextSlots.Length; index++)
    {
      StatusEffectSlot slot = nextSlots[index];
      if (slot.DefinitionId <= 0 || slot.RemainingTicks <= 0)
      {
        continue;
      }

      if (lowerBuffTime)
      {
        slot.RemainingTicks--;
      }

      bool removedCurrentSlot = false;
      switch (slot.DefinitionId)
      {
        case PoisonBuffType:
          nextStatus.Poisoned = true;
          break;
        case TipsyBuffType:
          nextStatus.Tipsy = true;
          break;
        case BleedingBuffType:
          nextStatus.Bleeding = true;
          break;
        case HemorrhageBuffType:
          nextStatus.Hemorrhage = true;
          break;
        case VenomBuffType:
          nextStatus.Venom = true;
          break;
        case OnFireBuffType:
          if (npcType.Value == 1 && aiState.State1 == 9.0f)
          {
            slot.RemainingTicks = 60;
          }

          nextStatus.OnFire = true;
          break;
        case MidasBuffType:
          nextStatus.Midas = true;
          break;
        case IchorBuffType:
          nextStatus.Ichor = true;
          break;
        case BrokenArmorBuffType:
          nextStatus.BrokenArmor = true;
          break;
        case ConfusedBuffType:
          nextStatus.Confused = true;
          break;
        case OnFire2BuffType:
          nextStatus.OnFire2 = true;
          break;
        case OnFrostBurnBuffType:
          if (npcType.Value == 1 && aiState.State1 == 9.0f)
          {
            slot.RemainingTicks = 60;
          }

          nextStatus.OnFrostBurn = true;
          break;
        case DrippingBuffType:
          nextStatus.Dripping = true;
          break;
        case DrippingSlimeBuffType:
          nextStatus.DrippingSlime = true;
          break;
        case DrippingSparkleSlimeBuffType:
          nextStatus.DrippingSparkleSlime = true;
          break;
        case LoveStruckBuffType:
          nextStatus.LoveStruck = true;
          break;
        case StinkyBuffType:
          nextStatus.Stinky = true;
          break;
        case SoulDrainBuffType:
          nextStatus.SoulDrain = true;
          break;
        case ShadowFlameBuffType:
          nextStatus.ShadowFlame = true;
          break;
        case DryadWardBuffType:
          nextStatus.DryadWard = true;
          break;
        case JavelinedBuffType:
          nextStatus.Javelined = true;
          break;
        case TentacleSpikedBuffType:
          nextStatus.TentacleSpiked = true;
          break;
        case BloodButcheredBuffType:
          nextStatus.BloodButchered = true;
          break;
        case CelledBuffType:
          nextStatus.Celled = true;
          break;
        case DryadBaneBuffType:
          nextStatus.DryadBane = true;
          break;
        case DaybreakBuffType:
          nextStatus.Daybreak = true;
          break;
        case BetsysCurseBuffType:
          nextStatus.BetsysCurse = true;
          break;
        case OiledBuffType:
          nextStatus.Oiled = true;
          break;
        case ScytheWhipBuffType:
          nextStatus.MarkedByScytheWhip = true;
          break;
        case EelWhipBuffType:
          nextStatus.MarkedByEelWhip = true;
          break;
        case OnFire3BuffType:
          nextStatus.OnFire3 = true;
          break;
        case OnFrostBurn2BuffType:
          nextStatus.OnFrostBurn2 = true;
          break;
        case ShimmeringBuffType:
          if (immuneByDefinition[ShimmeringBuffType])
          {
            RemoveBuffAt(nextSlots, index);
            removedCurrentSlot = true;
            effects.Add(new NpcBuffStateUpdateResult.EffectIntent(
              NpcBuffStateUpdateResult.EffectKind.BuffSlotSyncRequested));
          }
          else
          {
            nextStatus.Shimmering = true;
          }

          break;
      }

      if (!removedCurrentSlot)
      {
        nextSlots[index] = slot;
      }
    }

    if (nextStatus.Dripping)
    {
      effects.Add(new NpcBuffStateUpdateResult.EffectIntent(
        NpcBuffStateUpdateResult.EffectKind.WaterPerishableCleanupRequested));
    }

    bool buffSlotsChanged = !SlotsEqual(originalSlots, nextSlots);
    if (buffSlotsChanged)
    {
      int nextRevision = checked(slots.Revision + 1);
      slots = new StatusEffectSlotsComponent(nextSlots, nextRevision);
    }

    status = nextStatus;
    return new NpcBuffStateUpdateResult(
      applied: true,
      requiredInputMissing: false,
      buffSlotsChanged,
      effects.ToArray());
  }

  public NpcLifeRegenerationResult ApplyDamageOverTimePhase(
    ref NpcStatusFlagsComponent status,
    ref NpcLifeRegenerationStateComponent regenerationState,
    in NpcLifeRegenerationInput input)
  {
    int countBefore = regenerationState.LifeRegenerationCount;
    if (input.DamageProtected)
    {
      return RejectedLifeRegeneration(
        NpcLifeRegenerationFailureReason.DamageProtected,
        status,
        countBefore);
    }

    if (!input.NpcType.IsValid ||
        input.MaximumLife < 0 ||
        input.CurrentLife < 0 ||
        input.CurrentLife > input.MaximumLife)
    {
      return RejectedLifeRegeneration(
        NpcLifeRegenerationFailureReason.InvalidInput,
        status,
        countBefore);
    }

    bool needsProjectileSnapshot = status.Javelined ||
      status.TentacleSpiked ||
      status.BloodButchered ||
      status.Daybreak ||
      status.Celled;
    IReadOnlyList<NpcLifeRegenerationProjectileSnapshot> projectiles =
      input.ProjectileSnapshot ?? Array.Empty<NpcLifeRegenerationProjectileSnapshot>();
    if (needsProjectileSnapshot &&
        (!input.ProjectileSnapshotComplete || input.ProjectileSnapshot is null))
    {
      return RejectedLifeRegeneration(
        NpcLifeRegenerationFailureReason.ProjectileSnapshotRequired,
        status,
        countBefore);
    }

    if (needsProjectileSnapshot &&
        (input.NpcLegacySlot < 0 || projectiles.Count != ProjectileSlotCount))
    {
      return RejectedLifeRegeneration(
        NpcLifeRegenerationFailureReason.InvalidInput,
        status,
        countBefore);
    }

    if (status.DryadBane && input.TownNpcDamageMultiplier is null)
    {
      return RejectedLifeRegeneration(
        NpcLifeRegenerationFailureReason.TownNpcDamageMultiplierRequired,
        status,
        countBefore);
    }

    if (status.DryadBane &&
        (!float.IsFinite(input.TownNpcDamageMultiplier!.Value) ||
          input.TownNpcDamageMultiplier.Value < 0.0f))
    {
      return RejectedLifeRegeneration(
        NpcLifeRegenerationFailureReason.InvalidTownNpcDamageMultiplier,
        status,
        countBefore);
    }

    long lifeRegeneration = status.LifeRegenerationRate;
    int expectedDamagePerSecond = status.LifeRegenerationExpectedLossPerSecond;

    if (status.Poisoned)
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 12);
    }

    if (status.Bleeding)
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 24);
      SetMinimum(ref expectedDamagePerSecond, 4);
    }

    if (status.Hemorrhage)
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 200);
      SetMinimum(ref expectedDamagePerSecond, 40);
    }

    bool fireSuppressed = input.NpcType.Value == 1 &&
      input.AiState1 == 8.0f &&
      input.GoodWorld;
    if (status.OnFire && !fireSuppressed)
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 8);
      if (status.DrippingSlime)
      {
        lifeRegeneration -= 8;
      }

      if (input.NpcType.Value == 1 && input.AiState1 == 9.0f)
      {
        lifeRegeneration -= 16;
      }
    }

    if (status.OnFire3)
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 30);
      if (status.DrippingSlime)
      {
        lifeRegeneration -= 30;
      }

      SetMinimum(ref expectedDamagePerSecond, 5);
    }

    if (status.OnFrostBurn)
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 16);
      if (status.DrippingSlime)
      {
        lifeRegeneration -= 16;
      }

      SetMinimum(ref expectedDamagePerSecond, 2);
      if (input.NpcType.Value == 1 && input.AiState1 == 9.0f)
      {
        lifeRegeneration -= 16;
      }
    }

    if (status.OnFrostBurn2)
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 50);
      if (status.DrippingSlime)
      {
        lifeRegeneration -= 50;
      }

      SetMinimum(ref expectedDamagePerSecond, 10);
    }

    if (status.OnFire2)
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 48);
      if (status.DrippingSlime)
      {
        lifeRegeneration -= 48;
      }

      SetMinimum(ref expectedDamagePerSecond, 10);
    }

    if (status.Venom)
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 60);
      SetMinimum(ref expectedDamagePerSecond, 15);
    }

    if (status.ShadowFlame)
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 30);
      if (status.DrippingSlime)
      {
        lifeRegeneration -= 30;
      }

      SetMinimum(ref expectedDamagePerSecond, 5);
    }

    if (status.Oiled &&
        (status.OnFire || status.OnFire2 || status.OnFire3 ||
          status.OnFrostBurn || status.OnFrostBurn2 || status.ShadowFlame))
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 50);
      SetMinimum(ref expectedDamagePerSecond, 10);
    }

    if (status.Javelined)
    {
      int projectileCount = CountMatchingProjectiles(
        projectiles,
        JavelinProjectileType,
        input.NpcLegacySlot);
      ApplyRegenerationPenalty(ref lifeRegeneration, projectileCount * 6);
      SetMinimum(ref expectedDamagePerSecond, projectileCount * 3);
    }

    if (status.TentacleSpiked)
    {
      int projectileCount = CountMatchingProjectiles(
        projectiles,
        TentacleSpikeProjectileType,
        input.NpcLegacySlot);
      ApplyRegenerationPenalty(ref lifeRegeneration, projectileCount * 6);
      SetMinimum(ref expectedDamagePerSecond, projectileCount * 3);
    }

    if (status.BloodButchered)
    {
      int projectileCount = CountMatchingProjectiles(
        projectiles,
        BloodButcheredProjectileType,
        input.NpcLegacySlot);
      ApplyRegenerationPenalty(ref lifeRegeneration, projectileCount * 8);
      SetMinimum(ref expectedDamagePerSecond, projectileCount * 4);
    }

    if (status.Daybreak)
    {
      int projectileCount = CountMatchingProjectiles(
        projectiles,
        DaybreakProjectileType,
        input.NpcLegacySlot);
      projectileCount = Math.Max(projectileCount, 1);
      ApplyRegenerationPenalty(ref lifeRegeneration, projectileCount * 200);
      SetMinimum(ref expectedDamagePerSecond, projectileCount * 25);
    }

    if (status.Celled)
    {
      int projectileCount = CountMatchingProjectiles(
        projectiles,
        CelledProjectileType,
        input.NpcLegacySlot);
      ApplyRegenerationPenalty(ref lifeRegeneration, projectileCount * 40);
      int celledDamageThreshold = projectileCount * 20;
      if (expectedDamagePerSecond < celledDamageThreshold)
      {
        expectedDamagePerSecond = celledDamageThreshold / 2;
      }
    }

    if (status.DryadBane)
    {
      float progressionScale = CalculateDryadBaneProgressionScale(
        input.DryadBaneProgress);
      float baseDamage = input.InfectedSeed ? 8.0f : 4.0f;
      float scaledDamage = baseDamage * progressionScale *
        input.TownNpcDamageMultiplier!.Value;
      if (!float.IsFinite(scaledDamage) || (double)scaledDamage > int.MaxValue)
      {
        return RejectedLifeRegeneration(
          NpcLifeRegenerationFailureReason.ArithmeticOverflow,
          status,
          countBefore);
      }

      int townNpcDamage = (int)scaledDamage;
      ApplyRegenerationPenalty(ref lifeRegeneration, (long)townNpcDamage * 2);
      if (expectedDamagePerSecond < townNpcDamage)
      {
        expectedDamagePerSecond = townNpcDamage / 3;
      }
    }

    if (status.SoulDrain && !input.HasRealLifeParent)
    {
      ApplyRegenerationPenalty(ref lifeRegeneration, 50);
      SetMinimum(ref expectedDamagePerSecond, 5);
    }

    if (input.NpcType.Value == 59 && input.AiState1 == 174.0f && input.LavaWet)
    {
      lifeRegeneration += 32;
    }

    if (input.NpcType.Value == 1)
    {
      if (input.AiState1 == 29.0f)
      {
        lifeRegeneration += 16;
      }
      else if (input.AiState1 == 364.0f ||
          input.AiState1 == 1104.0f ||
          input.AiState1 == 365.0f ||
          input.AiState1 == 1105.0f ||
          input.AiState1 == 366.0f ||
          input.AiState1 == 1106.0f)
      {
        lifeRegeneration += 24;
      }
    }

    if (lifeRegeneration <= -240 && expectedDamagePerSecond < 2)
    {
      expectedDamagePerSecond = 2;
    }

    long lifeRegenerationCount =
      (long)regenerationState.LifeRegenerationCount + lifeRegeneration;
    long healingTicks = 0;
    if (lifeRegenerationCount >= LifeRegenerationThreshold)
    {
      healingTicks = lifeRegenerationCount / LifeRegenerationThreshold;
      lifeRegenerationCount %= LifeRegenerationThreshold;
    }

    long damageThreshold = expectedDamagePerSecond > 0
      ? (long)LifeRegenerationThreshold * expectedDamagePerSecond
      : LifeRegenerationThreshold;
    int damagePerEvent = expectedDamagePerSecond > 0
      ? expectedDamagePerSecond
      : 1;
    long damageEventCount = 0;
    if (lifeRegenerationCount <= -damageThreshold)
    {
      damageEventCount =
        (-damageThreshold - lifeRegenerationCount) / damageThreshold + 1;
      lifeRegenerationCount += damageEventCount * damageThreshold;
    }

    if (lifeRegeneration < int.MinValue ||
        lifeRegeneration > int.MaxValue ||
        lifeRegenerationCount < int.MinValue ||
        lifeRegenerationCount > int.MaxValue ||
        damageEventCount > int.MaxValue ||
        (regenerationState.LifeRegenerationCount != (int)lifeRegenerationCount &&
          regenerationState.Revision == int.MaxValue))
    {
      return RejectedLifeRegeneration(
        NpcLifeRegenerationFailureReason.ArithmeticOverflow,
        status,
        countBefore);
    }

    long missingLife = input.MaximumLife - input.CurrentLife;
    int healingPoints = input.Immortal
      ? 0
      : (int)Math.Min(healingTicks, missingLife);
    int committedLifeRegenerationCount = (int)lifeRegenerationCount;
    if (regenerationState.LifeRegenerationCount != committedLifeRegenerationCount)
    {
      regenerationState.LifeRegenerationCount = committedLifeRegenerationCount;
      regenerationState.Revision++;
    }

    status.LifeRegenerationRate = (int)lifeRegeneration;
    return new NpcLifeRegenerationResult(
      Computed: true,
      FailureReason: NpcLifeRegenerationFailureReason.None,
      LifeRegenerationAfter: (int)lifeRegeneration,
      LifeRegenerationCountBefore: countBefore,
      LifeRegenerationCountAfter: committedLifeRegenerationCount,
      HealingPoints: healingPoints,
      EffectiveDamagePerSecond: expectedDamagePerSecond,
      DamagePerEvent: damageEventCount == 0 ? 0 : damagePerEvent,
      DamageEventCount: (int)damageEventCount,
      EelWhipDotBehaviorUnknown: status.MarkedByEelWhip);
  }

  public NpcBuffStateUpdateResult ClearExpiredBuffs(
    ref StatusEffectSlotsComponent slots)
  {
    StatusEffectSlot[] originalSlots = slots.Slots.ToArray();
    StatusEffectSlot[] nextSlots = originalSlots.ToArray();
    bool expiredBuffRemoved = false;
    for (int index = 0; index < nextSlots.Length; index++)
    {
      if (nextSlots[index].DefinitionId > 0 &&
          nextSlots[index].RemainingTicks <= 0)
      {
        RemoveBuffAt(nextSlots, index);
        expiredBuffRemoved = true;
      }
    }

    bool buffSlotsChanged = !SlotsEqual(originalSlots, nextSlots);
    if (buffSlotsChanged)
    {
      int nextRevision = checked(slots.Revision + 1);
      slots = new StatusEffectSlotsComponent(nextSlots, nextRevision);
    }

    NpcBuffStateUpdateResult.EffectIntent[] effects = expiredBuffRemoved
      ? new[]
      {
        new NpcBuffStateUpdateResult.EffectIntent(
          NpcBuffStateUpdateResult.EffectKind.BuffSlotSyncRequested),
      }
      : Array.Empty<NpcBuffStateUpdateResult.EffectIntent>();
    return new NpcBuffStateUpdateResult(
      applied: true,
      requiredInputMissing: false,
      buffSlotsChanged,
      effects);
  }

  private static bool HasActiveBuff(
    IReadOnlyList<StatusEffectSlot> slots,
    int definitionId)
  {
    for (int index = 0; index < slots.Count; index++)
    {
      if (slots[index].DefinitionId == definitionId &&
          slots[index].RemainingTicks > 0)
      {
        return true;
      }
    }

    return false;
  }

  private static NpcLifeRegenerationResult RejectedLifeRegeneration(
    NpcLifeRegenerationFailureReason reason,
    in NpcStatusFlagsComponent status,
    int countBefore)
  {
    return new NpcLifeRegenerationResult(
      Computed: false,
      FailureReason: reason,
      LifeRegenerationAfter: status.LifeRegenerationRate,
      LifeRegenerationCountBefore: countBefore,
      LifeRegenerationCountAfter: countBefore,
      HealingPoints: 0,
      EffectiveDamagePerSecond: status.LifeRegenerationExpectedLossPerSecond,
      DamagePerEvent: 0,
      DamageEventCount: 0,
      EelWhipDotBehaviorUnknown: false);
  }

  private static void ApplyRegenerationPenalty(ref long lifeRegeneration, long amount)
  {
    if (lifeRegeneration > 0)
    {
      lifeRegeneration = 0;
    }

    lifeRegeneration -= amount;
  }

  private static void SetMinimum(ref int value, int candidate)
  {
    if (value < candidate)
    {
      value = candidate;
    }
  }

  private static int CountMatchingProjectiles(
    IReadOnlyList<NpcLifeRegenerationProjectileSnapshot> projectiles,
    int projectileType,
    int npcLegacySlot)
  {
    int count = 0;
    float legacySlot = npcLegacySlot;
    for (int index = 0; index < projectiles.Count; index++)
    {
      NpcLifeRegenerationProjectileSnapshot projectile = projectiles[index];
      if (projectile.IsActive &&
          projectile.ProjectileType == projectileType &&
          projectile.Ai0 == 1.0f &&
          projectile.Ai1 == legacySlot)
      {
        count++;
      }
    }

    return count;
  }

  private static float CalculateDryadBaneProgressionScale(
    in NpcDryadBaneProgressSnapshot progression)
  {
    float scale = 1.0f;
    if (progression.DownedBoss1)
    {
      scale += 0.1f;
    }

    if (progression.DownedBoss2)
    {
      scale += 0.1f;
    }

    if (progression.DownedBoss3)
    {
      scale += 0.1f;
    }

    if (progression.DownedQueenBee)
    {
      scale += 0.1f;
    }

    if (progression.HardMode)
    {
      scale += 0.4f;
    }

    if (progression.DownedQueenSlime)
    {
      scale += 0.15f;
    }

    if (progression.DownedMechBoss1)
    {
      scale += 0.15f;
    }

    if (progression.DownedMechBoss2)
    {
      scale += 0.15f;
    }

    if (progression.DownedMechBoss3)
    {
      scale += 0.15f;
    }

    if (progression.DownedPlantBoss)
    {
      scale += 0.15f;
    }

    if (progression.DownedGolemBoss)
    {
      scale += 0.15f;
    }

    if (progression.DownedAncientCultist)
    {
      scale += 0.15f;
    }

    if (progression.DownedEmpressOfLight)
    {
      scale += 0.15f;
    }

    if (progression.DownedFishron)
    {
      scale += 0.15f;
    }

    return scale;
  }

  private static bool SlotsEqual(
    IReadOnlyList<StatusEffectSlot> left,
    IReadOnlyList<StatusEffectSlot> right)
  {
    if (left.Count != right.Count)
    {
      return false;
    }

    for (int index = 0; index < left.Count; index++)
    {
      if (left[index].DefinitionId != right[index].DefinitionId ||
          left[index].RemainingTicks != right[index].RemainingTicks)
      {
        return false;
      }
    }

    return true;
  }

  private static void RemoveBuffAt(StatusEffectSlot[] slots, int index)
  {
    slots[index] = default;
    for (int slotIndex = 0; slotIndex < slots.Length - 1; slotIndex++)
    {
      if (slots[slotIndex].RemainingTicks != 0 &&
          slots[slotIndex].DefinitionId != 0)
      {
        continue;
      }

      for (int nextIndex = slotIndex + 1; nextIndex < slots.Length; nextIndex++)
      {
        slots[nextIndex - 1] = slots[nextIndex];
        slots[nextIndex] = default;
      }
    }
  }
}
