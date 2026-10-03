using NSSLC.WorldGeneration.Geometry;

namespace NSSLC.WorldGeneration
{



  public partial class Player : IEntitySourceTarget
  {
    public sealed class PlayerHooks
    {
      public event System.Action<Player> OnEnterWorld;
    }

    public Vector2 Center;
    public Vector2 position;
    public Vector2 velocity;
    public int width;
    public int height;
    public int direction;
    public float gfxOffY;
    public int afkCounter;
    public bool active;
    public bool dead;
    public bool flowerBoots;
    public SleepingState sleeping = new SleepingState();
    public bool cordage;
    public bool ZoneGlowshroom;
    public bool ZoneUnderworldHeight;
    public bool ZoneDesert;
    public bool ZoneHallow;
    public bool ZoneCorrupt;
    public bool ZoneCrimson;
    public bool ZoneJungle;
    public bool ZoneSnow;
    public bool ZoneBeach;
    public bool ZoneRain;
    public bool ZoneGraveyard;
    public bool staffOfRegrowthBonus;
    public Item HeldItem = new Item();

    public static dynamic FindClosest(params dynamic[] arguments) => default;


    public static dynamic GetClosestRollLuck(params dynamic[] arguments) => default;
    public int RollLuck(int value) => value;
    public int RollBadLuck(int value) => value;
    public static dynamic AFKTimeNeededForNoLuckyStars;
    public static PlayerHooks Hooks = new PlayerHooks();
    public static dynamic SavePlayer(params dynamic[] arguments) => default;

    public sealed class SleepingState
    {
      public bool isSleeping;
    }
  }


  public partial class WorldItem
  {
    public Item inner = new Item();
    public int whoAmI;
    public dynamic timeSinceItemSpawned;
    public bool active;
    public int type;
    public int stack;
    public Vector2 position;
    public void OverrideWith(Item item) { }
  }


  public partial class Dust
  {
    public int dustIndex;
    public bool noGravity;
    public bool noLight;
    public bool noLightEmittance;
    public float scale;
    public float fadeIn;
    public Vector2 velocity;
  }

  public partial class NPC : IEntitySourceTarget
  {
    public bool active;
    public dynamic townNPC;
    public dynamic type;
    public int homeTileX;
    public int homeTileY;
    public int direction;
    public bool homeless;
    public bool homelessDespawn;
    public int lookForHomeTimeout;
    public int townNpcVariationIndex;
    public int width;
    public int height;
    public int whoAmI;
    public int life;
    public bool netUpdate;
    public NSSLC.WorldGeneration.Geometry.Vector2 position;
    public NSSLC.WorldGeneration.Geometry.Vector2 velocity;
    public NSSLC.WorldGeneration.Geometry.Vector2 Center;
    public string GivenName { get; set; }
    public string FullName => GivenName;
    public bool CanBeReplacedByOtherNPCs { get; set; }

    
    public static dynamic AnyNPCs(params dynamic[] arguments) => default;
    public static dynamic CountNPCS(params dynamic[] arguments) => default;
    public static dynamic SpawnOnPlayer(params dynamic[] arguments) => default;
    public static dynamic GetSpawnSourceForTownSpawn(params dynamic[] arguments) => default;
    public NSSLC.WorldGeneration.Localization.NetworkText GetFullNetName() => default;
    public int GetImmuneTime(int type) => default;
    public int GetImmuneTime(int type, int time) => default;
    public void TargetClosest(bool faceTarget = true) { }
  }

  public partial class Projectile
  {
    public bool active;
    public int type;
    public int whoAmI;
    public bool originatedFromActivableTile;
    public bool netUpdate;
    public static dynamic NewProjectile(params dynamic[] arguments) => default;
  }



}

namespace NSSLC.WorldGeneration.DataStructures
{
  public interface IEntitySource
  {
  }

  public class EntitySource_TileBreak : IEntitySource
  {
    public EntitySource_TileBreak(int x, int y)
    {
    }
  }


}

namespace NSSLC.WorldGeneration.Enums
{


  public enum TileCuttingContext
  {
  }

  public enum TileScanGroup
  {
    None,
    Corruption,
    Crimson,
    Hallow,
    TotalGoodEvil
  }

  public enum TownNPCRoomCheckFailureReason
  {
    None,
    TooCloseToWorldEdge,
    RoomCheckStartedInASolidTile,
    RoomIsTooSmall,
    RoomIsTooBig,
    TooManyUnsafeWalls,
    HoleInWallIsTooBig
  }

  public enum TownNPCSpawnResult
  {
    Successful,
    Blocked,
    RelocatedHomeless,
    BlockedInfiHousing,
    BlockedTooManyNPCs,
    FoundHouseNoSpawn
  }
}

namespace NSSLC.WorldGeneration.GameContent
{
  public class BackgroundChangeFlashInfo
  {
    public void UpdateCache() { }
  }

  public partial class TownRoomManager
  {
    public dynamic maxItems;
    public bool HasRoom(int npcType, out NSSLC.WorldGeneration.Geometry.Point roomPosition)
    {
      roomPosition = default;
      return default;
    }
    public bool HasRoom(params dynamic[] arguments) => default;
    public dynamic CanNPCsLiveWithEachOther(params dynamic[] arguments) => default;
    public void Clear() { }
    public void SetRoom(params dynamic[] arguments) { }
    public void KickOut(params dynamic[] arguments) { }
    public void AddOccupantsToList(params dynamic[] arguments) { }
    public bool HasRoomQuick(params dynamic[] arguments) => default;
  }

}

namespace NSSLC.WorldGeneration.GameContent.Events
{
  public class MysticLogFairiesEvent
  {
    public void WorldClear() { }
    public void StartWorld() { }
    public void FallenLogDestroyed(int x, int y) { }
    public void FallenLogDestroyed() { }
  }
}

namespace NSSLC.WorldGeneration.ID
{
  public enum TileChangeType : byte
  {
    None,
    LavaWater,
    HoneyWater,
    HoneyLava,
    ShimmerWater,
    ShimmerLava,
    ShimmerHoney
  }
}

namespace NSSLC.WorldGeneration.IO
{

}

namespace NSSLC.WorldGeneration.Audio
{
  public class LegacySoundStyle
  {
  }
}

namespace NSSLC.WorldGeneration.Localization
{
  public class LocalizedText
  {
    public string Key { get; set; }
    public string Value => Key;
  }

  public partial class NetworkText
  {
    public static NetworkText FromKey(params dynamic[] arguments) => default;
  }
}

namespace NSSLC.WorldGeneration.Utilities
{

}

namespace NSSLC.WorldGeneration.UI
{
  public class UIElement
  {
  }
}
