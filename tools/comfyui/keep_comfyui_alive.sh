#!/bin/bash

# ComfyUI Keep-Alive Script
# Pings the ComfyUI server every 30 minutes to prevent auto-shutdown

# Function to read config from JSON file
read_config() {
    local config_file="comfyui_config.json"
    
    if [ ! -f "$config_file" ]; then
        echo "Warning: Config file $config_file not found, using default values"
        SERVER_URL="http://localhost:7860/"
        PING_INTERVAL=30
        return
    fi
    
    # Try to use jq if available, otherwise fall back to python
    if command -v jq > /dev/null 2>&1; then
        SERVER_URL=$(jq -r '.server_url' "$config_file" 2>/dev/null || echo "http://localhost:7860/")
        PING_INTERVAL=$(jq -r '.ping_interval_minutes' "$config_file" 2>/dev/null || echo "30")
    elif command -v python3 > /dev/null 2>&1; then
        SERVER_URL=$(python3 -c "import json; print(json.load(open('$config_file'))['server_url'])" 2>/dev/null || echo "http://localhost:7860/")
        PING_INTERVAL=$(python3 -c "import json; print(json.load(open('$config_file'))['ping_interval_minutes'])" 2>/dev/null || echo "30")
    else
        echo "Warning: Neither jq nor python3 available, using default values"
        SERVER_URL="http://localhost:7860/"
        PING_INTERVAL=30
    fi
}

# Read configuration
read_config

# Log file
LOG_FILE="comfyui_keepalive.log"

# Function to log messages with timestamp
log_message() {
    echo "$(date '+%Y-%m-%d %H:%M:%S') - $1" | tee -a "$LOG_FILE"
}

# Function to ping the server
ping_server() {
    log_message "Pinging ComfyUI server..."
    
    # Try to reach the server with a simple HTTP request
    if curl -s --max-time 10 "$SERVER_URL" > /dev/null 2>&1; then
        log_message "✓ Server is alive and responding"
        return 0
    else
        log_message "✗ Server did not respond (may be starting up or down)"
        return 1
    fi
}

# Function to run the keep-alive loop
keep_alive() {
    log_message "Starting ComfyUI keep-alive monitor"
    log_message "Server URL: $SERVER_URL"
    log_message "Ping interval: $PING_INTERVAL minutes"
    log_message "Log file: $LOG_FILE"
    echo ""
    
    # Calculate sleep duration in seconds
    local sleep_duration=$((PING_INTERVAL * 60))
    
    while true; do
        ping_server
        
        # Wait for the configured interval
        log_message "Waiting $PING_INTERVAL minutes until next ping..."
        sleep $sleep_duration
    done
}

# Handle script termination
cleanup() {
    log_message "Keep-alive script terminated"
    exit 0
}

# Set up signal handlers
trap cleanup SIGTERM SIGINT

# Check if running in background mode
if [ "$1" = "--daemon" ] || [ "$1" = "-d" ]; then
    log_message "Starting in daemon mode (background)"
    keep_alive &
    echo "Keep-alive script started in background (PID: $!)"
    echo "To stop: kill $!"
    echo "Log file: $LOG_FILE"
else
    # Show usage and run interactively
    echo "ComfyUI Keep-Alive Script"
    echo "========================"
    echo ""
    echo "Usage:"
    echo "  $0              # Run interactively (shows output)"
    echo "  $0 --daemon     # Run in background"
    echo "  $0 -d           # Run in background (short form)"
    echo ""
    echo "To run in background and return to terminal:"
    echo "  $0 --daemon"
    echo ""
    echo "To run interactively (Ctrl+C to stop):"
    read -p "Press Enter to continue or Ctrl+C to cancel..."
    echo ""
    
    keep_alive
fi
