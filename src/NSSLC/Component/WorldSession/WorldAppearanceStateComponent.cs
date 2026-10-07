namespace Terraria.WorldSession.Components;

public sealed class WorldAppearanceStateComponent {
  public byte MoonType { get; internal set; }
  public IReadOnlyList<int> TreeX { get; internal set; } = Array.Empty<int>();
  public IReadOnlyList<int> TreeStyle { get; internal set; } = Array.Empty<int>();
  public IReadOnlyList<int> CaveBackX { get; internal set; } = Array.Empty<int>();
  public IReadOnlyList<int> CaveBackStyle { get; internal set; } = Array.Empty<int>();
  public int IceBackStyle { get; internal set; }
  public int JungleBackStyle { get; internal set; }
  public int HellBackStyle { get; internal set; }
  public IReadOnlyList<byte> BackgroundStyles { get; internal set; } = Array.Empty<byte>();
  public int CloudBackgroundActive { get; internal set; }
  public short CloudCount { get; internal set; }
  public IReadOnlyList<byte> AdditionalBackgroundStyles { get; internal set; } = Array.Empty<byte>();
}

