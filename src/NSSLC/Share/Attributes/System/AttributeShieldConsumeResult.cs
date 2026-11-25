namespace Terraria.Share.Attributes;

/// <summary>Outcome of one shield consumption attempt.</summary>
/// <param name="HasShieldPool">Whether the target carried a shield pool.</param>
/// <param name="Consumed">Shield actually consumed; zero when the pool was insufficient.</param>
/// <remarks>
/// <see cref="HasShieldPool"/> is separate from <see cref="Consumed"/> on purpose: a target without
/// attributes must not be treated as "shield ran out", so callers can tell "nothing to consume"
/// apart from "consumed less than requested".
/// </remarks>
public readonly record struct AttributeShieldConsumeResult(bool HasShieldPool, int Consumed)
{
  [Obsolete("Use HasShieldPool.")]
  public bool HasAttributes => HasShieldPool;
}
