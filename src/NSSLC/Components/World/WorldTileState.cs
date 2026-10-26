namespace Terraria.World;

/// <summary>Read-only tile value returned by the world interaction boundary.</summary>
public readonly record struct WorldTileState(
    bool Active,
    ushort Type,
    ushort Wall,
    short FrameX,
    short FrameY);
