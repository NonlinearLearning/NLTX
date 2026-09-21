namespace Terraria.Content.StatusEffects;

public sealed class WhipTagEffectDefinition : TagEffectDefinition
{
  public WhipTagEffectDefinition(
    int effectTypeId,
    bool networkSyncPolicy,
    bool syncProcTimers,
    int tagDurationTicks,
    int playerBuffId,
    int playerBuffDurationTicks,
    bool applyPlayerBuffManually,
    int tagCritChance,
    int tagDamage)
    : base(
      effectTypeId,
      networkSyncPolicy,
      syncProcTimers,
      tagDurationTicks)
  {
    PlayerBuffId = playerBuffId;
    PlayerBuffDurationTicks = playerBuffDurationTicks;
    ApplyPlayerBuffManually = applyPlayerBuffManually;
    TagCritChance = tagCritChance;
    TagDamage = tagDamage;
  }

  public bool ApplyPlayerBuffManually { get; }

  public int PlayerBuffDurationTicks { get; }

  public int PlayerBuffId { get; }

  public int TagCritChance { get; }

  public int TagDamage { get; }
}
