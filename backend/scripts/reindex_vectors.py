#!/usr/bin/env python3
"""
Re-index vectors for all chunks in a workspace.
This creates embeddings for existing chunks that don't have vectors.
"""

import argparse
import asyncio
import json
import sys
from pathlib import Path

# Add the backend and src directories to the Python path
sys.path.insert(0, str(Path(__file__).parent.parent))
sys.path.insert(0, str(Path(__file__).parent.parent.parent / "src"))

from gdd_rag_backbone.rag_backend.chunk_qa import _load_json, _get_storage_paths, _embed_texts
from main import get_llm_provider


async def reindex_vectors(workspace_id: str) -> None:
    """Re-index all chunks in a workspace with embeddings."""
    print(f"[Reindex] Starting vector reindexing for workspace {workspace_id}")

    # Get all chunks in the workspace
    paths = _get_storage_paths(workspace_id)
    chunks_path = paths["chunks"]

    if not chunks_path.exists():
        print(f"[Reindex] ERROR: No chunks found for workspace {workspace_id}")
        return

    chunks_data = _load_json(chunks_path)
    if not chunks_data:
        print(f"[Reindex] ERROR: No chunk data found for workspace {workspace_id}")
        return

    print(f"[Reindex] Found {len(chunks_data)} chunks in workspace")

    # Get provider
    try:
        provider = get_llm_provider()
        print(f"[Reindex] Using provider: {provider.__class__.__name__}")
    except Exception as e:
        print(f"[Reindex] ERROR: Failed to get provider: {e}")
        return

    # Prepare vector data
    vector_data = {"data": []}
    processed_count = 0
    error_count = 0

    # Group chunks by content to avoid duplicate embeddings
    content_to_chunks = {}
    for chunk_id, chunk_info in chunks_data.items():
        content = chunk_info.get("content", "").strip()
        if content:
            if content not in content_to_chunks:
                content_to_chunks[content] = []
            content_to_chunks[content].append((chunk_id, chunk_info))

    print(f"[Reindex] Found {len(content_to_chunks)} unique content blocks to embed")

    # Process in batches to avoid memory issues
    batch_size = 5  # Smaller batch size for testing
    contents = list(content_to_chunks.keys())

    for i in range(0, len(contents), batch_size):
        batch_contents = contents[i:i+batch_size]
        print(f"[Reindex] Processing batch {i//batch_size + 1}/{len(contents)//batch_size + 1} with {len(batch_contents)} contents")

        try:
            embeddings = _embed_texts(provider, batch_contents)

            for content, embedding in zip(batch_contents, embeddings):
                for chunk_id, chunk_info in content_to_chunks[content]:
                    vector_entry = {
                        "__id__": chunk_id,
                        "id": chunk_id,
                        "full_doc_id": chunk_info.get("full_doc_id", ""),
                        "doc_id": chunk_info.get("full_doc_id", ""),
                        "chunk_id": chunk_id,
                        "vector": embedding
                    }
                    vector_data["data"].append(vector_entry)
                    processed_count += 1

            print(f"[Reindex] Batch completed: {len(batch_contents)} contents -> {len(content_to_chunks[batch_contents[0]])} chunks each")

        except Exception as e:
            print(f"[Reindex] ERROR: Failed to embed batch: {e}")
            error_count += len(batch_contents)

    # Save the vector data
    vdb_path = paths["vdb_chunks"]
    vdb_path.parent.mkdir(parents=True, exist_ok=True)

    with open(vdb_path, 'w') as f:
        json.dump(vector_data, f, indent=2)

    print(f"[Reindex] COMPLETED: {processed_count} vectors created, {error_count} errors")
    print(f"[Reindex] Vector file saved to: {vdb_path}")


def main():
    parser = argparse.ArgumentParser(description="Re-index vectors for workspace chunks")
    parser.add_argument("--workspace", default="tank_war", help="Workspace ID (default: tank_war)")

    args = parser.parse_args()

    asyncio.run(reindex_vectors(args.workspace))


if __name__ == "__main__":
    main()

