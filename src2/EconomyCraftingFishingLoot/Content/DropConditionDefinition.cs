using NLTX.EconomyCraftingFishingLoot.Loot;

namespace NLTX.EconomyCraftingFishingLoot.Content;

public sealed class DropConditionDefinition
{
  private DropConditionDefinition(
    DropConditionKind kind,
    string? neededName,
    int aiSlotToCheck,
    float valueToMatch,
    int neededWave,
    string? integrationKey)
  {
    Kind = kind;
    NeededName = neededName;
    AiSlotToCheck = aiSlotToCheck;
    ValueToMatch = valueToMatch;
    NeededWave = neededWave;
    IntegrationKey = integrationKey;
  }

  public DropConditionKind Kind { get; }

  public string? NeededName { get; }

  public int AiSlotToCheck { get; }

  public float ValueToMatch { get; }

  public int NeededWave { get; }

  public string? IntegrationKey { get; }

  public static DropConditionDefinition NamedNpc(string neededName)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(neededName);
    return new DropConditionDefinition(
      DropConditionKind.NamedNpc,
      neededName,
      0,
      0,
      0,
      null);
  }

  public static DropConditionDefinition SpecificAiValue(int aiSlotToCheck, float valueToMatch)
  {
    if (aiSlotToCheck < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(aiSlotToCheck));
    }

    if (!float.IsFinite(valueToMatch))
    {
      throw new ArgumentOutOfRangeException(nameof(valueToMatch));
    }

    return new DropConditionDefinition(
      DropConditionKind.SpecificAiValue,
      null,
      aiSlotToCheck,
      valueToMatch,
      0,
      null);
  }

  public static DropConditionDefinition FromWave(int neededWave)
  {
    if (neededWave < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(neededWave));
    }

    return new DropConditionDefinition(
      DropConditionKind.FromCertainWaveAndAbove,
      null,
      0,
      0,
      neededWave,
      null);
  }

  public static DropConditionDefinition IntegrationOwned(string integrationKey)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(integrationKey);
    return new DropConditionDefinition(
      DropConditionKind.IntegrationOwned,
      null,
      0,
      0,
      0,
      integrationKey);
  }

  public bool Matches(DropResolutionContext context)
  {
    ArgumentNullException.ThrowIfNull(context);
    return Kind switch
    {
      DropConditionKind.NamedNpc => context.NpcName == NeededName,
      DropConditionKind.SpecificAiValue =>
        context.TryGetNpcAiValue(AiSlotToCheck, out float value) &&
        value == ValueToMatch,
      DropConditionKind.FromCertainWaveAndAbove => context.Wave >= NeededWave,
      DropConditionKind.IntegrationOwned => false,
      _ => false,
    };
  }
}
