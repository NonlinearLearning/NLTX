using Newtonsoft.Json.Serialization;

namespace NSSLC.WorldGeneration.DataStructures
{
  public class EntitySource_ByProjectileSourceId : IEntitySource
  {
    public EntitySource_ByProjectileSourceId(int sourceId) { }
  }

  public class EntitySource_CoinRain : IEntitySource
  {
    public EntitySource_CoinRain(int x, int y) { }
  }

  public class EntitySource_WorldEvent : IEntitySource
  {
  }
}

namespace NSSLC.WorldGeneration.GameContent
{

}

namespace NSSLC.WorldGeneration.GameContent.Creative
{
  public class CreativePowers
  {
    public class FreezeTime
    {
      public bool Enabled;
    }

    public class StopBiomeSpreadPower
    {
      public bool Enabled;
      public bool GetIsUnlocked() => false;
    }
  }
}

namespace NSSLC.WorldGeneration.GameContent.Tile_Entities
{
  public class TEItemFrame
  {
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register(x, y, type);
    public NSSLC.WorldGeneration.Item item = new NSSLC.WorldGeneration.Item();
    public void DropItem() { }
  }

  public class TEWeaponsRack
  {
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register(x, y, type);
    public NSSLC.WorldGeneration.Item item = new NSSLC.WorldGeneration.Item();
    public void DropItem() { }
  }

  public class TEFoodPlatter
  {
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register(x, y, type);
    public NSSLC.WorldGeneration.Item item = new NSSLC.WorldGeneration.Item();
    public void DropItem() { }
  }

  public class TEDeadCellsDisplayJar
  {
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register(x, y, type);
    public NSSLC.WorldGeneration.Item item = new NSSLC.WorldGeneration.Item();
    public void DropItem() { }
  }

  public class TELeashedEntityAnchorWithItem
  {
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register(x, y, type);
    public NSSLC.WorldGeneration.Item item = new NSSLC.WorldGeneration.Item();
    public void DropItemForTileBreak() { }
  }

  public class TETrainingDummy
  {
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register(x, y, type);
  }
}

namespace NSSLC.WorldGeneration.GameContent.UI.States
{
  public class UIWorldLoad
  {
  }
}

namespace NSSLC.WorldGeneration.Testing
{
  public class EasyDeserializationJsonContractResolver : DefaultContractResolver
  {
  }
}
