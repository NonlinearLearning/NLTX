using System;
using System.IO;

namespace MinimalImplementation.Persistence;

/// <summary>Reads and writes the bounded WorldFile CreativePowers typed record sequence.</summary>
public sealed class CreativeWorldPowerDocumentCodec
{
  private const int MaximumPayloadBytes = 64 * 1024 * 1024;
  private const int FreezeTimePowerId = 0;
  private const int ModifyTimeRatePowerId = 8;
  private const int FreezeRainPowerId = 9;
  private const int FreezeWindPowerId = 10;
  private const int DifficultySliderPowerId = 12;
  private const int StopBiomeSpreadPowerId = 13;

  public CreativeWorldPowerSnapshot Decode(ReadOnlyMemory<byte> payload)
  {
    if (payload.IsEmpty || payload.Length > MaximumPayloadBytes)
    {
      throw new InvalidDataException("The CreativePowers payload is empty or exceeds its size limit.");
    }

    var values = CreativeWorldPowerSnapshot.Defaults;
    var seen = new bool[14];
    using var stream = new MemoryStream(payload.ToArray(), writable: false);
    using var reader = new BinaryReader(stream);
    try
    {
      int records = 0;
      while (ReadStrictBoolean(reader))
      {
        if (++records > 6)
        {
          throw new InvalidDataException("The CreativePowers payload contains too many records.");
        }

        ushort powerId = reader.ReadUInt16();
        if (powerId >= seen.Length || !IsPersistentPower(powerId) || seen[powerId])
        {
          throw new InvalidDataException(
            $"The CreativePowers payload contains an unknown or duplicate world power {powerId}.");
        }

        seen[powerId] = true;
        values = powerId switch
        {
          FreezeTimePowerId => values with { FreezeTime = ReadStrictBoolean(reader) },
          ModifyTimeRatePowerId => values with { ModifyTimeRate = reader.ReadSingle() },
          FreezeRainPowerId => values with { FreezeRain = ReadStrictBoolean(reader) },
          FreezeWindPowerId => values with { FreezeWind = ReadStrictBoolean(reader) },
          DifficultySliderPowerId => values with { DifficultySlider = reader.ReadSingle() },
          StopBiomeSpreadPowerId => values with { StopBiomeSpread = ReadStrictBoolean(reader) },
          _ => throw new InvalidDataException("The CreativePowers record is not supported.")
        };
      }

      if (records != 0 && records != 6)
      {
        throw new InvalidDataException(
          "A typed CreativePowers payload must contain all six persistent world powers.");
      }

      if (stream.Position != stream.Length)
      {
        throw new InvalidDataException("The CreativePowers payload has trailing bytes.");
      }
    }
    catch (EndOfStreamException exception)
    {
      throw new InvalidDataException("The CreativePowers payload is truncated.", exception);
    }

    try
    {
      values.Validate();
    }
    catch (ArgumentOutOfRangeException exception)
    {
      throw new InvalidDataException(exception.Message, exception);
    }

    return values;
  }

  public byte[] Encode(CreativeWorldPowerSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    snapshot.Validate();
    using var stream = new MemoryStream(32);
    using var writer = new BinaryWriter(stream);
    WriteBooleanRecord(writer, FreezeTimePowerId, snapshot.FreezeTime);
    WriteSingleRecord(writer, ModifyTimeRatePowerId, snapshot.ModifyTimeRate);
    WriteBooleanRecord(writer, FreezeRainPowerId, snapshot.FreezeRain);
    WriteBooleanRecord(writer, FreezeWindPowerId, snapshot.FreezeWind);
    WriteSingleRecord(writer, DifficultySliderPowerId, snapshot.DifficultySlider);
    WriteBooleanRecord(writer, StopBiomeSpreadPowerId, snapshot.StopBiomeSpread);
    writer.Write(false);
    return stream.ToArray();
  }

  private static bool IsPersistentPower(ushort powerId) => powerId is
    FreezeTimePowerId or ModifyTimeRatePowerId or FreezeRainPowerId or FreezeWindPowerId or
    DifficultySliderPowerId or StopBiomeSpreadPowerId;

  private static bool ReadStrictBoolean(BinaryReader reader)
  {
    byte value = reader.ReadByte();
    if (value > 1)
    {
      throw new InvalidDataException("CreativePowers Boolean fields require byte 0 or 1.");
    }

    return value != 0;
  }

  private static void WriteBooleanRecord(BinaryWriter writer, ushort powerId, bool value)
  {
    writer.Write(true);
    writer.Write(powerId);
    writer.Write(value);
  }

  private static void WriteSingleRecord(BinaryWriter writer, ushort powerId, float value)
  {
    writer.Write(true);
    writer.Write(powerId);
    writer.Write(value);
  }
}
