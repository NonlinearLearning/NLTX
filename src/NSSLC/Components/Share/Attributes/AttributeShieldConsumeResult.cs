namespace Terraria.Attributes;

/// <summary>Outcome of one shield consumption attempt.</summary>
/// <param name="HasAttributes">Whether the target carried an attribute set at all.</param>
/// <param name="Consumed">Shield actually consumed; zero when the pool was insufficient.</param>
/// <remarks>
/// <see cref="HasAttributes"/> is separate from <see cref="Consumed"/> on purpose: a target without
/// attributes must not be treated as "shield ran out", so callers can tell "nothing to consume"
/// apart from "consumed less than requested".
/// </remarks>
public readonly record struct AttributeShieldConsumeResult(bool HasAttributes, int Consumed);
