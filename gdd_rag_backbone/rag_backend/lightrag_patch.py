"""
Patch for lightrag to export LightRAG and QueryParam for raganything compatibility.

This patch attempts to find LightRAG and QueryParam in various locations
within the lightrag package and export them at the top level.
"""
import sys
import importlib

def patch_lightrag():
    """Patch lightrag module to export LightRAG and QueryParam."""
    try:
        import lightrag
        import os
        import inspect
        
        # Only patch if LightRAG is not already exported
        if not hasattr(lightrag, 'LightRAG'):
            # Try various possible import paths
            possible_paths = [
                'lightrag.lightrag',
                'lightrag.core.lightrag', 
                'lightrag.core',
                'lightrag.components.lightrag',
            ]
            
            for path in possible_paths:
                try:
                    module = importlib.import_module(path)
                    if hasattr(module, 'LightRAG'):
                        lightrag.LightRAG = module.LightRAG
                        print(f"✓ Patched LightRAG from {path}")
                        break
                except (ImportError, AttributeError):
                    continue
            
            # If still not found, search all modules recursively
            if not hasattr(lightrag, 'LightRAG'):
                lightrag_path = lightrag.__path__[0]
                for root, dirs, files in os.walk(lightrag_path):
                    for file in files:
                        if file.endswith('.py') and not file.startswith('__'):
                            rel_path = os.path.relpath(os.path.join(root, file), lightrag_path)
                            module_path = 'lightrag.' + rel_path.replace(os.sep, '.').replace('.py', '')
                            try:
                                module = importlib.import_module(module_path)
                                if hasattr(module, 'LightRAG'):
                                    lightrag.LightRAG = module.LightRAG
                                    print(f"✓ Patched LightRAG from {module_path}")
                                    break
                            except (ImportError, AttributeError, ValueError):
                                continue
                    if hasattr(lightrag, 'LightRAG'):
                        break
        
        # Try to find QueryParam
        if not hasattr(lightrag, 'QueryParam'):
            possible_paths = [
                'lightrag.lightrag',
                'lightrag.core.query',
                'lightrag.core',
            ]
            
            for path in possible_paths:
                try:
                    module = importlib.import_module(path)
                    if hasattr(module, 'QueryParam'):
                        lightrag.QueryParam = module.QueryParam
                        print(f"✓ Patched QueryParam from {path}")
                        break
                except (ImportError, AttributeError):
                    continue
            
            # If QueryParam still not found, create a minimal TypedDict
            if not hasattr(lightrag, 'QueryParam'):
                from typing import TypedDict
                class QueryParam(TypedDict, total=False):
                    """Minimal QueryParam for compatibility."""
                    pass
                lightrag.QueryParam = QueryParam
                print("✓ Created minimal QueryParam")
        
    except Exception as e:
        # If patching fails, the import error will be raised by raganything
        print(f"⚠ Warning: Could not patch lightrag: {e}")

# Apply patch immediately when this module is imported
patch_lightrag()

