using System;

namespace Terraria.WorldStorage;

/// <summary>
/// Maintains the owner/identity mapping for active projectiles.
/// All access must be serialized by the world lifecycle owner.
/// </summary>
public sealed class ProjectileIdentityIndex : IDisposable
{
  private Dictionary<OwnerProjectileIdentity, ProjectileHandle> _byOwnerIdentity = new();
  private Dictionary<ProjectileHandle, OwnerProjectileIdentity> _byProjectile = new();
  private bool _isDisposed;

  public int Count
  {
    get
    {
      VerifyAccess();
      return _byOwnerIdentity.Count;
    }
  }

  public bool TryGetHandle(
    OwnerProjectileIdentity identity,
    out ProjectileHandle handle)
  {
    VerifyAccess();
    ValidateIdentity(identity);
    return _byOwnerIdentity.TryGetValue(identity, out handle);
  }

  public bool TryGetIdentity(
    ProjectileHandle handle,
    out OwnerProjectileIdentity identity)
  {
    VerifyAccess();
    ValidateHandle(handle);
    return _byProjectile.TryGetValue(handle, out identity);
  }

  public bool TryRegister(
    OwnerProjectileIdentity identity,
    ProjectileHandle handle)
  {
    VerifyAccess();
    Validate(identity, handle);

    if (_byOwnerIdentity.TryGetValue(identity, out var existingHandle))
    {
      return existingHandle == handle &&
        _byProjectile.TryGetValue(handle, out var existingIdentity) &&
        existingIdentity == identity;
    }

    if (_byProjectile.ContainsKey(handle))
    {
      return false;
    }

    _byOwnerIdentity.EnsureCapacity(_byOwnerIdentity.Count + 1);
    _byProjectile.EnsureCapacity(_byProjectile.Count + 1);
    _byOwnerIdentity.Add(identity, handle);
    _byProjectile.Add(handle, identity);
    return true;
  }

  public bool TryReplace(
    OwnerProjectileIdentity identity,
    ProjectileHandle expectedHandle,
    ProjectileHandle replacementHandle)
  {
    VerifyAccess();
    return TryReplace(identity, expectedHandle, identity, replacementHandle);
  }

  public bool TryReplace(
    OwnerProjectileIdentity expectedIdentity,
    ProjectileHandle expectedHandle,
    OwnerProjectileIdentity replacementIdentity,
    ProjectileHandle replacementHandle)
  {
    VerifyAccess();
    Validate(expectedIdentity, expectedHandle);
    Validate(replacementIdentity, replacementHandle);

    if (!_byOwnerIdentity.TryGetValue(expectedIdentity, out var currentHandle) ||
      currentHandle != expectedHandle)
    {
      return false;
    }

    if (!_byProjectile.TryGetValue(expectedHandle, out var currentIdentity) ||
      currentIdentity != expectedIdentity)
    {
      throw new InvalidOperationException(
        "Projectile identity index maps are inconsistent.");
    }

    if (_byOwnerIdentity.TryGetValue(replacementIdentity, out var replacementCurrentHandle) &&
      (replacementIdentity != expectedIdentity || replacementCurrentHandle != expectedHandle))
    {
      return false;
    }

    if (_byProjectile.TryGetValue(replacementHandle, out var replacementCurrentIdentity) &&
      (replacementHandle != expectedHandle || replacementCurrentIdentity != expectedIdentity))
    {
      return false;
    }

    if (replacementIdentity == expectedIdentity && replacementHandle == expectedHandle)
    {
      return true;
    }

    if (replacementIdentity != expectedIdentity)
    {
      _byOwnerIdentity.EnsureCapacity(_byOwnerIdentity.Count + 1);
    }

    if (replacementHandle != expectedHandle)
    {
      _byProjectile.EnsureCapacity(_byProjectile.Count + 1);
    }

    if (replacementIdentity != expectedIdentity)
    {
      _byOwnerIdentity.Add(replacementIdentity, replacementHandle);
    }

    if (replacementHandle != expectedHandle)
    {
      _byProjectile.Add(replacementHandle, replacementIdentity);
    }
    else
    {
      _byProjectile[expectedHandle] = replacementIdentity;
    }

    if (replacementIdentity != expectedIdentity)
    {
      _byOwnerIdentity.Remove(expectedIdentity);
    }

    if (replacementHandle != expectedHandle)
    {
      _byProjectile.Remove(expectedHandle);
    }

    _byOwnerIdentity[replacementIdentity] = replacementHandle;
    return true;
  }

  public bool TryUnregister(
    ProjectileHandle expectedHandle,
    out OwnerProjectileIdentity identity)
  {
    VerifyAccess();
    ValidateHandle(expectedHandle);

    if (!_byProjectile.TryGetValue(expectedHandle, out identity))
    {
      return false;
    }

    if (!_byOwnerIdentity.TryGetValue(identity, out var currentHandle) ||
      currentHandle != expectedHandle)
    {
      throw new InvalidOperationException(
        "Projectile identity index maps are inconsistent.");
    }

    _byProjectile.Remove(expectedHandle);
    _byOwnerIdentity.Remove(identity);
    return true;
  }

  public void Rebuild(
    IReadOnlyCollection<KeyValuePair<OwnerProjectileIdentity, ProjectileHandle>> entries)
  {
    VerifyAccess();
    ArgumentNullException.ThrowIfNull(entries);

    var byOwnerIdentity = new Dictionary<OwnerProjectileIdentity, ProjectileHandle>(
      entries.Count);
    var byProjectile = new Dictionary<ProjectileHandle, OwnerProjectileIdentity>(
      entries.Count);

    foreach (var entry in entries)
    {
      Validate(entry.Key, entry.Value);
      if (!byOwnerIdentity.TryAdd(entry.Key, entry.Value) ||
        !byProjectile.TryAdd(entry.Value, entry.Key))
      {
        throw new ArgumentException(
          "Projectile identity snapshot contains a duplicate identity or handle.",
          nameof(entries));
      }
    }

    _byOwnerIdentity = byOwnerIdentity;
    _byProjectile = byProjectile;
  }

  public void Dispose()
  {
    if (_isDisposed)
    {
      return;
    }

    _byOwnerIdentity.Clear();
    _byProjectile.Clear();
    _isDisposed = true;
  }

  private void VerifyAccess()
  {
    ObjectDisposedException.ThrowIf(_isDisposed, this);
  }

  private static void Validate(
    OwnerProjectileIdentity identity,
    ProjectileHandle handle)
  {
    ValidateIdentity(identity);
    ValidateHandle(handle);
  }

  private static void ValidateIdentity(OwnerProjectileIdentity identity)
  {
    if ((uint)identity.Owner.Value > byte.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(identity));
    }

    if (identity.Value < 0)
    {
      throw new ArgumentOutOfRangeException(nameof(identity));
    }
  }

  private static void ValidateHandle(ProjectileHandle handle)
  {
    if (handle.Slot.Value < 0 || handle.Generation == 0)
    {
      throw new ArgumentOutOfRangeException(nameof(handle));
    }
  }
}
