using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using EntityEcs;
using NSSLC.WorldGeneration;
using NSSLC.WorldGeneration.IO;
using NSSLC.WorldGeneration.Utilities;
using Terraria.NonAuthoritative.Persistence;
using Terraria.NonAuthoritative.SimulationHost;
using Terraria.NonAuthoritative.WorldStorage;
using Terraria.Npc;
using Terraria.Projectile;
using RuntimeMain = NSSLC.WorldGeneration.Main;

if (args.Length != 2)
{
  throw new ArgumentException("Usage: <world.wld> <report.json>");
}

string worldPath = Path.GetFullPath(args[0]);
string reportPath = Path.GetFullPath(args[1]);
string sourceWorldHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(worldPath)));
var catalog = SimulationContentBootstrap.Build();
var identityRegistry = new EntityIdentityRegistry();
var immunity = new ProjectileStaticNpcImmunityRegistryComponent(
  checked(catalog.Snapshot.Projectiles.MaximumTypeId + 1), RuntimeNpcStore.MaximumNpcCapacity);
var npcs = new RuntimeNpcStore(immunity, identityRegistry);
var ioGate = new WorldStorageIoGate();
WorldLoadRecoveryResult? loadResult = null;
Exception? loadException = null;

RuntimeMain.InitializeHeadlessRuntime();
using IDisposable mainThreadQueue = RuntimeMain.BindMainThreadActionQueue();
RuntimeMain.dedServ = true;
RuntimeMain.netMode = 0;
RuntimeMain.gameMenu = false;
RuntimeMain.worldPathName = worldPath;
RuntimeMain.rand = new UnifiedRandom(0x4E504354);
new WorldFileData(Path.GetFileNameWithoutExtension(worldPath)).SetAsActive();

WorldStorageCoordinatorFactory.RegisterGeneratedWorldLoadHandlerWithRuntimeTileMap(
  ioGate,
  (session, _) =>
  {
    npcs.Hydrate(session, catalog);
    return WorldStorageOperationResult.Success;
  },
  static (_, _) => WorldStorageOperationResult.Success,
  static (_, _) => WorldStorageOperationResult.Success,
  resultObserver: result => loadResult = result,
  exceptionObserver: exception => loadException = exception,
  resetHostWorldState: _ =>
  {
    npcs.Reset();
    return WorldStorageOperationResult.Success;
  },
  sessionFactory: () => new LoadedWorldSession(identityRegistry));

try
{
  WorldGen.serverLoadWorldCallBack();
  if (loadException is not null || loadResult?.Succeeded != true ||
      WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession is not LoadedWorldSession session)
  {
    throw new InvalidOperationException("The production world-load entry did not publish a session.",
      loadException);
  }

  object evidence = NpcTaskLifecycleProbe.Run(npcs, session, catalog,
    new Vector2(session.World.Descriptor.SpawnTileX * 16f,
                session.World.Descriptor.SpawnTileY * 16f - 40f));
  object deathAuthorityEvidence =
    NpcDeathAuthorityProbe.Run(npcs, session, catalog);
  string finalWorldHash = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(worldPath)));
  if (finalWorldHash != sourceWorldHash)
  {
    throw new InvalidOperationException("The lifecycle verifier changed its source world file.");
  }

  string repositoryRoot = FindRepositoryRoot();
  string[] adapterFiles = [
    "src/NSSLC.Tools.Simulation/RuntimeNpcEntity.cs",
    "src/NSSLC.Tools.Simulation/RuntimeNpcStore.cs",
    "src/NSSLC.Tools.Simulation/NpcTaskLifecycleProbe.cs",
    "Test/Terraria.NpcAi.HostLifecycleVerification/NpcDeathAuthorityProbe.cs",
    "src/NSSLC/Component/Npc/System/NpcTaskLifecycleSystem.cs",
    "src/NSSLC/Component/Npc/NpcTaskStateComponent.cs",
  ];
  var sourceHashes = adapterFiles.ToDictionary(path => path,
    path => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(Path.Combine(repositoryRoot, path)))));
  Directory.CreateDirectory(Path.GetDirectoryName(reportPath)!);
  File.WriteAllText(reportPath, JsonSerializer.Serialize(new
  {
    Passed = true,
    WorldPath = worldPath,
    SourceWorldSha256 = sourceWorldHash,
    SourceWorldUnchanged = true,
    SourceFilesAtRunTime = sourceHashes,
    ExecutedAssemblies = new[]
    {
      typeof(RuntimeNpcStore).Assembly,
      typeof(NpcTaskLifecycleSystem).Assembly,
      typeof(EntityRuntime).Assembly,
    }.ToDictionary(assembly => assembly.GetName().Name!,
      assembly => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(assembly.Location)))),
    Scope =
      "Production NPC owner task termination, real world load and lethal-hit relation cleanup; " +
      "plus production Mother Slime life-root death split across SinglePlayer, Server, and Client; " +
      "excludes Simulation Program orchestration and rollback probe.",
    Evidence = evidence,
    DeathAuthorityEvidence = deathAuthorityEvidence,
  }, new JsonSerializerOptions
  {
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() },
  }));
  Console.WriteLine(
    $"PASS: production NPC host lifecycle and death authority; report {reportPath}");
}
finally
{
  npcs.Dispose();
  WorldStorageCoordinatorFactory.ActiveGeneratedWorldSession?.Dispose();
}

static string FindRepositoryRoot()
{
  for (DirectoryInfo? directory = new(Environment.CurrentDirectory);
       directory is not null; directory = directory.Parent)
  {
    if (File.Exists(Path.Combine(directory.FullName, "global.json")))
    {
      return directory.FullName;
    }
  }

  throw new InvalidOperationException("Run the verifier from the repository tree.");
}
