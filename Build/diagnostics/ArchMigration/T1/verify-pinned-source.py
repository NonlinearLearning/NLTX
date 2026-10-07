"""Check captured Arch source provenance and selected static invariants.

This is a local source audit only. It performs no network access and launches no tests.
"""

from __future__ import annotations

import hashlib
import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[4]
DIAGNOSTICS = Path(__file__).resolve().parent
PROBE = ROOT / "Test" / "Terraria.Arch.Verification" / "Program.cs"
PIN = "04d52e7268eb6f376ca4841a0c204334120c5e9d"


def read(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig", errors="replace")


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest().upper()


def relative_captured_path(value: str) -> Path:
    return ROOT / value.lstrip(".\\/").replace("\\", "/")


def main() -> None:
    source_records: list[dict[str, object]] = []
    pin_files = [
        DIAGNOSTICS / "arch-pinned-source-hashes.json",
        DIAGNOSTICS / "arch-query-api-source-hashes.json",
        DIAGNOSTICS / "arch-archetype-source-hashes.json",
    ]
    for metadata_path in pin_files:
        payload = json.loads(read(metadata_path))
        records = payload if isinstance(payload, list) else [payload]
        for record in records:
            captured = record.get("path")
            expected = record.get("sha256")
            if not captured or not expected:
                continue
            local_path = relative_captured_path(str(captured))
            actual = sha256(local_path) if local_path.exists() else None
            source_records.append(
                {
                    "path": str(local_path.relative_to(ROOT)),
                    "expectedSha256": str(expected).upper(),
                    "actualSha256": actual,
                    "matches": actual == str(expected).upper(),
                }
            )

    command_buffer_path = DIAGNOSTICS / "upstream-CommandBuffer.cs"
    world_path = DIAGNOSTICS / "upstream-World.cs"
    command_buffer = read(command_buffer_path)
    world = read(world_path)
    raw_crash_path = DIAGNOSTICS / "core-risk-sample-explicit-project.log"
    raw_crash = read(raw_crash_path)
    program = read(PROBE)

    case_names = re.findall(r'^\s*\("([^"]+)",\s*\w+\)', program, re.MULTILINE)
    selected_cases = [
        "world.id-reuse",
        "world.isalive-worldid-boundary",
        "component.struct-class-access-critical",
        "query.composition-critical",
        "command-buffer.create-and-groups-critical",
    ]

    checks = [
        {
            "id": "command-buffer-create-id-index-alignment",
            "status": "confirmed-by-pinned-source",
            "evidence": [
                "upstream-CommandBuffer.cs:138-149",
                "upstream-CommandBuffer.cs:173-182",
            ],
            "basis": all(
                token in command_buffer
                for token in (
                    "info = new BufferedEntityInfo(Size, setIndex, addIndex, removeIndex);",
                    "BufferedEntityInfo.Add(entity.Id, info);",
                    "var entity = new Entity(-(Size + 1), -1);",
                    "var command = new CreateCommand(Size - 1, types);",
                )
            ),
            "limit": "This establishes unique staged-ID/index alignment in one live buffer; cross-World entities sharing a local Id and stale handles across buffer reuse were not runtime-probed.",
        },
        {
            "id": "command-buffer-add-set-shared-value-slot",
            "status": "confirmed-by-pinned-source",
            "evidence": [
                "upstream-CommandBuffer.cs:215-227",
                "upstream-CommandBuffer.cs:238-251",
            ],
            "basis": all(
                token in command_buffer
                for token in (
                    "Sets.Set(info.SetIndex, in component);",
                    "Adds.Set<T>(info.AddIndex);",
                )
            ),
            "limit": "The final value-only assertions were not reached in the process that terminated with AccessViolationException; the stored value-slot behavior is source-verified.",
        },
        {
            "id": "world-move-refreshes-entity-data",
            "status": "confirmed-by-pinned-source",
            "evidence": ["upstream-World.cs:325-347"],
            "basis": all(
                token in world
                for token in (
                    "EntityInfo.Move(movedEntity, slot);",
                    "data.Archetype = destination;",
                    "data.Slot = destinationSlot;",
                )
            ),
            "limit": "This confirms the normal Move code updates EntityData after a structural move; it is not an independent runtime stress test of every EntityInfo capacity path.",
        },
        {
            "id": "access-violation-stack-captured",
            "status": "captured-runtime-evidence",
            "evidence": [
                "core-risk-sample-explicit-project.log",
                "upstream-CommandBuffer.cs:322-350",
            ],
            "basis": "System.AccessViolationException" in raw_crash
            and "Arch.Core.Chunk.GetArray" in raw_crash
            and "chunk.GetArray(sparseArray.Type)" in command_buffer,
            "limit": "Chunk.cs was not captured, so the internal unchecked lookup/index path remains unverified.",
        },
    ]

    chunk_source_path = DIAGNOSTICS / "upstream-src-Arch-Core-Chunk.cs"
    source_hashes_match = all(item["matches"] for item in source_records)
    all_static_checks_match = all(bool(item["basis"]) for item in checks)
    if not source_hashes_match or not all_static_checks_match:
        raise SystemExit("Captured source verification failed; inspect generated report.")

    result = {
        "status": "partial",
        "auditType": "local-static-source-and-evidence-audit",
        "networkAccess": False,
        "testsLaunched": False,
        "pinnedCommit": PIN,
        "sourceHashes": source_records,
        "allCapturedSourceHashesMatch": source_hashes_match,
        "checks": checks,
        "probeCaseCount": len(case_names),
        "selectedRiskCases": selected_cases,
        "selectedRiskCaseCount": len(selected_cases),
        "selectedShare": len(selected_cases) / len(case_names) if case_names else None,
        "chunkImplementation": {
            "expectedPath": "src/Arch/Core/Chunk.cs",
            "capturedPath": str(chunk_source_path.relative_to(ROOT)),
            "captured": chunk_source_path.exists(),
            "status": "not-verified",
        },
    }
    output_path = DIAGNOSTICS / "pinned-source-verification.json"
    output_path.write_text(json.dumps(result, indent=2) + "\n", encoding="utf-8")
    print(f"Wrote {output_path.relative_to(ROOT)}")
    print(f"Pinned source hashes matched: {len(source_records)}")
    print(f"Probe cases: {len(case_names)}; selected: {len(selected_cases)}")
    print("Chunk.cs captured: no; its implementation remains unverified")


if __name__ == "__main__":
    main()
