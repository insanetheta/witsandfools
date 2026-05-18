#!/usr/bin/env python3
"""
unity-mcp-wrapper.py

Sits between Claude Code and relay_mac_arm64 --mcp.
Claude Code spawns this wrapper (stays alive), wrapper spawns the relay and
proxies all stdio line-by-line. When the relay crashes (e.g. Unity domain
reload), the wrapper restarts it automatically — Claude Code's MCP connection
stays alive.

In Unity AI Assistant 2.3+, the relay binary ships as a zip inside
PackageCache and must be extracted before use. This wrapper handles that
automatically.
"""

import glob
import os
import platform
import subprocess
import sys
import threading
import time
import zipfile

UNITY_PROJECT = os.path.join(os.path.dirname(__file__), "..")
PKG_CACHE = os.path.join(UNITY_PROJECT, "Library", "PackageCache")
RESTART_DELAY = 3  # seconds to wait before relaunching


def log(msg):
    sys.stderr.write(f"[unity-mcp-wrapper] {msg}\n")
    sys.stderr.flush()


def find_relay_binary():
    """
    Locate the relay binary for the current platform, handling both:
    - Old style (pre-2.3): relay_mac_arm64.app/Contents/MacOS/relay_mac_arm64 in PackageCache
    - New style (2.3+):    relay_mac_arm64 is a zip — extract it, then use the binary inside
    """
    arch = platform.machine().lower()
    system = platform.system().lower()

    if system == "darwin":
        zip_name = "relay_mac_arm64" if arch == "arm64" else "relay_mac_x64"
        app_name = zip_name + ".app"
    elif system == "linux":
        zip_name = "relay_linux"
        app_name = None
    else:
        zip_name = "relay_win.exe"
        app_name = None

    # Find the package directory (hash changes between versions)
    pattern = os.path.join(PKG_CACHE, "com.unity.ai.assistant@*", "RelayApp~")
    matches = sorted(glob.glob(pattern))
    if not matches:
        log(f"No com.unity.ai.assistant package found in {PKG_CACHE}")
        return None

    relay_dir = matches[-1]  # use latest if somehow multiple exist

    # Check for old-style .app bundle directly in RelayApp~
    if app_name:
        old_style = os.path.join(relay_dir, app_name, "Contents", "MacOS", zip_name)
        if os.path.isfile(old_style):
            log(f"Found old-style relay at {old_style}")
            return old_style

    zip_path = os.path.join(relay_dir, zip_name)
    if not os.path.isfile(zip_path):
        log(f"Relay zip not found: {zip_path}")
        return None

    # Check if it's actually a zip (new style)
    if not zipfile.is_zipfile(zip_path):
        # It's a plain executable (Linux/Windows)
        os.chmod(zip_path, 0o755)
        log(f"Found plain relay binary at {zip_path}")
        return zip_path

    # New style: extract the zip
    extract_dir = zip_path + "_extracted"
    if app_name:
        binary_path = os.path.join(extract_dir, app_name, "Contents", "MacOS", zip_name)
    else:
        binary_path = os.path.join(extract_dir, zip_name)

    if not os.path.isfile(binary_path):
        log(f"Extracting relay from {zip_path} ...")
        os.makedirs(extract_dir, exist_ok=True)
        with zipfile.ZipFile(zip_path, "r") as zf:
            zf.extractall(extract_dir)
        log("Extraction complete")

    if not os.path.isfile(binary_path):
        log(f"Binary not found after extraction: {binary_path}")
        return None

    os.chmod(binary_path, 0o755)
    log(f"Found relay binary at {binary_path}")
    return binary_path


def forward_stdin(src, dst, stop):
    """Forward stdin line-by-line from Claude Code to relay."""
    try:
        for line in src:
            if stop.is_set():
                break
            dst.write(line)
            dst.flush()
    except (OSError, BrokenPipeError, ValueError):
        pass


def main():
    log("Starting")

    relay_binary = find_relay_binary()
    if not relay_binary:
        log("FATAL: Could not locate relay binary. Is Unity AI Assistant installed?")
        sys.exit(1)

    relay_cmd = [relay_binary, "--mcp"]

    while True:
        try:
            proc = subprocess.Popen(
                relay_cmd,
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
                stderr=sys.stderr,
            )
        except Exception as e:
            log(f"Failed to launch relay: {e} — retrying in {RESTART_DELAY}s")
            time.sleep(RESTART_DELAY)
            continue

        log(f"Relay started (pid {proc.pid})")

        stop = threading.Event()

        # Claude Code stdin → relay stdin (daemon thread)
        t = threading.Thread(
            target=forward_stdin,
            args=(sys.stdin.buffer, proc.stdin, stop),
            daemon=True,
        )
        t.start()

        # Relay stdout → Claude Code stdout (line-by-line, blocks until relay exits)
        try:
            for line in proc.stdout:
                sys.stdout.buffer.write(line)
                sys.stdout.buffer.flush()
        except (OSError, BrokenPipeError):
            pass

        stop.set()
        proc.wait()
        log(f"Relay exited (code {proc.returncode}) — restarting in {RESTART_DELAY}s")
        time.sleep(RESTART_DELAY)


if __name__ == "__main__":
    main()
