using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class CameraUiTransformSystem
{
  public void Apply(
    CameraUiTransformStateComponent state,
    CameraUiTransformInput input)
  {
    ArgumentNullException.ThrowIfNull(state);
    Validate(input);
    state.Replace(input);
  }

  private static void Validate(CameraUiTransformInput input)
  {
    EnsurePositiveFinite(input.UiScaleWanted, nameof(input.UiScaleWanted));
    EnsurePositiveFinite(input.UiScaleUsed, nameof(input.UiScaleUsed));
    EnsurePositiveFinite(input.Zoom.X, nameof(input.Zoom));
    EnsurePositiveFinite(input.Zoom.Y, nameof(input.Zoom));
    EnsureFinite(input.CameraPosition, nameof(input.CameraPosition));
    EnsurePositiveFinite(input.CameraSize.X, nameof(input.CameraSize));
    EnsurePositiveFinite(input.CameraSize.Y, nameof(input.CameraSize));
    EnsureFinite(input.Translation, nameof(input.Translation));
    EnsureFinite(input.Pan, nameof(input.Pan));

    if (!float.IsFinite(input.Lerp) || input.Lerp < 0f || input.Lerp > 1f)
    {
      throw new ArgumentOutOfRangeException(nameof(input.Lerp));
    }

    if (input.LerpTimer < 0 || input.LerpTimeToggle < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(input.LerpTimer));
    }
  }

  private static void EnsurePositiveFinite(float value, string parameterName)
  {
    if (!float.IsFinite(value) || value <= 0f)
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }

  private static void EnsureFinite(Vector2 value, string parameterName)
  {
    if (!float.IsFinite(value.X) || !float.IsFinite(value.Y))
    {
      throw new ArgumentOutOfRangeException(parameterName);
    }
  }
}
