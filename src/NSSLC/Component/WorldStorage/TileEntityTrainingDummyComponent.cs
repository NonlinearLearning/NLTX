namespace Terraria.WorldStorage;

/// <summary>Live Training Dummy TileEntity capability state.</summary>
public struct TileEntityTrainingDummyComponent
{
  public short NpcIndex;

  public TileEntityTrainingDummyComponent(short npcIndex)
  {
    NpcIndex = npcIndex;
  }
}
