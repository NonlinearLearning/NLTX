namespace Terraria.Network;

public readonly record struct SocialEmoteBubbleSnapshot(
    int BubbleId,
    byte AnchorKind,
    ushort AnchorId,
    ushort Lifetime,
    ushort LifetimeStart,
    byte Emote,
    short? Metadata);
