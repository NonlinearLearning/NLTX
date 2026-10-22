namespace MinimalImplementation.Combat.TagEffects;

/// <summary>Immutable Version4 facts used by the server's TagEffectState owner.</summary>
public sealed class TagEffectDefinition
{
  private readonly HashSet<short> _whipImmuneNpcTypes;
  private readonly IReadOnlyDictionary<short, float> _summonTagDamageMultipliers;

  public TagEffectDefinition(
    short effectType,
    bool netSync,
    bool syncProcs,
    int tagDuration,
    IReadOnlyCollection<short> whipImmuneNpcTypes,
    int tagDamage = 0,
    int critChance = 0,
    IReadOnlyDictionary<short, float>? summonTagDamageMultipliers = null,
    float procDamageMultiplier = 1f,
    TagEffectTaggedAction taggedAction = TagEffectTaggedAction.None,
    TagEffectProcAction procAction = TagEffectProcAction.None)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(effectType);
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tagDuration);
    ArgumentOutOfRangeException.ThrowIfNegative(tagDamage);
    if (critChance is < 0 or > 100)
    {
      throw new ArgumentOutOfRangeException(nameof(critChance));
    }
    if (!float.IsFinite(procDamageMultiplier) || procDamageMultiplier < 1f)
    {
      throw new ArgumentOutOfRangeException(nameof(procDamageMultiplier));
    }
    ArgumentNullException.ThrowIfNull(whipImmuneNpcTypes);
    if (whipImmuneNpcTypes.Any(static type => type < 0))
    {
      throw new ArgumentOutOfRangeException(nameof(whipImmuneNpcTypes));
    }

    EffectType = effectType;
    NetSync = netSync;
    SyncProcs = syncProcs;
    TagDuration = tagDuration;
    TagDamage = tagDamage;
    CritChance = critChance;
    ProcDamageMultiplier = procDamageMultiplier;
    TaggedAction = taggedAction;
    ProcAction = procAction;
    _whipImmuneNpcTypes = new HashSet<short>(whipImmuneNpcTypes);
    Dictionary<short, float> multipliers = summonTagDamageMultipliers is null
      ? new Dictionary<short, float>()
      : new Dictionary<short, float>(summonTagDamageMultipliers);
    if (multipliers.Any(static pair => pair.Key <= 0 || !float.IsFinite(pair.Value) || pair.Value < 0f))
    {
      throw new ArgumentException("Summon tag multipliers require positive projectile types and finite values.",
        nameof(summonTagDamageMultipliers));
    }

    _summonTagDamageMultipliers = new System.Collections.ObjectModel.ReadOnlyDictionary<short, float>(multipliers);
  }

  public short EffectType { get; }

  public bool NetSync { get; }

  public bool SyncProcs { get; }

  public int TagDuration { get; }

  public int TagDamage { get; }

  public int CritChance { get; }

  public float ProcDamageMultiplier { get; }

  public TagEffectTaggedAction TaggedAction { get; }

  public TagEffectProcAction ProcAction { get; }

  /// <summary>Version4 WhipTagEffect rejects NPC types marked ImmuneToWhips.</summary>
  public bool CanApplyTagToNpc(short npcType) => !_whipImmuneNpcTypes.Contains(npcType);

  public bool CanRunHitEffects(bool isOwned, bool isMinion, bool isMinionShot, bool isSentry)
  {
    return isOwned && (isMinion || isMinionShot || isSentry);
  }

  public int ModifyTaggedHit(
    int damage,
    short projectileType,
    bool isMinion,
    bool isMinionShot,
    bool isSentry,
    int bonusTagDamage)
  {
    return ModifyTaggedHit(
        damage,
        projectileType,
        isOwned: true,
        isMinion,
        isMinionShot,
        isSentry,
        bonusTagDamage);
  }

  public int ModifyTaggedHit(
    int damage,
    short projectileType,
    bool isOwned,
    bool isMinion,
    bool isMinionShot,
    bool isSentry,
    int bonusTagDamage)
  {
    if (!CanRunHitEffects(isOwned, isMinion, isMinionShot, isSentry))
    {
      return damage;
    }

    float multiplier = _summonTagDamageMultipliers.TryGetValue(projectileType, out float configured)
      ? configured
      : 1f;
    return checked(damage + (int)((TagDamage + bonusTagDamage) * multiplier));
  }

  public int ModifyProcHit(int damage)
  {
    return checked((int)(damage * ProcDamageMultiplier));
  }
}
