namespace Terraria.NonAuthoritative.Diagnostics;

public sealed class LegacyDelegateOperationContext
{
  private bool[] _tileCutIgnore = Array.Empty<bool>();

  public Vector2Value Vector2Result { get; private set; }

  public Vector3Value Vector3Result { get; private set; }

  public float Scalar { get; private set; }

  public bool Result { get; private set; }

  public TileCuttingContextValue TileCuttingContext { get; private set; }

  public ReadOnlyMemory<bool> TileCutIgnore => _tileCutIgnore;

  public void SetVector2(Vector2Value value)
  {
    Vector2Result = value;
  }

  public void SetVector3(Vector3Value value)
  {
    Vector3Result = value;
  }

  public void SetScalar(float value)
  {
    Scalar = value;
  }

  public void SetResult(bool value)
  {
    Result = value;
  }

  public void SetTileCuttingContext(TileCuttingContextValue value)
  {
    TileCuttingContext = value;
  }

  public void SetTileCutIgnore(ReadOnlySpan<bool> values)
  {
    _tileCutIgnore = values.ToArray();
  }
}
