using Terraria.ClientPresentation.Ui.Tree;

namespace Terraria.ClientPresentation.Ui.Rendering;

public sealed class UiElementGraphicsPolicyAdapter
{
  public Policy GetPolicy(UiElementInteractionQuery.Snapshot snapshot)
  {
    return new Policy(
      snapshot.UseImmediateMode,
      snapshot.OverflowHidden,
      snapshot.OverflowHidden ? RasterizerMode.Scissor : RasterizerMode.None,
      snapshot.UseImmediateMode ? SamplerMode.PointClamp : SamplerMode.Default);
  }

  public enum SamplerMode
  {
    Default,
    PointClamp
  }

  public enum RasterizerMode
  {
    None,
    Scissor
  }

  public readonly record struct Policy(
    bool UseImmediateMode,
    bool ClipOverflow,
    RasterizerMode Rasterizer,
    SamplerMode Sampler);
}
