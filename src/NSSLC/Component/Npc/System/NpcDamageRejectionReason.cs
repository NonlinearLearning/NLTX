namespace Terraria.Npc;

public enum NpcDamageRejectionReason : byte
{
  None,
  InvalidNpcType,
  InvalidDamage,
  InvalidContributor,
  InvalidMultiplier,
  Inactive,
  NoHealth,
  Immortal,
  InvalidParentRelation,
  InvalidIdentity,
  DamagePolicy,
}
