"""
Workspace-scoped storage paths and operations.

This module provides workspace-isolated storage paths, ensuring
each workspace has its own documents, indices, cache, and reports.
"""

from pathlib import Path
from typing import Optional
from dataclasses import dataclass


@dataclass
class WorkspacePaths:
    """All paths for a workspace."""
    workspace_dir: Path
    documents_dir: Path
    storage_dir: Path
    indices_dir: Path
    vectors_dir: Path
    cache_dir: Path
    behavior_cache_dir: Path
    reports_dir: Path
    output_dir: Path
    config_file: Path


class WorkspaceStorage:
    """
    Manages storage paths for a specific workspace.
    
    Each workspace has isolated storage:
    - Documents: workspace/{id}/documents/
    - Indices: workspace/{id}/storage/indices/
    - Cache: workspace/{id}/storage/cache/
    - Reports: workspace/{id}/reports/
    """
    
    def __init__(self, workspace_id: str, base_dir: Optional[Path] = None):
        """
        Initialize workspace storage.
        
        Args:
            workspace_id: Unique workspace identifier
            base_dir: Base directory for workspaces (defaults to PROJECT_ROOT/workspaces)
        """
        from gdd_rag_backbone.config import PROJECT_ROOT
        
        self.workspace_id = workspace_id
        self.base_dir = base_dir or (PROJECT_ROOT / "data" / "workspaces")
        self.workspace_dir = self.base_dir / workspace_id
        
        # Create all directories
        self._paths = self._create_paths()
        self._ensure_directories()
    
    def _create_paths(self) -> WorkspacePaths:
        """Create all workspace paths."""
        return WorkspacePaths(
            workspace_dir=self.workspace_dir,
            documents_dir=self.workspace_dir / "documents",
            storage_dir=self.workspace_dir / "storage",
            indices_dir=self.workspace_dir / "storage" / "indices",
            vectors_dir=self.workspace_dir / "storage" / "indices" / "vectors",
            cache_dir=self.workspace_dir / "storage" / "cache",
            behavior_cache_dir=self.workspace_dir / "behavior_cache",
            reports_dir=self.workspace_dir / "reports",
            output_dir=self.workspace_dir / "output",
            config_file=self.workspace_dir / "workspace.json",
        )
    
    def _ensure_directories(self) -> None:
        """Ensure all workspace directories exist."""
        paths = self._paths
        paths.documents_dir.mkdir(parents=True, exist_ok=True)
        paths.documents_dir.joinpath("gdd").mkdir(exist_ok=True)
        paths.documents_dir.joinpath("code").mkdir(exist_ok=True)
        paths.indices_dir.mkdir(parents=True, exist_ok=True)
        paths.vectors_dir.mkdir(parents=True, exist_ok=True)
        paths.cache_dir.mkdir(parents=True, exist_ok=True)
        paths.behavior_cache_dir.mkdir(parents=True, exist_ok=True)
        paths.reports_dir.mkdir(parents=True, exist_ok=True)
        paths.output_dir.mkdir(parents=True, exist_ok=True)
    
    @property
    def paths(self) -> WorkspacePaths:
        """Get all workspace paths."""
        return self._paths
    
    def get_documents_dir(self, doc_type: str = "gdd") -> Path:
        """
        Get documents directory for a specific type.
        
        Args:
            doc_type: "gdd" or "code"
        
        Returns:
            Path to documents directory
        """
        return self._paths.documents_dir / doc_type
    
    def get_storage_dir(self) -> Path:
        """Get RAG storage directory."""
        return self._paths.storage_dir
    
    def get_indices_dir(self) -> Path:
        """Get indices directory."""
        return self._paths.indices_dir
    
    def get_vectors_dir(self) -> Path:
        """Get vectors directory."""
        return self._paths.vectors_dir
    
    def get_cache_dir(self) -> Path:
        """Get cache directory."""
        return self._paths.cache_dir
    
    def get_behavior_cache_dir(self) -> Path:
        """Get behavior cache directory."""
        return self._paths.behavior_cache_dir
    
    def get_reports_dir(self) -> Path:
        """Get reports directory."""
        return self._paths.reports_dir
    
    def get_output_dir(self) -> Path:
        """Get output directory for parsed content."""
        return self._paths.output_dir
    
    def get_config_file(self) -> Path:
        """Get workspace config file."""
        return self._paths.config_file
    
    # Convenience methods for specific storage files
    def get_status_file(self) -> Path:
        """Get document status file."""
        return self._paths.storage_dir / "status.json"
    
    def get_chunks_file(self) -> Path:
        """Get text chunks file."""
        return self._paths.indices_dir / "chunks.json"
    
    def get_entities_file(self) -> Path:
        """Get entity chunks file."""
        return self._paths.indices_dir / "entities.json"
    
    def get_vector_chunks_file(self) -> Path:
        """Get vector chunks file."""
        return self._paths.vectors_dir / "chunks.json"
    
    def get_vector_entities_file(self) -> Path:
        """Get vector entities file."""
        return self._paths.vectors_dir / "entities.json"
    
    def get_vector_relations_file(self) -> Path:
        """Get vector relations file."""
        return self._paths.vectors_dir / "relations.json"
    
    def get_graph_file(self) -> Path:
        """Get knowledge graph file."""
        return self._paths.indices_dir / "graph.graphml"
    
    def get_llm_cache_file(self) -> Path:
        """Get LLM cache file."""
        return self._paths.cache_dir / "llm_cache.json"
    
    def get_parse_cache_file(self) -> Path:
        """Get parse cache file."""
        return self._paths.cache_dir / "parse_cache.json"

