using Terraria.Combat.TagEffects;
using Terraria.EntityLifecycleAttribution;

namespace Terraria.Transport.TagEffects;

public static class TagEffectStateProjection
{
  public readonly record struct ProjectedMark(
    EntityReference TargetReference,
    int TagTicksRemaining,
    int ProcTicksRemaining);

  public readonly record struct Snapshot(
    EntityReference OwnerReference,
    int ActiveEffectTypeId,
    IReadOnlyList<ProjectedMark> Marks);

  public static Snapshot CreateSnapshot(PlayerTagEffectStateComponent state)
  {
    ArgumentNullException.ThrowIfNull(state);
    var marks = new List<ProjectedMark>();
    foreach (PlayerTagEffectStateComponent.NpcTagMark mark in state.Marks)
    {
      marks.Add(new ProjectedMark(
        mark.TargetReference,
        mark.TagTicksRemaining,
        mark.ProcTicksRemaining));
    }

    return new Snapshot(
      state.OwnerReference,
      state.ActiveEffectTypeId,
      Array.AsReadOnly(marks.ToArray()));
  }
}
