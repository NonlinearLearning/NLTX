using System;

namespace Terraria.Npc;

public static class NpcDeathPhaseQuery
{
  public static NpcDeathPhaseDecision Evaluate(in NpcDeathPhaseInput input)
  {
    NpcDeathPhaseDecision decision = EvaluateCore(in input);
    if (!input.IsActive || !input.IsLifeOwner || input.CurrentLife > 0)
    {
      return decision;
    }

    if (input.NpcType.Value == 35 && input.Ai.State3 == 1.0f)
    {
      return decision with
      {
        AnnouncementIntent = new NpcDeathAnnouncementIntent(
          "SkeletronText.Taunt1",
          Red: 255,
          Green: 0,
          Blue: 0),
      };
    }

    if (input.NpcType.Value is 604 or 605)
    {
      return decision with
      {
        LadyBugKilledIntent = new NpcLadyBugKilledIntent(
          input.Center,
          GoldLadyBug: input.NpcType.Value == 605),
      };
    }

    if (input.NpcType.Value == 54)
    {
      return EvaluateClothierFollowUp(in input, decision);
    }

    return decision;
  }

  private static NpcDeathPhaseDecision EvaluateClothierFollowUp(
    in NpcDeathPhaseInput input,
    NpcDeathPhaseDecision decision)
  {
    if (input.ClothierSkeletronContext is not NpcDeathClothierSkeletronContext context)
    {
      return decision with
      {
        SuppressTerminalDeath = true,
        PhaseKind = NpcDeathPhaseKind.ClothierSkeletronInputMissing,
        RequiredInputMissing = true,
      };
    }

    if (context.IsDay || context.HasActiveSkeletron)
    {
      return decision;
    }

    ReadOnlyMemory<NpcDeathPlayerSnapshot> players = context.Players;
    if (players.Length != NpcDeathClothierSkeletronContext.RequiredPlayerSlots)
    {
      return decision with
      {
        SuppressTerminalDeath = true,
        PhaseKind = NpcDeathPhaseKind.ClothierSkeletronInputMissing,
        RequiredInputMissing = true,
      };
    }

    for (int playerSlot = 0; playerSlot < players.Length; playerSlot++)
    {
      NpcDeathPlayerSnapshot player = players.Span[playerSlot];
      if (player.IsActive && !player.IsDead && player.KillClothier)
      {
        return decision with
        {
          PhaseKind = NpcDeathPhaseKind.ClothierSkeletronSpawn,
          SkeletronSpawnIntent = new NpcDeathSkeletronSpawnIntent(playerSlot),
        };
      }
    }

    return decision;
  }

  private static NpcDeathPhaseDecision EvaluateCore(in NpcDeathPhaseInput input)
  {
    if (!input.IsActive || !input.IsLifeOwner || input.CurrentLife > 0)
    {
      return new NpcDeathPhaseDecision(
        SuppressTerminalDeath: true,
        StateChanged: false,
        PhaseKind: NpcDeathPhaseKind.EntryGuardRejected,
        AiAfter: input.Ai,
        RestoreLifeToMaximum: false,
        RejectAllDamage: false,
        RejectHostileDamage: false,
        ReplicationSyncRequested: false,
        SpawnIntent: null);
    }

    NpcAiStateComponent aiAfter = input.Ai;
    switch (input.NpcType.Value)
    {
      case 396:
      case 397:
        if (input.Ai.State0 == -2.0f)
        {
          return new NpcDeathPhaseDecision(
            SuppressTerminalDeath: true,
            StateChanged: false,
            PhaseKind: NpcDeathPhaseKind.Type396Or397PhaseHeld,
            AiAfter: aiAfter,
            RestoreLifeToMaximum: false,
            RejectAllDamage: false,
            RejectHostileDamage: false,
            ReplicationSyncRequested: false,
            SpawnIntent: null);
        }

        if (!input.SourceNpcInstanceId.IsValid)
        {
          return MissingSourceNpcIdentity(aiAfter);
        }

        aiAfter.State0 = -2.0f;
        return new NpcDeathPhaseDecision(
          SuppressTerminalDeath: true,
          StateChanged: true,
          PhaseKind: NpcDeathPhaseKind.Type396Or397SpawnPhase,
          AiAfter: aiAfter,
          RestoreLifeToMaximum: true,
          RejectAllDamage: true,
          RejectHostileDamage: false,
          ReplicationSyncRequested: true,
          SpawnIntent: new NpcDeathPhaseSpawnIntent(
            input.SourceNpcInstanceId,
            new NpcTypeId(400),
            (int)input.Center.X,
            (int)input.Center.Y,
            input.Ai.State3));

      case 398:
        if (input.Ai.State0 == 2.0f)
        {
          return new NpcDeathPhaseDecision(
            SuppressTerminalDeath: true,
            StateChanged: false,
            PhaseKind: NpcDeathPhaseKind.Type398PhaseHeld,
            AiAfter: aiAfter,
            RestoreLifeToMaximum: false,
            RejectAllDamage: false,
            RejectHostileDamage: false,
            ReplicationSyncRequested: false,
            SpawnIntent: null);
        }

        aiAfter.State0 = 2.0f;
        return new NpcDeathPhaseDecision(
          SuppressTerminalDeath: true,
          StateChanged: true,
          PhaseKind: NpcDeathPhaseKind.Type398EnterAi0Two,
          AiAfter: aiAfter,
          RestoreLifeToMaximum: true,
          RejectAllDamage: true,
          RejectHostileDamage: false,
          ReplicationSyncRequested: true,
          SpawnIntent: null);

      case 517:
      case 422:
      case 507:
      case 493:
        if (input.Ai.State2 == 1.0f)
        {
          break;
        }

        aiAfter.State2 = 1.0f;
        aiAfter.State1 = 0.0f;
        return new NpcDeathPhaseDecision(
          SuppressTerminalDeath: true,
          StateChanged: true,
          PhaseKind: NpcDeathPhaseKind.Type517422507493EnterAi2One,
          AiAfter: aiAfter,
          RestoreLifeToMaximum: true,
          RejectAllDamage: true,
          RejectHostileDamage: false,
          ReplicationSyncRequested: true,
          SpawnIntent: null);

      case 548:
        if (input.Ai.State1 == 1.0f)
        {
          break;
        }

        aiAfter.State1 = 1.0f;
        aiAfter.State0 = 0.0f;
        return new NpcDeathPhaseDecision(
          SuppressTerminalDeath: true,
          StateChanged: true,
          PhaseKind: NpcDeathPhaseKind.Type548EnterAi1One,
          AiAfter: aiAfter,
          RestoreLifeToMaximum: true,
          RejectAllDamage: false,
          RejectHostileDamage: true,
          ReplicationSyncRequested: true,
          SpawnIntent: null);
    }

    if (input.IsGoodWorld && input.NpcType.Value == 13)
    {
      if (!input.BottomY.HasValue || !input.SourceNpcInstanceId.IsValid)
      {
        return new NpcDeathPhaseDecision(
          SuppressTerminalDeath: true,
          StateChanged: false,
          PhaseKind: input.BottomY.HasValue
            ? NpcDeathPhaseKind.MissingSourceNpcIdentity
            : NpcDeathPhaseKind.GoodWorldType13MissingBottomY,
          AiAfter: aiAfter,
          RestoreLifeToMaximum: false,
          RejectAllDamage: false,
          RejectHostileDamage: false,
          ReplicationSyncRequested: false,
          SpawnIntent: null)
        {
          RequiredInputMissing = true,
        };
      }

      return new NpcDeathPhaseDecision(
        SuppressTerminalDeath: false,
        StateChanged: false,
        PhaseKind: NpcDeathPhaseKind.GoodWorldType13Spawn,
        AiAfter: aiAfter,
        RestoreLifeToMaximum: false,
        RejectAllDamage: false,
        RejectHostileDamage: false,
        ReplicationSyncRequested: false,
        SpawnIntent: null)
      {
        WorldEffectIntent = new NpcDeathWorldEffectIntent(
          Kind: NpcDeathWorldEffectKind.FixedPositionSpawn,
          SourceNpcInstanceId: input.SourceNpcInstanceId,
          SpawnNpcType: new NpcTypeId(-12),
          OriginCenter: input.Center,
          FixedPositionPixels: new System.Numerics.Vector2(
            (int)input.Center.X,
            (int)input.BottomY.Value),
          SpawnCount: 1,
          RandomOffsetRadiusInTiles: 0,
          SearchAttemptsPerSpawn: 0,
          WorldBottomMarginInTiles: 0,
          RequestReplicationSyncAfterSpawn: true),
      };
    }

    if (input.IsGoodWorld && input.NpcType.Value == 36)
    {
      if (!input.SourceNpcInstanceId.IsValid)
      {
        return MissingSourceNpcIdentity(aiAfter);
      }

      return new NpcDeathPhaseDecision(
        SuppressTerminalDeath: false,
        StateChanged: false,
        PhaseKind: NpcDeathPhaseKind.GoodWorldType36SpawnSearch,
        AiAfter: aiAfter,
        RestoreLifeToMaximum: false,
        RejectAllDamage: false,
        RejectHostileDamage: false,
        ReplicationSyncRequested: false,
        SpawnIntent: null)
      {
        WorldEffectIntent = new NpcDeathWorldEffectIntent(
          Kind: NpcDeathWorldEffectKind.SurfaceSearchSpawn,
          SourceNpcInstanceId: input.SourceNpcInstanceId,
          SpawnNpcType: new NpcTypeId(32),
          OriginCenter: input.Center,
          FixedPositionPixels: null,
          SpawnCount: 3,
          RandomOffsetRadiusInTiles: 50,
          SearchAttemptsPerSpawn: 1000,
          WorldBottomMarginInTiles: 200,
          RequestReplicationSyncAfterSpawn: true),
      };
    }

    return new NpcDeathPhaseDecision(
      SuppressTerminalDeath: false,
      StateChanged: false,
      PhaseKind: NpcDeathPhaseKind.None,
      AiAfter: aiAfter,
      RestoreLifeToMaximum: false,
      RejectAllDamage: false,
      RejectHostileDamage: false,
      ReplicationSyncRequested: false,
      SpawnIntent: null);
  }

  private static NpcDeathPhaseDecision MissingSourceNpcIdentity(
    NpcAiStateComponent ai)
  {
    return new NpcDeathPhaseDecision(
      SuppressTerminalDeath: true,
      StateChanged: false,
      PhaseKind: NpcDeathPhaseKind.MissingSourceNpcIdentity,
      AiAfter: ai,
      RestoreLifeToMaximum: false,
      RejectAllDamage: false,
      RejectHostileDamage: false,
      ReplicationSyncRequested: false,
      SpawnIntent: null)
    {
      RequiredInputMissing = true,
    };
  }
}
