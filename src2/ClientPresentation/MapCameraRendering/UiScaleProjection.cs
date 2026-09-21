using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class UiScaleProjection
{
  public Vector2 Project(
    CameraUiTransformStateComponent state,
    Vector2 unscaledPoint)
  {
    ArgumentNullException.ThrowIfNull(state);
    if (!float.IsFinite(unscaledPoint.X) || !float.IsFinite(unscaledPoint.Y))
    {
      throw new ArgumentOutOfRangeException(nameof(unscaledPoint));
    }

    return Vector2.Transform(unscaledPoint, state.UiScaleMatrix);
  }
}
