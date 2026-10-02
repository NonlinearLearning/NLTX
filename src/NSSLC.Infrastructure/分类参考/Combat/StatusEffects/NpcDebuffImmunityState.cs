namespace Terraria.Combat.StatusEffects;

public sealed class NpcDebuffImmunityState
{
  private int[] _specificEffectIds = Array.Empty<int>();

  public bool ImmuneToWhips { get; private set; }

  public bool ImmuneToNonWhipBuffs { get; private set; }

  public IReadOnlyList<int> SpecificEffectIds => _specificEffectIds;

  public bool IsImmune(int effectId, bool isWhip)
  {
    return isWhip ? ImmuneToWhips :
      ImmuneToNonWhipBuffs || Array.BinarySearch(_specificEffectIds, effectId) >= 0;
  }

  internal bool Replace(
    bool immuneToWhips,
    bool immuneToNonWhipBuffs,
    IReadOnlyList<int> specificEffectIds)
  {
    bool changed = ImmuneToWhips != immuneToWhips ||
      ImmuneToNonWhipBuffs != immuneToNonWhipBuffs ||
      !_specificEffectIds.SequenceEqual(specificEffectIds);

    ImmuneToWhips = immuneToWhips;
    ImmuneToNonWhipBuffs = immuneToNonWhipBuffs;
    _specificEffectIds = specificEffectIds.ToArray();
    return changed;
  }
}
