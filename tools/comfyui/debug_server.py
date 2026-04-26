#!/usr/bin/env python3
"""
Debug script to check ComfyUI server capabilities and available models.
"""

import urllib.request
import json
import sys

def check_server_info(server_url):
    """Check what's available on the ComfyUI server."""
    print(f"Checking server capabilities at: {server_url}")
    
    try:
        # Check if we can get system stats
        try:
            with urllib.request.urlopen(f"{server_url}/system_stats", timeout=10) as response:
                stats = json.loads(response.read())
                print(f"✓ System stats available: {stats}")
        except:
            print("- System stats not available")
        
        # Check if we can get object info (available nodes)
        try:
            with urllib.request.urlopen(f"{server_url}/object_info", timeout=10) as response:
                object_info = json.loads(response.read())
                print(f"✓ Available node types: {len(object_info)} nodes")
                
                # Check for CheckpointLoaderSimple
                if "CheckpointLoaderSimple" in object_info:
                    checkpoint_info = object_info["CheckpointLoaderSimple"]
                    if "input" in checkpoint_info and "required" in checkpoint_info["input"]:
                        if "ckpt_name" in checkpoint_info["input"]["required"]:
                            available_models = checkpoint_info["input"]["required"]["ckpt_name"][0]
                            print(f"✓ Available models: {available_models}")
                            return available_models
                        else:
                            print("- No ckpt_name field found")
                    else:
                        print("- No required input found for CheckpointLoaderSimple")
                else:
                    print("- CheckpointLoaderSimple not available")
                    
                # List some key node types
                key_nodes = ["KSampler", "CLIPTextEncode", "VAEDecode", "EmptyLatentImage", "SaveImage"]
                available_key_nodes = [node for node in key_nodes if node in object_info]
                print(f"✓ Key nodes available: {available_key_nodes}")
                
        except Exception as e:
            print(f"- Object info not available: {e}")
        
        # Check queue status
        try:
            with urllib.request.urlopen(f"{server_url}/queue", timeout=10) as response:
                queue = json.loads(response.read())
                print(f"✓ Queue status: {queue}")
        except:
            print("- Queue status not available")
            
    except Exception as e:
        print(f"✗ Error checking server: {e}")
        return None

def test_simple_prompt(server_url, models=None):
    """Test a very simple prompt structure."""
    if not models:
        models = ["v1-5-pruned-emaonly.safetensors"]  # fallback
    
    model_name = models[0] if models else "v1-5-pruned-emaonly.safetensors"
    
    # Very simple workflow
    simple_workflow = {
        "1": {
            "class_type": "CheckpointLoaderSimple",
            "inputs": {
                "ckpt_name": model_name
            }
        },
        "2": {
            "class_type": "CLIPTextEncode",
            "inputs": {
                "clip": ["1", 1],
                "text": "a red apple"
            }
        },
        "3": {
            "class_type": "CLIPTextEncode",
            "inputs": {
                "clip": ["1", 1],
                "text": "blurry"
            }
        },
        "4": {
            "class_type": "EmptyLatentImage",
            "inputs": {
                "width": 512,
                "height": 512,
                "batch_size": 1
            }
        },
        "5": {
            "class_type": "KSampler",
            "inputs": {
                "model": ["1", 0],
                "positive": ["2", 0],
                "negative": ["3", 0],
                "latent_image": ["4", 0],
                "seed": 123456,
                "steps": 20,
                "cfg": 8.0,
                "sampler_name": "euler",
                "scheduler": "normal",
                "denoise": 1.0
            }
        },
        "6": {
            "class_type": "VAEDecode",
            "inputs": {
                "samples": ["5", 0],
                "vae": ["1", 2]
            }
        },
        "7": {
            "class_type": "SaveImage",
            "inputs": {
                "images": ["6", 0],
                "filename_prefix": "test"
            }
        }
    }
    
    print(f"\nTesting simple workflow with model: {model_name}")
    
    try:
        # Test the prompt structure
        prompt_data = {
            "prompt": simple_workflow,
            "client_id": "test_client"
        }
        
        data = json.dumps(prompt_data).encode('utf-8')
        req = urllib.request.Request(f"{server_url}/prompt", data=data)
        req.add_header('Content-Type', 'application/json')
        
        with urllib.request.urlopen(req) as response:
            result = json.loads(response.read())
            print(f"✓ Prompt accepted: {result}")
            return True
            
    except urllib.error.HTTPError as e:
        error_body = e.read().decode('utf-8')
        print(f"✗ HTTP Error {e.code}: {error_body}")
        return False
    except Exception as e:
        print(f"✗ Error testing prompt: {e}")
        return False

def main():
    server_url = "http://internal-a470ffca6f7b346139bfa8c73cfc3aa9-555188267.us-east-1.elb.amazonaws.com:7860"
    
    print("=" * 60)
    print("ComfyUI Server Debug Information")
    print("=" * 60)
    
    # Check server capabilities
    available_models = check_server_info(server_url)
    
    print("\n" + "=" * 60)
    print("Testing Simple Workflow")
    print("=" * 60)
    
    # Test a simple workflow
    if available_models:
        test_simple_prompt(server_url, available_models)
    else:
        test_simple_prompt(server_url)

if __name__ == "__main__":
    main()
