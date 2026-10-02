namespace Terraria.Network.Presentation;

public readonly record struct NetworkBackgroundPresentationCommand(
  int InstantTransitionCounter,
  int BackgroundDelay,
  int BackgroundStyle,
  float FrontLayerAlpha,
  float FarBackLayerAlpha,
  int WallOfFleshNpcIndex,
  int DrawAreaTop,
  int DrawAreaBottom);
