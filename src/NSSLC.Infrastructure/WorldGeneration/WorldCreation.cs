using System;
using System.Threading;
using NSSLC.WorldGeneration.IO;
using NSSLC.WorldGeneration.WorldBuilding;

namespace NSSLC.WorldGeneration;

/// <summary>Runs generation serially because the imported algorithms share static state.</summary>
public static class WorldCreation {
  private static readonly object _generationLock = new();

  public static GeneratedWorld Create(WorldCreationRequest request,
                                      Action<string> passStarting = null,
                                      CancellationToken cancellationToken = default) {
    ArgumentNullException.ThrowIfNull(request);
    ArgumentException.ThrowIfNullOrWhiteSpace(request.Seed);
    if (request.Difficulty < 0 || request.Difficulty > 3 ||
        !Enum.IsDefined(request.Size) || !Enum.IsDefined(request.Evil)) {
      throw new ArgumentOutOfRangeException(nameof(request));
    }
    return WorldGen.RunWorldLifecycleOperation(() => {
      lock (_generationLock) {
        cancellationToken.ThrowIfCancellationRequested();
        WorldGenerationOptions.Reset();
        if (WorldGenerationOptions.GetOptionFromSeedText(request.Seed) != null) {
          throw new NotSupportedException("Special seed modes require additional game runtime adapters.");
        }
        (Main.maxTilesX, Main.maxTilesY) = request.Size switch {
          GeneratedWorldSize.Small => (4200, 1200),
          GeneratedWorldSize.Medium => (6400, 1800),
          GeneratedWorldSize.Large => (8400, 2400),
          _ => throw new ArgumentOutOfRangeException(nameof(request.Size))
        };
        Main.GameMode = request.Difficulty;
        Main.expertMode = request.Difficulty == 1 || request.Difficulty == 2;
        Main.masterMode = request.Difficulty == 2;
        Main.ActiveWorldFileData = new WorldFileData(request.Seed);
        WorldGen.WorldGenParam_Evil = (int)request.Evil;
        var controller = new WorldGenerator.Controller {
          PassStarting = name => {
            cancellationToken.ThrowIfCancellationRequested();
            passStarting?.Invoke(name);
          }
        };
        try {
          if (!WorldGen.GenerateWorld(customController: controller)) {
            throw new InvalidOperationException("World generation was aborted before completion.");
          }
          if (!WorldGen.InWorld(Main.spawnTileX, Main.spawnTileY, 10) ||
              !Player.Spawn_IsAreaValidSpawn(Main.spawnTileX, Main.spawnTileY, true)) {
            throw new InvalidOperationException("The generated world has an invalid spawn area.");
          }
          return new GeneratedWorld();
        } finally {
          WorldGenerator.CurrentController = null;
          WorldGenerator.CurrentGenerationProgress = null;
        }
      }
    });
  }
}
