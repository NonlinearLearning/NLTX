using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;

internal static class ReferenceHarness
{
  private const BindingFlags InstanceFields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
  private const BindingFlags StaticFields = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

  private static int Main(string[] args)
  {
    try
    {
      return Run(args);
    }
    catch (Exception exception)
    {
      Console.Error.WriteLine(exception);
      return 1;
    }
  }

  private static int Run(string[] args)
  {
    if (args.Length != 8 || args[0] != "--assembly" || args[2] != "--cases" ||
        args[4] != "--output" || args[6] != "--sandbox")
    {
      throw new ArgumentException("Usage: ReferenceHarness.exe --assembly <exe> --cases <json> --output <json> --sandbox <directory>");
    }

    string assemblyPath = Path.GetFullPath(args[1]);
    string casesPath = Path.GetFullPath(args[3]);
    string outputPath = Path.GetFullPath(args[5]);
    string sandboxRoot = Path.GetFullPath(args[7]);
    Directory.CreateDirectory(sandboxRoot);
    JavaScriptSerializer serializer = new JavaScriptSerializer();
    serializer.MaxJsonLength = int.MaxValue;
    Dictionary<string, object> request = (Dictionary<string, object>)serializer.DeserializeObject(File.ReadAllText(casesPath));
    object[] cases = (object[])request["cases"];

    Assembly serverAssembly = Assembly.LoadFrom(assemblyPath);
    Type callTrackerType = RequiredType(serverAssembly, "Terraria.CallTracker");
    RuntimeHelpers.RunClassConstructor(callTrackerType.TypeHandle);
    FieldInfo timerField = RequiredField(callTrackerType, "FlushTimer", StaticFields);
    Timer callTrackerTimer = (Timer)timerField.GetValue(null);
    callTrackerTimer.Change(Timeout.Infinite, Timeout.Infinite);

    Type programType = RequiredType(serverAssembly, "Terraria.Program");
    RequiredField(programType, "SavePath", StaticFields).SetValue(null, sandboxRoot);
    RequiredField(programType, "LaunchParameters", StaticFields).SetValue(null, new Dictionary<string, string>());

    Type mainType = RequiredType(serverAssembly, "Terraria.Main");
    Type playerType = RequiredType(serverAssembly, "Terraria.Player");
    Type npcType = RequiredType(serverAssembly, "Terraria.NPC");
    Type npcIdType = RequiredType(serverAssembly, "Terraria.ID.NPCID");
    Type npcIdSetsType = npcIdType.GetNestedType("Sets", BindingFlags.Public | BindingFlags.NonPublic);
    if (npcIdSetsType == null)
    {
      throw new TypeLoadException("Terraria.ID.NPCID.Sets was not found.");
    }
    Type randomType = RequiredType(serverAssembly, "Terraria.Utilities.UnifiedRandom");
    FieldInfo playerArrayField = RequiredField(mainType, "player", StaticFields);
    Array players = (Array)playerArrayField.GetValue(null);
    for (int playerIndex = 0; playerIndex < 255; playerIndex++)
    {
      object candidate = Activator.CreateInstance(playerType);
      SetField(candidate, "active", false);
      SetField(candidate, "dead", false);
      SetField(candidate, "ghost", false);
      players.SetValue(candidate, playerIndex);
    }
    object player = Activator.CreateInstance(playerType);
    SetField(player, "active", true);
    SetField(player, "dead", false);
    SetField(player, "ghost", false);
    players.SetValue(player, 0);
    object npc = Activator.CreateInstance(npcType);
    FieldInfo mainRandomField = RequiredField(mainType, "rand", StaticFields);
    PropertyInfo gameModeProperty = mainType.GetProperty("GameMode", BindingFlags.Public | BindingFlags.Static);
    PropertyInfo expertProperty = mainType.GetProperty("expertMode", BindingFlags.Public | BindingFlags.Static);
    FieldInfo getGoodWorldField = RequiredField(mainType, "getGoodWorld", StaticFields);
    FieldInfo dayTimeField = RequiredField(mainType, "dayTime", StaticFields);
    FieldInfo remixWorldField = RequiredField(mainType, "remixWorld", StaticFields);
    FieldInfo timeField = RequiredField(mainType, "time", StaticFields);
    FieldInfo netModeField = RequiredField(mainType, "netMode", StaticFields);
    FieldInfo worldSurfaceField = RequiredField(mainType, "worldSurface", StaticFields);
    FieldInfo slimeRainField = RequiredField(mainType, "slimeRain", StaticFields);
    FieldInfo myPlayerField = RequiredField(mainType, "myPlayer", StaticFields);
    Array slimeCanContainItems = (Array)RequiredField(npcIdSetsType, "SlimeCanContainItems", StaticFields).GetValue(null);
    FieldInfo callTrackerQueueField = RequiredField(callTrackerType, "LogQueue", StaticFields);
    FieldInfo callTrackerMethodsField = RequiredField(callTrackerType, "LoggedMethods", StaticFields);
    FieldInfo npcGravityField = RequiredField(npcType, "gravity", StaticFields);
    MethodInfo aiMethod = npcType.GetMethod("AI", BindingFlags.Public | BindingFlags.Instance);
    MethodInfo slimeAiMethod = npcType.GetMethod("AI_001_Slimes", InstanceFields);
    if (aiMethod == null)
    {
      throw new MissingMethodException("Terraria.NPC.AI was not found.");
    }
    if (slimeAiMethod == null)
    {
      throw new MissingMethodException("Terraria.NPC.AI_001_Slimes was not found.");
    }

    byte[] il = aiMethod.GetMethodBody().GetILAsByteArray();
    byte[] slimeIl = slimeAiMethod.GetMethodBody().GetILAsByteArray();
    Dictionary<string, object> output = new Dictionary<string, object>();
    output["schemaVersion"] = 1;
    output["assemblyPath"] = assemblyPath;
    output["assemblyFullName"] = serverAssembly.FullName;
    output["assemblySha256"] = HashFile(assemblyPath);
    output["npcAiIlSha256"] = HashBytes(il);
    output["npcAiIlByteCount"] = il.Length;
    output["slimeAiIlSha256"] = HashBytes(slimeIl);
    output["slimeAiIlByteCount"] = slimeIl.Length;
    output["callTrackerFlushTimerDisabled"] = true;
    output["callTrackerFlushTimerDueTime"] = Timeout.Infinite;
    output["runtimeSideEffectSandbox"] = sandboxRoot;
    List<object> caseOutputs = new List<object>();

    for (int caseIndex = 0; caseIndex < cases.Length; caseIndex++)
    {
      Dictionary<string, object> scenario = (Dictionary<string, object>)cases[caseIndex];
      int ticks = AsInt(scenario, "ticks");
      int expectedRandomCalls = AsInt(scenario, "expectedRandomCalls");
      int requestedSeed = AsInt(scenario, "randomSeed");
      int seed = requestedSeed != 0 ? requestedSeed : FindSeed(randomType, ticks);
      object random = Activator.CreateInstance(randomType, new object[] { seed });
      mainRandomField.SetValue(null, random);

      bool expertMode = AsBool(scenario, "expertMode");
      bool getGoodWorld = AsBool(scenario, "getGoodWorld");
      getGoodWorldField.SetValue(null, getGoodWorld);
      gameModeProperty.SetValue(null, expertMode ? 1 : 0, null);
      bool actualExpertMode = (bool)expertProperty.GetValue(null, null);
      if (actualExpertMode != expertMode)
      {
        throw new InvalidOperationException("Requested expert mode did not resolve to the expected Main.expertMode value.");
      }

      dayTimeField.SetValue(null, AsBool(scenario, "dayTime"));
      remixWorldField.SetValue(null, false);
      timeField.SetValue(null, 30000.0);
      netModeField.SetValue(null, AsInt(scenario, "netMode"));
      worldSurfaceField.SetValue(null, AsDouble(scenario, "worldSurface"));
      slimeRainField.SetValue(null, AsBool(scenario, "slimeRain"));
      myPlayerField.SetValue(null, 0);
      npcGravityField.SetValue(null, 0.3f);
      SetPlayer(player, scenario);
      SetNpc(npc, scenario);

      object probe = Activator.CreateInstance(randomType, new object[] { seed });
      MethodInfo nextMethod = randomType.GetMethod("Next", new Type[] { typeof(int) });
      MethodInfo peekMethod = randomType.GetMethod("Peek", Type.EmptyTypes);
      if (expectedRandomCalls < 0 || expectedRandomCalls > 1)
      {
        throw new InvalidOperationException("This source harness currently supports zero or one expected UnifiedRandom draw per tick.");
      }
      List<object> tickOutputs = new List<object>();
      for (int tickIndex = 0; tickIndex < ticks; tickIndex++)
      {
        int peekBefore = (int)peekMethod.Invoke(probe, null);
        int dustRoll = expectedRandomCalls == 1 ? (int)nextMethod.Invoke(probe, new object[] { 5 }) : -1;
        if (expectedRandomCalls == 1 && dustRoll == 0)
        {
          throw new InvalidOperationException("The selected seed requested Dust.NewDust; the no-dust fixture was violated.");
        }

        if ((string)scenario["profile"] != "eye-of-cthulhu")
        {
          SetField(npc, "netUpdate", false);
        }
        SetField(npc, "oldTarget", (int)GetField(npc, "target"));
        SetField(npc, "oldDirection", (int)GetField(npc, "direction"));
        SetField(npc, "oldDirectionY", (int)GetField(npc, "directionY"));
        ClearCallTracker(callTrackerMethodsField, callTrackerQueueField);
        aiMethod.Invoke(npc, null);
        int probePeek = (int)peekMethod.Invoke(probe, null);
        int actualPeek = (int)peekMethod.Invoke(random, null);
        bool randomAdvancedAsExpected = probePeek == actualPeek;
        if (!randomAdvancedAsExpected)
        {
          throw new InvalidOperationException("Main.rand did not advance by the fixed draw count requested by this case.");
        }

        tickOutputs.Add(CaptureTick(
          npc,
          scenario,
          tickIndex + 1,
          dustRoll,
          expectedRandomCalls,
          randomAdvancedAsExpected,
          peekBefore,
          probePeek,
          slimeCanContainItems,
          (float)npcGravityField.GetValue(null),
          CaptureDeduplicatedFirstEntryOrder(callTrackerQueueField)));
      }

      Dictionary<string, object> caseOutput = new Dictionary<string, object>();
      caseOutput["id"] = scenario["id"];
      caseOutput["seed"] = seed;
      caseOutput["expertMode"] = expertMode;
      caseOutput["getGoodWorld"] = getGoodWorld;
      caseOutput["expectedRandomCalls"] = expectedRandomCalls;
      caseOutput["netMode"] = AsInt(scenario, "netMode");
      caseOutput["worldSurface"] = AsDouble(scenario, "worldSurface");
      caseOutput["sourceGravity"] = (float)npcGravityField.GetValue(null);
      caseOutput["ticks"] = tickOutputs;
      caseOutputs.Add(caseOutput);
    }

    output["cases"] = caseOutputs;
    string json = serializer.Serialize(output);
    Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
    File.WriteAllText(outputPath, json, new UTF8Encoding(false));
    Console.WriteLine("NPC AI reference capture complete: " + outputPath);
    Console.WriteLine("Assembly SHA-256: " + output["assemblySha256"]);
    Console.WriteLine("NPC.AI IL SHA-256: " + output["npcAiIlSha256"]);
    Console.WriteLine("Scenarios/ticks: " + cases.Length.ToString(CultureInfo.InvariantCulture) + "/" + CountTicks(caseOutputs).ToString(CultureInfo.InvariantCulture));
    Console.WriteLine("CallTracker flush timer disabled; selected seeds avoid Dust.NewDust.");
    return 0;
  }

  private static void SetPlayer(object player, Dictionary<string, object> scenario)
  {
    SetField(player, "active", true);
    SetField(player, "dead", false);
    SetField(player, "width", 20);
    SetField(player, "height", 42);
    SetVectorField(player, "position", BitsToFloat(AsInt(scenario, "targetXBits")), BitsToFloat(AsInt(scenario, "targetYBits")));
  }

  private static void SetNpc(object npc, Dictionary<string, object> scenario)
  {
    SetField(npc, "type", AsInt(scenario, "typeId"));
    SetField(npc, "netID", AsInt(scenario, "netId"));
    SetField(npc, "aiStyle", AsInt(scenario, "aiStyle"));
    SetField(npc, "whoAmI", 7);
    SetField(npc, "target", AsInt(scenario, "targetSlot"));
    SetField(npc, "direction", AsInt(scenario, "direction"));
    SetField(npc, "directionY", 1);
    SetField(npc, "width", AsInt(scenario, "width"));
    SetField(npc, "height", AsInt(scenario, "height"));
    SetField(npc, "life", AsInt(scenario, "life"));
    SetField(npc, "lifeMax", AsInt(scenario, "lifeMax"));
    SetField(npc, "defense", AsInt(scenario, "baseDefense"));
    SetField(npc, "defDefense", AsInt(scenario, "baseDefense"));
    SetField(npc, "value", BitsToFloat(AsInt(scenario, "valueBits")));
    SetField(npc, "wet", AsBool(scenario, "wet"));
    SetField(npc, "collideX", AsBool(scenario, "collideX"));
    SetField(npc, "collideY", AsBool(scenario, "collideY"));
    SetVectorField(npc, "oldVelocity", BitsToFloat(AsInt(scenario, "oldVelocityXBits")), BitsToFloat(AsInt(scenario, "oldVelocityYBits")));
    SetField(npc, "rotation", BitsToFloat(AsInt(scenario, "rotationBits")));
    SetField(npc, "netUpdate", false);
    SetField(npc, "reflectsProjectiles", true);
    SetField(npc, "active", true);
    SetVectorField(npc, "position", BitsToFloat(AsInt(scenario, "positionXBits")), BitsToFloat(AsInt(scenario, "positionYBits")));
    SetVectorField(npc, "velocity", BitsToFloat(AsInt(scenario, "velocityXBits")), BitsToFloat(AsInt(scenario, "velocityYBits")));
    float[] ai = (float[])GetField(npc, "ai");
    ai[0] = BitsToFloat(AsInt(scenario, "ai0Bits"));
    ai[1] = BitsToFloat(AsInt(scenario, "ai1Bits"));
    ai[2] = BitsToFloat(AsInt(scenario, "ai2Bits"));
    ai[3] = BitsToFloat(AsInt(scenario, "ai3Bits"));
      object[] localAiValues = (object[])scenario["localAiBits"];
      float[] localAi = (float[])GetField(npc, "localAI");
      for (int index = 0; index < localAiValues.Length; index++)
      {
        localAi[index] = BitsToFloat(Convert.ToInt32(localAiValues[index], CultureInfo.InvariantCulture));
      }
  }

  private static Dictionary<string, object> CaptureTick(
    object npc,
    Dictionary<string, object> scenario,
    int tick,
    int dustRoll,
    int expectedRandomCalls,
    bool randomAdvancedAsExpected,
    int randomPeekBefore,
    int randomPeekAfter,
    Array slimeCanContainItems,
    float sourceGravity,
    List<string> deduplicatedFirstEntryOrder)
  {
    float[] ai = (float[])GetField(npc, "ai");
    float[] localAi = (float[])GetField(npc, "localAI");
    object position = GetField(npc, "position");
    object velocity = GetField(npc, "velocity");
    Dictionary<string, object> result = new Dictionary<string, object>();
    result["tick"] = tick;
    result["randomRoll"] = dustRoll;
    result["randomAdvancedExactlyOnce"] = expectedRandomCalls == 1 && randomAdvancedAsExpected;
    result["randomAdvancedAsExpected"] = randomAdvancedAsExpected;
    result["expectedRandomCalls"] = expectedRandomCalls;
    result["randomPeekBefore"] = randomPeekBefore;
    result["randomPeekAfter"] = randomPeekAfter;
    result["deduplicatedFirstEntryOrder"] = deduplicatedFirstEntryOrder;
    result["typeId"] = (int)GetField(npc, "type");
    result["netId"] = (int)GetField(npc, "netID");
    result["aiStyle"] = (int)GetField(npc, "aiStyle");
    result["direction"] = (int)GetField(npc, "direction");
    result["targetSlot"] = (int)GetField(npc, "target");
    result["wet"] = (bool)GetField(npc, "wet");
    result["collideX"] = (bool)GetField(npc, "collideX");
    result["collideY"] = (bool)GetField(npc, "collideY");
    object oldVelocity = GetField(npc, "oldVelocity");
    result["oldVelocityXBits"] = FloatToBits(GetVectorCoordinate(oldVelocity, "X"));
    result["oldVelocityYBits"] = FloatToBits(GetVectorCoordinate(oldVelocity, "Y"));
    result["sourceCanContainItems"] = (bool)slimeCanContainItems.GetValue((int)GetField(npc, "type"));
    result["sourceGravityBits"] = FloatToBits(sourceGravity);
    result["defense"] = (int)GetField(npc, "defense");
    result["valueBits"] = FloatToBits((float)GetField(npc, "value"));
    result["ai0Bits"] = FloatToBits(ai[0]);
    result["ai1Bits"] = FloatToBits(ai[1]);
    result["ai2Bits"] = FloatToBits(ai[2]);
    result["ai3Bits"] = FloatToBits(ai[3]);
    result["velocityXBits"] = FloatToBits(GetVectorCoordinate(velocity, "X"));
    result["velocityYBits"] = FloatToBits(GetVectorCoordinate(velocity, "Y"));
    result["positionXBits"] = FloatToBits(GetVectorCoordinate(position, "X"));
    result["positionYBits"] = FloatToBits(GetVectorCoordinate(position, "Y"));
    int[] localAiBits = new int[localAi.Length];
    for (int index = 0; index < localAi.Length; index++)
    {
      localAiBits[index] = FloatToBits(localAi[index]);
    }
    result["localAiBits"] = localAiBits;
    result["rotationBits"] = FloatToBits((float)GetField(npc, "rotation"));
    result["target"] = (int)GetField(npc, "target");
    result["netUpdate"] = (bool)GetField(npc, "netUpdate");
    result["reflectsProjectiles"] = (bool)GetField(npc, "reflectsProjectiles");
    result["life"] = (int)GetField(npc, "life");
    result["lifeMax"] = (int)GetField(npc, "lifeMax");
    result["dayTime"] = AsBool(scenario, "dayTime");
    result["expertMode"] = AsBool(scenario, "expertMode");
    result["getGoodWorld"] = AsBool(scenario, "getGoodWorld");
    return result;
  }

  private static int FindSeed(Type randomType, int ticks)
  {
    ConstructorInfo constructor = randomType.GetConstructor(new Type[] { typeof(int) });
    MethodInfo next = randomType.GetMethod("Next", new Type[] { typeof(int) });
    for (int seed = 1; seed < 1000000; seed++)
    {
      object candidate = constructor.Invoke(new object[] { seed });
      bool safe = true;
      for (int tick = 0; tick < ticks; tick++)
      {
        if ((int)next.Invoke(candidate, new object[] { 5 }) == 0)
        {
          safe = false;
          break;
        }
      }
      if (safe)
      {
        return seed;
      }
    }
    throw new InvalidOperationException("No deterministic UnifiedRandom seed avoided the dust branch.");
  }

  private static int CountTicks(List<object> cases)
  {
    int count = 0;
    for (int index = 0; index < cases.Count; index++)
    {
      Dictionary<string, object> item = (Dictionary<string, object>)cases[index];
      count += ((List<object>)item["ticks"]).Count;
    }
    return count;
  }

  private static void ClearCallTracker(FieldInfo methodsField, FieldInfo queueField)
  {
    object loggedMethods = methodsField.GetValue(null);
    MethodInfo clearMethod = loggedMethods.GetType().GetMethod("Clear", Type.EmptyTypes);
    if (clearMethod == null)
    {
      throw new MissingMethodException(loggedMethods.GetType().FullName, "Clear");
    }
    clearMethod.Invoke(loggedMethods, null);
    DrainCallTrackerQueue(queueField);
  }

  private static List<string> CaptureDeduplicatedFirstEntryOrder(FieldInfo queueField)
  {
    object queue = queueField.GetValue(null);
    MethodInfo tryDequeue = queue.GetType().GetMethod("TryDequeue", BindingFlags.Public | BindingFlags.Instance);
    if (tryDequeue == null)
    {
      throw new MissingMethodException(queue.GetType().FullName, "TryDequeue");
    }

    List<string> result = new List<string>();
    object[] arguments = new object[] { null };
    while ((bool)tryDequeue.Invoke(queue, arguments))
    {
      string entry = (string)arguments[0];
      int marker = entry.IndexOf("[ENTER] ", StringComparison.Ordinal);
      if (marker >= 0)
      {
        result.Add(entry.Substring(marker + 8));
      }
      arguments[0] = null;
    }
    return result;
  }

  private static void DrainCallTrackerQueue(FieldInfo queueField)
  {
    object queue = queueField.GetValue(null);
    MethodInfo tryDequeue = queue.GetType().GetMethod("TryDequeue", BindingFlags.Public | BindingFlags.Instance);
    if (tryDequeue == null)
    {
      throw new MissingMethodException(queue.GetType().FullName, "TryDequeue");
    }

    object[] arguments = new object[] { null };
    while ((bool)tryDequeue.Invoke(queue, arguments))
    {
      arguments[0] = null;
    }
  }

  private static Type RequiredType(Assembly assembly, string name)
  {
    Type type = assembly.GetType(name, false);
    if (type == null)
    {
      throw new TypeLoadException("Required reference type not found: " + name);
    }
    return type;
  }

  private static FieldInfo RequiredField(Type type, string name, BindingFlags flags)
  {
    FieldInfo field = type.GetField(name, flags);
    if (field == null)
    {
      throw new MissingFieldException(type.FullName, name);
    }
    return field;
  }

  private static object GetField(object instance, string name)
  {
    FieldInfo field = RequiredField(instance.GetType(), name, InstanceFields);
    return field.GetValue(instance);
  }

  private static void SetField(object instance, string name, object value)
  {
    RequiredField(instance.GetType(), name, InstanceFields).SetValue(instance, value);
  }

  private static void SetVectorField(object instance, string fieldName, float x, float y)
  {
    FieldInfo field = RequiredField(instance.GetType(), fieldName, InstanceFields);
    Type vectorType = field.FieldType;
    object vector = Activator.CreateInstance(vectorType, new object[] { x, y });
    field.SetValue(instance, vector);
  }

  private static float GetVectorCoordinate(object vector, string name)
  {
    FieldInfo field = RequiredField(vector.GetType(), name, InstanceFields);
    return (float)field.GetValue(vector);
  }

  private static int AsInt(Dictionary<string, object> value, string name)
  {
    return Convert.ToInt32(value[name], CultureInfo.InvariantCulture);
  }

  private static double AsDouble(Dictionary<string, object> value, string name)
  {
    return Convert.ToDouble(value[name], CultureInfo.InvariantCulture);
  }

  private static bool AsBool(Dictionary<string, object> value, string name)
  {
    return Convert.ToBoolean(value[name], CultureInfo.InvariantCulture);
  }

  private static int FloatToBits(float value)
  {
    return BitConverter.ToInt32(BitConverter.GetBytes(value), 0);
  }

  private static float BitsToFloat(int bits)
  {
    return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
  }

  private static string HashFile(string path)
  {
    using (FileStream stream = File.OpenRead(path))
    using (SHA256 algorithm = SHA256.Create())
    {
      return ToHex(algorithm.ComputeHash(stream));
    }
  }

  private static string HashBytes(byte[] bytes)
  {
    using (SHA256 algorithm = SHA256.Create())
    {
      return ToHex(algorithm.ComputeHash(bytes));
    }
  }

  private static string ToHex(byte[] bytes)
  {
    StringBuilder builder = new StringBuilder(bytes.Length * 2);
    for (int index = 0; index < bytes.Length; index++)
    {
      builder.Append(bytes[index].ToString("x2", CultureInfo.InvariantCulture));
    }
    return builder.ToString();
  }
}
