namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class WorldDrawingProjectionComponent
{
  private readonly List<EntityShadowSnapshot> _shadows = new();

  public RgbaColor HorizonColor { get; private set; }

  public float HorizonBlend { get; private set; }

  public string? ParticleToken { get; private set; }

  public IReadOnlyList<EntityShadowSnapshot> Shadows => _shadows.ToArray();

  public uint Revision { get; private set; }

  internal void Replace(WorldDrawingInput input)
  {
    HorizonColor = input.HorizonColor;
    HorizonBlend = input.HorizonBlend;
    ParticleToken = input.ParticleToken;
    _shadows.Clear();
    Revision++;
  }

  internal void AddShadow(EntityShadowSnapshot shadow)
  {
    _shadows.Add(shadow);
    Revision++;
  }
}
