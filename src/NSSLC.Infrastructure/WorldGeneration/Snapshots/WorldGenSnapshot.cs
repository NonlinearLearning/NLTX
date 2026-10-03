#define TRACE
using NSSLC.WorldGeneration.Testing;
using NSSLC.WorldGeneration.Utilities;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NSSLC.WorldGeneration.WorldBuilding;

public class WorldGenSnapshot
{
	[JsonConverter(typeof(SnapshotGenVars))]
	private class SnapshotGenVars : JsonConverter
	{
		public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
		{
			ContractResolver = new EasyDeserializationJsonContractResolver(),
			PreserveReferencesHandling = PreserveReferencesHandling.Objects,
			ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
			TypeNameHandling = TypeNameHandling.Auto
		};

		private static Dictionary<string, MemberInfo> fieldsAndProperties = (from m in ((IEnumerable<MemberInfo>)typeof(GenVars).GetFields(BindingFlags.Static | BindingFlags.Public)).Concat((IEnumerable<MemberInfo>)typeof(GenVars).GetProperties(BindingFlags.Static | BindingFlags.Public))
			where !(m is PropertyInfo) || ((PropertyInfo)m).CanWrite
			where !(m is FieldInfo) || !((FieldInfo)m).IsInitOnly
			where !m.GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true).Any()
			select m).ToDictionary((MemberInfo m) => m.Name);

		public static string Serialize()
{
			return JsonConvert.SerializeObject(new SnapshotGenVars(), SerializerSettings);
		}
		public static void DeserializeAndApply(string json)
{
			JsonConvert.DeserializeObject<SnapshotGenVars>(json, SerializerSettings);
		}
		public override bool CanConvert(Type objectType){
  return new bool ();
}
		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer
  serializer){
  return new object ();
}
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer){}
}

	private int _dataOffset;

	private List<GenPass> _matchingPasses;

	private static string SnapshotFolderSuffix = "_gensnapshots";

	private static string Extension = ".gensnapshot";

	private static IDictionary<string, long> _snapshotSizeCache = new Dictionary<string, long>();

	public WorldManifest Manifest { get; private set; }

	private string Path { get; set; }

	private string GenVarsJson { get; set; }

	public List<GenPassResult> GenPassResults => Manifest.GenPassResults;

	public bool Outdated
	{
		get
		{
			if (!(Manifest.GitSHA != GitStatus.GitSHA) && !(Manifest.Version != Main.versionNumber))
			{
				return !_matchingPasses.Zip(GenPassResults, (GenPass p, GenPassResult r) => p.Enabled == !r.Skipped).All((bool x) => x);
			}
			return true;
		}
	}

	private static string PathForActiveWorld => System.IO.Path.ChangeExtension(Main.ActiveWorldFileData.Path, null) + SnapshotFolderSuffix;

	public override string ToString(){
  return default;
}
	public static void DeleteAllForCurrentWorld()
{
		if (Directory.Exists(PathForActiveWorld))
		{
			try
			{
				Directory.Delete(PathForActiveWorld, recursive: true);
			}
			catch (Exception)
			{
			}
		}
		_snapshotSizeCache.Clear();
	}
	public static WorldGenSnapshot Create()
{
		WorldGenSnapshot worldGenSnapshot = new WorldGenSnapshot
		{
			Manifest = WorldGen.Manifest.Clone(),
			GenVarsJson = SnapshotGenVars.Serialize()
		};
		worldGenSnapshot._matchingPasses = WorldGenerator.CurrentController.Passes.GetRange(0, worldGenSnapshot.GenPassResults.Count);
		worldGenSnapshot.Path = System.IO.Path.Combine(PathForActiveWorld, string.Concat(worldGenSnapshot, Extension));
		if (!Directory.Exists(PathForActiveWorld))
		{
			Directory.CreateDirectory(PathForActiveWorld);
		}
		TileSnapshot.Create(worldGenSnapshot);
		using BinaryWriter binaryWriter = new BinaryWriter(File.Create(worldGenSnapshot.Path));
		binaryWriter.Write(worldGenSnapshot.Manifest.Serialize());
		binaryWriter.Write(worldGenSnapshot.GenVarsJson);
		worldGenSnapshot._dataOffset = (int)binaryWriter.BaseStream.Position;
		TileSnapshot.Save(binaryWriter);
		_snapshotSizeCache[worldGenSnapshot.Path] = binaryWriter.BaseStream.Length;
		return worldGenSnapshot;
	}
	public static void Delete(WorldGenSnapshot snap)
{
		try
		{
			File.Delete(snap.Path);
		}
		catch (Exception)
		{
		}
		_snapshotSizeCache.Remove(snap.Path);
	}
	public void Load()
{
		if (TileSnapshot.Context == this)
		{
			return;
		}
		using BinaryReader binaryReader = new BinaryReader(File.OpenRead(Path));
		binaryReader.BaseStream.Seek(_dataOffset, SeekOrigin.Current);
		TileSnapshot.Load(binaryReader, this);
	}
	public void Restore()
{
		Load();
		WorldGen.RestoreTemporaryStateChanges();
		WorldGen.Reset();
		WorldGen.Manifest = Manifest.Clone();
		SnapshotGenVars.DeserializeAndApply(GenVarsJson);
		TileSnapshot.Restore();
		NPC[] npc = Main.npc;
		for (int i = 0; i < npc.Length; i++)
		{
			npc[i].active = false;
		}
		Main.NewText("Restored " + this, byte.MaxValue, byte.MaxValue, 0);
	}}
