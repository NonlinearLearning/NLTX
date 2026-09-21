namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct RgbaColor(byte R, byte G, byte B, byte A)
{
  public static RgbaColor White => new(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
}
