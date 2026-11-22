namespace Terraria.Attributes;

/// <summary>Cross-domain request to consume one target's shared shield pool.</summary>
/// <remarks>
/// This is a command request, not a queued one: Combat decides how much shield the hit absorbs and
/// must know the outcome in the same call, so the Attributes domain performs the write and returns
/// the result immediately. Queueing it into a later phase would destroy the "shield no longer
/// covers this hit" abort that protects the damage commit.
/// </remarks>
public readonly record struct AttributeShieldConsumeRequestComponent(int RequestedDamage);
