# Full Client Server Packet Trace Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Record every raw Terraria V1456 frame exchanged by the complete source-built
client and the complete 1.4.5.6 server baseline, then annotate the resulting session with
the retained Version4 network-source locations.

**Architecture:** A standalone Python TCP proxy sits between the two external executables on
dedicated localhost ports. It copies bytes without protocol mutation, emits JSON Lines for
every fully framed packet and terminal transport event, then an offline summarizer verifies
and groups the captured evidence. The original `Version4` source is read only for the
message-ID index; its physically removed files are not reconstructed or modified.

**Tech Stack:** Python 3 standard library (`socket`, `threading`, `json`, `unittest`),
source-built .NET Framework 4.0 Terraria client and server executables.

---

### Task 1: Establish the recorder's frame-preservation contract

**Files:**
- Create: `Build/diagnostics/test_full_session_trace.py`
- Create: `Build/diagnostics/full_session_trace.py`

**Step 1: Write the failing test**

Create a `unittest` module that feeds a length-prefixed frame with an 80-byte payload to a
per-direction frame accumulator. Assert that the emitted event has the full original frame,
the full payload hex, the correct length and message ID, and no preview/truncation field.

**Step 2: Run test to verify it fails**

Run: `python Build/diagnostics/test_full_session_trace.py`

Expected: `ModuleNotFoundError` for `full_session_trace`, proving no recorder implementation
exists yet.

**Step 3: Write minimal implementation**

Add `FrameAccumulator` and `TraceWriter`. `FrameAccumulator` accepts arbitrary TCP chunks,
parses Terraria's little-endian two-byte length prefix only once sufficient bytes arrive, and
emits complete original frames. `TraceWriter` writes ASCII-safe JSON Lines with monotonic
sequence numbers protected by a lock.

**Step 4: Run test to verify it passes**

Run: `python Build/diagnostics/test_full_session_trace.py`

Expected: the complete-frame test passes without truncation.

### Task 2: Preserve malformed and terminal transport evidence

**Files:**
- Modify: `Build/diagnostics/test_full_session_trace.py`
- Modify: `Build/diagnostics/full_session_trace.py`

**Step 1: Write failing tests**

Add tests for split length prefixes, several frames in one receive buffer, invalid lengths,
and EOF with a residual partial frame. Assert the residual bytes are emitted in a terminal
event rather than discarded. Assert a Windows-style non-ASCII socket message can be recorded.

**Step 2: Run tests to verify failures**

Run: `python Build/diagnostics/test_full_session_trace.py`

Expected: failures for absent terminal handling and record sanitization.

**Step 3: Write minimal implementation**

Add `finish()` to the accumulator and write `eof`, `socket_error`, `invalid_length`, and
`unframed_tail` events. Serialize exception data as structured fields and use
`ensure_ascii=True`; never interpolate an operating-system message into an ASCII file.

**Step 4: Run tests to verify they pass**

Run: `python Build/diagnostics/test_full_session_trace.py`

Expected: all framing and terminal-event tests pass.

### Task 3: Implement transparent, isolated-session proxy operation

**Files:**
- Modify: `Build/diagnostics/full_session_trace.py`

**Step 1: Write the failing test**

Add a loopback test that starts a backend echo listener and proxy on dynamically selected
localhost ports. Send a frame larger than 64 bytes through the proxy in multiple writes and
assert received bytes are unchanged and trace records contain both directions.

**Step 2: Run test to verify it fails**

Run: `python Build/diagnostics/test_full_session_trace.py`

Expected: proxy-operation test fails before listener and relay implementation exists.

**Step 3: Write minimal implementation**

Provide `--listen-port`, `--backend-port`, `--trace`, and optional `--stop-after-seconds`
arguments. Each accepted client gets a unique connection ID and two relay threads. Both
directions use `sendall` for byte fidelity and share a close-once callback.

**Step 4: Run tests to verify they pass**

Run: `python Build/diagnostics/test_full_session_trace.py`

Expected: all tests pass and the loopback proves transparent byte delivery.

### Task 4: Create source-aware evidence summarization

**Files:**
- Create: `Build/diagnostics/summarize_full_session_trace.py`
- Modify: `Build/diagnostics/test_full_session_trace.py`

**Step 1: Write failing tests**

Use a small JSONL fixture with both directions and one large frame. Assert the summary counts
each direction by ID, records observed min/max lengths, identifies terminal events, and reports
whether complete frame hex parses back to the recorded length. Assert the Version4 message
index reports `NetMessage.cs` and `MessageBuffer.cs` switch-case locations where available.

**Step 2: Run test to verify it fails**

Run: `python Build/diagnostics/test_full_session_trace.py`

Expected: summarizer import or expected aggregation assertions fail.

**Step 3: Write minimal implementation**

Read JSONL once, validate every `frame` record against its declared length and frame hex,
aggregate by `(direction, messageId)`, and scan the two retained Version4 network files for
`case <id>:` locations. Emit deterministic JSON and Markdown evidence without claiming that
an absent case has an unknown semantic meaning.

**Step 4: Run tests to verify they pass**

Run: `python Build/diagnostics/test_full_session_trace.py`

Expected: framing, proxy, summarization, and source-index tests all pass.

### Task 5: Capture a complete executable-client/executable-server session

**Files:**
- Create: `Build/diagnostics/full-client-server-<timestamp>/serverconfig.txt`
- Create: `Build/diagnostics/full-client-server-<timestamp>/trace.jsonl`
- Create: `Build/diagnostics/full-client-server-<timestamp>/summary.json`
- Create: `Build/diagnostics/full-client-server-<timestamp>/summary.md`

**Step 1: Start isolated server and proxy**

Start `D:\\TRbackup\\无任何删减通过编译\\bin\\Debug\\net40\\TerrariaServer.exe`
with an isolated temporary world and a backend port unused by the Dome runtime. Start the
new proxy on another unused localhost port with its trace path under `Build/diagnostics`.

**Step 2: Start the complete client against the proxy**

Run `D:\\TRbackup\\客户端\\bin\\Debug\\net40\\Terraria.exe` from its own output directory,
using an isolated save directory and `-join 127.0.0.1 -port <proxy-port>`. Do not launch it
from `NLTX`, which lacks its runtime Content directory.

**Step 3: Stop only processes started for this capture**

After world entry or a terminal event, stop the new client, proxy, and server using their exact
process IDs. Preserve all trace and stdout/stderr artifacts.

**Step 4: Summarize and validate evidence**

Run the summarizer over the JSONL trace. Require at least one C2S and one S2C fully verified
frame, no parser mismatch, and an explicit terminal event if the session closes. Report the
result as a concrete session transcript, not a claim of complete gameplay coverage.
