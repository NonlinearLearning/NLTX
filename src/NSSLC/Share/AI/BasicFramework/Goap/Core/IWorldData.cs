// Adapted from CrashKonijn/GOAP WorldDataBase; see LICENSE.txt and SOURCES.md.
namespace Terraria.Share.Ai.Goap.Core;

/// <summary>Sensor snapshot. Missing values do not satisfy conditions.</summary>
public interface IWorldData {
  bool IsTrue(ICondition condition);
}
