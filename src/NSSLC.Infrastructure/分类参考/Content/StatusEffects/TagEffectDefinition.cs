namespace Terraria.Content.StatusEffects;

public class TagEffectDefinition
{
  public TagEffectDefinition(
    int effectTypeId,
    bool networkSyncPolicy,
    bool syncProcTimers,
    int tagDurationTicks,
    int defaultWhipMarkDurationTicks = 240)
  {
    if (effectTypeId < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(effectTypeId));
    }
    if (tagDurationTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(tagDurationTicks));
    }
    if (defaultWhipMarkDurationTicks < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(defaultWhipMarkDurationTicks));
    }

    EffectTypeId = effectTypeId;
    NetworkSyncPolicy = networkSyncPolicy;
    SyncProcTimers = syncProcTimers;
    TagDurationTicks = tagDurationTicks;
    DefaultWhipMarkDurationTicks = defaultWhipMarkDurationTicks;
  }

  public int DefaultWhipMarkDurationTicks { get; }

  public int EffectTypeId { get; }

  public bool NetworkSyncPolicy { get; }

  public bool SyncProcTimers { get; }

  public int TagDurationTicks { get; }
}
