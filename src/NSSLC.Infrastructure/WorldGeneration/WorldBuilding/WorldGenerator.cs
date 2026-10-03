#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NSSLC.WorldGeneration.Runtime;
using NSSLC.WorldGeneration.GameContent.UI.States;
using NSSLC.WorldGeneration.Testing;
using NSSLC.WorldGeneration.Utilities;

namespace NSSLC.WorldGeneration.WorldBuilding;

public class WorldGenerator
{
	public enum SnapshotFrequency
	{
		None = -1,
		Manual,
		Automatic,
		Always
	}

	public class Controller
	{
		private WorldManifest _previousManifest;

		private Dictionary<GenPass, WorldGenSnapshot> _snapshots;

		public Action<Controller> OnPassesLoaded;

    public Action<string> PassStarting;

		private WorldGenerator _generator;

		private bool _paused;

		public List<GenPass> Passes => _generator._passes;

		public GenPass CurrentPass => _generator._currentPass;

		public GenPass LastCompletedPass
		{
			get
			{
				if (PassResults.Count != 0)
				{
					return Passes[PassResults.Count - 1];
				}
				return null;
			}
		}

		public GenPass PauseAfterPass { get; set; }

		public bool PauseOnHashMismatch { get; set; }

		public bool PausedDueToHashMismatch { get; set; }

		public SnapshotFrequency SnapshotFrequency { get; set; }

		public bool Paused
		{
			get
			{
				return _paused;
			}
			set
			{
				_paused = value;
				if (value)
				{
					PauseAfterPass = null;
				}
				else
				{
					PausedDueToHashMismatch = false;
				}
			}
		}

		public bool QueuedAbort { get; set; }

		public WorldGenSnapshot GetSnapshot(GenPass pass)
{
		
			if (!_snapshots.TryGetValue(pass, out var value))
			{
				return null;
			}
			return value;
		
		}
		public Controller(WorldManifest prevManifest = null)
		{
			_previousManifest = prevManifest;
			PauseOnHashMismatch = true;
			SnapshotFrequency = SnapshotFrequency.None;
		}

		internal void SetGenerator(WorldGenerator generator)
{
    _generator = generator;
    _snapshots = new Dictionary<GenPass, WorldGenSnapshot>();
    OnPassesLoaded?.Invoke(this);
  }
		internal void OnPaused()
{
    Thread.Sleep(10);
  }
		internal void OnPassCompleted()
{
    if (PauseAfterPass == CurrentPass) {
      Paused = true;
    }
  }
		private void CheckLatestPassResultAgainstManifest(int currentPassIndex, GenPassResult result, WorldGenSnapshot prevSnapshot)
{
    throw new NotSupportedException("Disk snapshot operations require a snapshot storage adapter.");
  }
		public void DeleteSnapshot(GenPass pass)
{
		
			Utils.TryOperateInLock(pass, delegate
			{
				if (_snapshots.TryGetValue(pass, out var value))
				{
					_snapshots.Remove(pass);
					WorldGenSnapshot.Delete(value);
				}
			});
		
		}
		public void DeleteAllSnapshots()
{
		
			TryOperateInControlLock(delegate
			{
				_snapshots.Clear();
				WorldGenSnapshot.DeleteAllForCurrentWorld();
			});
		
		}
		private int MsSinceLastSnapshot()
{
		
			int num = Passes.GetRange(0, PassResults.Count).FindLastIndex(_snapshots.ContainsKey);
			return PassResults.Skip(num + 1).Sum((GenPassResult r) => r.DurationMs);
		
		}
		public void ForceUpdateProgress()
{
		
			GenerationProgress progress = _generator._progress;
			progress.Message = ((PassResults.Count == 0) ? "World Cleared" : ("Paused after " + Passes[PassResults.Count - 1].Name));
			progress.TotalWeight = Passes.Where((GenPass p) => p.Enabled).Sum((GenPass p) => p.Weight);
			progress.TotalWeightedProgress = (from p in Passes.Take(PassResults.Count)
				where p.Enabled
				select p).Sum((GenPass p) => p.Weight);
		
		}
		public bool TryOperateInControlLock(Action action)
{
		
			return Utils.TryOperateInLock(_generator._controlLock, action);
		
		}
		public bool TryCreateSnapshot()
{
    throw new NotSupportedException("Disk snapshot operations require a snapshot storage adapter.");
  }
		public bool TryReset()
{
		
			return TryOperateInControlLock(delegate
			{
				UpdatePreviousManifest();
				WorldGen.RestoreTemporaryStateChanges();
				WorldGen.clearWorld();
				WorldGen.Reset();
				ForceUpdateProgress();
				Paused = true;
				Main.NewText("World Reset", byte.MaxValue, byte.MaxValue, 0);
			});
		
		}
		private void UpdatePreviousManifest()
{
		
			if (_previousManifest == null || PassResults.Count > _previousManifest.GenPassResults.Count)
			{
				_previousManifest = WorldGen.Manifest;
			}
		
		}
		public bool TryResetToSnapshot(GenPass pass)
{
    throw new NotSupportedException("Disk snapshot operations require a snapshot storage adapter.");
  }
		public bool TryRunToEndOfPass(GenPass pass, bool useSnapshots = true, bool mustRunPass = true)
{
		
			if (!pass.Enabled)
			{
				return false;
			}
			int passIndex = Passes.IndexOf(pass);
			if (TryOperateInControlLock(delegate
			{
				GenPass genPass = Passes.Take(passIndex + ((!mustRunPass) ? 1 : 0)).Reverse().FirstOrDefault((GenPass p) => GetSnapshot(p) != null && !GetSnapshot(p).Outdated);
				bool flag = passIndex < PassResults.Count;
				if (useSnapshots && genPass != null && (flag || Passes.IndexOf(genPass) >= PassResults.Count))
				{
					TryResetToSnapshot(genPass);
				}
				else if (flag)
				{
					TryReset();
				}
				if (PassResults.Count == passIndex + 1)
				{
					Paused = true;
				}
				else
				{
					PauseAfterPass = pass;
					Paused = false;
				}
			}))
			{
				return true;
			}
			if (pass == CurrentPass || passIndex > PassResults.Count)
			{
				PauseAfterPass = pass;
				return true;
			}
			return false;
		
		}
		public bool TryResetToPreviousPass(GenPass pass)
{
		
			int count = Passes.IndexOf(pass);
			GenPass genPass = Passes.Take(count).Reverse().FirstOrDefault((GenPass p) => p.Enabled);
			if (genPass == null)
			{
				return TryReset();
			}
			return TryRunToEndOfPass(genPass, useSnapshots: true, mustRunPass: false);
		
		}
		internal void ReportException(string message, Exception ex = null)
{
    throw new InvalidOperationException(message, ex);
  }	}

	internal readonly List<GenPass> _passes = new List<GenPass>();

	private readonly int _seed;

	private readonly WorldGenConfiguration _configuration;

	private readonly GenerationProgress _progress;

	private readonly Controller _controller;

	private readonly object _controlLock = new object();

	private GenPass _currentPass;

	public static GenerationProgress CurrentGenerationProgress;

	public static Controller CurrentController;

	private static Stopwatch _hashTime = new Stopwatch();

	public static List<GenPassResult> PassResults => WorldGen.Manifest.GenPassResults;

	public WorldGenerator(int seed, WorldGenConfiguration configuration, GenerationProgress progress = null, Controller controller = null)
	{
		_seed = seed;
		_configuration = configuration;
		_progress = ((progress == null) ? new GenerationProgress() : progress);
		_controller = ((controller == null) ? new Controller() : controller);
	}

	public void Append(GenPass pass)
{
	
		_passes.Add(pass);
	
	}
	public bool GenerateWorld()
{
	
		_hashTime.Reset();
		_controller.SetGenerator(this);
		CurrentController = _controller;
		_progress.TotalWeight = _passes.Where((GenPass p) => p.Enabled).Sum((GenPass p) => p.Weight);
		CurrentGenerationProgress = _progress;
		if (_controller.PauseAfterPass != null)
		{
			SetDebugWorldGenUIVisibility(visible: true);
		}
		bool flag = false;
		while (true)
		{
			if (_controller.QueuedAbort)
			{
				flag = true;
				break;
			}
			if (_controller.Paused)
			{
				_controller.OnPaused();
				continue;
			}
			lock (_controlLock)
			{
				if (PassResults.Count == _passes.Count)
				{
					break;
				}
				_currentPass = _passes[PassResults.Count];
				lock (_currentPass)
				{
					PassResults.Add(RunPass(_currentPass));
					_controller.OnPassCompleted();
				}
				_currentPass = null;
				continue;
			}
		}
		Trace.WriteLine(string.Join("\n", PassResults) + $"\nFinished world - Seed: {Main.ActiveWorldFileData.SeedText} Width: {Main.maxTilesX}, Height: {Main.maxTilesY}, Evil: {WorldGen.WorldGenParam_Evil}, Difficulty: {Main.GameMode}\nTotal Generation Time: {PassResults.Sum((GenPassResult r) => r.DurationMs)}\n");
		SetDebugWorldGenUIVisibility(visible: false);
		CurrentGenerationProgress = null;
		CurrentController = null;
		return !flag;
	
	}
	private static void SetDebugWorldGenUIVisibility(bool visible)
{

  }
	private GenPassResult RunPass(GenPass pass)
{
	
		if (!pass.Enabled)
		{
			return new GenPassResult
			{
				Name = pass.Name,
				Skipped = true
			};
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
    _controller.PassStarting?.Invoke(pass.Name);
		Main.rand = new UnifiedRandom(_seed);
		_progress.Start(pass.Weight);
		try
		{
			pass.Apply(_progress, _configuration.GetPassConfiguration(pass.Name));
		}
    catch (OperationCanceledException) {
      throw;
    }
		catch (Exception ex)
		{
			_controller.ReportException("Exception in Pass: " + pass.Name, ex);
		}
		_progress.End();
		return new GenPassResult
		{
			Name = pass.Name,
			DurationMs = (int)stopwatch.ElapsedMilliseconds,
			RandNext = WorldGen.genRand.Next()
		};
	
	}
	public static uint HashWorld()
{
	
		_hashTime.Start();
		uint[] line_hashes = new uint[Main.maxTilesX];
		FastParallel.For(0, Main.maxTilesX, delegate(int x0, int x1, object _)
		{
			Tile[,] tile = Main.tile;
			int maxTilesY = Main.maxTilesY;
			for (int i = x0; i < x1; i++)
			{
				uint num4 = 0u;
				for (int j = 0; j < maxTilesY; j++)
				{
					num4 ^= (uint)TileSnapshot.TileStruct.From(tile[i, j]).GetHashCode();
					num4 = (num4 << 13) | (num4 >> 19);
					num4 = num4 * 5 + 3864292196u;
				}
				line_hashes[i] = num4;
			}
		});
		uint num = 0u;
		uint[] array = line_hashes;
		foreach (uint num3 in array)
		{
			num ^= num3;
			num = (num << 13) | (num >> 19);
			num = num * 5 + 3864292196u;
		}
		_hashTime.Stop();
		return num;
	
	}}
