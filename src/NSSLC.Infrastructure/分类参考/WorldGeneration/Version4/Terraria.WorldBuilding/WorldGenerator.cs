#define TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using ReLogic.Threading;
using Terraria.GameContent.UI.States;
using Terraria.Testing;
using Terraria.Utilities;

namespace Terraria.WorldBuilding;

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
		using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenerator.Controller.GetSnapshot"))
		{
			if (!_snapshots.TryGetValue(pass, out var value))
			{
				return null;
			}
			return value;
		}
		}
		public Controller(WorldManifest prevManifest = null)
		{
			_previousManifest = prevManifest;
			PauseOnHashMismatch = true;
			SnapshotFrequency = SnapshotFrequency.None;
		}

		internal void SetGenerator(WorldGenerator generator){}
		internal void OnPaused(){}
		internal void OnPassCompleted(){}
		public void DeleteSnapshot(GenPass pass)
{
		using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenerator.Controller.DeleteSnapshot"))
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
		}
		public void DeleteAllSnapshots()
{
		using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenerator.Controller.DeleteAllSnapshots"))
		{
			TryOperateInControlLock(delegate
			{
				_snapshots.Clear();
				WorldGenSnapshot.DeleteAllForCurrentWorld();
			});
		}
		}
		public void ForceUpdateProgress()
{
		using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenerator.Controller.ForceUpdateProgress"))
		{
			GenerationProgress progress = _generator._progress;
			progress.Message = ((PassResults.Count == 0) ? "World Cleared" : ("Paused after " + Passes[PassResults.Count - 1].Name));
			progress.TotalWeight = Passes.Where((GenPass p) => p.Enabled).Sum((GenPass p) => p.Weight);
			progress.TotalWeightedProgress = (from p in Passes.Take(PassResults.Count)
				where p.Enabled
				select p).Sum((GenPass p) => p.Weight);
		}
		}
		public bool TryOperateInControlLock(Action action)
{
		using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenerator.Controller.TryOperateInControlLock"))
		{
			return Utils.TryOperateInLock(_generator._controlLock, action);
		}
		}
		public bool TryCreateSnapshot()
{
		using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenerator.Controller.TryCreateSnapshot"))
		{
			return TryOperateInControlLock(delegate
			{
				if (!WorldGen.Manifest.FinalHash.HasValue)
				{
					Main.NewText("Pass was not run with worldgen debugging enabled, please re-run", 240, 30, 30);
				}
				else
				{
					if (WorldGen.Manifest.FinalHash == HashWorld())
					{
						try
						{
							_snapshots[LastCompletedPass] = WorldGenSnapshot.Create();
							return;
						}
						catch (Exception ex)
						{
							ReportException("Exception occured while creating snapshot", ex);
							return;
						}
					}
					Main.NewText("World has been modified since last gen pass completed. Please rerun or use /snapshot instead", 240, 30, 30);
				}
			});
		}
		}
		public bool TryReset()
{
		using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenerator.Controller.TryReset"))
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
		}
		private void UpdatePreviousManifest(){}
		public bool TryResetToSnapshot(GenPass pass)
{
		using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenerator.Controller.TryResetToSnapshot"))
		{
			WorldGenSnapshot snap = GetSnapshot(pass);
			if (snap == null || snap.Outdated)
			{
				return false;
			}
			return TryOperateInControlLock(delegate
			{
				try
				{
					UpdatePreviousManifest();
					snap.Restore();
					ForceUpdateProgress();
				}
				catch (Exception ex)
				{
					ReportException("Exception occured while restoring snapshot", ex);
				}
			});
		}
		}
		public bool TryRunToEndOfPass(GenPass pass, bool useSnapshots = true, bool mustRunPass = true)
{
		using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenerator.Controller.TryRunToEndOfPass"))
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
		}
		internal void ReportException(string message, Exception ex = null){}	}

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
	public bool GenerateWorld()
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenerator.GenerateWorld"))
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
	}
	private static void SetDebugWorldGenUIVisibility(bool visible){}
	private GenPassResult RunPass(GenPass pass){
  return new GenPassResult();
}
	public static uint HashWorld()
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenerator.HashWorld"))
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
	}
	}}
