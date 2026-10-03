namespace NSSLC.WorldGeneration.Geometry;

public struct Vector3 {
  public float X;
  public float Y;
  public float Z;
  public Vector3(float x, float y, float z) {
    X = x;
    Y = y;
    Z = z;
  }
  public static Vector3 operator *(Vector3 value, float scale) =>
    new(value.X * scale, value.Y * scale, value.Z * scale);
}
