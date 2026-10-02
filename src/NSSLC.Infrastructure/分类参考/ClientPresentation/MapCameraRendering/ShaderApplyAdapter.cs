namespace NLTX.ClientPresentation.MapCameraRendering;

public sealed class ShaderApplyAdapter
{
  private readonly IShaderParameterSink _sink;
  private readonly ShaderAssetAdapter _assets;

  public ShaderApplyAdapter(
    IShaderParameterSink sink,
    ShaderAssetAdapter assets)
  {
    _sink = sink ?? throw new ArgumentNullException(nameof(sink));
    _assets = assets ?? throw new ArgumentNullException(nameof(assets));
  }

  public ShaderApplyResult Apply(
    ShaderHandle handle,
    ShaderParameterComponent parameters,
    ShaderFamilyParameterComponent familyParameters)
  {
    ArgumentNullException.ThrowIfNull(parameters);
    ArgumentNullException.ThrowIfNull(familyParameters);
    if (!handle.IsValid)
    {
      return new ShaderApplyResult(false, 0, "invalid-handle");
    }

    if (parameters.Disabled)
    {
      return new ShaderApplyResult(false, 0, "disabled");
    }

    IReadOnlyDictionary<string, ShaderParameterValue> values = parameters.Snapshot();
    IReadOnlyDictionary<string, ShaderParameterValue> familyValues = familyParameters.Snapshot();
    foreach (ShaderParameterValue value in values.Values)
    {
      if (!TryResolveAsset(handle, value, out string? failure))
      {
        return new ShaderApplyResult(false, 0, failure);
      }
    }

    foreach (ShaderParameterValue value in familyValues.Values)
    {
      if (!TryResolveAsset(handle, value, out string? failure))
      {
        return new ShaderApplyResult(false, 0, failure);
      }
    }

    _sink.Apply(handle, values, familyValues);
    parameters.ClearStaged();
    familyParameters.ClearStaged();
    return new ShaderApplyResult(
      true,
      values.Count + familyValues.Count,
      null);
  }

  private bool TryResolveAsset(
    ShaderHandle handle,
    ShaderParameterValue value,
    out string? failure)
  {
    if (value.Kind != ShaderValueKind.Asset)
    {
      failure = null;
      return true;
    }

    if (value.AssetToken is not null &&
      _assets.TryResolve(handle, value.AssetToken, out _))
    {
      failure = null;
      return true;
    }

    failure = "missing-asset";
    return false;
  }
}
