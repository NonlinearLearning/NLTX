namespace Terraria.ExternalPlatformBoundaries.Nat;

public sealed class NatPortMappingAdapter : INatPortMappingPort
{
  private readonly INatPortMappingCollectionPort _collection;
  private readonly HashSet<NatPortMappingKey> _ownedMappings = new();

  public NatPortMappingAdapter(INatPortMappingCollectionPort collection)
  {
    _collection = collection ?? throw new ArgumentNullException(nameof(collection));
  }

  public NatPortMappingResult Ensure(NatPortMappingKey key)
  {
    try
    {
      if (FindMatchingMapping(key))
      {
        return new NatPortMappingResult(NatPortMappingStatus.AlreadyPresent, key, null);
      }

      _collection.Add(key);
      _ownedMappings.Add(key);
      return new NatPortMappingResult(NatPortMappingStatus.Added, key, null);
    }
    catch (InvalidOperationException exception)
    {
      return new NatPortMappingResult(NatPortMappingStatus.Failed, key, exception.Message);
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      return new NatPortMappingResult(NatPortMappingStatus.Unknown, key, exception.Message);
    }
    catch (Exception exception)
    {
      return new NatPortMappingResult(NatPortMappingStatus.Unknown, key, exception.Message);
    }
  }

  public NatPortMappingResult Release(NatPortMappingKey key)
  {
    if (!_ownedMappings.Remove(key))
    {
      return new NatPortMappingResult(NatPortMappingStatus.NotOwned, key, null);
    }

    try
    {
      _collection.Remove(key);
      return new NatPortMappingResult(NatPortMappingStatus.Removed, key, null);
    }
    catch (InvalidOperationException exception)
    {
      _ownedMappings.Add(key);
      return new NatPortMappingResult(NatPortMappingStatus.Failed, key, exception.Message);
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
      _ownedMappings.Add(key);
      return new NatPortMappingResult(NatPortMappingStatus.Unknown, key, exception.Message);
    }
    catch (Exception exception)
    {
      _ownedMappings.Add(key);
      return new NatPortMappingResult(NatPortMappingStatus.Unknown, key, exception.Message);
    }
  }

  private bool FindMatchingMapping(NatPortMappingKey key)
  {
    foreach (IStaticPortMappingSnapshot mapping in _collection.Enumerate())
    {
      if (mapping.InternalPort == key.InternalPort &&
        string.Equals(mapping.Protocol, key.Protocol, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(mapping.InternalClient, key.InternalClient, StringComparison.Ordinal))
      {
        return true;
      }
    }

    return false;
  }
}
