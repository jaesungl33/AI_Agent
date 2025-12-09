#!/bin/bash
# Start backend API server locally

cd "$(dirname "$0")/../backend_api"
python3 -m uvicorn main:app --reload --host 0.0.0.0 --port 8000
