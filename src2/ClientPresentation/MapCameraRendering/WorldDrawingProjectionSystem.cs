using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class WorldDrawingProjectionSystem
{
  public void Apply(
    WorldDrawingProjectionComponent component,
    WorldDrawingInput input)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (!float.IsFinite(input.HorizonBlend) ||
      input.HorizonBlend < 0f ||
      input.HorizonBlend > 1f)
    {
      throw new ArgumentOutOfRangeException(nameof(input.HorizonBlend));
    }

    component.Replace(input);
  }

  public EntityShadowSnapshot ProjectShadow(
    WorldDrawingProjectionComponent component,
    Guid entityId,
    Vector2 position,
    float rotation,
    float opacity,
    RgbaColor color,
    uint sourceRevision)
  {
    ArgumentNullException.ThrowIfNull(component);
    if (entityId == Guid.Empty)
    {
      throw new ArgumentException("An entity snapshot requires an identity.", nameof(entityId));
    }

    if (!float.IsFinite(position.X) ||
      !float.IsFinite(position.Y) ||
      !float.IsFinite(rotation))
    {
      throw new ArgumentOutOfRangeException(nameof(position));
    }

    if (!float.IsFinite(opacity) || opacity < 0f || opacity > 1f)
    {
      throw new ArgumentOutOfRangeException(nameof(opacity));
    }

    EntityShadowSnapshot snapshot = new(
      entityId,
      position,
      rotation,
      opacity,
      color,
      sourceRevision);
    component.AddShadow(snapshot);
    return snapshot;
  }
}
