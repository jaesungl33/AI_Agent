"""
Gradio UI Application for GDD RAG Backbone

This is a Gradio-based web interface that provides:
- Document indexing and management
- GDD analysis and exploration
- Structured requirement extraction
- Code coverage evaluation

Can be shared via Gradio's public link feature for easy collaboration.
"""
from __future__ import annotations

# Standard library imports
import asyncio
import json
import sys
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, List, Optional

# Third-party imports
import gradio as gr
import pandas as pd

# Project root path setup for imports
PROJECT_ROOT = Path(__file__).resolve().parents[1]
if str(PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(PROJECT_ROOT))

try:
    from gdd_rag_backbone.config import DEFAULT_DOCS_DIR
    from gdd_rag_backbone.llm_providers import (
        QwenProvider,
        make_embedding_func,
        make_llm_model_func,
    )
    from gdd_rag_backbone.rag_backend import indexing
    from gdd_rag_backbone.rag_backend.chunk_qa import (
        ask_across_docs,
        ask_with_chunks,
        list_indexed_docs,
        preview_chunks,
        get_top_chunks,
    )
    from gdd_rag_backbone.gdd.analysis import analyze_gdd
    from gdd_rag_backbone.gdd.extraction import extract_all_requirements
    from gdd_rag_backbone.gdd.todo import generate_todo_list
    from gdd_rag_backbone.gdd.schemas import (
        GddRequirement,
        GddObject,
        GddSystem,
        GddInteraction,
    )
    from gdd_rag_backbone.gdd.requirement_matching import (
        evaluate_all_requirements,
        search_code_chunks,
        classify_requirement_coverage,
    )
except ImportError as exc:
    raise ImportError(f"Failed to import project modules: {exc}") from exc

# Directory configuration for output files
CHECKLIST_DIR = Path("checklists")
REPORT_DIR = Path("reports") / "coverage_checks"

APP_CSS = """
.gradio-container {
    font-family: 'Inter', 'Segoe UI', system-ui, -apple-system, BlinkMacSystemFont, sans-serif;
    background: #0d1117;
    color: #f5f7fb;
}
.block.padded {
    padding: 1.25rem;
    background: #111827;
    border-radius: 18px;
    box-shadow: 0 8px 20px rgba(0, 0, 0, 0.25);
}
.gr-button {
    width: 150px !important;
    padding: 0.35rem 0.9rem;
    border-radius: 999px;
    justify-content: center;
    font-weight: 600;
}
.gr-button-secondary {
    background: rgba(255,255,255,0.08) !important;
    color: #f5f7fb !important;
}
.gr-textbox, .gr-dropdown, .gr-slider, .gr-box, .gr-chatbot, .gr-markdown, .gr-checkbox, .gr-text, .gr-panel {
    background: #0f172a;
    border-radius: 16px;
    border: 1px solid rgba(255,255,255,0.08);
}
.gr-box,
.gr-block.gr-markdown,
.gr-block.gr-panel,
.gr-markdown,
.gr-markdown > p,
.gr-panel,
.gr-group .gr-block,
.gr-group .gr-panel,
.gr-group .gr-box {
    background: transparent !important;
    border: none !important;
    box-shadow: none !important;
}
.gr-markdown h1, .gr-markdown h2, .gr-markdown h3 {
    color: #e2e8f0;
}
.gr-markdown p {
    color: #cbd5f5;
}
.gr-tab-nav {
    background: transparent;
}
.tabs > div {
    border-bottom: 1px solid rgba(255,255,255,0.08);
}
.tabs button {
    font-weight: 600;
    color: #94a3b8;
}
.tabs button.svelte-1vi7rhd[data-selected="true"] {
    color: #ffffff;
    border-bottom: 2px solid #6366f1;
}
.gr-group {
    background: transparent;
    border: none;
    padding: 0;
    gap: 0.75rem;
}
.gr-box > .prose * {
    color: #cbd5f5;
}
.gr-markdown > h3 {
    margin-top: 0;
}
"""


# -----------------------------------------------------------------------------
# Utility Functions
# -----------------------------------------------------------------------------

def _async_run(coro):
    """Execute an async coroutine, handling event loop conflicts."""
    try:
        return asyncio.run(coro)
    except RuntimeError:
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)
        try:
            return loop.run_until_complete(coro)
        finally:
            loop.close()


# Cache provider bundle
_provider_cache = None


def _provider_bundle():
    """Initialize and cache LLM provider bundle."""
    global _provider_cache
    if _provider_cache is None:
        provider = QwenProvider()
        _provider_cache = {
            "provider": provider,
            "llm_func": make_llm_model_func(provider),
            "embedding_func": make_embedding_func(provider),
        }
    return _provider_cache


def _doc_options() -> List[dict]:
    """Retrieve list of all indexed documents."""
    return list_indexed_docs()


def _make_doc_id(file_name: str) -> str:
    """Generate a clean document ID from a file name."""
    base = Path(file_name).stem.lower().replace(" ", "_")
    cleaned = "".join(c if c.isalnum() or c in "_-" else "_" for c in base)
    while "__" in cleaned:
        cleaned = cleaned.replace("__", "_")
    return cleaned.strip("_") or "gdd_doc"


def _game_spec_path(doc_id: str) -> Path:
    """Get the file path for storing the unified Game Spec (objects, systems, logic rules, requirements)."""
    CHECKLIST_DIR.mkdir(parents=True, exist_ok=True)
    return CHECKLIST_DIR / f"{doc_id}_game_spec.json"


def _requirements_path(doc_id: str) -> Path:
    """Get the file path for storing extracted requirements (backwards compatibility)."""
    return _game_spec_path(doc_id)


def _todo_path(doc_id: str) -> Path:
    """Get the file path for storing todo lists."""
    CHECKLIST_DIR.mkdir(parents=True, exist_ok=True)
    return CHECKLIST_DIR / f"{doc_id}_todo.json"


def _load_game_spec(doc_id: str) -> Optional[Dict[str, Any]]:
    """Load the unified Game Spec for a document."""
    path = _game_spec_path(doc_id)
    if not path.exists():
        return None
    return json.loads(path.read_text())


def _save_game_spec(doc_id: str, payload: Dict[str, Any], metadata: Optional[Dict[str, Any]] = None) -> Path:
    """Save the unified Game Spec with metadata (timestamp, version, etc.)."""
    spec = {
        "doc_id": doc_id,
        "metadata": {
            "created_at": datetime.now().isoformat(),
            "version": "1.0",
            **(metadata or {})
        },
        "spec": payload  # Contains objects, systems, logic_rules, requirements
    }
    path = _game_spec_path(doc_id)
    path.write_text(json.dumps(spec, indent=2, ensure_ascii=False))
    return path


def _load_structured_requirements(doc_id: str) -> Optional[Dict[str, Any]]:
    """Load structured requirements (backwards compatibility - loads from Game Spec)."""
    spec = _load_game_spec(doc_id)
    if spec is None:
        return None
    # Return the spec dict directly (backwards compatible)
    return spec.get("spec", spec)


def _save_structured_requirements(doc_id: str, payload: Dict[str, Any]) -> Path:
    """Save structured requirements (backwards compatibility - saves to Game Spec)."""
    return _save_game_spec(doc_id, payload)


def _save_todo_list(doc_id: str, todo_items: List[dict]) -> Path:
    path = _todo_path(doc_id)
    path.write_text(json.dumps(todo_items, indent=2, ensure_ascii=False))
    return path


def _load_coverage_report(doc_id: str, code_index_id: str) -> Optional[Dict[str, Any]]:
    report_path = REPORT_DIR / f"{doc_id}_{code_index_id}_coverage.json"
    if not report_path.exists():
        return None
    return json.loads(report_path.read_text())


# -----------------------------------------------------------------------------
# UI Component Functions
# -----------------------------------------------------------------------------

def _auto_extract_game_spec(doc_id: str) -> tuple[str, Optional[Dict[str, Any]]]:
    """Automatically extract and save Game Spec after indexing."""
    try:
        provider_data = _provider_bundle()
        payload = _async_run(extract_all_requirements(doc_id))
        save_path = _save_game_spec(doc_id, payload, metadata={"auto_extracted": True})
        return f"✅ Game Spec extracted and saved to {save_path}", payload
    except Exception as e:
        import traceback
        return f"⚠️ Auto-extraction failed: {e}\n\n{traceback.format_exc()}", None


def handle_indexing_upload(uploaded_file, doc_id_input):
    """Handle document upload and indexing with automatic Game Spec extraction."""
    if not uploaded_file:
        return "❌ Please upload a document first.", None
    
    if not doc_id_input or not doc_id_input.strip():
        return "❌ Please enter a document ID.", None
    
    try:
        provider_data = _provider_bundle()
        doc_id = doc_id_input.strip()
        
        # Gradio File component returns a tuple: (temp_file_path, file_name)
        # Or just a file path string in newer versions
        if isinstance(uploaded_file, tuple):
            file_path_str, file_name = uploaded_file
        elif isinstance(uploaded_file, str):
            file_path_str = uploaded_file
            file_name = Path(uploaded_file).name
        else:
            return "❌ Invalid file format.", None
        
        extension = Path(file_name).suffix or ".pdf"
        doc_path = DEFAULT_DOCS_DIR / f"{doc_id}{extension}"
        DEFAULT_DOCS_DIR.mkdir(parents=True, exist_ok=True)
        
        # Copy uploaded file to destination
        import shutil
        shutil.copy(file_path_str, doc_path)
        
        # Step 1: Index document
        _async_run(
            indexing.index_document(
                doc_path=doc_path,
                doc_id=doc_id,
                llm_func=provider_data["llm_func"],
                embedding_func=provider_data["embedding_func"],
            )
        )
        
        # Step 2: Auto-extract Game Spec
        extract_status, spec_payload = _auto_extract_game_spec(doc_id)
        
        status_msg = (
            f"✅ {doc_id} indexed successfully at {datetime.now():%Y-%m-%d %H:%M:%S}!\n\n"
            f"📋 {extract_status}"
        )
        
        return status_msg, doc_id
    except Exception as e:
        import traceback
        return f"❌ Indexing failed: {e}\n\n{traceback.format_exc()}", None


def handle_indexing_reindex(selected_doc):
    """Handle re-indexing of an existing document with automatic Game Spec extraction."""
    if not selected_doc or selected_doc == "":
        return "❌ Please select a document.", gr.update()
    
    # Preserve the selection throughout
    preserved_selection = selected_doc
    
    try:
        provider_data = _provider_bundle()
        docs = _doc_options()
        meta = next((doc for doc in docs if doc["doc_id"] == selected_doc), None)
        
        if not meta or not meta.get("file_path"):
            return f"❌ Could not find file path for {selected_doc}.", gr.update(choices=get_doc_choices(), value=preserved_selection)
        
        doc_path = DEFAULT_DOCS_DIR / meta["file_path"]
        if not doc_path.exists():
            return f"❌ File not found: {doc_path}", gr.update(choices=get_doc_choices(), value=preserved_selection)
        
        # Step 1: Re-index document
        _async_run(
            indexing.index_document(
                doc_path=doc_path,
                doc_id=selected_doc,
                llm_func=provider_data["llm_func"],
                embedding_func=provider_data["embedding_func"],
            )
        )
        
        # Step 2: Auto-extract Game Spec
        extract_status, spec_payload = _auto_extract_game_spec(selected_doc)
        
        status_msg = (
            f"✅ {selected_doc} re-indexed successfully at {datetime.now():%Y-%m-%d %H:%M:%S}!\n\n"
            f"📋 {extract_status}"
        )
        
        # Return status and keep the same selection
        return status_msg, gr.update(choices=get_doc_choices(), value=preserved_selection)
    except Exception as e:
        import traceback
        return f"❌ Re-indexing failed: {e}\n\n{traceback.format_exc()}", gr.update(choices=get_doc_choices(), value=preserved_selection)


def qa_query(question, use_all_docs, doc_ids_list, top_k):
    """Handle question-answering query."""
    if not question or not question.strip():
        error = "❌ Please enter a question."
        return error, f"**Error:**\n\n{error}", ""
    
    # Determine which documents to query
    if use_all_docs:
        doc_ids_to_query = get_doc_choices()
        if not doc_ids_to_query:
            error = "❌ No documents available. Index at least one document first."
            return error, f"**Error:**\n\n{error}", ""
    else:
        if not doc_ids_list or (isinstance(doc_ids_list, list) and len(doc_ids_list) == 0):
            error = "❌ No documents selected. Select documents or check 'Use All Documents'."
            return error, f"**Error:**\n\n{error}", ""
        doc_ids_to_query = doc_ids_list if isinstance(doc_ids_list, list) else [doc_ids_list]
    
    try:
        provider_data = _provider_bundle()
        result = ask_across_docs(
            doc_ids_to_query,
            question.strip(),
            provider=provider_data["provider"],
            top_k=max(top_k, 4),
        )
        
        answer = result.get("answer", "")
        
        # Build chunk reference list (chunk IDs only)
        chunks = [
            f"- `{ctx.get('chunk_id', 'unknown')}` (doc: {ctx.get('doc_id', 'unknown')}, score={ctx.get('score', 0):.3f})"
            for ctx in result.get("context", [])
        ]
        chunk_refs = "\n".join(chunks) if chunks else "No chunk references."
        
        # Just return the clean answer - no headers, no duplicates
        formatted_answer = answer.strip() if answer else "No answer generated."
        
        return "✅ Query successful!", formatted_answer, chunk_refs
    except Exception as e:
        import traceback
        error_msg = f"❌ **Query Failed**\n\n**Error:** {str(e)}\n\n"
        if "No documents" in str(e) or "not found" in str(e).lower():
            error_msg += "💡 **Tip:** Make sure you have indexed documents and selected the correct documents."
        error_details = f"**Error Details:**\n```\n{traceback.format_exc()}\n```"
        return f"❌ {error_msg}", f"**Error:**\n\n{error_msg}", error_details


def analyze_document(use_all_docs, selected_doc):
    """Generate high-level analysis of a document or all documents."""
    try:
        from gdd_rag_backbone.llm_providers import make_llm_model_func
        provider_data = _provider_bundle()
        provider = provider_data["provider"]
        llm_func = make_llm_model_func(provider)
        
        # Determine which documents to analyze
        if use_all_docs:
            doc_ids = get_doc_choices()
            if not doc_ids:
                return "❌ No documents available. Index at least one document first."
        else:
            if not selected_doc:
                return "❌ Please select a document or choose 'Analyze All Documents'."
            doc_ids = [selected_doc]
        
        # Use the same analysis approach but with multiple docs
        ANALYSIS_QUERY = (
            "Produce a comprehensive understanding of this game: genre, core loop, "
            "major systems, player interactions, key objects/entities, maps/modes, and special mechanics."
        )
        
        SYSTEM_PROMPT = (
            "You are a professional game designer. Using ONLY the retrieved context, "
            "generate a deep understanding of this game's design: genre, core gameplay loop, "
            "main systems and subsystems, player interactions, key objects/entities, maps and modes, "
            "and special mechanics. Do NOT invent details. If unsure, say 'unknown'."
        )
        
        # Get chunks from all selected documents
        chunks = get_top_chunks(doc_ids, ANALYSIS_QUERY, provider=provider, top_k=8)
        if not chunks:
            doc_list = ", ".join(doc_ids) if len(doc_ids) > 1 else doc_ids[0]
            return f"❌ No indexed chunks found for document(s). Please index the documents first."
        
        context = "\n\n".join(chunk["content"] for chunk in chunks)
        prompt = (
            "Using ONLY the following context, provide the requested analysis.\n\n"
            f"Context:\n{context}\n\nAnswer:"
        )
        
        # Generate analysis
        summary = _async_run(llm_func(prompt=prompt, system_prompt=SYSTEM_PROMPT, temperature=0.3))
        
        if not summary or not summary.strip():
            return "⚠️ Analysis generated but returned empty. Please try again."
        
        # Add header showing which docs were analyzed
        if use_all_docs:
            header = f"## 📊 Analysis of All Documents ({len(doc_ids)} documents)\n\n"
        else:
            header = f"## 📊 Analysis of: {selected_doc}\n\n"
        
        return header + summary.strip()
    except Exception as e:
        import traceback
        error_msg = f"❌ **Analysis Failed**\n\n**Error:** {str(e)}\n\n"
        error_msg += f"**Details:**\n```\n{traceback.format_exc()}\n```"
        return error_msg


def extract_requirements(doc_id):
    """Extract structured requirements from a document."""
    if not doc_id:
        return "❌ Please select a document first.", None, None, None, None
    
    try:
        payload = _async_run(extract_all_requirements(doc_id))
        save_path = _save_structured_requirements(doc_id, payload)
        
        # Convert to DataFrames
        objects_df = pd.DataFrame(payload.get("objects", [])) if payload.get("objects") else pd.DataFrame()
        systems_df = pd.DataFrame(payload.get("systems", [])) if payload.get("systems") else pd.DataFrame()
        logic_rules_df = pd.DataFrame(payload.get("logic_rules", [])) if payload.get("logic_rules") else pd.DataFrame()
        requirements_df = pd.DataFrame(payload.get("requirements", [])) if payload.get("requirements") else pd.DataFrame()
        
        return (
            f"✅ Extraction saved to {save_path}",
            objects_df,
            systems_df,
            logic_rules_df,
            requirements_df,
        )
    except Exception as e:
        return f"❌ Extraction failed: {e}", None, None, None, None


def generate_todo(doc_id):
    """Generate todo list from extracted requirements."""
    if not doc_id:
        return "❌ Please select a document first.", None
    
    structured = _load_structured_requirements(doc_id)
    if not structured:
        return "❌ Run structured extraction first.", None
    
    try:
        todo_items = _async_run(generate_todo_list(structured))
        if todo_items:
            _save_todo_list(doc_id, todo_items)
            todo_df = pd.DataFrame(todo_items)
            return "✅ To-do list generated successfully!", todo_df
        else:
            return "⚠️ LLM did not return any to-do items.", None
    except Exception as e:
        return f"❌ Failed to generate to-do list: {e}", None


async def _generate_item_queries(item_dict: dict, item_type: str) -> List[str]:
    """Generate search queries for a spec item (object, system, logic rule, or requirement)."""
    queries = []
    
    if item_type == "object":
        if item_dict.get("name"):
            queries.append(item_dict["name"])
        if item_dict.get("description"):
            queries.append(item_dict["description"])
        if item_dict.get("category"):
            queries.append(f"{item_dict['category']} {item_dict.get('name', '')}".strip())
        if item_dict.get("special_rules"):
            queries.append(item_dict["special_rules"])
    
    elif item_type == "system":
        if item_dict.get("name"):
            queries.append(item_dict["name"])
        if item_dict.get("description"):
            queries.append(item_dict["description"])
        if item_dict.get("mechanics"):
            queries.append(item_dict["mechanics"])
        if item_dict.get("objectives"):
            queries.append(item_dict["objectives"])
    
    elif item_type == "logic_rule":
        if item_dict.get("summary"):
            queries.append(item_dict["summary"])
        if item_dict.get("description"):
            queries.append(item_dict["description"])
        if item_dict.get("trigger"):
            queries.append(item_dict["trigger"])
        if item_dict.get("effect"):
            queries.append(item_dict["effect"])
    
    elif item_type == "requirement":
        if item_dict.get("title"):
            queries.append(item_dict["title"])
        if item_dict.get("description"):
            queries.append(item_dict["description"])
        if item_dict.get("acceptance_criteria"):
            queries.append(item_dict["acceptance_criteria"])
    
    return [q for q in queries if q and q.strip()]


async def _classify_item_coverage(item_dict: dict, item_type: str, item_id: str, code_chunks: List[Dict[str, Any]], llm_func) -> Dict[str, Any]:
    """Classify if a spec item exists in code (simplified - just checks existence, not correctness)."""
    system_prompt = (
        "You are a code reviewer. Check if the code contains references to or implementations of the described item. "
        "Do NOT check if it works correctly. Only check if it EXISTS in the code. "
        "If there are relevant code chunks mentioning or implementing this item, respond with 'implemented'. "
        "If there are no relevant chunks, respond with 'not_implemented'. "
        "Do NOT guess. If insufficient evidence, use 'not_implemented'."
    )
    
    item_summary = json.dumps(item_dict, indent=2)
    
    if code_chunks:
        snippet_lines = []
        for idx, chunk in enumerate(code_chunks[:8]):
            snippet = chunk.get("content", "")
            snippet_lines.append(
                f"[Chunk {idx + 1}] id={chunk.get('chunk_id')} score={chunk.get('score', 0):.3f}\n{snippet[:1200]}"
            )
        code_context = "\n\n".join(snippet_lines)
    else:
        code_context = "No relevant code chunks were retrieved."
    
    user_prompt = f"""
{item_type.replace('_', ' ').title()}:
{item_summary}

Candidate Code:
{code_context}

Classify: Does this code contain references to or implementations of the {item_type}? 
Respond with ONLY: "implemented" or "not_implemented".

Return ONLY JSON:
{{
  "item_id": "{item_id}",
  "item_type": "{item_type}",
  "status": "implemented/not_implemented",
  "evidence": [
    {{
      "file": "path/to/file.ext or chunk_id",
      "reason": "Brief reason"
    }}
  ]
}}
"""
    
    response = await llm_func(prompt=user_prompt, system_prompt=system_prompt, temperature=0.1)
    text = response.strip()
    if text.startswith("```"):
        lines = text.split("\n")
        text = "\n".join(lines[1:-1]) if lines[-1].strip() == "```" else "\n".join(lines[1:])
    
    try:
        payload = json.loads(text)
    except json.JSONDecodeError:
        payload = {
            "item_id": item_id,
            "item_type": item_type,
            "status": "error",
            "evidence": [{"file": None, "reason": "LLM response could not be parsed"}],
        }
    
    payload.setdefault("item_id", item_id)
    payload.setdefault("item_type", item_type)
    payload.setdefault("evidence", [])
    return payload


async def _evaluate_all_spec_items(doc_id: str, code_index_id: str, top_k: int) -> Dict[str, Any]:
    """Evaluate coverage for all items in the Game Spec (objects, systems, logic rules, requirements)."""
    from gdd_rag_backbone.llm_providers import make_llm_model_func
    
    spec = _load_game_spec(doc_id)
    if not spec:
        raise ValueError(f"No Game Spec found for {doc_id}. Please upload/re-index the document first.")
    
    spec_data = spec.get("spec", spec)  # Handle both old and new format
    
    provider_data = _provider_bundle()
    provider = provider_data["provider"]
    llm_func = make_llm_model_func(provider)
    
    all_results = []
    
    # Evaluate Objects
    objects = spec_data.get("objects", [])
    for obj_dict in objects:
        obj = GddObject(**obj_dict)
        queries = await _generate_item_queries(obj_dict, "object")
        if queries:
            chunks = await search_code_chunks(queries, code_index_id, provider=provider, top_k=top_k)
            result = await _classify_item_coverage(obj_dict, "object", obj.id, chunks, llm_func)
            result["retrieved_chunks"] = chunks
            result["item_name"] = obj.name
            all_results.append(result)
    
    # Evaluate Systems
    systems = spec_data.get("systems", [])
    for sys_dict in systems:
        sys_obj = GddSystem(**sys_dict)
        queries = await _generate_item_queries(sys_dict, "system")
        if queries:
            chunks = await search_code_chunks(queries, code_index_id, provider=provider, top_k=top_k)
            result = await _classify_item_coverage(sys_dict, "system", sys_obj.id, chunks, llm_func)
            result["retrieved_chunks"] = chunks
            result["item_name"] = sys_obj.name
            all_results.append(result)
    
    # Evaluate Logic Rules
    logic_rules = spec_data.get("logic_rules", [])
    for rule_dict in logic_rules:
        rule = GddInteraction(**rule_dict)
        queries = await _generate_item_queries(rule_dict, "logic_rule")
        if queries:
            chunks = await search_code_chunks(queries, code_index_id, provider=provider, top_k=top_k)
            result = await _classify_item_coverage(rule_dict, "logic_rule", rule.id, chunks, llm_func)
            result["retrieved_chunks"] = chunks
            result["item_name"] = rule.summary
            all_results.append(result)
    
    # Evaluate Requirements
    requirements = spec_data.get("requirements", [])
    for req_dict in requirements:
        req = GddRequirement(**req_dict)
        queries = await _generate_item_queries(req_dict, "requirement")
        if queries:
            chunks = await search_code_chunks(queries, code_index_id, provider=provider, top_k=top_k)
            result = await _classify_item_coverage(req_dict, "requirement", req.id, chunks, llm_func)
            result["retrieved_chunks"] = chunks
            result["item_name"] = req.title
            all_results.append(result)
    
    return {
        "doc_id": doc_id,
        "code_index_id": code_index_id,
        "results": all_results,
        "summary": {
            "total_items": len(all_results),
            "implemented": sum(1 for r in all_results if r.get("status") == "implemented"),
            "not_implemented": sum(1 for r in all_results if r.get("status") == "not_implemented"),
            "errors": sum(1 for r in all_results if r.get("status") == "error"),
        }
    }


def evaluate_coverage(doc_id, code_index_id, top_k):
    """Evaluate code coverage for all Game Spec items (objects, systems, logic rules, requirements)."""
    if not doc_id:
        return "❌ Please select a document first.", None, None
    
    spec = _load_game_spec(doc_id)
    if not spec:
        return "❌ Game Spec not found. Please upload/re-index the document to auto-extract the Game Spec.", None, None
    
    if not code_index_id.strip():
        return "❌ Please enter a code index ID.", None, None
    
    try:
        REPORT_DIR.mkdir(parents=True, exist_ok=True)
        
        # Evaluate all spec items
        report_payload = _async_run(_evaluate_all_spec_items(doc_id, code_index_id.strip(), top_k))
        
        # Save report
        report_path = REPORT_DIR / f"{doc_id}_{code_index_id.strip()}_coverage.json"
        report_path.write_text(json.dumps(report_payload, indent=2, ensure_ascii=False))
        
        if report_payload and report_payload.get("results"):
            results = report_payload["results"]
            
            # Create a flattened DataFrame for display
            display_rows = []
            for result in results:
                display_rows.append({
                    "item_id": result.get("item_id", ""),
                    "item_type": result.get("item_type", ""),
                    "item_name": result.get("item_name", ""),
                    "status": result.get("status", "unknown"),
                    "evidence_count": len(result.get("evidence", [])),
                    "chunk_count": len(result.get("retrieved_chunks", [])),
                })
            
            results_df = pd.DataFrame(display_rows)
            
            summary = report_payload.get("summary", {})
            status_msg = (
                f"✅ Coverage report saved to {report_path}\n\n"
                f"📊 Summary: {summary.get('implemented', 0)}/{summary.get('total_items', 0)} items implemented "
                f"({summary.get('not_implemented', 0)} not implemented)"
            )
            
            return status_msg, results_df, report_payload
        else:
            return f"✅ Coverage report saved to {report_path}", None, None
    except Exception as e:
        import traceback
        return f"❌ Coverage evaluation failed: {e}\n\n{traceback.format_exc()}", None, None


def get_coverage_details(selected_item_id, coverage_results_store):
    """Get detailed coverage information for a specific item (object, system, logic rule, or requirement)."""
    if not selected_item_id or coverage_results_store is None:
        return "No item selected or no results available."
    
    try:
        results = coverage_results_store.get("results", [])
        item_result = next((r for r in results if r.get("item_id") == selected_item_id), None)
        
        if not item_result:
            return f"Item {selected_item_id} not found in results."
        
        details = f"**Item ID:** {item_result.get('item_id', 'Unknown')}\n"
        details += f"**Item Type:** {item_result.get('item_type', 'Unknown')}\n"
        details += f"**Item Name:** {item_result.get('item_name', 'Unknown')}\n"
        details += f"**Status:** {item_result.get('status', 'Unknown')}\n\n"
        
        evidence = item_result.get("evidence", [])
        if evidence:
            details += "**Evidence:**\n"
            for ev in evidence:
                details += f"- **File:** {ev.get('file', 'N/A')}\n"
                details += f"  **Reason:** {ev.get('reason', 'N/A')}\n\n"
        
        retrieved = item_result.get("retrieved_chunks", [])
        if retrieved:
            details += f"**Retrieved Code Chunks ({len(retrieved)}):**\n\n"
            for idx, chunk in enumerate(retrieved[:5], 1):  # Show top 5 chunks
                details += f"**Chunk {idx}:** `{chunk.get('chunk_id', 'unknown')}` (score={chunk.get('score', 0):.3f})\n"
                content = chunk.get('content', '')
                details += f"```\n{content[:500]}{'...' if len(content) > 500 else ''}\n```\n\n"
        
        return details
    except Exception as e:
        import traceback
        return f"❌ Error retrieving details: {e}\n\n```\n{traceback.format_exc()}\n```"




# -----------------------------------------------------------------------------
# Gradio Interface
# -----------------------------------------------------------------------------

def get_doc_choices():
    """Helper to get current document choices."""
    return [doc["doc_id"] for doc in _doc_options()]


def create_interface():
    """Create and launch the Gradio interface."""
    
    with gr.Blocks(title="GDD RAG Backbone", theme=gr.themes.Soft(), css=APP_CSS) as app:
        gr.Markdown(
            "# 🎮 GDD RAG Backbone\n\n"
            "**A comprehensive framework for processing Game Design Documents**\n\n"
            "**Improved Workflow:** Upload a GDD → automatically extracts all functions, structures, systems, and requirements → "
            "compare everything against your codebase to see what's implemented.\n\n"
            "The system automatically builds a unified 'Game Spec' when you index documents, making code coverage checks simple and comprehensive."
        )
        
        # Initialize document dropdowns with current doc list
        doc_choices = get_doc_choices()
        qa_chat_history = gr.State([])
        
        with gr.Tabs():
            # Tab 1: Indexing
            with gr.Tab("1. GDD & Indexing"):
                gr.Markdown(
                    "## Document Indexing\n\n"
                    "Upload new documents or re-index existing ones. "
                    "**Game Spec extraction happens automatically after indexing** - all functions, structures, and requirements are extracted and stored."
                )
                
                with gr.Row():
                    with gr.Column():
                        gr.Markdown("### Upload New Document")
                        upload_file = gr.File(label="Upload GDD (PDF/DOCX)", file_types=[".pdf", ".docx"])
                        new_doc_id = gr.Textbox(label="Document ID", placeholder="e.g., my_gdd_document")
                        upload_btn = gr.Button("Upload & Index", variant="primary", elem_classes=["compact-btn"])
                        upload_status = gr.Textbox(
                            label="📊 Status",
                            interactive=False,
                            placeholder="Ready to upload and index documents...",
                            lines=3
                        )
                    
                    with gr.Column():
                        gr.Markdown("### Re-index Existing Document")
                        existing_docs = gr.Dropdown(
                            label="Select Document",
                            choices=doc_choices,
                            allow_custom_value=False,
                        )
                        refresh_docs_btn = gr.Button("🔄 Refresh List", size="sm", elem_classes=["compact-btn"])
                        reindex_btn = gr.Button("Re-index", variant="primary", elem_classes=["compact-btn"])
                        reindex_status = gr.Textbox(
                            label="📊 Status",
                            interactive=False,
                            placeholder="Ready to re-index documents...",
                            lines=3
                        )
                
                def refresh_existing_docs(current_selection=None):
                    """Refresh document list while preserving current selection."""
                    new_choices = get_doc_choices()
                    if current_selection and current_selection in new_choices:
                        return gr.update(choices=new_choices, value=current_selection)
                    return gr.update(choices=new_choices)
                
                upload_btn.click(
                    fn=lambda *args: ("⏳ Uploading and indexing document... This may take several minutes.", None),
                    inputs=[upload_file, new_doc_id],
                    outputs=[upload_status, existing_docs],
                ).then(
                    fn=handle_indexing_upload,
                    inputs=[upload_file, new_doc_id],
                    outputs=[upload_status, existing_docs],
                ).then(
                    fn=lambda new_doc_id, current: refresh_existing_docs(new_doc_id) if new_doc_id else refresh_existing_docs(current),
                    inputs=[new_doc_id, existing_docs],
                    outputs=[existing_docs],
                )
                
                reindex_btn.click(
                    fn=lambda doc: ("⏳ Re-indexing document... This may take several minutes.", doc),
                    inputs=[existing_docs],
                    outputs=[reindex_status, existing_docs],
                ).then(
                    fn=handle_indexing_reindex,
                    inputs=[existing_docs],
                    outputs=[reindex_status, existing_docs],
                )
                
                refresh_docs_btn.click(
                    fn=lambda current: refresh_existing_docs(current),
                    inputs=[existing_docs],
                    outputs=[existing_docs],
                )
            
            # Tab 2: Analysis & QA
            with gr.Tab("2. GDD Explorer & Analysis"):
                gr.Markdown("# 📊 Document Explorer & Analysis\n\nAnalyze documents and ask questions with the same clean layout as the indexing tab.")
                
                with gr.Row():
                    with gr.Column(scale=1, min_width=420):
                        with gr.Accordion("🔍 High-level Analysis", open=True):
                            gr.Markdown("Summarize one document or your entire corpus.")
                            
                            use_all_docs = gr.Checkbox(
                                label="📚 Analyze All Documents",
                                value=False,
                                info="✓ Check to analyze all indexed documents\n✗ Uncheck to analyze a specific document"
                            )
                            analysis_doc = gr.Dropdown(
                                label="📄 Select Document",
                                choices=doc_choices,
                                allow_custom_value=False,
                                visible=True,
                            )
                            analyze_btn = gr.Button("🚀 Analyze GDD", variant="primary", elem_classes=["compact-btn"])
                            analysis_status = gr.Textbox(
                                label="📊 Status",
                                interactive=False,
                                placeholder="Ready to analyze documents...",
                                visible=True
                            )
                            analysis_output = gr.Markdown(
                                label="📋 Analysis Results",
                                value="*Click 'Analyze GDD' to generate analysis...*",
                                show_copy_button=True
                            )
                    
                    with gr.Column(scale=1, min_width=420):
                        with gr.Accordion("💬 Ad-hoc Question Answering (Chat)", open=True):
                            gr.Markdown("Ask a question and receive a ChatGPT-style response with chunk references.")
                            
                            qa_use_all_docs = gr.Checkbox(
                                label="📚 Use All Documents",
                                value=False,
                                info="✓ Check to query all indexed documents\n✗ Uncheck to select specific documents"
                            )
                            qa_docs = gr.Dropdown(
                                label="📄 Select Documents",
                                choices=doc_choices,
                                multiselect=True,
                                visible=True,
                            )
                            qa_top_k = gr.Slider(
                                minimum=2,
                                maximum=10,
                                value=4,
                                step=1,
                                label="📊 Number of Chunks to Use",
                                info="More chunks = more context but may be slower"
                            )
                            qa_question = gr.Textbox(
                                label="💬 Ask a question",
                                placeholder="Example: Summarize the monetization strategy for Tank War.",
                                lines=3
                            )
                            with gr.Row():
                                qa_send_btn = gr.Button("Send", variant="primary", elem_classes=["compact-btn"])
                                qa_clear_btn = gr.Button("Clear", variant="secondary", elem_classes=["compact-btn"])
                            qa_status = gr.Markdown("Ready to answer questions.")
                            qa_chatbot = gr.Chatbot(
                                label="Conversation",
                                height=620,
                                show_copy_button=True,
                                type="tuples"
                            )
                
                # Update dropdown visibility based on checkbox
                def update_dropdown_visibility(use_all):
                    return gr.update(visible=not use_all)
                
                use_all_docs.change(
                    fn=update_dropdown_visibility,
                    inputs=[use_all_docs],
                    outputs=[analysis_doc],
                )
                
                analyze_btn.click(
                    fn=lambda use_all, doc: ("⏳ Analyzing documents... This may take a moment.", "*Analyzing... Please wait...*"),
                    inputs=[use_all_docs, analysis_doc],
                    outputs=[analysis_status, analysis_output],
                ).then(
                    fn=analyze_document,
                    inputs=[use_all_docs, analysis_doc],
                    outputs=[analysis_output],
                ).then(
                    fn=lambda result: "✅ Analysis complete!",
                    inputs=[analysis_output],
                    outputs=[analysis_status],
                )
                
                # Update QA dropdown visibility based on checkbox
                def update_qa_dropdown_visibility(use_all):
                    return gr.update(visible=not use_all)
                
                qa_use_all_docs.change(
                    fn=update_qa_dropdown_visibility,
                    inputs=[qa_use_all_docs],
                    outputs=[qa_docs],
                )
                
                def qa_chat_send(message, use_all, selected_docs, top_k, history):
                    if not message or not message.strip():
                        return "❌ Please enter a question.", history, message, history
                    
                    status, answer, chunk_refs = qa_query(
                        message,
                        use_all,
                        selected_docs,
                        top_k,
                    )
                    reply = f"{answer}\n\n**Chunk References:**\n{chunk_refs}"
                    new_history = history + [(message, reply)]
                    return status, new_history, "", new_history
                
                qa_send_btn.click(
                    fn=qa_chat_send,
                    inputs=[qa_question, qa_use_all_docs, qa_docs, qa_top_k, qa_chat_history],
                    outputs=[qa_status, qa_chatbot, qa_question, qa_chat_history],
                )
                
                def qa_clear_chat():
                    return "Ready to answer questions.", [], "", []
                
                qa_clear_btn.click(
                    fn=qa_clear_chat,
                    outputs=[qa_status, qa_chatbot, qa_question, qa_chat_history],
                )
                
                def refresh_analysis_docs():
                    new_choices = get_doc_choices()
                    return (
                        gr.update(choices=new_choices),  # analysis_doc
                        gr.update(choices=new_choices),  # qa_docs
                    )
                
                refresh_docs_btn2 = gr.Button("🔄 Refresh Document List", size="sm", elem_classes=["compact-btn"])
                refresh_docs_btn2.click(
                    fn=refresh_analysis_docs,
                    outputs=[analysis_doc, qa_docs],
                )
            
            # Tab 3: Requirements & To-Do
            with gr.Tab("3. Requirements & To-Do"):
                gr.Markdown(
                    "## Structured Extraction & To-Do\n\n"
                    "**Note:** Game Spec is automatically extracted when you upload/re-index documents. "
                    "You can manually re-run extraction here if needed, or generate developer to-do lists from the extracted spec."
                )
                
                req_doc = gr.Dropdown(
                    label="Select Document",
                    choices=doc_choices,
                    allow_custom_value=False,
                )
                extract_btn = gr.Button("Run Structured Extraction", variant="primary", elem_classes=["compact-btn"])
                extract_status = gr.Textbox(
                    label="📊 Status",
                    interactive=False,
                    placeholder="Ready to extract requirements...",
                    lines=2
                )
                
                with gr.Tabs():
                    with gr.Tab("Objects"):
                        objects_df = gr.Dataframe(label="Objects", interactive=False)
                    with gr.Tab("Systems"):
                        systems_df = gr.Dataframe(label="Systems", interactive=False)
                    with gr.Tab("Logic Rules"):
                        logic_rules_df = gr.Dataframe(label="Logic Rules", interactive=False)
                    with gr.Tab("Requirements"):
                        requirements_df = gr.Dataframe(label="Requirements", interactive=False)
                
                gr.Markdown("### Generate To-Do List")
                todo_btn = gr.Button("Generate Developer To-Do", variant="primary", elem_classes=["compact-btn"])
                todo_status = gr.Textbox(
                    label="📊 Status",
                    interactive=False,
                    placeholder="Ready to generate to-do list...",
                    lines=2
                )
                todo_df = gr.Dataframe(label="To-Do Items", interactive=False)
                
                extract_btn.click(
                    fn=lambda doc: ("⏳ Extracting structured data... This may take a few minutes.", pd.DataFrame(), pd.DataFrame(), pd.DataFrame(), pd.DataFrame()),
                    inputs=[req_doc],
                    outputs=[extract_status, objects_df, systems_df, logic_rules_df, requirements_df],
                ).then(
                    fn=extract_requirements,
                    inputs=[req_doc],
                    outputs=[extract_status, objects_df, systems_df, logic_rules_df, requirements_df],
                )
                
                todo_btn.click(
                    fn=lambda doc: ("⏳ Generating to-do list...", pd.DataFrame()),
                    inputs=[req_doc],
                    outputs=[todo_status, todo_df],
                ).then(
                    fn=generate_todo,
                    inputs=[req_doc],
                    outputs=[todo_status, todo_df],
                )
                
                def refresh_req_docs():
                    return gr.update(choices=get_doc_choices())
                
                refresh_docs_btn3 = gr.Button("🔄 Refresh Document List", size="sm", elem_classes=["compact-btn"])
                refresh_docs_btn3.click(
                    fn=refresh_req_docs,
                    outputs=[req_doc],
                )
            
            # Tab 4: Code Coverage
            with gr.Tab("4. Code Coverage"):
                gr.Markdown(
                    "## Code Coverage Check\n\n"
                    "Compare all Game Spec items (objects, systems, logic rules, requirements) against your codebase. "
                    "Checks if items exist in code - doesn't verify if they work correctly."
                )
                
                coverage_doc = gr.Dropdown(
                    label="Select Document",
                    choices=doc_choices,
                    allow_custom_value=False,
                )
                code_index_id = gr.Textbox(label="Code Index ID", placeholder="e.g., codebase", value="codebase")
                coverage_top_k = gr.Slider(minimum=4, maximum=12, value=8, step=1, label="Chunks per Query")
                coverage_btn = gr.Button("Run Coverage Evaluation", variant="primary", elem_classes=["compact-btn"])
                coverage_status = gr.Textbox(
                    label="📊 Status",
                    interactive=False,
                    placeholder="Ready to evaluate code coverage...",
                    lines=3
                )
                coverage_df = gr.Dataframe(label="Coverage Results", interactive=False)
                
                gr.Markdown("### Item Details")
                item_select = gr.Dropdown(
                    label="Select Item (Object/System/Logic Rule/Requirement)",
                    allow_custom_value=False,
                    info="Select an item to see detailed coverage information"
                )
                item_details = gr.Markdown(label="Details")
                
                # Store full results for details lookup
                coverage_results_store = gr.State()
                
                def update_item_dropdown(coverage_store):
                    """Update dropdown with all items from coverage results."""
                    if coverage_store is None or not coverage_store.get("results"):
                        return gr.update(choices=[])
                    
                    results = coverage_store.get("results", [])
                    # Create formatted choices showing type, name, and ID
                    choices = []
                    for r in results:
                        item_type = r.get('item_type', 'unknown')
                        item_name = r.get('item_name', r.get('item_id', 'unknown'))
                        item_id = r.get('item_id', 'unknown')
                        # Format: "type: name (id)"
                        choice_label = f"{item_type}: {item_name} ({item_id})"
                        choices.append(choice_label)
                    
                    return gr.update(choices=choices)
                
                def extract_item_id_from_choice(selected_choice):
                    """Extract item_id from the selected dropdown choice string."""
                    if not selected_choice:
                        return None
                    # Format is "type: name (id)" - extract the id part
                    import re
                    match = re.search(r'\(([^)]+)\)$', selected_choice)
                    if match:
                        return match.group(1)
                    return selected_choice
                
                coverage_btn.click(
                    fn=lambda *args: ("⏳ Evaluating code coverage for all Game Spec items... This may take several minutes.", pd.DataFrame(), None),
                    inputs=[coverage_doc, code_index_id, coverage_top_k],
                    outputs=[coverage_status, coverage_df, coverage_results_store],
                ).then(
                    fn=evaluate_coverage,
                    inputs=[coverage_doc, code_index_id, coverage_top_k],
                    outputs=[coverage_status, coverage_df, coverage_results_store],
                ).then(
                    fn=update_item_dropdown,
                    inputs=[coverage_results_store],
                    outputs=[item_select],
                )
                
                def get_item_details_wrapper(selected_choice, coverage_store):
                    """Wrapper to extract item_id from choice and get details."""
                    item_id = extract_item_id_from_choice(selected_choice)
                    return get_coverage_details(item_id, coverage_store)
                
                item_select.change(
                    fn=get_item_details_wrapper,
                    inputs=[item_select, coverage_results_store],
                    outputs=[item_details],
                )
                
                def refresh_coverage_docs():
                    return gr.update(choices=get_doc_choices())
                
                refresh_docs_btn4 = gr.Button("🔄 Refresh Document List", size="sm", elem_classes=["compact-btn"])
                refresh_docs_btn4.click(
                    fn=refresh_coverage_docs,
                    outputs=[coverage_doc],
                )
        
        return app


def main():
    """Main entry point for Gradio app."""
    import sys
    
    app = create_interface()
    print("\n" + "="*80, file=sys.stderr)
    print("🚀 Starting Gradio App...", file=sys.stderr)
    print("="*80, file=sys.stderr)
    
    # Launch with sharing enabled - this will print the share URL to stdout
    print("\n" + "="*80)
    print("🔗 PUBLIC SHARING LINK:")
    print("="*80)
    print("The public link will appear below when Gradio starts...")
    print("="*80 + "\n")
    
    # Flush to ensure output is visible
    sys.stdout.flush()
    sys.stderr.flush()
    
    app.launch(
        share=True,  # Enable public sharing link - this prints the URL
        server_name="0.0.0.0",  # Allow external access
        server_port=7860,  # Default Gradio port
        show_error=True,
    )


if __name__ == "__main__":
    main()

