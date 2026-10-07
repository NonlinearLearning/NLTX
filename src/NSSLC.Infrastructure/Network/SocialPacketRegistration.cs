using Terraria.Network;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace NSSLC.Infrastructure.Network;

public static class SocialPacketRegistration {
  public static void Register(PacketGateway gateway, SocialPacketHandlers handlers) {
    ArgumentNullException.ThrowIfNull(gateway);
    ArgumentNullException.ThrowIfNull(handlers);

    gateway.Register<InstrumentSoundPacket>(new PacketPolicy(58, NetworkSessionStage.Active,
        MaximumPerWindow: 120, MaximumBytesPerWindow: 600), handlers);
    gateway.Register<ItemUseSoundPacket>(new PacketPolicy(152, NetworkSessionStage.Active,
        MaximumPerWindow: 120, MaximumBytesPerWindow: 120), handlers);
    gateway.Register<RequestLucyPopupPacket>(new PacketPolicy(141, NetworkSessionStage.Active),
        handlers);
    gateway.Register<EmojiPacket>(new PacketPolicy(120, NetworkSessionStage.Active,
        MaximumPerWindow: 120, MaximumBytesPerWindow: 240), handlers);
  }

  /// <summary>Registers packet 144 when the host uses the Steam projectile packet layout.</summary>
  public static void RegisterSteamNpcEffects(PacketGateway gateway,
      SocialPacketHandlers handlers) {
    ArgumentNullException.ThrowIfNull(gateway);
    ArgumentNullException.ThrowIfNull(handlers);

    gateway.Register<RequestQuestEffectPacket>(new PacketPolicy(144,
        NetworkSessionStage.Active, MaximumPerWindow: 2, MaximumBytesPerWindow: 2), handlers);
  }
}
