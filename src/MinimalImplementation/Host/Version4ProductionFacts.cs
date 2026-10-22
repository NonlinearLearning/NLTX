using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MinimalImplementation.Server.Protocol;
using MinimalImplementation.Server.World;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;

namespace MinimalImplementation.Server.Host;

/// <summary>
 /// Publishes the complete same-version content facts used by the production protocol boundary.
 /// </summary>
internal static class Version4ProductionFacts
{
  private const int NpcCount = 697;
  private const int TravelMerchantSlotCount = 40;
  private const int TagEffectNpcSlotCount = 200;
  private const string CatchableNpcTypeCsv =
    "46,55,74,148,149,297,298,299,300,355,356,357,358,359,360,361,362,363,364,365,366,367,374,377,442" +
    ",443,444,445,446,447,448,484,485,486,487,538,539,583,584,585,592,593,595,596,597,598,599,600,601" +
    ",602,603,604,605,606,607,608,609,610,611,612,613,614,616,617,626,627,639,640,641,642,643,644,645" +
    ",646,647,648,649,650,651,652,653,654,655,661,669,671,672,673,674,675,677,688";
  private const string NpcLifeMaxCsv =
    "0,25,60,45,2800,8,40,100,100,100,30,30,30,150,150,150,90,250,250,250,250,60,250,26,70,1,60,80,11" +
    "0,40,1,80,50,1,40,4400,600,250,250,300,300,300,48,110,70,200,5,70,100,16,2000,34,500,200,250,5,6" +
    "0,100,30,50,46,40,120,34,70,300,140,40,9999,45,100,150,100,80,5,150,150,260,130,180,200,170,160," +
    "200,200,300,400,4000,4000,4000,4000,4000,4000,100,230,200,200,200,500,500,500,320,90,120,350,250" +
    ",250,250,250,800,210,80,1,8000,8000,240,80,60,60,60,370,60,220,250,250,20000,23000,28000,7000,90" +
    "00,9000,6000,40,300,80000,80000,80000,200,180,200,260,150,250,200,240,220,220,30,5,5,30,160,220," +
    "470,400,300,600,200,750,750,250,50,350,350,80,80,450,70,70,200,210,210,600,40,220,300,220,300,25" +
    "0,200,210,70,240,200,60,70,50,40,45,45,65,60,50,60,60,250,300,280,400,400,38,55,65,60,65,1000,28" +
    "0,250,250,500,20,10,300,450,225,350,3000,40,40,40,40,30,3400,50,20,35,260,250,250,250,5,50,42,38" +
    ",42,38,400,400,350,60,60,150,150,4000,400,15000,25000,10000,10000,16000,300,1000,100,700,180,220" +
    ",140,230,220,90,300,1,30000,4000,1000,1,1250,100,340,550,400,450,400,500,350,550,500,400,450,500" +
    ",500,400,450,300,450,200,250,1000,200,400,5000,400,400,400,70,70,120,5,5,5,5,35,25,5,175,500,400" +
    ",600,650,450,500,400,600,650,450,5000,50,75,60,40,60,34,115,65,70,14000,1200,26000,5000,1800,125" +
    "0,45,45,25,23,29,22,5,600,700,500,900,750,3500,13000,34000,18000,1200,1800,1800,900,2500,450,250" +
    ",250,5,5,5,5,5,5,5,5,5,5,5,5,5,250,250,60000,1,100,100,5,5,250,5,200,210,210,350,350,300,1000,75" +
    "0,400,200,300,600,350,600,100,5000,3500,10000,45000,25000,50000,500,100,400,1200,1200,1200,300,3" +
    "00,1500,1500,800,200,700,10000,10000,10000,800,800,700,600,800,1300,330,20000,850,700,800,1000,5" +
    "00,200,700,45,50,50,40,45,45,38,400,400,32000,10000,250,5,5,5,5,5,5,5,60,55,65,60,250,10000,1000" +
    "0,10000,10000,10000,10000,700,400,270,4000,75,110,550,350,500,600,75,2000,180,3500,3500,3500,350" +
    "0,6000,200,700,400,70,110,40,5,5,5,5,1000,75,50,50,2000,20000,50,50,50,50,65,65,65,65,65,65,65,6" +
    "5,65,20000,110,90,500,500,500,60,60,60,1,20000,1000,1,2000,2000,400,500,180,250,220,300,350,350," +
    "320,320,270,220,400,50,200,50,5,5,5,5000,360,380,400,450,50,10,1000,5,250,50000,30,170,560,50,20" +
    "0,700,60,180,600,60,300,1000,800,4000,25,480,500,1400,900,3000,260,800,170,580,5000,13000,500,25" +
    "0,80,60,30,5,5,5,400,300,250,250,45,45,5,5,1,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,7000," +
    "750,5000,6000,6000,6000,25,5,5,5,50,300,180,1000,45,250,16,60,70000,250,250,5,5,5,5,5,5,5,5,5,5," +
    "5,5,5,5,5,5,5,250,18000,150,150,120,5,500,250,20,1,1,300,7000,5,250,5,5,5,5,5,80,5,250,250,250,2" +
    "50,250,250,250,250,1,5,5,5,400,60,400,70,60,250,250";
  private static readonly int[] ProjectileTypesRequiringUuid = [625, 626, 627, 628];
  private static readonly int[] CatchableNpcTypes = ParseIntTable(CatchableNpcTypeCsv);
  private static readonly int[] NpcLifeMax = ParseIntTable(NpcLifeMaxCsv);

  public static Version4ProtocolFactsInput Create(WorldFacts worldFacts)
  {
    ArgumentNullException.ThrowIfNull(worldFacts);
    if (NpcLifeMax.Length != NpcCount)
    {
      throw new InvalidOperationException("The Version4 NPC life directory is incomplete.");
    }

    return new Version4ProtocolFactsInput(
      worldFacts.CopyFrameImportantForProtocolInputs(),
      worldFacts.CopyCompressionBatchingForProtocolInputs(),
      PacketTileEntityCodecsV4.Create(),
      CreateCatchableTypeTable(),
      ResolveLifeWidth,
      NeedsUuid,
      TravelMerchantSlotCount,
      TagEffectNpcSlotCount,
      static _ => false,
      ComputeFingerprint(worldFacts));
  }

  public static byte ResolveLifeWidth(short npcType, byte? playerCount, float? difficulty)
  {
    if (npcType < 0 || npcType >= NpcLifeMax.Length)
    {
      throw new ArgumentOutOfRangeException(nameof(npcType));
    }

    if (playerCount.HasValue || difficulty.HasValue)
    {
      throw new InvalidOperationException(
        "Packet 23 dynamic scaling requires an owner-supplied effective life width.");
    }

    return ResolveLifeWidthFromEffectiveLife(NpcLifeMax[npcType]);
  }

  public static byte ResolveLifeWidthFromEffectiveLife(int effectiveLife)
  {
    if (effectiveLife < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(effectiveLife));
    }

    return effectiveLife > 32767 ? (byte)4 : effectiveLife > 127 ? (byte)2 : (byte)1;
  }

  private static bool[] CreateCatchableTypeTable()
  {
    var result = new bool[NpcCount];
    foreach (int type in CatchableNpcTypes)
    {
      if (type < 0 || type >= result.Length)
      {
        throw new InvalidOperationException($"Catchable NPC type {type} is outside the Version4 directory.");
      }

      result[type] = true;
    }

    return result;
  }

  private static bool NeedsUuid(short projectileType)
  {
    return projectileType >= 0
      && Array.BinarySearch(ProjectileTypesRequiringUuid, projectileType) >= 0;
  }

  private static int[] ParseIntTable(string csv)
  {
    return csv
      .Split(',', StringSplitOptions.RemoveEmptyEntries)
      .Select(value => int.Parse(value, CultureInfo.InvariantCulture))
      .ToArray();
  }

  private static string ComputeFingerprint(WorldFacts worldFacts)
  {
    string canonical = string.Join(
      "|",
      WorldFacts.SourceProfile,
      WorldFacts.Version4MainSha256,
      WorldFacts.Version4TileIdSha256,
      worldFacts.Fingerprint,
      CatchableNpcTypeCsv,
      NpcLifeMaxCsv,
      string.Join(",", ProjectileTypesRequiringUuid),
      TravelMerchantSlotCount.ToString(CultureInfo.InvariantCulture),
      TagEffectNpcSlotCount.ToString(CultureInfo.InvariantCulture),
      "TagEffectProcTimes:none");
    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
  }
}

