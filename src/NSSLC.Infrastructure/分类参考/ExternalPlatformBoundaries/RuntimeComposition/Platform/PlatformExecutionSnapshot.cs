namespace Terraria.ExternalPlatformBoundaries.RuntimeComposition.Platform;

public readonly record struct PlatformExecutionSnapshot(bool IsHeld, uint PreviousExecutionState);
