using System;
using System.Collections.Generic;

namespace Terraria.Dome.Server.Replication;

public sealed class PlayerReplicationCursor
{
  private readonly Dictionary<byte, PlayerReplicationState> _sentStates = new();

  public void Clear()
  {
    _sentStates.Clear();
  }

  public IReadOnlyList<PlayerReplicationState> CollectChanged(
    IReadOnlyList<PlayerReplicationState> states)
  {
    ArgumentNullException.ThrowIfNull(states);

    List<PlayerReplicationState> changed = new();
    for (int index = 0; index < states.Count; index++)
    {
      PlayerReplicationState state = states[index];
      if (_sentStates.TryGetValue(state.PlayerSlot, out PlayerReplicationState sent) &&
          sent == state)
      {
        continue;
      }

      _sentStates[state.PlayerSlot] = state;
      changed.Add(state);
    }

    return changed;
  }
}
