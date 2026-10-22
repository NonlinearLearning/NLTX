using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Modules.Creative;

public sealed class CreativePowerWireCodec : IPacket82ModuleWireCodec
{
  private const int PlayerCount = 255;
  private const int PlayerToggleByteCount = 32;

  public ushort ModuleId => (ushort)Packet82ModuleId.CreativePowers;

  public void Write(
    PacketWireWriter writer,
    object? payload,
    int tagEffectNpcSlotCount,
    Func<short, bool> tagEffectUsesProcTimes)
  {
    ArgumentNullException.ThrowIfNull(writer);
    ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
    if (payload is not CreativePowerWireMessage message)
    {
      throw new PacketWireFormatException("Creative module codec requires a typed creative-power message.");
    }

    ValidatePowerValueKind(message.PowerId, message.ValueKind);
    writer.WriteUInt16(message.PowerId);
    switch (message.ValueKind)
    {
      case CreativePowerValueKind.SharedToggle:
        writer.WriteBoolean(message.ToggleValue);
        break;
      case CreativePowerValueKind.Button:
        break;
      case CreativePowerValueKind.PerPlayerToggleSync:
        writer.WriteByte(0);
        WritePlayerToggleBits(writer, message.CopyPlayerStates());
        break;
      case CreativePowerValueKind.PerPlayerToggle:
        writer.WriteByte(1);
        writer.WriteByte(message.PlayerId);
        writer.WriteBoolean(message.ToggleValue);
        break;
      case CreativePowerValueKind.SharedSlider:
        writer.WriteSingle(message.SliderValue);
        break;
      case CreativePowerValueKind.PerPlayerSlider:
        writer.WriteByte(message.PlayerId);
        writer.WriteSingle(message.SliderValue);
        break;
      default:
        throw new PacketWireFormatException("Unknown creative-power wire value kind.");
    }
  }

  public object? Read(
    PacketWireReader reader,
    int tagEffectNpcSlotCount,
    Func<short, bool> tagEffectUsesProcTimes)
  {
    ArgumentNullException.ThrowIfNull(reader);
    ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
    ushort powerId = reader.ReadUInt16();
    if (powerId > 14)
    {
      throw new PacketWireFormatException($"Unknown Version4 creative power id {powerId}.");
    }

    return powerId switch
    {
      0 or 9 or 10 or 13 => CreativePowerWireMessage.SharedToggle(powerId, ReadBoolean(reader)),
      1 or 2 or 3 or 4 => CreativePowerWireMessage.Button(powerId),
      5 or 11 => ReadPerPlayerToggle(reader, powerId),
      6 or 7 or 8 or 12 => CreativePowerWireMessage.SharedSlider(powerId, reader.ReadSingle()),
      14 => CreativePowerWireMessage.PerPlayerSlider(powerId, reader.ReadByte(), reader.ReadSingle()),
      _ => throw new PacketWireFormatException($"Unknown Version4 creative power id {powerId}.")
    };
  }

  private static bool ReadBoolean(PacketWireReader reader)
  {
    byte value = reader.ReadByte();
    if (value > 1)
    {
      throw new PacketWireFormatException("Creative-power Boolean fields require 0 or 1.");
    }

    return value != 0;
  }

  private static CreativePowerWireMessage ReadPerPlayerToggle(
    PacketWireReader reader,
    ushort powerId)
  {
    byte action = reader.ReadByte();
    if (action == 1)
    {
      byte playerId = reader.ReadByte();
      bool value = ReadBoolean(reader);
      return CreativePowerWireMessage.PerPlayerToggle(powerId, playerId, value);
    }

    if (action != 0)
    {
      throw new PacketWireFormatException(
        $"Unknown per-player creative toggle action {action}; expected action 0 or 1.");
    }

    return CreativePowerWireMessage.PerPlayerToggleSync(powerId, ReadPlayerToggleBits(reader));
  }

  private static void WritePlayerToggleBits(PacketWireWriter writer, bool[] states)
  {
    if (states.Length != PlayerCount)
    {
      throw new PacketWireFormatException("Per-player creative toggles require exactly 255 states.");
    }

    for (int byteIndex = 0; byteIndex < PlayerToggleByteCount; byteIndex++)
    {
      byte bits = 0;
      for (int bitIndex = 0; bitIndex < 8; bitIndex++)
      {
        int playerIndex = byteIndex * 8 + bitIndex;
        if (playerIndex < states.Length && states[playerIndex])
        {
          bits |= (byte)(1 << bitIndex);
        }
      }

      writer.WriteByte(bits);
    }
  }

  private static bool[] ReadPlayerToggleBits(PacketWireReader reader)
  {
    bool[] states = new bool[PlayerCount];
    for (int byteIndex = 0; byteIndex < PlayerToggleByteCount; byteIndex++)
    {
      byte bits = reader.ReadByte();
      int firstPlayer = byteIndex * 8;
      int validBits = Math.Min(8, states.Length - firstPlayer);
      if (validBits < 8 && (bits & ~((1 << validBits) - 1)) != 0)
      {
        throw new PacketWireFormatException("Per-player creative toggle contains bits outside the 255-player table.");
      }

      for (int bitIndex = 0; bitIndex < validBits; bitIndex++)
      {
        states[firstPlayer + bitIndex] = (bits & (1 << bitIndex)) != 0;
      }
    }

    return states;
  }

  private static void ValidatePowerValueKind(ushort powerId, CreativePowerValueKind valueKind)
  {
    CreativePowerValueKind expected = powerId switch
    {
      0 or 9 or 10 or 13 => CreativePowerValueKind.SharedToggle,
      1 or 2 or 3 or 4 => CreativePowerValueKind.Button,
      5 or 11 => valueKind is CreativePowerValueKind.PerPlayerToggleSync
        or CreativePowerValueKind.PerPlayerToggle
        ? valueKind
        : (CreativePowerValueKind)byte.MaxValue,
      6 or 7 or 8 or 12 => CreativePowerValueKind.SharedSlider,
      14 => CreativePowerValueKind.PerPlayerSlider,
      _ => (CreativePowerValueKind)byte.MaxValue
    };
    if (expected != valueKind)
    {
      throw new PacketWireFormatException($"Creative power {powerId} has an incompatible wire value kind.");
    }
  }
}
