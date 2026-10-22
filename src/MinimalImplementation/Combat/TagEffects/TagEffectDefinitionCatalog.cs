namespace MinimalImplementation.Combat.TagEffects;

/// <summary>Contains only explicitly migrated Version4 TagEffect content facts.</summary>
public sealed class TagEffectDefinitionCatalog
{
  private static readonly short[] Version4WhipImmuneNpcTypes =
  [68, 72, 392, 440, 491, 549, 583, 584, 585, 686, 687];

  private static readonly IReadOnlyDictionary<short, float> Version4SummonTagMultipliers =
    new Dictionary<short, float>
    {
      [4911] = 1.15f,
      [4678] = 1.15f,
      [5478] = 1.15f,
      [4680] = 1.15f,
      [4679] = 1.15f,
      [5479] = 1.15f,
      [4914] = 1.15f,
      [5480] = 1.15f,
      [4912] = 1.15f
    };

  private static readonly IReadOnlyDictionary<short, int> Version4TagDamage =
    new Dictionary<short, int>
    {
      [5688] = 3,
      [5473] = 3,
      [4672] = 4,
      [5474] = 5,
      [5475] = 5,
      [5476] = 2,
      [4913] = 6,
      [5074] = 7,
      [4911] = 6,
      [4678] = 9,
      [5477] = 9,
      [4680] = 15,
      [4679] = 8,
      [5478] = 12,
      [5479] = 15,
      [4914] = 20,
      [5480] = 25,
      [4912] = 0,
      [6143] = 4
    };

  private readonly Dictionary<short, TagEffectDefinition> _definitions;

  public TagEffectDefinitionCatalog(IReadOnlyCollection<TagEffectDefinition> definitions)
  {
    ArgumentNullException.ThrowIfNull(definitions);
    _definitions = new Dictionary<short, TagEffectDefinition>(definitions.Count);
    foreach (TagEffectDefinition definition in definitions)
    {
      ArgumentNullException.ThrowIfNull(definition);
      if (!_definitions.TryAdd(definition.EffectType, definition))
      {
        throw new ArgumentException(
          $"TagEffect type {definition.EffectType} has more than one content definition.",
          nameof(definitions));
      }
    }
  }

  public static TagEffectDefinitionCatalog CreateVersion4()
  {
    // Facts are from Terraria.ID.ItemID.Sets.UniqueTagEffects and WhipTagEffect.
    // Only 5478 sets NetSync; no registered Version4 effect sets SyncProcs.
    short[] effectTypes =
    [5688, 5473, 4672, 5474, 5475, 5476, 4913, 5074, 4911, 4678,
      5477, 4680, 4679, 5478, 5479, 4914, 5480, 4912, 6143];

    var definitions = new TagEffectDefinition[effectTypes.Length];
    for (int index = 0; index < effectTypes.Length; index++)
    {
      short type = effectTypes[index];
      definitions[index] = new TagEffectDefinition(
        type,
        netSync: type == 5478,
        syncProcs: false,
        tagDuration: 240,
        whipImmuneNpcTypes: Version4WhipImmuneNpcTypes,
        tagDamage: Version4TagDamage[type],
        critChance: type switch
        {
          5479 => 15,
          4679 => 10,
          4914 or 5480 or 4912 => 10,
          5478 => 5,
          6143 => 10,
          _ => 0
        },
        summonTagDamageMultipliers: Version4SummonTagMultipliers,
        procDamageMultiplier: type == 4912 ? 2.75f : 1f,
        taggedAction: type == 4680
          ? TagEffectTaggedAction.DarkHarvestLightning
          : TagEffectTaggedAction.None,
        procAction: type switch
        {
          5476 => TagEffectProcAction.StarcrashMeteor,
          5477 => TagEffectProcAction.FlowerPetals,
          5480 => TagEffectProcAction.PossessionMoonLordWhip,
          4912 => TagEffectProcAction.FirecrackerExplosion,
          _ => TagEffectProcAction.None
        });
    }

    return new TagEffectDefinitionCatalog(definitions);
  }

  public bool TryGet(short effectType, out TagEffectDefinition definition)
  {
    return _definitions.TryGetValue(effectType, out definition!);
  }
}
