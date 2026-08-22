using System;
using System.IO;
using Terraria.WorldFile.V319.Format;

namespace Terraria.WorldFile.V319.Verification.Fixtures;

internal static class WldVersionMatrixFixtureWriter
{
  private const ulong FileMetadata = 0x026369676F6C6572UL;
  private const int Height = 2;
  private const int MoonPhase = 6;
  private const string WorldName = "Matrix fixture";
  private const int Width = 2;

  public static MemoryStream CreateMinimalPointerWorld(
    int version,
    int moonPhase = MoonPhase,
    bool isBloodMoon = false,
    bool isEclipse = false,
    bool isCrimsonWorld = false,
    bool isHardMode = false,
    bool defeatedEyeOfCthulhu = false,
    bool defeatedEaterOrBrain = false,
    bool defeatedSkeletron = false,
    bool defeatedMechanicalBoss = false,
    bool defeatedPlantera = false,
    bool defeatedGolem = false,
    int invasionType = 0,
    int invasionSize = 0,
    int gameMode = 0,
    bool isMeteorScheduled = false,
    float windSpeedTarget = 0.0f,
    bool isRaining = false,
    int rainTimeTicks = 0,
    float maximumRainStrength = 0.0f,
    bool isRemixWorld = false,
    ulong worldGeneratorVersion = 0,
    Guid? uniqueId = null,
    string? seedText = null)
  {
    if (version < 88 || version > 319)
    {
      throw new ArgumentOutOfRangeException(nameof(version));
    }

    ExpectedVersionLayout layout = ExpectedVersionLayouts.Get(version);
    int sectionCount = layout.SectionCount;
    MemoryStream stream = new();
    using (BinaryWriter writer = new(stream, System.Text.Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(version);
      if (version >= 135)
      {
        writer.Write(FileMetadata);
        writer.Write(0U);
        writer.Write(0UL);
      }

      writer.Write((short)sectionCount);
      long offsetsPosition = stream.Position;
      for (int index = 0; index < sectionCount; index++)
      {
        writer.Write(0);
      }

      writer.Write((ushort)0);
      int[] offsets = new int[sectionCount];
      for (int sectionIndex = 0; sectionIndex < sectionCount; sectionIndex++)
      {
        offsets[sectionIndex] = checked((int)stream.Position);
        WriteSection(
          writer,
          version,
          sectionIndex,
          moonPhase,
          isBloodMoon,
          isEclipse,
          isCrimsonWorld,
          isHardMode,
          defeatedEyeOfCthulhu,
          defeatedEaterOrBrain,
          defeatedSkeletron,
          defeatedMechanicalBoss,
          defeatedPlantera,
          defeatedGolem,
          invasionType,
          invasionSize,
          gameMode,
          isMeteorScheduled,
          windSpeedTarget,
          isRaining,
          rainTimeTicks,
          maximumRainStrength,
          isRemixWorld,
          worldGeneratorVersion,
          uniqueId,
          seedText);
      }

      stream.Position = offsetsPosition;
      for (int index = 0; index < offsets.Length; index++)
      {
        writer.Write(offsets[index]);
      }
    }

    stream.Position = 0;
    return stream;
  }

  public static MemoryStream CreateVersionHeader(int version)
  {
    MemoryStream stream = new();
    using BinaryWriter writer = new(stream, System.Text.Encoding.UTF8, leaveOpen: true);
    writer.Write(version);
    stream.Position = 0;
    return stream;
  }

  private static void WriteHeader(
    BinaryWriter writer,
    int version,
    int moonPhase,
    bool isBloodMoon,
    bool isEclipse,
    bool isCrimsonWorld,
    bool isHardMode,
    bool defeatedEyeOfCthulhu,
    bool defeatedEaterOrBrain,
    bool defeatedSkeletron,
    bool defeatedMechanicalBoss,
    bool defeatedPlantera,
    bool defeatedGolem,
    int invasionType,
    int invasionSize,
    int gameMode,
    bool isMeteorScheduled,
    float windSpeedTarget,
    bool isRaining,
    int rainTimeTicks,
    float maximumRainStrength,
    bool isRemixWorld,
    ulong worldGeneratorVersion,
    Guid? uniqueId,
    string? seedText)
  {
    writer.Write(WorldName);
    if (version >= 179)
    {
      if (version == 179)
      {
        writer.Write(seedText is null ? 0 : int.Parse(seedText, System.Globalization.CultureInfo.InvariantCulture));
      }
      else
      {
        writer.Write(seedText ?? string.Empty);
      }

      writer.Write(worldGeneratorVersion);
    }

    if (version >= 181)
    {
      writer.Write((uniqueId ?? Guid.Empty).ToByteArray());
    }

    writer.Write(version);
    writer.Write(0);
    writer.Write(Width * 16);
    writer.Write(0);
    writer.Write(Height * 16);
    writer.Write(Height);
    writer.Write(Width);
    if (version >= 209)
    {
      writer.Write(gameMode);
      if (version >= 222)
      {
        WriteWorldVariantBooleans(writer, version, isRemixWorld);
      }
    }
    else if (version >= 112)
    {
      writer.Write(gameMode != 0);
      if (version == 208)
      {
        writer.Write(gameMode == 2);
      }
    }

    if (version >= 141)
    {
      writer.Write(0L);
    }

    if (version >= 284)
    {
      writer.Write(0L);
    }

    writer.Write((byte)0);
    WriteInt32Values(writer, 17);
    writer.Write(1);
    writer.Write(1);
    WriteDoubleValues(writer, 3);
    writer.Write(false);
    writer.Write(moonPhase);
    writer.Write(isBloodMoon);
    writer.Write(isEclipse);
    WriteInt32Values(writer, 2);
    writer.Write(isCrimsonWorld);
    writer.Write(defeatedEyeOfCthulhu);
    writer.Write(defeatedEaterOrBrain);
    writer.Write(defeatedSkeletron);
    writer.Write(false);
    WriteBooleans(writer, 3);
    writer.Write(defeatedMechanicalBoss);
    writer.Write(defeatedPlantera);
    writer.Write(defeatedGolem);
    if (version >= 118)
    {
      writer.Write(false);
    }

    WriteBooleans(writer, 7);
    writer.Write(false);
    writer.Write(isMeteorScheduled);
    writer.Write((byte)0);
    writer.Write(0);
    writer.Write(isHardMode);
    if (version >= 257)
    {
      writer.Write(false);
    }

    writer.Write(0);
    writer.Write(invasionSize);
    writer.Write(invasionType);
    writer.Write(0.0d);
    if (version >= 118)
    {
      writer.Write(0.0d);
    }

    if (version >= 113)
    {
      writer.Write((byte)0);
    }

    writer.Write(isRaining);
    writer.Write(rainTimeTicks);
    writer.Write(maximumRainStrength);
    WriteInt32Values(writer, 3);
    WriteByteValues(writer, 8);
    writer.Write(0);
    writer.Write((short)0);
    writer.Write(windSpeedTarget);
    WriteLaterHeaderFields(writer, version);
  }

  private static void WriteLaterHeaderFields(BinaryWriter writer, int version)
  {
    if (version < 95)
    {
      return;
    }

    writer.Write(0);
    if (version < 99)
    {
      return;
    }

    writer.Write(false);
    if (version < 101)
    {
      return;
    }

    writer.Write(0);
    if (version < 104)
    {
      return;
    }

    writer.Write(false);
    if (version >= 129)
    {
      writer.Write(false);
    }

    if (version >= 201)
    {
      writer.Write(false);
    }

    if (version >= 107)
    {
      writer.Write(0);
    }

    if (version >= 108)
    {
      writer.Write(0);
    }

    if (version < 109)
    {
      return;
    }

    writer.Write((short)0);
    if (version >= 289)
    {
      writer.Write((short)0);
    }

    if (version < 128)
    {
      return;
    }

    writer.Write(false);
    if (version < 131)
    {
      return;
    }

    WriteBooleans(writer, 9);
    if (version < 140)
    {
      return;
    }

    WriteBooleans(writer, 9);
    if (version >= 170)
    {
      WriteBooleans(writer, 2);
      writer.Write(0);
      writer.Write(0);
    }

    if (version >= 174)
    {
      writer.Write(false);
      writer.Write(0);
      writer.Write(0.0f);
      writer.Write(0.0f);
    }

    if (version >= 178)
    {
      WriteBooleans(writer, 4);
    }

    if (version > 194)
    {
      writer.Write((byte)0);
    }

    if (version >= 215)
    {
      writer.Write((byte)0);
    }

    if (version > 195)
    {
      WriteByteValues(writer, 3);
    }

    if (version >= 204)
    {
      writer.Write(false);
    }

    if (version >= 207)
    {
      writer.Write(0);
      WriteBooleans(writer, 3);
    }

    if (version >= 211)
    {
      writer.Write(0);
    }

    if (version >= 212)
    {
      WriteBooleans(writer, 2);
    }

    if (version >= 216)
    {
      WriteInt32Values(writer, 4);
    }

    if (version >= 217)
    {
      WriteBooleans(writer, 3);
    }

    if (version >= 223)
    {
      WriteBooleans(writer, 2);
    }

    if (version >= 240)
    {
      writer.Write(false);
    }

    if (version >= 250)
    {
      writer.Write(false);
    }

    if (version >= 251)
    {
      WriteBooleans(writer, 8);
    }

    if (version >= 259)
    {
      writer.Write(false);
    }

    if (version >= 260)
    {
      writer.Write(false);
    }

    if (version >= 261)
    {
      WriteBooleans(writer, 7);
    }

    if (version >= 264)
    {
      writer.Write(false);
      writer.Write((byte)0);
    }

    if (version >= 287)
    {
      WriteBooleans(writer, 2);
    }

    if (version >= 288)
    {
      writer.Write(false);
    }

    if (version >= 296)
    {
      writer.Write(false);
    }

    if (version >= 291)
    {
      WriteInt32Values(writer, 2);
    }

    if (version >= 297)
    {
      writer.Write(false);
      writer.Write((byte)0);
    }

    if (version >= 304)
    {
      writer.Write(false);
    }

    if (version is >= 299 and < 313)
    {
      writer.Write(0U);
    }

    if (version >= 299)
    {
      writer.Write(string.Empty);
    }
  }

  private static void WriteSection(
    BinaryWriter writer,
    int version,
    int sectionIndex,
    int moonPhase,
    bool isBloodMoon,
    bool isEclipse,
    bool isCrimsonWorld,
    bool isHardMode,
    bool defeatedEyeOfCthulhu,
    bool defeatedEaterOrBrain,
    bool defeatedSkeletron,
    bool defeatedMechanicalBoss,
    bool defeatedPlantera,
    bool defeatedGolem,
    int invasionType,
    int invasionSize,
    int gameMode,
    bool isMeteorScheduled,
    float windSpeedTarget,
    bool isRaining,
    int rainTimeTicks,
    float maximumRainStrength,
    bool isRemixWorld,
    ulong worldGeneratorVersion,
    Guid? uniqueId,
    string? seedText)
  {
    switch (sectionIndex)
    {
      case 0:
        WriteHeader(
          writer,
          version,
          moonPhase,
          isBloodMoon,
          isEclipse,
          isCrimsonWorld,
          isHardMode,
          defeatedEyeOfCthulhu,
          defeatedEaterOrBrain,
          defeatedSkeletron,
          defeatedMechanicalBoss,
          defeatedPlantera,
          defeatedGolem,
          invasionType,
          invasionSize,
          gameMode,
          isMeteorScheduled,
          windSpeedTarget,
          isRaining,
          rainTimeTicks,
          maximumRainStrength,
          isRemixWorld,
          worldGeneratorVersion,
          uniqueId,
          seedText);
        return;
      case 1:
        WriteDefaultTileSection(writer);
        return;
      case 2:
        writer.Write((short)0);
        if (version < 294)
        {
          writer.Write((short)0);
        }

        return;
      case 3:
        writer.Write((short)0);
        return;
      case 4:
        if (version >= 268)
        {
          writer.Write(0);
        }

        writer.Write(false);
        if (version >= 140)
        {
          writer.Write(false);
        }

        return;
    }

    int tailIndex = sectionIndex - 5;
    if (version >= 116)
    {
      if (tailIndex == 0)
      {
        writer.Write(0);
        return;
      }

      tailIndex--;
    }

    if (version >= 170)
    {
      if (tailIndex == 0)
      {
        writer.Write(0);
        return;
      }

      tailIndex--;
    }

    if (version >= 189)
    {
      if (tailIndex == 0)
      {
        writer.Write(0);
        return;
      }

      tailIndex--;
    }

    if (version >= 210)
    {
      if (tailIndex == 0)
      {
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        return;
      }

      tailIndex--;
    }

    if (version >= 220 && tailIndex == 0)
    {
      writer.Write((byte)0);
      return;
    }

    writer.Write(true);
    writer.Write(WorldName);
    writer.Write(version);
  }

  private static void WriteWorldVariantBooleans(
    BinaryWriter writer,
    int version,
    bool isRemixWorld)
  {
    int count = version >= 302 ? 9 : 8;
    for (int index = 0; index < count; index++)
    {
      writer.Write(version >= 249 && index == 5 && isRemixWorld);
    }
  }

  private static void WriteBooleans(BinaryWriter writer, int count)
  {
    for (int index = 0; index < count; index++)
    {
      writer.Write(false);
    }
  }

  private static void WriteByteValues(BinaryWriter writer, int count)
  {
    for (int index = 0; index < count; index++)
    {
      writer.Write((byte)0);
    }
  }

  private static void WriteDefaultTileSection(BinaryWriter writer)
  {
    writer.Write((byte)0x40);
    writer.Write((byte)1);
    writer.Write((byte)0x40);
    writer.Write((byte)1);
  }

  private static void WriteDoubleValues(BinaryWriter writer, int count)
  {
    for (int index = 0; index < count; index++)
    {
      writer.Write(0.0d);
    }
  }

  private static void WriteInt32Values(BinaryWriter writer, int count)
  {
    for (int index = 0; index < count; index++)
    {
      writer.Write(0);
    }
  }
}
