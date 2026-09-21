#!/usr/bin/env python3
"""Build the bounded Version4 -> NLTX member migration facts.

The script consumes only the current scoped Roslyn snapshots and the structured first-round
review analysis.  It deliberately does not infer targets from names alone: a target is admitted
only when the review analysis contains an explicit component/member pair and the current target
candidate scan resolves that pair to one declaration set.
"""

from __future__ import annotations

import argparse
import copy
import hashlib
import json
from collections import defaultdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


ROOT = Path(__file__).resolve().parents[2]
SOURCE_SNAPSHOT = ROOT / "Build/generated/version4-source-scoped.json"
TARGET_CANDIDATES = ROOT / "Build/generated/nltx-current-source-candidates.json"
SCOPE_ANALYSIS = ROOT / "Build/generated/version4-member-scope-analysis.json"
SCOPE_ALLOWLIST = ROOT / "Build/generated/version4-member-scope.json"
COVERAGE = ROOT / "docs/migration/ledgers/Version4源码覆盖.tsv"
TARGET_MANIFEST = ROOT / "Build/generated/nltx-target-manifest.json"
TARGET_SNAPSHOT = ROOT / "Build/generated/nltx-target-scoped.json"
LEDGER = ROOT / "docs/migration/ledgers/Version4-member-migration-map.json"

def normalize_analysis(analysis: dict[str, Any]) -> dict[str, Any]:
    """Normalize renamed review paths and compound rows before generation."""

    def normalize_path_value(value: Any) -> Any:
        if isinstance(value, str):
            # Rewrite legacy review prefixes to the current component-decomposition layout.
            return (
                value.replace("docs/第一轮审查/design/", "docs/component-decomposition/review-round-1/design/")
                .replace("docs/第一轮审查/设计/", "docs/component-decomposition/review-round-1/design/")
                .replace("docs/第一轮审查/", "docs/component-decomposition/review-round-1/")
                .replace("docs/组件文档/第一轮审查/设计/", "docs/component-decomposition/review-round-1/design/")
                .replace("docs/组件文档/第一轮审查/", "docs/component-decomposition/review-round-1/")
            )
        return value

    analysis["designDocuments"] = [normalize_path_value(item) for item in analysis.get("designDocuments", [])]
    references = analysis.get("references", {})
    for source_id, refs in references.items():
        for ref in refs:
            ref["doc"] = normalize_path_value(ref.get("doc"))
            line = int(ref.get("line", 0))
            if source_id == "Version4::Terraria.GameContent.LeashedEntities.LeashedCritter::State" and line == 101:
                ref["targetPairs"] = [["LeashedCritterBehaviorComponent", "State"]]
            elif source_id == SECTION_INDEX_ACTIVE_SOURCE_ID:
                ref["targetPairs"] = []
            elif source_id == "Version4::Terraria.Projectile::ai":
                if line == 78:
                    ref["targetPairs"] = [
                        ["ProjectileTrajectoryStateComponent", "Ai0"],
                        ["ProjectileTrajectoryStateComponent", "Ai1"],
                        ["ProjectileTrajectoryStateComponent", "Ai2"],
                    ]
                elif line == 98:
                    ref["targetPairs"] = [["ProjectileTrajectoryStateComponent", "Ai0"]]
                elif line == 99:
                    ref["targetPairs"] = [["ProjectileTrajectoryStateComponent", "Ai1"]]
            elif source_id == "Version4::Terraria.Projectile::netUpdate2" and line == 120:
                ref["targetPairs"] = [["ProjectileNetworkStateComponent", "SecondaryUpdatePending"]]
            elif source_id == "Version4::Terraria.Projectile::ownerHitCheckDistance" and line == 110:
                ref["targetPairs"] = [["ProjectileCollisionPolicyComponent", "OwnerHitCheckDistance"]]
    return analysis


COMPATIBILITY_SOURCE_IDS = frozenset(
    {
        "Version4::Terraria.GameContent.LeashedEntity::whoAmI",
        "Version4::Terraria.Projectile::projUUID",
        "Version4::Terraria.Projectile::magic",
        "Version4::Terraria.Projectile::melee",
        "Version4::Terraria.Projectile::ranged",
    }
)
SECTION_INDEX_ACTIVE_SOURCE_ID = "Version4::Terraria.GameContent.LeashedEntity.SectionEntityList::active"
CACHE_TARGET_MEMBERS = frozenset({"OldPositions", "OldRotations", "OldSpriteDirections", "SubstepCounter"})


def load(path: Path) -> Any:
    return json.loads(path.read_text(encoding="utf-8"))


def sha256_bytes(value: bytes) -> str:
    return f"sha256:{hashlib.sha256(value).hexdigest()}"


def sha256_file(path: Path) -> str:
    return sha256_bytes(path.read_bytes())


def canonical_bytes(value: Any) -> bytes:
    return (json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(",", ":")) + "\n").encode("utf-8")


def canonical_hash(value: Any) -> str:
    return sha256_bytes(canonical_bytes(value))


def write_json(path: Path, value: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")


def normalize_path(value: str) -> str:
    return value.replace("\\", "/")


def target_key(member: dict[str, Any]) -> tuple[str, str, str, str, str]:
    return (
        normalize_path(str(member["path"])),
        str(member["declaringType"]),
        str(member["member"]),
        str(member["signature"]),
        str(member["kind"]),
    )


def target_id(member: dict[str, Any]) -> str:
    return f"src::NLTX::{member['declaringType']}::{member['signature']}"


def target_matches(candidates: list[dict[str, Any]], component: str, member_name: str) -> list[dict[str, Any]]:
    """Resolve a short review pair without treating parallel target trees as a split."""

    matches = [
        member
        for member in candidates
        if normalize_path(str(member.get("path", ""))).startswith(("src/", "dome/src/"))
        and str(member.get("declaringType", "")).rsplit(".", 1)[-1] == component
        and str(member.get("member", "")) == member_name
    ]
    root_matches = [member for member in matches if normalize_path(str(member.get("path", ""))).startswith("src/")]
    return root_matches or matches


def source_type(source: dict[str, Any]) -> str:
    return str(source.get("type") or "unknown")


def first_review(refs: list[dict[str, Any]]) -> dict[str, Any]:
    return sorted(refs, key=lambda item: (str(item.get("doc", "")), int(item.get("line", 0))))[0]


def review_text(refs: list[dict[str, Any]]) -> str:
    values: list[str] = []
    for ref in sorted(refs, key=lambda item: (str(item.get("doc", "")), int(item.get("line", 0)))):
        raw = str(ref.get("raw") or ref.get("sourceText") or "").strip()
        if raw and raw not in values:
            values.append(raw)
    return " | ".join(values)


def explicit_target_data(
    analysis: dict[str, Any], candidates: dict[str, Any]
) -> tuple[dict[str, list[dict[str, Any]]], dict[tuple[str, str, str, str, str], list[str]], dict[str, list[dict[str, Any]]]]:
    candidate_members = list(candidates.get("members", []))

    source_matches: dict[str, list[dict[str, Any]]] = defaultdict(list)
    target_sources: dict[tuple[str, str, str, str, str], list[str]] = defaultdict(list)
    source_explicit_refs: dict[str, list[dict[str, Any]]] = defaultdict(list)
    for source_member_id, refs in analysis.get("references", {}).items():
        for ref in refs:
            for pair in ref.get("targetPairs", []):
                if not isinstance(pair, list) or len(pair) < 2:
                    continue
                component, member_name = str(pair[0]), str(pair[1])
                matches = target_matches(candidate_members, component, member_name)
                source_explicit_refs[source_member_id].append(
                    {"doc": ref.get("doc"), "line": ref.get("line"), "pair": [component, member_name], "matchCount": len(matches)}
                )
                if source_member_id == SECTION_INDEX_ACTIVE_SOURCE_ID:
                    continue
                for match in matches:
                    source_matches[source_member_id].append(match)

    for source_member_id, matches in source_matches.items():
        unique_matches = {target_key(match): match for match in matches}
        for only_key in unique_matches:
            target_sources[only_key].append(source_member_id)

    return source_matches, target_sources, source_explicit_refs


def docs_metadata(analysis: dict[str, Any]) -> list[dict[str, Any]]:
    result: list[dict[str, Any]] = []
    for relative in sorted(str(item) for item in analysis.get("designDocuments", [])):
        path = (ROOT / relative).resolve()
        result.append(
            {
                "absolutePath": normalize_path(str(path)),
                "relativePath": normalize_path(relative),
                "sha256": sha256_file(path),
            }
        )
    return result


def classification(source: dict[str, Any], refs: list[dict[str, Any]]) -> dict[str, Any]:
    if str(source.get("sourceMemberId")) == SECTION_INDEX_ACTIVE_SOURCE_ID:
        return {
            "fileOwner": Path(str(source["path"])).stem,
            "stateKind": "cache",
            "scope": "global",
            "lifecycle": ["section index refresh", "active-set add/remove", "section index reset"],
            "authority": "section index/workset ownership only; not entity lifecycle authority",
            "persistence": "not-applicable",
            "network": "not-applicable",
            "accessRisks": [],
        }
    text = " ".join(
        [
            str(ref.get("sourceText", ""))
            + " "
            + " ".join(str(cell) for cell in ref.get("cells", []))
            for ref in refs
        ]
    )
    if "缓存" in text or "临时" in text:
        state_kind = "cache"
    elif "配置" in text:
        state_kind = "configuration"
    elif "兼容" in text and "权威" not in text:
        state_kind = "compatibility"
    elif any(token in str(source.get("member", "")).lower() for token in ("identity", "uuid", "whoami")):
        state_kind = "identity"
    else:
        state_kind = "authoritative"

    first = first_review(refs)
    cells = first.get("cells", [])
    authority_claim = str(cells[3]) if len(cells) > 3 and cells[3] else "first-round review authority claim is not closed"
    lifecycle_claim = str(cells[4]) if len(cells) > 4 and cells[4] else "first-round review lifecycle claim is not closed"
    all_text = text + " " + authority_claim + " " + lifecycle_claim
    persistence = "required" if any(token in all_text for token in ("保存", "加载", "存档", "持久")) else "unknown"
    network = "required" if any(token in all_text for token in ("网络", "同步", "复制", "序列化")) else "unknown"
    scope = "global" if "static" in [str(item) for item in source.get("modifiers", [])] else "entity"
    return {
        "fileOwner": Path(str(source["path"])).stem,
        "stateKind": state_kind,
        "scope": scope,
        "lifecycle": [f"first-round review lifecycle claim: {lifecycle_claim}"],
        "authority": authority_claim,
        "persistence": persistence,
        "network": network,
        "accessRisks": [],
    }


def build_manifest(analysis: dict[str, Any], candidates: dict[str, Any]) -> tuple[list[dict[str, Any]], dict[str, list[dict[str, Any]]], dict[str, list[dict[str, Any]]]]:
    sources, target_sources, explicit_refs = explicit_target_data(analysis, candidates)
    candidate_by_key = {target_key(member): member for member in candidates.get("members", [])}
    selected_keys = sorted(target_sources)
    manifest: list[dict[str, Any]] = []
    source_refs = analysis.get("references", {})
    for key in selected_keys:
        member = candidate_by_key[key]
        source_ids = sorted(target_sources[key])
        locations = ", ".join(
            f"{source_id} ({source_refs[source_id][0].get('doc')}:{source_refs[source_id][0].get('line')})"
            for source_id in source_ids
        )
        target_member_id = target_id(member)
        is_damage_class_root = target_member_id == "src::NLTX::Terraria.Projectile.ProjectileDamagePayloadComponent::DamageClass"
        is_compatibility_target = target_member_id in {
            "src::NLTX::Terraria.LeashedEntity.LeashedEntityLegacySlotComponent::Slot",
            "src::NLTX::Terraria.Projectile.ProjectileIdentityComponent::ProjectileUuid",
        }
        authority_role = "projection" if is_compatibility_target else "authoritative"
        origin = "compatibility" if is_compatibility_target else ("new" if is_damage_class_root else "migrated")
        if is_damage_class_root:
            design_reason = (
                "第一轮审查将 magic/melee/ranged 定义为 DamageClass 的兼容布尔视图；"
                "DamageClass 是独立的 ProjectileDamageClass 权威根，不由任一旧 bool 单独承载。"
            )
        elif is_compatibility_target:
            design_reason = f"第一轮审查将 {locations} 归入兼容投影 {member['declaringType']}.{member['member']}；该字段不拥有实体、持久化或网络 authority。"
        else:
            design_reason = f"第一轮审查文档明确将 {locations} 归入 {member['declaringType']}.{member['member']}；该 target 仅因显式 Component.Member 声明进入 manifest。"
        if str(member["member"]) in CACHE_TARGET_MEMBERS:
            authority_scope = "runtime-owned cache only; not persistence or network authority"
            persistence_policy = "not a persistence authority; snapshot policy not closed"
            network_policy = "not a network authority; replication policy not closed"
        else:
            authority_scope = "target component state; scope and entity binding not closed"
            persistence_policy = "not closed by first-round review"
            network_policy = "not closed by first-round review"
        manifest.append(
            {
                "targetMemberId": target_member_id,
                "projectOrAssembly": "NLTX",
                "componentPath": normalize_path(str(member["path"])),
                "componentType": str(member["declaringType"]),
                "member": str(member["member"]),
                "signature": str(member["signature"]),
                "kind": str(member["kind"]),
                "authorityRole": authority_role,
                "origin": origin,
                "designReason": design_reason,
                "authorityOwner": str(member["declaringType"]),
                "structuralWriteRoot": "target write root not closed; source declaration and design ownership only",
                "authorityScope": authority_scope,
                "lifecycle": "component creation through cleanup; lifecycle evidence not closed",
                "persistencePolicy": persistence_policy,
                "networkPolicy": network_policy,
            }
        )
    return manifest, sources, explicit_refs


def source_evidence(
    index: int,
    source: dict[str, Any],
    refs: list[dict[str, Any]],
) -> tuple[list[dict[str, Any]], dict[str, Any]]:
    first = first_review(refs)
    base = f"E{index:04d}"
    first_doc = normalize_path(str(first["doc"]))
    first_line = int(first["line"])
    declaration_id = f"declaration-{base}"
    reader_id = f"reader-{base}"
    writer_id = f"writer-{base}"
    authority_id = f"authority-{base}"
    lifecycle_id = f"lifecycle-{base}"
    source_id = str(source["sourceMemberId"])
    exact = f"{source_id}; {source['path']}:{source['locations'][0]['line']}; {source['declaringType']}.{source['signature']} [{source['kind']}] : {source['type']}; fingerprint {source['sourceFingerprint']}"
    review_ids: list[str] = []
    evidence: list[dict[str, Any]] = []
    for review_index, ref in enumerate(sorted(refs, key=lambda item: (str(item.get("doc", "")), int(item.get("line", 0)))), 1):
        review_id = f"review-{base}-{review_index:03d}"
        review_ids.append(review_id)
        review_doc = normalize_path(str(ref["doc"]))
        review_line = int(ref["line"])
        review_raw = str(ref.get("raw") or ref.get("sourceText") or "").strip()
        evidence.append({"evidenceId": review_id, "kind": "review", "path": review_doc, "line": review_line, "symbol": source_id, "claim": f"第一轮审查原文：{review_raw}", "status": "confirmed"})
    evidence.extend([
        {"evidenceId": declaration_id, "kind": "declaration", "path": f"D:/TRbackup/Version4/{source['path']}", "line": int(source["locations"][0]["line"]), "symbol": source_id, "claim": f"Version4 scoped Roslyn snapshot declaration: {exact}", "status": "confirmed"},
        {"evidenceId": reader_id, "kind": "reader", "path": first_doc, "line": first_line, "symbol": source_id, "claim": "Reader closure has not been performed for this member; first-round review and declaration evidence do not enumerate all direct, indirect, reflection, serialization, or generated-code readers.", "status": "open"},
        {"evidenceId": writer_id, "kind": "writer", "path": first_doc, "line": first_line, "symbol": source_id, "claim": "Writer closure has not been performed for this member; initialization, update, reset, death/despawn, reload, and cross-domain writes remain open.", "status": "open"},
        {"evidenceId": authority_id, "kind": "authority", "path": first_doc, "line": first_line, "symbol": source_id, "claim": "Authority cutover is not closed; the review design is recorded as a candidate ownership statement only.", "status": "open"},
        {"evidenceId": lifecycle_id, "kind": "lifecycle", "path": first_doc, "line": first_line, "symbol": source_id, "claim": "Lifecycle closure is not closed; the review lifecycle column is preserved but not independently verified against runtime paths.", "status": "open"},
    ])
    return evidence, {"review": review_ids[0], "reviewRefs": review_ids, "declaration": declaration_id, "reader": reader_id, "writer": writer_id, "authority": authority_id, "lifecycle": lifecycle_id}


def target_evidence(index: int, member: dict[str, Any], target_member_id: str) -> tuple[dict[str, Any], str]:
    evidence_id = f"target-declaration-E{index:04d}"
    line = int(member.get("locations", [{}])[0].get("line", 1))
    claim = f"Current NLTX Roslyn candidate declaration: {target_member_id}; {member['path']}:{line}; {member['declaringType']}.{member['signature']} [{member['kind']}] : {member['type']}"
    return {
        "evidenceId": evidence_id,
        "kind": "declaration",
        "path": normalize_path(str(member["path"])),
        "line": line,
        "symbol": target_member_id,
        "claim": claim,
        "status": "confirmed",
    }, evidence_id


def mapping_contract(
    source: dict[str, Any],
    target: dict[str, Any] | None,
    relation: str,
    merge_group: str | None,
    *,
    slot_name: str | None = None,
) -> dict[str, Any]:
    source_type_value = source_type(source)
    target_type_value = str(target["type"]) if target is not None else "none"
    if relation == "compatibility" and target is None:
        conversion = "legacy bool is a compatibility projection; DamageClass remains the independent authority root"
    elif slot_name:
        conversion = f"source float[] element {slot_name} maps to the corresponding target float slot; array length and slot semantics remain behavior-verification obligations"
    elif source_type_value == target_type_value:
        conversion = "declaration type matches; runtime transfer and semantic equivalence not verified"
    else:
        conversion = "declared source and target types differ; conversion and semantic equivalence not verified"
    invariants = [
        "source and target declaration types and kinds were compared",
        "reader/writer equivalence, lifecycle closure, persistence, network behavior, and cutover are not verified",
    ]
    if slot_name:
        invariants.append(f"slot contract is explicit for {slot_name}; ai[0], ai[1], and ai[2] must remain distinct")
    if merge_group:
        invariants.append(f"multiple source members share this target under explicit merge group {merge_group}; merge semantics are not closed")
    if relation == "compatibility":
        invariants.append("compatibility projection cannot become a second authority root")
    return {
        "kind": f"{source['kind']}-to-compatibility-projection" if relation == "compatibility" else f"{source['kind']}-to-component-member" + ("-split" if slot_name else ("-merge" if merge_group else "")),
        "sourceType": source_type_value,
        "targetType": target_type_value,
        "conversion": conversion,
        "unit": "source-defined",
        "nullability": "as declared",
        "defaultSemantics": "not inferred; source and target defaults require lifecycle review",
        "rangeSemantics": "not inferred",
        "normalization": "none asserted",
        "lossiness": "unknown",
        "reversible": False,
        "invariants": invariants,
        "authorityTransfer": "no authority transfer; compatibility projection only" if relation == "compatibility" else "candidate target component authority; cutover not run",
        "dualWritePolicy": "legacy projection only; no dual authority" if relation == "compatibility" else "not-run",
    }


def fact_projection(ledger: dict[str, Any]) -> dict[str, Any]:
    result = copy.deepcopy(ledger)
    result.setdefault("metadata", {}).pop("ledgerFactSha256", None)
    result.pop("verificationCatalog", None)
    result.pop("changeLog", None)
    result.pop("generatedViewMetadata", None)
    return result


def ledger_fact_hash(ledger: dict[str, Any]) -> str:
    return canonical_hash(fact_projection(ledger))


def build_ledger(analysis: dict[str, Any], sources: dict[str, dict[str, Any]], manifest: list[dict[str, Any]], explicit_refs: dict[str, list[dict[str, Any]]]) -> dict[str, Any]:
    source_snapshot = load(SOURCE_SNAPSHOT)
    target_snapshot = load(TARGET_SNAPSHOT)
    source_ids = sorted(sources)
    candidate_members = {target_key(member): member for member in load(TARGET_CANDIDATES).get("members", [])}
    manifest_by_id = {str(item["targetMemberId"]): item for item in manifest}
    target_by_id = {}
    for member in load(TARGET_CANDIDATES).get("members", []):
        tid = target_id(member)
        if tid in manifest_by_id:
            target_by_id[tid] = member

    refs_by_source = analysis.get("references", {})
    source_target_sets: dict[str, set[str]] = defaultdict(set)
    for source_id, refs in refs_by_source.items():
        for ref in refs:
            for pair in ref.get("targetPairs", []):
                if not isinstance(pair, list) or len(pair) < 2:
                    continue
                component, member_name = str(pair[0]), str(pair[1])
                matches = target_matches(list(candidate_members.values()), component, member_name)
                if source_id == SECTION_INDEX_ACTIVE_SOURCE_ID:
                    continue
                for member in matches:
                    if target_key(member) in {target_key(candidate) for candidate in candidate_members.values()}:
                        source_target_sets[source_id].add(target_id(member))

    target_owners: dict[str, list[str]] = defaultdict(list)
    for source_id, target_ids in source_target_sets.items():
        if len(target_ids) == 1 and source_id not in COMPATIBILITY_SOURCE_IDS:
            target_owners[next(iter(target_ids))].append(source_id)
    merge_group_by_target = {
        target_member_id: f"merge-{index:03d}"
        for index, target_member_id in enumerate(sorted(target_owners), 1)
        if len(target_owners[target_member_id]) > 1
    }

    evidence_catalog: list[dict[str, Any]] = []
    evidence_refs_by_source: dict[str, dict[str, Any]] = {}
    for index, source_id in enumerate(source_ids, 1):
        entries, refs = source_evidence(index, sources[source_id], refs_by_source[source_id])
        evidence_catalog.extend(entries)
        evidence_refs_by_source[source_id] = refs

    target_evidence_by_id: dict[str, str] = {}
    for index, target_member_id in enumerate(sorted(manifest_by_id), 1):
        entry, evidence_id = target_evidence(index, target_by_id[target_member_id], target_member_id)
        evidence_catalog.append(entry)
        target_evidence_by_id[target_member_id] = evidence_id

    decisions: list[dict[str, Any]] = []
    mappings_by_source: dict[str, list[str]] = defaultdict(list)
    for index, source_id in enumerate(source_ids, 1):
        source = sources[source_id]
        refs = refs_by_source[source_id]
        source_evidence_refs = evidence_refs_by_source[source_id]
        candidate_targets = sorted(source_target_sets.get(source_id, set()))
        usable_targets = [target_id for target_id in candidate_targets if target_id in manifest_by_id]
        blocker = "reader/writer, authority, lifecycle, persistence, network, and runtime verifier closure not run"
        if source_id in COMPATIBILITY_SOURCE_IDS:
            compatibility_target_id = (
                usable_targets[0]
                if source_id not in {
                    "Version4::Terraria.Projectile::magic",
                    "Version4::Terraria.Projectile::melee",
                    "Version4::Terraria.Projectile::ranged",
                }
                and len(usable_targets) == 1
                else None
            )
            compatibility_target = target_by_id.get(compatibility_target_id) if compatibility_target_id else None
            mapping_id = f"mapping-{index:04d}"
            mapping = {
                "mappingId": mapping_id,
                "targetRef": {
                    "kind": "projection" if compatibility_target_id else "none",
                    "targetMemberId": compatibility_target_id,
                },
                "relation": "compatibility",
                "role": "projection",
                "mappingContract": mapping_contract(source, compatibility_target, "compatibility", None),
                "evidenceRefs": [source_evidence_refs["declaration"]]
                + ([target_evidence_by_id[compatibility_target_id]] if compatibility_target_id else [])
                + source_evidence_refs["reviewRefs"],
                "verificationRefs": [],
            }
            decision = {
                "sourceMemberId": source_id,
                "sourceFingerprint": source["sourceFingerprint"],
                "classification": classification(source, refs),
                "disposition": "compatibility",
                "status": "deferred",
                "targetMappings": [mapping],
                "evidenceRefs": source_evidence_refs["reviewRefs"] + [source_evidence_refs[key] for key in ("declaration", "reader", "writer", "authority", "lifecycle")],
                "verificationRefs": [],
                "readerWriterStatus": "partial",
                "verificationStatus": "not-run",
                "batchId": f"batch-{(index - 1) // 8 + 1:03d}",
                "priority": 30,
                "nextWorkItemRef": f"work-item-{(index - 1) // 8 + 1:03d}",
                "blocker": "compatibility projection authority and lifecycle closure remain open",
                "evidenceGap": "compatibility projection conversion, authority, and legacy write policy remain open",
                "compatibilityReason": "legacy member remains an adapter/projection boundary and cannot become a second authority root",
                "reentryCondition": "define the compatibility conversion and legacy write policy, then run a hash-bound verifier before changing status",
                "legacyRetention": "compatibility-only",
            }
            mappings_by_source[source_id].append(mapping_id)
        elif len(usable_targets) >= 2 and len(usable_targets) == len(candidate_targets):
            mappings = []
            for split_index, target_member_id in enumerate(usable_targets, 1):
                target = target_by_id[target_member_id]
                mapping_id = f"mapping-{index:04d}" if split_index == 1 else f"mapping-{index:04d}-split-{split_index:02d}"
                slot_name = target.get("member") if source_id == "Version4::Terraria.Projectile::ai" else None
                mapping = {
                    "mappingId": mapping_id,
                    "targetRef": {"kind": "component-member", "targetMemberId": target_member_id},
                    "relation": "split",
                    "role": "authoritative",
                    "mappingContract": mapping_contract(source, target, "split", None, slot_name=slot_name),
                    "evidenceRefs": [source_evidence_refs["declaration"], target_evidence_by_id[target_member_id]] + source_evidence_refs["reviewRefs"],
                    "verificationRefs": [],
                }
                mappings.append(mapping)
                mappings_by_source[source_id].append(mapping_id)
            decision = {
                "sourceMemberId": source_id,
                "sourceFingerprint": source["sourceFingerprint"],
                "classification": classification(source, refs),
                "disposition": "split",
                "status": "migrated",
                "targetMappings": mappings,
                "evidenceRefs": source_evidence_refs["reviewRefs"] + [source_evidence_refs[key] for key in ("declaration", "reader", "writer", "authority", "lifecycle")],
                "verificationRefs": [],
                "readerWriterStatus": "partial",
                "verificationStatus": "not-run",
                "batchId": f"batch-{(index - 1) // 8 + 1:03d}",
                "priority": 20,
                "nextWorkItemRef": f"work-item-{(index - 1) // 8 + 1:03d}",
                "blocker": blocker,
                "evidenceGap": blocker,
                "splitReason": "Version4 exposes a shared array; each explicit target slot must retain independent owner, default, reset, read/write, and cutover semantics.",
                "mergeGroupId": None,
                "mergeReason": None,
                "reentryCondition": "close per-slot reader/writer/authority/lifecycle evidence and run a hash-bound verifier before changing status",
                "legacyRetention": "active-legacy-authority",
            }
        elif len(usable_targets) == 1 and len(candidate_targets) == 1:
            target_member_id = usable_targets[0]
            target = target_by_id[target_member_id]
            owners = sorted(target_owners[target_member_id])
            merge_group = merge_group_by_target.get(target_member_id)
            disposition = "merge" if merge_group else "move"
            mapping_id = f"mapping-{index:04d}"
            mapping = {
                "mappingId": mapping_id,
                "targetRef": {"kind": "component-member", "targetMemberId": target_member_id},
                "relation": disposition,
                "role": "authoritative",
                "mappingContract": mapping_contract(source, target, disposition, merge_group),
                "evidenceRefs": [source_evidence_refs["declaration"], target_evidence_by_id[target_member_id]] + source_evidence_refs["reviewRefs"],
                "verificationRefs": [],
            }
            decision = {
                "sourceMemberId": source_id,
                "sourceFingerprint": source["sourceFingerprint"],
                "classification": classification(source, refs),
                "disposition": disposition,
                "status": "migrated",
                "targetMappings": [mapping],
                "evidenceRefs": source_evidence_refs["reviewRefs"] + [source_evidence_refs[key] for key in ("declaration", "reader", "writer", "authority", "lifecycle")],
                "verificationRefs": [],
                "readerWriterStatus": "partial",
                "verificationStatus": "not-run",
                "batchId": f"batch-{(index - 1) // 8 + 1:03d}",
                "priority": 10 if disposition == "move" else 20,
                "nextWorkItemRef": f"work-item-{(index - 1) // 8 + 1:03d}",
                "blocker": blocker,
                "evidenceGap": blocker,
                "mergeGroupId": merge_group,
                "mergeReason": (
                    f"First-round review explicitly assigns multiple source members to the same target "
                    f"{target_member_id}; the shared authority and semantic merge are not yet closed."
                    if merge_group
                    else None
                ),
                "reentryCondition": "close reader/writer/authority/lifecycle evidence and run a hash-bound verifier before changing status",
                "legacyRetention": "active-legacy-authority",
            }
            mappings_by_source[source_id].append(mapping_id)
        else:
            explicit = explicit_refs.get(source_id, [])
            if len(candidate_targets) > 1:
                blocker = "explicit target expression resolves to multiple current target declarations or multiple target semantics; no target selected"
            elif explicit and all(int(item.get("matchCount", 0)) == 0 for item in explicit):
                blocker = "explicit target expression cannot be matched to a current src/dome declaration"
            else:
                blocker = "first-round review does not provide one reconstructible NLTX target member; no target inferred"
            decision = {
                "sourceMemberId": source_id,
                "sourceFingerprint": source["sourceFingerprint"],
                "classification": classification(source, refs),
                "disposition": "deferred",
                "status": "deferred",
                "targetMappings": [],
                "evidenceRefs": source_evidence_refs["reviewRefs"] + [source_evidence_refs[key] for key in ("declaration", "reader", "writer", "authority", "lifecycle")],
                "verificationRefs": [],
                "readerWriterStatus": "partial",
                "verificationStatus": "not-run",
                "batchId": f"batch-{(index - 1) // 8 + 1:03d}",
                "priority": 30,
                "nextWorkItemRef": f"work-item-{(index - 1) // 8 + 1:03d}",
                "blocker": blocker,
                "evidenceGap": "reader/writer/authority/lifecycle evidence and exact target binding remain open",
                "reentryCondition": "supply a unique current target declaration and close the open evidence axes; then rerun audit and snapshot checks",
                "legacyRetention": "active-legacy-authority",
            }
        decisions.append(decision)

    inventory = []
    for source_id in source_ids:
        source = sources[source_id]
        inventory.append(
            {
                "sourceMemberId": source["sourceMemberId"],
                "path": source["path"],
                "declaringType": source["declaringType"],
                "member": source["member"],
                "signature": source["signature"],
                "kind": source["kind"],
                "type": source["type"],
                "accessibility": source["accessibility"],
                "modifiers": source["modifiers"],
                "locations": source["locations"],
                "sourceFingerprint": source["sourceFingerprint"],
            }
        )

    work_items: list[dict[str, Any]] = []
    for batch_number, start in enumerate(range(0, len(source_ids), 8), 1):
        batch_ids = source_ids[start : start + 8]
        batch_decisions = [decisions[source_ids.index(source_id)] for source_id in batch_ids]
        mapping_refs = [mapping_id for decision in batch_decisions for mapping_id in [str(item["mappingId"]) for item in decision["targetMappings"]]]
        target_refs = sorted({str(item["targetRef"]["targetMemberId"]) for decision in batch_decisions for item in decision["targetMappings"] if item["targetRef"].get("targetMemberId")})
        evidence_refs = sorted({str(ref) for decision in batch_decisions for ref in decision["evidenceRefs"]} | {str(ref) for decision in batch_decisions for item in decision["targetMappings"] for ref in item["evidenceRefs"]})
        work_items.append(
            {
                "workItemId": f"work-item-{batch_number:03d}",
                "kind": "member-evidence",
                "title": f"First-round member evidence batch {batch_number:03d}",
                "status": "ready",
                "priority": 30,
                "sourceMemberRefs": batch_ids,
                "behaviorRefs": [],
                "targetMappingRefs": mapping_refs,
                "targetMemberRefs": target_refs,
                "prerequisites": [],
                "requiredEvidenceRefs": evidence_refs,
                "allowedChangeScope": "Only the listed member decisions, mappings, evidence, and generated ledger views.",
                "allowedFiles": ["docs/migration/ledgers/Version4-member-migration-map.json", "docs/migration/ledgers/Version4-member-migration-quick-reference.json", "docs/migration/ledgers/Version4-member-migration-context-packet.json"],
                "forbiddenDomains": ["src/", "dome/src/", "Test/", "D:/TRbackup/Version4", "docs/component-decomposition/review-round-1/"],
                "completionCriteria": ["Read the listed source and target declarations", "Close the listed evidence gaps", "Run hash-bound audit and regenerate views"],
                "verificationSpec": {"tool": "python", "arguments": [".agents/skills/version4-member-migration-ledger/scripts/audit_ledger.py", "--ledger", "docs/migration/ledgers/Version4-member-migration-map.json", "--repo-root", "."], "workingDirectory": "D:/TRbackup/NLTX", "expectedExitCode": 0},
                "expectedArtifacts": ["docs/migration/ledgers/version4-member-migration-audit.json", "docs/migration/ledgers/Version4-member-migration-quick-reference.json"],
                "blocker": None,
                "checkpoint": "bootstrap: source and review evidence recorded; target/behavior closure pending",
                "claim": None,
            }
        )

    coverage_rows = [line for line in COVERAGE.read_text(encoding="utf-8-sig").splitlines() if line.strip()]
    if coverage_rows and coverage_rows[0].split("\t", 1)[0] == "relative_path":
        coverage_rows = coverage_rows[1:]
    scope_data = load(SCOPE_ALLOWLIST)
    member_scope_ids = scope_data.get("sourceMemberIds", scope_data if isinstance(scope_data, list) else [])
    member_scope_ids = sorted(str(item) for item in member_scope_ids)
    source_hash = sha256_file(SOURCE_SNAPSHOT)
    target_hash = sha256_file(TARGET_SNAPSHOT)
    manifest_hash = canonical_hash(sorted(manifest, key=lambda item: str(item["targetMemberId"])))
    ledger = {
        "$schema": "./version4-member-migration-ledger.schema.json",
        "metadata": {
            "schemaVersion": "1.0",
            "ledgerId": "version4-nltx-member-migration-map",
            "revision": 1,
            "scopeRevision": source_snapshot["memberScopeSha256"],
            "sourceSnapshotPath": "Build/generated/version4-source-scoped.json",
            "sourceSnapshotSha256": source_hash,
            "targetSnapshotPath": "Build/generated/nltx-target-scoped.json",
            "targetSnapshotSha256": target_hash,
        },
        "baseline": {
            "sourceRoot": "D:/TRbackup/Version4",
            "coverageFile": "docs/migration/ledgers/Version4源码覆盖.tsv",
            "sourceFileCount": len(coverage_rows),
            "coverageSha256": sha256_file(COVERAGE),
            "referenceSupplementRoot": None,
            "extensions": {"owner": "version4-member-migration-ledger", "version": "1.0", "consumer": "audit-and-recovery", "values": {"designDocuments": docs_metadata(analysis)}},
        },
        "scope": {
            "memberScope": {"kind": "allowlist", "sourceMemberIds": member_scope_ids, "sha256": source_snapshot["memberScopeSha256"]},
            "targetManifest": [],
            "targetManifestPath": "Build/generated/nltx-target-manifest.json",
            "targetManifestSha256": manifest_hash,
        },
        "policies": {
            "physicalRemoval": "forbidden",
            "sourceIdentity": "Version4::<fully-qualified-declaring-type>::<member-signature>",
            "targetIdentity": "<target-root>::<project-or-assembly>::<fully-qualified-type>::<member-signature>",
            "oneActiveWorkItem": True,
        },
        "inventory": {"sourceMembers": inventory},
        "decisions": decisions,
        "evidenceCatalog": evidence_catalog,
        "verificationCatalog": [],
        "workItems": work_items,
        "changeLog": [{"eventId": "event-0001", "revision": 1, "actor": "codex", "action": "bootstrap-scoped-member-mapping", "entityId": "version4-nltx-member-migration-map", "recordedAt": "2026-09-07T00:00:00+08:00", "reason": "Created from current scoped Roslyn snapshots and explicit first-round review target pairs."}],
        "generatedViewMetadata": {"views": []},
    }
    ledger["metadata"]["ledgerFactSha256"] = ledger_fact_hash(ledger)
    return ledger


def record_views() -> None:
    ledger = load(LEDGER)
    paths = [
        ("markdown-quick-reference", "markdown", ROOT / "docs/migration/ledgers/Version4-member-migration-quick-reference.md"),
        ("machine-quick-reference", "machine", ROOT / "docs/migration/ledgers/Version4-member-migration-quick-reference.json"),
        ("target-index", "targetIndex", ROOT / "docs/migration/ledgers/Version4-component-target-index.json"),
        ("context-packet", "contextPacket", ROOT / "docs/migration/ledgers/Version4-member-migration-context-packet.json"),
    ]
    fact_hash = ledger_fact_hash(ledger)
    ledger["metadata"]["ledgerFactSha256"] = fact_hash
    ledger["generatedViewMetadata"] = {"views": [{"viewId": f"view-{kind}", "kind": kind, "path": normalize_path(str(path.relative_to(ROOT))), "sha256": sha256_file(path), "generatedFromLedgerFactSha256": fact_hash} for kind, _, path in paths]}
    write_json(LEDGER, ledger)


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--mode", choices=["normalize-analysis", "manifest", "ledger", "record-views"], required=True)
    args = parser.parse_args()
    if args.mode == "normalize-analysis":
        write_json(SCOPE_ANALYSIS, normalize_analysis(load(SCOPE_ANALYSIS)))
        print(json.dumps({"analysisPath": normalize_path(str(SCOPE_ANALYSIS.relative_to(ROOT)))}, ensure_ascii=False, indent=2))
    elif args.mode == "manifest":
        analysis = normalize_analysis(load(SCOPE_ANALYSIS))
        candidates = load(TARGET_CANDIDATES)
        manifest, _, _ = build_manifest(analysis, candidates)
        write_json(TARGET_MANIFEST, {"targetMembers": manifest})
        print(json.dumps({"targetMembers": len(manifest), "targetManifestSha256": canonical_hash(sorted(manifest, key=lambda item: str(item["targetMemberId"])))}, ensure_ascii=False, indent=2))
    elif args.mode == "ledger":
        analysis = normalize_analysis(load(SCOPE_ANALYSIS))
        source_snapshot = load(SOURCE_SNAPSHOT)
        sources = {str(item["sourceMemberId"]): item for item in source_snapshot["members"]}
        manifest = load(TARGET_MANIFEST)["targetMembers"]
        _, _, explicit_refs = explicit_target_data(analysis, load(TARGET_CANDIDATES))
        ledger = build_ledger(analysis, sources, manifest, explicit_refs)
        write_json(LEDGER, ledger)
        print(json.dumps({"ledgerFactSha256": ledger["metadata"]["ledgerFactSha256"], "decisions": len(ledger["decisions"]), "evidence": len(ledger["evidenceCatalog"]), "workItems": len(ledger["workItems"])}, ensure_ascii=False, indent=2))
    else:
        record_views()
        print(json.dumps({"ledgerFactSha256": load(LEDGER)["metadata"]["ledgerFactSha256"]}, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
