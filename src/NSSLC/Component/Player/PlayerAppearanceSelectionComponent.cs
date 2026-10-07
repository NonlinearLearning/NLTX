namespace Terraria.Player;

// status: implemented-isolated-core
// source-members: P09-804, P09-805, P09-806
// crossSubsystemOwner: network bitmask, loadout exchange, and persistence remain integration-review
public sealed class PlayerAppearanceSelectionComponent
{
  public const int HiddenVisibleAccessoryCount = 10;

  public sbyte VoiceOverride { get; internal set; }

  public bool[] HiddenVisibleAccessories { get; } =
    new bool[HiddenVisibleAccessoryCount];

  // Version4 stores hideMisc as BitsByte; byte preserves its packed representation
  // until the protocol type is available in NLTX.
  public byte HideMiscBits { get; internal set; }

}
