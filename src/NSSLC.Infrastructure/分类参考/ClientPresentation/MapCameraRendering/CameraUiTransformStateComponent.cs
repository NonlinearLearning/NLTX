using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class CameraUiTransformStateComponent
{
  public float UiScaleWanted { get; private set; } = 1f;

  public float UiScaleUsed { get; private set; } = 1f;

  public Vector2 CameraPosition { get; private set; }

  public Vector2 CameraSize { get; private set; }

  public Vector2 Zoom { get; private set; } = Vector2.One;

  public Vector2 Translation { get; private set; }

  public Vector2 Pan { get; private set; }

  public float Lerp { get; private set; }

  public int LerpTimer { get; private set; }

  public int LerpTimeToggle { get; private set; }

  public Matrix4x4 UiScaleMatrix { get; private set; } = Matrix4x4.Identity;

  public Matrix4x4 ZoomMatrix { get; private set; } = Matrix4x4.Identity;

  public Matrix4x4 TransformationMatrix { get; private set; } = Matrix4x4.Identity;

  public uint Revision { get; private set; }

  internal void Replace(CameraUiTransformInput input)
  {
    Validate(input);
    UiScaleWanted = input.UiScaleWanted;
    UiScaleUsed = input.UiScaleUsed;
    CameraPosition = input.CameraPosition;
    CameraSize = input.CameraSize;
    Zoom = input.Zoom;
    Translation = input.Translation;
    Pan = input.Pan;
    Lerp = input.Lerp;
    LerpTimer = input.LerpTimer;
    LerpTimeToggle = input.LerpTimeToggle;
    UiScaleMatrix = Matrix4x4.CreateScale(UiScaleUsed, UiScaleUsed, 1f);
    ZoomMatrix = Matrix4x4.CreateScale(Zoom.X, Zoom.Y, 1f);
    TransformationMatrix = ZoomMatrix *
      Matrix4x4.CreateTranslation(Translation.X + Pan.X, Translation.Y + Pan.Y, 0f) *
      UiScaleMatrix;
    Revision++;
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
