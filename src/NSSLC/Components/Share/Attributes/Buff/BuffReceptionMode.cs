namespace Terraria.StatusEffects;

/// <summary>Controls which shared Buff definitions an entity may receive.</summary>
public enum BuffReceptionMode : byte
{
  Disabled = 0,
  AllowListed = 1,
  AllowAll = 2
}
