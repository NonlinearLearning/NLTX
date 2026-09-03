namespace Terraria.Dome.Simulation.Player.Components;

public struct PlayerEquipmentModifierStateComponent
{
  public bool MeleeScaleGlove;

  public void Rebuild(bool meleeScaleGlove)
  {
    MeleeScaleGlove = meleeScaleGlove;
  }
}
