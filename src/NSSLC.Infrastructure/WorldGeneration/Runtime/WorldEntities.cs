using System;
using System.Collections.Generic;
using System.IO;
using NSSLC.WorldGeneration.Geometry;
using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Systems;

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
    public int housingCategory;
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
    private const int _minimumKnownNpcNetId = -65;

    public dynamic maxItems;
    private TownHousingRegistryComponent _registry;

    public bool HasRoom(int npcType, out NSSLC.WorldGeneration.Geometry.Point roomPosition)
    {
      if (TownHousingRegistrySystem.TryGetRoom(
            Registry,
            new TownHousingResidentKey(npcType),
            out TilePosition room))
      {
        roomPosition = new NSSLC.WorldGeneration.Geometry.Point(room.X, room.Y);
        return true;
      }

      roomPosition = default;
      return false;
    }

    public bool HasRoomQuick(int npcType)
    {
      return TownHousingRegistrySystem.HasRoom(
        Registry,
        new TownHousingResidentKey(npcType));
    }

    public bool CanNPCsLiveWithEachOther(int npcType, NSSLC.WorldGeneration.NPC npc)
    {
      ArgumentNullException.ThrowIfNull(npc);
      if (npcType < _minimumKnownNpcNetId || npcType >= NSSLC.WorldGeneration.NPCID.Count)
      {
        return true;
      }

      return NSSLC.WorldGeneration.NPC.GetHousingCategoryForType(npcType) != npc.housingCategory;
    }

    public bool CanNPCsLiveWithEachOther(
      NSSLC.WorldGeneration.NPC npc1,
      NSSLC.WorldGeneration.NPC npc2)
    {
      ArgumentNullException.ThrowIfNull(npc1);
      ArgumentNullException.ThrowIfNull(npc2);
      return npc1.housingCategory != npc2.housingCategory;
    }

    public void SetRoom(int npcType, int x, int y)
    {
      SetRoom(npcType, new NSSLC.WorldGeneration.Geometry.Point(x, y));
    }

    public void SetRoom(
      int npcType,
      NSSLC.WorldGeneration.Geometry.Point roomPosition)
    {
      TownHousingRegistrySystem.AssignRoom(
        Registry,
        new TownHousingResidentKey(npcType),
        new TilePosition(roomPosition.X, roomPosition.Y));
    }

    public void KickOut(NSSLC.WorldGeneration.NPC npc)
    {
      ArgumentNullException.ThrowIfNull(npc);
      TownHousingRegistrySystem.MarkHomeless(
        Registry,
        new TownHousingResidentKey((int)npc.type));
    }

    public void KickOut(int npcType)
    {
      TownHousingRegistrySystem.RemoveResident(
        Registry,
        new TownHousingResidentKey(npcType));
    }

    public void AddOccupantsToList(int x, int y, List<int> occupants)
    {
      AddOccupantsToList(
        new NSSLC.WorldGeneration.Geometry.Point(x, y),
        occupants);
    }

    public void AddOccupantsToList(
      NSSLC.WorldGeneration.Geometry.Point tilePosition,
      List<int> occupants)
    {
      ArgumentNullException.ThrowIfNull(occupants);
      IReadOnlyList<TownHousingResidentKey> residents =
        TownHousingRegistrySystem.GetOccupants(
          Registry,
          new TilePosition(tilePosition.X, tilePosition.Y));
      for (int index = 0; index < residents.Count; index++)
      {
        occupants.Add(residents[index].NpcType);
      }
    }

    public byte GetHouseholdStatus(NSSLC.WorldGeneration.NPC npc)
    {
      ArgumentNullException.ThrowIfNull(npc);
      if (npc.homeless)
      {
        return 1;
      }

      return HasRoomQuick((int)npc.type) ? (byte)2 : (byte)0;
    }

    public void Save(BinaryWriter writer)
    {
      TownHousingRegistryPersistenceAdapter.Save(writer, Registry);
    }

    public void Load(BinaryReader reader)
    {
      TownHousingRegistryPersistenceAdapter.Load(reader, Registry);
    }

    public void Clear()
    {
      TownHousingRegistrySystem.Clear(Registry);
    }

    public void Bind(TownHousingRegistryComponent registry)
    {
      _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    private TownHousingRegistryComponent Registry =>
      _registry ?? NSSLC.WorldGeneration.Main.ActiveWorldSession.TownHousing;
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
