#!/usr/bin/env python3
"""Smoke test script to verify terminal runner functionality."""

import os
import sys
import time

def main():
    print("=== SMOKE TEST START ===")
    print(f"Current working directory: {os.getcwd()}")
    print(f"Python executable: {sys.executable}")
    print("Starting heartbeat...")

    for i in range(5):
        print(f"Heartbeat {i+1}/5 - {time.time()}")
        time.sleep(1)

    print("=== SMOKE TEST DONE ===")
    sys.exit(0)

if __name__ == "__main__":
    main()



