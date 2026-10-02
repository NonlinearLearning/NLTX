namespace Terraria.ExternalPlatformBoundaries.Nat;

public interface IStaticPortMappingSnapshot
{
  int InternalPort { get; }

  string Protocol { get; }

  string InternalClient { get; }
}
