namespace Terraria.Share.Ai.Examples;

/// <summary>Actual execution feedback for an attack request from this binding.</summary>
public readonly record struct ExampleAttackFeedbackComponent(long RequestId, bool Finished);
