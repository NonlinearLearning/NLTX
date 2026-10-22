using System;
using System.Collections.Generic;
using System.Linq;
using MinimalImplementation.Contracts;
using MinimalImplementation.Server.Identity;

namespace MinimalImplementation.Server.Session;

/// <summary>
/// Owns only the protocol-slot projection for authenticated sessions on the server loop.
/// </summary>
public sealed class SessionBindingStore
{
  public const int MaximumPlayerSlots = byte.MaxValue;

  private readonly Dictionary<SessionEpoch, SessionBinding> _byEpoch = [];
  private readonly SessionBinding?[] _bySlot = new SessionBinding[MaximumPlayerSlots];

  public int Count => _byEpoch.Count;

  public bool TryGetByEpoch(SessionEpoch epoch, out SessionBinding? binding)
  {
    return _byEpoch.TryGetValue(epoch, out binding);
  }

  public bool TryGetBySlot(byte playerSlot, out SessionBinding? binding)
  {
    if (playerSlot >= MaximumPlayerSlots)
    {
      binding = null;
      return false;
    }

    binding = _bySlot[playerSlot];
    return binding is not null;
  }

  public bool TryFindAvailableSlot(int maximumSlots, out byte playerSlot)
  {
    ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumSlots);
    if (maximumSlots > MaximumPlayerSlots)
    {
      throw new ArgumentOutOfRangeException(nameof(maximumSlots));
    }

    for (int slot = 0; slot < maximumSlots; slot++)
    {
      if (_bySlot[slot] is null)
      {
        playerSlot = (byte)slot;
        return true;
      }
    }

    playerSlot = default;
    return false;
  }

  public bool TryBind(SessionEpoch epoch, byte playerSlot, EntityHandle playerHandle)
  {
    if (!epoch.IsValid || !playerHandle.Id.IsValid || !playerHandle.WorldScope.IsValid
      || !playerHandle.Generation.IsValid || playerSlot >= MaximumPlayerSlots
      || _byEpoch.ContainsKey(epoch)
      || _bySlot[playerSlot] is not null)
    {
      return false;
    }

    var binding = new SessionBinding(epoch, playerSlot, playerHandle);
    _byEpoch.Add(epoch, binding);
    _bySlot[playerSlot] = binding;
    return true;
  }

  public bool TryRemove(SessionEpoch epoch, out SessionBinding? binding)
  {
    if (!_byEpoch.Remove(epoch, out binding) || binding is null)
    {
      binding = null;
      return false;
    }

    if (_bySlot[binding.PlayerSlot] == binding)
    {
      _bySlot[binding.PlayerSlot] = null;
    }

    return true;
  }

  public IReadOnlyList<SessionBinding> CaptureBindings()
  {
    return Array.AsReadOnly(_byEpoch.Values.OrderBy(binding => binding.PlayerSlot).ToArray());
  }
}
