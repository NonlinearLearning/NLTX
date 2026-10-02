using System.Collections.Generic;
using System.Numerics;
using Terraria.Combat;

namespace Terraria.Npc;

public readonly record struct NpcStatusTickInput(
  NpcAiStateComponent AiState,
  StatusEffectImmunityComponent Immunity,
  Vector2 NpcCenter,
  Vector2 NpcVelocity,
  IReadOnlyList<NpcSoulDrainPlayerSnapshot>? SoulDrainPlayerSnapshot,
  NpcLifeRegenerationInput LifeRegeneration,
  NpcDamageOverTimeRequest DamageOverTime,
  bool LowerBuffTime = true,
  bool BloodMoonActive = false,
  bool CrimsonWorld = false,
  float NpcValue = 0.0f,
  NpcGravityEnvironmentSnapshot? GravityEnvironment = null,
  bool CanDisplayBuffs = true,
  bool JustHit = false);
