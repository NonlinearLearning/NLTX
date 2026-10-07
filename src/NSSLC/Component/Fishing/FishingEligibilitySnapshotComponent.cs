using Terraria.Player;

namespace Terraria.Fishing;

// status: proposed
// componentId: FISHING-ELIGIBILITY-SNAPSHOT
// designStatus: candidate
// crossSubsystemOwner: integration-review
/// <summary>
/// 保存水域、鱼竿、鱼饵、环境和稀有度的钓鱼判定快照。
/// </summary>
/// <remarks>
/// <para>拆分来源：Terraria.DataStructures.FishingAttempt。</para>
/// <para>原始文件：D:/TRbackup/Version4/Terraria.DataStructures/FishingAttempt.cs。</para>
/// <para>
/// 主要源成员：waterTilesCount（第 31 行）； waterNeededToFish（第 33 行）； waterQuality（第 35 行）； chumsInWater（第
/// 37 行）； fishingLevel（第 39 行）； CanFishInLava（第 41 行）； heightLevel（第 47 行）。
/// </para>
/// <para>重组说明：原钓鱼判定数据按一次尝试形成快照，规则版本是新增的快照前提。</para>
/// <para>拆分依据目录：docs/component-decomposition/review-round-1/design/。</para>
/// <para>
/// 拆分依据文件：2026-09-05-version4-fishing-and-catch-simulation-component-code-draft.md。
/// </para>
/// <para>依据位置：第 295 行。</para>
/// </remarks>
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
