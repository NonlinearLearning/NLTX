namespace Terraria.WorldSession.Runtime;

public sealed class ScreenShaderAssetAdapter
{
  public string? AssetKey { get; private set; }

  public void Load(string assetKey)
  {
    AssetKey = string.IsNullOrWhiteSpace(assetKey)
      ? throw new ArgumentException("An asset key is required.", nameof(assetKey))
      : assetKey;
  }

  public void Unload()
  {
    AssetKey = null;
  }
}
