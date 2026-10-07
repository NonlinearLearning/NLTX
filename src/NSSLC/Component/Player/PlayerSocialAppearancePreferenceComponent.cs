namespace Terraria.Player;

/// <summary>
/// 保存玩家皮肤和声音偏好。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.Player。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria/Player.cs。</para>
/// <para>主要源成员：skinVariant（第 904 行）； voiceVariant（第 906 行）； voicePitchOffset（第 908 行）。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-2/。</para>
/// <para>拆分依据文件：2026-09-11-version4-P06-player-combat-status-component-design.md。</para>
/// <para>依据位置：第 477 行。</para>
/// </remarks>
public sealed class PlayerSocialAppearancePreferenceComponent
{
  public int SkinVariant { get; internal set; }

  public int VoiceVariant { get; internal set; }

  public float VoicePitchOffset { get; internal set; }

  internal void ResetForLifecycle()
  {
    SkinVariant = 0;
    VoiceVariant = 0;
    VoicePitchOffset = 0f;
  }
}
