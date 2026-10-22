using Terraria.NetWork.Prototype.PacketDesignCompiler.Sample;
using Terraria.NetWork.Prototype.PacketDesignCompiler.Wire;

namespace MinimalImplementation.Server.Inventory.Protocol;

/// <summary>
/// Compatibility codec for inventory fixtures. Production uses Version4ModuleWireCodec.
/// Do not register this codec alongside the production module 11 codec.
/// </summary>
public sealed class CraftingModule11WireCodec : IPacket82ModuleWireCodec
{
  private readonly bool _isServer;

  public CraftingModule11WireCodec(bool isServer = true)
  {
    _isServer = isServer;
  }

  public ushort ModuleId => (ushort)Packet82ModuleId.CraftingRequests;

  public void Write(
    PacketWireWriter writer,
    object? payload,
    int tagEffectNpcSlotCount,
    Func<short, bool> tagEffectUsesProcTimes)
  {
    ArgumentNullException.ThrowIfNull(writer);
    ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
    if (tagEffectNpcSlotCount is < 0 or > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(tagEffectNpcSlotCount));
    }

    if (_isServer && payload is CraftingModule11Response response)
    {
      WriteResponse(writer, response);
      return;
    }

    if (!_isServer && payload is CraftingModule11Request request)
    {
      WriteRequest(writer, request);
      return;
    }

    throw new ArgumentException(
      "The module 11 payload does not match the configured wire direction.",
      nameof(payload));
  }

  public object Read(
    PacketWireReader reader,
    int tagEffectNpcSlotCount,
    Func<short, bool> tagEffectUsesProcTimes)
  {
    ArgumentNullException.ThrowIfNull(reader);
    ArgumentNullException.ThrowIfNull(tagEffectUsesProcTimes);
    if (tagEffectNpcSlotCount is < 0 or > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(tagEffectNpcSlotCount));
    }

    if (!_isServer)
    {
      return ReadResponse(reader);
    }

    int materialCount = Read7BitEncodedInt(reader);
    // Each entry is Int32 + at least one stack byte; reserve one chest-count byte.
    if (materialCount < 0 || reader.Remaining < 1
      || materialCount > (reader.Remaining - 1) / 5)
    {
      throw new PacketWireFormatException(
        "The crafting material count is outside the remaining module payload.",
        PacketReadErrorCode.LengthOutOfRange);
    }

    var materials = new CraftingMaterialRequirement[materialCount];
    for (int index = 0; index < materialCount; index++)
    {
      int itemIdOrRecipeGroup = reader.ReadInt32();
      int stack = Read7BitEncodedInt(reader);
      if (stack <= 0)
      {
        throw new PacketWireFormatException(
          "Crafting material stacks must be positive.");
      }

      materials[index] = new CraftingMaterialRequirement(itemIdOrRecipeGroup, stack);
    }

    int chestCount = Read7BitEncodedInt(reader);
    if (chestCount < 0 || chestCount > reader.Remaining)
    {
      throw new PacketWireFormatException(
        "The crafting chest count is outside the remaining module payload.",
        PacketReadErrorCode.LengthOutOfRange);
    }

    var chestIds = new int[chestCount];
    for (int index = 0; index < chestCount; index++)
    {
      chestIds[index] = Read7BitEncodedInt(reader);
    }

    reader.RequireFrameEnd();
    return new CraftingModule11Request(materials, chestIds);
  }

  public static CraftingModule11Response ReadResponse(PacketWireReader reader)
  {
    ArgumentNullException.ThrowIfNull(reader);
    var response = new CraftingModule11Response(reader.ReadBoolean());
    reader.RequireFrameEnd();
    return response;
  }

  public static void WriteRequest(PacketWireWriter writer, CraftingModule11Request request)
  {
    ArgumentNullException.ThrowIfNull(writer);
    ArgumentNullException.ThrowIfNull(request);
    foreach (CraftingMaterialRequirement material in request.Materials)
    {
      if (material.Stack <= 0)
      {
        throw new ArgumentOutOfRangeException(nameof(request), "Crafting material stacks must be positive.");
      }
    }

    Write7BitEncodedInt(writer, request.Materials.Count);
    foreach (CraftingMaterialRequirement material in request.Materials)
    {
      writer.WriteInt32(material.ItemIdOrRecipeGroup);
      Write7BitEncodedInt(writer, material.Stack);
    }

    Write7BitEncodedInt(writer, request.ChestIds.Count);
    foreach (int chestId in request.ChestIds)
    {
      Write7BitEncodedInt(writer, chestId);
    }
  }

  public static void WriteResponse(PacketWireWriter writer, CraftingModule11Response response)
  {
    ArgumentNullException.ThrowIfNull(writer);
    ArgumentNullException.ThrowIfNull(response);
    writer.WriteBoolean(response.Approved);
  }

  private static int Read7BitEncodedInt(PacketWireReader reader)
  {
    uint value = 0;
    for (int shift = 0; shift < 35; shift += 7)
    {
      byte current = reader.ReadByte();
      if (shift == 28 && (current & 0xF0) != 0)
      {
        throw new PacketWireFormatException(
          "A crafting 7-bit integer exceeds Int32.",
          PacketReadErrorCode.InvalidLengthPrefix);
      }

      value |= (uint)(current & 0x7F) << shift;
      if ((current & 0x80) == 0)
      {
        return unchecked((int)value);
      }
    }

    throw new PacketWireFormatException(
      "A crafting 7-bit integer is longer than five bytes.",
      PacketReadErrorCode.InvalidLengthPrefix);
  }

  private static void Write7BitEncodedInt(PacketWireWriter writer, int value)
  {
    uint remaining = unchecked((uint)value);
    while (remaining >= 0x80)
    {
      writer.WriteByte((byte)(remaining | 0x80));
      remaining >>= 7;
    }

    writer.WriteByte((byte)remaining);
  }
}
