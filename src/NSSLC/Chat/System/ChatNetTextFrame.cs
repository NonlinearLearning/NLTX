using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace Terraria.Chat;

/// <summary>Framing of the chat line inside the reference's text net module.</summary>
/// <remarks>
/// Chat is not its own message id: Version4 sends it as <c>MessageID.NetModules</c> (82) carrying
/// <c>NetTextModule</c>, whose id is 1 because <c>NetworkInitializer</c> registers
/// <c>NetLiquidModule</c> (0) before <c>NetTextModule</c> (1).
/// <para>
/// The server-to-client frame is fully grounded in <c>NetTextModule.SerializeServerMessage</c>:
/// module id, author byte, a <c>NetworkText</c> in literal mode (a 7-bit-length-prefixed UTF-8
/// string) and the color as three bytes (<c>WriteRGB</c>).
/// </para>
/// <para>
/// The client-to-server frame carries the text command name next to the typed text instead of an
/// author: this repository's earlier protocol layer decoded the pair as <c>CommandName</c> then
/// <c>Text</c>, which is the only available precedent because <c>NetTextModule.Deserialize</c> is
/// stripped from the reference dump. Both strings use the same 7-bit-length-prefixed UTF-8 layout.
/// </para>
/// </remarks>
public static class ChatNetTextFrame
{
  /// <summary><c>MessageID.NetModules</c>.</summary>
  public const byte NetModulesMessageId = 82;

  /// <summary><c>NetManager</c> id of <c>NetTextModule</c>.</summary>
  public const ushort TextModuleId = 1;

  /// <summary>Author byte used when the speaker is not one particular player.</summary>
  public const byte ServerAuthorId = byte.MaxValue;

  private const byte LiteralMode = 0;

  /// <summary>Encodes one server-to-client chat line, as the reference does.</summary>
  public static byte[] EncodeServerMessage(byte authorId, string text, ChatColor color)
  {
    ArgumentNullException.ThrowIfNull(text);
    using MemoryStream stream = new(text.Length + 8);
    using (BinaryWriter writer = new(stream, Encoding.UTF8, leaveOpen: true))
    {
      writer.Write(TextModuleId);
      writer.Write(authorId);
      writer.Write(LiteralMode);
      writer.Write(text);
      writer.Write(color.R);
      writer.Write(color.G);
      writer.Write(color.B);
    }

    return stream.ToArray();
  }

  /// <summary>Decodes a client chat frame into its text command name and typed text.</summary>
  public static bool TryDecodeClientMessage(
      ReadOnlySpan<byte> payload,
      out string commandName,
      out string text)
  {
    commandName = string.Empty;
    text = string.Empty;
    if (payload.Length < 3 ||
        BinaryPrimitives.ReadUInt16LittleEndian(payload) != TextModuleId)
    {
      return false;
    }

    int offset = 2;
    return TryReadString(payload, ref offset, out commandName) &&
        TryReadString(payload, ref offset, out text) &&
        commandName.Length > 0 &&
        text.Length > 0;
  }

  private static bool TryReadString(ReadOnlySpan<byte> payload, ref int offset, out string text)
  {
    text = string.Empty;
    int byteCount = 0;
    int shift = 0;
    while (true)
    {
      if (offset >= payload.Length || shift > 28)
      {
        return false;
      }

      byte value = payload[offset++];
      byteCount |= (value & 0x7F) << shift;
      if ((value & 0x80) == 0)
      {
        break;
      }

      shift += 7;
    }

    if (byteCount <= 0 || byteCount > payload.Length - offset)
    {
      return false;
    }

    text = Encoding.UTF8.GetString(payload.Slice(offset, byteCount));
    offset += byteCount;
    return true;
  }

  private static bool TryReadString(ReadOnlySpan<byte> payload, out string text)
  {
    text = string.Empty;
    int offset = 0;
    int byteCount = 0;
    int shift = 0;
    while (true)
    {
      if (offset >= payload.Length || shift > 28)
      {
        return false;
      }

      byte value = payload[offset++];
      byteCount |= (value & 0x7F) << shift;
      if ((value & 0x80) == 0)
      {
        break;
      }

      shift += 7;
    }

    if (byteCount <= 0 || byteCount > payload.Length - offset)
    {
      return false;
    }

    text = Encoding.UTF8.GetString(payload.Slice(offset, byteCount));
    return true;
  }
}
