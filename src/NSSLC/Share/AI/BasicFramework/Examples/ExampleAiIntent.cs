using System.Numerics;

namespace Terraria.Share.Ai.Examples;

/// <summary>
/// Shared HTN/GOAP output. Value is a jump direction or destination, depending on Kind.
/// RequestId correlates attack feedback within one binding; it does not identify an entity.
/// </summary>
public readonly record struct ExampleAiIntent(
    ExampleAiIntentKind Kind, Vector2 Value, int TargetRevision = 0, long RequestId = 0);
