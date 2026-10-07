namespace Terraria.Npc;

/// <summary>
/// Per-instance state owned by the Guide source profile. Task lifecycle state is
/// deliberately excluded; NpcTaskLifecycleSystem remains the task owner.
/// </summary>
public readonly record struct NpcGuideSourceProfileState(
  float Ai0,
  float Ai1,
  float Ai2,
  float LocalAi3,
  float LocalAi2 = 0f);
