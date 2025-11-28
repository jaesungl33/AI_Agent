"""
Streamlit UI Application for GDD RAG Backbone

This is the main entry point for the interactive web interface that provides:
- Document indexing and management
- GDD analysis and exploration
- Structured requirement extraction
- Code coverage evaluation

The application uses Streamlit to create a zero-code dashboard for the entire
GDD processing pipeline from ingestion to code coverage analysis.
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
import pandas as pd
import streamlit as st

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
    )
    from gdd_rag_backbone.gdd.analysis import analyze_gdd
    from gdd_rag_backbone.gdd.extraction import extract_all_requirements
    from gdd_rag_backbone.gdd.todo import generate_todo_list
    from gdd_rag_backbone.gdd.schemas import GddRequirement
    from gdd_rag_backbone.gdd.requirement_matching import evaluate_all_requirements
except ImportError as exc:  # pragma: no cover - Streamlit guard
    st.error(f"Failed to import project modules: {exc}")
    st.stop()

# Directory configuration for output files
CHECKLIST_DIR = Path("checklists")
REPORT_DIR = Path("reports") / "coverage_checks"


# -----------------------------------------------------------------------------
# Utility Functions
# -----------------------------------------------------------------------------

def _async_run(coro):
    """
    Execute an async coroutine, handling event loop conflicts.
    
    Streamlit may run in an environment with an existing event loop, so this
    function creates a new loop if needed to avoid RuntimeError.
    
    Args:
        coro: The coroutine to execute
        
    Returns:
        The result of the coroutine execution
    """
    try:
        return asyncio.run(coro)
    except RuntimeError:
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)
        try:
            return loop.run_until_complete(coro)
        finally:
            loop.close()


@st.cache_resource
def _provider_bundle():
    """
    Initialize and cache LLM provider bundle for the session.
    
    Uses Streamlit's cache_resource to ensure the provider is only initialized
    once per session, improving performance and avoiding redundant API setup.
    
    Returns:
        Dictionary containing:
            - provider: QwenProvider instance
            - llm_func: Async LLM function wrapper
            - embedding_func: Async embedding function wrapper
    """
    provider = QwenProvider()
    return {
        "provider": provider,
        "llm_func": make_llm_model_func(provider),
        "embedding_func": make_embedding_func(provider),
    }


def _doc_options() -> List[dict]:
    """
    Retrieve list of all indexed documents.
    
    Returns:
        List of dictionaries containing document metadata (doc_id, file_path, etc.)
    """
    return list_indexed_docs()


def _make_doc_id(file_name: str) -> str:
    """
    Generate a clean document ID from a file name.
    
    Converts file names to lowercase, replaces spaces with underscores,
    and removes special characters to create valid identifiers.
    
    Args:
        file_name: Original file name
        
    Returns:
        Clean document ID suitable for use as an identifier
    """
    base = Path(file_name).stem.lower().replace(" ", "_")
    cleaned = "".join(c if c.isalnum() or c in "_-" else "_" for c in base)
    while "__" in cleaned:
        cleaned = cleaned.replace("__", "_")
    return cleaned.strip("_") or "gdd_doc"


def _requirements_path(doc_id: str) -> Path:
    """
    Get the file path for storing extracted requirements.
    
    Args:
        doc_id: Document identifier
        
    Returns:
        Path object pointing to the requirements JSON file
    """
    CHECKLIST_DIR.mkdir(parents=True, exist_ok=True)
    return CHECKLIST_DIR / f"{doc_id}_requirements.json"


def _todo_path(doc_id: str) -> Path:
    """
    Get the file path for storing todo lists.
    
    Args:
        doc_id: Document identifier
        
    Returns:
        Path object pointing to the todo JSON file
    """
    CHECKLIST_DIR.mkdir(parents=True, exist_ok=True)
    return CHECKLIST_DIR / f"{doc_id}_todo.json"


def _load_structured_requirements(doc_id: str) -> Optional[Dict[str, Any]]:
    path = _requirements_path(doc_id)
    if not path.exists():
        return None
    return json.loads(path.read_text())


def _save_structured_requirements(doc_id: str, payload: Dict[str, Any]) -> Path:
    path = _requirements_path(doc_id)
    path.write_text(json.dumps(payload, indent=2, ensure_ascii=False))
    return path


def _save_todo_list(doc_id: str, todo_items: List[dict]) -> Path:
    path = _todo_path(doc_id)
    path.write_text(json.dumps(todo_items, indent=2, ensure_ascii=False))
    return path


def _records_to_dataframe(records: List[dict]) -> pd.DataFrame:
    if not records:
        return pd.DataFrame()
    return pd.DataFrame(records)


def _load_coverage_report(doc_id: str, code_index_id: str) -> Optional[Dict[str, Any]]:
    report_path = REPORT_DIR / f"{doc_id}_{code_index_id}_coverage.json"
    if not report_path.exists():
        return None
    return json.loads(report_path.read_text())


# -----------------------------------------------------------------------------
# UI Widget Functions
# -----------------------------------------------------------------------------

def _qa_widget(provider_data: dict, doc_ids: List[str]):
    """
    Render a question-answering widget in the Streamlit interface.
    
    Allows users to ask natural language questions about indexed documents
    and displays answers with source chunks for transparency.
    
    Args:
        provider_data: Dictionary containing LLM provider and functions
        doc_ids: List of document IDs to query across
    """
    if not doc_ids:
        st.info("Index at least one document to enable QA.")
        return

    st.caption(f"Answering using {len(doc_ids)} indexed document(s).")
    question = st.text_area("Ask a question", key="qa_prompt")
    top_k = st.slider("Top chunks", 2, 10, 4)

    if st.button("Get Answer", type="primary"):
        if not question.strip():
            st.warning("Please enter a question.")
            return
        try:
            with st.spinner("Retrieving answer..."):
                result = ask_across_docs(
                    doc_ids,
                    question.strip(),
                    provider=provider_data["provider"],
                    top_k=max(top_k, 4),
                )
            st.success("Answer")
            st.write(result["answer"])
            with st.expander("Sources"):
                for ctx in result.get("context", []):
                    st.markdown(
                        f"**Doc:** {ctx.get('doc_id')} — **Chunk:** {ctx.get('chunk_id')} "
                        f"(score={ctx.get('score', 0):.3f})"
                    )
                    st.write(ctx.get("content"))
        except Exception as exc:  # pylint: disable=broad-except
            st.error(f"QA failed: {exc}")


def _handle_indexing(mode: str, provider_data: dict):
    """
    Handle document indexing workflow in the UI.
    
    Supports two modes:
    1. Re-indexing existing documents
    2. Uploading and indexing new documents
    
    Args:
        mode: Either "Select indexed document" or "Upload new document"
        provider_data: Dictionary containing LLM provider and functions
    """
    docs = _doc_options()
    doc_id: Optional[str] = None
    doc_path: Optional[Path] = None
    uploaded_file = None

    if mode == "Select indexed document":
        options = [doc["doc_id"] for doc in docs]
        doc_id = st.selectbox("Indexed documents", options) if options else None
        if doc_id:
            meta = next((doc for doc in docs if doc["doc_id"] == doc_id), {})
            if meta.get("file_path"):
                doc_path = DEFAULT_DOCS_DIR / meta["file_path"]
    else:
        uploaded_file = st.file_uploader("Upload new GDD (PDF/DOCX)", type=["pdf", "docx"])
        suggested = _make_doc_id(uploaded_file.name) if uploaded_file else ""
        doc_id = st.text_input("New document ID", value=suggested)

    if st.button("Index / Reindex", use_container_width=True):
        if mode == "Upload new document" and not uploaded_file:
            st.error("Upload a document before indexing.")
            return
        if not doc_id:
            st.error("Document ID is required.")
            return
        if uploaded_file:
            extension = Path(uploaded_file.name).suffix or ".pdf"
            doc_path = DEFAULT_DOCS_DIR / f"{doc_id}{extension}"
            DEFAULT_DOCS_DIR.mkdir(parents=True, exist_ok=True)
            doc_path.write_bytes(uploaded_file.getbuffer())
        if not doc_path or not doc_path.exists():
            st.error("Could not locate the document on disk.")
            return
        try:
            with st.spinner(f"Indexing {doc_id}..."):
                _async_run(
                    indexing.index_document(
                        doc_path=doc_path,
                        doc_id=doc_id,
                        llm_func=provider_data["llm_func"],
                        embedding_func=provider_data["embedding_func"],
                    )
                )
            st.success(f"{doc_id} indexed successfully at {datetime.now():%Y-%m-%d %H:%M:%S}.")
        except Exception as exc:  # pylint: disable=broad-except
            st.error(f"Indexing failed: {exc}")

    if doc_id:
        st.markdown("### Quick QA")
        doc_ids = [doc["doc_id"] for doc in docs]
        if doc_id not in doc_ids:
            doc_ids.append(doc_id)
        _qa_widget(provider_data, doc_ids)


def _analysis_page(doc_id: Optional[str], provider_data: dict):
    """
    Render the GDD Explorer & Analysis page.
    
    Provides high-level document analysis and ad-hoc question answering
    capabilities for exploring indexed Game Design Documents.
    
    Args:
        doc_id: Selected document ID (optional)
        provider_data: Dictionary containing LLM provider and functions
    """
    st.subheader("Document Explorer & Analysis")
    if not doc_id:
        st.info("Select a document from the sidebar first.")
        return

    st.markdown("#### High-level Analysis")
    if st.button("Analyze GDD", key="analyze_button"):
        try:
            with st.spinner("Generating analysis..."):
                summary = _async_run(analyze_gdd(doc_id))
            st.markdown(summary)
        except Exception as exc:  # pylint: disable=broad-except
            st.error(f"Analysis failed: {exc}")

    st.markdown("#### Ad-hoc QA")
    docs = [doc["doc_id"] for doc in _doc_options()]
    _qa_widget(provider_data, docs)


def _requirements_page(doc_id: Optional[str]):
    """
    Render the Requirements & To-Do extraction page.
    
    Allows users to:
    - Extract structured data (objects, systems, logic rules, requirements)
    - Generate developer todo lists from extracted requirements
    - View extracted data in tabular and JSON formats
    
    Args:
        doc_id: Selected document ID (optional)
    """
    st.subheader("Structured Extraction & To-Do")
    docs = _doc_options()
    if not docs:
        st.info("Index a document first.")
        return

    doc_ids = [doc["doc_id"] for doc in docs]
    default_index = doc_ids.index(doc_id) if doc_id in doc_ids else 0
    selected_doc = st.selectbox(
        "Document",
        doc_ids,
        index=default_index,
        key="requirements_doc_select",
    )

    if st.button("Run Structured Extraction", type="primary"):
        try:
            with st.spinner("Extracting structured data..."):
                payload = _async_run(extract_all_requirements(selected_doc))
            save_path = _save_structured_requirements(selected_doc, payload)
            st.success(f"Extraction saved to {save_path}")
        except Exception as exc:  # pylint: disable=broad-except
            st.error(f"Extraction failed: {exc}")

    structured = _load_structured_requirements(selected_doc)
    if not structured:
        st.info("No structured extraction found yet.")
        return

    tabs = st.tabs(["Objects", "Systems", "Logic Rules", "Requirements", "To-Do"])

    def _render_tab(data: List[dict], tab, empty_message: str):
        with tab:
            if not data:
                st.info(empty_message)
                return
            df = _records_to_dataframe(data)
            st.dataframe(df, use_container_width=True)
            with st.expander("Raw JSON"):
                st.json(data)

    _render_tab(structured.get("objects", []), tabs[0], "No objects extracted.")
    _render_tab(structured.get("systems", []), tabs[1], "No systems extracted.")
    _render_tab(structured.get("logic_rules", []), tabs[2], "No logic rules extracted.")
    requirements_tab = tabs[3]
    with requirements_tab:
        requirements = structured.get("requirements", [])
        if not requirements:
            st.info("No requirements extracted.")
        else:
            df = _records_to_dataframe(requirements)
            st.dataframe(df, use_container_width=True)
            with st.expander("Raw JSON"):
                st.json(requirements)

    with tabs[4]:
        todo_path = _todo_path(selected_doc)
        existing_todo = json.loads(todo_path.read_text()) if todo_path.exists() else []
        if st.button("Generate Developer To-Do", key="todo_generate"):
            try:
                with st.spinner("Generating to-do items..."):
                    todo_items = _async_run(generate_todo_list(structured))
                if todo_items:
                    _save_todo_list(selected_doc, todo_items)
                    existing_todo = todo_items
                    st.success("To-do list updated.")
                else:
                    st.warning("LLM did not return any to-do items.")
            except Exception as exc:  # pylint: disable=broad-except
                st.error(f"Failed to generate to-do list: {exc}")
        if not existing_todo:
            st.info("No to-do items generated yet.")
        else:
            df = _records_to_dataframe(existing_todo)
            st.dataframe(df, use_container_width=True)
            with st.expander("Raw JSON"):
                st.json(existing_todo)


def _coverage_page(doc_id: Optional[str]):
    """
    Render the Code Coverage Check page.
    
    Evaluates how well the codebase implements requirements from the GDD.
    Matches requirements against code chunks using semantic search and
    LLM-based classification to determine implementation status.
    
    Args:
        doc_id: Selected document ID (optional)
    """
    st.subheader("Code Coverage Check")
    if not doc_id:
        st.info("Select a document first.")
        return

    structured = _load_structured_requirements(doc_id)
    if not structured or not structured.get("requirements"):
        st.warning("Run structured extraction first to obtain requirements.")
        return

    code_index_id = st.text_input("Code index doc_id (from ingestion)", value="codebase")
    top_k = st.slider("Chunks per query", min_value=4, max_value=12, value=8)

    if st.button("Run Coverage Evaluation", type="primary"):
        try:
            requirements = [GddRequirement(**item) for item in structured.get("requirements", [])]
            REPORT_DIR.mkdir(parents=True, exist_ok=True)
            with st.spinner("Matching requirements against code..."):
                report_path = _async_run(
                    evaluate_all_requirements(
                        doc_id,
                        code_index_id.strip(),
                        requirements,
                        top_k=top_k,
                    )
                )
            st.success(f"Coverage report saved to {report_path}")
        except Exception as exc:  # pylint: disable=broad-except
            st.error(f"Coverage evaluation failed: {exc}")

    report = _load_coverage_report(doc_id, code_index_id.strip())
    if not report:
        st.info("No coverage report available yet.")
        return

    results = report.get("results", [])
    if not results:
        st.info("Coverage report is empty.")
        return

    df = _records_to_dataframe(results)
    st.dataframe(df[[col for col in df.columns if col != "retrieved_chunks"]], use_container_width=True)

    selection = st.selectbox("Inspect requirement", df["requirement_id"].tolist())
    if selection:
        entry = next(item for item in results if item.get("requirement_id") == selection)
        st.markdown(f"**Status:** {entry.get('status')}" )
        evidence = entry.get("evidence") or []
        if evidence:
            with st.expander("Evidence"):
                st.json(evidence)
        retrieved = entry.get("retrieved_chunks") or []
        if retrieved:
            with st.expander("Retrieved Code Chunks"):
                for chunk in retrieved:
                    st.markdown(f"`{chunk.get('chunk_id')}` (score={chunk.get('score', 0):.3f})")
                    st.write(chunk.get("content"))
                    st.divider()


# -----------------------------------------------------------------------------
# Main Application Entry Point
# -----------------------------------------------------------------------------

def main() -> None:
    """
    Main entry point for the Streamlit application.
    
    Sets up the page configuration, initializes the provider bundle,
    and routes users to different pipeline stages based on sidebar selection.
    
    Pipeline stages:
    1. GDD & Indexing - Document upload and indexing
    2. GDD Explorer & Analysis - Document exploration and QA
    3. Requirements & To-Do - Structured extraction and task generation
    4. Code Coverage - Requirement-to-code matching evaluation
    """
    st.set_page_config(page_title="GDD Pipeline", layout="wide")
    st.sidebar.title("GDD → Tasks → Code")
    provider_data = _provider_bundle()

    docs = _doc_options()
    doc_options = [doc["doc_id"] for doc in docs]
    selected_doc = st.sidebar.selectbox("Active document", options=doc_options) if doc_options else None

    page = st.sidebar.radio(
        "Pipeline Step",
        (
            "1. GDD & Indexing",
            "2. GDD Explorer & Analysis",
            "3. Requirements & To-Do",
            "4. Code Coverage",
        ),
    )

    if page == "1. GDD & Indexing":
        mode = st.radio(
            "Document Source",
            ("Select indexed document", "Upload new document"),
            horizontal=True,
        )
        _handle_indexing(mode, provider_data)
    elif page == "2. GDD Explorer & Analysis":
        _analysis_page(selected_doc, provider_data)
    elif page == "3. Requirements & To-Do":
        _requirements_page(selected_doc)
    else:
        _coverage_page(selected_doc)


if __name__ == "__main__":
    main()
