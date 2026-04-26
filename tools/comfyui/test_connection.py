#!/usr/bin/env python3
"""
Simple test to check if the ComfyUI server is accessible.
"""

import urllib.request
import json
from config_loader import get_server_url

def test_server_connection(server_url):
    """Test basic connectivity to ComfyUI server."""
    print(f"Testing connection to: {server_url}")
    
    try:
        # Test basic HTTP connectivity
        response = urllib.request.urlopen(f"{server_url}/", timeout=10)
        print(f"✓ HTTP connection successful (status: {response.getcode()})")
        return True
        
    except urllib.error.URLError as e:
        print(f"✗ Connection failed: {e}")
        return False
    except Exception as e:
        print(f"✗ Unexpected error: {e}")
        return False

def main():
    server_url = get_server_url()
    
    print("=" * 50)
    print("ComfyUI Server Connection Test")
    print("=" * 50)
    
    if test_server_connection(server_url):
        print("\n✓ Server is accessible! You can use the ComfyUI client.")
    else:
        print("\n✗ Server is not accessible. Please check:")
        print("  - Server is running")
        print("  - URL is correct")
        print("  - Network connectivity")
        print("  - Firewall settings")

if __name__ == "__main__":
    main()
