"""
Workspace management module for isolated project workspaces.

Each workspace has its own:
- Documents storage
- RAG indices
- Behavior cache
- Reports
- Configuration
"""

from gdd_rag_backbone.workspace.manager import WorkspaceManager
from gdd_rag_backbone.workspace.storage import WorkspaceStorage

__all__ = ["WorkspaceManager", "WorkspaceStorage"]

