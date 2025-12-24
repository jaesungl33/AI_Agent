"""
Tree-sitter based code parsing for extracting code structure and metadata.
Based on code_qa implementation.
"""

import os
from typing import List, Dict, Any, Optional, Tuple
from pathlib import Path
import tree_sitter_python as tspython
import tree_sitter_javascript as tsjavascript
import tree_sitter_java as tsjava
import tree_sitter_rust as tsrust
import tree_sitter_go as tsgo
import tree_sitter_cpp as tscpp
from tree_sitter import Language, Parser


class CodeParser:
    """Parse code files using tree-sitter to extract functions, classes, and other symbols."""

    def __init__(self):
        self.parsers = {}
        self._setup_parsers()

    def _setup_parsers(self):
        """Initialize tree-sitter parsers for supported languages."""
        # Python
        try:
            PYTHON_LANGUAGE = Language(tspython.language())
            self.parsers['.py'] = Parser(PYTHON_LANGUAGE)
        except Exception as e:
            print(f"Failed to load Python parser: {e}")

        # JavaScript/TypeScript
        try:
            JS_LANGUAGE = Language(tsjavascript.language())
            self.parsers['.js'] = Parser(JS_LANGUAGE)
            self.parsers['.jsx'] = Parser(JS_LANGUAGE)
            self.parsers['.ts'] = Parser(JS_LANGUAGE)
            self.parsers['.tsx'] = Parser(JS_LANGUAGE)
        except Exception as e:
            print(f"Failed to load JavaScript parser: {e}")

        # Java
        try:
            JAVA_LANGUAGE = Language(tsjava.language())
            self.parsers['.java'] = Parser(JAVA_LANGUAGE)
        except Exception as e:
            print(f"Failed to load Java parser: {e}")

        # Rust
        try:
            RUST_LANGUAGE = Language(tsrust.language())
            self.parsers['.rs'] = Parser(RUST_LANGUAGE)
        except Exception as e:
            print(f"Failed to load Rust parser: {e}")

        # Go
        try:
            GO_LANGUAGE = Language(tsgo.language())
            self.parsers['.go'] = Parser(GO_LANGUAGE)
        except Exception as e:
            print(f"Failed to load Go parser: {e}")

        # C++
        try:
            CPP_LANGUAGE = Language(tscpp.language())
            self.parsers['.cpp'] = Parser(CPP_LANGUAGE)
            self.parsers['.cc'] = Parser(CPP_LANGUAGE)
            self.parsers['.cxx'] = Parser(CPP_LANGUAGE)
            self.parsers['.c'] = Parser(CPP_LANGUAGE)
            self.parsers['.h'] = Parser(CPP_LANGUAGE)
            self.parsers['.hpp'] = Parser(CPP_LANGUAGE)
        except Exception as e:
            print(f"Failed to load C++ parser: {e}")

    def get_parser(self, file_extension: str) -> Optional[Parser]:
        """Get the appropriate parser for a file extension."""
        return self.parsers.get(file_extension.lower())

    def parse_file(self, file_path: str) -> List[Dict[str, Any]]:
        """Parse a code file and extract symbols (functions, classes, etc.)."""
        path = Path(file_path)
        if not path.exists():
            return []

        parser = self.get_parser(path.suffix)
        if not parser:
            return []

        try:
            with open(file_path, 'r', encoding='utf-8', errors='ignore') as f:
                code = f.read()

            tree = parser.parse(bytes(code, 'utf-8'))
            symbols = self._extract_symbols(tree, code, str(path))
            return symbols

        except Exception as e:
            print(f"Error parsing {file_path}: {e}")
            return []

    def _extract_symbols(self, tree, code: str, file_path: str) -> List[Dict[str, Any]]:
        """Extract symbols from the parsed AST."""
        symbols = []

        def traverse_node(node, parent_context=""):
            """Recursively traverse AST nodes to find symbols."""
            if node.type in ['function_definition', 'method_definition']:
                symbol = self._extract_function(node, code, file_path, parent_context)
                if symbol:
                    symbols.append(symbol)

            elif node.type in ['class_definition', 'interface_declaration', 'struct_item']:
                symbol = self._extract_class(node, code, file_path, parent_context)
                if symbol:
                    symbols.append(symbol)

            elif node.type in ['constructor_declaration', 'destructor_declaration']:
                symbol = self._extract_constructor(node, code, file_path, parent_context)
                if symbol:
                    symbols.append(symbol)

            # Continue traversing children
            for child in node.children:
                if child.type not in ['block', 'body']:  # Skip implementation blocks
                    context = parent_context
                    if node.type in ['class_definition', 'interface_declaration', 'struct_item']:
                        # Extract class/struct name for context
                        name_node = None
                        for c in node.children:
                            if c.type in ['identifier', 'type_identifier']:
                                name_node = c
                                break
                        if name_node:
                            class_name = code[name_node.start_byte:name_node.end_byte]
                            context = f"{parent_context}.{class_name}" if parent_context else class_name

                    traverse_node(child, context)

        traverse_node(tree.root_node)
        return symbols

    def _extract_function(self, node, code: str, file_path: str, context: str) -> Optional[Dict[str, Any]]:
        """Extract function/method information."""
        try:
            # Find function name
            name_node = None
            for child in node.children:
                if child.type in ['identifier', 'operator_name']:
                    name_node = child
                    break

            if not name_node:
                return None

            name = code[name_node.start_byte:name_node.end_byte]
            full_name = f"{context}.{name}" if context else name

            # Get function signature
            signature = code[node.start_byte:node.end_byte].split('{')[0].split('\n')[0].strip()

            return {
                'type': 'function',
                'name': name,
                'full_name': full_name,
                'signature': signature,
                'file_path': file_path,
                'line_start': node.start_point[0] + 1,
                'line_end': node.end_point[0] + 1,
                'context': context,
                'content': code[node.start_byte:node.end_byte],
                'language': self._detect_language(file_path)
            }
        except Exception as e:
            print(f"Error extracting function: {e}")
            return None

    def _extract_class(self, node, code: str, file_path: str, context: str) -> Optional[Dict[str, Any]]:
        """Extract class/struct/interface information."""
        try:
            # Find class name
            name_node = None
            for child in node.children:
                if child.type in ['identifier', 'type_identifier']:
                    name_node = child
                    break

            if not name_node:
                return None

            name = code[name_node.start_byte:name_node.end_byte]
            full_name = f"{context}.{name}" if context else name

            return {
                'type': 'class',
                'name': name,
                'full_name': full_name,
                'file_path': file_path,
                'line_start': node.start_point[0] + 1,
                'line_end': node.end_point[0] + 1,
                'context': context,
                'content': code[node.start_byte:node.end_byte],
                'language': self._detect_language(file_path)
            }
        except Exception as e:
            print(f"Error extracting class: {e}")
            return None

    def _extract_constructor(self, node, code: str, file_path: str, context: str) -> Optional[Dict[str, Any]]:
        """Extract constructor information."""
        try:
            name = "constructor"  # Generic name for constructors
            full_name = f"{context}.{name}" if context else name

            return {
                'type': 'constructor',
                'name': name,
                'full_name': full_name,
                'file_path': file_path,
                'line_start': node.start_point[0] + 1,
                'line_end': node.end_point[0] + 1,
                'context': context,
                'content': code[node.start_byte:node.end_byte],
                'language': self._detect_language(file_path)
            }
        except Exception as e:
            print(f"Error extracting constructor: {e}")
            return None

    def _detect_language(self, file_path: str) -> str:
        """Detect programming language from file extension."""
        ext = Path(file_path).suffix.lower()
        language_map = {
            '.py': 'python',
            '.js': 'javascript',
            '.jsx': 'javascript',
            '.ts': 'typescript',
            '.tsx': 'typescript',
            '.java': 'java',
            '.rs': 'rust',
            '.go': 'go',
            '.cpp': 'cpp',
            '.cc': 'cpp',
            '.cxx': 'cpp',
            '.c': 'c',
            '.h': 'c',
            '.hpp': 'cpp'
        }
        return language_map.get(ext, 'unknown')


# Global parser instance
code_parser = CodeParser()


def parse_codebase(codebase_path: str) -> List[Dict[str, Any]]:
    """Parse an entire codebase and extract all symbols."""
    path = Path(codebase_path)
    if not path.exists():
        raise ValueError(f"Codebase path {codebase_path} does not exist")

    all_symbols = []

    # Supported file extensions
    extensions = ['.py', '.js', '.jsx', '.ts', '.tsx', '.java', '.rs', '.go', '.cpp', '.cc', '.cxx', '.c', '.h', '.hpp']

    for ext in extensions:
        for file_path in path.rglob(f'*{ext}'):
            if file_path.is_file() and not any(part.startswith('.') for part in file_path.parts):
                print(f"Parsing {file_path}")
                symbols = code_parser.parse_file(str(file_path))
                all_symbols.extend(symbols)

    return all_symbols


