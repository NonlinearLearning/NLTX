namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class ShaderAssetAdapter
{
  private readonly IShaderAssetSource _source;

  public ShaderAssetAdapter(IShaderAssetSource source)
  {
    _source = source ?? throw new ArgumentNullException(nameof(source));
  }

  public bool TryResolve(ShaderHandle handle, string assetToken, out string resolvedToken)
  {
    if (!handle.IsValid || string.IsNullOrWhiteSpace(assetToken))
    {
      resolvedToken = string.Empty;
      return false;
    }

    return _source.TryResolve(assetToken, out resolvedToken);
  }
}
