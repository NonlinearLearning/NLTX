using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Terraria.Dome.Protocol.V1456.Dispatch;
using Terraria.Dome.Protocol.V1456.Compatibility;
using Terraria.Dome.Protocol.V1456.Isolation;
using Terraria.Dome.Protocol.V1456.Packets;
using Terraria.Dome.Protocol.V1456.Protocol;
using Terraria.Dome.Protocol.V1456.Session;
using Terraria.Dome.Server.Replication;
using Terraria.Dome.Simulation;
using Terraria.Dome.Simulation.Players;
using Terraria.Dome.Simulation.WorldModel;
using Terraria.Dome.Simulation.WorldObjects;

namespace Terraria.Dome.Server.Protocol;

internal sealed class TerrariaProtocolSessionHost
{
  private const int LengthPrefixLength = 2;
  private readonly TerrariaPacketDispatcher _dispatcher = new();
  private readonly Func<
    byte,
    int,
    int,
    PlayerPersistentState,
    SessionReplicationState,
    CancellationToken,
    Task<PlayerInitialProjection>> _createSessionPlayerAsync;
  private readonly Func<TerrariaProtocolCommand, bool> _enqueueCommand;
  private readonly Func<NetworkInboundEnvelope, bool> _enqueueInboundEnvelope;
  private readonly Func<byte, byte[], bool> _enqueueOutboundFrame;
  private readonly Func<int, int, CancellationToken, Task> _ensureInitialNpcsAsync;
  private readonly Func<IReadOnlyList<ChestSnapshot>> _createChestSnapshots;
  private readonly Func<IReadOnlyList<NpcHomeSnapshot>> _createNpcHomeSnapshots;
  private readonly Func<IReadOnlyList<NpcReplicationSnapshot>> _createNpcReplicationSnapshots;
  private readonly Func<IReadOnlyList<TileEntityPersistentState>> _createTileEntitySnapshots;
  private readonly Func<WorldJoinStateSnapshot> _createWorldJoinState;
  private readonly Func<LegacyWorldDataContext> _createWorldDataContext;
  private readonly Func<byte, IReadOnlyList<NpcProjectileCursorEntry>>
    _restoreNpcProjectileCursor;
  private readonly Func<string, IReadOnlyList<NpcProjectileCursorEntry>>
    _restoreNpcProjectileCursorByAccount;
  private readonly Action _recordSessionActivity;
  private readonly Func<TimeSpan> _sessionTimeout;
  private readonly Func<PlayerBootstrapState, CancellationToken, Task<PlayerPersistentState>>
    _resolvePlayerAccountAsync;
  private readonly WorldSectionReplication _worldReplication;

  public TerrariaProtocolSessionHost(
    Func<TerrariaProtocolCommand, bool> enqueueCommand,
    Func<NetworkInboundEnvelope, bool> enqueueInboundEnvelope,
    Func<byte, byte[], bool> enqueueOutboundFrame,
    Func<int, int, CancellationToken, Task> ensureInitialNpcsAsync,
    Func<IReadOnlyList<ChestSnapshot>> createChestSnapshots,
    Func<IReadOnlyList<NpcHomeSnapshot>> createNpcHomeSnapshots,
    Func<IReadOnlyList<NpcReplicationSnapshot>> createNpcReplicationSnapshots,
    Func<IReadOnlyList<TileEntityPersistentState>> createTileEntitySnapshots,
    Func<WorldJoinStateSnapshot> createWorldJoinState,
    Func<PlayerBootstrapState, CancellationToken, Task<PlayerPersistentState>>
      resolvePlayerAccountAsync,
    Func<
      byte,
      int,
      int,
      PlayerPersistentState,
      SessionReplicationState,
      CancellationToken,
      Task<PlayerInitialProjection>> createSessionPlayerAsync,
    WorldSectionReplication worldReplication,
    Func<LegacyWorldDataContext> createWorldDataContext,
    Func<byte, IReadOnlyList<NpcProjectileCursorEntry>> restoreNpcProjectileCursor,
    Func<string, IReadOnlyList<NpcProjectileCursorEntry>> restoreNpcProjectileCursorByAccount,
    Action recordSessionActivity,
    Func<TimeSpan> sessionTimeout)
  {
    _enqueueCommand = enqueueCommand;
    _enqueueInboundEnvelope = enqueueInboundEnvelope ??
      throw new ArgumentNullException(nameof(enqueueInboundEnvelope));
    _enqueueOutboundFrame = enqueueOutboundFrame ??
      throw new ArgumentNullException(nameof(enqueueOutboundFrame));
    _ensureInitialNpcsAsync = ensureInitialNpcsAsync ??
      throw new ArgumentNullException(nameof(ensureInitialNpcsAsync));
    _createChestSnapshots = createChestSnapshots ??
      throw new ArgumentNullException(nameof(createChestSnapshots));
    _createNpcHomeSnapshots = createNpcHomeSnapshots ??
      throw new ArgumentNullException(nameof(createNpcHomeSnapshots));
    _createNpcReplicationSnapshots = createNpcReplicationSnapshots ??
      throw new ArgumentNullException(nameof(createNpcReplicationSnapshots));
    _createTileEntitySnapshots = createTileEntitySnapshots ??
      throw new ArgumentNullException(nameof(createTileEntitySnapshots));
    _createWorldJoinState = createWorldJoinState ??
      throw new ArgumentNullException(nameof(createWorldJoinState));
    _createWorldDataContext = createWorldDataContext ??
      throw new ArgumentNullException(nameof(createWorldDataContext));
    _restoreNpcProjectileCursor = restoreNpcProjectileCursor ??
      throw new ArgumentNullException(nameof(restoreNpcProjectileCursor));
    _restoreNpcProjectileCursorByAccount = restoreNpcProjectileCursorByAccount ??
      throw new ArgumentNullException(nameof(restoreNpcProjectileCursorByAccount));
    _recordSessionActivity = recordSessionActivity ??
      throw new ArgumentNullException(nameof(recordSessionActivity));
    _sessionTimeout = sessionTimeout ?? throw new ArgumentNullException(nameof(sessionTimeout));
    _resolvePlayerAccountAsync = resolvePlayerAccountAsync;
    _createSessionPlayerAsync = createSessionPlayerAsync;
    _worldReplication = worldReplication ??
      throw new ArgumentNullException(nameof(worldReplication));
  }

  public async Task HandleAsync(
    TcpClient client,
    byte playerSlot,
    CancellationToken cancellationToken)
  {
    using NetworkStream stream = client.GetStream();
    byte[] helloFrame = await ReadFrameAsync(stream, cancellationToken)
      .WaitAsync(_sessionTimeout.Invoke(), cancellationToken);
    TerrariaSession session = new(playerSlot);
    byte[] response = session.AcceptHello(helloFrame);
    SessionReplicationState replicationState = new(
      (frame, writeCancellationToken) => stream.WriteAsync(frame, writeCancellationToken).AsTask());
    replicationState.RestoreNpcProjectileCursor(_restoreNpcProjectileCursor(playerSlot));
    await replicationState.WriteFramesAsync(
      [response, TerrariaPacketCodec.EncodeInitialNetModules()],
      cancellationToken);
    SpawnTileDataRequestPacket? spawnRequest = null;
    PlayerPersistentState? playerAccount = null;
    try
    {
      while (!cancellationToken.IsCancellationRequested)
      {
        try
        {
          byte[] frameBytes = await ReadFrameAsync(stream, cancellationToken)
            .WaitAsync(_sessionTimeout.Invoke(), cancellationToken);
          _recordSessionActivity.Invoke();
          TerrariaPacketDispatchResult result = _dispatcher.Dispatch(session, frameBytes);
          replicationState.SetContractCapabilities(session.ContractCapabilities);
          if (result.ResponseFrame is byte[] responseFrame)
          {
            if (!_enqueueOutboundFrame(playerSlot, responseFrame))
            {
              throw new IOException("The isolated network output queue is full.");
            }
          }

          if (result.SignOpenRequest is SignOpenRequestPacket signRequest)
          {
            TaskCompletionSource<SignReplicationSnapshot?> completion = new(
              TaskCreationOptions.RunContinuationsAsynchronously);
            if (!_enqueueCommand(new OpenSignCommand(
                  playerSlot,
                  signRequest,
                  replicationState,
                  completion)))
            {
              throw new IOException("The sign-open command queue is full.");
            }

            SignReplicationSnapshot? sign = await completion.Task.WaitAsync(cancellationToken);
            if (sign is SignReplicationSnapshot signResponse)
            {
              await replicationState.WriteFramesAsync(
                [TerrariaPacketCodec.EncodeSignState(signResponse)],
                cancellationToken);
            }
          }

          if (result.Outcome == TerrariaPacketDispatchOutcome.WorldDataRequested)
          {
            playerAccount = await _resolvePlayerAccountAsync(
              result.PlayerBootstrap ?? throw new InvalidDataException(
                "RequestWorldData did not include a player bootstrap state."),
              cancellationToken);
            replicationState.RestoreNpcProjectileCursor(
              _restoreNpcProjectileCursorByAccount(playerAccount.Uuid));
            byte[] worldData = TerrariaV1456Compatibility.EncodeWorldData(
              _createWorldDataContext());
            await replicationState.WriteFramesAsync([worldData], cancellationToken);
          }

          if (result.Outcome == TerrariaPacketDispatchOutcome.TileDataRequested)
          {
            SpawnTileDataRequestPacket request = TerrariaPacketCodec.DecodeSpawnTileData(frameBytes);
            await _ensureInitialNpcsAsync(request.SpawnX, request.SpawnY, cancellationToken);
            IReadOnlyList<byte[]> initialWorldStream = _worldReplication.CreateInitialWorldStream(
              request,
              replicationState,
              _createChestSnapshots(),
              _createTileEntitySnapshots());
            int initialSpawnIndex = initialWorldStream.Count - 1;
            List<byte[]> initialFrames = new(initialWorldStream.Count + 29)
            {
              TerrariaV1456Compatibility.EncodeWorldData(_createWorldDataContext())
            };
            for (int index = 0; index < initialSpawnIndex; index++)
            {
              initialFrames.Add(initialWorldStream[index]);
            }

            IReadOnlyList<NpcReplicationSnapshot> npcs = _createNpcReplicationSnapshots();
            for (int index = 0; index < npcs.Count; index++)
            {
              NpcReplicationSnapshot npc = npcs[index];
              if (!npc.IsActive)
              {
                continue;
              }

              initialFrames.Add(TerrariaPacketCodec.EncodeNpcReplication(npc));
              initialFrames.Add(TerrariaPacketCodec.EncodeNpcBuffs(npc));
              replicationState.MarkCombatNpcSent(npc);
            }

            initialFrames.AddRange(TerrariaPacketCodec.CreateDefaultJoinStateNetModules());
            WorldJoinStateSnapshot worldJoinState = _createWorldJoinState();
            initialFrames.Add(TerrariaPacketCodec.EncodeWorldBiomeTypes(worldJoinState));
            initialFrames.Add(TerrariaPacketCodec.EncodeTowerShieldStrengths(worldJoinState));
            initialFrames.Add(TerrariaPacketCodec.EncodeCavernMonsterTypes(worldJoinState));
            initialFrames.Add(TerrariaPacketCodec.EncodeAnglerQuest(worldJoinState));
            initialFrames.Add(initialWorldStream[initialSpawnIndex]);
            await replicationState.WriteFramesAsync(initialFrames, cancellationToken);
            spawnRequest = request;
            replicationState.LockInitialVisibility();

            session.MarkInitialWorldStreamSent();
          }

          if (result.Outcome == TerrariaPacketDispatchOutcome.SectionRequested)
          {
            RequestSectionPacket request = TerrariaPacketCodec.DecodeRequestSection(frameBytes);
            IReadOnlyList<byte[]> requestedSectionStream =
              _worldReplication.CreateRequestedSectionStream(
                request,
                replicationState,
                _createChestSnapshots(),
                _createTileEntitySnapshots());
            if (requestedSectionStream.Count > 0)
            {
              await replicationState.WriteFramesAsync(requestedSectionStream, cancellationToken);
            }
          }

          if (result.Outcome == TerrariaPacketDispatchOutcome.PlayerSpawnAccepted &&
              result.PlayerSpawn is PlayerSpawnPacket spawn)
          {
            if (spawnRequest is null)
            {
              throw new InvalidDataException("PlayerSpawn arrived without an initial section request.");
            }

            PlayerPersistentState account = playerAccount ?? throw new InvalidDataException(
              "PlayerSpawn arrived without a resolved player account.");
            WorldSpawnCoordinates spawnCoordinates = _worldReplication.ResolvePlayerSpawn(spawn);
            PlayerInitialProjection initialProjection = await _createSessionPlayerAsync(
              playerSlot,
              spawnCoordinates.TileX,
              spawnCoordinates.TileY,
              account,
              replicationState,
              cancellationToken);
            IReadOnlyList<NpcHomeSnapshot> npcHomes = _createNpcHomeSnapshots();
            List<byte[]> completionFrames = new(npcHomes.Count + 16);
            for (int index = 0; index < npcHomes.Count; index++)
            {
              completionFrames.Add(TerrariaPacketCodec.EncodeNpcHome(npcHomes[index]));
            }

            completionFrames.Add(PlayerPersistentStateMapper.ToProfileFrame(playerSlot, account));
            completionFrames.AddRange(PlayerBootstrapProjection.CreateFrames(playerSlot, account));
            completionFrames.AddRange(CreateInitialPlayerStateFrames(playerSlot, initialProjection));
            completionFrames.Add(TerrariaPacketCodec.EncodeHostStatus(playerSlot, isHost: true));
            completionFrames.AddRange(TerrariaPacketCodec.CreateJoinGreetingNetModules(
              account.Profile.Name));
            completionFrames.Add(TerrariaPacketCodec.EncodeFinishedConnectingToServer());
            await replicationState.WriteFramesAsync(
              completionFrames,
              cancellationToken);
            replicationState.MarkActive();
          }

          if (result.PlayerControls is PlayerControlIntent controls)
          {
            replicationState.UnlockVisibility();
            if (result.LegacyPlayerControls is LegacyPlayerControlsState legacyControls)
            {
              replicationState.SetClientViewPosition(new SimulationVector(
                legacyControls.Position.X / TerrariaWorldCoordinates.PixelsPerTile,
                legacyControls.Position.Y / TerrariaWorldCoordinates.PixelsPerTile));
            }

            EnqueueInboundFrame(playerSlot, frameBytes);
          }

          if (result.TileManipulation is TileManipulationIntent manipulation)
          {
            EnqueueInboundFrame(playerSlot, frameBytes);
          }

          if (result.TileEntityPlacement is TileEntityPlacementIntent)
          {
            EnqueueInboundFrame(playerSlot, frameBytes);
          }

          if (result.ChestOpen is ChestOpenIntent chestOpen)
          {
            EnqueueInboundFrame(playerSlot, frameBytes);
          }

          if (result.DoorToggle is DoorToggleIntent doorToggle)
          {
            EnqueueInboundFrame(playerSlot, frameBytes);
          }

          if (result.SignUpdate is SignUpdateIntent signUpdate)
          {
            EnqueueInboundFrame(playerSlot, frameBytes);
          }

          if (result.ChestTransfer is ChestTransferIntent chestTransfer)
          {
            EnqueueInboundFrame(playerSlot, frameBytes);
          }

          if (result.AddPlayerBuffPvp is AddPlayerBuffPvpPacket)
          {
            EnqueueInboundFrame(playerSlot, frameBytes);
          }
        }
        catch (EndOfStreamException)
        {
          return;
        }
      }
    }
    finally
    {
      _ = _enqueueCommand(new DestroySessionPlayerCommand(
        playerSlot,
        replicationState,
        playerAccount?.Uuid));
    }
  }

  private void EnqueueInboundFrame(byte playerSlot, ReadOnlySpan<byte> frameBytes)
  {
    NetworkInboundEnvelope envelope = NetworkInboundEnvelope.FromFrame(playerSlot, frameBytes);
    _ = _enqueueInboundEnvelope(envelope);
  }

  private static IReadOnlyList<byte[]> CreateInitialPlayerStateFrames(
    byte playerSlot,
    PlayerInitialProjection projection)
  {
    return
    [
      TerrariaPacketCodec.EncodePlayerActive(playerSlot, projection.IsActive),
      TerrariaPacketCodec.EncodePlayerControls(
        new PlayerControlIntent(
          playerSlot,
          MoveLeft: false,
          MoveRight: false,
          Jump: false,
          UseItem: false,
          FacingRight: projection.IsFacingRight,
          SelectedItem: 0),
        TerrariaWorldCoordinates.ToPixels(projection.PositionX),
        TerrariaWorldCoordinates.ToPixels(projection.PositionY))
    ];
  }

  private static async Task<byte[]> ReadFrameAsync(
    NetworkStream stream,
    CancellationToken cancellationToken)
  {
    byte[] lengthPrefix = new byte[LengthPrefixLength];
    await stream.ReadExactlyAsync(lengthPrefix, cancellationToken);
    int frameLength = lengthPrefix[0] | lengthPrefix[1] << 8;
    if (frameLength < TerrariaProtocolVersion.MinimumFrameLength)
    {
      throw new InvalidDataException("Terraria frame declares an invalid length.");
    }

    byte[] frameBytes = new byte[frameLength];
    lengthPrefix.CopyTo(frameBytes, 0);
    await stream.ReadExactlyAsync(
      frameBytes.AsMemory(LengthPrefixLength),
      cancellationToken);
    _ = TerrariaFrameCodec.Decode(frameBytes);
    return frameBytes;
  }

}
