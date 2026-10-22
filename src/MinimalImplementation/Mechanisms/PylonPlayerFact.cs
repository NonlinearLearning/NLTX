namespace MinimalImplementation.Server.Mechanisms;

/// <summary>Player hitbox captured in pixels from the authoritative player owner.</summary>
public sealed record PylonPlayerFact(double PositionX, double PositionY, int Width, int Height);
