using System.Collections.Generic;

namespace Terraria.Npc;

public readonly record struct NpcLifeRegenerationInput(
  NpcTypeId NpcType,
  float AiState1,
  int NpcLegacySlot,
  int CurrentLife,
  int MaximumLife,
  bool DamageProtected,
  bool Immortal,
  bool GoodWorld,
  bool LavaWet,
  bool HasRealLifeParent,
  bool InfectedSeed,
  NpcDryadBaneProgressSnapshot DryadBaneProgress,
  float? TownNpcDamageMultiplier,
  IReadOnlyList<NpcLifeRegenerationProjectileSnapshot>? ProjectileSnapshot,
  bool ProjectileSnapshotComplete);
