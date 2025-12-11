"""
Workspace management - create, list, delete workspaces.
"""

import json
from pathlib import Path
from typing import List, Optional, Dict, Any
from datetime import datetime
from dataclasses import dataclass, asdict

from gdd_rag_backbone.workspace.storage import WorkspaceStorage


@dataclass
class WorkspaceInfo:
    """Workspace metadata."""
    id: str
    name: str
    description: str
    created_at: str
    updated_at: str
    settings: Dict[str, Any]
    stats: Dict[str, int]


class WorkspaceManager:
    """
    Manages workspace creation, listing, and deletion.
    
    Workspaces are stored in workspaces/ directory with a registry
    file at workspaces/.workspaces.json
    """
    
    def __init__(self, base_dir: Optional[Path] = None):
        """
        Initialize workspace manager.
        
        Args:
            base_dir: Base directory for workspaces (defaults to PROJECT_ROOT/workspaces)
        """
        from gdd_rag_backbone.config import PROJECT_ROOT
        
        self.base_dir = base_dir or (PROJECT_ROOT / "workspaces")
        self.base_dir.mkdir(parents=True, exist_ok=True)
        self.registry_file = self.base_dir / ".workspaces.json"
        self.default_file = self.base_dir / ".default_workspace"
    
    def _load_registry(self) -> Dict[str, Dict[str, Any]]:
        """Load workspace registry."""
        if not self.registry_file.exists():
            return {}
        try:
            with open(self.registry_file, 'r', encoding='utf-8') as f:
                return json.load(f)
        except Exception:
            return {}
    
    def _save_registry(self, registry: Dict[str, Dict[str, Any]]) -> None:
        """Save workspace registry."""
        with open(self.registry_file, 'w', encoding='utf-8') as f:
            json.dump(registry, f, indent=2, ensure_ascii=False)
    
    def create_workspace(
        self,
        name: str,
        description: str = "",
        workspace_id: Optional[str] = None,
        settings: Optional[Dict[str, Any]] = None
    ) -> str:
        """
        Create a new workspace.
        
        Args:
            name: Workspace name
            description: Workspace description
            workspace_id: Optional custom ID (auto-generated if not provided)
            settings: Optional workspace settings
        
        Returns:
            Workspace ID
        """
        # Generate workspace ID if not provided
        if not workspace_id:
            workspace_id = self._generate_workspace_id(name)
        
        # Check if workspace already exists
        if self.workspace_exists(workspace_id):
            raise ValueError(f"Workspace '{workspace_id}' already exists")
        
        # Create workspace storage
        storage = WorkspaceStorage(workspace_id, base_dir=self.base_dir)
        
        # Create workspace config
        now = datetime.utcnow().isoformat() + "Z"
        config = {
            "id": workspace_id,
            "name": name,
            "description": description,
            "created_at": now,
            "updated_at": now,
            "settings": settings or {
                "llm_model": "qwen-max",
                "embedding_model": "text-embedding-v3",
                "parser": "mineru"
            },
            "stats": {
                "gdd_count": 0,
                "code_count": 0,
                "total_documents": 0
            }
        }
        
        # Save workspace config
        with open(storage.get_config_file(), 'w', encoding='utf-8') as f:
            json.dump(config, f, indent=2, ensure_ascii=False)
        
        # Update registry
        registry = self._load_registry()
        registry[workspace_id] = {
            "name": name,
            "description": description,
            "created_at": now,
            "updated_at": now
        }
        self._save_registry(registry)
        
        # Set as default if it's the first workspace
        if len(registry) == 1:
            self.set_default_workspace(workspace_id)
        
        return workspace_id
    
    def _generate_workspace_id(self, name: str) -> str:
        """Generate workspace ID from name."""
        import re
        # Convert to lowercase, replace spaces/special chars with underscores
        workspace_id = re.sub(r'[^a-z0-9_]+', '_', name.lower())
        workspace_id = re.sub(r'_+', '_', workspace_id).strip('_')
        
        # Ensure uniqueness
        base_id = workspace_id
        counter = 1
        while self.workspace_exists(workspace_id):
            workspace_id = f"{base_id}_{counter}"
            counter += 1
        
        return workspace_id
    
    def list_workspaces(self) -> List[WorkspaceInfo]:
        """List all workspaces."""
        registry = self._load_registry()
        workspaces = []
        
        for workspace_id, meta in registry.items():
            config_file = self.base_dir / workspace_id / "workspace.json"
            if config_file.exists():
                try:
                    with open(config_file, 'r', encoding='utf-8') as f:
                        config = json.load(f)
                    workspaces.append(WorkspaceInfo(**config))
                except Exception:
                    # Fallback to registry data
                    workspaces.append(WorkspaceInfo(
                        id=workspace_id,
                        name=meta.get("name", workspace_id),
                        description=meta.get("description", ""),
                        created_at=meta.get("created_at", ""),
                        updated_at=meta.get("updated_at", ""),
                        settings={},
                        stats={}
                    ))
        
        return sorted(workspaces, key=lambda w: w.created_at, reverse=True)
    
    def get_workspace(self, workspace_id: str) -> Optional[WorkspaceInfo]:
        """Get workspace information."""
        config_file = self.base_dir / workspace_id / "workspace.json"
        if not config_file.exists():
            return None
        
        try:
            with open(config_file, 'r', encoding='utf-8') as f:
                config = json.load(f)
            return WorkspaceInfo(**config)
        except Exception:
            return None
    
    def workspace_exists(self, workspace_id: str) -> bool:
        """Check if workspace exists."""
        return (self.base_dir / workspace_id / "workspace.json").exists()
    
    def delete_workspace(self, workspace_id: str) -> bool:
        """
        Delete a workspace and all its data.
        
        Args:
            workspace_id: Workspace ID to delete
        
        Returns:
            True if deleted, False if not found
        """
        workspace_dir = self.base_dir / workspace_id
        if not workspace_dir.exists():
            return False
        
        # Remove workspace directory
        import shutil
        shutil.rmtree(workspace_dir)
        
        # Update registry
        registry = self._load_registry()
        if workspace_id in registry:
            del registry[workspace_id]
            self._save_registry(registry)
        
        # Clear default if this was the default
        default_id = self.get_default_workspace()
        if default_id == workspace_id:
            self.default_file.unlink(missing_ok=True)
        
        return True
    
    def set_default_workspace(self, workspace_id: str) -> None:
        """Set default workspace."""
        if not self.workspace_exists(workspace_id):
            raise ValueError(f"Workspace '{workspace_id}' does not exist")
        
        self.default_file.write_text(workspace_id)
    
    def get_default_workspace(self) -> Optional[str]:
        """Get default workspace ID."""
        if not self.default_file.exists():
            # Return first workspace if exists
            workspaces = self.list_workspaces()
            if workspaces:
                return workspaces[0].id
            return None
        
        default_id = self.default_file.read_text().strip()
        if self.workspace_exists(default_id):
            return default_id
        
        return None
    
    def update_workspace(
        self,
        workspace_id: str,
        name: Optional[str] = None,
        description: Optional[str] = None,
        settings: Optional[Dict[str, Any]] = None
    ) -> bool:
        """
        Update workspace metadata.
        
        Args:
            workspace_id: Workspace ID
            name: New name (optional)
            description: New description (optional)
            settings: New settings (optional, merged with existing)
        
        Returns:
            True if updated, False if not found
        """
        config_file = self.base_dir / workspace_id / "workspace.json"
        if not config_file.exists():
            return False
        
        with open(config_file, 'r', encoding='utf-8') as f:
            config = json.load(f)
        
        if name:
            config["name"] = name
        if description is not None:
            config["description"] = description
        if settings:
            config["settings"] = {**config.get("settings", {}), **settings}
        
        config["updated_at"] = datetime.utcnow().isoformat() + "Z"
        
        with open(config_file, 'w', encoding='utf-8') as f:
            json.dump(config, f, indent=2, ensure_ascii=False)
        
        # Update registry
        registry = self._load_registry()
        if workspace_id in registry:
            registry[workspace_id]["updated_at"] = config["updated_at"]
            if name:
                registry[workspace_id]["name"] = name
            if description is not None:
                registry[workspace_id]["description"] = description
            self._save_registry(registry)
        
        return True

