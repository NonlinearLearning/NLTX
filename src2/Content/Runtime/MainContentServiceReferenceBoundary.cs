namespace Terraria.NonAuthoritative.ContentDefinitions;

public enum ContentServiceKind
{
  Ambience,
  ItemDrops,
  FishDrops,
  Bestiary,
  ItemDropSolver,
  BestiaryTracker,
  Pylons,
  Shop,
  Golf,
  DroneCamera
}

public interface IContentServicePort
{
  string Name { get; }
}

public sealed class MainContentServiceReferenceBoundary
{
  private readonly Dictionary<ContentServiceKind, IContentServicePort> _services = new();

  public bool IsInitialized { get; internal set; }

  public bool TryGet(ContentServiceKind kind, out IContentServicePort? service)
  {
    return _services.TryGetValue(kind, out service);
  }

  public IReadOnlyDictionary<ContentServiceKind, IContentServicePort> Snapshot()
  {
    return new Dictionary<ContentServiceKind, IContentServicePort>(_services);
  }

  internal void Attach(ContentServiceKind kind, IContentServicePort service)
  {
    ArgumentNullException.ThrowIfNull(service);
    _services[kind] = service;
  }

  internal void ClearReferences()
  {
    _services.Clear();
    IsInitialized = false;
  }
}

public static class MainContentServiceBootstrapSystem
{
  public static MainContentServiceReferenceBoundary Create()
  {
    return new MainContentServiceReferenceBoundary
    {
      IsInitialized = true
    };
  }

  public static void Attach(
    MainContentServiceReferenceBoundary boundary,
    ContentServiceKind kind,
    IContentServicePort service)
  {
    ArgumentNullException.ThrowIfNull(boundary);
    boundary.Attach(kind, service);
  }

  public static void Clear(MainContentServiceReferenceBoundary boundary)
  {
    ArgumentNullException.ThrowIfNull(boundary);
    boundary.ClearReferences();
  }
}
