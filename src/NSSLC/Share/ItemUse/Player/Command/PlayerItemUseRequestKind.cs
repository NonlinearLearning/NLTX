namespace Terraria.Share.Player;

/// <summary>Intentions a session may raise about using an item.</summary>
public enum PlayerItemUseRequestKind : byte
{
  /// <summary>Try to use the item in one inventory slot now.</summary>
  BeginUse = 0,

  /// <summary>Release a held channel item.</summary>
  ReleaseUse = 1
}
