namespace Terraria.Network;

public sealed record NetworkSessionContext(ConnectionIdentity Connection, string ProfileKey,
    NetworkSessionStage Stage, SenderBinding Actor, bool IsHost);
