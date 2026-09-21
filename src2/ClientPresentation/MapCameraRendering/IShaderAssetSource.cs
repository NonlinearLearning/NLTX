namespace NLTX.ClientPresentation.MapCameraRendering;

public interface IShaderAssetSource
{
  bool TryResolve(string assetToken, out string resolvedToken);
}
