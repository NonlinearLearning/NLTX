namespace NSSLC.WorldGeneration.Geometry;

public struct Vector4 {
  public float X;
  public float Y;
  public float Z;
  public float W;
  public Vector4(Vector2 first, float z, float w) {
    X = first.X;
    Y = first.Y;
    Z = z;
    W = w;
  }
}
