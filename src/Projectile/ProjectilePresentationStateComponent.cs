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
    bool hide = false)
  {
    Alpha = alpha;
    GlowMask = glowMask;
    Light = light;
    IsPreviewDummy = isPreviewDummy;
    IsPreviewDisplayDoll = isPreviewDisplayDoll;
    DrawLayer = drawLayer;
    UsesOwnerLight = usesOwnerLight;
    Hide = hide;
  }

  public int Alpha;
  public short GlowMask;
  public float Light;
  public bool IsPreviewDummy;
  public bool IsPreviewDisplayDoll;
  public int DrawLayer;
  public bool UsesOwnerLight;
  public bool Hide;

  public readonly float Opacity => 1.0f - (float)Alpha / 255.0f;
}
