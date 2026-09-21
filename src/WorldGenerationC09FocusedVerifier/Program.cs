using Terraria.WorldGeneration.Components;
using Terraria.WorldGeneration.Queries;
using Terraria.WorldGeneration.Systems;
using Terraria.WorldGeneration;

static class Program
{
  private static int Main()
  {
    try
    {
      TestMountainCaveHistoryBoundary();
      TestSurfaceTunnelHistoryBoundary();
      TestSurfaceOrePatchHistoryBoundary();
      Console.WriteLine("C09 history focused verifier passed.");
      return 0;
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static void TestMountainCaveHistoryBoundary()
  {
    MountainCaveHistoryComponent component =
      new(generationId: 31);

    Require(
      MountainCaveHistorySystem.TryAppend(component, 100, 200),
      "mountain cave first append");
    Require(
      MountainCaveHistorySystem.TryAppend(component, 300, 400),
      "mountain cave second append");

    MountainCaveHistorySnapshot snapshot =
      MountainCaveHistoryQuery.Snapshot(component);
    Require(snapshot.GenerationId == 31, "mountain cave generation");
    Require(snapshot.Count == 2, "mountain cave count");
    Require(snapshot.XOrigins[0] == 100 && snapshot.YOrigins[0] == 200,
      "mountain cave first paired origin");
    Require(snapshot.XOrigins[1] == 300 && snapshot.YOrigins[1] == 400,
      "mountain cave second paired origin");
    Require(
      CaveTunnelAvoidanceQuery.HasMountainCaveWithinDistance(snapshot, 199, 100),
      "mountain cave strict distance query");
    Require(
      !CaveTunnelAvoidanceQuery.HasMountainCaveWithinDistance(snapshot, 200, 100),
      "mountain cave strict boundary");

    while (component.Count < MountainCaveHistoryComponent.Capacity)
    {
      Require(
        MountainCaveHistorySystem.TryAppend(component, component.Count, component.Count + 1),
        "mountain cave fill");
    }

    Require(
      !MountainCaveHistorySystem.TryAppend(component, 999, 1000),
      "mountain cave capacity rejection");
    MountainCaveHistorySnapshot beforeClear =
      MountainCaveHistoryQuery.Snapshot(component);
    MountainCaveHistorySystem.Clear(component);
    Require(component.Count == 0, "mountain cave clear");
    Require(beforeClear.Count == MountainCaveHistoryComponent.Capacity,
      "mountain cave snapshot isolation");
  }

  private static void TestSurfaceTunnelHistoryBoundary()
  {
    SurfaceTunnelHistoryComponent component =
      new(generationId: 32);

    Require(
      SurfaceTunnelHistorySystem.TryAppend(component, 500),
      "surface tunnel first append");
    SurfaceTunnelHistorySnapshot snapshot =
      SurfaceTunnelHistoryQuery.Snapshot(component);
    Require(snapshot.GenerationId == 32, "surface tunnel generation");
    Require(snapshot.Capacity == 50, "surface tunnel declared capacity");
    Require(snapshot.EffectiveAppendCapacity == 49, "surface tunnel effective capacity");
    Require(snapshot.Count == 1 && snapshot.CenterX[0] == 500,
      "surface tunnel center");
    Require(
      CaveTunnelAvoidanceQuery.HasSurfaceTunnelWithinDistance(snapshot, 599, 100),
      "surface tunnel strict distance query");
    Require(
      !CaveTunnelAvoidanceQuery.HasSurfaceTunnelWithinDistance(snapshot, 600, 100),
      "surface tunnel strict boundary");

    while (component.Count < SurfaceTunnelHistoryDefinition.EffectiveAppendCapacity)
    {
      Require(
        SurfaceTunnelHistorySystem.TryAppend(component, component.Count * 10),
        "surface tunnel fill");
    }

    Require(component.Count == 49, "surface tunnel effective count");
    Require(
      !SurfaceTunnelHistorySystem.TryAppend(component, 999),
      "surface tunnel capacity rejection");
    SurfaceTunnelHistorySnapshot beforeClear =
      SurfaceTunnelHistoryQuery.Snapshot(component);
    SurfaceTunnelHistorySystem.Clear(component);
    Require(component.Count == 0, "surface tunnel clear");
    Require(beforeClear.Count == 49, "surface tunnel snapshot isolation");
  }

  private static void TestSurfaceOrePatchHistoryBoundary()
  {
    SurfaceOrePatchHistoryComponent component =
      new(generationId: 33);

    Require(
      !SurfaceOrePatchHistorySystem.TryAppendAfterSuccessfulCommit(
        component,
        700,
        patchCommitted: false),
      "surface ore patch rejected commit");
    Require(component.Count == 0, "surface ore patch rejected count");
    Require(
      SurfaceOrePatchHistorySystem.TryAppendAfterSuccessfulCommit(
        component,
        700,
        patchCommitted: true),
      "surface ore patch first append");
    SurfaceOrePatchHistorySnapshot snapshot =
      SurfaceOrePatchHistoryQuery.Snapshot(component);
    Require(snapshot.GenerationId == 33, "surface ore patch generation");
    Require(snapshot.Capacity == 50, "surface ore patch declared capacity");
    Require(snapshot.EffectiveAppendCapacity == 49, "surface ore patch effective capacity");
    Require(snapshot.Count == 1 && snapshot.PatchX[0] == 700,
      "surface ore patch X");
    Require(
      CaveTunnelAvoidanceQuery.HasSurfaceOrePatchWithinDistance(snapshot, 899, 200),
      "surface ore patch strict distance query");
    Require(
      !CaveTunnelAvoidanceQuery.HasSurfaceOrePatchWithinDistance(snapshot, 900, 200),
      "surface ore patch strict boundary");

    while (component.Count < SurfaceOrePatchHistoryDefinition.EffectiveAppendCapacity)
    {
      Require(
        SurfaceOrePatchHistorySystem.TryAppendAfterSuccessfulCommit(
          component,
          component.Count * 10,
          patchCommitted: true),
        "surface ore patch fill");
    }

    Require(component.Count == 49, "surface ore patch effective count");
    Require(
      !SurfaceOrePatchHistorySystem.TryAppendAfterSuccessfulCommit(
        component,
        999,
        patchCommitted: true),
      "surface ore patch capacity rejection");
    SurfaceOrePatchHistorySnapshot beforeClear =
      SurfaceOrePatchHistoryQuery.Snapshot(component);
    SurfaceOrePatchHistorySystem.Clear(component);
    Require(component.Count == 0, "surface ore patch clear");
    Require(beforeClear.Count == 49, "surface ore patch snapshot isolation");
  }

  private static void Require(bool condition, string message)
  {
    if (!condition)
    {
      throw new InvalidOperationException(message);
    }
  }
}
