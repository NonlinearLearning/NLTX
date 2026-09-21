namespace NLTX.ClientPresentation.MapCameraRendering;

public readonly record struct SpriteBatchStateInput(
  string SortMode,
  string BlendState,
  string SamplerState,
  string DepthStencilState,
  string RasterizerState,
  string? EffectToken,
  string TransformToken);
