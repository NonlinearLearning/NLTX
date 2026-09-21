namespace Terraria.Player;

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
