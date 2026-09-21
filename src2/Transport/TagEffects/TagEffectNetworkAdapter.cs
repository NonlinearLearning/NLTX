using Terraria.Combat.TagEffects;
using Terraria.EntityLifecycleAttribution;

namespace Terraria.Transport.TagEffects;

public static class TagEffectNetworkAdapter
{
  public readonly record struct NetworkMark(
    int NetworkTargetId,
    int TagTicksRemaining,
    int ProcTicksRemaining);

  public readonly record struct NetworkPayload(
    bool ShouldSync,
    int NetworkPlayerId,
    int EffectTypeId,
    IReadOnlyList<NetworkMark> Marks);

  public static NetworkPayload CreatePayload(
    PlayerTagEffectStateComponent state,
    int networkPlayerId,
    Func<EntityReference, int> targetNetworkIdResolver)
  {
    ArgumentNullException.ThrowIfNull(state);
    ArgumentNullException.ThrowIfNull(targetNetworkIdResolver);

    if (state.ActiveEffectDefinition is not
      Terraria.Content.StatusEffects.TagEffectDefinition definition ||
      !definition.NetworkSyncPolicy)
    {
      return new NetworkPayload(
        false,
        networkPlayerId,
        -1,
        Array.AsReadOnly(Array.Empty<NetworkMark>()));
    }

    var marks = new List<NetworkMark>();
    foreach (PlayerTagEffectStateComponent.NpcTagMark mark in state.Marks)
    {
      if (mark.TagTicksRemaining <= 0)
      {
        continue;
      }

      int networkTargetId = targetNetworkIdResolver(mark.TargetReference);
      if (networkTargetId < 0)
      {
        continue;
      }

      marks.Add(new NetworkMark(
        networkTargetId,
        mark.TagTicksRemaining,
        definition.SyncProcTimers ? mark.ProcTicksRemaining : 0));
    }

    return new NetworkPayload(
      true,
      networkPlayerId,
      definition.EffectTypeId,
      Array.AsReadOnly(marks.ToArray()));
  }
}
