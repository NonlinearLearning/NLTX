namespace Terraria.WorldSession.Components;

/// <summary>
/// 保存世界月亮、树、洞穴、背景和云的外观选择。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Main。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Main.cs。</para>
/// <para>
/// 主要源成员：treeX（第 364 行）； treeStyle（第 366 行）； caveBackX（第 368 行）； caveBackStyle（第 370 行）；
/// iceBackStyle（第 372 行）； hellBackStyle（第 374 行）； jungleBackStyle（第 376 行）； moonType（第 679 行）。
/// </para>
/// </remarks>
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

