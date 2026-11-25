// Adapted from CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
using System.Numerics;

namespace Terraria.Share.Ai.Goap.Core;

public interface ITarget {
  Vector3 Position { get; }
  bool IsValid();
}
