# Real Client Object Fixture Host Design

## Purpose

Provide a disposable, isolated Dome server for real `Terraria.exe` object
interaction checks. The current shared server at port 7778 must remain
unchanged while door, sign, and chest request behavior is measured.

## Scope

Create a test-only executable under `Test/` that:

- accepts one explicit TCP port;
- constructs a new `DomeServer` in its own process;
- creates a door, sign, and chest close to the default spawn position;
- emits one machine-readable ready record with the port, object IDs, and tile
  coordinates; and
- remains alive until Ctrl+C or process termination.

The host will not read or write the shared server state, bind port 7778, or
change production server startup behavior.

## Fixture Layout

The default spawn is at tile `(2100, 300)`. The host creates all fixture
objects in its visible section and within normal interaction range:

| Object | Tile coordinate | Notes |
| --- | --- | --- |
| Door | `(2100, 300)` | Used by client packet 19. |
| Sign | `(2102, 300)` | Initial text is `Fixture Sign`; used by packets 46 and 47. |
| Chest | `(2104, 300)` | Used after packet 31/34 compatibility repair. |

The real client automation receives these coordinates through explicit launch
scenario options or a generated diagnostic manifest. It keeps using Terraria's
own `NetMessage.SendData` path; no raw client packet generator is introduced.

## Lifecycle And Failure Handling

The host validates its single port argument before binding. It writes a
`READY` JSON line only after `DomeServer.Start(port)` succeeds. A failed bind
or invalid argument exits nonzero. Ctrl+C releases the listener through the
existing `DomeServer.Dispose` lifecycle.

The real-client runner starts the host, waits for `READY`, then starts the
transparent recorder and client. On any failure it stops only its own host,
proxy, and client process. The shared server process is never targeted.

## Acceptance Criteria

1. The fixture host starts on a non-7778 temporary port and reports its exact
   fixture coordinates.
2. A source-built full client can enter the fixture server through the normal
   bootstrap flow.
3. Door packet 19 receives the expected door-state replication only when the
   target is the fixture door.
4. Sign-open packet 46 receives the fixture sign response for the fixture
   coordinate.
5. The tests leave the shared server listening on port 7778 and retain a
   frame-level JSONL trace for each result.
