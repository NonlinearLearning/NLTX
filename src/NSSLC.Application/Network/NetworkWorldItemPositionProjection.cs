namespace Terraria.Network;

/// <summary>Detached slot and position projection for Steam packet 160.</summary>
public readonly record struct NetworkWorldItemPositionProjection(
  short ItemIndex,
  float PositionX,
  float PositionY);
