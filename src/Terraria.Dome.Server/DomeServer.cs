using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Compatibility;
using Terraria.Dome.Protocol.V1456.Isolation;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Protocol.V1456.Session;
using Terraria.Dome.Server.Protocol;
using Terraria.Dome.Server.Replication;
using Terraria.Dome.Server.Startup;
using Terraria.Dome.Server.Validation;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Commands;
using Terraria.Dome.Simulation.Items;
using Terraria.Dome.Simulation.Items.Events;
using Terraria.Dome.Simulation.Liquid.Components;
using Terraria.Dome.Simulation.Liquid.Snapshots;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.World.Events;
using Terraria.Dome.Simulation.WorldObjects;
using PipelineLiquidSourceComponent = Terraria.Dome.Simulation.Liquid.Components.LiquidSourceComponent;

namespace Terraria.Dome.Server;

public sealed class DomeServer : IDisposable
{
  private readonly record struct DoorReplicationEvent(
    int DoorId,
    DoorStateReplicationSnapshot Snapshot,
    WorldSectionCoordinates Section);
  private readonly record struct SessionRemovalRequest(
    byte PlayerSlot,
    SessionReplicationState ReplicationState);

  public const int MaximumPendingProtocolCommands = 4096;
  public const int MaximumProtocolCommandsPerTick = 256;

  private const int DefaultNpcSpawnY = 200;
  private const int DefaultFirstNpcSpawnOffsetX = -100;
  private const int DefaultSecondNpcSpawnOffsetX = -101;

  private readonly CancellationTokenSource _cancellation = new();
  private readonly ConcurrentQueue<SequencedTerrariaProtocolCommand> _protocolCommands = new();
  private readonly DomeNetworkIsolation _networkIsolation;
  private readonly DomeNetworkIsolationAdapter _networkIsolationAdapter;
  private readonly NetMessage _networkMessage;
  private readonly List<DoorReplicationEvent> _pendingDoorReplications = new();
  private readonly List<(int X, int Y, TileManipulationAction Action)> _pendingTileEntityActions = new();
  private readonly Dictionary<byte, PlayerHandle> _playersBySlot = new();
  private readonly Dictionary<byte, long> _inventoryRevisionsBySlot = new();
  private readonly Dictionary<byte, long> _equipmentRevisionsBySlot = new();
  private readonly Dictionary<byte, SessionReplicationState> _replicationBySlot = new();
  private readonly object _sessionGate = new();
  private readonly DomeSimulation _simulation;
  private readonly TerrariaProtocolSessionHost _protocolSessionHost;
  private readonly ItemReplicationAssembler _itemReplication = new();
  private readonly InventoryReplicationAssembler _inventoryReplication = new();
  private readonly EquipmentReplicationAssembler _equipmentReplication = new();
  private readonly ItemInteractionValidator _itemInteractionValidator = new();
  private readonly ContainerInteractionValidator _containerInteractionValidator = new();
  private readonly CombatReplicationAssembler _combatReplication = new();
  private readonly WorldGrid _world;
  private readonly LegacyWorldDataContext _baseWorldDataContext;
  private readonly DefaultWorldEnvironmentConvergence _worldEnvironment;
  private readonly WorldSectionReplication _worldReplication;
  private readonly List<Task> _sessionTasks = new();
  private Task? _acceptTask;
  private TcpListener? _listener;
  private Task? _simulationTask;
  private int _nextPlayerSlot = 1;
  private long _nextProtocolSequence;
  private int _pendingProtocolCommandCount;
  private long _droppedProtocolCommandCount;
  private bool _usesDefaultWorld;
  private bool _disposed;

  public DomeServer()
    : this(WorldBootstrap.CreateDefault())
  {
  }

  public DomeServer(WorldBootstrapResult bootstrap)
    : this(bootstrap?.Snapshot ?? throw new ArgumentNullException(nameof(bootstrap)))
  {
    _usesDefaultWorld = bootstrap.UsesDefaultWorld;
  }

  public DomeServer(WorldGrid world)
  {
    _world = world ?? throw new ArgumentNullException(nameof(world));
    _networkIsolation = new(
      maxInboundCount: MaximumPendingProtocolCommands,
      maxOutboundCount: MaximumPendingProtocolCommands);
    _networkMessage = new NetMessage(_networkIsolation);
    _networkIsolationAdapter = new(_networkIsolation, EnqueueProtocolCommand);
    _worldEnvironment = DefaultWorldEnvironmentConvergence.Create(_world);
    _simulation = new DomeSimulation(_world);
    _baseWorldDataContext = LegacyWorldDataContext.CreateDomeDefaults();
    _worldReplication = new WorldSectionReplication(_world);
    _protocolSessionHost = new TerrariaProtocolSessionHost(
      EnqueueProtocolCommand,
      _networkIsolation.EnqueueInbound,
      this.EnqueueOutboundFrame,
      EnsureInitialNpcsAsync,
      CreateChestSnapshots,
      CreateNpcHomeSnapshots,
      _simulation.CreateNpcReplicationSnapshots,
      _simulation.CreateTileEntitySnapshots,
      CreateWorldJoinState,
      ResolvePlayerAccountAsync,
      CreateSessionPlayerAsync,
      _worldReplication,
      CreateWorldDataContext);
  }

  public DomeServer(DomeSimulationSnapshot snapshot)
  {
    ArgumentNullException.ThrowIfNull(snapshot);
    _world = WorldGrid.FromSnapshot(snapshot.World);
    _networkIsolation = new(
      maxInboundCount: MaximumPendingProtocolCommands,
      maxOutboundCount: MaximumPendingProtocolCommands);
    _networkMessage = new NetMessage(_networkIsolation);
    _networkIsolationAdapter = new(_networkIsolation, EnqueueProtocolCommand);
    _worldEnvironment = DefaultWorldEnvironmentConvergence.Create(_world);
    _simulation = new DomeSimulation(_world, snapshot);
    _baseWorldDataContext = LegacyWorldDataContext.FromWorldMetadata(snapshot.World.Metadata);
    _worldReplication = new WorldSectionReplication(_world);
    _protocolSessionHost = new TerrariaProtocolSessionHost(
      EnqueueProtocolCommand,
      _networkIsolation.EnqueueInbound,
      this.EnqueueOutboundFrame,
      EnsureInitialNpcsAsync,
      CreateChestSnapshots,
      CreateNpcHomeSnapshots,
      _simulation.CreateNpcReplicationSnapshots,
      _simulation.CreateTileEntitySnapshots,
      CreateWorldJoinState,
      ResolvePlayerAccountAsync,
      CreateSessionPlayerAsync,
      _worldReplication,
      CreateWorldDataContext);
  }

  public SimulationSnapshot LatestSnapshot { get; private set; } = new(
    0,
    Array.Empty<PlayerSnapshot>(),
    Array.Empty<NpcSnapshot>(),
    Array.Empty<ProjectileSnapshot>());

  public int Port { get; private set; }
  public Exception? LastSessionFault { get; private set; }
  public int PendingProtocolCommandCount => Volatile.Read(ref _pendingProtocolCommandCount);
  public long DroppedProtocolCommandCount => Interlocked.Read(ref _droppedProtocolCommandCount);
  public Exception? SimulationFault { get; private set; }
  public WorldGrid World => _world;

  public DomeNetworkIsolation NetworkIsolation => _networkIsolation;

  public LegacyWorldDataContext CreateWorldDataContext()
  {
    return _baseWorldDataContext.WithWorldState(
      ProjectWorldTimeToLegacy(_simulation.TimeOfDay),
      _simulation.IsDayTime,
      _simulation.MoonPhase,
      _simulation.CreateWorldProgressionSnapshot(),
      _simulation.CreateWorldRuleState());
  }

  public DomeSimulationSnapshot CreatePersistenceSnapshot(WorldMetadata metadata)
  {
    return _simulation.CreatePersistenceSnapshot(metadata);
  }

  public bool TryQueueWorldEvent(WorldEventStartCommand command)
  {
    return _simulation.TryQueueWorldEvent(command);
  }

  public bool TryQueueWorldInvasion(WorldInvasionStartCommand command)
  {
    return _simulation.TryQueueWorldInvasion(command);
  }

  public bool TryQueueWorldInvasion(
    WorldInvasionStartCommand command,
    IReadOnlyList<WorldInvasionPlayerSnapshot> players)
  {
    return _simulation.TryQueueWorldInvasion(command, players);
  }

  public bool TryQueueWorldInvasion(
    int invasionType,
    long sequence,
    IReadOnlyList<WorldInvasionPlayerSnapshot> players)
  {
    return _simulation.TryQueueWorldInvasion(invasionType, sequence, players);
  }

  public IReadOnlyList<WorldInvasionPlayerSnapshot> CreateWorldInvasionPlayerSnapshots()
  {
    return _simulation.CreateWorldInvasionPlayerSnapshots();
  }

  public bool TryQueueWorldInvasionProgress(WorldInvasionProgressCommand command)
  {
    return _simulation.TryQueueWorldInvasionProgress(command);
  }

  public IReadOnlyList<WorldInvasionCompletedEvent> CreateWorldInvasionCompletedEvents()
  {
    return _simulation.CreateWorldInvasionCompletedEvents();
  }

  public IReadOnlyList<WorldItemSnapshot> CreateWorldItemSnapshots()
  {
    return _simulation.CreateWorldItemSnapshots();
  }

  public IReadOnlyList<WorldItemCreatedEvent> CreateWorldItemCreatedEvents()
  {
    return _simulation.CreateWorldItemCreatedEvents();
  }

  public IReadOnlyList<WorldItemPickedUpEvent> CreateWorldItemPickedUpEvents()
  {
    return _simulation.CreateWorldItemPickedUpEvents();
  }

  public int SpawnWorldItem(
    ItemStack stack,
    SimulationVector position,
    int pickupDelayTicks = 0)
  {
    return _simulation.SpawnWorldItem(stack, position, pickupDelayTicks);
  }

  public void QueueMoveWorldItem(
    int worldItemId,
    SimulationVector position,
    long expectedRevision)
  {
    _simulation.QueueMoveWorldItem(worldItemId, position, expectedRevision);
  }

  public void QueueDestroyWorldItem(int worldItemId, long expectedRevision)
  {
    _simulation.QueueDestroyWorldItem(worldItemId, expectedRevision);
  }

  public void QueueTransferItem(
    PlayerHandle player,
    int sourceSlot,
    int destinationSlot,
    int quantity)
  {
    _simulation.QueueTransferItem(player, sourceSlot, destinationSlot, quantity);
  }

  public void QueueSplitItemStack(
    PlayerHandle player,
    int sourceSlot,
    int destinationSlot,
    int quantity)
  {
    _simulation.QueueSplitItemStack(player, sourceSlot, destinationSlot, quantity);
  }

  public void QueueMergeItemStack(
    PlayerHandle player,
    int sourceSlot,
    int destinationSlot,
    int quantity)
  {
    _simulation.QueueMergeItemStack(player, sourceSlot, destinationSlot, quantity);
  }

  public void QueueDropItem(PlayerHandle player, int sourceSlot, int quantity)
  {
    _simulation.QueueDropItem(player, sourceSlot, quantity);
  }

  public IReadOnlyList<ItemStack> CreateInventorySnapshot(byte playerSlot)
  {
    if (!_playersBySlot.TryGetValue(playerSlot, out PlayerHandle player))
    {
      return [];
    }

    List<ItemStack> nonEmpty = new();
    foreach (ItemStack item in _simulation.GetInventory(player).Slots)
    {
      if (!item.IsEmpty)
      {
        nonEmpty.Add(item);
      }
    }

    return nonEmpty;
  }

  public int CreateChest(int tileX, int tileY)
  {
    return _simulation.CreateChest(tileX, tileY);
  }

  public IReadOnlyList<ChestSnapshot> CreateChestSnapshots()
  {
    return _simulation.CreateChestSnapshots();
  }

  public IReadOnlyList<NpcReplicationSnapshot> CreateNpcReplicationSnapshots()
  {
    return _simulation.CreateNpcReplicationSnapshots();
  }

  public IReadOnlyList<NpcHomeSnapshot> CreateNpcHomeSnapshots()
  {
    IReadOnlyList<NpcReplicationSnapshot> npcs = _simulation.CreateNpcReplicationSnapshots();
    List<NpcHomeSnapshot> homes = new(Math.Min(npcs.Count, 2));
    for (int index = 0; index < npcs.Count && homes.Count < 2; index++)
    {
      NpcReplicationSnapshot npc = npcs[index];
      if (!npc.IsActive)
      {
        continue;
      }

      homes.Add(new NpcHomeSnapshot(
        npc.ReplicationId,
        (short)Math.Clamp((int)MathF.Floor(npc.Position.X), short.MinValue, short.MaxValue),
        (short)Math.Clamp((int)MathF.Floor(npc.Position.Y), short.MinValue, short.MaxValue),
        IsHomeless: homes.Count == 1));
    }

    return homes;
  }

  public async Task<bool> QueueLiquidSourceAsync(
    PipelineLiquidSourceComponent source,
    CancellationToken cancellationToken = default)
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(DomeServer));
    }

    TaskCompletionSource<bool> completion = new(
      TaskCreationOptions.RunContinuationsAsynchronously);
    if (!EnqueueProtocolCommand(new QueueLiquidSourceCommand(source, completion)))
    {
      throw new IOException("The liquid source command queue is full.");
    }

    return await completion.Task.WaitAsync(cancellationToken);
  }

  public void SetChestItem(int chestId, int chestSlot, ItemStack stack)
  {
    _simulation.SetChestItem(chestId, chestSlot, stack);
  }

  public int CreateDoor(int tileX, int tileY)
  {
    return _simulation.CreateDoor(tileX, tileY);
  }

  public int CreateTrapdoor(int tileX, int tileY, bool opensDown)
  {
    return _simulation.CreateTrapdoor(tileX, tileY, opensDown);
  }

  public int CreateTallGate(int tileX, int tileY)
  {
    return _simulation.CreateTallGate(tileX, tileY);
  }

  public IReadOnlyList<DoorSnapshot> CreateDoorSnapshots()
  {
    return _simulation.CreateDoorSnapshots();
  }

  public int CreateSign(int tileX, int tileY, string text)
  {
    return _simulation.CreateSign(tileX, tileY, text);
  }

  public IReadOnlyList<SignSnapshot> CreateSignSnapshots()
  {
    return _simulation.CreateSignSnapshots();
  }

  public WorldJoinStateSnapshot CreateWorldJoinState()
  {
    return WorldJoinStateSnapshot.CreateDefault();
  }

  public void Dispose()
  {
    if (_disposed)
    {
      return;
    }

    _disposed = true;
    _cancellation.Cancel();
    _networkIsolation.Close();
    _listener?.Stop();
    if (_acceptTask is not null)
    {
      try
      {
        _acceptTask.Wait(TimeSpan.FromSeconds(2));
      }
      catch (AggregateException)
      {
      }
    }

    if (_simulationTask is not null)
    {
      try
      {
        _simulationTask.Wait(TimeSpan.FromSeconds(2));
      }
      catch (AggregateException)
      {
      }
    }

    Task[] sessionTasks = GetSessionTasks();
    try
    {
      Task.WaitAll(sessionTasks, TimeSpan.FromSeconds(2));
    }
    catch (AggregateException)
    {
    }

    _simulation.Dispose();
    _cancellation.Dispose();
  }

  public void Start(int port = 0)
  {
    ThrowIfStarted();

    _listener = new TcpListener(IPAddress.Loopback, port);
    _listener.Start();
    Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
    _acceptTask = AcceptLoopAsync(_cancellation.Token);
    _simulationTask = SimulationLoopAsync(_cancellation.Token);
  }

  private async Task AcceptLoopAsync(CancellationToken cancellationToken)
  {
    TcpListener listener = _listener ?? throw new InvalidOperationException("Server is not started.");
    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        TcpClient client = await listener.AcceptTcpClientAsync(cancellationToken);
        Task sessionTask = HandleAcceptedClientAsync(client, cancellationToken);
        lock (_sessionGate)
        {
          _sessionTasks.Add(sessionTask);
        }

        _ = sessionTask.ContinueWith(
          completedTask => RemoveSessionTask(completedTask),
          CancellationToken.None,
          TaskContinuationOptions.ExecuteSynchronously,
          TaskScheduler.Default);
      }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
    }
    catch (SocketException) when (cancellationToken.IsCancellationRequested)
    {
    }
  }

  private async Task HandleAcceptedClientAsync(
    TcpClient client,
    CancellationToken cancellationToken)
  {
    using (client)
    {
      try
      {
        await HandleClientAsync(client, cancellationToken);
      }
      catch (InvalidDataException exception)
    {
      // Protocol failures belong to one session and must not stop accepting peers.
      LastSessionFault = exception;
      }
      catch (EndOfStreamException)
      {
      }
    catch (IOException exception)
    {
      // A peer closing its socket during a frame read is a normal session teardown.
      LastSessionFault = exception;
    }
      catch (Exception exception)
      {
        LastSessionFault = exception;
      }
    }
  }

  private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
  {
    if (_nextPlayerSlot > byte.MaxValue)
    {
      client.Close();
      return;
    }

    byte playerSlot = (byte)_nextPlayerSlot;
    _nextPlayerSlot++;
    await _protocolSessionHost.HandleAsync(client, playerSlot, cancellationToken);
  }

  private void CreateDefaultWorldNpcs(SimulationVector playerSpawn)
  {
    SimulationVector firstSpawn = ResolveDefaultNpcSpawn(
      WorldBootstrap.DefaultSpawnX + DefaultFirstNpcSpawnOffsetX,
      playerSpawn);
    SimulationVector secondSpawn = ResolveDefaultNpcSpawn(
      WorldBootstrap.DefaultSpawnX + DefaultSecondNpcSpawnOffsetX,
      playerSpawn);
    _simulation.CreateNpc(firstSpawn, DomeSimulation.FixtureNpcType);
    _simulation.CreateNpc(secondSpawn, DomeSimulation.FixtureNpcType);
  }

  private SimulationVector ResolveDefaultNpcSpawn(
    int requestedX,
    SimulationVector playerSpawn)
  {
    int x = Math.Clamp(requestedX, 0, _world.Width - 1);
    if (IsNpcSpawnSpaceFree(x, DefaultNpcSpawnY))
    {
      return new SimulationVector(x, DefaultNpcSpawnY);
    }

    if (IsNpcSpawnSpaceFree(x, 0))
    {
      return new SimulationVector(x, 0.0f);
    }

    int requestedY = (int)MathF.Floor(playerSpawn.Y);
    if (IsNpcSpawnSpaceFree(x, requestedY))
    {
      return new SimulationVector(x, requestedY);
    }

    int firstNearbyY = Math.Max(0, requestedY - 64);
    int lastNearbyY = Math.Min(_world.Height - 2, requestedY + 64);
    for (int y = firstNearbyY; y <= lastNearbyY; y++)
    {
      if (IsNpcSpawnSpaceFree(x, y))
      {
        return new SimulationVector(x, y);
      }
    }

    for (int y = 0; y <= _world.Height - 2; y++)
    {
      if (IsNpcSpawnSpaceFree(x, y))
      {
        return new SimulationVector(x, y);
      }
    }

    throw new InvalidOperationException("The default world has no valid NPC spawn space.");
  }

  private bool IsNpcSpawnSpaceFree(int x, int y)
  {
    return y >= 0 && y + 1 < _world.Height &&
      _world.Contains(x, y) &&
      !_world.GetTile(x, y).IsActive &&
      !_world.GetTile(x, y + 1).IsActive;
  }

  private bool EnqueueProtocolCommand(TerrariaProtocolCommand command)
  {
    if (command is ResolvePlayerAccountCommand)
    {
    }
    int pending = Interlocked.Increment(ref _pendingProtocolCommandCount);
    if (pending > MaximumPendingProtocolCommands)
    {
      Interlocked.Decrement(ref _pendingProtocolCommandCount);
      Interlocked.Increment(ref _droppedProtocolCommandCount);
      return false;
    }

    long sequence = Interlocked.Increment(ref _nextProtocolSequence);
    _protocolCommands.Enqueue(new SequencedTerrariaProtocolCommand(sequence, command));
    return true;
  }

  private bool EnqueueOutboundFrame(byte playerSlot, byte[] frameBytes)
  {
    ArgumentNullException.ThrowIfNull(frameBytes);
    TerrariaFrame frame = TerrariaFrameCodec.Decode(frameBytes);
    return _networkMessage.TrySendData(
      frame.MessageId,
      playerSlot,
      playerSlot,
      frame.Payload.Span);
  }

  private async Task<PlayerPersistentState> ResolvePlayerAccountAsync(
    PlayerBootstrapState bootstrap,
    CancellationToken cancellationToken)
  {
    TaskCompletionSource<PlayerPersistentState> completion = new(
      TaskCreationOptions.RunContinuationsAsynchronously);
    if (!EnqueueProtocolCommand(new ResolvePlayerAccountCommand(0, bootstrap, completion)))
    {
      throw new IOException("The player account command queue is full.");
    }

    return await completion.Task.WaitAsync(cancellationToken);
  }

  private Task EnsureInitialNpcsAsync(
    int requestedSpawnX,
    int requestedSpawnY,
    CancellationToken cancellationToken)
  {
    SimulationVector playerSpawn = requestedSpawnX >= 0 && requestedSpawnY >= 0
      ? new SimulationVector(requestedSpawnX, requestedSpawnY)
      : new SimulationVector(WorldBootstrap.DefaultSpawnX, WorldBootstrap.DefaultSurfaceY);
    TaskCompletionSource<bool> completion = new(
      TaskCreationOptions.RunContinuationsAsynchronously);
    if (!EnqueueProtocolCommand(new EnsureInitialNpcsCommand(playerSpawn, completion)))
    {
      throw new IOException("The NPC initialization command queue is full.");
    }

    return completion.Task.WaitAsync(cancellationToken);
  }

  private async Task<PlayerInitialProjection> CreateSessionPlayerAsync(
    byte playerSlot,
    int spawnX,
    int spawnY,
    PlayerPersistentState account,
    SessionReplicationState replicationState,
    CancellationToken cancellationToken)
  {
    TaskCompletionSource<PlayerInitialProjection> completion = new(
      TaskCreationOptions.RunContinuationsAsynchronously);
    if (!EnqueueProtocolCommand(new CreateSessionPlayerCommand(
      playerSlot,
      spawnX,
      spawnY,
      account,
      replicationState,
      completion)))
    {
      throw new IOException("The player creation command queue is full.");
    }

    return await completion.Task.WaitAsync(cancellationToken);
  }

  private Task[] GetSessionTasks()
  {
    lock (_sessionGate)
    {
      return [.. _sessionTasks];
    }
  }

  private void RemoveSessionTask(Task sessionTask)
  {
    lock (_sessionGate)
    {
      _sessionTasks.Remove(sessionTask);
    }
  }

  private async Task SimulationLoopAsync(CancellationToken cancellationToken)
  {
    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        _networkIsolationAdapter.Update();
        List<PlayerInput> inputs = new();
        Dictionary<byte, int> tileActionsBySlot = new();
        int processedCommands = 0;
        while (processedCommands < MaximumProtocolCommandsPerTick &&
               _protocolCommands.TryDequeue(out SequencedTerrariaProtocolCommand? command))
        {
          Interlocked.Decrement(ref _pendingProtocolCommandCount);
          ApplyProtocolCommand(command, inputs, tileActionsBySlot);
          processedCommands++;
        }

        _simulation.QueueProximityWorldItemPickups();
        _simulation.Tick(new SimulationInputBatch(NormalizePlayerInputs(inputs)));
        _world.CommitTileChanges();
        ApplyPendingTrainingDummyTileActions();
        IReadOnlyList<LiquidReplicationSnapshot> liquidChanges =
          _simulation.CreateLiquidReplicationSnapshots();
        IReadOnlyList<WorldEnvironmentChange> environmentChanges =
          HasActiveReplicationSessions() ? _worldEnvironment.Advance(_world) : [];
        LatestSnapshot = _simulation.CreateSnapshot();
        await ReplicateLiquidChangesAsync(liquidChanges, cancellationToken);
        await ReplicateEnvironmentChangesAsync(environmentChanges, cancellationToken);
        await ReplicateChangedWorldSectionsAsync(cancellationToken);
        await ReplicatePlayerVisibilityAsync(cancellationToken);
        await ReplicatePlayerStatesAsync(cancellationToken);
        await ReplicateInventoryStatesAsync(cancellationToken);
        await ReplicateEquipmentStatesAsync(cancellationToken);
        await ReplicateCombatStatesAsync(cancellationToken);
        await ReplicateItemStatesAsync(cancellationToken);
        await ReplicateChestStatesAsync(cancellationToken);
        await ReplicateDoorStatesAsync(cancellationToken);
        await ReplicateSignStatesAsync(cancellationToken);
        await ReplicateTileEntityStatesAsync(cancellationToken);
        await ReplicateWorldRulesAsync(cancellationToken);
        await FlushNetworkIsolationOutboundAsync(cancellationToken);
        await Task.Delay(TimeSpan.FromMilliseconds(16), cancellationToken);
      }
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
    }
    catch (Exception exception)
    {
      SimulationFault = exception;
    }
  }

  private void ApplyProtocolCommand(
    SequencedTerrariaProtocolCommand sequencedCommand,
    List<PlayerInput> inputs,
    Dictionary<byte, int> tileActionsBySlot)
  {
    TerrariaProtocolCommand command = sequencedCommand.Command;
    switch (command)
    {
      case ResolvePlayerAccountCommand resolveAccount:
        try
        {
          PlayerPersistentState bootstrap = PlayerPersistentStateMapper.FromBootstrap(
            resolveAccount.Bootstrap);
          PlayerPersistentState account = _simulation.ImportPlayerIfMissing(bootstrap);
          resolveAccount.Completion.TrySetResult(account);
        }
        catch (Exception exception)
        {
          resolveAccount.Completion.TrySetException(exception);
        }

        return;
      case EnsureInitialNpcsCommand ensureInitialNpcs:
        try
        {
          if (_simulation.NpcCount == 0)
          {
            if (_usesDefaultWorld)
            {
              CreateDefaultWorldNpcs(ensureInitialNpcs.PlayerSpawn);
            }
            else
            {
              _simulation.CreateNpc(new SimulationVector(
                ensureInitialNpcs.PlayerSpawn.X + 20.0f,
                ensureInitialNpcs.PlayerSpawn.Y));
            }
          }

          ensureInitialNpcs.Completion.TrySetResult(true);
        }
        catch (Exception exception)
        {
          ensureInitialNpcs.Completion.TrySetException(exception);
        }
        return;
      case QueueLiquidSourceCommand queueLiquidSource:
        try
        {
          queueLiquidSource.Completion.TrySetResult(
            _simulation.TryQueueLiquidSource(queueLiquidSource.Source));
        }
        catch (Exception exception)
        {
          queueLiquidSource.Completion.TrySetException(exception);
        }
        return;
      case CreateSessionPlayerCommand createPlayer:
        try
        {
          if (!_playersBySlot.TryGetValue(createPlayer.PlayerSlot, out PlayerHandle player))
          {
            player = _simulation.CreatePlayer(
              createPlayer.Account,
              new SimulationVector(createPlayer.SpawnX, createPlayer.SpawnY),
              createPlayer.PlayerSlot);
            if (_simulation.NpcCount == 0)
            {
              if (_usesDefaultWorld)
              {
                CreateDefaultWorldNpcs(new SimulationVector(
                  createPlayer.SpawnX,
                  createPlayer.SpawnY));
              }
              else
              {
                _simulation.CreateNpc(new SimulationVector(
                  createPlayer.SpawnX + 20.0f,
                  createPlayer.SpawnY));
              }
            }

            _playersBySlot.Add(createPlayer.PlayerSlot, player);
            _replicationBySlot.Add(createPlayer.PlayerSlot, createPlayer.ReplicationState);
          }

          PlayerSnapshot snapshot = _simulation.CreateSnapshot().FindPlayer(player);
          PlayerStateSnapshot playerState = _simulation.CreatePlayerStateSnapshot(player);
          createPlayer.Completion.TrySetResult(new PlayerInitialProjection(
            playerState.IsActive,
            snapshot.Facing > 0,
            snapshot.Position.X,
            snapshot.Position.Y));
        }
        catch (Exception exception)
        {
          createPlayer.Completion.TrySetException(exception);
        }

          return;
      case ApplyPlayerControlCommand applyControls:
        if (!_playersBySlot.TryGetValue(applyControls.PlayerSlot, out PlayerHandle controlledPlayer))
        {
          return;
        }

        PlayerControlIntent controls = applyControls.Controls;
        ItemInteractionValidation itemValidation = _itemInteractionValidator.Validate(
          controls,
          _simulation.CreatePlayerStateSnapshot(controlledPlayer),
          _simulation.GetInventory(controlledPlayer));
        if (!itemValidation.IsAccepted)
        {
          return;
        }

        inputs.Add(new PlayerInput(
          controlledPlayer,
          MoveLeft: controls.MoveLeft,
          MoveRight: controls.MoveRight,
          Jump: controls.Jump,
          Fire: controls.UseItem,
          SelectedSlot: controls.SelectedItem,
          UseItem: controls.UseItem,
          Facing: controls.FacingRight ? 1 : -1,
          Down: controls.Down));
        return;
      case ApplyTileManipulationCommand tileManipulation:
        if (!_playersBySlot.TryGetValue(tileManipulation.PlayerSlot, out PlayerHandle actor) ||
            !_replicationBySlot.TryGetValue(tileManipulation.PlayerSlot, out SessionReplicationState? state))
        {
          return;
        }

        int actionsThisTick = tileActionsBySlot.GetValueOrDefault(tileManipulation.PlayerSlot);
        PlayerSnapshot playerSnapshot = _simulation.CreateSnapshot().FindPlayer(actor);
        TileInteractionValidation validation = new TileInteractionValidator().Validate(
          tileManipulation.Intent,
          playerSnapshot,
          _world,
          state.VisibleSections,
          actionsThisTick,
          sequencedCommand.Sequence);
        if (!validation.IsAccepted || validation.Command is not TileChangeCommand tileChange)
        {
          return;
        }

        tileActionsBySlot[tileManipulation.PlayerSlot] = actionsThisTick + 1;
        _world.EnqueueTileChange(tileChange);
        if (tileManipulation.Intent.TileType == 378 ||
            tileManipulation.Intent.Action == TileManipulationAction.KillTile)
        {
          _pendingTileEntityActions.Add((
            tileManipulation.Intent.X,
            tileManipulation.Intent.Y,
            tileManipulation.Intent.Action));
        }
        return;
      case PlaceTileEntityCommand placeTileEntity:
        if (placeTileEntity.Intent.EntityType != 0 ||
            !_playersBySlot.ContainsKey(placeTileEntity.PlayerSlot) ||
            !_replicationBySlot.TryGetValue(
              placeTileEntity.PlayerSlot,
              out SessionReplicationState? placementState) ||
            !_world.Contains(placeTileEntity.Intent.TileX, placeTileEntity.Intent.TileY) ||
            !placementState.VisibleSections.Contains(_world.GetSectionCoordinates(
              placeTileEntity.Intent.TileX,
              placeTileEntity.Intent.TileY)))
        {
          return;
        }

        _ = _simulation.TryPlaceTrainingDummy(
          placeTileEntity.Intent.TileX,
          placeTileEntity.Intent.TileY,
          out _);
        return;
      case OpenChestCommand openChest:
        if (!_playersBySlot.TryGetValue(openChest.PlayerSlot, out PlayerHandle chestPlayer) ||
            !_replicationBySlot.TryGetValue(openChest.PlayerSlot, out SessionReplicationState? chestState))
        {
          return;
        }

        ChestSnapshot? chest = FindChest(openChest.Intent.ChestId);
        if (chest is null)
        {
          return;
        }

        PlayerSnapshot chestPlayerSnapshot = _simulation.CreateSnapshot().FindPlayer(chestPlayer);
        if (!_containerInteractionValidator.IsOpenRequestAccepted(
          chest.Value,
          chestPlayerSnapshot,
          openChest.Intent.TileX,
          openChest.Intent.TileY,
          chestState.VisibleSections))
        {
          return;
        }

        _ = _simulation.TryOpenChest(
          chest.Value.ChestId,
          chestPlayer,
          chestPlayerSnapshot.Position);
        return;
      case ToggleDoorCommand toggleDoor:
        if (!_playersBySlot.TryGetValue(toggleDoor.PlayerSlot, out PlayerHandle doorPlayer) ||
            !_replicationBySlot.TryGetValue(toggleDoor.PlayerSlot, out SessionReplicationState? doorState))
        {
          return;
        }

        if (!TryGetDoorTransition(toggleDoor.Intent.Action, out DoorTransition transition))
        {
          return;
        }

        DoorSnapshot? door = _simulation.FindDoorAt(
          toggleDoor.Intent.TileX,
          toggleDoor.Intent.TileY);
        if (door is null ||
            !doorState.VisibleSections.Contains(door.Value.Section))
        {
          return;
        }

        PlayerSnapshot doorPlayerSnapshot = _simulation.CreateSnapshot().FindPlayer(doorPlayer);
        if (_simulation.TryApplyDoorTransition(
          door.Value.DoorId,
          transition,
          toggleDoor.Intent.Direction,
          doorPlayerSnapshot.Position))
        {
          _pendingDoorReplications.Add(new DoorReplicationEvent(
            door.Value.DoorId,
            new DoorStateReplicationSnapshot(
              toggleDoor.Intent.Action,
              toggleDoor.Intent.TileX,
              toggleDoor.Intent.TileY,
              toggleDoor.Intent.Direction),
            door.Value.Section));
        }
        return;
      case UpdateSignCommand updateSign:
        if (!_playersBySlot.TryGetValue(updateSign.PlayerSlot, out PlayerHandle signPlayer) ||
            !_replicationBySlot.TryGetValue(updateSign.PlayerSlot, out SessionReplicationState? signState))
        {
          return;
        }

        SignSnapshot? sign = FindSign(updateSign.Intent.SignId);
        if (sign is null || sign.Value.TileX != updateSign.Intent.TileX ||
            sign.Value.TileY != updateSign.Intent.TileY ||
            !signState.VisibleSections.Contains(sign.Value.Section))
        {
          return;
        }

        PlayerSnapshot signPlayerSnapshot = _simulation.CreateSnapshot().FindPlayer(signPlayer);
        _ = _simulation.TryUpdateSign(
          sign.Value.SignId,
          signPlayerSnapshot.Position,
          updateSign.Intent.Text);
        return;
      case OpenSignCommand openSign:
        if (!_replicationBySlot.TryGetValue(
              openSign.PlayerSlot,
              out SessionReplicationState? openSignState))
        {
          openSign.Completion.TrySetResult(null);
          return;
        }

        SignSnapshot? requestedSign = FindSignAt(
          openSign.Request.TileX,
          openSign.Request.TileY);
        if (requestedSign is null ||
            !openSignState.VisibleSections.Contains(requestedSign.Value.Section))
        {
          openSign.Completion.TrySetResult(null);
          return;
        }

        openSign.Completion.TrySetResult(new SignReplicationSnapshot(
          requestedSign.Value.SignId,
          (short)requestedSign.Value.TileX,
          (short)requestedSign.Value.TileY,
          requestedSign.Value.Text,
          openSign.PlayerSlot,
          SuppressOpenSign: false,
          requestedSign.Value.Revision));
        return;
      case TransferChestItemCommand transferChestItem:
        if (!_playersBySlot.TryGetValue(transferChestItem.PlayerSlot, out PlayerHandle transferPlayer) ||
            !_replicationBySlot.TryGetValue(transferChestItem.PlayerSlot,
              out SessionReplicationState? transferState))
        {
          return;
        }

        ChestSnapshot? transferChest = FindChest(transferChestItem.Intent.ChestId);
        if (transferChest is null || transferChest.Value.Opener != transferPlayer ||
            !transferState.VisibleSections.Contains(transferChest.Value.Section))
        {
          return;
        }

        PlayerSnapshot transferPlayerSnapshot = _simulation.CreateSnapshot().FindPlayer(transferPlayer);
        if (!transferState.TryGetChestRevision(transferChest.Value.ChestId, out long expectedRevision))
        {
          return;
        }

        _ = _simulation.TryTransferChestItem(
          transferChest.Value.ChestId,
          transferPlayer,
          transferChestItem.Intent.InventorySlot,
          transferChestItem.Intent.ChestSlot,
          transferChestItem.Intent.Withdraw,
          transferPlayerSnapshot.Position,
          expectedRevision);
        return;
      case DestroySessionPlayerCommand destroyPlayer:
        RemoveDestroyedPlayerInputs(inputs, destroyPlayer);
        RemoveSessionPlayer(destroyPlayer.PlayerSlot, destroyPlayer.ReplicationState);
        return;
      default:
        throw new InvalidOperationException("Unknown Terraria protocol command.");
    }
  }

  private void ApplyPendingTrainingDummyTileActions()
  {
    for (int index = 0; index < _pendingTileEntityActions.Count; index++)
    {
      (int x, int y, TileManipulationAction action) = _pendingTileEntityActions[index];
      if (action == TileManipulationAction.PlaceTile)
      {
        _ = _simulation.TryPlaceTrainingDummy(x, y, out _);
      }
      else if (action == TileManipulationAction.KillTile)
      {
        _ = _simulation.TryRemoveTrainingDummy(x, y);
      }
    }

    _pendingTileEntityActions.Clear();
  }

  private static PlayerInput[] NormalizePlayerInputs(IReadOnlyList<PlayerInput> inputs)
  {
    Dictionary<PlayerHandle, PlayerInput> latestByPlayer = new();
    for (int index = 0; index < inputs.Count; index++)
    {
      PlayerInput input = inputs[index];
      latestByPlayer[input.Player] = input;
    }

    List<PlayerInput> normalized = new(latestByPlayer.Values);
    normalized.Sort(static (first, second) => first.Player.Value.CompareTo(second.Player.Value));
    return normalized.ToArray();
  }

  private void QueueOutboundFrames(byte playerSlot, IReadOnlyList<byte[]> frames)
  {
    for (int index = 0; index < frames.Count; index++)
    {
      TerrariaFrame frame = TerrariaFrameCodec.Decode(frames[index]);
      if (!_networkMessage.TrySendData(
        frame.MessageId,
        playerSlot,
        playerSlot,
        frame.Payload.Span))
      {
        throw new IOException("The isolated network output queue is full.");
      }
    }
  }

  private static void ObserveQueuedWrite(Task writeTask)
  {
    _ = writeTask.ContinueWith(
      static completedTask => _ = completedTask.Exception,
      CancellationToken.None,
      TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
      TaskScheduler.Default);
  }

  private Task FlushNetworkIsolationOutboundAsync(CancellationToken cancellationToken)
  {
    IReadOnlyList<NetworkOutboundEnvelope> pending = _networkIsolation.ReadOutbound();
    if (pending.Count == 0)
    {
      return Task.CompletedTask;
    }

    Dictionary<(int TargetClient, int IgnoreClient), List<byte[]>> framesByTarget = new();
    for (int index = 0; index < pending.Count; index++)
    {
      NetworkOutboundEnvelope envelope = pending[index];
      (int TargetClient, int IgnoreClient) target =
        (envelope.TargetClient, envelope.IgnoreClient);
      if (!framesByTarget.TryGetValue(target, out List<byte[]>? frames))
      {
        frames = new List<byte[]>();
        framesByTarget.Add(target, frames);
      }

      frames.Add(envelope.FrameBytes.ToArray());
    }

    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<(int TargetClient, int IgnoreClient), List<byte[]>> entry in framesByTarget)
    {
      if (entry.Key.TargetClient < 0)
      {
        foreach (KeyValuePair<byte, SessionReplicationState> session in _replicationBySlot)
        {
          if (!session.Value.IsActive || session.Key == entry.Key.IgnoreClient)
          {
            continue;
          }

          try
          {
            ObserveQueuedWrite(session.Value.WriteFramesAsync(entry.Value, cancellationToken));
          }
          catch (IOException)
          {
            failedSessions.Add(new SessionRemovalRequest(session.Key, session.Value));
          }
        }

        continue;
      }

      if (!_replicationBySlot.TryGetValue(
            (byte)entry.Key.TargetClient,
            out SessionReplicationState? state) ||
          !state.IsActive)
      {
        continue;
      }

      try
      {
        ObserveQueuedWrite(state.WriteFramesAsync(entry.Value, cancellationToken));
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest((byte)entry.Key.TargetClient, state));
      }
    }

    _networkIsolation.ClearOutbound();
    RemoveFailedSessions(failedSessions);
    return Task.CompletedTask;
  }

  private async Task ReplicatePlayerVisibilityAsync(CancellationToken cancellationToken)
  {
    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      if (!entry.Value.IsActive)
      {
        continue;
      }

      if (!_playersBySlot.TryGetValue(entry.Key, out PlayerHandle player))
      {
        continue;
      }

      PlayerSnapshot snapshot = LatestSnapshot.FindPlayer(player);
      IReadOnlyList<byte[]> frames = _worldReplication.UpdatePlayerVisibility(snapshot, entry.Value);
      if (frames.Count == 0)
      {
        continue;
      }

      try
      {
        QueueOutboundFrames(entry.Key, frames);
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, entry.Value));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private async Task ReplicateChangedWorldSectionsAsync(CancellationToken cancellationToken)
  {
    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      if (!entry.Value.IsActive)
      {
        continue;
      }

      IReadOnlyList<byte[]> frames = _worldReplication.CreateChangedWorldStream(entry.Value);
      if (frames.Count == 0)
      {
        continue;
      }

      try
      {
        QueueOutboundFrames(entry.Key, frames);
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, entry.Value));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private bool HasActiveReplicationSessions()
  {
    foreach (SessionReplicationState state in _replicationBySlot.Values)
    {
      if (state.IsActive)
      {
        return true;
      }
    }

    return false;
  }


  private async Task ReplicateEnvironmentChangesAsync(
    IReadOnlyList<WorldEnvironmentChange> changes,
    CancellationToken cancellationToken)
  {
    if (changes.Count == 0)
    {
      return;
    }

    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      SessionReplicationState state = entry.Value;
      if (!state.IsActive)
      {
        continue;
      }

      List<byte[]> frames = new(changes.Count);
      for (int index = 0; index < changes.Count; index++)
      {
        WorldEnvironmentChange change = changes[index];
        bool hasVisibleSection = HasVisibleAffectedSection(state, change);
        switch (change.Kind)
        {
          case WorldEnvironmentChangeKind.TileSquare:
            if (hasVisibleSection)
            {
              frames.Add(TerrariaPacketCodec.EncodeTileSquare(
                _world,
                change.X,
                change.Y,
                change.Width,
                change.Height,
                change.ChangeType));
            }

            break;
          case WorldEnvironmentChangeKind.TileManipulation:
            if (hasVisibleSection)
            {
              frames.Add(TerrariaPacketCodec.EncodeServerTileManipulation(
                change.ChangeType,
                change.X,
                change.Y,
                change.Tile.Type,
                style: 0));
            }

            break;
          case WorldEnvironmentChangeKind.Liquid:
            List<WorldLiquidSnapshot> visibleLiquidChanges = new(1);
            if (hasVisibleSection)
            {
              WorldTile tile = _world.GetTile(change.X, change.Y);
              visibleLiquidChanges.Add(new WorldLiquidSnapshot(
                change.X,
                change.Y,
                tile.LiquidAmount,
                tile.LiquidType));
            }

            frames.Add(TerrariaPacketCodec.EncodeLiquidNetModule(visibleLiquidChanges));
            break;
          default:
            throw new InvalidOperationException("Unknown world environment change kind.");
        }
      }

      try
      {
        QueueOutboundFrames(entry.Key, frames);
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, state));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private async Task ReplicateLiquidChangesAsync(
    IReadOnlyList<LiquidReplicationSnapshot> changes,
    CancellationToken cancellationToken)
  {
    if (changes.Count == 0)
    {
      return;
    }

    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      SessionReplicationState state = entry.Value;
      if (!state.IsActive)
      {
        continue;
      }

      List<WorldLiquidSnapshot> visibleChanges = new();
      for (int index = 0; index < changes.Count; index++)
      {
        LiquidReplicationSnapshot change = changes[index];
        if (!_world.Contains(change.X, change.Y) ||
            !state.VisibleSections.Contains(_world.GetSectionCoordinates(change.X, change.Y)))
        {
          continue;
        }

        visibleChanges.Add(new WorldLiquidSnapshot(
          change.X,
          change.Y,
          change.Amount,
          change.Type));
      }

      if (visibleChanges.Count == 0)
      {
        continue;
      }

      try
      {
        QueueOutboundFrames(entry.Key, [TerrariaPacketCodec.EncodeLiquidNetModule(visibleChanges)]);
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, state));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private bool HasVisibleAffectedSection(
    SessionReplicationState state,
    WorldEnvironmentChange change)
  {
    for (int yOffset = 0; yOffset < change.Height; yOffset++)
    {
      for (int xOffset = 0; xOffset < change.Width; xOffset++)
      {
        WorldSectionCoordinates section = _world.GetSectionCoordinates(
          change.X + xOffset,
          change.Y + yOffset);
        if (!state.VisibleSections.Contains(section))
        {
          continue;
        }

        return true;
      }
    }

    return false;
  }

  private async Task ReplicatePlayerStatesAsync(CancellationToken cancellationToken)
  {
    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> recipient in _replicationBySlot)
    {
      if (!recipient.Value.IsActive)
      {
        continue;
      }

      List<PlayerReplicationState> visiblePlayers = new();
      foreach (KeyValuePair<byte, PlayerHandle> playerEntry in _playersBySlot)
      {
        if (playerEntry.Key == recipient.Key)
        {
          continue;
        }

        PlayerSnapshot snapshot = LatestSnapshot.FindPlayer(playerEntry.Value);
        int tileX = Math.Clamp((int)MathF.Floor(snapshot.Position.X), 0, _world.Width - 1);
        int tileY = Math.Clamp((int)MathF.Floor(snapshot.Position.Y), 0, _world.Height - 1);
        WorldSectionCoordinates section = _world.GetSectionCoordinates(tileX, tileY);
        if (!recipient.Value.VisibleSections.Contains(section))
        {
          continue;
        }

        visiblePlayers.Add(new PlayerReplicationState(
          playerEntry.Key,
          snapshot.Position,
          snapshot.Velocity,
          snapshot.Facing,
          snapshot.IsActive,
          snapshot.Health,
          _simulation.CreatePlayerStateSnapshot(playerEntry.Value).MaximumHealth));
      }

      IReadOnlyList<PlayerReplicationState> changed = recipient.Value.CollectChangedPlayers(
        visiblePlayers);
      if (changed.Count == 0)
      {
        continue;
      }

      List<byte[]> frames = new(changed.Count * 2);
      for (int index = 0; index < changed.Count; index++)
      {
        PlayerReplicationState player = changed[index];
        PlayerSnapshot snapshot = LatestSnapshot.FindPlayer(_playersBySlot[player.PlayerSlot]);
        PlayerStateSnapshot state = _simulation.CreatePlayerStateSnapshot(
          _playersBySlot[player.PlayerSlot]);
        frames.AddRange(PlayerStateProjection.CreateFrames(
          player.PlayerSlot,
          snapshot,
          state));
      }

      try
      {
        QueueOutboundFrames(recipient.Key, frames);
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(recipient.Key, recipient.Value));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private async Task ReplicateInventoryStatesAsync(CancellationToken cancellationToken)
  {
    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      if (!entry.Value.IsActive ||
          !_playersBySlot.TryGetValue(entry.Key, out PlayerHandle player))
      {
        continue;
      }

      Terraria.Dome.Simulation.Items.Snapshots.InventorySnapshot snapshot =
        _simulation.CreateInventorySnapshot(player);
      if (_inventoryRevisionsBySlot.TryGetValue(entry.Key, out long sentRevision) &&
          sentRevision == snapshot.Revision)
      {
        continue;
      }

      IReadOnlyList<byte[]> frames = _inventoryReplication.CollectFrames(entry.Key, snapshot);
      try
      {
        QueueOutboundFrames(entry.Key, frames);
        _inventoryRevisionsBySlot[entry.Key] = snapshot.Revision;
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, entry.Value));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private async Task ReplicateEquipmentStatesAsync(CancellationToken cancellationToken)
  {
    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      if (!entry.Value.IsActive ||
          !_playersBySlot.TryGetValue(entry.Key, out PlayerHandle player))
      {
        continue;
      }

      Terraria.Dome.Simulation.Items.Snapshots.EquipmentSnapshot snapshot =
        _simulation.CreateEquipmentSnapshot(player);
      if (_equipmentRevisionsBySlot.TryGetValue(entry.Key, out long sentRevision) &&
          sentRevision == snapshot.Revision)
      {
        continue;
      }

      IReadOnlyList<byte[]> frames = _equipmentReplication.CollectFrames(
        entry.Key,
        snapshot,
        _simulation.CreateInventorySnapshot(player));
      try
      {
        QueueOutboundFrames(entry.Key, frames);
        _equipmentRevisionsBySlot[entry.Key] = snapshot.Revision;
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, entry.Value));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private async Task ReplicateCombatStatesAsync(CancellationToken cancellationToken)
  {
    IReadOnlyList<NpcReplicationSnapshot> npcs = _simulation.CreateNpcReplicationSnapshots();
    IReadOnlyList<ProjectileReplicationSnapshot> projectiles =
      _simulation.CreateProjectileReplicationSnapshots();
    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      if (!entry.Value.IsActive)
      {
        continue;
      }

      CombatReplicationBatch batch = _combatReplication.CollectBatch(
        entry.Value,
        npcs,
        projectiles);
      if (batch.Frames.Count == 0)
      {
        continue;
      }

      try
      {
        QueueOutboundFrames(entry.Key, batch.Frames);
        entry.Value.ConfirmCombatBatch(batch);
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, entry.Value));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private async Task ReplicateItemStatesAsync(CancellationToken cancellationToken)
  {
    IReadOnlyList<ItemReplicationSnapshot> items = _simulation.CreateItemReplicationSnapshots();
    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      if (!entry.Value.IsActive)
      {
        continue;
      }

      IReadOnlyList<byte[]> frames = _itemReplication.CollectFrames(entry.Value, items);
      if (frames.Count == 0)
      {
        continue;
      }

      try
      {
        QueueOutboundFrames(entry.Key, frames);
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, entry.Value));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private async Task ReplicateChestStatesAsync(CancellationToken cancellationToken)
  {
    IReadOnlyList<ChestSnapshot> chests = _simulation.CreateChestSnapshots();
    List<SessionRemovalRequest> failedSessions = new();
    foreach (ChestSnapshot chest in chests)
    {
      if (chest.Opener is not PlayerHandle opener ||
          !TryGetPlayerSlot(opener, out byte openerSlot) ||
          !_replicationBySlot.TryGetValue(openerSlot, out SessionReplicationState? state) ||
          !state.IsActive || !state.ShouldSendChest(chest))
      {
        continue;
      }

      List<byte[]> frames = new(ChestComponent.SlotCount);
      for (byte slot = 0; slot < ChestComponent.SlotCount; slot++)
      {
        frames.Add(TerrariaPacketCodec.EncodeChestItem(new ChestItemReplicationSnapshot(
          chest.ChestId,
          slot,
          chest.Slots[slot],
          openerSlot,
          chest.Revision)));
      }

      try
      {
        QueueOutboundFrames(openerSlot, frames);
        state.MarkChestSent(chest);
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(openerSlot, state));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private async Task ReplicateWorldRulesAsync(CancellationToken cancellationToken)
  {
    WorldRuleSnapshot snapshot = _simulation.CreateWorldRuleSnapshot();
    if (snapshot.TimeOfDay != 0)
    {
      return;
    }

    byte[] frame = TerrariaPacketCodec.EncodeWorldTime(snapshot);
    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      if (!entry.Value.IsActive ||
          !entry.Value.ShouldSendWorldRules(snapshot.Tick, intervalTicks: 1))
      {
        continue;
      }

      try
      {
        QueueOutboundFrames(entry.Key, [frame]);
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, entry.Value));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private async Task ReplicateSignStatesAsync(CancellationToken cancellationToken)
  {
    IReadOnlyList<SignSnapshot> signs = _simulation.CreateSignSnapshots();
    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      if (!entry.Value.IsActive || entry.Value.IsInitialVisibilityLocked)
      {
        continue;
      }

      List<byte[]> frames = new();
      List<SignSnapshot> sentSigns = new();
      for (int index = 0; index < signs.Count; index++)
      {
        SignSnapshot sign = signs[index];
        if (!entry.Value.VisibleSections.Contains(sign.Section) ||
            !entry.Value.ShouldSendSign(sign))
        {
          continue;
        }

        frames.Add(TerrariaPacketCodec.EncodeSignState(new SignReplicationSnapshot(
          sign.SignId,
          (short)sign.TileX,
          (short)sign.TileY,
          sign.Text,
          byte.MaxValue,
          SuppressOpenSign: true,
          sign.Revision)));
        sentSigns.Add(sign);
      }

      if (frames.Count == 0)
      {
        continue;
      }

      try
      {
        QueueOutboundFrames(entry.Key, frames);
        for (int index = 0; index < sentSigns.Count; index++)
        {
          entry.Value.MarkSignSent(sentSigns[index]);
        }
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, entry.Value));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private async Task ReplicateTileEntityStatesAsync(CancellationToken cancellationToken)
  {
    IReadOnlyList<TileEntityPersistentState> entities =
      _simulation.CreateTileEntitySnapshots();
    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      SessionReplicationState state = entry.Value;
      if (!state.IsActive || state.IsInitialVisibilityLocked)
      {
        continue;
      }

      List<byte[]> frames = new();
      HashSet<int> currentIds = new();
      for (int index = 0; index < entities.Count; index++)
      {
        TileEntityPersistentState entity = entities[index];
        if (entity.Type != 0 || entity.IsOpaque ||
            !state.VisibleSections.Contains(_simulation.WorldGrid.GetSectionCoordinates(
              entity.TileX,
              entity.TileY)))
        {
          continue;
        }

        currentIds.Add(entity.Id);
        if (state.ShouldSendTileEntity(entity))
        {
          frames.Add(TerrariaPacketCodec.EncodeTrainingDummyTileEntitySharing(entity));
        }
      }

      foreach (int entityId in state.SentTileEntityIds)
      {
        if (!currentIds.Contains(entityId))
        {
          frames.Add(TerrariaPacketCodec.EncodeTrainingDummyTileEntityRemoval(entityId));
        }
      }

      if (frames.Count == 0)
      {
        continue;
      }

      try
      {
        QueueOutboundFrames(entry.Key, frames);
        for (int index = 0; index < entities.Count; index++)
        {
          TileEntityPersistentState entity = entities[index];
          if (currentIds.Contains(entity.Id))
          {
            state.MarkTileEntitySent(entity);
          }
        }

        foreach (int entityId in new List<int>(state.SentTileEntityIds))
        {
          if (!currentIds.Contains(entityId))
          {
            state.MarkTileEntityRemoved(entityId);
          }
        }
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, state));
      }
    }

    RemoveFailedSessions(failedSessions);
    await Task.CompletedTask;
  }

  private async Task ReplicateDoorStatesAsync(CancellationToken cancellationToken)
  {
    IReadOnlyList<DoorSnapshot> doors = _simulation.CreateDoorSnapshots();
    List<DoorReplicationEvent> transitions = [.. _pendingDoorReplications];
    _pendingDoorReplications.Clear();
    HashSet<int> transitionedDoorIds = new();
    foreach (DoorReplicationEvent transition in transitions)
    {
      transitionedDoorIds.Add(transition.DoorId);
    }

    List<SessionRemovalRequest> failedSessions = new();
    foreach (KeyValuePair<byte, SessionReplicationState> entry in _replicationBySlot)
    {
      if (!entry.Value.IsActive)
      {
        continue;
      }

      List<byte[]> frames = new();
      for (int index = 0; index < doors.Count; index++)
      {
        DoorSnapshot door = doors[index];
        if (!entry.Value.VisibleSections.Contains(door.Section))
        {
          continue;
        }

        if (transitionedDoorIds.Contains(door.DoorId))
        {
          continue;
        }

        if (entry.Value.ShouldSendDoor(door) &&
            TryCreateDoorStateSnapshot(door, out DoorStateReplicationSnapshot snapshot))
        {
          frames.Add(TerrariaPacketCodec.EncodeDoorState(snapshot));
        }
      }

      foreach (DoorReplicationEvent transition in transitions)
      {
        if (!entry.Value.VisibleSections.Contains(transition.Section))
        {
          continue;
        }

        for (int index = 0; index < doors.Count; index++)
        {
          if (doors[index].DoorId == transition.DoorId)
          {
            _ = entry.Value.ShouldSendDoor(doors[index]);
            break;
          }
        }

        frames.Add(TerrariaPacketCodec.EncodeDoorState(transition.Snapshot));
      }

      if (frames.Count == 0)
      {
        continue;
      }

      try
      {
        QueueOutboundFrames(entry.Key, frames);
      }
      catch (IOException)
      {
        failedSessions.Add(new SessionRemovalRequest(entry.Key, entry.Value));
      }
    }

    RemoveFailedSessions(failedSessions);
  }

  private ChestSnapshot? FindChest(int chestId)
  {
    IReadOnlyList<ChestSnapshot> chests = _simulation.CreateChestSnapshots();
    for (int index = 0; index < chests.Count; index++)
    {
      if (chests[index].ChestId == chestId)
      {
        return chests[index];
      }
    }

    return null;
  }

  private static bool TryGetDoorTransition(DoorToggleAction action, out DoorTransition transition)
  {
    switch (action)
    {
      case DoorToggleAction.OpenDoor:
        transition = DoorTransition.OpenDoor;
        return true;
      case DoorToggleAction.CloseDoor:
        transition = DoorTransition.CloseDoor;
        return true;
      case DoorToggleAction.ShiftTrapdoor:
        transition = DoorTransition.CloseTrapdoor;
        return true;
      case DoorToggleAction.ShiftTrapdoorReverse:
        transition = DoorTransition.OpenTrapdoor;
        return true;
      case DoorToggleAction.OpenTallGate:
        transition = DoorTransition.OpenTallGate;
        return true;
      case DoorToggleAction.CloseTallGate:
        transition = DoorTransition.CloseTallGate;
        return true;
      default:
        transition = default;
        return false;
    }
  }

  private static bool TryCreateDoorStateSnapshot(
    DoorSnapshot door,
    out DoorStateReplicationSnapshot snapshot)
  {
    if (door.ObjectKind == DoorObjectKind.Door)
    {
      snapshot = new DoorStateReplicationSnapshot(
        door.IsOpen ? DoorToggleAction.OpenDoor : DoorToggleAction.CloseDoor,
        (short)door.TileX,
        (short)door.TileY,
        door.IsOpen);
      return true;
    }

    snapshot = default;
    return false;
  }

  private SignSnapshot? FindSign(int signId)
  {
    IReadOnlyList<SignSnapshot> signs = _simulation.CreateSignSnapshots();
    for (int index = 0; index < signs.Count; index++)
    {
      if (signs[index].SignId == signId)
      {
        return signs[index];
      }
    }

    return null;
  }

  private SignSnapshot? FindSignAt(short tileX, short tileY)
  {
    IReadOnlyList<SignSnapshot> signs = _simulation.CreateSignSnapshots();
    for (int index = 0; index < signs.Count; index++)
    {
      SignSnapshot sign = signs[index];
      if (sign.TileX == tileX && sign.TileY == tileY)
      {
        return sign;
      }
    }

    return null;
  }

  private bool TryGetPlayerSlot(PlayerHandle player, out byte playerSlot)
  {
    foreach (KeyValuePair<byte, PlayerHandle> entry in _playersBySlot)
    {
      if (entry.Value == player)
      {
        playerSlot = entry.Key;
        return true;
      }
    }

    playerSlot = default;
    return false;
  }

  private void RemoveDestroyedPlayerInputs(
    List<PlayerInput> inputs,
    DestroySessionPlayerCommand destroyPlayer)
  {
    if (!_replicationBySlot.TryGetValue(
          destroyPlayer.PlayerSlot,
          out SessionReplicationState? currentState) ||
        !ReferenceEquals(currentState, destroyPlayer.ReplicationState) ||
        !_playersBySlot.TryGetValue(destroyPlayer.PlayerSlot, out PlayerHandle player))
    {
      return;
    }

    _ = inputs.RemoveAll(input => input.Player == player);
  }

  private void RemoveFailedSessions(IReadOnlyList<SessionRemovalRequest> sessions)
  {
    for (int index = 0; index < sessions.Count; index++)
    {
      SessionRemovalRequest session = sessions[index];
      RemoveSessionPlayer(session.PlayerSlot, session.ReplicationState);
    }
  }

  private void RemoveSessionPlayer(byte playerSlot, SessionReplicationState replicationState)
  {
    if (!_replicationBySlot.TryGetValue(playerSlot, out SessionReplicationState? currentState) ||
        !ReferenceEquals(currentState, replicationState))
    {
      return;
    }

    _replicationBySlot.Remove(playerSlot);
    _inventoryRevisionsBySlot.Remove(playerSlot);
    _equipmentRevisionsBySlot.Remove(playerSlot);
    if (_playersBySlot.Remove(playerSlot, out PlayerHandle player))
    {
      _simulation.CloseChest(player);
      _ = _simulation.DestroyPlayer(player);
    }

    currentState.Dispose();
  }

  private static int ProjectWorldTimeToLegacy(double timeOfDay)
  {
    if (!double.IsFinite(timeOfDay) || timeOfDay < int.MinValue || timeOfDay > int.MaxValue)
    {
      throw new ArgumentOutOfRangeException(nameof(timeOfDay));
    }

    return checked((int)Math.Truncate(timeOfDay));
  }

  private void ThrowIfStarted()
  {
    if (_disposed)
    {
      throw new ObjectDisposedException(nameof(DomeServer));
    }

    if (_listener is not null)
    {
      throw new InvalidOperationException("Server is already started.");
    }
  }
}

internal sealed record SequencedTerrariaProtocolCommand(
  long Sequence,
  TerrariaProtocolCommand Command);
