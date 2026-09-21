namespace Terraria.ExternalPlatformBoundaries.Nat;

public interface INatPortMappingCollectionPort
{
  IReadOnlyList<IStaticPortMappingSnapshot> Enumerate();

  void Add(NatPortMappingKey key);

  void Remove(NatPortMappingKey key);
}
