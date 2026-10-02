namespace Terraria.ExternalPlatformBoundaries.Nat;

public interface INatPortMappingPort
{
  NatPortMappingResult Ensure(NatPortMappingKey key);

  NatPortMappingResult Release(NatPortMappingKey key);
}
