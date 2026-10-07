namespace Terraria.Npc;

/// <summary>
/// 保存 NPC 微光转化的透明度。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.NPC。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/NPC.cs。</para>
/// <para>主要源成员：shimmerTransparency（第 6061 行）。</para>
/// </remarks>
public sealed class NpcShimmerStateComponent
{
  public float Transparency { get; private set; }

  internal void ApplyTransparency(float transparency)
  {
    Transparency = transparency;
  }

  internal void ResetForLifecycle()
  {
    Transparency = 0.0f;
  }
}
