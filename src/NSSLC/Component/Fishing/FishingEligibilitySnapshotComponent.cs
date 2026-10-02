using Terraria.Player;

namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-ELIGIBILITY-SNAPSHOT
// designStatus: candidate
// crossSubsystemOwner: integration-review
public struct FishingEligibilitySnapshotComponent
{
  public FishingEligibilitySnapshotComponent(
    TileCoordinate pondCoordinate,
    int bobberTypeId)
  {
    PondCoordinate = pondCoordinate;
    BobberTypeId = bobberTypeId;
    LiquidKind = FishingLiquidKind.Unknown;
    WaterTilesCount = 0;
    WaterNeededToFish = 0;
    WaterQuality = 0;
    ChumsInWater = 0;
    PolePower = 0;
    PoleItemType = 0;
    BaitPower = 0;
    BaitItemType = 0;
    FishingLevel = 0;
    FinalFishingLevel = 0;
    CanFishInLava = false;
    HeightLevel = 0;
    BiomeFlags = default;
    QuestFishType = null;
    RarityFlags = default;
    IsJunkCandidate = false;
    RulesetRevision = null;
  }

  // Current NLTX value-object candidate; coordinate ownership remains under review.
  public TileCoordinate PondCoordinate;

  // Candidate content/type value; replace with the accepted typed content ID if required.
  public int BobberTypeId;

  // Does not own Liquid Tile state.
  public FishingLiquidKind LiquidKind;

  public int WaterTilesCount;
  public int WaterNeededToFish;
  public float WaterQuality;
  public int ChumsInWater;

  // Snapshots of Player/Item inputs, not Player or Item ownership fields.
  public int PolePower;
  public int PoleItemType;
  public int BaitPower;
  public int BaitItemType;
  public int FishingLevel;
  public int FinalFishingLevel;
  public bool CanFishInLava;
  public int HeightLevel;

  // Opaque cross-domain snapshots; their bit layout is not confirmed.
  public FishingBiomeFlags BiomeFlags;
  public int? QuestFishType;
  public FishingRarityFlags RarityFlags;

  public bool IsJunkCandidate;

  // Current NLTX has no runtime ruleset revision.
  public uint? RulesetRevision;

  public bool HasEnoughWater =>
    WaterTilesCount >= 0 &&
    WaterNeededToFish >= 0 &&
    WaterTilesCount >= WaterNeededToFish;
}
