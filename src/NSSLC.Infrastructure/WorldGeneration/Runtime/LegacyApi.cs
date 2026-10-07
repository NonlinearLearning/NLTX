using System.Collections.Generic;

namespace NSSLC.WorldGeneration
{
  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public partial class AchievementsHelper
  {
    public static dynamic NotifyProgressionEvent(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class ActiveSections
  {
    public static dynamic Reset(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class BannerSystem
  {
    public static dynamic Clear(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class BirthdayParty
  {
    public static List<int> CelebratingNPCs = new();
    public static bool GenuineParty;
    public static bool ManualParty;
    public static int PartyDaysOnCooldown;

    public static dynamic WorldClear(params dynamic[] arguments)
    {
      CelebratingNPCs.Clear();
      GenuineParty = false;
      ManualParty = false;
      PartyDaysOnCooldown = 0;
      return null;
    }
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class ChatColors
  {
    public static dynamic BossOrEvent = default;
    public static dynamic NPCTravel = default;
    public static dynamic World = default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class ChatHelper
  {
    public static dynamic BroadcastChatMessage(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class Cloud
  {
    public static dynamic resetClouds(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class CombatText
  {
    public static dynamic clearAll(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class CreativePowerManager
  {
    public static dynamic Instance = default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class CreditsRollEvent
  {
    public static dynamic Reset(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class DD2Event
  {
    public static bool DownedInvasionT1;
    public static bool DownedInvasionT2;
    public static bool DownedInvasionT3;

    public static dynamic ResetProgressEntirely(params dynamic[] arguments)
    {
      DownedInvasionT1 = false;
      DownedInvasionT2 = false;
      DownedInvasionT3 = false;
      return null;
    }
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class DebugOptions
  {
    public static dynamic noLimits = default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class DelegateMethods
  {
    public static bool CheckResultOut;
    public static System.Action CheckStopForSolids = static () => { };
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class DontStarveDarknessDamageDealer
  {
    public static dynamic Reset(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public partial class Dust
  {
    // Generation suppresses particles and uses the original reserved dust slot.
    public static int NewDust(Geometry.Vector2 Position, int Width, int Height, int Type,
      float SpeedX = 0, float SpeedY = 0, int Alpha = 0,
      Geometry.Color newColor = default, float Scale = 1) => 6000;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class FileMetadata
  {
    public static dynamic FromCurrentSettings(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class FileType
  {
    public static dynamic World = default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class FileUtilities
  {
    public static dynamic Copy(params dynamic[] arguments) => default;
    public static dynamic Delete(params dynamic[] arguments) => default;
    public static dynamic Exists(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class FixExploitManEaters
  {
    public static dynamic SpotProtected(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class GameDifficultyLevel
  {
    public static dynamic Expert = default;
    public static dynamic Master = default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class GameNotificationType
  {
    public static dynamic WorldGen = default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class GitStatus
  {
    public static dynamic GitSHA = default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class Gore
  {
    public static dynamic NewGore(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class LanternNight
  {
    public static bool GenuineLanterns;
    public static int LanternNightsOnCooldown;
    public static bool ManualLanterns;
    public static bool NextNightIsLanternNight;

    public static dynamic WorldClear(params dynamic[] arguments)
    {
      GenuineLanterns = false;
      LanternNightsOnCooldown = 0;
      ManualLanterns = false;
      NextNightIsLanternNight = false;
      return null;
    }
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class LeashedEntity
  {
    public static dynamic Clear(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class MapUpdateQueue
  {
    public static dynamic Add(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class NetMessage
  {
    public static dynamic ResyncTiles(params dynamic[] arguments) => default;
    public static dynamic SendData(params dynamic[] arguments) => default;
    public static dynamic SendTileSquare(params dynamic[] arguments) => default;
    public static dynamic SendTravelShop(params dynamic[] arguments) => default;
    public static dynamic sendWater(params dynamic[] arguments) => default;
    public static dynamic TrySendData(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public partial class Netplay
  {
    public static dynamic ResetSections(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class NewProjectileModifiers
  {
    public static dynamic RainHazard = default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class NPCDamageTracker
  {
    public static dynamic Reset(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class PopupText
  {
    public static dynamic ClearAll(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class PressurePlateHelper
  {
    public static object EntityCreationLock = new object();
    public static Dictionary<Geometry.Point, bool[]> PressurePlatesPressed = new();
    public static bool NeedsFirstUpdate;

    public static dynamic DestroyPlate(params dynamic[] arguments) => default;
    public static dynamic Reset(params dynamic[] arguments)
    {
      PressurePlatesPressed.Clear();
      return null;
    }
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class Rain
  {
    public static dynamic GetRainFallVelocity(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class Sandstorm
  {
    public static bool Happening;
    public static float IntendedSeverity;
    public static float Severity;
    public static int TimeLeft;

    public static dynamic WorldClear(params dynamic[] arguments)
    {
      Happening = false;
      IntendedSeverity = 0f;
      Severity = 0f;
      TimeLeft = 0;
      return null;
    }
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class SceneMetrics
  {
    public const int MushroomTileThreshold = 100;
    public const int ShimmerTileThreshold = 300;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class Secrets
  {
    public static dynamic ToSecret(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class SoundEngine
  {
    public static dynamic PlaySound(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public partial class SoundID
  {
    public static dynamic Item127 = default;
    public static dynamic Item173 = default;
    public static dynamic Item177 = default;
    public static dynamic Item27 = default;
    public static dynamic Item30 = default;
    public static dynamic Item48 = default;
    public static dynamic Item49 = default;
    public static dynamic Item50 = default;
    public static dynamic Item52 = default;
    public static dynamic MenuAccept = default;
    public static dynamic NPCHit25 = default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class Star
  {
    public static dynamic starfallBoost = default;
    public static dynamic SpawnStars(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class TECritterAnchor : NSSLC.WorldGeneration.GameContent.Tile_Entities.TELeashedEntityAnchorWithItem
  {
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register<TECritterAnchor>(x, y, type);
    public static dynamic Kill(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class TEDisplayDoll : TileEntity
  {
    public NSSLC.WorldGeneration.Item[] items = CreateItems(19);
    public byte pose;
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register<TEDisplayDoll>(x, y, type);
    public static dynamic Framing_CheckTile(params dynamic[] arguments) => default;
    public static dynamic IsBreakable(params dynamic[] arguments) => default;

    private static NSSLC.WorldGeneration.Item[] CreateItems(int count)
    {
      var items = new NSSLC.WorldGeneration.Item[count];
      for (int index = 0; index < items.Length; index++)
      {
        items[index] = new NSSLC.WorldGeneration.Item();
      }
      return items;
    }
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class TEHatRack : TileEntity
  {
    public NSSLC.WorldGeneration.Item[] items = CreateItems(4);
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register<TEHatRack>(x, y, type);
    public static dynamic Framing_CheckTile(params dynamic[] arguments) => default;
    public static dynamic IsBreakable(params dynamic[] arguments) => default;

    private static NSSLC.WorldGeneration.Item[] CreateItems(int count)
    {
      var items = new NSSLC.WorldGeneration.Item[count];
      for (int index = 0; index < items.Length; index++)
      {
        items[index] = new NSSLC.WorldGeneration.Item();
      }
      return items;
    }
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class TEKiteAnchor : NSSLC.WorldGeneration.GameContent.Tile_Entities.TELeashedEntityAnchorWithItem
  {
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register<TEKiteAnchor>(x, y, type);
    public static dynamic Kill(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class TELogicSensor : TileEntity
  {
    public byte logicCheck;
    public bool On;
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register<TELogicSensor>(x, y, type);
    public static dynamic Kill(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class TETeleportationPylon : TileEntity
  {
    public static int PlacementPreviewHook_CheckIfCanPlace(int x, int y, int type, int style, int direction, int alternate) => 0;
    public static int PlacementPreviewHook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register<TETeleportationPylon>(x, y, type);
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register<TETeleportationPylon>(x, y, type);
    public static dynamic Framing_CheckTile(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class TEWeaponsRack : TileEntity
  {
    public static int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate) => TileEntity.Register<TEWeaponsRack>(x, y, type);
    public NSSLC.WorldGeneration.Item item = new NSSLC.WorldGeneration.Item();
    public void DropItem() { }
    public static dynamic Framing_CheckTile(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class TimeLogger
  {
    public static dynamic Reset(params dynamic[] arguments) => default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class UIWorldSelect
  {
    public static dynamic NewlyGeneratedWorld = default;
  }

  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>


  /// <summary>Compile-only API shape inferred from WorldGeneration call sites.</summary>
  public class Wiring
  {
    public static dynamic IgnoreWhenValidatingTraps = default;
    public static dynamic IsAMechanism = default;
    public static dynamic IsATrigger = default;
    public static dynamic running = default;
    public static dynamic ClearAll(params dynamic[] arguments) => default;
    public static dynamic HitSwitch(params dynamic[] arguments) => default;
    public static dynamic PokeLogicGate(params dynamic[] arguments) => default;
    public static dynamic SkipWire(params dynamic[] arguments) => default;
    public static dynamic Toggle2x2Light(params dynamic[] arguments) => default;
    public static dynamic ToggleCampFire(params dynamic[] arguments) => default;
    public static dynamic ToggleCandle(params dynamic[] arguments) => default;
    public static dynamic ToggleChandelier(params dynamic[] arguments) => default;
    public static dynamic ToggleFirePlace(params dynamic[] arguments) => default;
    public static dynamic ToggleHangingLantern(params dynamic[] arguments) => default;
    public static dynamic ToggleHolidayLight(params dynamic[] arguments) => default;
    public static dynamic ToggleLamp(params dynamic[] arguments) => default;
    public static dynamic ToggleLampPost(params dynamic[] arguments) => default;
    public static dynamic ToggleTorch(params dynamic[] arguments) => default;
    public static dynamic UpdateMech(params dynamic[] arguments) => default;
  }

  public class TileEntityType<T>
  {
    public static void Kill(params dynamic[] arguments) { }
    public static dynamic Get(params dynamic[] arguments) => default;
  }
}
