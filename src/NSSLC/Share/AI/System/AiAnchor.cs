namespace Terraria.Share.Ai;

/// <summary>
/// Capability roles an AI behavior can name when it needs to state when it runs.
/// </summary>
/// <remarks>
/// An anchor is a role, not a schedule position: it says what <em>kind of step</em> the AI must
/// precede or follow, never which registration implements it. The mapping from anchor to
/// registration key belongs to the composition root, because a domain cannot know its position
/// relative to other domains. <see cref="InputApply"/> means "the step that turns external input
/// into domain state", not "the system named PlayerInputRequestSystem".
///
/// The set is deliberately as small as the declarations that exist: every anchor must be bound
/// exactly once, so an anchor with no consumer is not free — it is vocabulary nobody can exercise,
/// and a binding that only looks checked. The plan's ceiling was five members; the review at P3
/// start found two consumers (input applied before the AI decides, movement intent committed after
/// it), so only those two exist. Adding a third anchor is a deliberate edit here, made when a
/// behavior actually needs to state that relation.
/// </remarks>
public enum AiAnchor : byte
{
  /// <summary>External input has been applied and is readable as domain state.</summary>
  InputApply = 0,

  /// <summary>Movement intents have been committed onto the components spatial solving reads.</summary>
  MovementCommit = 1
}
