using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Terraria.IO;

namespace Terraria.WorldBuilding;

public class WorldGenConfiguration : GameConfiguration
{
	private readonly JObject _biomeRoot;

	private readonly JObject _passRoot;

	public WorldGenConfiguration(JObject configurationRoot)
		: base(configurationRoot)
	{
		_biomeRoot = ((JObject)configurationRoot["Biomes"]) ?? new JObject();
		_passRoot = ((JObject)configurationRoot["Passes"]) ?? new JObject();
	}

	public T CreateBiome<T>() where T : MicroBiome, new()
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenConfiguration.CreateBiome"))
	{
		return CreateBiome<T>(typeof(T).Name);
	}
	}
	public T CreateBiome<T>(string name) where T : MicroBiome, new()
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenConfiguration.CreateBiome"))
	{
		if (_biomeRoot.TryGetValue(name, out var value))
		{
			return value.ToObject<T>();
		}
		return new T();
	}
	}
	public static WorldGenConfiguration FromEmbeddedPath(string path)
{
	using (new global::Terraria.CallTracker("Terraria.WorldBuilding.WorldGenConfiguration.FromEmbeddedPath"))
	{
		using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
		using StreamReader streamReader = new StreamReader(stream);
		return new WorldGenConfiguration(JsonConvert.DeserializeObject<JObject>(streamReader.ReadToEnd()));
	}
	}}
