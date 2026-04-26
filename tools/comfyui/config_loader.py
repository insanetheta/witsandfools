import json
import os

def load_comfyui_config():
    """Load ComfyUI configuration from config file"""
    config_path = os.path.join(os.path.dirname(__file__), 'comfyui_config.json')
    
    try:
        with open(config_path, 'r') as f:
            config = json.load(f)
        return config
    except FileNotFoundError:
        # Fallback to default if config file not found
        print(f"Warning: Config file not found at {config_path}, using default values")
        return {
            "server_url": "http://localhost:7860/",
            "server_host": "localhost", 
            "server_port": 7860,
            "ping_interval_minutes": 30
        }
    except json.JSONDecodeError as e:
        print(f"Error parsing config file: {e}")
        raise

def get_server_url():
    """Get the ComfyUI server URL"""
    config = load_comfyui_config()
    return config["server_url"]

def get_server_host():
    """Get the ComfyUI server host"""
    config = load_comfyui_config()
    return config["server_host"]

def get_server_port():
    """Get the ComfyUI server port"""
    config = load_comfyui_config()
    return config["server_port"]

def get_ping_interval():
    """Get the ping interval in minutes"""
    config = load_comfyui_config()
    return config["ping_interval_minutes"]
