using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

public sealed record PacketGatewayDiagnostic(ConnectionIdentity Connection, string Code,
    byte? MessageId = null, PacketSendCertainty? SendCertainty = null, int? BodyOffset = null,
    ushort? ModuleId = null, byte? Action = null, int? BodyLength = null);
