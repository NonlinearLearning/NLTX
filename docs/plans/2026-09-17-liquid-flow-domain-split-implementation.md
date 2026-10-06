# Liquid Flow Domain Split Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Extract pure liquid-flow state-update calculation while leaving `LiquidFlowSystem` as the unique commit-port caller.

**Architecture:** Copy the current untracked liquid-flow source closure into the disposable branch, add a direct-compile executable verifier, and create `LiquidFlowStateUpdateQuery`. The System will validate input, evaluate the existing budget query, ask the new query for an update, and issue the sole commit.

**Tech Stack:** C# / .NET 10, `Invoke-SerialDotnet.ps1`, Git worktree, focused executable verifier.

---

### Task 1: Snapshot The LiquidFlow Source Closure

**Files:**
- Create: `src/WorldStorage/LiquidFlowSystem.cs`
- Create: `src/WorldStorage/LiquidFlowTickInput.cs`
- Create: `src/WorldStorage/LiquidFlowTickResult.cs`
- Create: `src/WorldStorage/LiquidFlowBudgetPolicy.cs`
- Create: `src/WorldStorage/LiquidFlowBudgetDecision.cs`
- Create: `src/WorldStorage/LiquidFlowBudgetQuery.cs`
- Create: `src/WorldStorage/LiquidFlowStateUpdate.cs`
- Create: `src/WorldStorage/LiquidFlowCommitResult.cs`
- Create: `src/WorldStorage/ILiquidFlowCommitPort.cs`

**Step 1: Record and compare source hashes**

Run `Get-FileHash -Algorithm SHA256` for every listed current-checkout source,
then repeat it in the experimental worktree after copying. The hashes must match.

**Step 2: Copy the exact source closure**

Copy only the listed files from `D:\TRbackup\NLTX\src\WorldStorage` to the
same path in this worktree. This is a disposable source snapshot, not a merge
of the current untracked workspace.

**Step 3: Commit the snapshot**

```powershell
git add src/WorldStorage
git commit -m "test: snapshot liquid flow baseline"
```

### Task 2: Add And Run The Red Focused Verifier

**Files:**
- Create: `Test/Terraria.LiquidFlowSplitVerification/Terraria.LiquidFlowSplitVerification.csproj`
- Create: `Test/Terraria.LiquidFlowSplitVerification/Program.cs`

**Step 1: Write the verifier before production code**

Use a direct compile closure so unrelated untracked projects cannot mask the
experiment. Include the optional future query only when its source exists.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="..\..\src\WorldStorage\LiquidFlow*.cs" Link="WorldStorage\%(Filename)%(Extension)" />
    <Compile Include="..\..\src\WorldStorage\ILiquidFlowCommitPort.cs" Link="WorldStorage\ILiquidFlowCommitPort.cs" />
  </ItemGroup>
</Project>
```

The test first proves current normal and panic orchestration. It then asserts
that the new query type and static `Create` method exist through reflection,
which is the deliberate red condition before production code is added.

```csharp
Type? queryType = typeof(LiquidFlowSystem).Assembly.GetType(
  "Terraria.WorldStorage.LiquidFlowStateUpdateQuery");
Assert(queryType is not null, "The state-update calculation needs a pure query seam.");
```

**Step 2: Run the red test serially**

After checking active `dotnet.exe` and `csc.exe` owners, run:

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '.\Test\Terraria.LiquidFlowSplitVerification\Terraria.LiquidFlowSplitVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
```

Expected: compilation succeeds and the executable exits nonzero because only
the query-type assertion fails.

**Step 3: Commit the red verifier**

```powershell
git add Test/Terraria.LiquidFlowSplitVerification
git commit -m "test: specify liquid flow update query seam"
```

### Task 3: Extract The Pure State-Update Query

**Files:**
- Create: `src/WorldStorage/LiquidFlowStateUpdateQuery.cs`
- Modify: `src/WorldStorage/LiquidFlowSystem.cs`

**Step 1: Add the query**

```csharp
public static class LiquidFlowStateUpdateQuery
{
  public static LiquidFlowStateUpdate Create(
    LiquidFlowTickInput input,
    LiquidFlowBudgetDecision decision)
  {
    // Return normal or panic-entry state without a component or port reference.
  }
}
```

Move the current `CreateStateUpdate` logic without changing its conditions or
field values. The query contains no validation, logging, clock, random source,
or side effect.

**Step 2: Make the System orchestrate and commit**

Replace the private mapping call in `Advance` with:

```csharp
LiquidFlowStateUpdate update = LiquidFlowStateUpdateQuery.Create(input, decision);
LiquidFlowCommitResult commitResult = _commitPort.Commit(in update);
```

Remove the old private mapping method. Keep input validation and the commit
port private field unchanged.

**Step 3: Extend the verifier behavior checks**

Use the reflected method to check that normal decisions preserve state and that
panic entry resets active liquid and panic counter, sets panic mode, and uses
`PanicStartY`. Also assert one commit per valid `Advance`, rejection propagation,
and no commit for invalid input.

**Step 4: Run green and no-build verification**

```powershell
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '.\Test\Terraria.LiquidFlowSplitVerification\Terraria.LiquidFlowSplitVerification.csproj', '-m:1', '-nr:false', '-p:UseSharedCompilation=false', '-p:MSBuildNodeReuse=false', '-p:BuildInParallel=false')
pwsh -NoProfile -File .\Build\Tools\Invoke-SerialDotnet.ps1 -DotnetArguments @('run', '.\Test\Terraria.LiquidFlowSplitVerification\Terraria.LiquidFlowSplitVerification.csproj', '--no-build', '--no-restore')
```

Expected: both commands exit zero and the compiler output is under `Build/bin/`.

**Step 5: Commit the split**

```powershell
git add src/WorldStorage Test/Terraria.LiquidFlowSplitVerification
git commit -m "refactor: extract liquid flow state update query"
```

### Task 4: Inspect, Report, And Dispose Of The Experiment

**Files:**
- Verify: all branch changes.

**Step 1: Inspect the final evidence**

```powershell
git diff --check main...HEAD
git diff --stat main...HEAD
git status --short
```

Record the fresh red and green command output, exit codes, artifact path, and
the fact that the focused verifier covers the copied source closure rather than
claiming the complete Version4 liquid runtime is compiled.

**Step 2: Delete the experiment as requested**

After reporting all evidence, remove this worktree and delete
`codex/liquid-flow-domain-split-experiment-20260917`. Do not modify `main` or
the concurrently active P06 worktree.
