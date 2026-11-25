// Based on CrashKonijn/GOAP 7fb29cfe59dc61aa8cf1f69880d3ef95fc29126c.
// Licensed under Apache-2.0; see LICENSE.txt and NOTICE.txt.
namespace Terraria.Share.Ai.Goap.Core;

public interface IActionData {
  ITarget? Target { get; set; }
}
