using System;

namespace Terraria.Dome.Protocol.V1456.Protocol;

public readonly record struct TerrariaFrame(
  TerrariaMessageId MessageId,
  ReadOnlyMemory<byte> Payload);
