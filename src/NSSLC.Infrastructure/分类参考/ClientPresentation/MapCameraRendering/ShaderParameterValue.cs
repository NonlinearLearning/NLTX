using System.Numerics;

namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct ShaderParameterValue(
  ShaderValueKind Kind,
  float Scalar,
  Vector4 Vector,
  string? AssetToken)
{
  public static ShaderParameterValue FromScalar(float value)
  {
    if (!float.IsFinite(value))
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    return new ShaderParameterValue(ShaderValueKind.Scalar, value, default, null);
  }

  public static ShaderParameterValue FromVector(Vector4 value)
  {
    if (!float.IsFinite(value.X) ||
      !float.IsFinite(value.Y) ||
      !float.IsFinite(value.Z) ||
      !float.IsFinite(value.W))
    {
      throw new ArgumentOutOfRangeException(nameof(value));
    }

    return new ShaderParameterValue(ShaderValueKind.Vector, 0f, value, null);
  }

  public static ShaderParameterValue FromAsset(string assetToken)
  {
    if (string.IsNullOrWhiteSpace(assetToken))
    {
      throw new ArgumentException("A shader asset token is required.", nameof(assetToken));
    }

    return new ShaderParameterValue(ShaderValueKind.Asset, 0f, default, assetToken);
  }
}
