# Real Client Object Fixture Host Implementation Plan


**Goal:** Start a disposable server with nearby door, sign, and chest fixtures so a
source-built full Terraria client can verify object interaction semantics without
changing the shared server on port 7778.

**Architecture:** A standalone `net10.0` executable under `Test/` owns one
`DomeServer` instance and an explicit port. It creates all objects before binding,
prints a `READY` JSON line, and blocks until Ctrl+C. The real-client automation uses
fixture coordinates for door, sign, and chest scenarios; the trace proxy remains the
authority for socket-close direction and server response frames.

**Tech Stack:** .NET 10, `Terraria.Dome.Server`, source-built Terraria 1.4.5.6
client, PowerShell process orchestration, Python JSONL TCP recorder.

---

### Task 1: Add the fixture-host project contract

**Files:**
- Create: `Test/Terraria.Dome.RealClientFixtureHost/Terraria.Dome.RealClientFixtureHost.csproj`
- Create: `Test/Terraria.Dome.RealClientFixtureHost/FixtureHostOptions.cs`
- Create: `Test/Terraria.Dome.RealClientFixtureHost/FixtureHostOptionsVerification.cs`

**Step 1: Write the failing parser verification**

Add `FixtureHostOptionsVerification.cs` with a `Verify()` method that requires
`--port 7845` to parse and rejects no argument, duplicate options, non-integer
ports, port `0`, and port `7778`.

```csharp
FixtureHostOptions options = FixtureHostOptions.Parse(["--port", "7845"]);
if (options.Port != 7845)
{
  throw new InvalidOperationException("Fixture host did not retain its port.");
}
```

**Step 2: Run the project to verify the missing parser fails**

Run:

```powershell
dotnet build Test/Terraria.Dome.RealClientFixtureHost/Terraria.Dome.RealClientFixtureHost.csproj -c Release -p:UseSharedCompilation=false
```

Expected: failure because `FixtureHostOptions` is missing.

**Step 3: Implement the minimal parser**

Implement `FixtureHostOptions.Parse(IReadOnlyList<string>)` with these invariants:

- exactly `--port <1..65535>` is accepted;
- port `7778` is rejected to protect the shared live server;
- unknown, duplicate, or missing values throw `ArgumentException`;
- the parsed port is exposed through a get-only `Port` property.

Add a `net10.0` executable project referencing only
`src/Terraria.Dome.Server/Terraria.Dome.Server.csproj`.

**Step 4: Run the parser verification**

Run:

```powershell
dotnet build Test/Terraria.Dome.RealClientFixtureHost/Terraria.Dome.RealClientFixtureHost.csproj -c Release -p:UseSharedCompilation=false
```

Expected: exit `0` with no errors.

**Step 5: Commit the project contract**

```powershell
git add Test/Terraria.Dome.RealClientFixtureHost
git commit -m "test: add real client fixture host options"
```

Commit only after the repository author identity is configured by the user.

### Task 2: Host isolated world objects

**Files:**
- Create: `Test/Terraria.Dome.RealClientFixtureHost/Program.cs`
- Modify: `Test/Terraria.Dome.RealClientFixtureHost/FixtureHostOptionsVerification.cs`

**Step 1: Extend the verification with fixture identity checks**

Declare expected fixture coordinates and assert that the ready record contains:

```csharp
DoorTileX == 2100
DoorTileY == 300
SignTileX == 2102
SignTileY == 300
ChestTileX == 2104
ChestTileY == 300
```

**Step 2: Run the verification to confirm the host is absent**

Run the fixture project with a temporary port:

```powershell
dotnet run --project Test/Terraria.Dome.RealClientFixtureHost/Terraria.Dome.RealClientFixtureHost.csproj -c Release -- --port 7845
```

Expected: failure before `Program.cs` exists.

**Step 3: Implement the host lifecycle**

Implement `Program.cs` using top-level statements:

1. Parse options and run `FixtureHostOptionsVerification.Verify()`.
2. Construct `DomeServer` without a persistence source.
3. Call `CreateDoor(2100, 300)`, `CreateSign(2102, 300, "Fixture Sign")`, and
   `CreateChest(2104, 300)` before starting the listener.
4. Call `server.Start(options.Port)`.
5. Emit `READY ` followed by one ASCII JSON object containing actual listener port,
   object IDs, and fixture coordinates.
6. Wait for Ctrl+C, then dispose the server through `using` scope.

Do not modify `src/Terraria.Dome.Server/Program.cs`, default world creation, or
the running service at port 7778.

**Step 4: Verify startup and directed cleanup**

Run from the repository root:

```powershell
dotnet build Test/Terraria.Dome.RealClientFixtureHost/Terraria.Dome.RealClientFixtureHost.csproj -c Release -p:UseSharedCompilation=false
```

Then start the host with `Start-Process`, wait for a `READY` line, confirm
`Get-NetTCPConnection -LocalPort 7845 -State Listen`, terminate only that host PID,
and confirm the existing port-7778 listener remains owned by its original process.

**Step 5: Commit the host implementation**

```powershell
git add Test/Terraria.Dome.RealClientFixtureHost
git commit -m "test: host isolated real client object fixtures"
```

### Task 3: Target fixture coordinates from the full client

**Files:**
- Modify: `D:/TRbackup/客户端/Terraria.Testing/TestAutomation/TestAutomationRuntime.cs`

**Step 1: Add a failing scenario assertion**

Add a diagnostic result assertion that the fixture door and sign scenarios send
their actions to distinct expected coordinates. The door must use `(2100, 300)` and
the sign must use `(2102, 300)`.

**Step 2: Build the full client to confirm the assertion cannot compile**

Run:

```powershell
dotnet build D:/TRbackup/客户端/Terraria.csproj -c Debug -p:UseSharedCompilation=false
```

Expected: compilation failure until fixture coordinate constants and scenario
methods are split.

**Step 3: Implement the minimal coordinate split**

Replace the shared `DefaultWorldObjectTileX/Y` use with named door and sign
constants. Keep the call path as Terraria's existing `NetMessage.SendData` calls:

```csharp
NetMessage.SendData(19, -1, -1, null, 0, FixtureDoorTileX, FixtureDoorTileY, 1);
NetMessage.SendData(46, -1, -1, null, FixtureSignTileX, FixtureSignTileY);
```

Do not add a raw socket client or custom packet encoder.

**Step 4: Build the full client**

Run the same build command. Expected: exit `0`; report existing warnings separately
from any new errors.

**Step 5: Commit only the automation change when external source is versioned**

The external client directory is not a Git repository in this environment. Do not
attempt to create a cross-repository commit; record the exact modified path in the
diagnostic matrix instead.

### Task 4: Run fixture-backed real-client object scenarios

**Files:**
- Modify: `Build/diagnostics/real-client-compatibility-20260818.md`
- Create: `Build/diagnostics/full-client-fixture-door-<timestamp>/`
- Create: `Build/diagnostics/full-client-fixture-sign-<timestamp>/`

**Step 1: Start all scoped processes**

For each scenario, create a fresh directory and save path, then start:

1. fixture host on a temporary port other than `7778`;
2. `full_session_trace.py` proxy forwarding a separate client port to the fixture
   host port; and
3. the source-built `Terraria.exe` with `-join`, `-port`, `-savedirectory`,
   `-testautomation`, and `-testresult`.

Use at least 35 seconds for client timeout. Do not use a short proxy deadline that
can expire during full-client startup.

**Step 2: Capture a red semantic condition first**

Point the door or sign scenario at a non-fixture coordinate. Assert there is no
door/sign replication frame. This proves the fixture coordinate is load-bearing.

**Step 3: Run the fixture coordinate scenario**

Door acceptance requires a C2S packet `19` followed by a server-originated door
state frame before client process exit. Sign acceptance requires C2S packet `46`
followed by its server sign response before client process exit. A client-side
`success` JSON alone is insufficient.

**Step 4: Stop only created processes**

After receiving terminal frames or the test result, stop the fixture host and proxy
PIDs that the runner created. Do not stop PID 16400 or port-7778 processes.

**Step 5: Update the matrix and commit it**

Update the two prior packet-only entries with exact coordinates, frame IDs, terminal
direction, timing, and trace paths.

```powershell
git add Build/diagnostics/real-client-compatibility-20260818.md
git commit -m "docs: record fixture-backed real client object results"
```

Again, commit only after Git author identity is available.
