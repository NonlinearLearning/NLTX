namespace Terraria.Network;

/// <summary>Owns authoritative social bubble IDs, anchor state and tick lifetimes.</summary>
public sealed class SocialEmoteBubbleStateOwner {
  public const byte PlayerAnchorKind = 1;
  public const int PlayerEmoteLifetimeTicks = 360;
  public const int PreviousBubbleFadeTicks = 6;
  public const int VanillaEmoteCount = 151;

  private readonly object _gate = new();
  private readonly Dictionary<int, SocialEmoteBubbleSnapshot> _bubbles = new();
  private readonly int[] _playerEmoteTicks = new int[byte.MaxValue];
  private int _nextBubbleId;

  public SocialEmoteBubbleSnapshot CreatePlayerBubble(byte playerSlot, byte emote) {
    if (playerSlot == byte.MaxValue) {
      throw new ArgumentOutOfRangeException(nameof(playerSlot));
    }
    if (emote >= VanillaEmoteCount) {
      throw new ArgumentOutOfRangeException(nameof(emote));
    }

    lock (_gate) {
      foreach (int previousId in _bubbles.Values
          .Where(bubble => bubble.AnchorKind == PlayerAnchorKind
              && bubble.AnchorId == playerSlot)
          .Select(bubble => bubble.BubbleId)
          .ToArray()) {
        SocialEmoteBubbleSnapshot previous = _bubbles[previousId];
        _bubbles[previousId] = previous with {
          Lifetime = PreviousBubbleFadeTicks
        };
      }

      int bubbleId = AllocateBubbleId();
      var bubble = new SocialEmoteBubbleSnapshot(bubbleId, PlayerAnchorKind, playerSlot,
          PlayerEmoteLifetimeTicks, PlayerEmoteLifetimeTicks, emote, null);
      _bubbles.Add(bubbleId, bubble);
      _playerEmoteTicks[playerSlot] = PlayerEmoteLifetimeTicks;
      return bubble;
    }
  }

  public bool UpsertAuthoritativeBubble(SocialEmoteBubbleSnapshot bubble) {
    if (bubble.AnchorKind > 2 || bubble.LifetimeStart == 0
        || (bubble.Emote == byte.MaxValue) != bubble.Metadata.HasValue) {
      return false;
    }
    if (bubble.AnchorKind == PlayerAnchorKind && bubble.AnchorId >= byte.MaxValue) {
      return false;
    }

    lock (_gate) {
      if (bubble.AnchorKind == PlayerAnchorKind) {
        FadePreviousPlayerBubbles(bubble.AnchorId, bubble.BubbleId);
        _playerEmoteTicks[bubble.AnchorId] = PlayerEmoteLifetimeTicks;
      }
      _bubbles[bubble.BubbleId] = bubble;
      return true;
    }
  }

  public bool RemoveBubble(int bubbleId, out SocialEmoteBubbleSnapshot removed) {
    lock (_gate) {
      return _bubbles.Remove(bubbleId, out removed);
    }
  }

  public bool TryGetBubble(int bubbleId, out SocialEmoteBubbleSnapshot bubble) {
    lock (_gate) {
      return _bubbles.TryGetValue(bubbleId, out bubble);
    }
  }

  public int GetPlayerEmoteTime(byte playerSlot) {
    if (playerSlot == byte.MaxValue) {
      throw new ArgumentOutOfRangeException(nameof(playerSlot));
    }

    lock (_gate) {
      return _playerEmoteTicks[playerSlot];
    }
  }

  public IReadOnlyList<SocialEmoteBubbleSnapshot> GetBubbles() {
    lock (_gate) {
      SocialEmoteBubbleSnapshot[] bubbles = _bubbles.Values
          .OrderBy(bubble => bubble.BubbleId)
          .ToArray();
      return Array.AsReadOnly(bubbles);
    }
  }

  /// <summary>
  /// Advances one world tick. Expired bubbles are removed locally; packet 91 clients
  /// perform the same lifetime countdown and do not require a removal packet at expiry.
  /// </summary>
  public IReadOnlyList<int> AdvanceTick() {
    lock (_gate) {
      var expired = new List<int>();
      foreach (SocialEmoteBubbleSnapshot bubble in _bubbles.Values.ToArray()) {
        if (bubble.Lifetime <= 1) {
          _bubbles.Remove(bubble.BubbleId);
          expired.Add(bubble.BubbleId);
        } else {
          _bubbles[bubble.BubbleId] = bubble with {
            Lifetime = (ushort)(bubble.Lifetime - 1)
          };
        }
      }

      for (int index = 0; index < _playerEmoteTicks.Length; index++) {
        if (_playerEmoteTicks[index] > 0) {
          _playerEmoteTicks[index]--;
        }
      }
      return Array.AsReadOnly(expired.ToArray());
    }
  }

  private void FadePreviousPlayerBubbles(ushort playerSlot, int currentBubbleId) {
    foreach (int previousId in _bubbles.Values
        .Where(bubble => bubble.BubbleId != currentBubbleId
            && bubble.AnchorKind == PlayerAnchorKind
            && bubble.AnchorId == playerSlot)
        .Select(bubble => bubble.BubbleId)
        .ToArray()) {
      SocialEmoteBubbleSnapshot previous = _bubbles[previousId];
      _bubbles[previousId] = previous with {
        Lifetime = PreviousBubbleFadeTicks
      };
    }
  }

  private int AllocateBubbleId() {
    int firstCandidate = _nextBubbleId;
    do {
      int candidate = _nextBubbleId;
      _nextBubbleId = unchecked(_nextBubbleId + 1);
      if (!_bubbles.ContainsKey(candidate)) {
        return candidate;
      }
    } while (_nextBubbleId != firstCandidate);

    throw new InvalidOperationException("No social bubble IDs are available.");
  }
}
