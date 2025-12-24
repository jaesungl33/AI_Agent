"""Configuration utilities for the FastAPI backend."""

from functools import lru_cache
from typing import List

from pydantic import BaseSettings, Field, HttpUrl


class Settings(BaseSettings):
    """Runtime configuration loaded from environment variables."""

    supabase_url: HttpUrl = Field(..., env="SUPABASE_URL")
    supabase_service_role_key: str = Field(..., env="SUPABASE_SERVICE_ROLE_KEY")
    supabase_anon_key: str | None = Field(None, env="SUPABASE_ANON_KEY")

    openai_api_key: str | None = Field(None, env="OPENAI_API_KEY")

    allowed_origins: List[str] = Field(
        default_factory=lambda: ["http://localhost:3000", "http://127.0.0.1:3000"]
    )

    debug: bool = Field(False, env="DEBUG")
    port: int = Field(8000, env="PORT")

    class Config:
        env_file = ".env"
        env_file_encoding = "utf-8"


@lru_cache(maxsize=1)
def get_settings() -> Settings:
    """Return cached settings instance."""
    return Settings()


__all__ = ["Settings", "get_settings"]
