using System;
using System.Collections.Generic;

using Terraria.Communication;

namespace Terraria.Network;

/// <summary>Associates one submitted intent with the receipts for that intent.</summary>
public sealed class OutboundIntentDelivery
{
  public OutboundIntentDelivery(
    OutboundPacketIntent intent,
    IReadOnlyCollection<SendReceipt> receipts)
  {
    ArgumentNullException.ThrowIfNull(intent);
    ArgumentNullException.ThrowIfNull(receipts);
    Intent = intent;
    Receipts = Array.AsReadOnly(new List<SendReceipt>(receipts).ToArray());
  }

  public OutboundPacketIntent Intent { get; }

  public IReadOnlyList<SendReceipt> Receipts { get; }
}