"""Backend package initialization.

This file marks the backend directory as a package so that absolute imports
can be used throughout the application. Keeping imports consistent prevents
issues when running the server both as a module and as a script.
"""

# Version marker for the backend package
__all__ = ["__version__"]
__version__ = "0.1.0"
