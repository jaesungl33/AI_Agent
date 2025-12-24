#!/usr/bin/env python3
"""
CLI runner for GDD coverage evaluation pipeline.
Provides clear logging and heartbeat for debugging the coverage pipeline.
"""

import argparse
import asyncio
import json
import logging
import sys
import threading
import time
from pathlib import Path
from typing import Dict, List, Optional

# Add the backend and src directories to the Python path
sys.path.insert(0, str(Path(__file__).parent.parent))
sys.path.insert(0, str(Path(__file__).parent.parent.parent / "src"))

# No imports that might cause crashes initially


def _parse_gdd_export_markdown(content: str, gdd_doc_id: str) -> List[Dict]:
    """Parse requirements from the exported GDD markdown file."""
    requirements = []
    lines = content.split('\n')

    current_section = None
    req_id = None
    title = None
    description_lines = []

    for line in lines:
        line = line.strip()

        # Check for section headers
        if line.startswith('## Requirements'):
            current_section = 'requirements'
            continue
        elif line.startswith('## '):
            current_section = line[3:].lower().replace(' ', '_')
            continue

        if current_section == 'requirements':
            # Parse requirement lines
            if line.startswith('- **') and '**:' in line:
                # Save previous requirement if exists
                if req_id and title:
                    requirements.append({
                        "id": req_id,
                        "title": title,
                        "description": '\n'.join(description_lines).strip() if description_lines else "",
                        "triggers": [],
                        "effects": [],
                        "acceptance_criteria": []
                    })

                # Parse new requirement: - **req_id**: title
                try:
                    parts = line[3:].split('**:', 1)
                    req_id = parts[0].strip()
                    title = parts[1].strip()
                    description_lines = []
                except:
                    req_id = None
                    title = None
                    description_lines = []
            elif line.startswith('  - ') and req_id:
                # Description line
                description_lines.append(line[4:].strip())
            elif line.startswith('- ') and not line.startswith('- **'):
                # End of current requirement
                if req_id and title:
                    requirements.append({
                        "id": req_id,
                        "title": title,
                        "description": '\n'.join(description_lines).strip() if description_lines else "",
                        "triggers": [],
                        "effects": [],
                        "acceptance_criteria": []
                    })
                    req_id = None
                    title = None
                    description_lines = []

    # Save last requirement
    if req_id and title:
        requirements.append({
            "id": req_id,
            "title": title,
            "description": '\n'.join(description_lines).strip() if description_lines else "",
            "triggers": [],
            "effects": [],
            "acceptance_criteria": []
        })

    return requirements


def _parse_code_export_markdown(content: str, code_batch_id: str) -> List[Dict]:
    """Parse functions from the exported code markdown file."""
    behaviors = []
    lines = content.split('\n')

    current_doc_id = None

    for line in lines:
        line = line.strip()

        # Check for document headers
        if line.startswith('# ') and not line.startswith('# ' + code_batch_id):
            current_doc_id = line[2:].strip()
            continue

        # Parse function lines
        if line.startswith('- ') and not line.startswith('- (no functions detected)'):
            func_name = line[2:].strip()
            if func_name and current_doc_id:
                behaviors.append({
                    "file_path": f"{current_doc_id}.cs",  # Assume .cs extension
                    "chunk_id": f"{current_doc_id}_{func_name}",
                    "signature": f"public void {func_name}()",
                    "summary": f"Function {func_name} in {current_doc_id}",
                    "triggers": [func_name.lower()],
                    "effects": ["unknown"]
                })

    return behaviors


class HeartbeatThread(threading.Thread):
    """Heartbeat thread that prints status every 2 seconds."""

    def __init__(self, current_stage_callback):
        super().__init__(daemon=True)
        self.current_stage_callback = current_stage_callback
        self.running = True
        self.start_time = time.time()

    def run(self):
        while self.running:
            elapsed = time.time() - self.start_time
            current_stage = self.current_stage_callback()
            print(f"[HEARTBEAT] still running... t={elapsed:.1f}s stage={current_stage}", flush=True)
            time.sleep(2)

    def stop(self):
        self.running = False


def list_available_resources():
    """List available workspaces, GDD docs, and code batches."""
    print("Available resources:")
    print("=" * 50)

    # List workspaces - look in the project data directory
    script_dir = Path(__file__).parent
    project_root = script_dir.parent.parent
    workspaces_dir = project_root / "data" / "workspaces"

    if workspaces_dir.exists():
        workspaces = [d.name for d in workspaces_dir.iterdir() if d.is_dir()]
        print(f"Workspaces ({len(workspaces)}):")
        for ws in sorted(workspaces):
            print(f"  - {ws}")
    else:
        print("Workspaces: No workspaces directory found")
        return

    print()

    # For each workspace, list GDD and code docs
    for workspace in sorted(workspaces):
        print(f"Workspace: {workspace}")
        print("-" * 30)

        status_file = project_root / "data" / "workspaces" / workspace / "storage" / "status.json"
        if status_file.exists():
            try:
                with open(status_file, 'r') as f:
                    status = json.load(f)

                gdd_docs = [(k, v) for k, v in status.items() if v.get('doc_type') != 'code']
                code_docs = [(k, v) for k, v in status.items() if v.get('doc_type') == 'code']

                print(f"GDD Documents ({len(gdd_docs)}):")
                for doc_id, meta in sorted(gdd_docs)[:10]:  # Show first 10
                    status_str = meta.get('status', 'unknown')
                    print(f"  - {doc_id} (status: {status_str})")
                if len(gdd_docs) > 10:
                    print(f"  ... and {len(gdd_docs) - 10} more")

                print(f"\nCode Batches ({len(code_docs)}):")
                for doc_id, meta in sorted(code_docs)[:10]:  # Show first 10
                    status_str = meta.get('status', 'unknown')
                    print(f"  - {doc_id} (status: {status_str})")
                if len(code_docs) > 10:
                    print(f"  ... and {len(code_docs) - 10} more")

            except Exception as e:
                print(f"Error reading status for {workspace}: {e}")
        else:
            print("No status.json found")

        print()


def load_or_extract_requirements(workspace_id: str, gdd_doc_id: str, force: bool = False, dry_run: bool = False) -> List[Dict]:
    """Load cached requirements or extract fresh ones."""
    import json
    from pathlib import Path
    from datetime import datetime

    spec_path = Path(f"data/workspaces/{workspace_id}/reports/{gdd_doc_id}_spec.json")

    # Try to load from cache
    if not force and spec_path.exists():
        try:
            with open(spec_path, 'r') as f:
                cached_spec = json.load(f)
            if "schemaVersion" in cached_spec and "requirements" in cached_spec:
                requirements = cached_spec["requirements"]
                print(f"[CACHE] Loaded {len(requirements)} requirements from cache")
                return requirements
        except Exception as e:
            print(f"[WARNING] Failed to load cached spec: {e}")

    if dry_run:
        # Return mock requirements for dry-run
        requirements = [
            {
                "id": "req_1",
                "title": f"Mock requirement 1 for {gdd_doc_id}",
                "description": "This is a mock requirement for dry-run testing",
                "triggers": ["some trigger"],
                "effects": ["some effect"],
                "acceptance_criteria": ["must work"]
            }
        ]
        print(f"[DRY-RUN] Created {len(requirements)} mock requirements")
        return requirements

    # For real extraction, use the backend API to trigger extraction and then read the results
    print(f"[EXTRACT] Calling backend API to extract requirements from {gdd_doc_id}")

    import requests
    backend_url = "http://localhost:8000"

    try:
        # First trigger the export
        response = requests.get(f"{backend_url}/export/gdd", params={"workspaceId": workspace_id}, timeout=300)
        if response.status_code != 200:
            raise RuntimeError(f"Backend export failed with status {response.status_code}: {response.text}")

        # Then read the exported file
        export_data = response.json()
        export_file = Path(export_data.get("file", ""))
        if not export_file.exists():
            raise RuntimeError(f"Export file not found: {export_file}")

        # Parse the markdown file to extract requirements
        content = export_file.read_text(encoding="utf-8")
        requirements = _parse_gdd_export_markdown(content, gdd_doc_id)

        print(f"[EXTRACT] Parsed {len(requirements)} requirements from export")

        # Cache the results
        spec_path.parent.mkdir(parents=True, exist_ok=True)
        spec_data = {
            "schemaVersion": "1.0",
            "extractedAt": datetime.utcnow().isoformat() + "Z",
            "gddDocId": gdd_doc_id,
            "requirements": requirements
        }
        with open(spec_path, 'w') as f:
            json.dump(spec_data, f, indent=2)

        return requirements

    except Exception as e:
        print(f"[ERROR] Failed to extract via backend API: {e}")
        print("[ERROR] Falling back to mock data for testing")
        # Fallback to mock data
        requirements = [
            {
                "id": "req_fallback",
                "title": f"Fallback requirement for {gdd_doc_id}",
                "description": f"Backend extraction failed: {str(e)}",
                "triggers": ["unknown"],
                "effects": ["unknown"],
                "acceptance_criteria": ["backend available"]
            }
        ]
        return requirements


def load_or_build_behavior_index(workspace_id: str, code_batch_id: str, force: bool = False, dry_run: bool = False) -> List[Dict]:
    """Load cached behavior index or build fresh one."""
    import json
    import hashlib
    from pathlib import Path
    from datetime import datetime

    index_path = Path(f"data/workspaces/{workspace_id}/behavior_indices/{code_batch_id}.json")

    # Try to load from cache
    if not force and index_path.exists():
        try:
            with open(index_path, 'r') as f:
                cached_index = json.load(f)
            if "behaviors" in cached_index and "fileHashes" in cached_index:
                # Check if files have changed
                current_hashes = _compute_file_hashes_for_batch(workspace_id, code_batch_id)
                if current_hashes == cached_index.get("fileHashes", {}):
                    behaviors = cached_index["behaviors"]
                    print(f"[CACHE] Loaded {len(behaviors)} behaviors from cache")
                    return behaviors
                else:
                    print("[CACHE] Files changed, rebuilding index")
        except Exception as e:
            print(f"[WARNING] Failed to load cached index: {e}")

    if dry_run:
        # Return mock behaviors for dry-run
        behaviors = [
            {
                "file_path": f"mock/{code_batch_id}.cs",
                "chunk_id": f"{code_batch_id}_chunk_1",
                "signature": f"public class {code_batch_id}",
                "summary": f"Mock behavior summary for {code_batch_id}",
                "triggers": ["some trigger"],
                "effects": ["some effect"]
            }
        ]
        print(f"[DRY-RUN] Created {len(behaviors)} mock behaviors")
        return behaviors

    # For real indexing, use the backend API instead of duplicating the logic
    print(f"[INDEX] Calling backend API to build behavior index for {code_batch_id}")

    import requests
    backend_url = "http://localhost:8000"

    try:
        # First trigger the export
        response = requests.get(f"{backend_url}/export/code", params={"workspaceId": workspace_id}, timeout=300)
        if response.status_code != 200:
            raise RuntimeError(f"Backend export failed with status {response.status_code}: {response.text}")

        # Then read the exported file
        export_data = response.json()
        export_file = Path(export_data.get("file", ""))
        if not export_file.exists():
            raise RuntimeError(f"Export file not found: {export_file}")

        # Parse the markdown file to extract behaviors
        content = export_file.read_text(encoding="utf-8")
        behaviors = _parse_code_export_markdown(content, code_batch_id)

        print(f"[INDEX] Parsed {len(behaviors)} behaviors from export")

        # Cache the results
        file_hashes = _compute_file_hashes_for_batch(workspace_id, code_batch_id)
        index_data = {
            "schemaVersion": "1.0",
            "builtAt": datetime.utcnow().isoformat() + "Z",
            "codeBatchId": code_batch_id,
            "fileHashes": file_hashes,
            "behaviors": behaviors
        }
        index_path.parent.mkdir(parents=True, exist_ok=True)
        with open(index_path, 'w') as f:
            json.dump(index_data, f, indent=2)

        return behaviors
    except Exception as e:
        print(f"[ERROR] Failed to index via backend API: {e}")
        print("[ERROR] Falling back to mock data for testing")
        # Fallback to mock data
        behaviors = [
            {
                "file_path": f"fallback/{code_batch_id}.cs",
                "chunk_id": f"{code_batch_id}_fallback",
                "signature": f"public class {code_batch_id}",
                "summary": f"Fallback behavior - backend indexing failed: {str(e)}",
                "triggers": ["unknown"],
                "effects": ["unknown"]
            }
        ]
        return behaviors


def _compute_file_hashes_for_batch(workspace_id: str, code_batch_id: str) -> Dict[str, str]:
    """Compute SHA256 hashes for all files in a code batch."""
    import hashlib

    hashes = {}

    try:
        chunks = load_doc_chunks(code_batch_id, workspace_id=workspace_id)
        for chunk in chunks:
            content_hash = hashlib.sha256(chunk.content.encode('utf-8')).hexdigest()
            hashes[chunk.chunk_id] = content_hash
    except Exception:
        pass

    return hashes


def run_coverage_evaluation(requirements: List[Dict], behaviors: List[Dict],
                          top_k: int, mode: str, limit: Optional[int],
                          dry_run: bool = False, workspace_id: str = "", gdd_doc_id: str = "", code_batch_id: str = "") -> Dict:
    """Run the coverage evaluation pipeline."""
    print(f"[EVAL] Evaluating {len(requirements)} requirements against {len(behaviors)} behaviors")

    if dry_run:
        print("[DRY-RUN] Skipping LLM calls, marking all as UNKNOWN")
        results = []
        for req in requirements:
            results.append({
                "requirement": req,
                "status": "UNKNOWN",
                "confidence": 0.0,
                "matchedCriteria": [],
                "missingCriteria": ["dry-run mode"],
                "evidence": [],
                "notes": "Skipped due to dry-run mode"
            })
        return {"results": results, "dry_run": True}

    # For real evaluation, use the backend API
    print(f"[EVAL] Calling backend API for coverage evaluation")

    import requests
    backend_url = "http://localhost:8000"

    # Prepare the evaluation payload
    payload = {
        "workspaceId": workspace_id,
        "gddDocId": gdd_doc_id,
        "codeBatchId": code_batch_id,
        "mode": mode,
        "topK": top_k,
        "maxRequirements": limit if mode == "fast" else None
    }

    try:
        response = requests.post(f"{backend_url}/coverage/run", json=payload, timeout=600)  # 10 minute timeout
        if response.status_code == 200:
            data = response.json()
            if "results" in data:
                results = data["results"]
                print(f"[EVAL] Backend completed evaluation of {len(results)} requirements")
                return {"results": results}
            else:
                raise RuntimeError(f"Backend response missing results: {data}")
        else:
            raise RuntimeError(f"Backend returned status {response.status_code}: {response.text}")
    except Exception as e:
        print(f"[ERROR] Failed to evaluate via backend API: {e}")
        print("[ERROR] Falling back to mock evaluation for testing")
        # Fallback to mock evaluation
        results = []
        for req in requirements[:limit] if limit else requirements:
            results.append({
                "requirement": req,
                "status": "UNKNOWN",
                "confidence": 0.0,
                "matchedCriteria": [],
                "missingCriteria": [f"backend evaluation failed: {str(e)}"],
                "evidence": [],
                "notes": f"Backend API call failed: {str(e)}"
            })
        return {"results": results}


def save_coverage_run(workspace_id: str, gdd_doc_id: str, code_batch_id: str, result: Dict) -> str:
    """Save coverage run results to JSON file."""
    from datetime import datetime

    run_id = f"{datetime.utcnow().strftime('%Y%m%d_%H%M%S')}_{gdd_doc_id}_{code_batch_id}"
    results_path = Path(f"data/workspaces/{workspace_id}/coverage_runs/{run_id}.json")

    run_data = {
        "schemaVersion": "1.0",
        "runId": run_id,
        "timestamp": datetime.utcnow().isoformat() + "Z",
        "workspaceId": workspace_id,
        "gddDocId": gdd_doc_id,
        "codeBatchId": code_batch_id,
        "mode": "dry-run" if result.get("dry_run") else "normal",
        **result
    }

    results_path.parent.mkdir(parents=True, exist_ok=True)
    with open(results_path, 'w') as f:
        json.dump(run_data, f, indent=2)

    print(f"[SAVE] Results saved to {results_path}")
    return str(results_path)


def main():
    parser = argparse.ArgumentParser(description="CLI runner for GDD coverage evaluation")
    parser.add_argument("--workspace", default="tank_war", help="Workspace ID (default: tank_war)")
    parser.add_argument("--gdd", help="GDD document ID")
    parser.add_argument("--code", help="Code batch ID")
    parser.add_argument("--mode", choices=["fast", "full"], default="fast", help="Evaluation mode (default: fast)")
    parser.add_argument("--topk", type=int, default=5, help="Top-K behaviors to retrieve (default: 5)")
    parser.add_argument("--limit", type=int, default=5, help="Max requirements for fast mode (default: 5)")
    parser.add_argument("--dry-run", action="store_true", help="Skip LLM calls for debugging")
    parser.add_argument("--force-gdd", action="store_true", help="Force re-extraction of GDD spec")
    parser.add_argument("--force-code", action="store_true", help="Force re-indexing of code behaviors")
    parser.add_argument("--list", action="store_true", help="List available workspaces, GDD docs, and code batches")

    args = parser.parse_args()

    if args.list:
        list_available_resources()
        return

    # Validate required arguments for coverage run
    if not args.gdd or not args.code:
        print("ERROR: --gdd and --code are required for coverage evaluation")
        sys.exit(1)

    print(f"[START] Coverage run")
    print(f"[INPUT] workspace={args.workspace} gdd={args.gdd} code={args.code} mode={args.mode} topk={args.topk} limit={args.limit}")

    start_time = time.time()
    current_stage = "init"

    # Start heartbeat thread
    heartbeat = HeartbeatThread(lambda: current_stage)
    heartbeat.start()

    try:
        # Stage 1: Load/extract requirements
        current_stage = "requirements"
        print("[STAGE] Loading/extracting requirements")
        requirements = load_or_extract_requirements(args.workspace, args.gdd, dry_run=args.dry_run)
        print(f"[OK] Loaded {len(requirements)} requirements")

        # Stage 2: Load/build behavior index
        current_stage = "indexing"
        print("[STAGE] Loading/building behavior index")
        behaviors = load_or_build_behavior_index(args.workspace, args.code, dry_run=args.dry_run)
        print(f"[OK] Loaded {len(behaviors)} behaviors")

        # Stage 3: Run evaluation via backend API
        current_stage = "evaluation"
        print("[STAGE] Running coverage evaluation via backend API")

        import requests
        backend_url = "http://localhost:8000"

        payload = {
            "workspaceId": args.workspace,
            "gddDocId": args.gdd,
            "codeBatchId": args.code,
            "mode": args.mode,
            "topK": args.topk,
            "maxRequirements": args.limit if args.mode == "fast" else None,
            "forceGddSpec": getattr(args, 'force_gdd', False),
            "forceCodeIndex": getattr(args, 'force_code', False)
        }

        try:
            response = requests.post(f"{backend_url}/coverage/run", json=payload, timeout=600)
            if response.status_code == 200:
                result = response.json()
                print(f"[OK] Evaluation complete - {result.get('summary', {}).get('totalRequirements', 0)} requirements processed")

                # Display cache information
                cache_info = result.get('cache', {})
                if cache_info.get('gddSpecReused') is not None:
                    print(f"[CACHE] GDD spec reused: {cache_info['gddSpecReused']}")
                if cache_info.get('behaviorIndexReused') is not None:
                    print(f"[CACHE] Behavior index reused: {cache_info['behaviorIndexReused']}")
                if cache_info.get('reindexedFilesCount', 0) > 0:
                    print(f"[CACHE] Files re-indexed: {cache_info['reindexedFilesCount']}")

            else:
                raise RuntimeError(f"Backend returned status {response.status_code}: {response.text}")
        except Exception as e:
            print(f"[ERROR] Failed to evaluate via backend API: {e}")
            # Fallback to mock result for testing
            result = {
                "results": [],
                "summary": {"totalRequirements": 0, "implemented": 0, "partial": 0, "missing": 0},
                "cache": {"gddSpecReused": False, "behaviorIndexReused": False, "reindexedFilesCount": 0}
            }

        # Stage 4: Save results
        current_stage = "saving"
        print("[STAGE] Saving results")
        output_path = save_coverage_run(args.workspace, args.gdd, args.code, result)
        print(f"[OK] Results saved to {output_path}")

        # Get summary from backend response
        summary = result.get("summary", {})
        implemented = summary.get("implemented", 0)
        partial = summary.get("partial", 0)
        missing = summary.get("missing", 0)

        elapsed = time.time() - start_time
        print(f"[DONE] Coverage run completed in {elapsed:.2f}s")
        print(f"[SUMMARY] {implemented} implemented, {partial} partial, {missing} missing")

    except Exception as e:
        elapsed = time.time() - start_time
        print(f"[ERROR] Coverage run failed after {elapsed:.2f}s: {e}")
        import traceback
        traceback.print_exc()
        sys.exit(1)
    finally:
        heartbeat.stop()
        heartbeat.join(timeout=1)


if __name__ == "__main__":
    main()
