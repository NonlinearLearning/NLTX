# Legacy `Main.rand` Meteor Ordering Boundary

## Source Trace

The frozen source calls `Main.UpdateTime_StartNight` at `Main.cs:13735-13820`. The meteor
eligibility observation is `Main.rand.Next(50) == 0 && NPC.downedBoss2` at line 13761, but it is
not the first random consumer in the night-start path.

The ordered call boundary before that observation is:

1. `Star.NightSetup()` (`Star.cs:39-61`) conditionally consumes one to three `Main.rand` values.
2. `NPC.setFireFlyChance()` (`NPC.cs:79347-79390`) mixes conditional `WorldGen.genRand` and
   `Main.rand` consumption, with counts depending on mutable world/NPC facts.
3. `BirthdayParty.CheckNight()` calls `NaturalAttempt()`, whose `Main.rand.Next(10/7)` branch
   depends on the live NPC collection and party cooldown.
4. `LanternNight.CheckNight()` remains a call boundary even though its frozen `NaturalAttempt`
   body is empty.
5. `MysticLogFairiesEvent.StartNight()` performs a dynamic tile scan and mutates NPC spawn facts;
   it has no direct `Main.rand` call in the retained source.
6. Only then does `Main.rand.Next(50)` observe the meteor probability.

The same method continues consuming the global stream for eye, hard-boss and NPC spawn branches.
Therefore a stream value before the meteor observation cannot be restored from the current
Simulation snapshot without also reproducing unrelated client/world-generation/entity consumers.

## Decision

B-007 remains `explicit-deferred`. The executable trace can enumerate the source call order and the
first non-recoverable boundary, but it cannot supply a closed seeded oracle for global `Main.rand`
state. `WorldEventRandomState` must remain a separate deterministic domain stream, not a substitute
for legacy global ordering.

The supported route remains an explicit typed meteor-schedule command followed by deterministic
impact resolution. Automatic probability, landing search, meteor shower, ambience and client/UI
effects require separate source-backed cards.

Evidence: `Build/diagnostics/main-migration/task-10-meteor-rng-oracle-audit/20260822-155000/`.
