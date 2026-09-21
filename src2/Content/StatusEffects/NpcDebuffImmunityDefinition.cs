namespace Terraria.Content.StatusEffects;

public sealed class NpcDebuffImmunityDefinition
{
  private readonly int[] _specificEffectIds;
  private readonly IReadOnlyList<int> _readOnlySpecificEffectIds;

  public NpcDebuffImmunityDefinition(
    bool immuneToWhips,
    bool immuneToNonWhipBuffs,
    IReadOnlyList<int> specificallyImmuneTo)
  {
    ArgumentNullException.ThrowIfNull(specificallyImmuneTo);
    if (specificallyImmuneTo.Any(effectId => effectId < 0))
    {
      throw new ArgumentOutOfRangeException(
        nameof(specificallyImmuneTo),
        "Status effect IDs cannot be negative.");
    }

    ImmuneToWhips = immuneToWhips;
    ImmuneToNonWhipBuffs = immuneToNonWhipBuffs;
    _specificEffectIds = specificallyImmuneTo.Distinct().Order().ToArray();
    _readOnlySpecificEffectIds = Array.AsReadOnly(_specificEffectIds);
  }

  public bool ImmuneToWhips { get; }

  public bool ImmuneToNonWhipBuffs { get; }

  public IReadOnlyList<int> SpecificEffectIds => _readOnlySpecificEffectIds;

  public bool IsImmuneTo(int effectId, bool isWhip)
  {
    return isWhip ? ImmuneToWhips :
      ImmuneToNonWhipBuffs || Array.BinarySearch(_specificEffectIds, effectId) >= 0;
  }
}
