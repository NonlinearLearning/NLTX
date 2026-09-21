namespace Terraria.ExternalPlatformBoundaries.Social.Provider;

public interface ISocialJoinRequestTickSource
{
  IDisposable Subscribe(Action callback);
}
