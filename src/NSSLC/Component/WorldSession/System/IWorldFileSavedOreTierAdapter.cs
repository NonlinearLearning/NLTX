namespace Terraria.WorldSession.Components;

/// <summary>
/// Maps versioned world-file fields to and from the saved-tier value boundary.
/// </summary>
public interface IWorldFileSavedOreTierAdapter
{
  OreTierState Read(in WorldSavedOreTierFileInput input);

  WorldSavedOreTierFileSaveValues PrepareSave(in OreTierState state);
}
