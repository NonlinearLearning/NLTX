using System;
using System.Collections.Generic;
using System.Linq;
using NSSLC.WorldGeneration.WorldBuilding;

namespace NSSLC.WorldGeneration;

/// <summary>
/// Owns a completed generation buffer. Later generations allocate a new buffer.
/// Consumers read value copies and commit them through their own world owner.
/// </summary>
public sealed class GeneratedWorld {
  private readonly Tile[,] _tiles;

  public string Seed { get; }
  public int WorldId { get; }
  public int Width => _tiles.GetLength(0);
  public int Height => _tiles.GetLength(1);
  public int SpawnX { get; }
  public int SpawnY { get; }
  public double Surface { get; }
  public double RockLayer { get; }
  public bool Crimson { get; }
  public IReadOnlyList<GeneratedChest> Chests { get; }
  public IReadOnlyList<GeneratedNpc> Npcs { get; }
  public IReadOnlyList<CompletedGenerationPass> Passes { get; }
  public GeneratedWorldSettings Settings { get; }
  public IReadOnlyList<bool> FrameImportant { get; }
  public IReadOnlyList<bool> CompressionBatching { get; }
  public IReadOnlyList<GeneratedSign> Signs { get; }
  public IReadOnlyList<GeneratedTileEntity> TileEntities { get; }
  public string ManifestJson { get; }

  internal GeneratedWorld() {
    _tiles = Main.tile;
    Seed = Main.ActiveWorldFileData.SeedText;
    WorldId = Main.ActiveWorldFileData.WorldId;
    SpawnX = Main.spawnTileX;
    SpawnY = Main.spawnTileY;
    Surface = Main.worldSurface;
    RockLayer = Main.rockLayer;
    Crimson = WorldGen.crimson;
    Settings = new GeneratedWorldSettings();
    FrameImportant = Array.AsReadOnly(Main.tileFrameImportant.ToArray());
    CompressionBatching = Array.AsReadOnly(ID.TileID.Sets.AllowsSaveCompressionBatching.ToArray());
    Signs = Array.AsReadOnly(Main.sign.Where(sign => sign is not null && sign.text is not null)
        .Select(sign => new GeneratedSign(sign.x, sign.y, sign.text)).ToArray());
    TileEntities = TileEntity.CreateSnapshot();
    ManifestJson = WorldGen.Manifest.Serialize();
    Chests = Array.AsReadOnly(Main.chest.Where(chest => chest != null)
      .Select(chest => new GeneratedChest(chest.x, chest.y, chest.name ?? "",
        Array.AsReadOnly(chest.item.Select(item =>
          new GeneratedItem(item.type, item.stack, item.prefix)).ToArray())))
      .ToArray());
    Npcs = Array.AsReadOnly(Main.npc.Where(npc => npc.active)
      .Select(npc => new GeneratedNpc(npc.type, npc.GivenName ?? "", npc.position.X,
        npc.position.Y, npc.homeTileX, npc.homeTileY, npc.homeless,
        npc.townNPC, npc.townNpcVariationIndex, npc.homelessDespawn)).ToArray());
    Passes = Array.AsReadOnly(WorldGenerator.PassResults
      .Select(pass => new CompletedGenerationPass(pass.Name, pass.DurationMs, pass.Skipped,
        pass.RandNext))
      .ToArray());
  }

  public GeneratedTile GetTile(int x, int y) {
    Tile tile = _tiles[x, y];
    return new GeneratedTile(tile.type, tile.wall, tile.liquid, tile.frameX, tile.frameY,
                             tile.sTileHeader, tile.bTileHeader, tile.bTileHeader2,
                             tile.bTileHeader3);
  }
}

public sealed record GeneratedItem(int Type, int Stack, byte Prefix);
public sealed record GeneratedChest(int X, int Y, string Name, IReadOnlyList<GeneratedItem> Items);
public sealed record GeneratedNpc(int Type, string Name, float X, float Y,
                                 int HomeX, int HomeY, bool Homeless,
                                 bool IsTownNpc = true, int Variation = 0,
                                 bool HomelessDespawn = false);
public sealed record GeneratedSign(int X, int Y, string Text);
public sealed record GeneratedTileEntity(int Id, int X, int Y, int TileType);
public sealed record CompletedGenerationPass(string Name, int DurationMs, bool Skipped,
                                            int RandNext);
