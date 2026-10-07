namespace Terraria.Projectile;

public struct ProjectilePresentationStateComponent
{
  public ProjectilePresentationStateComponent(
    int alpha = 0,
    short glowMask = 0,
    float light = 0.0f,
    bool isPreviewDummy = false,
    bool isPreviewDisplayDoll = false,
    int drawLayer = 0,
    bool usesOwnerLight = false,
    bool hide = false,
    int trailingMode = -1)
  {
    Alpha = alpha;
    GlowMask = glowMask;
    Light = light;
    IsPreviewDummy = isPreviewDummy;
    IsPreviewDisplayDoll = isPreviewDisplayDoll;
    DrawLayer = drawLayer;
    UsesOwnerLight = usesOwnerLight;
    Hide = hide;
    TrailingMode = trailingMode;
  }

  public int Alpha;
  public short GlowMask;
  public float Light;
  public bool IsPreviewDummy;
  public bool IsPreviewDisplayDoll;
  public int DrawLayer;
  public bool UsesOwnerLight;
  public bool Hide;
  public int TrailingMode;

  public float Opacity
  {
    readonly get => 1.0f - (float)Alpha / 255.0f;
    set => Alpha = (int)System.Math.Clamp((1.0f - value) * 255.0f, 0.0f, 255.0f);
  }
}
