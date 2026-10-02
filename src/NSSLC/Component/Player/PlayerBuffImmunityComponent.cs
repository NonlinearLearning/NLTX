namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-718
// crossSubsystemOwner: buff catalog capacity and effect rebuild remain integration-review
public sealed class PlayerBuffImmunityComponent
{
  private readonly bool[] _immuneBuffTypes;
  private readonly IReadOnlyList<bool> _immuneBuffTypesView;

  public PlayerBuffImmunityComponent(int buffTypeCount)
  {
    if (buffTypeCount < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(buffTypeCount));
    }

    _immuneBuffTypes = new bool[buffTypeCount];
    _immuneBuffTypesView = Array.AsReadOnly(_immuneBuffTypes);
  }

  public IReadOnlyList<bool> ImmuneBuffTypes => _immuneBuffTypesView;

  public bool IsImmune(ContentId<BuffDefinition> effectType)
  {
    return effectType.Value >= 0 &&
      effectType.Value < _immuneBuffTypes.Length &&
      _immuneBuffTypes[effectType.Value];
  }

  public void SetImmunity(ContentId<BuffDefinition> effectType, bool isImmune)
  {
    if (effectType.Value < 0 || effectType.Value >= _immuneBuffTypes.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(effectType));
    }

    _immuneBuffTypes[effectType.Value] = isImmune;
  }

  public void ResetForTick()
  {
    Array.Clear(_immuneBuffTypes);
  }

  public void ResetForLifecycle()
  {
    ResetForTick();
  }
}
