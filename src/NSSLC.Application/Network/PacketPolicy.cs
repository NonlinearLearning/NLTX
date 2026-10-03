namespace Terraria.Network;

public sealed record PacketPolicy(byte MessageId, NetworkSessionStage AllowedStages,
    ushort? ModuleId = null, byte? Action = null, int MaximumPerWindow = 120,
    int MaximumBytesPerWindow = 128 * 1024, bool RequiresHost = false);
