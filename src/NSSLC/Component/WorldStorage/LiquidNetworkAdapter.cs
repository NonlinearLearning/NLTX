using System;

namespace Terraria.WorldStorage;

public sealed class LiquidNetworkAdapter
{
  private readonly ILiquidPublicationPort _publicationPort;

  public LiquidNetworkAdapter(ILiquidPublicationPort publicationPort)
  {
    ArgumentNullException.ThrowIfNull(publicationPort);
    _publicationPort = publicationPort;
  }

  public LiquidNetworkPublishResult Publish(
    LiquidChangePublicationProjection projection,
    bool publicationEnabled)
  {
    ArgumentNullException.ThrowIfNull(projection);
    LiquidPublicationProjectionResult projectionResult =
      projection.BeginPublication(publicationEnabled);
    if (projectionResult.Status != LiquidPublicationProjectionStatus.Ready)
    {
      return LiquidNetworkPublishResult.FromProjection(projectionResult);
    }

    LiquidChangePublicationSnapshot snapshot = projectionResult.Snapshot!;
    LiquidPublicationDeliveryResult delivery;
    try
    {
      delivery = _publicationPort.Publish(snapshot);
    }
    catch (OperationCanceledException exception)
    {
      return new LiquidNetworkPublishResult(
        LiquidNetworkPublishStatus.Cancelled,
        snapshot,
        exception.GetType().Name);
    }
    catch (Exception exception)
    {
      return new LiquidNetworkPublishResult(
        LiquidNetworkPublishStatus.Failed,
        snapshot,
        exception.GetType().Name);
    }

    if (!delivery.Succeeded)
    {
      return new LiquidNetworkPublishResult(
        delivery.Status switch
        {
          LiquidPublicationDeliveryStatus.Unknown => LiquidNetworkPublishStatus.Unknown,
          LiquidPublicationDeliveryStatus.Cancelled => LiquidNetworkPublishStatus.Cancelled,
          _ => LiquidNetworkPublishStatus.Failed
        },
        snapshot,
        delivery.FailureReason);
    }

    LiquidPublicationProjectionResult acknowledgement =
      projection.AcknowledgePublication(snapshot.Revision);
    if (acknowledgement.Status != LiquidPublicationProjectionStatus.Acknowledged)
    {
      return new LiquidNetworkPublishResult(
        LiquidNetworkPublishStatus.Rejected,
        snapshot,
        acknowledgement.FailureReason ?? "acknowledgement-rejected");
    }

    return new LiquidNetworkPublishResult(
      LiquidNetworkPublishStatus.Delivered,
      snapshot,
      null);
  }
}
