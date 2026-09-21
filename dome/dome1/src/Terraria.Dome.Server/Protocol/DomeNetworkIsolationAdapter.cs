using System;
using Terraria.Dome.Protocol.V1456.Isolation;

namespace Terraria.Dome.Server.Protocol;

internal sealed class DomeNetworkIsolationAdapter
{
  private readonly DomeNetworkIsolation _isolation;
  private readonly DomeNetworkUpdateBridge _updateBridge;

  public DomeNetworkIsolationAdapter(
    DomeNetworkIsolation isolation,
    Func<TerrariaProtocolCommand, bool> enqueueCommand)
  {
    _isolation = isolation ?? throw new ArgumentNullException(nameof(isolation));
    _updateBridge = new DomeNetworkUpdateBridge(enqueueCommand);
  }

  public int Update()
  {
    return _isolation.Update(_updateBridge);
  }
}
