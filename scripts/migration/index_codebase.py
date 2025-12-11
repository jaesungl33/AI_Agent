#!/usr/bin/env python3
"""Build an indexed code snapshot and ingest it via the existing RAG pipeline."""
from __future__ import annotations

import argparse
import asyncio
import sys
from pathlib import Path

PROJECT_ROOT = Path(__file__).resolve().parents[1]
if str(PROJECT_ROOT) not in sys.path:
    sys.path.insert(0, str(PROJECT_ROOT))

from gdd_rag_backbone.config import DEFAULT_DOCS_DIR
from gdd_rag_backbone.llm_providers import QwenProvider, make_embedding_func, make_llm_model_func
from gdd_rag_backbone.rag_backend import indexing

CODE_EXTENSIONS = {
    ".cs",
    ".shader",
    ".cginc",
    ".hlsl",
    ".json",
    ".yml",
    ".yaml",
    ".xml",
    ".txt",
    ".md",
    ".asmdef",
    ".compute",
}

IGNORE_DIRS = {
    ".git",
    "Library",
    "Temp",
    "Logs",
    "Obj",
    "Build",
    "Builds",
    "UserSettings",
    "Packages/bin",
}

CHUNK_DIVIDER = "\n" + "=" * 80 + "\n"


def iter_code_files(root: Path):
    for path in root.rglob("*"):
        if not path.is_file():
            continue
        if any(part in IGNORE_DIRS for part in path.parts):
            continue
        if path.suffix.lower() in CODE_EXTENSIONS:
            yield path


def build_snapshot(root: Path, out_path: Path) -> None:
    files = sorted(iter_code_files(root))
    if not files:
        raise RuntimeError(f"No code files found under {root}")

    out_path.parent.mkdir(parents=True, exist_ok=True)
    with out_path.open("w", encoding="utf-8") as outfile:
        for file_path in files:
            rel = file_path.relative_to(root)
            outfile.write(f"FILE: {rel}\n")
            outfile.write(CHUNK_DIVIDER)
            try:
                text = file_path.read_text(encoding="utf-8")
            except UnicodeDecodeError:
                text = file_path.read_text(encoding="latin-1")
            outfile.write(text)
            outfile.write("\n" + CHUNK_DIVIDER + "\n\n")
    print(f"📦 Snapshot written to {out_path} ({len(files)} files)")


async def index_snapshot(doc_path: Path, doc_id: str) -> None:
    provider = QwenProvider()
    llm_func = make_llm_model_func(provider)
    embedding_func = make_embedding_func(provider)

    await indexing.index_document(
        doc_path=doc_path,
        doc_id=doc_id,
        llm_func=llm_func,
        embedding_func=embedding_func,
        parser="plain_text",
        parse_method="auto",
    )


def main() -> None:
    parser = argparse.ArgumentParser(description="Index an entire codebase as a single RAG doc")
    parser.add_argument("root", type=Path, help="Path to the codebase root (e.g., tank_online_1-dev)")
    parser.add_argument("--doc-id", default="code_tank_online", help="Doc ID to register in RAG store")
    parser.add_argument("--output", type=Path, default=None, help="Optional explicit snapshot file")
    args = parser.parse_args()

    if not args.root.exists():
        raise FileNotFoundError(args.root)

    out_path = args.output or DEFAULT_DOCS_DIR / f"{args.doc_id}.txt"
    build_snapshot(args.root, out_path)
    asyncio.run(index_snapshot(out_path, args.doc_id))
    print(f"✅ Codebase indexed as doc_id='{args.doc_id}'")


if __name__ == "__main__":
    main()
