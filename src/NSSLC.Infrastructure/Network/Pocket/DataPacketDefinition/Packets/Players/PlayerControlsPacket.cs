namespace Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

public sealed partial class PlayerControlsPacket
{
    public byte Player { get; set; }
    public byte ControlFlags { get; set; }
    public byte MovementFlags { get; set; }
    public byte PlayerFeatureFlags { get; set; }
    public byte ActionFlags { get; set; }
    public byte SelectedItem { get; set; }
    public PacketVector2 Position { get; set; }
    public PacketVector2? Velocity { get; set; }
    public ushort? MountType { get; set; }
    public PacketVector2? PotionOfReturnUsePosition { get; set; }
    public PacketVector2? PotionOfReturnHomePosition { get; set; }
    public PacketVector2? CameraTarget { get; set; }
}
