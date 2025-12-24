#!/usr/bin/env python3
"""
Supabase Storage operations
"""

from supabase import Client, create_client

from backend.app.config import get_settings

class Storage:
    """Supabase Storage operations"""

    def __init__(self):
        """Initialize Supabase storage client"""
        settings = get_settings()
        self.supabase_url = settings.supabase_url
        self.supabase_key = settings.supabase_service_role_key or settings.supabase_anon_key

        if not self.supabase_url or not self.supabase_key:
            raise ValueError("Missing SUPABASE_URL or SUPABASE_SERVICE_ROLE_KEY/SUPABASE_ANON_KEY")

        self.client: Client = create_client(self.supabase_url, self.supabase_key)

    def health_check(self) -> bool:
        """Check storage connectivity"""
        try:
            # Try to list buckets or check if uploads bucket exists
            result = self.client.storage.list_buckets()
            return True
        except Exception:
            return False

    async def upload_file(self, file_content: bytes, path: str) -> str:
        """Upload a file to Supabase Storage"""
        try:
            # Upload to 'uploads' bucket
            result = self.client.storage.from_('uploads').upload(
                path=path,
                file=file_content,
                file_options={"content-type": "application/octet-stream"}
            )

            return path  # Return the path as storage identifier

        except Exception as e:
            raise Exception(f"Failed to upload file: {str(e)}")

    async def download_file(self, path: str) -> bytes:
        """Download a file from Supabase Storage"""
        try:
            result = self.client.storage.from_('uploads').download(path)
            return result

        except Exception as e:
            raise Exception(f"Failed to download file: {str(e)}")

    async def delete_file(self, path: str):
        """Delete a file from Supabase Storage"""
        try:
            self.client.storage.from_('uploads').remove([path])

        except Exception as e:
            raise Exception(f"Failed to delete file: {str(e)}")
