using Terraria.Network;

namespace NSSLC.Infrastructure.Network;

internal sealed record PacketHandlerRegistration(PacketPolicy Policy,
    Func<NetworkSessionContext, object, CancellationToken, ValueTask<PacketHandlingResult>> Handle);
